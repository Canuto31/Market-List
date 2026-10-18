using UnityEngine;

public class ShoppingCartCapacityUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private ShoppingCartInventory inventory;

    [Header("UI Settings")]
    [SerializeField]
    private bool showCapacity = true;

    [SerializeField, Min(0.1f)]
    private float messageDuration = 2f;

    private float messageEndTime;

    private void Awake()
    {
        if (inventory == null)
            inventory = GetComponent<ShoppingCartInventory>();
    }

    private void OnEnable()
    {
        if (inventory != null)
            inventory.OnCartFull += ShowCartFullMessage;
    }

    private void OnDisable()
    {
        if (inventory != null)
            inventory.OnCartFull -= ShowCartFullMessage;
    }

    private void ShowCartFullMessage()
    {
        messageEndTime = Time.unscaledTime + messageDuration;
    }

    private void OnGUI()
    {
        if (inventory == null)
            return;

        if (showCapacity)
        {
            GUI.Box(
                new Rect(20, 20, 180, 40),
                $"Carrito: {inventory.TotalProducts}/{inventory.MaxCapacity}"
            );
        }

        if (Time.unscaledTime < messageEndTime)
        {
            float width = 320f;
            float height = 55f;

            Rect messageRect = new Rect(
                (Screen.width - width) / 2f,
                Screen.height * 0.75f,
                width,
                height
            );

            GUI.Box(
                messageRect,
                $"El carrito está lleno ({inventory.MaxCapacity}/{inventory.MaxCapacity})"
            );
        }
    }
}