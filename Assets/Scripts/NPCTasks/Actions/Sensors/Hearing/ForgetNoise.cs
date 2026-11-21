using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public class ForgetNoise : NPCAction
    {
        protected override TaskStatus OnUpdate()
        {
            sensor.ForgetNoise();
            return TaskStatus.Success;
        }
    }
}