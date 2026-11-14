using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Cozyffee.TaskSystem;
using UnityEngine.InputSystem.Processors;

namespace Game.Cozyffee.ExternalBehaviorTrees
{
    [CreateAssetMenu(menuName = "Cozyffee/AI/BehaviorTrees/Test/InterruptionTest")]
    public class TestInterruption : ExternalBehaviorTree
    {
        public override Task CreateBehaviorTree()
        {
            Selector selector = new Selector();

            selector.AddTask(CreateOrderBranch());
            selector.AddTask(CreateIdleBranch());

            return selector;
        }

        Task CreateOrderBranch()
        {
            Sequence sequence = new Sequence();

            // sequence.InterruptLowerPriority = true;

            sequence.AddCondition(new IsHungry());

            sequence.AddTask(new Say("AAAAAAAAAAAA"));

            sequence.AddTask(new LookAtPlayer { KeepLooking = true });

            return sequence;
        }

        Task CreateIdleBranch()
        {
            Interrupt interrupt = new Interrupt(new Idle());

            interrupt.AddCondition(new IsHungry { Invert = true });

            return interrupt;
        }
    }
}