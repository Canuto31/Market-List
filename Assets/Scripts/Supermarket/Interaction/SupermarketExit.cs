using UnityEngine;

public class SupermarketExit : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;

    public string InteractionPrompt
    {
        get
        {
            if (gameManager == null)
                return string.Empty;

            return gameManager.CurrentState switch
            {
                GameState.Shopping =>
                    "[E] Revisar salida",

                GameState.ExitUnlocked =>
                    "[E] Salir del supermercado",

                _ => string.Empty
            };
        }
    }

    public bool CanInteract =>
        gameManager != null &&
        gameManager.CurrentState != GameState.Completed;

    public void Interact()
    {
        if (gameManager == null)
            return;

        switch (gameManager.CurrentState)
        {
            case GameState.Shopping:
                Debug.Log(
                    "[EXIT] Todavía tienes productos pendientes.",
                    this
                );
                break;

            case GameState.ExitUnlocked:
                gameManager.CompleteGame();
                break;
        }
    }
}