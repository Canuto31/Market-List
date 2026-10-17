using UnityEngine;

public class ShoppingCartController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShoppingCartInventory inventory;
    [SerializeField] private ShoppingCartVisual cartVisual;

    private void Awake()
    {
        if (inventory == null)
            inventory = GetComponent<ShoppingCartInventory>();

        if (cartVisual == null)
            cartVisual = GetComponent<ShoppingCartVisual>();
    }

    public bool TryAddProduct(ProductDefinition product)
    {
        if (inventory == null)
        {
            Debug.LogError(
                "[CART] ShoppingCartInventory missing.",
                this
            );
            return false;
        }

        if (!inventory.AddProduct(product))
            return false;

        if (cartVisual != null)
            cartVisual.AddVisualProduct(product);

        return true;
    }
}