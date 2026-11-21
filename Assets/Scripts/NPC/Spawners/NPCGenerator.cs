using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.NavigationTutorial
{
	public class NPCGenerator : MonoBehaviour
	{
		[SerializeField]
		List<NPC> NPCPrefabs = new List<NPC>();

		[SerializeField]
		Area Area;

		[SerializeField]
		int Count = 15;

		[Header("Starting Hunger")]
		[Tooltip("Random starting hunger points are picked between these values (clamped to each NPC's MaxPoints).")]
		[SerializeField]
		float MinStartingHunger = 20f;

		[SerializeField]
		float MaxStartingHunger = 100f;

		[Header("Spacing")]
		[Tooltip("Minimum distance (on the XZ plane) between spawned NPCs. Roughly twice the NavMeshAgent radius works well.")]
		[SerializeField]
		float MinSpacing = 1f;

		[Tooltip("How many random points to try before giving up on a spawn.")]
		[SerializeField]
		int MaxPlacementAttempts = 30;

		readonly List<Vector3> NPCPositions = new List<Vector3>();

		private void Start()
		{
			for (int i = 0; i < Count; i++)
			{
				Spawn();
			}
		}

		public NPC Spawn()
		{
			NPC prefab = GetRandomPrefab();

			if (prefab == null)
			{
				Debug.LogWarning($"NPCGenerator '{name}' has no valid NPC prefabs assigned.", this);
				return null;
			}

			if (!TryGetFreePosition(out Vector3 position))
			{
				Debug.LogWarning($"NPCGenerator '{name}' couldn't find a free spot to spawn an NPC; the area may be too crowded or off the NavMesh.", this);
				return null;
			}

			NPCPositions.Add(position);
			Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

			NPC npc = Instantiate(prefab, position, rotation);

			RandomizeHunger(npc);

			return npc;
		}

		bool TryGetFreePosition(out Vector3 position)
		{
			float minSqr = MinSpacing * MinSpacing;

			for (int attempt = 0; attempt < MaxPlacementAttempts; attempt++)
			{
				if (!Area.TryGetRandomPoint(out position))
				{
					continue;
				}

				bool free = true;
				foreach (Vector3 other in NPCPositions)
				{
					Vector3 delta = position - other;
					delta.y = 0f;

					if (delta.sqrMagnitude < minSqr)
					{
						free = false;
						break;
					}
				}

				if (free)
				{
					return true;
				}
			}

			position = default;
			return false;
		}

		NPC GetRandomPrefab()
		{
			if (NPCPrefabs == null || NPCPrefabs.Count == 0)
			{
				return null;
			}

			// Pick among non-null entries so empty inspector slots are ignored
			int validCount = 0;
			foreach (NPC prefab in NPCPrefabs)
			{
				if (prefab != null) validCount++;
			}

			if (validCount == 0)
			{
				return null;
			}

			int pick = Random.Range(0, validCount);
			foreach (NPC prefab in NPCPrefabs)
			{
				if (prefab == null) continue;
				if (pick-- == 0) return prefab;
			}

			return null;
		}

		void RandomizeHunger(NPC npc)
		{
			// NPCHunger registers itself on the NPC during Awake, which runs inside Instantiate
			if (npc.Hunger == null)
			{
				return;
			}

			float min = Mathf.Clamp(Mathf.Min(MinStartingHunger, MaxStartingHunger), 0f, npc.Hunger.MaxPoints);
			float max = Mathf.Clamp(Mathf.Max(MinStartingHunger, MaxStartingHunger), 0f, npc.Hunger.MaxPoints);

			npc.Hunger.Points = Random.Range(min, max);
		}

		private void OnValidate()
		{
			Count = Mathf.Max(Count, 0);
			MinSpacing = Mathf.Max(MinSpacing, 0f);
			MaxPlacementAttempts = Mathf.Max(MaxPlacementAttempts, 1);
			MinStartingHunger = Mathf.Max(MinStartingHunger, 0f);
			MaxStartingHunger = Mathf.Max(MaxStartingHunger, MinStartingHunger);
		}
	}
}
