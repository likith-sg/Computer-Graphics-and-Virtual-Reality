using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using InteriorStudio.Environment;
using InteriorStudio.Interaction;

namespace InteriorStudio.Core
{
    public class StudioManager : MonoBehaviour
    {
        public static StudioManager Instance { get; private set; }

        [Header("Room Components")]
        public RoomEnvironmentBuilder environmentBuilder;
        public LightingPresetManager lightingManager;
        public ScreenToWorldRaycaster raycaster;
        public FurnitureSelector selector;
        public FurniturePlacementController placementController;
        public CameraNavigation cameraNavigation;

        [Header("Catalogue Items")]
        public List<FurnitureItem> catalogue = new List<FurnitureItem>();

        [Header("Material Presets")]
        public List<MaterialPreset> materialPresets = new List<MaterialPreset>();

        [Header("Runtime State")]
        public List<GameObject> placedFurnitureObjects = new List<GameObject>();
        public GameObject selectedFurnitureObject;
        public int currentLightingPreset = 0;

        public event Action<GameObject> OnSelectionChanged;
        public event Action<int> OnLightingPresetChanged;
        public event Action OnRoomLayoutUpdated;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            InitializeDefaultCatalogue();
            InitializeDefaultMaterialPresets();
        }

        private void Start()
        {
            if (lightingManager != null)
            {
                lightingManager.ApplyPreset(currentLightingPreset);
            }
        }

        public void SelectFurniture(GameObject obj)
        {
            selectedFurnitureObject = obj;
            OnSelectionChanged?.Invoke(selectedFurnitureObject);
        }

        public void DeselectFurniture()
        {
            selectedFurnitureObject = null;
            OnSelectionChanged?.Invoke(null);
        }

        public void RegisterPlacedObject(GameObject obj)
        {
            if (!placedFurnitureObjects.Contains(obj))
            {
                placedFurnitureObjects.Add(obj);
                OnRoomLayoutUpdated?.Invoke();
            }
        }

        public void RemovePlacedObject(GameObject obj)
        {
            if (placedFurnitureObjects.Contains(obj))
            {
                placedFurnitureObjects.Remove(obj);
                if (selectedFurnitureObject == obj)
                {
                    DeselectFurniture();
                }
                Destroy(obj);
                OnRoomLayoutUpdated?.Invoke();
            }
        }

        public void SetLightingPreset(int index)
        {
            currentLightingPreset = Mathf.Clamp(index, 0, 2);
            if (lightingManager != null)
            {
                lightingManager.ApplyPreset(currentLightingPreset);
            }
            OnLightingPresetChanged?.Invoke(currentLightingPreset);
        }

        public void ApplyMaterialPresetToSelected(int presetIndex)
        {
            if (selectedFurnitureObject == null || presetIndex < 0 || presetIndex >= materialPresets.Count) return;

            var preset = materialPresets[presetIndex];
            var renderers = selectedFurnitureObject.GetComponentsInChildren<MeshRenderer>();
            foreach (var r in renderers)
            {
                // Apply to primary upholstered / surface slot (slot 0)
                if (r.materials.Length > 0)
                {
                    Material mat = r.materials[0];
                    mat.color = preset.baseColor;
                    if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", preset.smoothness);
                    if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", preset.metallic);
                }
            }
            Debug.Log($"[StudioManager] Applied material preset '{preset.displayName}' to '{selectedFurnitureObject.name}'");
        }

        public string SaveRoomLayoutToJson()
        {
            var data = new RoomLayoutSaveData
            {
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                activeLightingPresetIndex = currentLightingPreset
            };

            for (int i = 0; i < placedFurnitureObjects.Count; i++)
            {
                var obj = placedFurnitureObjects[i];
                if (obj == null) continue;

                var itemData = new PlacedFurnitureSaveData
                {
                    instanceId = $"Item_{i:D2}",
                    furnitureId = obj.name.Replace("(Clone)", "").Trim(),
                    position = obj.transform.position,
                    rotationEuler = obj.transform.eulerAngles,
                    scale = obj.transform.localScale,
                    appliedMaterialPresetId = "default"
                };
                data.placedItems.Add(itemData);
            }

            string json = JsonUtility.ToJson(data, true);
            string savePath = Path.Combine(Application.persistentDataPath, "room_layout.json");
            File.WriteAllText(savePath, json);
            Debug.Log($"[StudioManager] Room layout saved successfully to: {savePath}");
            return json;
        }

        public bool LoadRoomLayoutFromJson(string json = null)
        {
            string loadPath = Path.Combine(Application.persistentDataPath, "room_layout.json");
            if (string.IsNullOrEmpty(json))
            {
                if (!File.Exists(loadPath))
                {
                    Debug.LogWarning("[StudioManager] No saved layout found at: " + loadPath);
                    return false;
                }
                json = File.ReadAllText(loadPath);
            }

            var data = JsonUtility.FromJson<RoomLayoutSaveData>(json);
            if (data == null) return false;

            // Clear current objects
            foreach (var obj in placedFurnitureObjects)
            {
                if (obj != null) Destroy(obj);
            }
            placedFurnitureObjects.Clear();
            DeselectFurniture();

            // Set lighting
            SetLightingPreset(data.activeLightingPresetIndex);

            // Reconstruct placed objects
            foreach (var item in data.placedItems)
            {
                var catItem = catalogue.Find(c => c.id == item.furnitureId || c.displayName == item.furnitureId);
                if (catItem != null && catItem.prefab != null)
                {
                    var spawned = Instantiate(catItem.prefab, item.position, Quaternion.Euler(item.rotationEuler));
                    spawned.transform.localScale = item.scale;
                    spawned.name = catItem.displayName;
                    RegisterPlacedObject(spawned);
                }
            }

            Debug.Log($"[StudioManager] Reconstructed {data.placedItems.Count} furniture items from saved layout.");
            return true;
        }

        public void ResetRoomLayout()
        {
            foreach (var obj in placedFurnitureObjects)
            {
                if (obj != null) Destroy(obj);
            }
            placedFurnitureObjects.Clear();
            DeselectFurniture();
            SetLightingPreset(0);
            Debug.Log("[StudioManager] Room layout reset to blank state.");
        }

        private void InitializeDefaultCatalogue()
        {
            catalogue.Clear();

            catalogue.Add(new FurnitureItem
            {
                id = "chair_velvet",
                displayName = "Nordic Velvet Armchair",
                category = FurnitureCategory.Seating,
                description = "Ergonomic Scandinavian accent armchair in crimson red velvet with warm natural oak tapered legs.",
                defaultDimensions = new Vector3(0.85f, 0.95f, 0.85f),
                basePrice = 450f,
                polygonCount = 1840
            });

            catalogue.Add(new FurnitureItem
            {
                id = "sofa_leather",
                displayName = "Stockholm 3-Seater Leather Sofa",
                category = FurnitureCategory.Seating,
                description = "Modern low-profile Scandinavian sofa in saddle cognac leather, featuring detailed normal-mapped seams and grain.",
                defaultDimensions = new Vector3(2.20f, 0.85f, 0.95f),
                basePrice = 1250f,
                polygonCount = 2450
            });

            catalogue.Add(new FurnitureItem
            {
                id = "table_coffee",
                displayName = "Oslo Oak Coffee Table",
                category = FurnitureCategory.Tables,
                description = "Low Scandinavian solid oak coffee table with rounded pill silhouette and tapered tripod legs.",
                defaultDimensions = new Vector3(1.20f, 0.45f, 0.65f),
                basePrice = 320f,
                polygonCount = 920
            });

            catalogue.Add(new FurnitureItem
            {
                id = "table_side",
                displayName = "Bergen Pedestal Side Table",
                category = FurnitureCategory.Tables,
                description = "Minimalist circular side table with white Carrara marble top and black matte steel pedestal.",
                defaultDimensions = new Vector3(0.50f, 0.55f, 0.50f),
                basePrice = 180f,
                polygonCount = 640
            });

            catalogue.Add(new FurnitureItem
            {
                id = "lamp_floor",
                displayName = "Kobenhavn Arc Floor Lamp",
                category = FurnitureCategory.Lighting,
                description = "Contemporary curved brass floor lamp providing focused warm task lighting and ambient glow.",
                defaultDimensions = new Vector3(0.70f, 1.85f, 0.45f),
                basePrice = 210f,
                polygonCount = 1120
            });

            catalogue.Add(new FurnitureItem
            {
                id = "decor_plant",
                displayName = "Monstera in Ceramic Planter",
                category = FurnitureCategory.Decor,
                description = "Lush potted botanical Monstera deliciosa in a matte terracotta Scandinavian planter.",
                defaultDimensions = new Vector3(0.60f, 1.10f, 0.60f),
                basePrice = 95f,
                polygonCount = 1450
            });
        }

        private void InitializeDefaultMaterialPresets()
        {
            materialPresets.Clear();

            materialPresets.Add(new MaterialPreset
            {
                id = "mat_velvet_crimson",
                displayName = "Royal Crimson Velvet",
                materialType = MaterialType.Velvet,
                baseColor = new Color(0.65f, 0.08f, 0.15f),
                smoothness = 0.25f,
                metallic = 0.05f,
                textureName = "T_Velvet_Albedo",
                normalMapName = "T_Velvet_Normal",
                tiling = new Vector2(2f, 2f),
                description = "Rich velvet with soft backscattering and micro-pile sheen."
            });

            materialPresets.Add(new MaterialPreset
            {
                id = "mat_velvet_emerald",
                displayName = "Nordic Emerald Velvet",
                materialType = MaterialType.Velvet,
                baseColor = new Color(0.08f, 0.35f, 0.22f),
                smoothness = 0.28f,
                metallic = 0.05f,
                textureName = "T_Velvet_Albedo",
                normalMapName = "T_Velvet_Normal",
                tiling = new Vector2(2f, 2f),
                description = "Deep jewel-tone emerald green velvet upholstery."
            });

            materialPresets.Add(new MaterialPreset
            {
                id = "mat_leather_cognac",
                displayName = "Vintage Cognac Leather",
                materialType = MaterialType.Leather,
                baseColor = new Color(0.72f, 0.42f, 0.22f),
                smoothness = 0.58f,
                metallic = 0.10f,
                textureName = "T_Leather_Albedo",
                normalMapName = "T_Leather_Normal",
                tiling = new Vector2(3f, 3f),
                description = "Supple full-grain aniline leather with natural pebble grain and seam normals."
            });

            materialPresets.Add(new MaterialPreset
            {
                id = "mat_leather_noir",
                displayName = "Midnight Obsidian Leather",
                materialType = MaterialType.Leather,
                baseColor = new Color(0.12f, 0.12f, 0.14f),
                smoothness = 0.62f,
                metallic = 0.15f,
                textureName = "T_Leather_Albedo",
                normalMapName = "T_Leather_Normal",
                tiling = new Vector2(3f, 3f),
                description = "Sleek contemporary black matte leather with crisp specular highlights."
            });

            materialPresets.Add(new MaterialPreset
            {
                id = "mat_wood_oak",
                displayName = "Scandinavian Natural Oak",
                materialType = MaterialType.Wood,
                baseColor = new Color(0.85f, 0.72f, 0.55f),
                smoothness = 0.45f,
                metallic = 0.0f,
                textureName = "T_Wood_Albedo",
                normalMapName = "T_Wood_Normal",
                tiling = new Vector2(2f, 2f),
                description = "Light blonde solid oak with satin matte protective finish."
            });

            materialPresets.Add(new MaterialPreset
            {
                id = "mat_wood_walnut",
                displayName = "Dark Smoked Walnut",
                materialType = MaterialType.Wood,
                baseColor = new Color(0.35f, 0.24f, 0.18f),
                smoothness = 0.52f,
                metallic = 0.0f,
                textureName = "T_Wood_Albedo",
                normalMapName = "T_Wood_Normal",
                tiling = new Vector2(2f, 2f),
                description = "Rich architectural dark walnut with subtle grain depth."
            });

            materialPresets.Add(new MaterialPreset
            {
                id = "mat_marble_carrara",
                displayName = "Carrara White Marble",
                materialType = MaterialType.Marble,
                baseColor = new Color(0.95f, 0.95f, 0.96f),
                smoothness = 0.88f,
                metallic = 0.10f,
                textureName = "T_Marble_Albedo",
                normalMapName = "T_Marble_Normal",
                tiling = new Vector2(1.5f, 1.5f),
                description = "Polished Italian Carrara marble with elegant smoke grey veining."
            });

            materialPresets.Add(new MaterialPreset
            {
                id = "mat_linen_oat",
                displayName = "Natural Oat Linen",
                materialType = MaterialType.Fabric,
                baseColor = new Color(0.82f, 0.78f, 0.72f),
                smoothness = 0.20f,
                metallic = 0.0f,
                textureName = "T_Fabric_Albedo",
                normalMapName = "T_Fabric_Normal",
                tiling = new Vector2(4f, 4f),
                description = "Breathable textured Scandinavian woven flax linen."
            });
        }
    }
}
