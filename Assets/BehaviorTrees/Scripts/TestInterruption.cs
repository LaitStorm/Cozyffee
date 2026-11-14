using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Cozyffee.TaskSystem;

namespace Game.Cozyffee.ExternalBehaviorTrees
{
    [CreateAssetMenu(menuName = "Cozyffee/AI/BehaviorTrees/Test/InterruptionTest")]
    public class TestInterruption : ExternalBehaviorTree
    {
        public override Task CreateBehaviorTree()
        {
            Selector selector = new Selector();

            

            return selector;
        }
    }
}