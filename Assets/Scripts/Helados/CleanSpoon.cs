using UnityEngine;

public class CleanSpoon : MonoBehaviour
{
    [Header("Audio Opcional")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip splashSound;

    private void OnTriggerEnter(Collider other)
    {
        // Busca el script IceCreamSpoon en el objeto que tocó el agua o en su padre
        IceCreamSpoon spoon = other.GetComponentInParent<IceCreamSpoon>();

        if (spoon != null)
        {
            // Solo reproduce el sonido si la cuchara efectivamente tenía un scoop para limpiar
            if (spoon.ClearScoop())
            {
                if (audioSource != null && splashSound != null)
                {
                    audioSource.PlayOneShot(splashSound);
                }
            }
        }
    }
}