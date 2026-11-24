using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee
{
    /// <summary>
    /// A list of things NPCs can order. Each entry is either a <see cref="CafeMenuItem"/> or another <see cref="CafeMenu"/>,
    /// so you can build a Donuts menu and a Macarons menu, then add both to a Main menu.
    /// Create one: Assets > Create > Cozyffee > Menu > Cafe Menu.
    ///
    /// Picking an order rolls an entry by weight. If it lands on a sub-menu, that menu rolls one of its own entries, and so on.
    /// So adding a 9th cupcake to the Cupcakes menu makes each cupcake rarer, but doesn't make cupcakes as a whole more common.
    /// Entries that can't be ordered (unavailable, weight 0, or a menu with nothing orderable) are skipped,
    /// and the remaining weights share their chance. The Chance fields show the result as a percentage.
    /// </summary>
    [CreateAssetMenu(menuName = "Cozyffee/Menu/Cafe Menu", fileName = "Cafe Menu")]
    public class CafeMenu : CafeMenuOption
    {
        [Serializable]
        public class Entry
        {
            [Tooltip("A Menu Item, or another Cafe Menu.")]
            public CafeMenuOption Option;

            [Min(0f), Tooltip("Chance relative to the other entries in this menu. 2 is twice as likely as 1; 0 is never.")]
            public float Weight = 1f;

            [ReadOnly, Tooltip("Chance of this entry when ordering from this menu (calculated).")]
            public string Chance = "";

            [SerializeField, HideInInspector]
            internal bool initialized = true;

            internal bool IsOrderable(HashSet<CafeMenu> visiting)
            {
                return Option != null && Weight > 0f && Option.IsOrderable(visiting);
            }
        }

        [SerializeField]
        List<Entry> entries = new List<Entry>();

        public IReadOnlyList<Entry> Entries => entries;

        /// <summary>Every item reachable from this menu, including through sub-menus (and including unavailable ones).</summary>
        public IEnumerable<CafeMenuItem> AllItems
        {
            get
            {
                var items = new List<CafeMenuItem>();
                CollectItems(items, new HashSet<CafeMenu>());
                return items;
            }
        }

        /// <summary>
        /// Picks an entry by weight; if it's a sub-menu, picks inside that, until it reaches an item.
        /// Returns false if nothing on the menu can be ordered.
        /// </summary>
        public bool TryPickRandom(out CafeMenuItem picked)
        {
            picked = PickItem(new HashSet<CafeMenu>());
            return picked != null;
        }

        public bool TryGet(string id, out CafeMenuItem result)
        {
            foreach (CafeMenuItem item in AllItems)
            {
                if (item.Id == id)
                {
                    result = item;
                    return true;
                }
            }

            result = null;
            return false;
        }

        internal override bool IsOrderable(HashSet<CafeMenu> visiting)
        {
            if (!Available || !visiting.Add(this))
            {
                return false;
            }

            try
            {
                foreach (Entry entry in entries)
                {
                    if (entry != null && entry.IsOrderable(visiting))
                    {
                        return true;
                    }
                }

                return false;
            }
            finally
            {
                visiting.Remove(this);
            }
        }

        internal override CafeMenuItem PickItem(HashSet<CafeMenu> visiting)
        {
            if (!Available || !visiting.Add(this))
            {
                return null;
            }

            try
            {
                float total = 0f;

                foreach (Entry entry in entries)
                {
                    if (entry != null && entry.IsOrderable(visiting)) total += entry.Weight;
                }

                if (total <= 0f)
                {
                    return null;
                }

                float roll = UnityEngine.Random.Range(0f, total);
                Entry chosen = null;

                foreach (Entry entry in entries)
                {
                    if (entry == null || !entry.IsOrderable(visiting)) continue;

                    chosen = entry;
                    roll -= entry.Weight;

                    if (roll < 0f) break;
                }

                // If floating point rounding leaves roll at exactly 0, chosen is the last valid entry.
                return chosen?.Option.PickItem(visiting);
            }
            finally
            {
                visiting.Remove(this);
            }
        }

        void CollectItems(List<CafeMenuItem> items, HashSet<CafeMenu> visited)
        {
            if (!visited.Add(this))
            {
                return;
            }

            foreach (Entry entry in entries)
            {
                if (entry?.Option is CafeMenuItem item && !items.Contains(item))
                {
                    items.Add(item);
                }
                else if (entry?.Option is CafeMenu subMenu)
                {
                    subMenu.CollectItems(items, visited);
                }
            }
        }

        bool Contains(CafeMenu target, HashSet<CafeMenu> visited)
        {
            if (!visited.Add(this))
            {
                return false;
            }

            foreach (Entry entry in entries)
            {
                if (entry?.Option is CafeMenu subMenu && (subMenu == target || subMenu.Contains(target, visited)))
                {
                    return true;
                }
            }

            return false;
        }

        #region Editor

        private void OnValidate()
        {
            ApplyDefaultsToNewEntries();
            WarnAboutLoops();
            UpdateChances();
            WarnAboutDuplicates();
        }

        // Unity creates the first element of an empty list with every field zeroed (Weight 0 = never ordered),
        // ignoring the defaults above. Give new entries sensible defaults instead.
        void ApplyDefaultsToNewEntries()
        {
            foreach (Entry entry in entries)
            {
                if (entry != null && !entry.initialized)
                {
                    entry.Weight = 1f;
                    entry.initialized = true;
                }
            }
        }

        void WarnAboutLoops()
        {
            if (Contains(this, new HashSet<CafeMenu>()))
            {
                Debug.LogWarning($"Cafe Menu '{name}' contains itself (directly or through a sub-menu). That branch will never be picked.", this);
            }
        }

        [ContextMenu("Refresh Chances")]
        void UpdateChances()
        {
            var visiting = new HashSet<CafeMenu> { this };
            float total = 0f;

            foreach (Entry entry in entries)
            {
                if (entry != null && entry.IsOrderable(visiting)) total += entry.Weight;
            }

            foreach (Entry entry in entries)
            {
                if (entry == null) continue;

                entry.Chance = entry.IsOrderable(visiting) && Available
                    ? Percent(entry.Weight / total)
                    : ReasonSkipped(entry);
            }
        }

        [ContextMenu("Log Order Chances")]
        void LogChances()
        {
            UpdateChances();

            var log = new System.Text.StringBuilder($"Overall order chances for '{name}':\n");
            AppendChances(log, 1f, 0, new HashSet<CafeMenu>());
            Debug.Log(log.ToString(), this);
        }

        void AppendChances(System.Text.StringBuilder log, float menuChance, int depth, HashSet<CafeMenu> visiting)
        {
            visiting.Add(this);

            float total = 0f;

            foreach (Entry entry in entries)
            {
                if (entry != null && entry.IsOrderable(visiting)) total += entry.Weight;
            }

            string indent = new string(' ', depth * 4);

            foreach (Entry entry in entries)
            {
                if (entry == null) continue;

                string label = entry.Option != null ? entry.Option.DisplayName : "(empty slot)";
                bool orderable = entry.IsOrderable(visiting) && total > 0f;
                float chance = orderable ? menuChance * entry.Weight / total : 0f;

                log.AppendLine($"{indent}{label}: {(orderable ? Percent(chance) : ReasonSkipped(entry))}");

                if (orderable && entry.Option is CafeMenu subMenu)
                {
                    subMenu.AppendChances(log, chance, depth + 1, visiting);
                }
            }

            visiting.Remove(this);
        }

        void WarnAboutDuplicates()
        {
            var seen = new HashSet<string>();
            var reported = new HashSet<string>();
            var visited = new HashSet<CafeMenu>();
            var items = new List<CafeMenuItem>();

            CollectAllIncludingDuplicates(items, visited);

            foreach (CafeMenuItem item in items)
            {
                if (!string.IsNullOrEmpty(item.Id) && !seen.Add(item.Id) && reported.Add(item.Id))
                {
                    Debug.LogWarning($"Cafe Menu '{name}' reaches more than one entry with ID '{item.Id}'. " +
                                     "That's allowed, but it makes that item more likely than its weight suggests.", this);
                }
            }
        }

        void CollectAllIncludingDuplicates(List<CafeMenuItem> items, HashSet<CafeMenu> visited)
        {
            if (!visited.Add(this)) return;

            foreach (Entry entry in entries)
            {
                if (entry?.Option is CafeMenuItem item) items.Add(item);
                else if (entry?.Option is CafeMenu subMenu) subMenu.CollectAllIncludingDuplicates(items, visited);
            }
        }

        string ReasonSkipped(Entry entry)
        {
            if (!Available) return "never (this menu unavailable)";
            if (entry.Option == null) return "never (empty slot)";
            if (entry.Option == this) return "never (menu contains itself)";
            if (!entry.Option.Available) return "never (unavailable)";
            if (entry.Weight <= 0f) return "never (weight 0)";
            if (entry.Option is CafeMenu) return "never (nothing orderable inside)";
            if (entry.Option is IceCreamMenuItem iceCream && iceCream.Scoops.Count == 0) return "never (no scoops)";
            return "never (no ID)";
        }

        static string Percent(float chance)
        {
            return $"{chance * 100f:0.#}%";
        }

        #endregion
    }
}
