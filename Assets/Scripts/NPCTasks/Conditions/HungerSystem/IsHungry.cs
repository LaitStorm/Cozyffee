using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public class IsHungry : NPCCondition
    {
        public SharedFloat Threshold = 0f;

        public IsHungry()
        {
        }

        public IsHungry(SharedFloat threshold)
        {
            Threshold = threshold;
        }

        protected override TaskStatus OnUpdate()
        {
            if (npc == null || npc.Hunger == null || Threshold == null)
            {
                return TaskStatus.Failure;
            }

            return npc.Hunger.Points <= Threshold.Value ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}
