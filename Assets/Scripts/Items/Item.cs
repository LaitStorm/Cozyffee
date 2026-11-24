using UnityEngine;

namespace Game.Cozyffee
{
    public class Item : MonoBehaviour
    {
        [Tooltip("What this object is on the menu. When set, its ID is used and the text ID below is ignored.")]
        [SerializeField]
        CafeMenuItem definition;

        [Tooltip("Fallback ID used when no Definition is assigned.")]
        [SerializeField]
        string id = "";

        public CafeMenuItem Definition => definition;

        public virtual string Id => definition != null ? definition.Id : id;
    }
}
