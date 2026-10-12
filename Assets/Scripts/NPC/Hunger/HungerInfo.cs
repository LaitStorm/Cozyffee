using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee
{
    public struct HungerInfo
    {
        public float HungerFill;

        public static implicit operator HungerInfo(float hungerFill)
        {
            return new HungerInfo{HungerFill = hungerFill};
        }
    }
}