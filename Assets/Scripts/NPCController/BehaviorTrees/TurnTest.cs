using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Game.Cozyffee.TaskSystem;

namespace Game.Cozyffee.ExternalBehaviorTrees
{
    [CreateAssetMenu(menuName = "Cozyffee/AI/BehaviorTrees/Test/TurningTest")]
    public class TurnTest : ExternalBehaviorTree
    {
        public override Task CreateBehaviorTree()
        {
            Sequence sequence = new Sequence();
            sequence.AddTask(new LookAtRandomAngle());
            sequence.AddTask(new Wait(1f));

            SayRandom say = new SayRandom("Turning...", "Uh-uh...", "Yep", "What's over there?", "Let's see...", "Huh?");

            //sequence.AddTask(new RandomChance(say, 0.2f));
            sequence.AddTask(say);

            return new Repeater(sequence);
        }
    }
}