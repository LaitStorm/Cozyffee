using System.Collections.Generic;
using System.Text;

namespace Game.Cozyffee
{
    /// <summary>
    /// Builds the item ID of an ice cream from what's on it, so a cone can be matched against an order by ID
    /// like any other item. Scoop and topping order don't matter, and None entries are ignored.
    /// e.g. Chocolate + Vanilla scoops with Kiwi, Cherry -> "ice_cream_vanilla_chocolate_with_kiwi_cherry".
    /// </summary>
    public static class IceCreamRecipe
    {
        public const string IdPrefix = "ice_cream";

        /// <summary>Returns "" when there are no scoops: an empty cone isn't an ice cream anyone can order.</summary>
        public static string BuildId(IEnumerable<IceCreamFlavor> scoops, IEnumerable<IceCreamToppingType> toppings)
        {
            var sortedScoops = new List<IceCreamFlavor>();

            if (scoops != null)
            {
                foreach (IceCreamFlavor scoop in scoops)
                {
                    if (scoop != IceCreamFlavor.None) sortedScoops.Add(scoop);
                }
            }

            if (sortedScoops.Count == 0)
            {
                return "";
            }

            var sortedToppings = new List<IceCreamToppingType>();

            if (toppings != null)
            {
                foreach (IceCreamToppingType topping in toppings)
                {
                    if (topping != IceCreamToppingType.None) sortedToppings.Add(topping);
                }
            }

            sortedScoops.Sort();
            sortedToppings.Sort();

            var id = new StringBuilder(IdPrefix);

            foreach (IceCreamFlavor scoop in sortedScoops)
            {
                id.Append('_').Append(scoop.ToString().ToLowerInvariant());
            }

            if (sortedToppings.Count > 0)
            {
                id.Append("_with");

                foreach (IceCreamToppingType topping in sortedToppings)
                {
                    id.Append('_').Append(topping.ToString().ToLowerInvariant());
                }
            }

            return id.ToString();
        }
    }
}
