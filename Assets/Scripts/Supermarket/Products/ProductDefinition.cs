using UnityEngine;

[CreateAssetMenu(
    fileName = "NewProduct",
    menuName = "Market List/Product"
    )]
public class ProductDefinition : ScriptableObject
{
    public string productName;
    public GameObject prefab;

    [Header("Placement")]
    [Min(0.01f)] public float width = 0.2f;
    [Min(0.01f)] public float depth = 0.2f;
    
    [Tooltip("Distance from the prefab pivot to its bottom.")]
    public float bottomOffSet = 0f;
    
}
