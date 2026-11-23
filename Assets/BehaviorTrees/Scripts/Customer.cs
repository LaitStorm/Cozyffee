using UnityEngine;
using Game.Cozyffee.TaskSystem;

namespace Game.Cozyffee.ExternalBehaviorTrees
{
    /// <summary>
    /// Hunger-driven customer:
    /// - Idle (default): wanders outside the order area while hunger slowly ticks down.
    ///   Every so often, if well fed, it rolls a chance to get sleepy.
    /// - Order: when hunger drops below <see cref="HungryThreshold"/>, walks to the order area and orders.
    /// - Sleeping: when well fed (above <see cref="SleepyThreshold"/>) and the sleep roll passed,
    ///   walks somewhere outside the order area and sleeps, losing hunger more slowly, then goes back to Idle.
    /// </summary>
    [CreateAssetMenu(menuName = "Cozyffee/AI/BehaviorTrees/Customer")]
    public class Customer : ExternalBehaviorTree
    {
        const float OrderUtility = 10f;
        const float SleepUtility = 5f;
        const float IdleUtility = 1f;

        [SerializeField]
        public WanderParameters wanderParameters;
        public string AreaId = "OrderArea";

        [Header("Order")]
        [Tooltip("What NPCs can order. Each order picks a random available item from it. Leave empty to use the single item below.")]
        public CafeMenu Menu;

        [Tooltip("How long the NPC waits at the order area for its item.")]
        public float OrderWaitTime = 600f;

        [Tooltip("Seconds between 'Ordering' animations while waiting for the item. The first one plays as soon as the order starts. 0 = play it only once.")]
        [Min(0f)]
        public float OrderAnimationInterval = BehaviorFactory.DefaultOrderAnimationInterval;

        [Header("Single Item (used only when Menu is empty)")]
        public Sprite OrderBubble;

        [Tooltip("ID of the Item the NPC asks for.")]
        public string ItemId = "cinnamon_roll";

        [Tooltip("Hunger Points restored when the NPC eats its item (clamped to MaxPoints). With a Menu, each item's own Hunger Fill is used.")]
        public float FillAmount = 60f;

        [Header("Hunger Thresholds")]
        [Tooltip("Below this many Hunger Points, the NPC goes to order.")]
        public float HungryThreshold = 30f;

        [Tooltip("Above this many Hunger Points, the NPC may get sleepy.")]
        public float SleepyThreshold = 80f;

        [Header("Idle Hunger Tick")]
        public float IdleHungerInterval = 3f;
        public float IdleHungerIntervalRandom = 4f;
        public float IdleHungerAmount = 3f;
        public float IdleHungerAmountRandom = 5f;

        [Header("Sleep")]
        [Range(0f, 1f), Tooltip("Chance of getting sleepy on each sleep check while well fed.")]
        public float SleepChance = 0.3f;

        [Tooltip("Seconds between sleep checks while idle.")]
        public float SleepCheckInterval = 10f;

        public float SleepDuration = 20f;
        public float SleepDurationRandom = 10f;

        [Header("Sleep Hunger Tick")]
        public float SleepHungerInterval = 5f;
        public float SleepHungerIntervalRandom = 3f;
        public float SleepHungerAmount = 0.5f;
        public float SleepHungerAmountRandom = 1f;

        public override Task CreateBehaviorTree(BehaviorTree behaviorTree)
        {
            // Per-NPC state: this ScriptableObject is shared by every NPC that uses it.
            SharedBool wantsToSleep = false;

            UtilitySelector root = new UtilitySelector { Name = "Customer" };
            root.AddTask(CreateOrderBranch());
            root.AddTask(CreateSleepingBranch(wantsToSleep));
            root.AddTask(CreateIdleBranch(wantsToSleep));

            // When Order or Sleeping finishes, the selector succeeds and restarts, so it falls back to Idle.
            return new Repeater(root) { Name = "Customer Loop" };
        }

        Task CreateIdleBranch(SharedBool wantsToSleep)
        {
            ParallelSelector branch = new ParallelSelector
            {
                Name = "Idle Branch",
                UtilityOverride = _ => IdleUtility,
            };

            CustomAction getSleepy = new CustomAction
            {
                Name = "Get Sleepy",
                Update = () =>
                {
                    wantsToSleep.Value = true;
                    return TaskStatus.Success;
                }
            };

            // Every SleepCheckInterval: if well fed (Points > SleepyThreshold), roll SleepChance to get sleepy.
            // A failed condition fails the sequence, and the Repeater just tries again next interval.
            Sequence sleepRoll = new Sequence { Name = "Sleep Roll" };
            sleepRoll.AddTask(new Wait(SleepCheckInterval));
            sleepRoll.AddTask(new IsHungry(SleepyThreshold) { Invert = true });
            sleepRoll.AddTask(new RandomChance(getSleepy, SleepChance));

            Repeater sleepCheck = new Repeater(sleepRoll)
            {
                Name = "Sleep Check",
                FoldoutVisible = false,
            };

            branch.AddTask(BehaviorFactory.Wander(wanderParameters, AreaId));
            branch.AddTask(BehaviorFactory.HungerTick(IdleHungerInterval, IdleHungerIntervalRandom, IdleHungerAmount, IdleHungerAmountRandom));
            branch.AddTask(sleepCheck);

            return branch;
        }

        Task CreateOrderBranch()
        {
            Sequence branch = new Sequence
            {
                Name = "Order Branch",
                UtilityOverride = task => UtilityIfConditionsMet(task, OrderUtility),
            };

            // Picked while Points <= HungryThreshold. Hunger only rises when EatItem runs as the last step,
            // so the condition holds for the whole order.
            branch.AddCondition(new IsHungry(HungryThreshold));

            Task order = Menu != null
                ? BehaviorFactory.Order(AreaId, Menu, OrderWaitTime, eatForHunger: true, waitingWander: wanderParameters,
                                        orderAnimationInterval: OrderAnimationInterval)
                : BehaviorFactory.Order(AreaId, ItemId, OrderBubble, OrderWaitTime, new EatItem(FillAmount), wanderParameters,
                                        OrderAnimationInterval);

            branch.AddTask(order);

            return branch;
        }

        Task CreateSleepingBranch(SharedBool wantsToSleep)
        {
            Sequence branch = new Sequence
            {
                Name = "Sleeping Branch",
                UtilityOverride = task => UtilityIfConditionsMet(task, SleepUtility),
            };

            // The flag is only set while well fed (see Sleep Roll) and stays set until the NPC wakes up,
            // so hunger ticking below SleepyThreshold mid-sleep doesn't cut the nap short.
            branch.AddCondition(new IsTrue(wantsToSleep));

            SharedVector3 sleepSpot = new();

            CustomAction wakeUp = new CustomAction
            {
                Name = "Wake Up",
                Update = () =>
                {
                    wantsToSleep.Value = false;
                    return TaskStatus.Success;
                }
            };

            branch.AddTask(new PickRandomDestination(wanderParameters.SearchRadius, sleepSpot) { AvoidAreaId = AreaId });
            branch.AddTask(new GoTo(sleepSpot));
            branch.AddTask(new ParallelSelector(
                new Sleep(SleepDuration, SleepDurationRandom),
                BehaviorFactory.HungerTick(SleepHungerInterval, SleepHungerIntervalRandom, SleepHungerAmount, SleepHungerAmountRandom)
            ) { Name = "Sleep" });
            branch.AddTask(wakeUp);

            return branch;
        }

        /// <summary>
        /// Returns <paramref name="utility"/> when the branch's own conditions pass, 0 otherwise.
        /// The same conditions also abort the branch if they stop passing while it runs.
        /// </summary>
        static float UtilityIfConditionsMet(Task task, float utility)
        {
            return task is Composite composite && composite.EvaluateConditions() ? utility : 0f;
        }
    }
}
