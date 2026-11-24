using Unity.XR.CoreUtils;
using UnityEngine;

/// <summary>
/// Pasa los visuales del jugador (rayos, controles, manos) a la capa <see cref="layer"/> ("XRVisuals").
/// Los renderers de URP dibujan esa capa al final, después de los menús (feature MenusOnTop),
/// así el rayo y los controles se ven encima de los menús, que se dibujan encima de todo lo demás.
///
/// Solo cambia la capa de objetos que tienen un Renderer y ningún Collider, para no afectar la física ni las interacciones.
/// Revisa cada <see cref="rescanInterval"/> segundos para incluir visuales que se activan o crean después (ej: manos).
/// </summary>
public class DrawXRVisualsAboveMenus : MonoBehaviour
{
    private const int UILayer = 5;

    [Tooltip("Raíz del jugador. Si está vacío, se usa el XR Origin de la escena.")]
    [SerializeField] private Transform root;

    [Tooltip("Capa que los renderers de URP dibujan después de los menús (XRVisuals).")]
    [SerializeField] private int layer = 29;

    [Min(0.1f)]
    [SerializeField] private float rescanInterval = 1f;

    private float nextScan;

    private void Start()
    {
        if (root == null)
        {
            XROrigin origin = FindAnyObjectByType<XROrigin>();
            if (origin != null)
                root = origin.transform;
        }

        if (root == null)
        {
            Debug.LogWarning($"{nameof(DrawXRVisualsAboveMenus)}: no se encontró el XR Origin.", this);
            enabled = false;
            return;
        }

        Apply();
    }

    private void Update()
    {
        if (Time.time >= nextScan)
            Apply();
    }

    private void Apply()
    {
        nextScan = Time.time + rescanInterval;

        foreach (Renderer rendererComponent in root.GetComponentsInChildren<Renderer>(true))
        {
            GameObject go = rendererComponent.gameObject;

            // Los menús (capa UI) ya tienen su propio pase; los objetos con Collider dependen de su capa para la física.
            if (go.layer == layer || go.layer == UILayer || go.GetComponent<Collider>() != null)
                continue;

            go.layer = layer;
        }
    }
}
