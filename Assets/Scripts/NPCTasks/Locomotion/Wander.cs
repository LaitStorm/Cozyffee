using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using Game.Cozyffee.TaskSystem;

namespace Game.Cozyffee
{
	[System.Serializable]
	public class WanderParameters
	{
		public float SearchRadius = 10.0f;
		[Space(15)]
		public float MaxWanderTime = 3f;
		public float MaxWanderTimeRandom = 5f;
		[Space(15)]
		public float MaxWaitTime = 3f;
		public float MaxWaitTimeRandom = 5f;
	}

	public class Wander : NPCAction
	{
		private WanderParameters parameters;

		enum EState
		{
			Wandering,
			Waiting
		}

		EState state = EState.Wandering;
		private float waitTime = 0f;
		private float wanderTime = 0f;
		Vector3 wanderDestination;

		public Wander(WanderParameters parameters)
		{
			this.parameters = parameters;
		}

		protected override void OnEntry()
		{
			if (state == EState.Waiting)
			{
				ChangeState(EState.Wandering);
			}
			else
			{
				npc.SetDestination?.Invoke(wanderDestination);
				npc.CanMove = true;
			}
		}

		protected override TaskStatus OnUpdate()
		{
			if (state == EState.Waiting)
			{
				waitTime -= Time.deltaTime;

				if (waitTime < 0f)
				{
					ChangeState(EState.Wandering);
				}
			}
			else if (state == EState.Wandering)
			{
				wanderTime -= Time.deltaTime;

				if (HasArrived())
				{
					SetRandomDestination();
				}

				if (wanderTime < 0f)
				{
					ChangeState(EState.Waiting);
				}
			}

			return TaskStatus.Running;
		}

		void ChangeState(EState newState)
		{
			state = newState;

			if (state == EState.Wandering)
			{
				npc.CanMove = true;
				SetRandomDestination();
				wanderTime = parameters.MaxWanderTime + Random.Range(0f, parameters.MaxWanderTimeRandom);
			}
			else if (state == EState.Waiting)
			{
				waitTime = parameters.MaxWaitTime + Random.Range(0f, parameters.MaxWaitTimeRandom);
				npc.CanMove = false;
			}
		}

		bool HasArrived()
		{
			return Vector3.Distance(npc.Position, wanderDestination) <= 1f;
		}

		void SetRandomDestination()
		{
			for (int i = 0; i < 1000; i++)
			{
				Vector3 randomDirection = Random.insideUnitSphere * parameters.SearchRadius;
				randomDirection += npc.Position;

				if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, 5f, NavMesh.AllAreas))
				{
					wanderDestination = hit.position;
					npc.SetDestination?.Invoke(wanderDestination);
					return;
				}
			}
		}
	}
}