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

        public override Task CreateBehaviorTree()
        {
            RandomChoice randomChoice = new RandomChoice();
            randomChoice.AddTask(CreateWander(), 9.0f);
            randomChoice.AddTask(CreateOrderThenWander(), 1.0f);

            Sequence sequence = new Sequence();
            sequence.AddTask(CreateWander());
            sequence.AddTask(CreateOrderThenWander());
            sequence.AddTask(new Repeater(randomChoice));

            return sequence;
        }

        public Task CreateOrderThenWander()
        {
            Sequence orderThenWander = new Sequence() { Name = "Order then wander" };
            orderThenWander.AddTask(CreateOrder());
            orderThenWander.AddTask(CreateWander());

            return orderThenWander;
        }

        public Task CreateWander()
        {
            Sequence wanderSequence = new Sequence();
            SharedVector3 wanderDestination = new();
            ParallelSelector parallelSelector = new();

            Sequence moveSequence = new Sequence() { Name = "Wander" };
            moveSequence.AddTask(new PickRandomDestination { Destination = wanderDestination });
            moveSequence.AddTask(new GoTo { Destination = wanderDestination });

            parallelSelector.AddTask(new Repeater(moveSequence));
            parallelSelector.AddTask(new Wait(2f, 3f));

            wanderSequence.AddTask(new Wait(1f, 3f));
            wanderSequence.AddTask(parallelSelector);

            return wanderSequence;
        }

        public Task CreateOrder()
        {
            Sequence sequence = new Sequence() { Name = "Order" };
            Sequence goToOrderArea = new Sequence() { Name = "Go to Order Area" };
            SharedVector3 orderDestination = new();
            ParallelSelector waitForItem = new();

            goToOrderArea.AddTask(new PickRandomDestinationInArea { Destination = orderDestination, AreaId = AreaId });
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

        public override void CreateData(BehaviorTreeData data)
        {
        }
    }
}