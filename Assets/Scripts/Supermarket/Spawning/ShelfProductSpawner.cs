
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System;

public class ShelfProductSpawner : MonoBehaviour
{
    [Header("Shelf Configurations")]
    [SerializeField]
    private List<ShelfSpawnConfiguration> configurations = new();

    [Header("Distribution")]
    [SerializeField, Min(0f)]
    private float spacing = 0.02f;

    [SerializeField, Min(1)]
    private int maxAttemptsPerProduct = 100;

    [Header("Adaptive Search")]
    [SerializeField]
    private bool useAdaptiveSearch = true;

    [SerializeField, Min(0.01f)]
    private float candidateStep = 0.04f;

    [Header("Generation")]
    [SerializeField]
    private bool generateOnStart = true;

    private readonly List<GameObject> spawnedProducts = new();
    public IReadOnlyList<GameObject> SpawnedProducts => spawnedProducts;
    public event Action<ShelfProductSpawner> OnGenerationCompleted;

    private readonly List<OccupiedSpot> occupiedSpots = new();

    private struct OccupiedSpot
    {
        public ShelfSpawnArea Area;
        public Vector3 Position;
        public float Width;
        public float Depth;
        public GameObject ProductObject;
    }

    private void Start()
    {
        if (generateOnStart)
            GenerateProducts();
    }

    [ContextMenu("Generate Products")]
    public void GenerateProducts()
    {
        ClearProducts();

        foreach (ShelfSpawnConfiguration configuration
                 in configurations)
        {
            if (configuration == null)
                continue;

            GenerateSection(configuration);
        }

        OnGenerationCompleted?.Invoke(this);
    }
    
    
    public void RegenerateProducts()
    {
        GenerateProducts();
    }

    
    public int ReplenishProduct(
        ProductDefinition product,
        int requestedQuantity)
    {
        if (product == null || requestedQuantity <= 0)
            return 0;

        int generated = 0;

        foreach (ShelfSpawnConfiguration configuration in configurations)
        {
            if (configuration == null ||
                configuration.products == null ||
                configuration.spawnAreas == null ||
                !configuration.products.Contains(product))
            {
                continue;
            }
            
            CleanupOccupiedSpots();

            while (generated < requestedQuantity)
            {
                if (!TrySpawnSpecificProduct(configuration, product))
                    break;

                generated++;
            }

            if (generated >= requestedQuantity)
                break;
        }

        Debug.Log(
            $"[SHELF] Replenished {generated}/{requestedQuantity} " +
            $"units of {product.name}.",
            this
        );

        return generated;
    }
    
    
    private bool TrySpawnSpecificProduct(
        ShelfSpawnConfiguration configuration,
        ProductDefinition product)
    {
        if (configuration.spawnAreas.Count == 0 ||
            product == null ||
            product.prefab == null)
        {
            return false;
        }

        // Fase 1: búsqueda aleatoria.
        for (int attempt = 0;
             attempt < maxAttemptsPerProduct;
             attempt++)
        {
            ShelfSpawnArea area = GetRandomArea(configuration);

            if (TryPlaceProduct(area, product, null))
                return true;
        }

        if (!useAdaptiveSearch)
            return false;

        // Fase 2: búsqueda adaptativa.
        List<ShelfSpawnArea> areas =
            new(configuration.spawnAreas);

        Shuffle(areas);

        foreach (ShelfSpawnArea area in areas)
        {
            if (area == null)
                continue;

            List<Vector3> candidates =
                area.GetCandidatePositions(
                    product.width,
                    product.depth,
                    candidateStep
                );

            Shuffle(candidates);

            foreach (Vector3 position in candidates)
            {
                if (TryPlaceProduct(area, product, position))
                    return true;
            }
        }

        return false;
    }



    private void GenerateSection(
        ShelfSpawnConfiguration configuration)
    {
        if (configuration.spawnAreas == null ||
            configuration.products == null ||
            configuration.spawnAreas.Count == 0 ||
            configuration.products.Count == 0)
        {
            Debug.LogWarning(
                $"{name}: Invalid configuration " +
                $"{configuration.sectionName}",
                this
            );
            return;
        }

        int minimum = Mathf.Min(
            configuration.minProducts,
            configuration.maxProducts
        );

        int maximum = Mathf.Max(
            configuration.minProducts,
            configuration.maxProducts
        );

        int targetCount = Random.Range(
            Mathf.Max(0, minimum),
            Mathf.Max(0, maximum) + 1
        );

        int generatedCount = 0;

        for (int i = 0; i < targetCount; i++)
        {
            if (TrySpawnProduct(configuration))
                generatedCount++;
        }

        Debug.Log(
            $"{name} [{configuration.sectionName}]: " +
            $"Generated {generatedCount}/{targetCount} products.",
            this
        );
    }

    private bool TrySpawnProduct(
        ShelfSpawnConfiguration configuration)
    {
        // Primera fase: búsqueda completamente aleatoria.
        for (int attempt = 0;
             attempt < maxAttemptsPerProduct;
             attempt++)
        {
            ShelfSpawnArea area = GetRandomArea(configuration);
            ProductDefinition product = GetRandomProduct(configuration);

            if (TryPlaceProduct(area, product, null))
                return true;
        }

        if (!useAdaptiveSearch)
            return false;

        // Segunda fase: exploración de espacios disponibles.
        List<ShelfSpawnArea> areas =
            new(configuration.spawnAreas);

        Shuffle(areas);

        List<ProductDefinition> products =
            new(configuration.products);

        Shuffle(products);

        foreach (ShelfSpawnArea area in areas)
        {
            if (area == null)
                continue;

            foreach (ProductDefinition product in products)
            {
                if (product == null || product.prefab == null)
                    continue;

                List<Vector3> candidates =
                    area.GetCandidatePositions(
                        product.width,
                        product.depth,
                        candidateStep
                    );

                Shuffle(candidates);

                foreach (Vector3 position in candidates)
                {
                    if (TryPlaceProduct(area, product, position))
                        return true;
                }
            }
        }

        return false;
    }

    private bool TryPlaceProduct(
        ShelfSpawnArea area,
        ProductDefinition product,
        Vector3? candidatePosition)
    {
        if (area == null ||
            product == null ||
            product.prefab == null)
            return false;

        Vector3 position;

        if (candidatePosition.HasValue)
        {
            position = candidatePosition.Value;
        }
        else
        {
            if (!area.TryGetRandomPosition(
                    product.width,
                    product.depth,
                    out position))
                return false;
        }

        if (IsOccupied(area, position, product))
            return false;

        Quaternion rotation = area.GetSpawnRotation();

        Vector3 spawnPosition =
            position +
            rotation * Vector3.up * product.bottomOffSet;

        GameObject instance = Instantiate(
            product.prefab,
            spawnPosition,
            rotation,
            transform
        );
        
        // Agregar el componente de interacción.
        ProductInstance productInstance = instance.GetComponent<ProductInstance>();

        if (productInstance == null)
        {
            productInstance = instance.AddComponent<ProductInstance>();
        }

        // Inicialmente todos los productos
        // estarán disponibles.
        productInstance.Initialize(product, productState.Available);

        spawnedProducts.Add(instance);

        occupiedSpots.Add(new OccupiedSpot
        {
            Area = area,
            Position = position,
            Width = product.width,
            Depth = product.depth,
            ProductObject = instance
        });

        return true;
    }
    
    
    private void CleanupOccupiedSpots()
    {
        occupiedSpots.RemoveAll(spot =>
            spot.ProductObject == null ||
            !spot.ProductObject.activeInHierarchy
        );
    }


    private bool IsOccupied(
        ShelfSpawnArea area,
        Vector3 position,
        ProductDefinition product)
    {
        Vector3 localPosition =
            area.transform.InverseTransformPoint(position);

        foreach (OccupiedSpot spot in occupiedSpots)
        {
            if (spot.Area != area)
                continue;

            Vector3 other =
                area.transform.InverseTransformPoint(spot.Position);

            float requiredX =
                (spot.Width + product.width) * 0.5f + spacing;

            float requiredZ =
                (spot.Depth + product.depth) * 0.5f + spacing;

            if (Mathf.Abs(localPosition.x - other.x) < requiredX &&
                Mathf.Abs(localPosition.z - other.z) < requiredZ)
            {
                return true;
            }
        }

        return false;
    }

    private ShelfSpawnArea GetRandomArea(
        ShelfSpawnConfiguration configuration)
    {
        return configuration.spawnAreas[
            Random.Range(0, configuration.spawnAreas.Count)
        ];
    }

    private ProductDefinition GetRandomProduct(
        ShelfSpawnConfiguration configuration)
    {
        return configuration.products[
            Random.Range(0, configuration.products.Count)
        ];
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            (list[i], list[randomIndex]) =
                (list[randomIndex], list[i]);
        }
    }

    [ContextMenu("Clear Products")]
    public void ClearProducts()
    {
        foreach (GameObject product in spawnedProducts)
        {
            if (product == null)
                continue;

            if (Application.isPlaying)
                Destroy(product);
            else
                DestroyImmediate(product);
        }

        spawnedProducts.Clear();
        occupiedSpots.Clear();
    }
    
    
    public int RemoveAvailableProducts(
        ProductDefinition product,
        int maxToRemove)
    {
        if (product == null || maxToRemove <= 0)
            return 0;

        int removed = 0;

        for (int i = spawnedProducts.Count - 1; i >= 0; i--)
        {
            GameObject instance = spawnedProducts[i];

            if (instance == null || !instance.activeInHierarchy)
                continue;

            ProductInstance productInstance =
                instance.GetComponent<ProductInstance>();

            if (productInstance == null ||
                productInstance.Definition != product ||
                productInstance.CurrentState != productState.Available)
            {
                continue;
            }

            instance.SetActive(false);
            removed++;

            if (removed >= maxToRemove)
                break;
        }

        CleanupOccupiedSpots();

        return removed;
    }

}
