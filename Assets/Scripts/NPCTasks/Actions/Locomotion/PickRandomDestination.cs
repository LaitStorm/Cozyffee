using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using Game.Cozyffee.TaskSystem;

namespace Game.Cozyffee
{
	public class PickRandomDestination : NPCAction
	{
		public SharedFloat Radius = 20.0f;
		public SharedVector3 Destination = new SharedVector3();

		protected override TaskStatus OnUpdate()
		{
			for (int i = 0; i < 1000; i++)
			{
				Vector3 randomDirection = Random.insideUnitSphere * Radius.Value;
				randomDirection += npc.Position;

				if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, 5f, NavMesh.AllAreas))
				{
					Destination.Value = hit.position;
					return TaskStatus.Success;
				}
			}

			return TaskStatus.Failure;
		}
	}
}