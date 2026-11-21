using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors; // Requerido en XRIT 3.x para XRSocketInteractor

public class CoffeeSocketRefill : MonoBehaviour
{
    [Header("Configuración del Socket y Objetos")]
    [Tooltip("Tag que identifica a la taza vacía.")]
    [SerializeField] private string emptyCupTag = "EmptyCup";

    [Tooltip("Prefab de la taza llena de café que reemplazará a la vacía.")]
    [SerializeField] private GameObject fullCupPrefab;

    [Header("Efectos y Audio")]
    [Tooltip("Sistema de partículas que se activará al terminar el refill.")]
    [SerializeField] private ParticleSystem finishParticles;

    [Tooltip("AudioSource para los sonidos.")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("Clip de sonido que se reproduce durante el llenado.")]
    [SerializeField] private AudioClip refillSound;

    [Tooltip("Clip opcional al completar el llenado (ej. un 'ding' o vapor).")]
    [SerializeField] private AudioClip completeSound;

    [Header("Tiempo")]
    [Tooltip("Duración en segundos del proceso de llenado.")]
    [SerializeField] private float refillDuration = 2.5f;

    [Tooltip("Margen vertical para evitar que el collider choque con la bandeja al nacer.")]
    [SerializeField] private float verticalSpawnOffset = 0.01f;

    private XRSocketInteractor socketInteractor;
    private bool isRefilling = false;

    private void Awake()
    {
        socketInteractor = GetComponent<XRSocketInteractor>();
    }

    private void OnEnable()
    {
        // Escuchamos cuando un objeto encaja en el socket
        socketInteractor.selectEntered.AddListener(OnItemInserted);
    }

    private void OnDisable()
    {
        socketInteractor.selectEntered.RemoveListener(OnItemInserted);
    }

    private void OnItemInserted(SelectEnterEventArgs args)
    {
        // Obtenemos el objeto que acaba de encajar
        GameObject insertedObject = args.interactableObject.transform.gameObject;

        if (insertedObject.CompareTag(emptyCupTag) && !isRefilling)
        {
            StartCoroutine(RefillProcessRoutine(insertedObject));
        }
    }

    private IEnumerator RefillProcessRoutine(GameObject emptyCup)
    {
        isRefilling = true;

        // 1. Reproducir sonido de vertido
        if (audioSource != null && refillSound != null)
        {
            audioSource.clip = refillSound;
            audioSource.Play();
        }

        // 2. Esperar el tiempo de llenado
        yield return new WaitForSeconds(refillDuration);

        // Detener sonido de vertido si sigue sonando
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // 3. Obtener posición, rotación y parent exactos antes de reemplazar
        Vector3 spawnPos = emptyCup.transform.position;
        Quaternion spawnRot = emptyCup.transform.rotation;

        // 4. Destruir la taza vacía
        Destroy(emptyCup);

        // 5. Instanciar la taza llena
        if (fullCupPrefab != null)
        {
            Vector3 safeSpawnPos = spawnPos + Vector3.up * verticalSpawnOffset;
            GameObject newCup = Instantiate(fullCupPrefab, safeSpawnPos, spawnRot, null);

            Rigidbody newRb = newCup.GetComponent<Rigidbody>();
            if (newRb != null)
            {
                newRb.linearVelocity = Vector3.zero; 
                newRb.angularVelocity = Vector3.zero;
            }
        }

        // 6. Activar el efecto de partículas de finalización
        if (finishParticles != null)
        {
            finishParticles.Play();
        }

        // 7. Reproducir sonido de finalización (opcional)
        if (audioSource != null && completeSound != null)
        {
            audioSource.PlayOneShot(completeSound);
        }

        isRefilling = false;
    }
}