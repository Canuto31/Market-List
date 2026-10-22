
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShoppingCycleManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private CheckoutProcessor checkoutProcessor;

    [SerializeField]
    private ShoppingListManager shoppingListManager;
    
    [SerializeField]
    private ShoppingStockReplenisher stockReplenisher;

    [SerializeField]
    private List<ShelfProductSpawner> shelfSpawners = new();

    [Header("Cycle Settings")]
    [SerializeField, Min(1)]
    private int initialCycle = 1;

    [Header("Debug")]
    [SerializeField]
    private bool showDebug = true;

    public int CurrentCycle { get; private set; }

    public bool IsTransitioning { get; private set; }

    public event Action<int> OnCycleStarted;
    public event Action<int> OnCycleCompleted;
    public event Action OnIncompleteCheckout;

    private void Awake()
    {
        CurrentCycle = Mathf.Max(1, initialCycle);
    }

    private void OnEnable()
    {
        if (checkoutProcessor == null)
        {
            Debug.LogError(
                "[CYCLE] CheckoutProcessor reference missing.",
                this
            );
            return;
        }

        checkoutProcessor.OnPurchaseCompleted +=
            HandlePurchaseCompleted;

        checkoutProcessor.OnPurchaseIncomplete +=
            HandlePurchaseIncomplete;
    }

    private void Start()
    {
        if (showDebug)
            Debug.Log($"[CYCLE] Started cycle {CurrentCycle}.", this);

        OnCycleStarted?.Invoke(CurrentCycle);
    }

    private void OnDisable()
    {
        if (checkoutProcessor == null)
            return;

        checkoutProcessor.OnPurchaseCompleted -=
            HandlePurchaseCompleted;

        checkoutProcessor.OnPurchaseIncomplete -=
            HandlePurchaseIncomplete;

        StopAllCoroutines();
        IsTransitioning = false;
    }

    private void HandlePurchaseCompleted()
    {
        if (IsTransitioning)
            return;

        StartCoroutine(TransitionToNextCycle());
    }

    private IEnumerator TransitionToNextCycle()
    {
        IsTransitioning = true;

        if (shoppingListManager == null)
        {
            Debug.LogError(
                "[CYCLE] ShoppingListManager reference missing.",
                this
            );

            IsTransitioning = false;
            checkoutProcessor.FinishProcessing();
            yield break;
        }

        if (shelfSpawners.Count == 0)
        {
            Debug.LogError(
                "[CYCLE] No shelf spawners assigned.",
                this
            );

            IsTransitioning = false;
            checkoutProcessor.FinishProcessing();
            yield break;
        }

        int completedCycle = CurrentCycle;

        if (showDebug)
        {
            Debug.Log(
                $"[CYCLE] Preparing next cycle after {completedCycle}.",
                this
            );
        }

        // Regeneramos los productos de todas las estanterías.
        foreach (ShelfProductSpawner spawner in shelfSpawners)
        {
            if (spawner == null)
                continue;

            spawner.RegenerateProducts();
        }

        // Permitimos que Unity termine de destruir
        // los productos del ciclo anterior.
        yield return null;

        // Generamos la nueva lista usando los productos actuales.
        bool generated =
            shoppingListManager.ResetAndGenerateShoppingList();

        if (!generated)
        {
            Debug.LogError(
                "[CYCLE] Could not generate the next shopping list.",
                this
            );

            IsTransitioning = false;
            yield break;
        }

        OnCycleCompleted?.Invoke(completedCycle);

        CurrentCycle++;

        if (showDebug)
        {
            Debug.Log(
                $"[CYCLE] Started cycle {CurrentCycle}.",
                this
            );
        }

        IsTransitioning = false;
        
        checkoutProcessor.FinishProcessing();

        OnCycleStarted?.Invoke(CurrentCycle);
    }

    
    private void HandlePurchaseIncomplete()
    {
        if (IsTransitioning)
            return;

        if (showDebug)
        {
            Debug.Log(
                $"[CYCLE] Incomplete checkout. " +
                $"Remaining in cycle {CurrentCycle}.",
                this
            );
        }

        if (stockReplenisher == null)
        {
            Debug.LogError(
                "[CYCLE] ShoppingStockReplenisher reference missing.",
                this
            );
        }
        else
        {
            bool stockAvailable =
                stockReplenisher.EnsureRemainingProductsAvailable();

            if (!stockAvailable)
            {
                Debug.LogWarning(
                    "[CYCLE] Some shopping objectives " +
                    "cannot currently be completed.",
                    this
                );
            }
        }

        OnIncompleteCheckout?.Invoke();
    }

}
