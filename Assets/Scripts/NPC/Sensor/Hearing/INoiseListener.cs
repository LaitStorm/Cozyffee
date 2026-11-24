using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Cozyffee
{
    public interface INoiseListener
    {
        void OnNoiseHeard(NoiseInfo noise);
    }
}