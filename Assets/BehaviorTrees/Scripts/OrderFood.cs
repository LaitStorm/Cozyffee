using UnityEngine;

using Game.Cozyffee.TaskSystem;

namespace Game.Cozyffee.ExternalBehaviorTrees
{
    [CreateAssetMenu(menuName = "Cozyffee/AI/BehaviorTrees/OrderFood")]
    public class OrderFood : ExternalBehaviorTree
    {
        public string AreaId = "OrderArea";

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
            Sequence sequence = new Sequence() { Name = "Order" };
            Sequence goToOrderArea = new Sequence() { Name = "Go to Order Area" };
            SharedVector3 orderDestination = new();
            ParallelSelector waitForItem = new();

            goToOrderArea.AddTask(new PickRandomDestinationInAreaID { Destination = orderDestination, AreaId = AreaId });
            goToOrderArea.AddTask(new GoTo { Destination = orderDestination });

            waitForItem.AddTask(new LookAtPlayer { KeepLooking = true });
            waitForItem.AddTask(new ReceiveItem { ItemId = ItemId });
            waitForItem.AddTask(new Wait(OrderWaitTime, 0.0f));

            sequence.AddTask(goToOrderArea);
            sequence.AddTask(new SayImage(CinnamonBubble, OrderWaitTime));
            sequence.AddTask(waitForItem);
            sequence.AddTask(new HideBubble());
            sequence.AddTask(new ConsumeItem());

            return sequence;
        }
    }
}