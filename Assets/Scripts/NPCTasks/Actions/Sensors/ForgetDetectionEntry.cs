using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Cozyffee.TaskSystem;

namespace Game.Cozyffee
{
    public class ForgetDetectionEntry : NPCAction
    {
        public SharedDetectionEntry Entry = new();

        protected override TaskStatus OnUpdate()
        {
            if (Entry.Value == null)
            {
                Debug.LogError($"{GetType().Name} action: entry is null!");

                return TaskStatus.Failure;
            }
            else if (Entry.Value.NPC == null)
            {
                Debug.LogError($"{GetType().Name} action: entry.NPC is null!");
            }

            sensor.DetectionList.Remove(Entry.Value.NPC);

            Entry.Value = null;

            return TaskStatus.Success;
        }
    }
}