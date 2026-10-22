
using System.Collections.Generic;
using UnityEngine;

public class ShoppingStockReplenisher : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private ShoppingListManager shoppingListManager;

    [SerializeField]
    private List<ShelfProductSpawner> shelfSpawners = new();

    [Header("Debug")]
    [SerializeField]
    private bool showDebug = true;

    public bool EnsureRemainingProductsAvailable()
    {
        if (shoppingListManager == null)
        {
            Debug.LogError(
                "[STOCK] ShoppingListManager reference missing.",
                this
            );
            return false;
        }

        Dictionary<ProductDefinition, int> remaining =
            shoppingListManager.GetRemainingProducts();

        Dictionary<ProductDefinition, int> available =
            shoppingListManager.GetAvailableProducts();

        foreach (var item in remaining)
        {
            ProductDefinition product = item.Key;
            int required = item.Value;

            available.TryGetValue(
                product,
                out int currentStock
            );

            int missing = Mathf.Max(
                0,
                required - currentStock
            );

            if (missing == 0)
                continue;

            if (showDebug)
            {
                Debug.Log(
                    $"[STOCK] Missing {missing} units of {product.name}.",
                    this
                );
            }

            foreach (ShelfProductSpawner spawner in shelfSpawners)
            {
                if (spawner == null || missing <= 0)
                    continue;

                int generated = spawner.ReplenishProduct(
                    product,
                    missing
                );

                missing -= generated;
            }

            
            if (missing > 0)
            {
                Debug.LogWarning(
                    $"[STOCK] Normal replenishment failed for " +
                    $"{product.name}. Attempting emergency recovery.",
                    this
                );

                bool recovered = TryEmergencyReplenishment(
                    product,
                    missing
                );

                if (!recovered)
                {
                    Debug.LogError(
                        $"[STOCK] Emergency replenishment could not " +
                        $"fully restore {product.name}.",
                        this
                    );
                }
            }

        }

        bool enough =
            shoppingListManager.HasEnoughProductsForRemainingList();
        
        
        if (!enough)
        {
            Dictionary<ProductDefinition, int> pending =
                shoppingListManager.GetRemainingProducts();

            Dictionary<ProductDefinition, int> currentStock =
                shoppingListManager.GetAvailableProducts();

            foreach (var item in pending)
            {
                currentStock.TryGetValue(
                    item.Key,
                    out int availableQuantity
                );

                int shortage = Mathf.Max(
                    0,
                    item.Value - availableQuantity
                );

                if (shortage > 0)
                {
                    Debug.LogWarning(
                        $"[STOCK] Unresolved shortage: " +
                        $"{item.Key.name} | " +
                        $"Required: {item.Value} | " +
                        $"Available: {availableQuantity} | " +
                        $"Missing: {shortage}",
                        this
                    );
                }
            }
        }


        if (showDebug)
        {
            Debug.Log(
                enough
                    ? "[STOCK] All remaining products are available."
                    : "[STOCK] Some required products are still missing.",
                this
            );
        }

        return enough;
    }
    
    
    private bool TryEmergencyReplenishment(
        ProductDefinition requiredProduct,
        int missingQuantity)
    {
        if (requiredProduct == null || missingQuantity <= 0)
            return true;

        Dictionary<ProductDefinition, int> remaining =
            shoppingListManager.GetRemainingProducts();

        Dictionary<ProductDefinition, int> available =
            shoppingListManager.GetAvailableProducts();

        int missing = missingQuantity;

        foreach (var stock in available)
        {
            ProductDefinition candidate = stock.Key;

            if (candidate == null || candidate == requiredProduct)
                continue;

            remaining.TryGetValue(
                candidate,
                out int requiredQuantity
            );

            int removable = Mathf.Max(
                0,
                stock.Value - requiredQuantity
            );

            if (removable <= 0)
                continue;

            foreach (ShelfProductSpawner spawner in shelfSpawners)
            {
                if (spawner == null || missing <= 0)
                    continue;

                int removed = spawner.RemoveAvailableProducts(
                    candidate,
                    Mathf.Min(removable, missing)
                );

                removable -= removed;

                if (removed == 0)
                    continue;

                foreach (ShelfProductSpawner targetSpawner in shelfSpawners)
                {
                    if (targetSpawner == null || missing <= 0)
                        continue;

                    missing -= targetSpawner.ReplenishProduct(
                        requiredProduct,
                        missing
                    );
                }

                if (missing <= 0)
                    return true;
            }
        }

        return missing <= 0;
    }

}
