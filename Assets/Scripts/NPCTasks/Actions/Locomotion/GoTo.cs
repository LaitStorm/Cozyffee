using Game.Cozyffee.TaskSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee
{
	public class GoTo : NPCAction
	{
		public SharedVector3 Destination;
		public SharedFloat ArriveDistance = 1f;
		public SharedBool Run = false;

		public GoTo()
		{
			
		}

		public GoTo(SharedVector3 destination)
		{
			Destination = destination;
		}

		public GoTo(SharedVector3 destination, SharedBool run)
		{
			Destination = destination;
			Run = run;
		}

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

			if (npc.HasPathToDestination == null || !npc.HasPathToDestination())
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
			return Vector3.Distance(npc.Position, Destination.Value) <= ArriveDistance.Value;
		}

	}
}