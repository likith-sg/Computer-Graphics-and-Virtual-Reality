using System;
using System.Collections.Generic;
using UnityEngine;

namespace InteriorStudio.Core
{
    [Serializable]
    public class PlacedFurnitureSaveData
    {
        public string instanceId;
        public string furnitureId;
        public Vector3 position;
        public Vector3 rotationEuler;
        public Vector3 scale;
        public string appliedMaterialPresetId;
    }

    [Serializable]
    public class RoomLayoutSaveData
    {
        public string projectName = "Virtual Interior Design Studio";
        public string timestamp;
        public int activeLightingPresetIndex;
        public List<PlacedFurnitureSaveData> placedItems = new List<PlacedFurnitureSaveData>();
    }
}
