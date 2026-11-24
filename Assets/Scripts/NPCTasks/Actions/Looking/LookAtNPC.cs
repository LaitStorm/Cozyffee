using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Cozyffee.TaskSystem;

namespace Game.Cozyffee
{
    public class LookAtNPC : NPCAction
    {
        public SharedNPC Target;

        protected override TaskStatus OnUpdate()
        {
            if (Target == null || Target.Value == null)
            {
                return TaskStatus.Failure;
            }

            Vector3 targetPosition = Target.Value.SensorPosition;

            npc.LookAt?.Invoke(targetPosition);

            if (npc.IsLookingAt(targetPosition))
            {
                return TaskStatus.Success;
            }

            return TaskStatus.Running;
        }
    }
}