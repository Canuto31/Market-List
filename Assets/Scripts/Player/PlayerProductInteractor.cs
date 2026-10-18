
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
    
    [Header("Shopping Cart")]
    [SerializeField]
    private ShoppingCartController shoppingCart;
    
    private SupermarketExit currentExit;

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
        DetectExit();

        if (currentExit != null && currentExit.CanInteract)
        {
            interactionUI.ShowInteraction(
                currentExit.InteractionPrompt
            );

            return;
        }
        
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
        DetectExit();

        if (currentExit != null && currentExit.CanInteract)
        {
            currentExit.Interact();
            return;
        }
        
        DetectProduct();

        if (currentProduct == null)
            return;

        ProductInstance product = currentProduct;

        if (product.CurrentState == productState.Blocked)
        {
            product.TryCollect();
            return;
        }

        if (product.CurrentState != productState.Available)
            return;

        if (shoppingCart == null)
        {
            Debug.LogWarning(
                "[INTERACTION] ShoppingCart not assigned.",
                this
            );
            return;
        }

        if (!shoppingCart.TryAddProduct(product.Definition))
            return;

        if (product.TryCollect())
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
    
    private void DetectExit()
    {
        currentExit = null;

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
                interactionLayers))
        {
            currentExit =
                hit.collider.GetComponentInParent<SupermarketExit>();
        }
    }
}
