using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee
{
    /// <summary>
    /// An ice cream order: the scoops come from the <see cref="IceCreamMenuItem"/>, the toppings were rolled for this order.
    /// <see cref="CafeOrder.Icon"/> is the scoops bubble and <see cref="CafeOrder.Overlays"/> are the toppings drawn on top.
    /// </summary>
    public class IceCreamOrder : CafeOrder
    {
        public IceCreamOrder(IceCreamMenuItem menuItem, string id, string displayName, Sprite icon, float hungerFill,
                             IReadOnlyList<Sprite> overlays, IReadOnlyList<IceCreamFlavor> scoops,
                             IReadOnlyList<IceCreamToppingType> toppings)
            : base(menuItem, id, displayName, icon, hungerFill, overlays)
        {
            Scoops = scoops;
            Toppings = toppings;
        }

        /// <summary>In the order shown on the bubble (first = bottom).</summary>
        public IReadOnlyList<IceCreamFlavor> Scoops { get; }

        /// <summary>0 to 2 toppings. The first goes in the left slot of the bubble, the second in the right slot.</summary>
        public IReadOnlyList<IceCreamToppingType> Toppings { get; }
    }
}
