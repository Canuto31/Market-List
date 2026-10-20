using UnityEngine;

public class CheckoutCounter : MonoBehaviour
{
    [Header("Checkout Settings")]
    [SerializeField]
    private bool isAvailable = true;

    public bool CanInteract => isAvailable;

    public string InteractionPrompt =>
        isAvailable ? "[E] Pagar compras" : string.Empty;
    
    [Header("References")]
    [SerializeField] private CheckoutProcessor checkoutProcessor;

    public void Interact()
    {
        if (!CanInteract)
            return;

        if (checkoutProcessor == null)
        {
            Debug.LogWarning(
                "[CHECKOUT] CheckoutProcessor not assigned.",
                this
            );
            return;
        }

        checkoutProcessor.ProcessCheckout();
    }
}