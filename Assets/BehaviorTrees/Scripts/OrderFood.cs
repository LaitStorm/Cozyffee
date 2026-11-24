using UnityEngine;

using Game.Cozyffee.TaskSystem;

namespace Game.Cozyffee.ExternalBehaviorTrees
{
    [CreateAssetMenu(menuName = "Cozyffee/AI/BehaviorTrees/OrderFood")]
    public class OrderFood : ExternalBehaviorTree
    {
        public string AreaId = "OrderArea";

        [Tooltip("What NPCs can order. Each order picks a random available item from it. Leave empty to use the single item below.")]
        public CafeMenu Menu;

        [Header("Single Item (used only when Menu is empty)")]
        public Sprite CinnamonBubble;

        [Tooltip("ID of the Item the NPC asks for.")]
        public string ItemId = "cinnamon_roll";

        [Tooltip("Placeholder: how long the NPC waits at the order area.")]
        public float OrderWaitTime = 600f;

        public override Task CreateBehaviorTree(BehaviorTree behaviorTree)
        {
            WanderParameters wanderParameters = new WanderParameters();

            SharedNPC npc = new SharedNPC();
            npc.Value = behaviorTree.GetComponentInParent<NPC>();
            behaviorTree.Data.SetNPC("npc", npc);

            RandomChoice randomChoice = new RandomChoice();
            randomChoice.AddTask(BehaviorFactory.Wander(wanderParameters), 9.0f);
            randomChoice.AddTask(CreateOrderThenWander(wanderParameters), 1.0f);

            Sequence sequence = new Sequence();
            sequence.AddTask(BehaviorFactory.Wander(wanderParameters));
            sequence.AddTask(CreateOrderThenWander(wanderParameters));
            sequence.AddTask(new Repeater(randomChoice));

            return sequence;
        }

        public Task CreateOrderThenWander(WanderParameters wanderParameters)
        {
            Sequence orderThenWander = new Sequence() { Name = "Order then wander" };
            orderThenWander.AddTask(CreateOrder());
            orderThenWander.AddTask(BehaviorFactory.Wander(wanderParameters));

            return orderThenWander;
        }

        public Task CreateOrder()
        {
            return Menu != null
                ? BehaviorFactory.Order(AreaId, Menu, OrderWaitTime, eatForHunger: false)
                : BehaviorFactory.Order(AreaId, ItemId, CinnamonBubble, OrderWaitTime);
        }
    }
}