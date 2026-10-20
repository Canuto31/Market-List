using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField]
    private bool showDebug = true;

    public GameState CurrentState { get; private set; }
        = GameState.Shopping;

    public event Action<GameState> OnGameStateChanged;

    public void UnlockExit()
    {
        if (CurrentState != GameState.Shopping)
            return;

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