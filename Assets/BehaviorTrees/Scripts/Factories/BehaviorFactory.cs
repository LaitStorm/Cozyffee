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
        public static Task Order(string areaId, string itemId, Sprite bubble, float waitTime, Task eatTask = null, WanderParameters waitingWander = null,
                                 float orderAnimationInterval = DefaultOrderAnimationInterval)
        {
            Sequence sequence = new Sequence() { Name = "Order" };
            AddOrderSteps(sequence, areaId, itemId, itemId, bubble, waitTime, eatTask ?? new ConsumeItem(), waitingWander, orderAnimationInterval);

            return new OrderScope(sequence) { Name = "Single Item Order" };
        }

        /// <summary>
        /// Like <see cref="Order(string, string, Sprite, float, Task)"/>, but each time it runs the NPC picks
        /// a random item from <paramref name="menu"/> and asks for that one, showing its icon.
        /// If <paramref name="eatForHunger"/> is true, the NPC eats the item and regains its
        /// <see cref="CafeMenuItem.HungerFill"/>; otherwise the item is just consumed.
        /// While every order spot is taken, the NPC wanders with <paramref name="waitingWander"/> (or stands still if null).
        /// </summary>
        public static Task Order(string areaId, CafeMenu menu, float waitTime, bool eatForHunger = true, WanderParameters waitingWander = null,
                                 float orderAnimationInterval = DefaultOrderAnimationInterval)
        {
            // One set of variables per order task, so each NPC's tree tracks its own order.
            SharedString itemId = new();
            SharedString displayName = new();
            SharedSprite icon = new();
            SharedFloat hungerFill = new();

            Sequence sequence = new Sequence() { Name = "Order" };
            sequence.AddTask(new PickOrder(menu, itemId, displayName, icon, hungerFill) { Name = "Pick Order" });

            Task eatTask = eatForHunger ? new EatItem(hungerFill) : new ConsumeItem();
            AddOrderSteps(sequence, areaId, itemId, displayName, icon, waitTime, eatTask, waitingWander, orderAnimationInterval);

            return new OrderScope(sequence) { Name = "Menu Order" };
        }

        /// <summary>Seconds between "Ordering" animations while an NPC waits for its item, when the caller doesn't say.</summary>
        public const float DefaultOrderAnimationInterval = 8f;

        static void AddOrderSteps(Sequence sequence, string areaId, SharedString itemId, SharedString displayName, SharedSprite icon, float waitTime, Task eatTask, WanderParameters waitingWander,
                                  float orderAnimationInterval)
        {
            Sequence goToOrderArea = new Sequence() { Name = "Go to Order Area" };
            SharedVector3 orderDestination = new();
            ParallelSelector waitForItem = new();

            // Reserve a free order spot (one NPC per spot). If they're all taken, wait for one, wandering outside the area.
            Task claimSpot = new ClaimOrderSpot { Destination = orderDestination, AreaId = areaId, Name = "Claim Order Spot" };

            if (waitingWander != null)
            {
                claimSpot = new ParallelSelector(claimSpot, Wander(waitingWander, areaId)) { Name = "Wait for a Free Spot" };
            }

            goToOrderArea.AddTask(claimSpot);
            goToOrderArea.AddTask(new GoTo { Destination = orderDestination });

            // Walks back to the spot if a passing NPC pushes it away while it waits
            waitForItem.AddTask(new HoldPosition(orderDestination) { Name = "Hold Order Spot" });
            waitForItem.AddTask(new LookAtPlayer { KeepLooking = true });
            waitForItem.AddTask(new ReceiveItem { ItemId = itemId });
            waitForItem.AddTask(new Wait(waitTime, 0.0f));

            // Play the "Ordering" animation as soon as the order starts, then again every orderAnimationInterval seconds.
            // 0 or less: play it only once.
            Sequence orderAnimation = new Sequence() { Name = "Ordering Animation" };
            orderAnimation.AddTask(new PlayAnimation("Ordering"));

            if (orderAnimationInterval > 0f)
            {
                orderAnimation.AddTask(new Wait(orderAnimationInterval));
                waitForItem.AddTask(new Repeater(orderAnimation) { Name = "Repeat Ordering Animation" });
            }
            else
            {
                // A ParallelSelector ends when any child succeeds, so keep this branch Running after playing once
                orderAnimation.AddTask(new Idle());
                waitForItem.AddTask(orderAnimation);
            }

            // Show the item's icon. Until an icon exists, fall back to saying its name so the order still works.
            Selector showOrder = new Selector(
                new SayImage(icon, waitTime),
                new Say(displayName)
            ) { Name = "Show Order" };

            sequence.AddTask(goToOrderArea);
            sequence.AddTask(new AnnounceOrder { Name = "Announce Order" });
            sequence.AddTask(showOrder);
            sequence.AddTask(waitForItem);
            sequence.AddTask(new HideBubble());
            sequence.AddTask(eatTask);
        }
    }
}