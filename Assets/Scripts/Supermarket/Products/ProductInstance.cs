using System;
using UnityEngine;

public class ProductInstance : MonoBehaviour
{
    [Header("Product Information")]
    [SerializeField] private ProductDefinition definition;

    [Header("Product State")]
    [SerializeField] private productState currentState = productState.Available;

    public ProductDefinition Definition => definition;
    public productState CurrentState => currentState;

    public event Action<ProductInstance> OnCollected;
    public event Action<ProductInstance> OnUnlocked;
    public event Action<ProductInstance> OnBlockedInteraction;

    public void Initialize(ProductDefinition product, productState initialState)
    {
        definition = product;
        currentState = initialState;
    }

    public bool TryCollect()
    {
        if (currentState == productState.Blocked)
        {
            Debug.Log("Qué raro, parece que está pegado...", this);
            OnBlockedInteraction?.Invoke(this);
            return false;
        }

        if (currentState != productState.Available)
            return false;

        currentState = productState.Collected;

        Debug.Log(
            $"[PRODUCT] Recogiendo {name} | ID: {GetInstanceID()}",
            this
        );

        OnCollected?.Invoke(this);

        gameObject.SetActive(false);

        Debug.Log(
            $"[PRODUCT] {name} desactivado correctamente. " +
            $"ActiveSelf: {gameObject.activeSelf}"
        );

        return true;
    }

    public bool Unlock()
    {
        if (currentState != productState.Blocked)
            return false;

        currentState = productState.Available;

        OnUnlocked?.Invoke(this);

        return true;
    }
}