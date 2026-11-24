using Game.Cozyffee;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class IceCreamTopping : MonoBehaviour
{
    [Header("Configuración del Topping")]
    [Tooltip("Tipo de topping (ej: Frutilla, Cereza, Blueberry, Kiwi)")]
    [SerializeField] private IceCreamToppingType topping = IceCreamToppingType.Cherry;

    [Tooltip("Arrastra aquí el Prefab limpio de esta misma fruta desde la ventana Project.")]
    [SerializeField] private GameObject toppingVisualPrefab;

    // Nombre de texto antiguo (ej: "Cherry"). Se convierte a 'topping' al abrir la escena; borrar cuando todas las escenas estén guardadas.
    [SerializeField, HideInInspector] private string toppingName = "";

    private bool isConsumed = false;

    public IceCreamToppingType Topping => topping;

    private void OnTriggerEnter(Collider other)
    {
        if (isConsumed) return;

        // Detecta si tocó el Trigger del cono
        if (other.TryGetComponent(out IceCreamCone cone))
        {
            GameObject prefabToUse = toppingVisualPrefab != null ? toppingVisualPrefab : gameObject;

            if (cone.TryAddTopping(prefabToUse, topping))
            {
                isConsumed = true;

                // Suelta el objeto de la mano del jugador antes de destruir la fruta suelta
                if (TryGetComponent(out XRGrabInteractable grab) && grab.isSelected)
                {
                    grab.interactionManager.SelectExit(grab.firstInteractorSelecting, grab);
                }

                Destroy(gameObject);
            }
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(toppingName))
        {
            return;
        }

        if (IceCreamLegacyNames.TryParseTopping(toppingName, out IceCreamToppingType parsed))
        {
            topping = parsed;
        }
        else
        {
            Debug.LogWarning($"{nameof(IceCreamTopping)} '{name}': unknown old topping name '{toppingName}'. Set its Topping by hand.", this);
        }

        toppingName = "";
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
