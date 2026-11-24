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

		[Tooltip("How many NPCs this generator keeps alive. NPCPopulation sets it from the settings menu.")]
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

		// Height above an NPC's feet where the spawn/despawn effect plays
		const float EffectHeight = 0.4f;

		readonly List<NPC> spawned = new List<NPC>();

		/// <summary>How many NPCs this generator keeps alive. Changing it doesn't spawn or remove anyone; see <see cref="ApplyCount"/>.</summary>
		public int TargetCount
		{
			get => Count;
			set => Count = Mathf.Max(0, value);
		}

		/// <summary>NPCs this generator spawned that still exist.</summary>
		public int AliveCount
		{
			get
			{
				spawned.RemoveAll(npc => npc == null);
				return spawned.Count;
			}
		}

		private void Start()
		{
			SpawnMissing(null);
		}

		/// <summary>
		/// Removes NPCs above <see cref="TargetCount"/> (idle ones first, those with an order last) and spawns new ones
		/// until there are <see cref="TargetCount"/>. <paramref name="effect"/> plays where each NPC appears or disappears.
		/// </summary>
		public void ApplyCount(GameObject effect)
		{
			RemoveExtra(effect);
			SpawnMissing(effect);
		}

		void SpawnMissing(GameObject effect)
		{
			int missing = Count - AliveCount;

			if (missing <= 0)
			{
				return;
			}

			// Keep new NPCs away from everyone already in the scene, not just this generator's NPCs
			List<Vector3> occupied = new List<Vector3>();
			foreach (NPC npc in FindObjectsByType<NPC>())
			{
				occupied.Add(npc.Position);
			}

			for (int i = 0; i < missing; i++)
			{
				NPC npc = Spawn(occupied);

				if (npc == null)
				{
					// No free spot or no prefab: trying again right away would fail the same way
					break;
				}

				OneShotEffect.Play(effect, npc.Position + Vector3.up * EffectHeight);
			}
		}

		void RemoveExtra(GameObject effect)
		{
			int extra = AliveCount - Count;

			if (extra <= 0)
			{
				return;
			}

			// Random order, then NPCs without an order first: someone waiting at the counter only leaves if there's no one else
			List<NPC> candidates = new List<NPC>(spawned);

			for (int i = candidates.Count - 1; i > 0; i--)
			{
				int j = Random.Range(0, i + 1);
				(candidates[i], candidates[j]) = (candidates[j], candidates[i]);
			}

			candidates.Sort((a, b) => IsOrdering(a).CompareTo(IsOrdering(b)));

			for (int i = 0; i < extra; i++)
			{
				Despawn(candidates[i], effect);
			}
		}

		static bool IsOrdering(NPC npc) => npc.CurrentOrder != null;

		void Despawn(NPC npc, GameObject effect)
		{
			spawned.Remove(npc);

			// Destroying the NPC doesn't end its order task, so free the order spot it reserved ourselves
			Area.ReleaseAll(npc);
			OneShotEffect.Play(effect, npc.Position + Vector3.up * EffectHeight);
			Destroy(npc.gameObject);
		}

		NPC Spawn(List<Vector3> occupied)
		{
			NPC prefab = GetRandomPrefab();

			if (prefab == null)
			{
				Debug.LogWarning($"NPCGenerator '{name}' has no valid NPC prefabs assigned.", this);
				return null;
			}

			if (!TryGetFreePosition(occupied, out Vector3 position))
			{
				Debug.LogWarning($"NPCGenerator '{name}' couldn't find a free spot to spawn an NPC; the area may be too crowded or off the NavMesh.", this);
				return null;
			}

			occupied.Add(position);
			Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

			NPC npc = Instantiate(prefab, position, rotation);

			RandomizeHunger(npc);
			spawned.Add(npc);

			return npc;
		}

		bool TryGetFreePosition(List<Vector3> occupied, out Vector3 position)
		{
			float minSqr = MinSpacing * MinSpacing;

			for (int attempt = 0; attempt < MaxPlacementAttempts; attempt++)
			{
				if (!Area.TryGetRandomPoint(out position))
				{
					continue;
				}

				bool free = true;
				foreach (Vector3 other in occupied)
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
