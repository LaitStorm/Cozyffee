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
        /// Walks to a random point in the Area with <paramref name="areaId"/>, shows the order bubble and
        /// waits for the item (or until <paramref name="waitTime"/> runs out). Once served, the NPC takes and eats it
        /// (see <see cref="AddOrderSteps"/>) and finally runs <paramref name="eatTask"/>, if any (e.g. <see cref="EatItem"/>
        /// to restore hunger). <paramref name="mood"/> sets how its emotion changes while waiting and afterwards
        /// (null = the <see cref="OrderMood"/> defaults).
        /// </summary>
        public static OrderScope Order(string areaId, string itemId, Sprite bubble, float waitTime, Task eatTask = null, WanderParameters waitingWander = null,
                                    float orderAnimationInterval = DefaultOrderAnimationInterval, OrderMood mood = null)
        {
            mood ??= new OrderMood();

            Sequence sequence = new Sequence() { Name = "Order" };
            AddOrderSteps(sequence, areaId, itemId, itemId, bubble, waitTime, eatTask, waitingWander, orderAnimationInterval, mood);

            return new OrderScope(sequence, mood) { Name = "Single Item Order" };
        }

        /// <summary>
        /// Like <see cref="Order(string, string, Sprite, float, Task)"/>, but each time it runs the NPC picks
        /// a random item from <paramref name="menu"/> and asks for that one, showing its icon.
        /// If <paramref name="eatForHunger"/> is true, eating the item also restores its
        /// <see cref="CafeMenuItem.HungerFill"/>.
        /// While every order spot is taken, the NPC wanders with <paramref name="waitingWander"/> (or stands still if null).
        /// </summary>
        public static OrderScope Order(string areaId, CafeMenu menu, float waitTime, bool eatForHunger = true, WanderParameters waitingWander = null,
                                    float orderAnimationInterval = DefaultOrderAnimationInterval, OrderMood mood = null)
        {
            mood ??= new OrderMood();

            // One set of variables per order task, so each NPC's tree tracks its own order.
            SharedString itemId = new();
            SharedString displayName = new();
            SharedSprite icon = new();
            SharedFloat hungerFill = new();

            Sequence sequence = new Sequence() { Name = "Order" };
            sequence.AddTask(new PickOrder(menu, itemId, displayName, icon, hungerFill) { Name = "Pick Order" });

            Task eatTask = eatForHunger ? new EatItem(hungerFill) : null;
            AddOrderSteps(sequence, areaId, itemId, displayName, icon, waitTime, eatTask, waitingWander, orderAnimationInterval, mood);

            return new OrderScope(sequence, mood) { Name = "Menu Order" };
        }

        /// <summary>Seconds between "Ordering" animations while an NPC waits for its item, when the caller doesn't say.</summary>
        public const float DefaultOrderAnimationInterval = 8f;

        static void AddOrderSteps(Sequence sequence, string areaId, SharedString itemId, SharedString displayName, SharedSprite icon, float waitTime, Task eatTask, WanderParameters waitingWander,
                                float orderAnimationInterval, OrderMood mood)
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

            // The longer it waits, the sadder it gets: Crying, then Angry. Never succeeds (Idle), since that would end the wait.
            // Being served or giving up stops it; OrderScope then sets the emotion that follows.
            Sequence patience = new Sequence() { Name = "Patience" };
            patience.AddTask(new Wait(mood.CryingAfter) { Name = "Wait to Cry" });
            patience.AddTask(new SetEmotion(Emotion.Crying) { Name = "Start Crying" });
            patience.AddTask(new Wait(Mathf.Max(0f, mood.AngryAfter - mood.CryingAfter)) { Name = "Wait to Get Angry" });
            patience.AddTask(new SetEmotion(Emotion.Angry) { Name = "Get Angry" });
            patience.AddTask(new Idle());
            waitForItem.AddTask(patience);

            // Show the order's icon with its overlays (ice cream toppings). Orders that didn't come from a menu have no
            // CurrentOrder, so they show the plain icon. Until an icon exists, fall back to saying its name so the order still works.
            Selector showOrder = new Selector(
                new SayOrder(waitTime),
                new SayImage(icon, waitTime),
                new Say(displayName)
            ) { Name = "Show Order" };

            sequence.AddTask(goToOrderArea);
            sequence.AddTask(new AnnounceOrder { Name = "Announce Order" });
            sequence.AddTask(showOrder);
            sequence.AddTask(waitForItem);
            sequence.AddTask(new HideBubble());

            // Served: take the item (destroys it; NPC.AcceptFood plays the particles and sound, and OrderScope sets Neutral),
            // play one cycle of the Eating animation, then stay Happy for a while without blocking the tree (it goes back to
            // Neutral afterwards). If the order timed out there's nothing to take, ConsumeItem fails and none of this runs.
            sequence.AddTask(new ConsumeItem { Name = "Take Item" });
            sequence.AddTask(new PlayAnimation("Eating") { WaitUntilFinished = true, Name = "Eating Animation" });
            sequence.AddTask(new SetEmotion(Emotion.Happy) { Duration = mood.HappyDuration, Name = "Happy After Eating" });

            // Last on purpose: restoring hunger can end the branch that started this order (Customer only orders while
            // hungry), which would cut short anything after it.
            if (eatTask != null)
            {
                sequence.AddTask(eatTask);
            }
        }
    }
}