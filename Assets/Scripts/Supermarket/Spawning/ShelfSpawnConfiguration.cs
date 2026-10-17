using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ShelfSpawnConfiguration
{
    [Header("Configuration")]
    public string sectionName;

    [Header("Spawn Areas")]
    public List<ShelfSpawnArea> spawnAreas = new();

    [Header("Available Products")]
    public List<ProductDefinition> products = new();

    [Header("Product Count")]
    [Min(0)] public int minProducts = 8;
    [Min(0)] public int maxProducts = 15;
}
