using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Cozyffee
{
	public class Area : MonoBehaviour
	{
		[Tooltip("Behavior trees look this area up by ID. Several areas can share an ID; the closest one to the NPC is used.")]
		public string Id = "";
		public float Radius = 20f;

		[Tooltip("How far (in any direction) to search for the nearest NavMesh point from each random candidate.")]
		public float SampleDistance = 2f;

		[Tooltip("How many NPCs can reserve this area at once (e.g. one customer per order spot). 0 = unlimited. " +
		         "Only NPCs that reserve it count; NPCs just walking through don't.")]
		[Min(0)]
		public int Capacity = 1;

		static readonly List<Area> registry = new();

		readonly List<NPC> occupants = new();

		private void OnEnable() => registry.Add(this);
		private void OnDisable()
		{
			registry.Remove(this);
			occupants.Clear();
		}

		static void ResetStatics() => registry.Clear();

		#region Reservations

		/// <summary>Every enabled Area (for debugging tools).</summary>
		public static IReadOnlyList<Area> All => registry;

		/// <summary>NPCs currently holding a reservation here.</summary>
		public IReadOnlyList<NPC> Occupants => occupants;

		public int OccupantCount
		{
			get
			{
				// NPCs destroyed while holding a spot shouldn't block it forever.
				occupants.RemoveAll(o => o == null);
				return occupants.Count;
			}
		}

		public bool IsReservedBy(NPC npc) => npc != null && occupants.Contains(npc);

		public bool HasRoomFor(NPC npc) => IsReservedBy(npc) || Capacity <= 0 || OccupantCount < Capacity;

		public bool TryReserve(NPC npc)
		{
			if (npc == null || !HasRoomFor(npc))
			{
				return false;
			}

			if (!occupants.Contains(npc))
			{
				occupants.Add(npc);
			}

			return true;
		}

		public void Release(NPC npc) => occupants.Remove(npc);

		/// <summary>Releases every area <paramref name="npc"/> has reserved.</summary>
		public static void ReleaseAll(NPC npc)
		{
			foreach (Area area in registry)
			{
				area.Release(npc);
			}
		}

		/// <summary>
		/// The closest enabled Area with this ID that still has room for <paramref name="npc"/>, or null if they're all full.
		/// </summary>
		public static Area FindFree(string id, Vector3 nearTo, NPC npc)
		{
			if (string.IsNullOrEmpty(id))
			{
				return null;
			}

			Area best = null;
			float bestSqrDistance = float.MaxValue;

			foreach (Area candidate in registry)
			{
				if (candidate.Id != id || !candidate.HasRoomFor(npc))
					continue;

				float sqrDistance = (candidate.transform.position - nearTo).sqrMagnitude;
				if (sqrDistance < bestSqrDistance)
				{
					best = candidate;
					bestSqrDistance = sqrDistance;
				}
			}

			return best;
		}

		public static bool Exists(string id)
		{
			foreach (Area candidate in registry)
			{
				if (candidate.Id == id) return true;
			}

			return false;
		}

		#endregion

		public static Area Find(string id, Vector3 nearTo)
		{
			if (string.IsNullOrEmpty(id))
			{
				return null;
			}

			Area best = null;
			float bestSqrDistance = float.MaxValue;

			foreach (Area candidate in registry)
			{
				if (candidate.Id != id)
					continue;

				float sqrDistance = (candidate.transform.position - nearTo).sqrMagnitude;
				if (sqrDistance < bestSqrDistance)
				{
					best = candidate;
					bestSqrDistance = sqrDistance;
				}
			}

			return best;
		}

		/// <summary>
		/// True if any enabled Area with the given ID contains <paramref name="point"/> (on the XZ plane).
		/// </summary>
		public static bool IsInsideAny(string id, Vector3 point)
		{
			if (string.IsNullOrEmpty(id))
			{
				return false;
			}

			foreach (Area candidate in registry)
			{
				if (candidate.Id == id && candidate.Contains(point))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// True if <paramref name="point"/> is within <see cref="Radius"/> of this area's center, ignoring height.
		/// </summary>
		public bool Contains(Vector3 point)
		{
			Vector3 flat = point - transform.position;
			flat.y = 0f;

			return flat.sqrMagnitude <= Radius * Radius;
		}

		public bool TryGetRandomPoint(out Vector3 point, int maxAttempts = 30)
		{
			Vector3 center = transform.position;

			for (int i = 0; i < maxAttempts; i++)
			{
				Vector2 offset = Random.insideUnitCircle * Radius;
				Vector3 candidate = center + new Vector3(offset.x, 0f, offset.y);

				if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, SampleDistance, NavMesh.AllAreas))
				{
					continue;
				}

				Vector3 flat = hit.position - center;
				flat.y = 0f;

				if (flat.sqrMagnitude <= Radius * Radius)
				{
					point = hit.position;
					return true;
				}
			}

			point = center;
			return false;
		}

		public Vector3 GetRandomPoint()
		{
			TryGetRandomPoint(out Vector3 point);
			return point;
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = new Color(0f, 1f, 0.5f, 0.6f);
			Gizmos.DrawWireSphere(transform.position, Radius);
		}
	}
}
