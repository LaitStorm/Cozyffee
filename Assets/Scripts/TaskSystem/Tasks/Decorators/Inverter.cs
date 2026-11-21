using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public class Inverter : Decorator
    {
        public Inverter(Task childTask) : base(childTask) {}

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
                return status == TaskStatus.Success ? TaskStatus.Failure : TaskStatus.Success;
            }

            return TaskStatus.Running;
        }
    }
}