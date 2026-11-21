using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public class HasHostiles : NPCCondition
    {
        protected override TaskStatus OnUpdate()
        {
            if (sensor.HostilesCount > 0)
            {
                return TaskStatus.Success;
            }

            return TaskStatus.Failure;
        }
    }
}