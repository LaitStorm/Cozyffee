using Game.Cozyffee.TaskSystem;
using UnityEngine;

namespace Game.Cozyffee
{
	/// <summary>
	/// Keeps the NPC on <see cref="Spot"/> while it waits (e.g. at the order counter).
	/// If something pushes it further than <see cref="ReturnDistance"/>, it walks back until it's within
	/// <see cref="ArriveDistance"/>. The gap between the two avoids jittering at the edge.
	/// While running, the NPC is also more important for crowd avoidance, so other NPCs steer around it.
	///
	/// Never finishes on its own (always Running): put it in a ParallelSelector next to the task
	/// that decides when the wait is over, e.g. ReceiveItem.
	/// </summary>
	public class HoldPosition : NPCAction
	{
		public SharedVector3 Spot;

		[Tooltip("Pushed further than this from Spot (meters, ignoring height) and the NPC walks back.")]
		public SharedFloat ReturnDistance = 1.5f;

		[Tooltip("Walking back stops within this distance. Keep it above the NavMeshAgent's Stopping Distance.")]
		public SharedFloat ArriveDistance = 1f;

		bool returning;

		public HoldPosition()
		{
		}

		public HoldPosition(SharedVector3 spot)
		{
			Spot = spot;
		}

		protected override void OnEntry()
		{
			returning = false;
			npc.SetHoldingPosition?.Invoke(true);
		}

		protected override void OnExit()
		{
			if (returning)
			{
				npc.CanMove = false;
				returning = false;
			}

			npc.SetHoldingPosition?.Invoke(false);
		}

		protected override TaskStatus OnUpdate()
		{
			if (Spot == null)
			{
				return TaskStatus.Running;
			}

			float distance = FlatDistance(npc.Position, Spot.Value);

			if (!returning && distance > ReturnDistance.Value)
			{
				returning = true;
				npc.CanMove = true;
				npc.SetDestination?.Invoke(Spot.Value);
			}
			else if (returning && distance <= ArriveDistance.Value)
			{
				returning = false;
				npc.CanMove = false;
			}

			return TaskStatus.Running;
		}

		static float FlatDistance(Vector3 a, Vector3 b)
		{
			a.y = 0f;
			b.y = 0f;
			return Vector3.Distance(a, b);
		}
	}
}
