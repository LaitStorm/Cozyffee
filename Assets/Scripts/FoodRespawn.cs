using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables; // Necesario para XR Interaction Toolkit 3.x

public class FoodSpawner : MonoBehaviour
{
    [Header("Configuración del Prefab")]
    [Tooltip("El Prefab que se va a instanciar.")]
    [SerializeField] private GameObject foodPrefab;

    [Header("Ajustes de Regeneración")]
    [Tooltip("Tiempo en segundos antes de que reaparezca instancia.")]
    [SerializeField] private float respawnDelay = 3.0f;

    private XRGrabInteractable grabInteractable;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalParent;
    private bool hasRespawned = false;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        
        // Guardamos la posición, rotación y padre iniciales
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalParent = transform.parent;
    }

    private void OnEnable()
    {
        // Nos suscribimos al evento cuando el objeto es agarrado
        grabInteractable.selectEntered.AddListener(OnFoodGrabbed);
    }

    private void OnDisable()
    {
        // Nos desuscribimos para evitar errores de memoria
        grabInteractable.selectEntered.RemoveListener(OnFoodGrabbed);
    }

    private void OnFoodGrabbed(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args)
    {
        // Aseguramos que solo cree una copia por cada agarre
        if (!hasRespawned)
        {
            hasRespawned = true;
            StartCoroutine(SpawnNewFoodRoutine());
        }
    }

    private IEnumerator SpawnNewFoodRoutine()
    {
        // Espera los segundos configurados
        yield return new WaitForSeconds(respawnDelay);

        // Instancia un nuevo FOod en el lugar original
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