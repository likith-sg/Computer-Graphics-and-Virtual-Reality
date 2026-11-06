using System;
using UnityEngine;

namespace InteriorStudio.Core
{
    public enum FurnitureCategory
    {
        Seating,
        Tables,
        Lighting,
        Decor
    }

    [System.Serializable]
    public class FurnitureItem
    {
        public string id;
        public string displayName;
        public FurnitureCategory category;
        public string description;
        public GameObject prefab;
        public Vector3 defaultDimensions;
        public float basePrice;
        public int polygonCount;
    }
}
