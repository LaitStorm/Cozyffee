using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee
{
    public interface ICanHunger
    {
        void Hungry(HungerInfo hungerInfo);
    }
}