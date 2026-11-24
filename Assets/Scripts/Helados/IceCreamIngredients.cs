namespace Game.Cozyffee
{
    public enum IceCreamFlavor
    {
        Vanilla,
        Mint,
        Chocolate,
        Pineapple,
        None
    }

    // Not "IceCreamTopping": that name is taken by the topping pickup MonoBehaviour.
    public enum IceCreamToppingType
    {
        Strawberry,
        Blueberry,
        Kiwi,
        Cherry,
        Raspberry,
        None
    }

#if UNITY_EDITOR
    /// <summary>
    /// Converts the old free-text flavor/topping names (e.g. "Vainilla", "Cherry") saved in scenes into the enums.
    /// Only used to migrate old data; safe to delete once every scene has been re-saved.
    /// </summary>
    internal static class IceCreamLegacyNames
    {
        public static bool TryParseFlavor(string legacyName, out IceCreamFlavor flavor)
        {
            switch (Normalize(legacyName))
            {
                case "vanilla":
                case "vainilla":
                    flavor = IceCreamFlavor.Vanilla;
                    return true;
                case "mint":
                case "menta":
                    flavor = IceCreamFlavor.Mint;
                    return true;
                case "chocolate":
                    flavor = IceCreamFlavor.Chocolate;
                    return true;
                case "pineapple":
                case "piña":
                case "pina":
                    flavor = IceCreamFlavor.Pineapple;
                    return true;
                default:
                    flavor = IceCreamFlavor.None;
                    return false;
            }
        }

        public static bool TryParseTopping(string legacyName, out IceCreamToppingType topping)
        {
            switch (Normalize(legacyName))
            {
                case "strawberry":
                case "frutilla":
                case "fresa":
                    topping = IceCreamToppingType.Strawberry;
                    return true;
                case "blueberry":
                case "arandano":
                case "arándano":
                    topping = IceCreamToppingType.Blueberry;
                    return true;
                case "kiwi":
                    topping = IceCreamToppingType.Kiwi;
                    return true;
                case "cherry":
                case "cereza":
                    topping = IceCreamToppingType.Cherry;
                    return true;
                case "raspberry":
                case "frambuesa":
                    topping = IceCreamToppingType.Raspberry;
                    return true;
                default:
                    topping = IceCreamToppingType.None;
                    return false;
            }
        }

        static string Normalize(string name)
        {
            return (name ?? "").Trim().ToLowerInvariant();
        }
    }
#endif
}
