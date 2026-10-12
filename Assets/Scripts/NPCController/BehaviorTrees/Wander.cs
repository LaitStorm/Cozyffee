using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

using Game.Cozyffee.TaskSystem;

namespace Game.Cozyffee.ExternalBehaviorTrees
{
	[CreateAssetMenu(menuName = "Cozyffee/AI/BehaviorTrees/Wander")]
	public class Wander : ExternalBehaviorTree
	{
		public enum EVersion
		{
			V1,
			V2
		}

		public EVersion Version = EVersion.V2;

		public override Task CreateBehaviorTree()
		{
			if (Version == EVersion.V1)
			{
				return CreateV1();
			}
			else if (Version == EVersion.V2)
			{
				return CreateV2();
			}

			return null;
		}

		public override void CreateData(BehaviorTreeData data)
		{
		}

		Task CreateV1()
		{
			Sequence sequence = new Sequence();

			SharedVector3 wanderDestination = new();

			sequence.AddTask(new PickRandomDestination { Destination = wanderDestination });
			sequence.AddTask(new GoTo { Destination = wanderDestination });
			sequence.AddTask(new Wait(1f, 3f));

			return new Repeater(sequence);
		}
		
		Task CreateV2()
		{
			Sequence sequence = new Sequence();
			SharedVector3 wanderDestination = new();
			ParallelSelector parallelSelector = new();

			Sequence moveSequence = new Sequence() { Name = "Move" };
			moveSequence.AddTask(new PickRandomDestination { Destination = wanderDestination });
			moveSequence.AddTask(new GoTo { Destination = wanderDestination });

			parallelSelector.AddTask(new Repeater(moveSequence));
			parallelSelector.AddTask(new Wait(2f, 3f));

			sequence.AddTask(new Wait(1f, 3f));
			sequence.AddTask(parallelSelector);

			return new Repeater(sequence);
		}
		
	}
}