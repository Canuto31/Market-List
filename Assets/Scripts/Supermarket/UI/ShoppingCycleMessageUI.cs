
using UnityEngine;

public class ShoppingCycleMessageUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private ShoppingCycleManager cycleManager;

    [Header("Message Settings")]
    [SerializeField, Min(0.1f)]
    private float messageDuration = 3.5f;

    [SerializeField]
    private bool showMessages = true;

    private string currentMessage;
    private float messageEndTime;
    private GUIStyle messageStyle;

    private void OnEnable()
    {
        if (cycleManager == null)
        {
            Debug.LogError(
                "[CYCLE UI] ShoppingCycleManager reference missing.",
                this
            );
            return;
        }

        cycleManager.OnCycleStarted += HandleCycleStarted;
        cycleManager.OnIncompleteCheckout += HandleIncompleteCheckout;
    }

    private void OnDisable()
    {
        if (cycleManager == null)
            return;

        cycleManager.OnCycleStarted -= HandleCycleStarted;
        cycleManager.OnIncompleteCheckout -= HandleIncompleteCheckout;
    }

    private void HandleCycleStarted(int cycle)
    {
        if (cycle <= 1)
            return;

        string message = cycle switch
        {
            2 => "Creo que me falta algo...",
            3 => "¿No había comprado esto antes?",
            _ => "Esto ya lo he vivido..."
        };

        ShowMessage(message);
    }

    private void HandleIncompleteCheckout()
    {
        ShowMessage("Creo que todavía me falta algo...");
    }

    private void ShowMessage(string message)
    {
        currentMessage = message;
        messageEndTime = Time.unscaledTime + messageDuration;
    }

    private void OnGUI()
    {
        if (!showMessages ||
            string.IsNullOrEmpty(currentMessage) ||
            Time.unscaledTime >= messageEndTime)
        {
            return;
        }

        if (messageStyle == null)
        {
            messageStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Italic,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };

            messageStyle.normal.textColor = Color.white;
        }

        float width = Mathf.Min(650f, Screen.width - 40f);
        float height = 90f;

        Rect messageRect = new Rect(
            (Screen.width - width) * 0.5f,
            Screen.height * 0.72f,
            width,
            height
        );

        GUI.Label(messageRect, currentMessage, messageStyle);
    }
}
