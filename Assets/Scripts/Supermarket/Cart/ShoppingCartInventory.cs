using System;
using System.Collections.Generic;
using UnityEngine;

public class ShoppingCartInventory : MonoBehaviour
{
    private readonly Dictionary<ProductDefinition, int> products = new();

    public event Action<ProductDefinition, int> OnProductAdded;

    public int TotalProducts { get; private set; }

    public bool AddProduct(ProductDefinition product)
    {
        if (product == null)
            return false;

        if (!products.ContainsKey(product))
            products[product] = 0;

        products[product]++;
        TotalProducts++;

        OnProductAdded?.Invoke(product, products[product]);

        Debug.Log(
            $"[CART] Added: {product.name} | " +
            $"Quantity: {products[product]} | " +
            $"Total: {TotalProducts}"
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
    }
}