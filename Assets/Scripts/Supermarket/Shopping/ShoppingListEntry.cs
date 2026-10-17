using System;
using UnityEngine;

[Serializable]
public class ShoppingListEntry
{
    public ProductDefinition Product { get; private set; }

    public int RequiredQuantity { get; private set; }

    public int CollectedQuantity { get; private set; }

    public bool IsCompleted =>
        CollectedQuantity >= RequiredQuantity;

    public ShoppingListEntry(
        ProductDefinition product,
        int requiredQuantity)
    {
        Product = product;
        RequiredQuantity = Mathf.Max(1, requiredQuantity);
        CollectedQuantity = 0;
    }

    public bool TryAddCollected(int amount = 1)
    {
        if (amount <= 0 || IsCompleted)
            return false;

        CollectedQuantity = Mathf.Min(
            CollectedQuantity + amount,
            RequiredQuantity
        );

        return true;
    }
}