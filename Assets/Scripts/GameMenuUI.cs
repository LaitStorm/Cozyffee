using UnityEngine;
using UnityEngine.UIElements;

public class GameMenuUI : MonoBehaviour
{
    [Header("Documentos UI (UIDocuments)")]
    [Tooltip("El UIDocument de este menú principal.")]
    [SerializeField] private UIDocument menuDocument;

    [Tooltip("El UIDocument del tutorial básico.")]
    [SerializeField] private UIDocument tutorialDocument;

    [Tooltip("El UIDocument de configuraciones (opcional por ahora).")]
    [SerializeField] private UIDocument configDocument;

    // Nombres exactos definidos en Menu.uxml
    private const string BtnTutorialName = "Boton_tutorial";
    private const string BtnConfigName = "Boton_config";
    private const string BtnSalirName = "Boton_salir";
    private const string BtnVolverName = "Boton_volver";

    private Button btnTutorial;
    private Button btnConfig;
    private Button btnSalir;
    private Button btnVolver;

    private void Awake()
    {
        if (menuDocument == null)
            menuDocument = GetComponent<UIDocument>();

        // Asegurar que los sub-paneles empiecen cerrados al arrancar
        if (tutorialDocument != null)
            tutorialDocument.gameObject.SetActive(false);

        if (configDocument != null)
            configDocument.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (menuDocument == null) return;

        VisualElement root = menuDocument.rootVisualElement;

        // Buscar cada botón por su atributo 'name' del UXML
        btnTutorial = root.Q<Button>(BtnTutorialName);
        btnConfig = root.Q<Button>(BtnConfigName);
        btnSalir = root.Q<Button>(BtnSalirName);
        btnVolver = root.Q<Button>(BtnVolverName);

        // Suscribir eventos de clic
        if (btnTutorial != null) btnTutorial.clicked += OnTutorialClicked;
        if (btnConfig != null) btnConfig.clicked += OnConfigClicked;
        if (btnSalir != null) btnSalir.clicked += OnSalirClicked;
        if (btnVolver != null) btnVolver.clicked += OnVolverClicked;
    }

    private void OnDisable()
    {
        // Desuscribir eventos al desactivar el panel
        if (btnTutorial != null) btnTutorial.clicked -= OnTutorialClicked;
        if (btnConfig != null) btnConfig.clicked -= OnConfigClicked;
        if (btnSalir != null) btnSalir.clicked -= OnSalirClicked;
        if (btnVolver != null) btnVolver.clicked -= OnVolverClicked;
    }

    private void OnTutorialClicked()
    {
        if (tutorialDocument != null)
        {
            tutorialDocument.gameObject.SetActive(true);
            menuDocument.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[GameMenuUI] Falta asignar el UIDocument de Tutorial en el Inspector.");
        }
    }

    private void OnConfigClicked()
    {
        if (configDocument != null)
        {
            // El menú puede haberse movido frente al jugador (VRMenuToggle): Configuración aparece donde está el menú
            configDocument.transform.SetPositionAndRotation(menuDocument.transform.position, menuDocument.transform.rotation);
            configDocument.gameObject.SetActive(true);
            menuDocument.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("[GameMenuUI] Panel de configuraciones aún no asignado.");
        }
    }

    private void OnVolverClicked()
    {
        // Oculta el menú para seguir jugando en la cafetería
        menuDocument.gameObject.SetActive(false);
    }

    private void OnSalirClicked()
    {
        Debug.Log("[GameMenuUI] Saliendo del juego...");

#if UNITY_EDITOR
        // Detiene el Play Mode si estás probando dentro del editor de Unity
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Cierra la aplicación compilada en los lentes VR / PC
        Application.Quit();
#endif
    }
}