using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class CoffeeSocketRefill : MonoBehaviour
{
    [Serializable]
    public class RefillRecipe
    {
        [Tooltip("Tag del envase vacío (ej: EmptyCup, EmptyGlass1, EmptyGlass2).")]
        public string emptyTag;

        [Tooltip("Prefab del envase lleno que reemplazará al vacío.")]
        public GameObject fullPrefab;
    }

    [Header("Configuración del Socket y Objetos")]
    [Tooltip("Lista de pares (Tag vacío -> Prefab lleno) que acepta esta máquina.")]
    [SerializeField] private List<RefillRecipe> refillRecipes = new List<RefillRecipe>();

    [Header("Efectos y Audio")]
    [SerializeField] private ParticleSystem finishParticles;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip refillSound;
    [SerializeField] private AudioClip completeSound;

    [Header("Tiempo")]
    [SerializeField] private float refillDuration = 2f;
    [SerializeField] private float verticalSpawnOffset = 0.01f;

    private XRSocketInteractor socketInteractor;
    private Coroutine activeRefillCoroutine;
    private IXRSelectInteractable currentEmptyInteractable;

    private void Awake()
    {
        socketInteractor = GetComponent<XRSocketInteractor>();
    }

    private void OnEnable()
    {
        if (socketInteractor != null)
        {
            socketInteractor.selectEntered.AddListener(OnObjectPlacedInSocket);
            socketInteractor.selectExited.AddListener(OnObjectRemovedFromSocket);
        }
    }

    private void OnDisable()
    {
        if (socketInteractor != null)
        {
            socketInteractor.selectEntered.RemoveListener(OnObjectPlacedInSocket);
            socketInteractor.selectExited.RemoveListener(OnObjectRemovedFromSocket);
        }
    }

    private void OnObjectPlacedInSocket(SelectEnterEventArgs args)
    {
        Transform placedTransform = args.interactableObject.transform;
        GameObject matchingFullPrefab = GetMatchingPrefab(placedTransform);

        // Si el objeto colocado no coincide con ningún Tag de la lista (o ya está lleno), no hacemos nada
        if (matchingFullPrefab == null)
            return;

        currentEmptyInteractable = args.interactableObject;

        if (activeRefillCoroutine != null)
            StopCoroutine(activeRefillCoroutine);

        activeRefillCoroutine = StartCoroutine(RefillRoutine(placedTransform.gameObject, matchingFullPrefab));
    }

    private void OnObjectRemovedFromSocket(SelectExitEventArgs args)
    {
        // Si el jugador retira el envase antes de que termine de llenarse, cancelamos el proceso y el audio
        if (currentEmptyInteractable == args.interactableObject && activeRefillCoroutine != null)
        {
            StopCoroutine(activeRefillCoroutine);
            activeRefillCoroutine = null;
            currentEmptyInteractable = null;

            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }

    private GameObject GetMatchingPrefab(Transform obj)
    {
        foreach (RefillRecipe recipe in refillRecipes)
        {
            if (!string.IsNullOrEmpty(recipe.emptyTag) && recipe.fullPrefab != null)
            {
                if (obj.CompareTag(recipe.emptyTag))
                {
                    return recipe.fullPrefab;
                }
            }
        }
        return null;
    }

    private IEnumerator RefillRoutine(GameObject emptyObject, GameObject fullPrefab)
    {
        // 1. Reproducir sonido de llenado
        if (audioSource != null && refillSound != null)
        {
            audioSource.clip = refillSound;
            audioSource.Play();
        }

        // 2. Esperar la duración del llenado
        yield return new WaitForSeconds(refillDuration);

        // Verificar que el envase vacío siga existiendo antes de reemplazarlo
        if (emptyObject == null)
            yield break;

        // Guardar posición y rotación exacta donde encajó en el socket
        Transform attachPoint = socketInteractor.attachTransform != null ? socketInteractor.attachTransform : transform;
        Vector3 spawnPos = attachPoint.position + (Vector3.up * verticalSpawnOffset);
        Quaternion spawnRot = attachPoint.rotation;

        // 3. Eliminar el envase vacío e instanciar el lleno
        currentEmptyInteractable = null;
        Destroy(emptyObject);

        GameObject fullInstance = Instantiate(fullPrefab, spawnPos, spawnRot);

        // Hacer que el socket agarre automáticamente el nuevo envase lleno si es un XRBaseInteractable
        if (fullInstance.TryGetComponent(out IXRSelectInteractable newInteractable) && socketInteractor.interactionManager != null)
        {
            socketInteractor.interactionManager.SelectEnter(socketInteractor, newInteractable);
        }

        // 4. Efectos finales (partículas y sonido de listo)
        if (finishParticles != null)
        {
            finishParticles.Play();
        }

        if (audioSource != null)
        {
            audioSource.Stop();
            if (completeSound != null)
            {
                audioSource.PlayOneShot(completeSound);
            }
        }

        activeRefillCoroutine = null;
    }
}