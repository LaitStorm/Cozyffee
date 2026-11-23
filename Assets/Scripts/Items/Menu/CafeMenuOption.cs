using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee
{
    /// <summary>
    /// Something that can go in a <see cref="CafeMenu"/>: either a single <see cref="CafeMenuItem"/>,
    /// or another <see cref="CafeMenu"/> (e.g. a Donuts menu inside the main menu).
    /// </summary>
    public abstract class CafeMenuOption : ScriptableObject
    {
        [SerializeField]
        protected string displayName = "";

        [Tooltip("Untick to take this off every menu it appears in, without deleting it (e.g. out of stock).")]
        [SerializeField]
        protected bool available = true;

        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public bool Available => available;

        /// <summary>True if picking this option can produce an item right now.</summary>
        public bool CanBeOrdered => IsOrderable(new HashSet<CafeMenu>());

        /// <param name="visiting">Menus already being looked at higher up, so a menu that contains itself can't loop forever.</param>
        internal abstract bool IsOrderable(HashSet<CafeMenu> visiting);

        /// <summary>Returns the item this option resolves to (itself, or a random pick for a menu), or null.</summary>
        internal abstract CafeMenuItem PickItem(HashSet<CafeMenu> visiting);
    }
}
