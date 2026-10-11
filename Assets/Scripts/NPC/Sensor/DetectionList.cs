using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Game.Cozyffee
{
    [System.Serializable]
    public class DetectionList
    {
        [SerializeField]
        List<DetectionEntry> list = new List<DetectionEntry>();

        public IReadOnlyList<DetectionEntry> List => list.AsReadOnly();

        public int Count => list.Count;

        #region Useful Counts
        public int VisibleHostileCount
        {
            get
            {
                int count = 0;
                foreach (var entry in list)
                {
                    if (entry.Hostile && entry.Visible)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public int OutOfSightHostileCount
        {
            get
            {
                int count = 0;
                foreach (var entry in list)
                {
                    if (entry.Hostile && !entry.Visible)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public int VisibleNPCsCount
        {
            get
            {
                int count = 0;
                foreach (var entry in list)
                {
                    if (entry.Visible)
                    {
                        count++;
                    }
                }

                return count;
            }
        }
        #endregion

        public bool IsVisible(NPC npc)
        {
            if (Contains(npc, out DetectionEntry entry) == false)
            {
                return false;
            }

            return entry.Visible;
        }

        public DetectionEntry GetEntryAtIndex(int index)
        {
            return list[index];
        }

        public DetectionEntry GetEntry(NPC npc)
        {
            foreach (var entry in list)
            {
                if (npc == entry.NPC) return entry;
            }

            return null;
        }

        public void Sort()
        {
            list.Sort((a, b) => a.Distance.CompareTo(b.Distance));
        }

        public void Clear()
        {
            list.Clear();
        }

        public void Add(DetectionEntry entry)
        {
            list.Add(entry);
        }

        public void Remove(DetectionEntry entry)
        {
            if (entry == null)
            {
                Debug.LogError("Trying to remove a null entry!");
                return;
            }

            list.Remove(entry);
        }

        public void Remove(NPC npc)
        {
            DetectionEntry entry = GetEntry(npc);
            Remove(entry);
        }

        public bool Contains(NPC npc)
        {
            return Contains(npc, out _);
        }

        public bool Contains (NPC npc, out DetectionEntry outputEntry)
        {
            foreach (DetectionEntry entry in list)
            {
                if (npc == entry.NPC)
                {
                    outputEntry = entry;
                    return true;
                }
            }
            outputEntry = null;
            return false;
        }

        public void RemoveDeadEntries()
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                DetectionEntry entry = list[i];

                if (entry.Alive == false)
                {
                    list.RemoveAt(i);
                }
            }
        }

        public void RemoveOldEntries(float maxTime = 120f)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                DetectionEntry entry = list[i];
                float timeSinceVisible = Time.time - entry.LastSeenTime;

                if (timeSinceVisible > maxTime)
                {
                    list.RemoveAt(i);
                }
            }
        }
    }
}