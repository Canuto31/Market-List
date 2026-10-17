
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(BoxCollider))]
public class ShelfSpawnArea : MonoBehaviour
{
    [Header("Product Orientation")]
    [SerializeField, Range(0f, 360f)]
    private float productRotationY = 0f;

    private BoxCollider spawnCollider;

    private void Awake()
    {
        spawnCollider = GetComponent<BoxCollider>();
    }

    private BoxCollider GetSpawnCollider()
    {
        if (spawnCollider == null)
            spawnCollider = GetComponent<BoxCollider>();

        return spawnCollider;
    }

    public bool TryGetRandomPosition(
        float productWidth,
        float productDepth,
        out Vector3 position)
    {
        BoxCollider box = GetSpawnCollider();

        Vector3 size = box.size;
        Vector3 center = box.center;

        float halfWidth = productWidth * 0.5f;
        float halfDepth = productDepth * 0.5f;

        float minX = center.x - size.x * 0.5f + halfWidth;
        float maxX = center.x + size.x * 0.5f - halfWidth;

        float minZ = center.z - size.z * 0.5f + halfDepth;
        float maxZ = center.z + size.z * 0.5f - halfDepth;

        if (minX > maxX || minZ > maxZ)
        {
            position = default;
            return false;
        }

        Vector3 localPosition = new Vector3(
            Random.Range(minX, maxX),
            center.y + size.y * 0.5f,
            Random.Range(minZ, maxZ)
        );

        position = transform.TransformPoint(localPosition);
        return true;
    }

    public List<Vector3> GetCandidatePositions(
        float productWidth,
        float productDepth,
        float step)
    {
        List<Vector3> candidates = new();

        BoxCollider box = GetSpawnCollider();

        Vector3 size = box.size;
        Vector3 center = box.center;

        float minX = center.x - size.x * 0.5f
                     + productWidth * 0.5f;

        float maxX = center.x + size.x * 0.5f
                     - productWidth * 0.5f;

        float minZ = center.z - size.z * 0.5f
                     + productDepth * 0.5f;

        float maxZ = center.z + size.z * 0.5f
                     - productDepth * 0.5f;

        if (minX > maxX || minZ > maxZ)
            return candidates;

        step = Mathf.Max(0.01f, step);

        // Desplazamiento aleatorio para evitar patrones fijos.
        float offsetX = Random.Range(0f, step);
        float offsetZ = Random.Range(0f, step);

        for (float x = minX + offsetX; x <= maxX; x += step)
        {
            for (float z = minZ + offsetZ; z <= maxZ; z += step)
            {
                Vector3 localPosition = new Vector3(
                    x,
                    center.y + size.y * 0.5f,
                    z
                );

                candidates.Add(
                    transform.TransformPoint(localPosition)
                );
            }
        }

        // Incluir esquinas válidas para no perder
        // espacios por el desplazamiento aleatorio.
        float y = center.y + size.y * 0.5f;

        candidates.Add(transform.TransformPoint(
            new Vector3(minX, y, minZ)));

        candidates.Add(transform.TransformPoint(
            new Vector3(maxX, y, minZ)));

        candidates.Add(transform.TransformPoint(
            new Vector3(minX, y, maxZ)));

        candidates.Add(transform.TransformPoint(
            new Vector3(maxX, y, maxZ)));

        return candidates;
    }

    public Quaternion GetSpawnRotation()
    {
        return Quaternion.Euler(0f, productRotationY, 0f);
    }

    [ContextMenu("Show Spawn Area Dimensions")]
    private void ShowSpawnAreaDimensions()
    {
        BoxCollider box = GetSpawnCollider();

        Debug.Log(
            $"SPAWN AREA: {name}\n" +
            $"Local Rotation: {transform.localEulerAngles}\n" +
            $"World Rotation: {transform.eulerAngles}\n" +
            $"Local Size: {box.size}\n" +
            $"World X Axis: {transform.right}\n" +
            $"World Y Axis: {transform.up}\n" +
            $"World Z Axis: {transform.forward}\n" +
            $"World Scale: {transform.lossyScale}",
            this
        );
    }
    
    [ContextMenu("Debug Spawn Area")]
    private void DebugSpawnArea()
    {
        BoxCollider box = GetComponent<BoxCollider>();

        Vector3 worldSize = Vector3.Scale(
            box.size,
            transform.lossyScale
        );

        Debug.Log(
            $"===== {name} =====\n" +
            $"Parent: {transform.parent.name}\n" +
            $"Local Rotation: {transform.localEulerAngles}\n" +
            $"World Rotation: {transform.eulerAngles}\n" +
            $"Local Scale: {transform.localScale}\n" +
            $"Lossy Scale: {transform.lossyScale}\n" +
            $"Collider Local Size: {box.size}\n" +
            $"Collider Approx. World Size: {worldSize}\n" +
            $"World Up: {transform.up}\n" +
            $"World Right: {transform.right}\n" +
            $"World Forward: {transform.forward}",
            this
        );
    }
}
