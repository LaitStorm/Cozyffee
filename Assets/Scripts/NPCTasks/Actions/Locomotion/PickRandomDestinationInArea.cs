using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Game.Cozyffee.TaskSystem;

namespace Game.Cozyffee
{
	public class PickRandomDestinationInArea : Action
	{
		public SharedVector3 AreaCenter = new SharedVector3();
		public SharedFloat AreaRadius = 20.0f;
		public SharedVector3 Destination = new SharedVector3();

		public PickRandomDestinationInArea()
		{
			
		}

		public PickRandomDestinationInArea(SharedVector3 areaCenter, SharedFloat areaRadius, SharedVector3 destination)
		{
			AreaCenter = areaCenter;
			AreaRadius = areaRadius;
			Destination = destination;
		}

		protected override TaskStatus OnUpdate()
		{
			for (int i = 0; i < 1000; i++)
			{
				Vector3 randomPoint = Random.insideUnitSphere * AreaRadius.Value;
				randomPoint += AreaCenter.Value;

				if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 5f, NavMesh.AllAreas))
				{
					Destination.Value = hit.position;
					return TaskStatus.Success;
				}
			}

			return TaskStatus.Failure;
		}
	}
}