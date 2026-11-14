using UnityEngine;

using Game.Cozyffee.TaskSystem;

namespace Game.Cozyffee
{
	public class PickRandomDestinationInArea : NPCAction
	{
		public SharedString AreaId = new SharedString();
		public Area area;
		public SharedVector3 Destination = new SharedVector3();
		public int MaxAttempts = 30;

		bool hasWarned;

		protected override TaskStatus OnUpdate()
		{
			if (area == null)
			{
				area = Area.Find(AreaId.Value, npc.Position);
			}

			if (area == null)
			{
				if (!hasWarned)
				{
					Debug.LogWarning($"{nameof(PickRandomDestinationInArea)}: no enabled Area with ID '{AreaId.Value}' found.");
					hasWarned = true;
				}

				return TaskStatus.Failure;
			}

			if (area.TryGetRandomPoint(out Vector3 point, MaxAttempts))
			{
				Destination.Value = point;
				return TaskStatus.Success;
			}

			return TaskStatus.Failure;
		}
	}
}