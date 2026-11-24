using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Basurero: destruye un objeto cuando el jugador lo suelta dentro de la zona (este trigger) o muy cerca de ella,
/// o cuando un objeto suelto cae o se lanza adentro. Reproduce un efecto de partículas y un sonido opcional.
/// Mientras el jugador lo tenga en la mano, el objeto no se destruye.
/// </summary>
[RequireComponent(typeof(Collider))]
public class TrashBin : MonoBehaviour
{
    [Header("Filtros de Eliminación")]
    [Tooltip("Lista de Tags permitidos para eliminar (ej: Food, Trash, Cup, IceCream).")]
    [SerializeField] private List<string> disposableTags = new List<string> { "Food" };

    [Tooltip("Si está marcado, destruye automáticamente cualquier objeto con IceCreamCone o IceCreamTopping sin importar su Tag.")]
    [SerializeField] private bool destroyIceCreamItems = true;

    [Header("Zona")]
    [Tooltip("Soltar un objeto a esta distancia (en metros) del borde de la zona también lo bota.")]
    [Min(0f)]
    [SerializeField] private float nearDistance = 0.15f;

    [Header("Efectos")]
    [Tooltip("Prefab de partículas que aparece donde se botó el objeto (ej: TrashPoofFX). Se destruye solo al terminar.")]
    [SerializeField] private GameObject trashEffectPrefab;

    [Header("Audio Opcional")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip trashSound;

    // Objetos ya enviados a Destroy este frame (Destroy es diferido), para no botarlos ni reproducir el efecto dos veces.
    private static readonly HashSet<GameObject> trashed = new HashSet<GameObject>();

    private Collider zone;
    private XRInteractionManager[] managers = System.Array.Empty<XRInteractionManager>();
    private readonly List<IXRInteractable> registeredBuffer = new List<IXRInteractable>();
    private readonly HashSet<XRGrabInteractable> listening = new HashSet<XRGrabInteractable>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => trashed.Clear();

    private void Awake()
    {
        zone = GetComponent<Collider>();
        if (!zone.isTrigger)
            Debug.LogWarning($"TrashBin '{name}': el Collider debería ser Trigger.", this);
    }

    private void OnEnable()
    {
        // Escuchamos al soltar cualquier objeto agarrable, incluidas las copias que FoodRespawn crea después.
        managers = FindObjectsByType<XRInteractionManager>();
        foreach (XRInteractionManager manager in managers)
        {
            manager.interactableRegistered += OnInteractableRegistered;
            manager.interactableUnregistered += OnInteractableUnregistered;

            registeredBuffer.Clear();
            manager.GetRegisteredInteractables(registeredBuffer);
            foreach (IXRInteractable interactable in registeredBuffer)
                Listen(interactable);
        }
    }

    private void OnDisable()
    {
        foreach (XRInteractionManager manager in managers)
        {
            if (manager == null) continue;
            manager.interactableRegistered -= OnInteractableRegistered;
            manager.interactableUnregistered -= OnInteractableUnregistered;
        }

        foreach (XRGrabInteractable grab in listening)
        {
            if (grab != null)
                grab.selectExited.RemoveListener(OnReleased);
        }

        listening.Clear();
        managers = System.Array.Empty<XRInteractionManager>();
    }

    private void OnInteractableRegistered(InteractableRegisteredEventArgs args) => Listen(args.interactableObject);

    private void OnInteractableUnregistered(InteractableUnregisteredEventArgs args)
    {
        if (args.interactableObject is XRGrabInteractable grab && listening.Remove(grab))
            grab.selectExited.RemoveListener(OnReleased);
    }

    private void Listen(IXRInteractable interactable)
    {
        if (interactable is XRGrabInteractable grab && listening.Add(grab))
            grab.selectExited.AddListener(OnReleased);
    }

    // El jugador soltó un objeto: si quedó dentro de la zona o muy cerca, se bota.
    private void OnReleased(SelectExitEventArgs args)
    {
        // Cancelado = se deshabilitó o destruyó, no lo soltó el jugador. Un socket (cafetera, NPC) tampoco es el jugador.
        if (args.isCanceled || args.interactorObject is XRSocketInteractor)
            return;

        if (args.interactableObject is not XRGrabInteractable grab || grab == null || grab.isSelected)
            return;

        if (CanDispose(grab) && IsInOrNearZone(grab.gameObject))
            Trash(grab.gameObject);
    }

    // Un objeto suelto (lanzado o que se cayó) entra al basurero.
    private void OnTriggerEnter(Collider other)
    {
        XRGrabInteractable grab = other.GetComponentInParent<XRGrabInteractable>();

        // En la mano no se bota: se bota cuando el jugador lo suelte (OnReleased).
        if (grab == null || grab.isSelected)
            return;

        if (CanDispose(grab))
            Trash(grab.gameObject);
    }

    private bool CanDispose(XRGrabInteractable grab)
    {
        GameObject target = grab.gameObject;

        // Herramientas permanentes (como la cuchara de helado) nunca se botan.
        if (target.GetComponentInChildren<IceCreamSpoon>() != null)
            return false;

        bool isIceCreamAsset = destroyIceCreamItems &&
                               (target.GetComponent<IceCreamCone>() != null ||
                                target.GetComponent<IceCreamTopping>() != null);

        return isIceCreamAsset || disposableTags.Contains(target.tag);
    }

    private bool IsInOrNearZone(GameObject target)
    {
        float maxSqr = nearDistance * nearDistance;

        foreach (Collider col in target.GetComponentsInChildren<Collider>())
        {
            if (!col.enabled || col.isTrigger)
                continue;

            // Punto de la zona más cercano al objeto, y punto del objeto más cercano a ese (0 si se tocan).
            Vector3 onZone = ClosestPoint(zone, col.bounds.center);
            Vector3 onItem = ClosestPoint(col, onZone);
            onZone = ClosestPoint(zone, onItem);

            if ((onZone - onItem).sqrMagnitude <= maxSqr)
                return true;
        }

        return false;
    }

    // Collider.ClosestPoint solo funciona con primitivas y MeshCollider convexos; para el resto usamos su caja.
    private static Vector3 ClosestPoint(Collider col, Vector3 point)
    {
        if (col is MeshCollider mesh && !mesh.convex)
            return col.bounds.ClosestPoint(point);

        return col.ClosestPoint(point);
    }

    private void Trash(GameObject target)
    {
        trashed.RemoveWhere(go => go == null);
        if (!trashed.Add(target))
            return;

        Vector3 position = GetCenter(target);

        OneShotEffect.Play(trashEffectPrefab, position);

        if (trashSound != null)
        {
            if (audioSource != null)
                audioSource.PlayOneShot(trashSound);
            else
                AudioSource.PlayClipAtPoint(trashSound, position);
        }

        Destroy(target);
    }

    private static Vector3 GetCenter(GameObject target)
    {
        bool found = false;
        Bounds bounds = default;

        foreach (Collider col in target.GetComponentsInChildren<Collider>())
        {
            if (!col.enabled || col.isTrigger) continue;

            if (!found) { bounds = col.bounds; found = true; }
            else bounds.Encapsulate(col.bounds);
        }

        return found ? bounds.center : target.transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        Collider col = zone != null ? zone : GetComponent<Collider>();
        if (col == null) return;

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.6f);
        Bounds b = col.bounds;
        b.Expand(nearDistance * 2f);
        Gizmos.DrawWireCube(b.center, b.size);
    }
}
