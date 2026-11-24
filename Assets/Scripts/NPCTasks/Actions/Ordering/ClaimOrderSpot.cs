using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    /// <summary>
    /// Reserves the closest free Area with <see cref="AreaId"/> and writes a point inside it to <see cref="Destination"/>.
    /// Stays Running while every matching Area is full, so the NPC waits its turn instead of crowding the counter.
    /// The reservation is released by <see cref="OrderScope"/> when the order ends.
    /// Fails if no Area with that ID exists, or the chosen Area has no reachable NavMesh point.
    /// </summary>
    public class ClaimOrderSpot : NPCAction
    {
        public SharedString AreaId = new SharedString();
        public SharedVector3 Destination = new SharedVector3();
        public int MaxAttempts = 30;

        bool hasWarned;

        protected override TaskStatus OnUpdate()
        {
            string id = AreaId?.Value ?? "";

            if (!Area.Exists(id))
            {
                Warn($"no enabled Area with ID '{id}' found.");
                return TaskStatus.Failure;
            }

            Area area = Area.FindFree(id, npc.Position, npc);

            if (area == null)
            {
                // Every spot is taken: keep waiting.
                return TaskStatus.Running;
            }

            if (!area.TryGetRandomPoint(out Vector3 point, MaxAttempts))
            {
                Warn($"Area '{area.name}' has no NavMesh point within its radius. Move it over the NavMesh or raise its Sample Distance.");
                return TaskStatus.Failure;
            }

            area.TryReserve(npc);
            Destination.Value = point;
            return TaskStatus.Success;
        }

        void Warn(string message)
        {
            if (!hasWarned)
            {
                Debug.LogWarning($"{nameof(ClaimOrderSpot)} on '{npc.name}': {message}");
                hasWarned = true;
            }
        }
    }
}
