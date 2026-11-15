using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Cozyffee.TaskSystem;

namespace Game.Cozyffee
{
    public class BehaviorFactory
    {
        public static Task LookAround()
        {
            Repeater repeater = new Repeater
            {
                RepeatForever = false,
                Iterations = 3,
                IterationsRandom = 2,

                Name = "Look Around",
                FoldoutVisible = false,
            };

            Sequence sequence = new Sequence(new LookAtRandomAngle(), new Wait(1f, 2f));

            repeater.Child = sequence;

            return repeater;
        }

        public static Task Wander(WanderParameters p)
        {
            Sequence sequence = new Sequence();

            SharedVector3 wanderDestination = new();

            Sequence moveSequence = new Sequence() { Name = "Move" };
            moveSequence.AddTask(new PickRandomDestination(p.SearchRadius, wanderDestination));
            moveSequence.AddTask(new GoTo(wanderDestination));

            sequence.AddTask(new Wait(p. MaxWaitTime, p.MaxWaitTimeRandom));
            sequence.AddTask(new ParallelSelector(
                new Repeater(moveSequence),
                new Wait(p.MaxWanderTime, p.MaxWanderTimeRandom)
            ));

            Repeater root = new Repeater(sequence) {
                Name = "Wander",
                FoldoutVisible = false,
            };

            return root;
        }
    }
}