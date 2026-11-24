using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee
{
    /// <summary>
    /// What an NPC asked for this time. Created by <see cref="CafeMenuItem.CreateOrder"/> when the NPC orders:
    /// most items always give the same order, but some (like <see cref="IceCreamMenuItem"/>) roll parts of it each time.
    /// </summary>
    public class CafeOrder
    {
        public CafeOrder(CafeMenuItem menuItem, string id, string displayName, Sprite icon, float hungerFill,
                         IReadOnlyList<Sprite> overlays = null)
        {
            MenuItem = menuItem;
            Id = id;
            DisplayName = displayName;
            Icon = icon;
            HungerFill = hungerFill;
            Overlays = overlays ?? System.Array.Empty<Sprite>();
        }

        /// <summary>The menu entry this order came from.</summary>
        public CafeMenuItem MenuItem { get; }

        /// <summary>The item ID the NPC will accept.</summary>
        public string Id { get; }

        public string DisplayName { get; }

        /// <summary>Shown in the NPC's speech bubble while it waits.</summary>
        public Sprite Icon { get; }

        /// <summary>Drawn over <see cref="Icon"/> in this order, e.g. an ice cream's toppings. Usually empty.</summary>
        public IReadOnlyList<Sprite> Overlays { get; }

        public float HungerFill { get; }
    }
}
