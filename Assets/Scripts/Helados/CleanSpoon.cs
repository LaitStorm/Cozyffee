using UnityEngine;

/// <summary>
/// Agua para lavar la cuchara: al meterla con un scoop, el scoop desaparece, suena un sonido y salen burbujas.
/// </summary>
public class CleanSpoon : MonoBehaviour
{
    [Header("Efectos")]
    [Tooltip("Prefab de partículas que aparece en la punta de la cuchara al lavarla (ej: SpoonCleanBubblesFX). Se destruye solo al terminar.")]
    [SerializeField] private GameObject cleanEffectPrefab;

    [Header("Audio Opcional")]
    [Tooltip("Si está vacío, el sonido se reproduce en la posición de la cuchara.")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip splashSound;

    private void OnTriggerEnter(Collider other)
    {
        // Busca el script IceCreamSpoon en el objeto que tocó el agua o en su padre
        IceCreamSpoon spoon = other.GetComponentInParent<IceCreamSpoon>();
        if (spoon == null)
            return;

        // La posición se toma antes de limpiar, mientras el scoop sigue en la punta
        Vector3 position = spoon.ScoopPosition;

        // Solo hay efectos si la cuchara efectivamente tenía un scoop para limpiar
        if (!spoon.ClearScoop())
            return;

        OneShotEffect.Play(cleanEffectPrefab, position);

        if (splashSound != null)
        {
            if (audioSource != null)
                audioSource.PlayOneShot(splashSound);
            else
                AudioSource.PlayClipAtPoint(splashSound, position);
        }
    }
}
