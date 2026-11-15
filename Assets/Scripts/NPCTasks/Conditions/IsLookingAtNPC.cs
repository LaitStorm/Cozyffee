using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public class IsLookingAtNPC : NPCCondition
    {
        public SharedNPC Target = new SharedNPC();

        public IsLookingAtNPC(){}

        public IsLookingAtNPC(SharedNPC target)
        {
            Target = target;
        }

        protected override TaskStatus OnUpdate()
        {
            if (Target == null)
            {
                Debug.LogError("Target is null");
                return TaskStatus.Failure;
            }

            if (Target.Value == null)
            {
                Debug.LogError("Target.Value is null");
                return TaskStatus.Failure;
            }

            return npc.IsLookingAt(Target.Value.SensorPosition) ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}