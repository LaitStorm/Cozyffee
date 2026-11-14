using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public class SharedNPC : SharedVariable<NPC>
    {
        public static implicit operator SharedNPC(NPC value)
        {
            return new SharedNPC { Value = value};
        }
    }

    public class SharedDetectionEntry : SharedVariable<DetectionEntry>
    {
        public static implicit operator SharedDetectionEntry(DetectionEntry value)
        {
            return new SharedDetectionEntry { Value = value };
        }
    }
}