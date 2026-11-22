using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class TrashBin : MonoBehaviour
{
    [Header("Filtros de Eliminación")]
    [Tooltip("Lista de Tags permitidos para eliminar (ej: Food, Trash, Cup, IceCream).")]
    [SerializeField] private List<string> disposableTags = new List<string> { "Food", "Untagged" };

    [Tooltip("Si está marcado, destruye automáticamente cualquier objeto con IceCreamCone o IceCreamTopping sin importar su Tag.")]
    [SerializeField] private bool destroyIceCreamItems = true;

    [Header("Audio Opcional")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip trashSound;

    private void OnTriggerEnter(Collider other)
    {
        // Obtener el objeto raíz del interactuable (evita destruir solo un collider hijo)
        XRGrabInteractable grabInteractable = other.GetComponentInParent<XRGrabInteractable>();
        GameObject targetObject = grabInteractable != null ? grabInteractable.gameObject : other.gameObject;

        // 1. Proteger herramientas permanentes (como la cuchara de helado) y objetos que no sean agarrables
        if (grabInteractable == null || targetObject.GetComponentInChildren<IceCreamSpoon>() != null)
            return;

        // 2. Comprobar si es un helado/topping o si coincide con los Tags configurados
        bool isIceCreamAsset = destroyIceCreamItems &&
                               (targetObject.GetComponent<IceCreamCone>() != null ||
                                targetObject.GetComponent<IceCreamTopping>() != null);

        bool hasValidTag = disposableTags.Contains(targetObject.tag);

        if (!isIceCreamAsset && !hasValidTag)
            return;

        // 3. Si el jugador aún lo tiene agarrado en la mano, soltarlo antes de destruir para evitar errores en XRIT
        if (grabInteractable.isSelected && grabInteractable.interactionManager != null)
        {
            grabInteractable.interactionManager.SelectExit(grabInteractable.firstInteractorSelecting, grabInteractable);
        }

        if (audioSource != null && trashSound != null)
        {
            audioSource.PlayOneShot(trashSound);
        }

        Destroy(targetObject);
    }
}