using System.Collections.Generic;
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
    public List<string> AddedScoopFlavors { get; private set; } = new List<string>();
    public List<string> AddedToppingNames { get; private set; } = new List<string>();

    private readonly List<GameObject> stackedScoops = new List<GameObject>();
    private readonly List<GameObject> stackedToppings = new List<GameObject>();
    private XRGrabInteractable grabInteractable;
    private bool isUnlockedForScoops;

    public bool HasAtLeastOneScoop => stackedScoops.Count > 0;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        isUnlockedForScoops = !requireGrabFirst;
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

    public bool TryAddScoop(GameObject scoopPrefab, string flavorName = "Default")
    {
        if (!isUnlockedForScoops || stackedScoops.Count >= maxScoops || scoopPrefab == null)
            return false;

        Vector3 spawnPos = firstScoopPoint.position + (firstScoopPoint.up * (scoopStackHeight * stackedScoops.Count));
        GameObject newScoop = Instantiate(scoopPrefab, spawnPos, firstScoopPoint.rotation, transform);

        DisablePhysics(newScoop);

        stackedScoops.Add(newScoop);
        AddedScoopFlavors.Add(flavorName);

        // Si ya había toppings puestos en el 1er scoop y se agrega un 2do scoop, los subimos al nuevo tope
        RepositionAllToppings();

        if (audioSource != null && placeScoopClip != null)
            audioSource.PlayOneShot(placeScoopClip);

        return true;
    }

    public bool TryAddTopping(GameObject toppingPrefab, string toppingName)
    {
        // Solo acepta toppings si el cono ya tiene al menos 1 scoop y no supera los 2 toppings
        if (!HasAtLeastOneScoop || stackedToppings.Count >= maxToppings || toppingPrefab == null)
            return false;

        int index = stackedToppings.Count;
        Vector3 spawnPos = GetToppingPosition(index);
        GameObject newTopping = Instantiate(toppingPrefab, spawnPos, firstScoopPoint.rotation, transform);

        DisablePhysics(newTopping);

        stackedToppings.Add(newTopping);
        AddedToppingNames.Add(toppingName);

        if (audioSource != null && placeToppingClip != null)
            audioSource.PlayOneShot(placeToppingClip);

        return true;
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