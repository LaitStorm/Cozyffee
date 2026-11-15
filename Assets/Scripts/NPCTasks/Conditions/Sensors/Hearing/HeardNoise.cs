using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public class HeardNoise : NPCCondition
    {
        public SharedBool OnlyCombatNoise = false;
        public SharedBool OnlyCoffeeNoise = false;

        protected override TaskStatus OnUpdate()
        {
            if (OnlyCombatNoise.Value && sensor.IsCombatNoise == false)
            {
                return TaskStatus.Failure;
            }
            if (OnlyCoffeeNoise.Value && sensor.IsCoffeeMachineNoise == false)
            {
                return TaskStatus.Failure;
            }
            
            return sensor.NoiseHeard ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}