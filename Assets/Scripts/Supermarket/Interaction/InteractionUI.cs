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

    
    private string currentMessage = string.Empty;

    public void ShowInteraction(string message)
    {
        if (interactionPrompt == null)
            return;

        bool isVisible = interactionPrompt.gameObject.activeSelf;

        // Si ya está visible y muestra el mismo mensaje,
        // no necesitamos actualizar nada.
        if (isVisible && currentMessage == message)
            return;

        currentMessage = message;

        interactionPrompt.text = message;
        interactionPrompt.gameObject.SetActive(true);

        Debug.Log($"[UI] Mostrando: {message}", this);
    }

    public void HideInteraction()
    {
        if (interactionPrompt == null)
            return;

        // Si ya está oculto, no repetimos la operación.
        if (!interactionPrompt.gameObject.activeSelf)
            return;

        interactionPrompt.gameObject.SetActive(false);
        currentMessage = string.Empty;

        Debug.Log("[UI] Interacción ocultada", this);
    }

}