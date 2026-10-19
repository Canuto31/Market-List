using UnityEngine;

public class CheckoutCounter : MonoBehaviour
{
    [Header("Checkout Settings")]
    [SerializeField]
    private bool isAvailable = true;

    [Header("Debug")]
    [SerializeField]
    private bool showDebug = true;

    public bool CanInteract => isAvailable;

    public string InteractionPrompt =>
        isAvailable ? "[E] Pagar compras" : string.Empty;

    public void Interact()
    {
        if (!CanInteract)
            return;

        if (showDebug)
        {
            Debug.Log(
                "[CHECKOUT] El jugador interactuó con la caja registradora.",
                this
            );
        }
    }
}