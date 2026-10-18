
using System;
using System.Collections.Generic;
using UnityEngine;

public class ShoppingCartInventory : MonoBehaviour
{
    [Header("Cart Configuration")]
    [SerializeField, Min(1)]
    private int maxCapacity = 20;

    private readonly Dictionary<ProductDefinition, int> products = new();

    public event Action<ProductDefinition, int> OnProductAdded;
    public event Action OnCartFull;
    public event Action OnInventoryChanged;

    public int TotalProducts { get; private set; }

    public int MaxCapacity => maxCapacity;

    public int RemainingCapacity => Mathf.Max(0, maxCapacity - TotalProducts);

    public bool IsFull => TotalProducts >= maxCapacity;

    public bool AddProduct(ProductDefinition product)
    {
        if (product == null)
            return false;

        if (IsFull)
        {
            Debug.Log("[CART] Cannot add product: cart is full.");

            OnCartFull?.Invoke();

            return false;
        }

        if (!products.ContainsKey(product))
            products[product] = 0;

        products[product]++;

        TotalProducts++;

        OnProductAdded?.Invoke(product, products[product]);
        OnInventoryChanged?.Invoke();

        Debug.Log(
            $"[CART] Added: {product.name} | " +
            $"Quantity: {products[product]} | " +
            $"Capacity: {TotalProducts}/{maxCapacity}"
        );

        return true;
    }

    public int GetQuantity(ProductDefinition product)
    {
        if (product == null)
            return 0;

        return products.TryGetValue(product, out int quantity)
            ? quantity
            : 0;
    }

    public IReadOnlyDictionary<ProductDefinition, int> GetProducts()
    {
        return products;
    }

    public void Clear()
    {
        products.Clear();

        TotalProducts = 0;

        OnInventoryChanged?.Invoke();

        Debug.Log("[CART] Inventory cleared.");
    }
}
