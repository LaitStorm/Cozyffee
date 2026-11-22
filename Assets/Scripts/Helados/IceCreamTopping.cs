using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class IceCreamTopping : MonoBehaviour
{
    [Header("Configuración del Topping")]
    [Tooltip("Nombre del topping (ej: Frutilla, Cereza, Blueberry, Kiwi)")]
    [SerializeField] private string toppingName = "Cereza";

    [Tooltip("Arrastra aquí el Prefab limpio de esta misma fruta desde la ventana Project.")]
    [SerializeField] private GameObject toppingVisualPrefab;

    private bool isConsumed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isConsumed) return;

        // Detecta si tocó el Trigger del cono
        if (other.TryGetComponent(out IceCreamCone cone))
        {
            GameObject prefabToUse = toppingVisualPrefab != null ? toppingVisualPrefab : gameObject;

            if (cone.TryAddTopping(prefabToUse, toppingName))
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
}