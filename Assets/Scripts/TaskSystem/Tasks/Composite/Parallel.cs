using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public class Parallel : Composite
    {
        public Parallel() {}

        public Parallel(params Task[] tasks) : base(tasks) {}

        protected override void OnEntry()
        {
            foreach (Task task in tasks)
            {
                Task.Start(task);
            }
        }

        protected bool AllChildrenFinished()
        {
            foreach (Task task in tasks)
            {
                if (task.Active)
                {
                    return false;
                }
            }

            return true;
        }

        protected override TaskStatus OnUpdate()
        {
            if (tasks.Count <= 0)
            {
                return TaskStatus.Failure;
            }
            
            foreach (Task task in tasks)
            {
                if (task.Active == false)
                {
                    continue;
                }

                TaskStatus status = Task.Update(task);

                if (status == TaskStatus.Failure)
                {
                    Task.Stop(task);
                    return TaskStatus.Failure;
                }
                else if (status == TaskStatus.Success)
                {
                    Task.Stop(task);
                }
            }

            if (AllChildrenFinished())
            {
                return TaskStatus.Success;
            }

            return TaskStatus.Running;
        }
    }
}