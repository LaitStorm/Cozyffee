using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public class Repeater : Decorator
    {
        public bool RepeatForever = true;
        public int Iterations = 10;
        public int IterationsRandom = 0;
        public bool EndOnFailure = false;
        private int timesToRepeat = 0;

        public Repeater(Task childTask, bool forever = true, int iterations = 10, int iterationsRandom = 0) : base(childTask)
        {
            RepeatForever = forever;
            Iterations = iterations;
            IterationsRandom = iterationsRandom;
        }

        protected override void OnEntry()
        {
            base.OnEntry();

            timesToRepeat = Iterations + Random.Range(0, IterationsRandom + 1);
        }

        protected override TaskStatus OnUpdate()
        {
            if (child == null)
            {
                return TaskStatus.Failure;
            }

            TaskStatus status = Task.Update(child);

            if (EndOnFailure && status == TaskStatus.Failure)
            {
                Task.Stop(child);
                return TaskStatus.Failure;
            }

            bool childFinished = status == TaskStatus.Success || status == TaskStatus.Failure;

            if (RepeatForever && childFinished)
            {
                Task.Restart(child);
            }
            if (!RepeatForever && childFinished)
            {
                timesToRepeat--;
                Task.Stop(child);

                if (timesToRepeat <= 0)
                {
                    return TaskStatus.Success;
                }
                else
                {
                    Task.Start(child);
                }
            }

            return TaskStatus.Running;
        }
    }
}