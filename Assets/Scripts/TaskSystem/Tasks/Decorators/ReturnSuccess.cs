using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public class ReturnSuccess : Decorator
    {
        public ReturnSuccess(Task childTask) : base(childTask) {}

        protected override TaskStatus OnUpdate()
        {
            if (child == null)
            {
                return TaskStatus.Failure;
            }

            TaskStatus status = Task.Update(child);

            if (status == TaskStatus.Success || status == TaskStatus.Failure)
            {
                Task.Stop(child);
                return TaskStatus.Success;
            }

            return TaskStatus.Running;
        }
    }
}