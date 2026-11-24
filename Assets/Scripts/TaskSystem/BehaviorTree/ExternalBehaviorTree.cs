using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public abstract class ExternalBehaviorTree : ScriptableObject
    {
        public abstract Task CreateBehaviorTree(BehaviorTree behaviorTree);
    }
}