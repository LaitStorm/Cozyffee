using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public class MakeNoise : NPCAction
    {
        public SharedFloat Radius = 5f;

        protected override TaskStatus OnUpdate()
        {
            npc.MakeNoise(new NoiseInfo { Position = npc.Position, Radius = Radius.Value });
            return TaskStatus.Success;
        }
    }
}