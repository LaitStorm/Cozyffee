using UnityEngine;

namespace Game.Cozyffee
{
    /// <summary>
    /// Makes an <see cref="IceCreamCone"/> something an NPC can be handed. Its ID comes from the scoops and toppings
    /// currently on the cone (see <see cref="IceCreamRecipe"/>), so the Definition and ID fields above are ignored.
    /// The cone adds this automatically at runtime if it's missing.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(IceCreamCone))]
    public class IceCreamItem : Item
    {
        IceCreamCone cone;

        public IceCreamCone Cone => cone != null ? cone : cone = GetComponent<IceCreamCone>();

        public override string Id => Cone != null ? Cone.RecipeId : "";
    }
}
