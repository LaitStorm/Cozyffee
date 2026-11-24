using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TutorialControllerUI : MonoBehaviour
{
    [Serializable]
    public class TutorialStep
    {
        [Tooltip("Título de esta etapa del tutorial.")]
        public string stepTitle = "Paso 1: Preparar Café";

        [TextArea(2, 5)]
        [Tooltip("Instrucciones detalladas que aparecerán en el panel.")]
        public string stepDescription = "Toma una taza y colócala bajo la cafetera...";

        [Tooltip("Objeto vacío (Transform) en la escena donde aparecerá el panel en este paso.")]
        public Transform worldAnchor;
    }

    [Header("Documentos UI")]
    [Tooltip("El UIDocument de este tutorial.")]
    [SerializeField] private UIDocument tutorialDocument;

    [Tooltip("El UIDocument del menú principal para volver atrás.")]
    [SerializeField] private UIDocument menuDocument;

    [Header("Pasos y Zonas del Tutorial")]
    [SerializeField] private List<TutorialStep> steps = new List<TutorialStep>();

    [Header("Nombres de Elementos en UI Builder (UXML)")]
    [SerializeField] private string titleLabelName = "Titulo_paso";
    [SerializeField] private string descriptionLabelName = "Texto_paso";
    [SerializeField] private string btnNextName = "Boton_siguiente";
    [SerializeField] private string btnPrevName = "Boton_anterior";
    [SerializeField] private string btnMenuName = "Boton_menu";

    private Label titleLabel;
    private Label descriptionLabel;
    private Button btnNext;
    private Button btnPrev;
    private Button btnMenu;

    private int currentStepIndex = 0;

    private void Awake()
    {
        if (tutorialDocument == null)
            tutorialDocument = GetComponent<UIDocument>();
    }

    private void OnEnable()
    {
        if (tutorialDocument == null) return;

        VisualElement root = tutorialDocument.rootVisualElement;

        // Buscar textos y botones por su atributo 'name' en el UXML
        titleLabel = root.Q<Label>(titleLabelName);
        descriptionLabel = root.Q<Label>(descriptionLabelName);
        btnNext = root.Q<Button>(btnNextName);
        btnPrev = root.Q<Button>(btnPrevName);
        btnMenu = root.Q<Button>(btnMenuName);

        // Suscribir botones
        if (btnNext != null) btnNext.clicked += OnNextClicked;
        if (btnPrev != null) btnPrev.clicked += OnPrevClicked;
        if (btnMenu != null) btnMenu.clicked += OnBackToMenuClicked;

        // Siempre iniciar desde el paso 0 al abrir el tutorial
        currentStepIndex = 0;
        UpdateStepDisplay();
    }

    private void OnDisable()
    {
        if (btnNext != null) btnNext.clicked -= OnNextClicked;
        if (btnPrev != null) btnPrev.clicked -= OnPrevClicked;
        if (btnMenu != null) btnMenu.clicked -= OnBackToMenuClicked;
    }

    private void OnNextClicked()
    {
        if (currentStepIndex < steps.Count - 1)
        {
            currentStepIndex++;
            UpdateStepDisplay();
        }
        else
        {
            // Si ya estaba en el último paso, vuelve al menú (o cierra el tutorial)
            OnBackToMenuClicked();
        }
    }

    private void OnPrevClicked()
    {
        if (currentStepIndex > 0)
        {
            currentStepIndex--;
            UpdateStepDisplay();
        }
    }

    private void OnBackToMenuClicked()
    {
        if (menuDocument != null)
        {
            // El menú vuelve donde está el tutorial (el paso actual), no donde se abrió la última vez
            menuDocument.transform.SetPositionAndRotation(transform.position, transform.rotation);
            menuDocument.gameObject.SetActive(true);
        }

        tutorialDocument.gameObject.SetActive(false);
    }

    private void UpdateStepDisplay()
    {
        if (steps == null || steps.Count == 0)
        {
            Debug.LogWarning("[TutorialControllerUI] No hay pasos configurados en la lista 'Steps'.");
            return;
        }

        TutorialStep current = steps[currentStepIndex];

        // 1. Actualizar textos en el UIDocument
        if (titleLabel != null)
            titleLabel.text = current.stepTitle;

        if (descriptionLabel != null)
            descriptionLabel.text = current.stepDescription;

        // 2. Mover y rotar el panel a la zona específica de la cafetería
        if (current.worldAnchor != null)
        {
            transform.position = current.worldAnchor.position;
            transform.rotation = current.worldAnchor.rotation;
        }

        // 3. Ajustar estado de los botones Siguiente / Anterior
        if (btnPrev != null)
        {
            // Oculta o desactiva "Anterior" en el primer paso
            btnPrev.SetEnabled(currentStepIndex > 0);
            btnPrev.style.opacity = (currentStepIndex > 0) ? 1f : 0.4f;
        }

        if (btnNext != null)
        {
            // En el último paso cambia el texto de "Siguiente" a "Finalizar"
            bool isLastStep = (currentStepIndex == steps.Count - 1);
            btnNext.text = isLastStep ? "Finalizar" : "Siguiente";
        }
    }
}