using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee
{
    /// <summary>
    /// One thing an NPC can order (a cinnamon roll, a macaron, a coffee...).
    /// Create one per item: Assets > Create > Cozyffee > Menu > Menu Item, then add it to a <see cref="CafeMenu"/>.
    /// Link the matching scene/prefab objects to it through their <see cref="Item"/> component.
    /// How often it gets ordered is set in the menu, not here.
    /// </summary>
    [CreateAssetMenu(menuName = "Cozyffee/Menu/Menu Item", fileName = "New Menu Item")]
    public class CafeMenuItem : CafeMenuOption
    {
        [Tooltip("Unique ID. Leave empty to use the asset name (\"CakeBlackForest\" -> \"cake_black_forest\"). " +
                "Items whose Item component uses this ID (or references this asset) count as this order.")]
        [SerializeField]
        string id = "";

        [Tooltip("Shown in the NPC's speech bubble while it waits for this order.")]
        [SerializeField]
        Sprite icon;

        [Tooltip("Hunger Points restored when the NPC eats this (clamped to the NPC's MaxPoints).")]
        [SerializeField, Min(0f)]
        float hungerFill = 60f;

        public string Id => string.IsNullOrWhiteSpace(id) ? IdFromName(name) : id.Trim();
        public Sprite Icon => icon;
        public float HungerFill => hungerFill;

        /// <summary>What an NPC asks for when it orders this. Override to vary parts of the order each time.</summary>
        public virtual CafeOrder CreateOrder()
        {
            return new CafeOrder(this, Id, DisplayName, Icon, HungerFill);
        }

        internal override bool IsOrderable(HashSet<CafeMenu> visiting)
        {
            return Available && !string.IsNullOrEmpty(Id);
        }

        internal override CafeMenuItem PickItem(HashSet<CafeMenu> visiting)
        {
            return IsOrderable(visiting) ? this : null;
        }

        /// <summary>"CinnamonRoll" / "Cinnamon Roll" -> "cinnamon_roll", matching the IDs already used in scenes.</summary>
        static string IdFromName(string assetName)
        {
            var builder = new System.Text.StringBuilder();
            string trimmed = assetName.Trim();

            for (int i = 0; i < trimmed.Length; i++)
            {
                char c = trimmed[i];

                if (c == ' ' || c == '-')
                {
                    if (builder.Length > 0 && builder[builder.Length - 1] != '_') builder.Append('_');
                    continue;
                }

                if (char.IsUpper(c) && i > 0 && char.IsLetterOrDigit(trimmed[i - 1]) && !char.IsUpper(trimmed[i - 1]))
                {
                    builder.Append('_');
                }

                builder.Append(char.ToLowerInvariant(c));
            }

            return builder.ToString();
        }
    }
}
