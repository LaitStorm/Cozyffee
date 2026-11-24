using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee
{
    /// <summary>
    /// An ice cream on the menu: one or two fixed scoops, plus 0-2 toppings rolled every time an NPC orders it.
    /// Create one per scoop bubble sprite: Assets > Create > Cozyffee > Menu > Ice Cream Item.
    ///
    /// NPCs don't care about scoop or topping order, so "Vanilla + Chocolate" and "Chocolate + Vanilla" accept the same cone;
    /// they're separate entries only so both bubble sprites get used. The order's ID comes from <see cref="IceCreamRecipe"/>,
    /// so the ID field above is only used to tell menu entries apart.
    /// </summary>
    [CreateAssetMenu(menuName = "Cozyffee/Menu/Ice Cream Item", fileName = "New Ice Cream Item")]
    public class IceCreamMenuItem : CafeMenuItem
    {
        [Header("Scoops")]
        [Tooltip("Bottom scoop.")]
        public IceCreamFlavor FirstScoop = IceCreamFlavor.None;

        [Tooltip("Top scoop. None for a single-scoop ice cream.")]
        public IceCreamFlavor SecondScoop = IceCreamFlavor.None;

        [Header("Toppings (rolled for each order)")]
        [SerializeField, Min(0f), Tooltip("Chance of no toppings, relative to the two below.")]
        float noToppingsWeight = 0.5f;

        [SerializeField, Min(0f), Tooltip("Chance of one topping, relative to the others.")]
        float oneToppingWeight = 1f;

        [SerializeField, Min(0f), Tooltip("Chance of two toppings (can be the same one twice), relative to the others.")]
        float twoToppingsWeight = 1f;

        [SerializeField, Tooltip("Toppings this ice cream can come with, each equally likely. List one twice to make it twice as likely.")]
        List<IceCreamToppingType> toppings = new List<IceCreamToppingType>
        {
            IceCreamToppingType.Strawberry,
            IceCreamToppingType.Blueberry,
            IceCreamToppingType.Kiwi,
            IceCreamToppingType.Cherry,
            IceCreamToppingType.Raspberry,
        };

        [SerializeField, Tooltip("Overlays drawn on the bubble for each topping. \"Find Sprites\" in this asset's menu fills it in.")]
        IceCreamToppingSprites toppingSprites;

        bool warnedMissingSprites;

        /// <summary>The scoops that are set, bottom first.</summary>
        public List<IceCreamFlavor> Scoops
        {
            get
            {
                var scoops = new List<IceCreamFlavor>(2);
                if (FirstScoop != IceCreamFlavor.None) scoops.Add(FirstScoop);
                if (SecondScoop != IceCreamFlavor.None) scoops.Add(SecondScoop);
                return scoops;
            }
        }

        public override CafeOrder CreateOrder()
        {
            List<IceCreamFlavor> scoops = Scoops;
            List<IceCreamToppingType> rolledToppings = RollToppings();

            return new IceCreamOrder(this, IceCreamRecipe.BuildId(scoops, rolledToppings), OrderDisplayName(rolledToppings),
                                     Icon, HungerFill, ToppingOverlays(scoops.Count, rolledToppings), scoops, rolledToppings);
        }

        internal override bool IsOrderable(HashSet<CafeMenu> visiting)
        {
            return base.IsOrderable(visiting) && Scoops.Count > 0;
        }

        List<IceCreamToppingType> RollToppings()
        {
            var choices = new List<IceCreamToppingType>();

            foreach (IceCreamToppingType topping in toppings)
            {
                if (topping != IceCreamToppingType.None) choices.Add(topping);
            }

            var rolled = new List<IceCreamToppingType>(2);

            if (choices.Count == 0)
            {
                return rolled;
            }

            int count = RollToppingCount();

            for (int i = 0; i < count; i++)
            {
                rolled.Add(choices[Random.Range(0, choices.Count)]);
            }

            return rolled;
        }

        int RollToppingCount()
        {
            float[] weights = { noToppingsWeight, oneToppingWeight, twoToppingsWeight };
            float total = 0f;

            foreach (float weight in weights) total += Mathf.Max(0f, weight);

            if (total <= 0f)
            {
                return 0;
            }

            float roll = Random.Range(0f, total);
            int lastValid = 0;

            for (int count = 0; count < weights.Length; count++)
            {
                if (weights[count] <= 0f) continue;

                lastValid = count;
                roll -= weights[count];

                if (roll < 0f) return count;
            }

            // Random.Range can return exactly 'total'; fall back to the last option that can happen.
            return lastValid;
        }

        List<Sprite> ToppingOverlays(int scoopCount, List<IceCreamToppingType> rolledToppings)
        {
            var overlays = new List<Sprite>(rolledToppings.Count);

            for (int slot = 0; slot < rolledToppings.Count; slot++)
            {
                Sprite overlay = toppingSprites != null ? toppingSprites.Get(rolledToppings[slot], scoopCount > 1, slot) : null;

                if (overlay != null)
                {
                    overlays.Add(overlay);
                }
                else if (!warnedMissingSprites)
                {
                    string reason = toppingSprites == null ? "no Topping Sprites assigned" : $"'{toppingSprites.name}' has no sprite for {rolledToppings[slot]}";
                    Debug.LogWarning($"Ice Cream Item '{name}': {reason}, so its toppings won't show on the bubble.", this);
                    warnedMissingSprites = true;
                }
            }

            return overlays;
        }

        string OrderDisplayName(List<IceCreamToppingType> rolledToppings)
        {
            return rolledToppings.Count == 0 ? DisplayName : $"{DisplayName} with {string.Join(" and ", rolledToppings)}";
        }

        #region Editor
#if UNITY_EDITOR

        void OnValidate()
        {
            if (Scoops.Count == 0)
            {
                Debug.LogWarning($"Ice Cream Item '{name}' has no scoops set, so it will never be ordered.", this);
            }
        }

        /// <summary>
        /// Assigns "BubbleIceCream{First}{Second}" as the Icon (e.g. BubbleIceCreamVanillaChocolate),
        /// and the project's Ice Cream Topping Sprites asset if none is assigned yet.
        /// </summary>
        [ContextMenu("Find Sprites")]
        public void FindSprites()
        {
            var serialized = new UnityEditor.SerializedObject(this);
            string spriteName = "BubbleIceCream" + string.Concat(Scoops);
            Sprite icon = FindAsset<Sprite>(spriteName, "Sprite");

            if (icon != null)
            {
                serialized.FindProperty("icon").objectReferenceValue = icon;
            }
            else
            {
                Debug.LogWarning($"Ice Cream Item '{name}': no sprite named '{spriteName}' found.", this);
            }

            if (toppingSprites == null)
            {
                string[] tables = UnityEditor.AssetDatabase.FindAssets($"t:{nameof(IceCreamToppingSprites)}");

                if (tables.Length == 1)
                {
                    serialized.FindProperty(nameof(toppingSprites)).objectReferenceValue =
                        UnityEditor.AssetDatabase.LoadAssetAtPath<IceCreamToppingSprites>(UnityEditor.AssetDatabase.GUIDToAssetPath(tables[0]));
                }
                else
                {
                    Debug.LogWarning($"Ice Cream Item '{name}': found {tables.Length} Ice Cream Topping Sprites assets; assign one by hand.", this);
                }
            }

            serialized.ApplyModifiedProperties();
        }

        static T FindAsset<T>(string assetName, string typeFilter) where T : Object
        {
            foreach (string guid in UnityEditor.AssetDatabase.FindAssets($"{assetName} t:{typeFilter}"))
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);

                if (System.IO.Path.GetFileNameWithoutExtension(path) == assetName)
                {
                    return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
                }
            }

            return null;
        }

#endif
        #endregion
    }
}
