using UnityEngine;
using UnityEngine.InputSystem;

public class VRMenuToggle : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El GameObject que tiene el UIDocument de tu Menu.uxml.")]
    [SerializeField] private GameObject menuObject;

    [Tooltip("Opcional: el GameObject del Tutorial por si el jugador abre el menú mientras estaba en el tutorial.")]
    [SerializeField] private GameObject tutorialObject;

    [Tooltip("La Main Camera dentro de XR Origin > Camera Offset.")]
    [SerializeField] private Transform playerCamera;

    [Header("Botón del Control VR")]
    [Tooltip("Acción de Input System (ej: XRI LeftHand/Menu o PrimaryButton).")]
    [SerializeField] private InputActionProperty toggleMenuAction;

    [Header("Posicionamiento frente al Jugador")]
    [Tooltip("Distancia en metros frente a la cara donde aparecerá el menú.")]
    [SerializeField] private float distanceInFront = 1.2f;

    [Tooltip("Ajuste vertical respecto a la altura de los ojos.")]
    [SerializeField] private float heightOffset = -0.1f;

    private void Awake()
    {
        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }
    }

    private void OnEnable()
    {
        if (toggleMenuAction.action != null)
        {
            toggleMenuAction.action.Enable();
            toggleMenuAction.action.performed += OnToggleMenuPerformed;
        }
    }

    private void OnDisable()
    {
        if (toggleMenuAction.action != null)
        {
            toggleMenuAction.action.performed -= OnToggleMenuPerformed;
        }
    }

    private void OnToggleMenuPerformed(InputAction.CallbackContext context)
    {
        if (menuObject == null) return;

        bool willOpen = !menuObject.activeSelf;

        if (willOpen)
        {
            // Si estaba abierto el tutorial, lo cerramos para evitar paneles encimados
            if (tutorialObject != null && tutorialObject.activeSelf)
            {
                tutorialObject.SetActive(false);
            }

            PositionMenuInFrontOfPlayer();
        }

        menuObject.SetActive(willOpen);
    }

    private void PositionMenuInFrontOfPlayer()
    {
        if (playerCamera == null) return;

        // Proyectamos la mirada en el plano horizontal para que el panel no quede inclinado hacia el techo o piso
        Vector3 forwardFlat = playerCamera.forward;
        forwardFlat.y = 0f;
        if (forwardFlat.sqrMagnitude < 0.001f)
            forwardFlat = Vector3.forward;
        forwardFlat.Normalize();

        Vector3 targetPos = playerCamera.position + (forwardFlat * distanceInFront);
        targetPos.y = playerCamera.position.y + heightOffset;

        menuObject.transform.position = targetPos;

        // Hacemos que el menú mire hacia el jugador (con su cara frontal +Z hacia la cámara)
        menuObject.transform.rotation = Quaternion.LookRotation(forwardFlat);
    }
}