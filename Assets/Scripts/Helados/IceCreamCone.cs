using System.Collections.Generic;
using Game.Cozyffee;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class IceCreamCone : MonoBehaviour
{
    [Header("Puntos de Apilamiento")]
    [Tooltip("Punto vacío en el borde superior del cono donde va la primera bola.")]
    [SerializeField] private Transform firstScoopPoint;

    [Tooltip("Altura que sube el segundo scoop respecto al primero.")]
    [SerializeField] private float scoopStackHeight = 0.045f;

    [Tooltip("Altura adicional desde el centro del scoop superior hasta donde apoyan los toppings.")]
    [SerializeField] private float toppingOffsetHeight = 0.025f;

    [Tooltip("Separación lateral entre el 1er y 2do topping para que no se superpongan.")]
    [SerializeField] private float toppingSideOffset = 0.015f;

    [Header("Reglas de Armado")]
    [SerializeField] private int maxScoops = 2;
    [SerializeField] private int maxToppings = 2;
    [SerializeField] private bool requireGrabFirst = true;

    [Header("Audio Opcional")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip placeScoopClip;
    [SerializeField] private AudioClip placeToppingClip;

    // Registro de ingredientes para validar pedidos
    private readonly List<IceCreamFlavor> addedScoopFlavors = new List<IceCreamFlavor>();
    private readonly List<IceCreamToppingType> addedToppings = new List<IceCreamToppingType>();

    /// <summary>Sabores de abajo hacia arriba.</summary>
    public IReadOnlyList<IceCreamFlavor> ScoopFlavors => addedScoopFlavors;

    /// <summary>Toppings en el orden en que se pusieron.</summary>
    public IReadOnlyList<IceCreamToppingType> Toppings => addedToppings;

    /// <summary>ID de lo que hay en el cono (ver <see cref="IceCreamRecipe"/>); "" si está vacío.</summary>
    public string RecipeId { get; private set; } = "";

    // Los scoops y toppings se instancian dentro de este hijo. Si FoodRespawn clona un cono ya armado,
    // el clon copia este hijo pero no la lista de ingredientes, así que se borra en Awake para que empiece vacío.
    private const string ContentsName = "IceCreamContents";
    private Transform contents;

    private readonly List<GameObject> stackedScoops = new List<GameObject>();
    private readonly List<GameObject> stackedToppings = new List<GameObject>();
    private XRGrabInteractable grabInteractable;
    private bool isUnlockedForScoops;

    public bool HasAtLeastOneScoop => stackedScoops.Count > 0;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        isUnlockedForScoops = !requireGrabFirst;

        // Sin un Item el NPC no acepta el cono; se agrega por si no se puso en el Inspector.
        if (!TryGetComponent(out IceCreamItem _))
            gameObject.AddComponent<IceCreamItem>();

        Transform copiedContents = transform.Find(ContentsName);
        if (copiedContents != null)
            Destroy(copiedContents.gameObject);

        contents = new GameObject(ContentsName).transform;
        contents.SetParent(transform, false);
    }

    private void OnEnable()
    {
        if (grabInteractable != null)
            grabInteractable.selectEntered.AddListener(OnConeGrabbed);
    }

    private void OnDisable()
    {
        if (grabInteractable != null)
            grabInteractable.selectEntered.RemoveListener(OnConeGrabbed);
    }

    private void OnConeGrabbed(SelectEnterEventArgs args)
    {
        isUnlockedForScoops = true;
    }

    public bool TryAddScoop(GameObject scoopPrefab, IceCreamFlavor flavor)
    {
        if (!isUnlockedForScoops || stackedScoops.Count >= maxScoops || scoopPrefab == null)
            return false;

        Vector3 spawnPos = firstScoopPoint.position + (firstScoopPoint.up * (scoopStackHeight * stackedScoops.Count));
        GameObject newScoop = Instantiate(scoopPrefab, spawnPos, firstScoopPoint.rotation, contents);

        DisablePhysics(newScoop);

        stackedScoops.Add(newScoop);
        addedScoopFlavors.Add(flavor);
        UpdateRecipeId();

        // Si ya había toppings puestos en el 1er scoop y se agrega un 2do scoop, los subimos al nuevo tope
        RepositionAllToppings();

        if (audioSource != null && placeScoopClip != null)
            audioSource.PlayOneShot(placeScoopClip);

        return true;
    }

    public bool TryAddTopping(GameObject toppingPrefab, IceCreamToppingType topping)
    {
        // Solo acepta toppings si el cono ya tiene al menos 1 scoop y no supera los 2 toppings
        if (!HasAtLeastOneScoop || stackedToppings.Count >= maxToppings || toppingPrefab == null)
            return false;

        int index = stackedToppings.Count;
        Vector3 spawnPos = GetToppingPosition(index);
        GameObject newTopping = Instantiate(toppingPrefab, spawnPos, firstScoopPoint.rotation, contents);

        DisablePhysics(newTopping);

        stackedToppings.Add(newTopping);
        addedToppings.Add(topping);
        UpdateRecipeId();

        if (audioSource != null && placeToppingClip != null)
            audioSource.PlayOneShot(placeToppingClip);

        return true;
    }

    private void UpdateRecipeId()
    {
        RecipeId = IceCreamRecipe.BuildId(addedScoopFlavors, addedToppings);
    }

    private Vector3 GetToppingPosition(int toppingIndex)
    {
        int topScoopIndex = Mathf.Max(0, stackedScoops.Count - 1);
        Vector3 topScoopCenter = firstScoopPoint.position + (firstScoopPoint.up * (scoopStackHeight * topScoopIndex));
        Vector3 baseTopPos = topScoopCenter + (firstScoopPoint.up * toppingOffsetHeight);

        // Desplaza ligeramente a la izquierda (índice 0) o derecha (índice 1)
        float direction = (toppingIndex == 0) ? -1f : 1f;
        return baseTopPos + (firstScoopPoint.right * (toppingSideOffset * direction));
    }

    private void RepositionAllToppings()
    {
        for (int i = 0; i < stackedToppings.Count; i++)
        {
            if (stackedToppings[i] != null)
                stackedToppings[i].transform.position = GetToppingPosition(i);
        }
    }

    private void DisablePhysics(GameObject obj)
    {
        if (obj.TryGetComponent(out Rigidbody rb))
            rb.isKinematic = true;

        foreach (var col in obj.GetComponentsInChildren<Collider>())
            col.enabled = false;
    }
}