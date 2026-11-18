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

			Vector3 position = Area.GetRandomPoint();
			Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

			NPC npc = Instantiate(prefab, position, rotation);

			RandomizeHunger(npc);

			return npc;
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
			MinStartingHunger = Mathf.Max(MinStartingHunger, 0f);
			MaxStartingHunger = Mathf.Max(MaxStartingHunger, MinStartingHunger);
		}
	}
}
