using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public class HeardNewNoise : NPCCondition
    {
        public SharedBool OnlyCombatNoise = false;
        public SharedBool OnlyCoffeeNoise = false;
        
        protected override TaskStatus OnUpdate()
        {
            if (sensor.NoiseHeard == false)
            {
                return TaskStatus.Failure;
            }

            if (OnlyCombatNoise.Value && sensor.IsCombatNoise == false)
            {
                return TaskStatus.Failure;
            }
            if (OnlyCoffeeNoise.Value && sensor.IsCoffeeMachineNoise == false)
            {
                return TaskStatus.Failure;
            }

            if (sensor.TimeSinceHeardNoise <= 0.1f)
            {
                return TaskStatus.Success;
            }

            return TaskStatus.Failure;
        }
    }
}