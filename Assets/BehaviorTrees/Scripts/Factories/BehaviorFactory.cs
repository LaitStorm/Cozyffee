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

        /// <param name="avoidAreaId">If set, the NPC never picks wander destinations inside an Area with this ID.</param>
        public static Task Wander(WanderParameters p, string avoidAreaId = null)
        {
            Sequence sequence = new Sequence();

            SharedVector3 wanderDestination = new();

            Sequence moveSequence = new Sequence() { Name = "Move" };
            moveSequence.AddTask(new PickRandomDestination(p.SearchRadius, wanderDestination) { AvoidAreaId = avoidAreaId ?? "" });
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

        public static Task Wander()
        {
            return Wander(new WanderParameters
            {
                SearchRadius = 20,

                MaxWanderTime = 5,
                MaxWanderTimeRandom = 25,
                
                MaxWaitTime = 1,
                MaxWaitTimeRandom = 5
            });
        }

        /// <summary>
        /// Forever: waits <paramref name="interval"/> (+ random) seconds, then lowers hunger by <paramref name="amount"/> (+ random).
        /// </summary>
        public static Task HungerTick(float interval, float intervalRandom, float amount, float amountRandom)
        {
            Sequence sequence = new Sequence(
                new Wait(interval, intervalRandom),
                new NPCHungry(amount, amountRandom)
            );

            Repeater repeater = new Repeater(sequence)
            {
                Name = "Hunger Tick",
            };
            repeater.SetFoldoutVisible(false);

            return repeater;
        }

        /// <summary>
        /// Walks to a random point in the Area with <paramref name="areaId"/>, shows the order bubble,
        /// waits for the item (or until <paramref name="waitTime"/> runs out), then runs <paramref name="eatTask"/>
        /// (defaults to <see cref="ConsumeItem"/>).
        /// </summary>
        public static Task Order(string areaId, string itemId, Sprite bubble, float waitTime, Task eatTask = null)
        {
            Sequence sequence = new Sequence() { Name = "Order" };
            Sequence goToOrderArea = new Sequence() { Name = "Go to Order Area" };
            SharedVector3 orderDestination = new();
            ParallelSelector waitForItem = new();

            goToOrderArea.AddTask(new PickRandomDestinationInAreaID { Destination = orderDestination, AreaId = areaId });
            goToOrderArea.AddTask(new GoTo { Destination = orderDestination });

            waitForItem.AddTask(new LookAtPlayer { KeepLooking = true });
            waitForItem.AddTask(new ReceiveItem { ItemId = itemId });
            waitForItem.AddTask(new Wait(waitTime, 0.0f));

            sequence.AddTask(goToOrderArea);
            sequence.AddTask(new SayImage(bubble, waitTime));
            sequence.AddTask(waitForItem);
            sequence.AddTask(new HideBubble());
            sequence.AddTask(eatTask ?? new ConsumeItem());

            return sequence;
        }
    }
}