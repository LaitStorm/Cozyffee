using Game.Cozyffee;
using UnityEngine;

public class IceCreamCassata : MonoBehaviour
{
    [Tooltip("Sabor de esta cassata. Es el sabor que queda registrado en el cono.")]
    public IceCreamFlavor flavor = IceCreamFlavor.Vanilla;

    [Tooltip("Prefab del scoop (bola de helado) correspondiente a este sabor.")]
    public GameObject scoopPrefab;

    // Nombre de texto antiguo (ej: "Vainilla"). Se convierte a 'flavor' al abrir la escena; borrar cuando todas las escenas estén guardadas.
    [SerializeField, HideInInspector]
    string flavorName = "";

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!string.IsNullOrEmpty(flavorName))
        {
            if (IceCreamLegacyNames.TryParseFlavor(flavorName, out IceCreamFlavor parsed))
            {
                flavor = parsed;
            }
            else
            {
                Debug.LogWarning($"{nameof(IceCreamCassata)} '{name}': unknown old flavor name '{flavorName}'. Set its Flavor by hand.", this);
            }

            flavorName = "";
            UnityEditor.EditorUtility.SetDirty(this);
        }

        if (flavor == IceCreamFlavor.None)
        {
            Debug.LogWarning($"{nameof(IceCreamCassata)} '{name}' has no flavor; its scoops won't match any order.", this);
        }
    }
#endif
}
