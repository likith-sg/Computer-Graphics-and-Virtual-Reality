using System;
using UnityEngine;

namespace InteriorStudio.Core
{
    public enum MaterialType
    {
        Velvet,
        Leather,
        Wood,
        Marble,
        Fabric,
        Metal
    }

    [System.Serializable]
    public class MaterialPreset
    {
        public string id;
        public string displayName;
        public MaterialType materialType;
        public Color baseColor = Color.white;
        [Range(0f, 1f)] public float smoothness = 0.5f;
        [Range(0f, 1f)] public float metallic = 0.0f;
        public string textureName;
        public string normalMapName;
        public Vector2 tiling = Vector2.one;
        public string description;
    }
}
