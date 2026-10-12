using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee
{
    public interface IHunger
    {
        float Points { get; set; }
        float MaxPoints { get; set; }
        bool CanOrder { get; }
    }
}