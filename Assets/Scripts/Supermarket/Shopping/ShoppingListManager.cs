
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShoppingListManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private List<ShelfProductSpawner> shelfSpawners = new();

    [SerializeField]
    private ShoppingCartInventory inventory;

    [Header("List Generation")]
    [SerializeField, Min(1)]
    private int maxProductTypes = 4;

    [SerializeField, Min(1)]
    private int maxQuantityPerProduct = 5;

    [Header("Debug")]
    [SerializeField]
    private bool showDebug = true;

    private readonly List<ShoppingListEntry> entries = new();

    public IReadOnlyList<ShoppingListEntry> Entries => entries;

    public event Action OnListChanged;
    public event Action OnListCompleted;
    

    private bool listGenerated;
    private bool listCompleted;
    public bool IsListCompleted => listGenerated && listCompleted;
    public bool HasActiveList => listGenerated && entries.Count > 0;
    
    [Header("Completion Debug")]
    [SerializeField] private bool showCompletionMessage = true;

    [SerializeField, Min(0.1f)]
    private float completionMessageDuration = 2f;

    private float completionMessageEndTime;
    private GUIStyle completionStyle;
    
    [Header("Shopping List Debug UI")]
    [SerializeField] private bool showShoppingList = true;

    [SerializeField] private float listWidth = 280f;
    [SerializeField] private float listMargin = 20f;
    [SerializeField] private float listRowHeight = 30f;

    private GUIStyle listTitleStyle;
    private GUIStyle listItemStyle;

    private IEnumerator Start()
    {
        if (inventory == null)
        {
            Debug.LogError(
                "[SHOPPING LIST] Inventory reference missing.",
                this
            );
            yield break;
        }

        inventory.OnProductAdded += HandleProductAdded;

        // Espera a que los Start() de los spawners
        // activos hayan tenido oportunidad de ejecutarse.
        yield return null;

        GenerateShoppingList();
    }

    private void OnDestroy()
    {
        if (inventory != null)
            inventory.OnProductAdded -= HandleProductAdded;
    }

    public void GenerateShoppingList()
    {
        if (listGenerated)
            return;

        Dictionary<ProductDefinition, int> available =
            CountAvailableProducts();

        List<ProductDefinition> candidates =
            new(available.Keys);

        Shuffle(candidates);

        entries.Clear();

        
        int remainingCapacity = inventory.MaxCapacity;

        int selectedTypes = Mathf.Min(
            maxProductTypes,
            candidates.Count,
            remainingCapacity
        );

        for (int i = 0; i < selectedTypes; i++)
        {
            ProductDefinition product = candidates[i];

            // Reservamos un espacio para cada tipo de producto pendiente.
            int reservedForOtherTypes = selectedTypes - i - 1;

            int maximumQuantity = Mathf.Min(
                maxQuantityPerProduct,
                available[product],
                remainingCapacity - reservedForOtherTypes
            );

            if (maximumQuantity < 1)
                continue;

            int requestedQuantity = Random.Range(
                1,
                maximumQuantity + 1
            );

            ShoppingListEntry entry = new(
                product,
                requestedQuantity
            );

            entries.Add(entry);

            remainingCapacity -= requestedQuantity;
        }


        listGenerated = true;
        listCompleted = false;

        // Si el inventario ya tiene productos,
        // sincronizamos las cantidades recogidas.
        foreach (ShoppingListEntry entry in entries)
        {
            int alreadyCollected =
                inventory.GetQuantity(entry.Product);

            entry.TryAddCollected(alreadyCollected);
        }

        OnListChanged?.Invoke();

        if (showDebug)
            PrintShoppingList();

        CheckCompletion();
    }

    private Dictionary<ProductDefinition, int>
        CountAvailableProducts()
    {
        Dictionary<ProductDefinition, int> result = new();

        foreach (ShelfProductSpawner spawner in shelfSpawners)
        {
            if (spawner == null)
                continue;

            foreach (GameObject obj in spawner.SpawnedProducts)
            {
                if (obj == null || !obj.activeInHierarchy)
                    continue;

                ProductInstance instance =
                    obj.GetComponent<ProductInstance>();

                if (instance == null ||
                    instance.Definition == null ||
                    instance.CurrentState != productState.Available)
                {
                    continue;
                }

                ProductDefinition definition =
                    instance.Definition;

                if (!result.ContainsKey(definition))
                    result[definition] = 0;

                result[definition]++;
            }
        }

        return result;
    }

    private void HandleProductAdded(
        ProductDefinition product,
        int totalQuantity)
    {
        if (!listGenerated)
            return;

        foreach (ShoppingListEntry entry in entries)
        {
            if (entry.Product != product)
                continue;

            if (!entry.TryAddCollected())
                return;

            if (showDebug)
            {
                Debug.Log(
                    $"[SHOPPING LIST] {product.name}: " +
                    $"{entry.CollectedQuantity}/" +
                    $"{entry.RequiredQuantity}",
                    this
                );
            }

            OnListChanged?.Invoke();
            CheckCompletion();

            return;
        }
    }

    private void CheckCompletion()
    {
        if (!listGenerated ||
            listCompleted ||
            entries.Count == 0)
            return;

        foreach (ShoppingListEntry entry in entries)
        {
            if (!entry.IsCompleted)
                return;
        }

        listCompleted = true;
        
        completionMessageEndTime =
            Time.unscaledTime + completionMessageDuration;

        Debug.Log(
            "[SHOPPING LIST] All products collected!",
            this
        );

        OnListCompleted?.Invoke();
    }

    private void PrintShoppingList()
    {
        Debug.Log(
            $"[SHOPPING LIST] Generated {entries.Count} objectives.",
            this
        );

        foreach (ShoppingListEntry entry in entries)
        {
            Debug.Log(
                $"[SHOPPING LIST] {entry.Product.name}: " +
                $"{entry.CollectedQuantity}/" +
                $"{entry.RequiredQuantity}",
                this
            );
        }
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int index = Random.Range(0, i + 1);

            (list[i], list[index]) =
                (list[index], list[i]);
        }
    }
    
    private void OnGUI()
    {
        DrawShoppingList();
        
        if (!showCompletionMessage ||
            !listCompleted ||
            Time.unscaledTime >= completionMessageEndTime)
        {
            return;
        }

        if (completionStyle == null)
        {
            completionStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 30,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };

            completionStyle.normal.textColor = Color.green;
        }

        float width = 500f;
        float height = 100f;

        Rect messageRect = new Rect(
            (Screen.width - width) / 2f,
            (Screen.height - height) / 2f,
            width,
            height
        );

        GUI.Box(
            messageRect,
            "¡LISTA DE COMPRAS COMPLETADA!",
            completionStyle
        );
    }
    
    private void DrawShoppingList()
    {
        if (!showShoppingList || !listGenerated)
            return;

        if (listTitleStyle == null)
        {
            listTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold
            };

            listTitleStyle.normal.textColor = Color.white;

            listItemStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16
            };

            listItemStyle.normal.textColor = Color.white;
        }

        int completed = 0;

        foreach (ShoppingListEntry entry in entries)
        {
            if (entry.IsCompleted)
                completed++;
        }

        float height = 55f + entries.Count * listRowHeight;

        Rect panelRect = new Rect(
            Screen.width - listWidth - listMargin,
            listMargin,
            listWidth,
            height
        );

        GUI.Box(panelRect, "");

        GUI.Label(
            new Rect(
                panelRect.x + 10f,
                panelRect.y + 10f,
                listWidth - 20f,
                30f
            ),
            $"LISTA DE COMPRAS ({completed}/{entries.Count})",
            listTitleStyle
        );

        for (int i = 0; i < entries.Count; i++)
        {
            ShoppingListEntry entry = entries[i];

            string marker = entry.IsCompleted ? "[X]" : "[ ]";

            string productName = entry.Product != null
                ? entry.Product.name
                : "Producto";

            string label =
                $"{marker} {productName} " +
                $"{entry.CollectedQuantity}/{entry.RequiredQuantity}";

            GUI.Label(
                new Rect(
                    panelRect.x + 10f,
                    panelRect.y + 45f + i * listRowHeight,
                    listWidth - 20f,
                    listRowHeight
                ),
                label,
                listItemStyle
            );
        }
    }
}
