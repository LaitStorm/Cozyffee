using UnityEngine;

namespace Game.Cozyffee
{
    public class Item : MonoBehaviour
    {
        [SerializeField]
        string id = "";

        public string Id => id;
    }
}