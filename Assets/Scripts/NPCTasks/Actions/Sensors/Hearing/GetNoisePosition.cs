using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public class GetNoisePosition : NPCAction
    {
        public SharedVector3 NoisePosition = new SharedVector3();

        public GetNoisePosition() {}

        public GetNoisePosition(SharedVector3 noisePosition)
        {
            NoisePosition = noisePosition;
        }

        protected override TaskStatus OnUpdate()
        {
            NoisePosition.Value = sensor.NoisePosition;
            return TaskStatus.Success;
        }
    }
}