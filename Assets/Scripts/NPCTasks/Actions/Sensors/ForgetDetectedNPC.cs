using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public class ForgetDetectedNPC : NPCAction
    {
        public SharedNPC Target = new();

        protected override TaskStatus OnUpdate()
        {
            if (Target.Value == null)
            {
                Debug.LogError($"{GetType().Name} action: entry is null!");

                return TaskStatus.Failure;
            }

            sensor.DetectionList.Remove(Target.Value);

            Target.Value = null;

            return TaskStatus.Success;
        }
    }
}