using System.Collections.Generic;
using UnityEngine;

public class IceCreamCone : MonoBehaviour
{
    [Header("Configuración del Cono")]
    [Tooltip("Punto vacío en el borde superior del cono donde va la primera bola.")]
    [SerializeField] private Transform firstScoopPoint;

    [Tooltip("Altura que sube cada bola adicional apilada.")]
    [SerializeField] private float scoopStackHeight = 0.045f;

    [SerializeField] private int maxScoops = 3;

    [Header("Audio Opcional")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip placeScoopClip;

    private readonly List<GameObject> stackedScoops = new List<GameObject>();

    public bool TryAddScoop(GameObject scoopPrefab)
    {
        if (stackedScoops.Count >= maxScoops || scoopPrefab == null)
            return false;

        // Calcular altura acumulada según cuántas bolas ya tiene el cono
        Vector3 spawnPos = firstScoopPoint.position + (firstScoopPoint.up * (scoopStackHeight * stackedScoops.Count));
        GameObject newScoop = Instantiate(scoopPrefab, spawnPos, firstScoopPoint.rotation, transform);

        // Mantener el scoop fijo al cono sin físicas independientes
        if (newScoop.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
        }
        foreach (var col in newScoop.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        stackedScoops.Add(newScoop);

        if (audioSource != null && placeScoopClip != null)
        {
            audioSource.PlayOneShot(placeScoopClip);
        }

        return true;
    }
}