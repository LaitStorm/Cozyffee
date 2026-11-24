using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class FoodRespawn : MonoBehaviour
{
    [Header("Configuración del Prefab")]
    [Tooltip("Prefab (asset) que aparece en su lugar. Si está vacío o apunta a un objeto de la escena, " +
             "aparece una copia de este mismo objeto tal como estaba al tomarlo por primera vez.")]
    [SerializeField] private GameObject foodPrefab;

    [Header("Ajustes de Regeneración")]
    [Tooltip("Tiempo en segundos antes de que reaparezca un nuevo objeto. " +
             "Si este objeto se destruye antes (se come, se usa como topping...), reaparece en ese momento.")]
    [SerializeField] private float respawnDelay = 3.0f;

    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;

    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private Vector3 originalLocalScale;
    private Transform originalParent;
    private bool hasRespawned = false;

    // Copia preparada en Awake, mientras el objeto está intacto. Vive dormida (sin Awake) bajo un padre inactivo
    // hasta que se coloca. Se toma en Awake y no al agarrarlo porque XR ya modificó el Rigidbody (gravedad, drag) en ese momento.
    private GameObject pendingCopy;

    private static Transform copiesHolder;
    private static bool isQuitting;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        copiesHolder = null;
        isQuitting = false;
        Application.quitting -= OnApplicationQuitting;
        Application.quitting += OnApplicationQuitting;
    }

    private static void OnApplicationQuitting()
    {
        isQuitting = true;
    }

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        // Guardamos la transformación inicial (relativa al padre, que es donde reaparece)
        originalParent = transform.parent;
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;
        originalLocalScale = transform.localScale;

        if (grabInteractable == null)
        {
            Debug.LogWarning($"FoodRespawn on '{name}' needs an XRGrabInteractable on the same GameObject; respawning is disabled.", this);
        }

        // Inmóvil al inicio
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        if (grabInteractable != null)
        {
            pendingCopy = CreateCopy();
        }
    }

    private void OnEnable()
    {
        if (grabInteractable == null) return;

        grabInteractable.selectEntered.AddListener(OnFoodGrabbed);
        grabInteractable.selectExited.AddListener(OnFoodReleased);
    }

    private void OnDisable()
    {
        if (grabInteractable == null) return;

        grabInteractable.selectEntered.RemoveListener(OnFoodGrabbed);
        grabInteractable.selectExited.RemoveListener(OnFoodReleased);
    }

    private void OnDestroy()
    {
        if (pendingCopy == null) return;

        // Nunca se tomó, o se está cerrando la escena o el juego: no hay que reponer nada.
        if (!hasRespawned || isQuitting || !gameObject.scene.isLoaded)
        {
            Destroy(pendingCopy);
            pendingCopy = null;
            return;
        }

        // Se destruyó antes de terminar la espera (la corrutina muere con él), así que la copia aparece ahora.
        PlacePendingCopy();
    }

    private void OnFoodGrabbed(SelectEnterEventArgs args)
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

    private void OnFoodReleased(SelectExitEventArgs args)
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
        PlacePendingCopy();
    }

    private GameObject CreateCopy()
    {
        // Un prefab (asset) se copia tal cual. Un objeto de la escena (este mismo, u otro igual) podría comerse, usarse
        // o cambiar (un cono con helado) antes de que termine la espera, así que copiamos este objeto ahora.
        bool isPrefabAsset = foodPrefab != null && !foodPrefab.scene.IsValid();
        GameObject source = isPrefabAsset ? foodPrefab : gameObject;

        // Bajo un padre inactivo la copia no ejecuta Awake hasta que la colocamos.
        GameObject copy = Instantiate(source, GetCopiesHolder());
        copy.name = source.name;
        return copy;
    }

    private void PlacePendingCopy()
    {
        if (pendingCopy == null) return;

        GameObject copy = pendingCopy;
        pendingCopy = null;

        // Primero la transformación y después el padre: al activarse, su Awake guarda estos valores como su posición inicial.
        Transform copyTransform = copy.transform;
        copyTransform.localPosition = originalLocalPosition;
        copyTransform.localRotation = originalLocalRotation;
        copyTransform.localScale = originalLocalScale;
        copyTransform.SetParent(originalParent, false);

        copy.SetActive(true);
    }

    private static Transform GetCopiesHolder()
    {
        if (copiesHolder == null)
        {
            var holder = new GameObject("FoodRespawn Copies");
            holder.SetActive(false);
            copiesHolder = holder.transform;
        }

        return copiesHolder;
    }
}
