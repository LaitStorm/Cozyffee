using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public class CanSeeNPC : NPCCondition
    {
        public SharedNPC Target = new SharedNPC();

        public CanSeeNPC()
        {
            
        }

        public CanSeeNPC(SharedNPC target)
        {
            Target = target;
        }

        protected override TaskStatus OnUpdate()
        {
            if (Target == null || Target.Value == null || sensor == null)
            {
                return TaskStatus.Failure;
            }

            return sensor.IsVisible(Target.Value) ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}