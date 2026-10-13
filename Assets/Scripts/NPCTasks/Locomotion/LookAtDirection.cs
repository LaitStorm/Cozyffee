using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public class LookAtDirection : NPCAction
    {
        public SharedVector3 Direction;

        protected override TaskStatus OnUpdate()
        {
            Vector3 targetPosition = npc.SensorPosition + Direction.Value * 100.0f;
            npc.LookAt?.Invoke(targetPosition);

            if (npc.IsLookingAt(targetPosition))
            {
                return TaskStatus.Success;
            }

            return TaskStatus.Running;
        }
    }
}