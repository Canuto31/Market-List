using System;
using UnityEngine;

public class CheckoutProcessor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShoppingListManager shoppingList;
    [SerializeField] private ShoppingCartInventory inventory;
    [SerializeField] private ShoppingCartVisual cartVisual;

    public event Action OnPurchaseCompleted;
    public event Action OnPurchaseIncomplete;

    public void ProcessCheckout()
    {
        if (shoppingList == null || inventory == null)
        {
            Debug.LogError(
                "[CHECKOUT] Missing references.",
                this
            );
            return;
        }

        if (!shoppingList.HasActiveList)
        {
            Debug.LogWarning(
                "[CHECKOUT] No active shopping list.",
                this
            );
            return;
        }

        if (shoppingList.IsListCompleted)
        {
            CompletePurchase();
            return;
        }

        if (inventory.IsFull)
        {
            IncompletePurchase();
            return;
        }

        Debug.Log(
            "[CHECKOUT] Todavía tienes productos pendientes.",
            this
        );
    }

    private void CompletePurchase()
    {
        ClearCart();

        Debug.Log(
            "[CHECKOUT] Compra completada.",
            this
        );

        OnPurchaseCompleted?.Invoke();
    }

    private void IncompletePurchase()
    {
        ClearCart();

        Debug.Log(
            "[CHECKOUT] Creo que me falta algo...",
            this
        );

        OnPurchaseIncomplete?.Invoke();
    }

    private void ClearCart()
    {
        inventory.Clear();

        if (cartVisual != null)
            cartVisual.ClearVisuals();
    }
}