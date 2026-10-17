using TMPro;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text interactionPrompt;

    private void Awake()
    {
        HideInteraction();
    }

    public void ShowInteraction(string message = "[E]")
    {
        if (interactionPrompt == null)
        {
            Debug.LogError(
                "[UI] InteractionPrompt no está asignado."
            );
            return;
        }

        interactionPrompt.text = message;
        interactionPrompt.gameObject.SetActive(true);

        Debug.Log(
            $"[UI] Mostrando: {message} | " +
            $"Activo: {interactionPrompt.gameObject.activeInHierarchy}"
        );
    }

    public void HideInteraction()
    {
        if (interactionPrompt == null)
            return;

        interactionPrompt.gameObject.SetActive(false);
    }
}