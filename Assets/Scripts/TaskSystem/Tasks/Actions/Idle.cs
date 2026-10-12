using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public class Idle : Action
    {
        protected override TaskStatus OnUpdate()
        {
            return TaskStatus.Running;
        }
    }
}