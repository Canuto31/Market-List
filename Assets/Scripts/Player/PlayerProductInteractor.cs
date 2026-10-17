
using UnityEngine;

public class PlayerProductInteractor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private InteractionUI interactionUI;

    [Header("Interaction Settings")]
    [SerializeField, Min(0.1f)]
    private float interactionDistance = 3f;

    [SerializeField]
    private LayerMask interactionLayers = ~0;

    [Header("Debug")]
    [SerializeField] private bool showDebug = false;

    private ProductInstance currentProduct;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        if (inputHandler == null)
            inputHandler = GetComponent<PlayerInputHandler>();

        if (interactionUI == null)
            interactionUI = FindFirstObjectByType<InteractionUI>();
    }

    private void OnEnable()
    {
        if (inputHandler != null)
            inputHandler.OnInteract += HandleInteraction;
    }

    private void OnDisable()
    {
        if (inputHandler != null)
            inputHandler.OnInteract -= HandleInteraction;

        if (interactionUI != null)
            interactionUI.HideInteraction();
    }

    private void Update()
    {
        DetectProduct();
        UpdateInteractionUI();
    }

    private void DetectProduct()
    {
        currentProduct = null;

        if (playerCamera == null)
            return;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(
                ray,
                out RaycastHit hit,
                interactionDistance,
                interactionLayers,
                QueryTriggerInteraction.Ignore))
        {
            currentProduct =
                hit.collider.GetComponentInParent<ProductInstance>();

            if (showDebug && currentProduct != null)
            {
                Debug.DrawLine(
                    ray.origin,
                    hit.point,
                    Color.green
                );
            }
        }
    }

    private void UpdateInteractionUI()
    {
        if (interactionUI == null)
            return;

        if (currentProduct == null ||
            currentProduct.CurrentState == productState.Collected)
        {
            interactionUI.HideInteraction();
            return;
        }

        if (currentProduct.CurrentState == productState.Blocked)
        {
            interactionUI.ShowInteraction("[E] Examinar");
        }
        else
        {
            interactionUI.ShowInteraction("[E] Recoger");
        }
    }

    private void HandleInteraction()
    {
        // Actualizar detección al presionar E.
        DetectProduct();

        if (currentProduct == null)
            return;

        bool collected = currentProduct.TryCollect();

        if (collected)
        {
            currentProduct = null;
            interactionUI?.HideInteraction();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Camera cameraReference = playerCamera;

        if (cameraReference == null)
            cameraReference = GetComponentInChildren<Camera>();

        if (cameraReference == null)
            return;

        Gizmos.color = Color.green;

        Gizmos.DrawRay(
            cameraReference.transform.position,
            cameraReference.transform.forward * interactionDistance
        );
    }
}
