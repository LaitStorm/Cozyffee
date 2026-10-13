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

		static readonly List<Area> registry = new();
 
		private void OnEnable() => registry.Add(this);
		private void OnDisable() => registry.Remove(this);
		static void ResetStatics() => registry.Clear();

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
