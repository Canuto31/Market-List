
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShoppingCartVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform productContainer;
    [SerializeField] private BoxCollider visualBounds;

    [Header("Visual Settings")]
    [SerializeField, Min(1)]
    private int maxVisibleProducts = 8;

    [SerializeField, Min(0f)]
    private float spacing = 0.02f;

    [SerializeField, Min(1)]
    private int placementAttempts = 100;

    [Header("Debug")]
    [SerializeField] private bool showDebug = false;

    private readonly List<GameObject> visualProducts = new();
    private readonly List<OccupiedSpace> occupiedSpaces = new();

    private struct OccupiedSpace
    {
        public Vector3 Position;
        public Vector3 Size;
    }

    public void AddVisualProduct(ProductDefinition product)
    {
        if (product == null || product.prefab == null ||
            productContainer == null || visualBounds == null)
        {
            Debug.LogWarning(
                "[CART VISUAL] Missing references.",
                this
            );
            return;
        }

        if (visualProducts.Count >= maxVisibleProducts)
            return;

        if (!TryFindPosition(product, out Vector3 position,
                out Quaternion rotation, out Vector3 size))
        {
            if (showDebug)
                Debug.Log(
                    "[CART VISUAL] No available space.",
                    this
                );

            return;
        }

        GameObject instance = Instantiate(
            product.prefab,
            productContainer
        );

        instance.transform.SetPositionAndRotation(
            position,
            rotation
        );

        // Los productos del carrito son decorativos.
        foreach (Collider col in
                 instance.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        foreach (ProductInstance component in
                 instance.GetComponentsInChildren<ProductInstance>())
        {
            component.enabled = false;
        }

        foreach (Rigidbody rb in
                 instance.GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        visualProducts.Add(instance);

        occupiedSpaces.Add(new OccupiedSpace
        {
            Position = visualBounds.transform
                .InverseTransformPoint(position),
            Size = size
        });

        if (showDebug)
            Debug.Log(
                $"[CART VISUAL] Added {product.name}",
                this
            );
    }

    private bool TryFindPosition(
        ProductDefinition product,
        out Vector3 position,
        out Quaternion rotation,
        out Vector3 occupiedSize)
    {
        position = default;
        rotation = Quaternion.identity;
        occupiedSize = default;

        Vector3 boundsSize = visualBounds.size;
        Vector3 center = visualBounds.center;

        // Dimensiones físicas expresadas en unidades
        // locales del volumen. Se presupone escala
        // uniforme y positiva en VisualBounds.
        Vector3 productSize = new Vector3(
            product.width,
            boundsSize.y,
            product.depth
        );

        for (int attempt = 0;
             attempt < placementAttempts;
             attempt++)
        {
            bool rotate90 = Random.value > 0.5f;

            float width = rotate90
                ? product.depth
                : product.width;

            float depth = rotate90
                ? product.width
                : product.depth;

            if (width > boundsSize.x ||
                depth > boundsSize.z)
                continue;

            float minX = center.x - boundsSize.x / 2f
                         + width / 2f;

            float maxX = center.x + boundsSize.x / 2f
                         - width / 2f;

            float minZ = center.z - boundsSize.z / 2f
                         + depth / 2f;

            float maxZ = center.z + boundsSize.z / 2f
                         - depth / 2f;

            Vector3 candidate = new Vector3(
                Random.Range(minX, maxX),
                center.y,
                Random.Range(minZ, maxZ)
            );

            Vector3 candidateSize = new Vector3(
                width,
                boundsSize.y,
                depth
            );

            if (IsOccupied(candidate, candidateSize))
                continue;

            position = visualBounds.transform
                .TransformPoint(candidate);

            rotation = visualBounds.transform.rotation *
                Quaternion.Euler(
                    0f,
                    rotate90 ? 90f : 0f,
                    0f
                );

            occupiedSize = candidateSize;
            return true;
        }

        return false;
    }

    private bool IsOccupied(
        Vector3 position,
        Vector3 size)
    {
        foreach (OccupiedSpace occupied in occupiedSpaces)
        {
            float requiredX =
                (occupied.Size.x + size.x) / 2f + spacing;

            float requiredZ =
                (occupied.Size.z + size.z) / 2f + spacing;

            if (Mathf.Abs(
                    position.x - occupied.Position.x) < requiredX &&
                Mathf.Abs(
                    position.z - occupied.Position.z) < requiredZ)
            {
                return true;
            }
        }

        return false;
    }

    public void ClearVisuals()
    {
        foreach (GameObject product in visualProducts)
        {
            if (product == null)
                continue;

            if (Application.isPlaying)
                Destroy(product);
            else
                DestroyImmediate(product);
        }

        visualProducts.Clear();
        occupiedSpaces.Clear();
    }
}
