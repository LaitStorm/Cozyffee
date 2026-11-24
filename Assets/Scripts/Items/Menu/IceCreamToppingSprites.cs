using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee
{
    /// <summary>
    /// The topping overlays drawn on top of an ice cream's bubble. Each topping has four: left or right slot,
    /// on a one- or two-scoop ice cream. They're full-size layers, so they line up with the scoop bubble as-is.
    /// Create one: Assets > Create > Cozyffee > Menu > Ice Cream Topping Sprites, then use "Find Sprites" in its menu.
    /// </summary>
    [CreateAssetMenu(menuName = "Cozyffee/Menu/Ice Cream Topping Sprites", fileName = "IceCreamToppingSprites")]
    public class IceCreamToppingSprites : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public IceCreamToppingType Topping = IceCreamToppingType.None;

            [Header("One scoop")]
            [Tooltip("First topping, e.g. BubbleToppingCherry.")]
            public Sprite Left;

            [Tooltip("Second topping, e.g. BubbleToppingCherryRight.")]
            public Sprite Right;

            [Header("Two scoops")]
            [Tooltip("First topping, e.g. BubbleDoubleToppingCherry.")]
            public Sprite DoubleLeft;

            [Tooltip("Second topping, e.g. BubbleDoubleToppingCherryRight.")]
            public Sprite DoubleRight;
        }

        [SerializeField]
        List<Entry> entries = new List<Entry>();

        /// <param name="slot">0 for the first topping (left), 1 for the second (right).</param>
        /// <returns>The overlay, or null if it isn't assigned.</returns>
        public Sprite Get(IceCreamToppingType topping, bool twoScoops, int slot)
        {
            foreach (Entry entry in entries)
            {
                if (entry == null || entry.Topping != topping) continue;

                if (twoScoops)
                {
                    return slot == 0 ? entry.DoubleLeft : entry.DoubleRight;
                }

                return slot == 0 ? entry.Left : entry.Right;
            }

            return null;
        }

        #region Editor
#if UNITY_EDITOR

        /// <summary>Adds an entry per topping and assigns Bubble[Double]Topping{Name}[Right] sprites by name.</summary>
        [ContextMenu("Find Sprites")]
        public void FindSprites()
        {
            UnityEditor.Undo.RecordObject(this, "Find Topping Sprites");

            foreach (IceCreamToppingType topping in Enum.GetValues(typeof(IceCreamToppingType)))
            {
                if (topping == IceCreamToppingType.None) continue;

                Entry entry = entries.Find(e => e != null && e.Topping == topping);

                if (entry == null)
                {
                    entry = new Entry { Topping = topping };
                    entries.Add(entry);
                }

                entry.Left = FindSprite($"BubbleTopping{topping}") ?? entry.Left;
                entry.Right = FindSprite($"BubbleTopping{topping}Right") ?? entry.Right;
                entry.DoubleLeft = FindSprite($"BubbleDoubleTopping{topping}") ?? entry.DoubleLeft;
                entry.DoubleRight = FindSprite($"BubbleDoubleTopping{topping}Right") ?? entry.DoubleRight;
            }

            UnityEditor.EditorUtility.SetDirty(this);
        }

        Sprite FindSprite(string spriteName)
        {
            foreach (string guid in UnityEditor.AssetDatabase.FindAssets($"{spriteName} t:Sprite"))
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);

                if (System.IO.Path.GetFileNameWithoutExtension(path) == spriteName)
                {
                    return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
                }
            }

            Debug.LogWarning($"Ice Cream Topping Sprites '{name}': no sprite named '{spriteName}' found.", this);
            return null;
        }

#endif
        #endregion
    }
}
