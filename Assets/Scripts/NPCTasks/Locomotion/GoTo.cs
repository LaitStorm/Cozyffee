using Game.Cozyffee.TaskSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee
{
	public class GoTo : NPCAction
	{
		public SharedVector3 Destination;

		protected override void OnEntry()
		{
			npc.CanMove = true;
		}

		protected override void OnExit()
		{
			npc.CanMove = false;
		}

		protected override TaskStatus OnUpdate()
		{
			npc.SetDestination?.Invoke(Destination.Value);

			if (npc.HasPathToDestination() == false)
			{
				return TaskStatus.Failure;
			}

			if (HasArrived())
			{
				return TaskStatus.Success;
			}

			return TaskStatus.Running;
		}

		bool HasArrived()
		{
			return Vector3.Distance(npc.Position, Destination.Value) <= 1f;
		}

	}
}