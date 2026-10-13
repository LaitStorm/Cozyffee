using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
	public class LookAtPlayer : NPCAction
	{
		public bool KeepLooking = false;
		public bool YawOnly = true;
		bool hasWarned;

		protected override TaskStatus OnUpdate()
		{
			Transform head = PlayerLocator.Head;

			if (head == null)
			{
				if (!hasWarned)
				{
					Debug.LogWarning($"{nameof(LookAtPlayer)}: couldn't find the player (no XROrigin or MainCamera in the scene).");
					hasWarned = true;
				}

				return TaskStatus.Failure;
			}

			Vector3 targetPosition = head.position;

			if (YawOnly)
			{
				targetPosition.y = npc.SensorPosition.y;
			}

			npc.LookAt?.Invoke(targetPosition);

			if (KeepLooking)
			{
				return TaskStatus.Running;
			}

			if (npc.IsLookingAt?.Invoke(targetPosition) ?? false)
			{
				return TaskStatus.Success;
			}

			return TaskStatus.Running;
		}
	}
}