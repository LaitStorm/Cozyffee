using UnityEngine;

public class IceCreamSpoon : MonoBehaviour
{
    [Header("Configuración de Cuchara")]
    [Tooltip("Punto vacío en la punta cóncava de la cuchara donde aparece el scoop.")]
    [SerializeField] private Transform scoopHoldPoint;

    [Tooltip("Distancia mínima de movimiento dentro de la cassata para formar la bola (metros).")]
    [SerializeField] private float scrapeDistanceRequired = 0.08f;

    [Header("Audio Opcional")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip scrapeClip;

    private GameObject currentScoopInstance;
    private GameObject currentScoopPrefab;
    private Vector3 lastPosition;
    private float accumulatedScrapeDistance;

    public bool HasScoop => currentScoopInstance != null;

    private void OnTriggerEnter(Collider other)
    {
        // 1. Si ya tenemos un scoop y tocamos un cono, lo transferimos al cono
        if (HasScoop && other.TryGetComponent(out IceCreamCone cone))
        {
            if (cone.TryAddScoop(currentScoopPrefab))
            {
                Destroy(currentScoopInstance);
                currentScoopInstance = null;
                currentScoopPrefab = null;
            }
            return;
        }

        // 2. Si entramos a una cassata con la cuchara vacía, empezamos a medir el raspado
        if (!HasScoop && other.GetComponent<IceCreamCassata>() != null)
        {
            lastPosition = scoopHoldPoint.position;
            accumulatedScrapeDistance = 0f;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (HasScoop) return;

        if (other.TryGetComponent(out IceCreamCassata cassata) && cassata.scoopPrefab != null)
        {
            // Sumar cuánto se ha movido la cuchara dentro del helado
            float step = Vector3.Distance(scoopHoldPoint.position, lastPosition);
            accumulatedScrapeDistance += step;
            lastPosition = scoopHoldPoint.position;

            if (accumulatedScrapeDistance >= scrapeDistanceRequired)
            {
                SpawnScoopOnSpoon(cassata.scoopPrefab);
            }
        }
    }

    private void SpawnScoopOnSpoon(GameObject prefab)
    {
        currentScoopPrefab = prefab;
        currentScoopInstance = Instantiate(prefab, scoopHoldPoint.position, scoopHoldPoint.rotation, scoopHoldPoint);

        // Desactivar físicas y colisiones mientras está en la cuchara para evitar fallos
        if (currentScoopInstance.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
        }
        foreach (var col in currentScoopInstance.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        if (audioSource != null && scrapeClip != null)
        {
            audioSource.PlayOneShot(scrapeClip);
        }
    }
}