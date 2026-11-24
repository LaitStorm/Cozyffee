using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Game.Cozyffee
{
    /// <summary>
    /// Cozyffee > Create Ice Cream Items: creates (or updates) one <see cref="IceCreamMenuItem"/> per scoop bubble sprite,
    /// i.e. every single scoop and every pair of scoops, named "{First}{Second}IceCream" (e.g. VanillaChocolateIceCream).
    /// Existing assets are updated in place, so menus that reference them keep working; topping weights aren't touched.
    /// Also creates (or updates) a Single Ice Creams and a Double Ice Creams menu with those items. Re-running only adds
    /// missing items to them, so weights you've changed stay.
    /// </summary>
    static class IceCreamMenuItemGenerator
    {
        const string Folder = "Assets/__Cozyffee/Menu/Items/IceCream";
        const string ToppingSpritesPath = Folder + "/Toppings/IceCreamToppingSprites.asset";
        const string MenuFolder = "Assets/__Cozyffee/Menu";
        const string SingleMenuPath = MenuFolder + "/SingleIceCreams.asset";
        const string DoubleMenuPath = MenuFolder + "/DoubleIceCreams.asset";
        const float HungerFill = 60f;

        [MenuItem("Cozyffee/Create Ice Cream Items")]
        static void CreateAll()
        {
            EnsureFolder(Folder);
            IceCreamToppingSprites toppingSprites = FindOrCreateToppingSprites();

            int created = 0;
            int updated = 0;
            var singles = new List<IceCreamMenuItem>();
            var doubles = new List<IceCreamMenuItem>();

            foreach (IceCreamFlavor first in Flavors())
            {
                singles.Add(CreateOrUpdate(first, IceCreamFlavor.None, toppingSprites, ref created, ref updated));

                foreach (IceCreamFlavor second in Flavors())
                {
                    doubles.Add(CreateOrUpdate(first, second, toppingSprites, ref created, ref updated));
                }
            }

            CafeMenu singleMenu = CreateOrUpdateMenu(SingleMenuPath, "Single Ice Creams", singles);
            CafeMenu doubleMenu = CreateOrUpdateMenu(DoubleMenuPath, "Double Ice Creams", doubles);

            AssetDatabase.SaveAssets();
            Debug.Log($"Ice cream items: {created} created, {updated} updated in {Folder}. " +
                      $"Menus: '{singleMenu.name}' ({singles.Count} items), '{doubleMenu.name}' ({doubles.Count} items).");
        }

        /// <summary>Adds any of <paramref name="items"/> the menu doesn't have yet, with weight 1. Leaves other entries alone.</summary>
        static CafeMenu CreateOrUpdateMenu(string path, string displayName, List<IceCreamMenuItem> items)
        {
            var menu = AssetDatabase.LoadAssetAtPath<CafeMenu>(path);

            if (menu == null)
            {
                menu = ScriptableObject.CreateInstance<CafeMenu>();
                AssetDatabase.CreateAsset(menu, path);
            }

            var serialized = new SerializedObject(menu);
            serialized.FindProperty("displayName").stringValue = displayName;

            SerializedProperty entries = serialized.FindProperty("entries");
            var existing = new HashSet<UnityEngine.Object>();

            for (int i = 0; i < entries.arraySize; i++)
            {
                existing.Add(entries.GetArrayElementAtIndex(i).FindPropertyRelative(nameof(CafeMenu.Entry.Option)).objectReferenceValue);
            }

            foreach (IceCreamMenuItem item in items)
            {
                if (existing.Contains(item)) continue;

                int index = entries.arraySize;
                entries.InsertArrayElementAtIndex(index);

                SerializedProperty entry = entries.GetArrayElementAtIndex(index);
                entry.FindPropertyRelative(nameof(CafeMenu.Entry.Option)).objectReferenceValue = item;
                entry.FindPropertyRelative(nameof(CafeMenu.Entry.Weight)).floatValue = 1f;
                entry.FindPropertyRelative("initialized").boolValue = true;
            }

            // The Chance column is filled in by the menu's OnValidate (or its "Refresh Chances" context menu).
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(menu);

            return menu;
        }

        static IceCreamMenuItem CreateOrUpdate(IceCreamFlavor first, IceCreamFlavor second, IceCreamToppingSprites toppingSprites,
                                   ref int created, ref int updated)
        {
            string assetName = $"{first}{(second == IceCreamFlavor.None ? "" : second.ToString())}IceCream";
            string path = $"{Folder}/{assetName}.asset";

            var item = AssetDatabase.LoadAssetAtPath<IceCreamMenuItem>(path);

            if (item == null)
            {
                item = ScriptableObject.CreateInstance<IceCreamMenuItem>();

                // Set before saving so its "no scoops" warning doesn't fire.
                item.FirstScoop = first;
                item.SecondScoop = second;

                AssetDatabase.CreateAsset(item, path);
                created++;
            }
            else
            {
                updated++;
            }

            var serialized = new SerializedObject(item);
            serialized.FindProperty(nameof(IceCreamMenuItem.FirstScoop)).intValue = (int)first;
            serialized.FindProperty(nameof(IceCreamMenuItem.SecondScoop)).intValue = (int)second;
            serialized.FindProperty("id").stringValue = ItemId(first, second);
            serialized.FindProperty("displayName").stringValue = DisplayName(first, second);
            serialized.FindProperty("hungerFill").floatValue = HungerFill;
            serialized.FindProperty("toppingSprites").objectReferenceValue = toppingSprites;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            // Assigns the BubbleIceCream{First}{Second} icon.
            item.FindSprites();
            EditorUtility.SetDirty(item);

            return item;
        }

        // Same pattern as the hand-made ones: vanillaicecream, doublevanillaicecream, vanillachocolateicecream.
        static string ItemId(IceCreamFlavor first, IceCreamFlavor second)
        {
            string id;

            if (second == IceCreamFlavor.None) id = $"{first}icecream";
            else if (second == first) id = $"double{first}icecream";
            else id = $"{first}{second}icecream";

            return id.ToLowerInvariant();
        }

        // "Helado de Vainilla", "Helado doble de Vainilla", "Helado de Vainilla y Chocolate".
        static string DisplayName(IceCreamFlavor first, IceCreamFlavor second)
        {
            if (second == IceCreamFlavor.None) return $"Helado de {SpanishName(first)}";
            if (second == first) return $"Helado doble de {SpanishName(first)}";
            return $"Helado de {SpanishName(first)} y {SpanishName(second)}";
        }

        static string SpanishName(IceCreamFlavor flavor)
        {
            switch (flavor)
            {
                case IceCreamFlavor.Vanilla: return "Vainilla";
                case IceCreamFlavor.Mint: return "Menta";
                case IceCreamFlavor.Chocolate: return "Chocolate";
                case IceCreamFlavor.Pineapple: return "Piña";
                default: return flavor.ToString();
            }
        }

        static IceCreamFlavor[] Flavors()
        {
            return Array.FindAll((IceCreamFlavor[])Enum.GetValues(typeof(IceCreamFlavor)), f => f != IceCreamFlavor.None);
        }

        static IceCreamToppingSprites FindOrCreateToppingSprites()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(IceCreamToppingSprites)}");

            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);

                if (guids.Length > 1)
                {
                    Debug.LogWarning($"Found {guids.Length} Ice Cream Topping Sprites assets; using '{path}'.");
                }

                return AssetDatabase.LoadAssetAtPath<IceCreamToppingSprites>(path);
            }

            EnsureFolder(System.IO.Path.GetDirectoryName(ToppingSpritesPath).Replace('\\', '/'));

            var toppingSprites = ScriptableObject.CreateInstance<IceCreamToppingSprites>();
            AssetDatabase.CreateAsset(toppingSprites, ToppingSpritesPath);
            toppingSprites.FindSprites();
            EditorUtility.SetDirty(toppingSprites);

            return toppingSprites;
        }

        static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;

            string parent = System.IO.Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(folder));
        }
    }
}
