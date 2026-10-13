using UnityEngine;

using Game.Cozyffee.TaskSystem;

namespace Game.Cozyffee.ExternalBehaviorTrees
{
    [CreateAssetMenu(menuName = "Cozyffee/AI/BehaviorTrees/OrderFood")]
    public class OrderFood : ExternalBehaviorTree
    {
        public string AreaId = "OrderArea";

        public Sprite CinnamonBubble;

        [Tooltip("Placeholder: how long the NPC waits at the order area, and how long the bubble is shown.")]
        public float OrderWaitTime = 10f;

        public override Task CreateBehaviorTree()
        {
            RandomChoice randomChoice = new RandomChoice();
            randomChoice.AddTask(CreateWander(), 19.0f);
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
            ParallelSelector greet = new();

            goToOrderArea.AddTask(new PickRandomDestinationInArea { Destination = orderDestination, AreaId = AreaId });
            goToOrderArea.AddTask(new GoTo { Destination = orderDestination });

            greet.AddTask(new LookAtPlayer { KeepLooking = true });
            greet.AddTask(new Wait(OrderWaitTime, 0.0f)); // placeholder

            sequence.AddTask(goToOrderArea);
            sequence.AddTask(new SayImage(CinnamonBubble, OrderWaitTime)); // shown once, not every frame
            sequence.AddTask(greet);

            return sequence;
        }

        public override void CreateData(BehaviorTreeData data)
        {
        }
    }
}