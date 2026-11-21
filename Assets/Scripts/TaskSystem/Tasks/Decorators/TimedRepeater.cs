using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Cozyffee.TaskSystem
{
    public class TimedRepeater : Decorator
    {
        public float Duration = 3f;
        public float DurationRandom = 0f;

        private float currentDuration = 3f;
        private float timer = 0f;

        public TimedRepeater(Task childTask) : base(childTask) {}

        public TimedRepeater(Task childTask, float duration = 3f, float durationRandom = 0f) : base(childTask)
        {
            this.Duration = duration;
            this.DurationRandom = durationRandom;
        }

        protected override void OnEntry()
        {
            base.OnEntry();

            timer = 0f;
            currentDuration = Duration + Random.Range(0f, DurationRandom);
        }

        protected override TaskStatus OnUpdate()
        {
            if (child == null)
            {
                return TaskStatus.Failure;
            }

            TaskStatus status = Task.Update(child);

            bool childFinished = status == TaskStatus.Success || status == TaskStatus.Failure;

            if (childFinished)
            {
                Task.Restart(child);
            }

            timer += Time.deltaTime;

            if (timer >= currentDuration)
            {
                return TaskStatus.Success;
            }

            return TaskStatus.Running;
        }
    }
}