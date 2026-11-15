using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee
{
    public enum NoiseType
    {
        Common,
        GunFire,
        BulletImpact,
        CoffeeMachine
    }
    public struct NoiseInfo
    {
        public NPC Owner;
        public NoiseType Type;
        public Vector3 Position;
        public float Radius;
    }
}