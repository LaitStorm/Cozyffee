using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class FoodRespawn : MonoBehaviour
{
    [Header("Configuración del Prefab")]
    [Tooltip("El Prefab del objeto que se va a instanciar.")]
    [SerializeField] private GameObject foodPrefab;

    [Header("Ajustes de Regeneración")]
    [Tooltip("Tiempo en segundos antes de que reaparezca un nuevo objeto.")]
    [SerializeField] private float respawnDelay = 3.0f;

    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalParent;
    private bool hasRespawned = false;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        // Guardamos la transformación inicial
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalParent = transform.parent;

        // Inmóvil al inicio
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    private void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnFoodGrabbed);
        grabInteractable.selectExited.AddListener(OnFoodReleased);
    }

    private void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnFoodGrabbed);
        grabInteractable.selectExited.RemoveListener(OnFoodReleased);
    }

    private void OnFoodGrabbed(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args)
    {
        // Al tomarlo, habilitamos la física para cuando vuelva a ser soltado
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        // Iniciar el temporizador de reaparición
        if (!hasRespawned)
        {
            hasRespawned = true;
            StartCoroutine(SpawnNewFoodRoutine());
        }
    }

    private void OnFoodReleased(UnityEngine.XR.Interaction.Toolkit.SelectExitEventArgs args)
    {
        // Al soltarlo, garantizamos que vuelva a responder totalmente a la gravedad e impulsos
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    private IEnumerator SpawnNewFoodRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        if (foodPrefab != null)
        {
            Instantiate(foodPrefab, originalPosition, originalRotation, originalParent);
        }
        else
        {
            Debug.LogWarning("No se asignó el 'foodPrefab' en el Inspector.");
        }
    }
}