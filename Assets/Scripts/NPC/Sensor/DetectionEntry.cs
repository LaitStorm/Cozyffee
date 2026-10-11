using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee
{
    [System.Serializable]
    public class DetectionEntry : System.ICloneable
    {
        public NPC NPC;

        public bool Alive = true;

        public bool Visible;

        public float Distance = 0f;

        public Vector3 LastKnownPosition;

        public float LastSeenTime = 0f;

        public bool Hostile = false;

        public object Clone()
        {
            DetectionEntry clone = new DetectionEntry
            {
                NPC = this.NPC,
                Alive = this.Alive,
                Visible = this.Visible,
                Distance = this.Distance,
                LastKnownPosition = this.LastKnownPosition,
                LastSeenTime = this.LastSeenTime,
                Hostile = this.Hostile
            };

            return clone;
        }
    }
}