using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private ShoppingListManager shoppingListManager;

    [Header("Debug")]
    [SerializeField]
    private bool showDebug = true;

    public GameState CurrentState { get; private set; }
        = GameState.Shopping;

    public event Action<GameState> OnGameStateChanged;

    private void OnEnable()
    {
        if (shoppingListManager != null)
        {
            shoppingListManager.OnListCompleted +=
                HandleShoppingCompleted;
        }
    }

    private void OnDisable()
    {
        if (shoppingListManager != null)
        {
            shoppingListManager.OnListCompleted -=
                HandleShoppingCompleted;
        }
    }

    private void HandleShoppingCompleted()
    {
        ChangeState(GameState.ExitUnlocked);
    }

    public void CompleteGame()
    {
        if (CurrentState != GameState.ExitUnlocked)
            return;

        ChangeState(GameState.Completed);
    }

    private void ChangeState(GameState newState)
    {
        if (CurrentState == newState)
            return;

        CurrentState = newState;

        OnGameStateChanged?.Invoke(newState);

        if (showDebug)
        {
            Debug.Log(
                $"[GAME] State changed: {newState}",
                this
            );
        }
    }
}