using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using InteriorStudio.Environment;
using InteriorStudio.Interaction;
using InteriorStudio.UI;

namespace InteriorStudio.Core
{
    [DefaultExecutionOrder(-100)]
    public class StudioBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnSceneLoaded()
        {
            if (FindObjectOfType<StudioManager>() == null)
            {
                Debug.Log("[StudioBootstrap] Initializing Studio Environment dynamically on scene load...");
                GameObject root = new GameObject("InteriorStudio_RuntimeRoot");
                root.AddComponent<StudioBootstrap>();
            }
        }

        private void Awake()
        {
            SetupCamera();
            SetupLighting();
            SetupEnvironment();
            SetupManagerAndUI();
            SpawnStarterFurniture();
        }

        private void SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                camObj.tag = "MainCamera";
                cam = camObj.AddComponent<Camera>();
                camObj.AddComponent<AudioListener>();
            }

            cam.transform.position = new Vector3(-2.8f, 1.8f, -2.1f);
            cam.transform.rotation = Quaternion.Euler(18f, 48f, 0f);
            cam.fieldOfView = 50f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;

            if (cam.GetComponent<CameraNavigation>() == null)
            {
                cam.gameObject.AddComponent<CameraNavigation>();
            }

            var raycaster = cam.GetComponent<ScreenToWorldRaycaster>();
            if (raycaster == null)
            {
                raycaster = cam.gameObject.AddComponent<ScreenToWorldRaycaster>();
                raycaster.targetCamera = cam;
            }
        }

        private void SetupLighting()
        {
            var existingLight = FindObjectOfType<Light>();
            if (existingLight != null && existingLight.type == LightType.Directional)
            {
                existingLight.color = new Color(1.0f, 0.95f, 0.88f);
                existingLight.intensity = 1.35f;
                existingLight.shadows = LightShadows.Soft;
                existingLight.transform.rotation = Quaternion.Euler(38f, 32f, 0f);
            }
            else
            {
                GameObject lightObj = new GameObject("Directional_SunLight");
                var l = lightObj.AddComponent<Light>();
                l.type = LightType.Directional;
                l.color = new Color(1.0f, 0.95f, 0.88f);
                l.intensity = 1.35f;
                l.shadows = LightShadows.Soft;
                lightObj.transform.rotation = Quaternion.Euler(38f, 32f, 0f);
            }

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.65f, 0.72f, 0.85f);
            RenderSettings.ambientEquatorColor = new Color(0.55f, 0.52f, 0.48f);
            RenderSettings.ambientGroundColor = new Color(0.35f, 0.28f, 0.22f);
        }

        private void SetupEnvironment()
        {
            if (FindObjectOfType<RoomEnvironmentBuilder>() == null)
            {
                GameObject envObj = new GameObject("Room_Environment");
                var builder = envObj.AddComponent<RoomEnvironmentBuilder>();

                Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
                if (litShader == null) litShader = Shader.Find("Standard");

                // Assign procedural PBR materials
                builder.floorMaterial = CreateMaterial(litShader, "M_Floor", new Color(0.85f, 0.72f, 0.55f), "T_Floor_Albedo.png", "T_Floor_Normal.png", 0.45f);
                builder.wallMaterial = CreateMaterial(litShader, "M_Wall", new Color(0.94f, 0.93f, 0.91f), "T_Wall_Albedo.png", "T_Wall_Normal.png", 0.15f);
                builder.ceilingMaterial = CreateMaterial(litShader, "M_Ceiling", new Color(0.98f, 0.98f, 0.98f), null, null, 0.1f);
                builder.windowFrameMaterial = CreateMaterial(litShader, "M_BlackSteel", new Color(0.12f, 0.12f, 0.14f), null, null, 0.8f, 0.7f);
                builder.glassMaterial = CreateMaterial(litShader, "M_Glass", new Color(0.7f, 0.85f, 0.95f, 0.25f), null, null, 0.95f, 0.1f);
                builder.rugMaterial = CreateMaterial(litShader, "M_Rug", new Color(0.88f, 0.85f, 0.80f), "T_Rug_Albedo.png", "T_Rug_Normal.png", 0.15f);
                builder.wallArtMaterial = CreateMaterial(litShader, "M_WallArt", Color.white, "T_WallArt.png", null, 0.4f);

                builder.BuildLivingRoom();
            }
        }

        private void SetupManagerAndUI()
        {
            var studioManager = FindObjectOfType<StudioManager>();
            if (studioManager == null)
            {
                GameObject mgrObj = new GameObject("StudioManager");
                studioManager = mgrObj.AddComponent<StudioManager>();
            }

            var cam = Camera.main;
            var raycaster = cam != null ? cam.GetComponent<ScreenToWorldRaycaster>() : null;

            var selector = studioManager.GetComponent<FurnitureSelector>();
            if (selector == null)
            {
                selector = studioManager.gameObject.AddComponent<FurnitureSelector>();
                selector.raycaster = raycaster;
            }

            var manipulator = studioManager.GetComponent<FurnitureManipulator>();
            if (manipulator == null)
            {
                manipulator = studioManager.gameObject.AddComponent<FurnitureManipulator>();
                manipulator.raycaster = raycaster;
            }

            var placement = studioManager.GetComponent<FurniturePlacementController>();
            if (placement == null)
            {
                placement = studioManager.gameObject.AddComponent<FurniturePlacementController>();
                placement.raycaster = raycaster;
            }

            var lightingMgr = studioManager.GetComponent<LightingPresetManager>();
            if (lightingMgr == null)
            {
                lightingMgr = studioManager.gameObject.AddComponent<LightingPresetManager>();
                lightingMgr.directionalSunLight = FindObjectOfType<Light>();
            }
            studioManager.lightingManager = lightingMgr;

            if (FindObjectOfType<StudioUIManager>() == null)
            {
                GameObject uiObj = new GameObject("Studio_UI");
                var uiMgr = uiObj.AddComponent<StudioUIManager>();
                uiMgr.studioManager = studioManager;
            }
        }

        private void SpawnStarterFurniture()
        {
            var studioManager = StudioManager.Instance;
            if (studioManager == null) return;

            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");

            // 1. Red Velvet Armchair (Question 1)
            Material matVelvet = CreateMaterial(litShader, "M_Velvet_Red", new Color(0.68f, 0.08f, 0.16f), "T_Velvet_Albedo.png", "T_Velvet_Normal.png", 0.22f, 0.04f);
            GameObject chair = CreatePrimitiveModel("Nordic Velvet Armchair", PrimitiveType.Cube, new Vector3(0.85f, 0.95f, 0.85f), matVelvet);
            chair.transform.position = new Vector3(-1.80f, 0f, 0.40f);
            chair.transform.rotation = Quaternion.Euler(0f, 25f, 0f);
            studioManager.RegisterPlacedObject(chair);

            // 2. Leather Sofa with Normal Mapping (Question 2)
            Material matLeather = CreateMaterial(litShader, "M_Leather_Cognac", new Color(0.72f, 0.42f, 0.22f), "T_Leather_Albedo.png", "T_Leather_Normal.png", 0.58f, 0.12f);
            GameObject sofa = CreatePrimitiveModel("Stockholm 3-Seater Leather Sofa", PrimitiveType.Cube, new Vector3(2.20f, 0.85f, 0.95f), matLeather);
            sofa.transform.position = new Vector3(0.00f, 0f, 1.20f);
            sofa.transform.rotation = Quaternion.identity;
            studioManager.RegisterPlacedObject(sofa);

            // 3. Oak Coffee Table
            Material matWood = CreateMaterial(litShader, "M_Wood_Oak", new Color(0.88f, 0.76f, 0.60f), "T_Wood_Albedo.png", "T_Wood_Normal.png", 0.45f, 0.0f);
            GameObject table = CreatePrimitiveModel("Oslo Oak Coffee Table", PrimitiveType.Cube, new Vector3(1.20f, 0.45f, 0.65f), matWood);
            table.transform.position = new Vector3(0.00f, 0f, -0.10f);
            table.transform.rotation = Quaternion.identity;
            studioManager.RegisterPlacedObject(table);

            // 4. Marble Side Table
            Material matMarble = CreateMaterial(litShader, "M_Marble_Carrara", new Color(0.95f, 0.95f, 0.95f), "T_Marble_Albedo.png", "T_Marble_Normal.png", 0.88f, 0.10f);
            GameObject sideTable = CreatePrimitiveModel("Bergen Marble Side Table", PrimitiveType.Cylinder, new Vector3(0.50f, 0.55f, 0.50f), matMarble);
            sideTable.transform.position = new Vector3(-2.45f, 0f, 0.80f);
            studioManager.RegisterPlacedObject(sideTable);

            // 5. Floor Lamp
            Material matSteel = CreateMaterial(litShader, "M_LampSteel", new Color(0.18f, 0.18f, 0.20f), null, null, 0.75f, 0.85f);
            GameObject lamp = CreatePrimitiveModel("Kobenhavn Arc Floor Lamp", PrimitiveType.Cylinder, new Vector3(0.40f, 1.85f, 0.40f), matSteel);
            lamp.transform.position = new Vector3(-2.60f, 0f, 1.80f);
            studioManager.RegisterPlacedObject(lamp);

            // 6. Potted Plant
            Material matPlant = CreateMaterial(litShader, "M_MonsteraPlant", new Color(0.15f, 0.45f, 0.20f), null, null, 0.35f, 0.0f);
            GameObject plant = CreatePrimitiveModel("Monstera in Ceramic Planter", PrimitiveType.Cylinder, new Vector3(0.60f, 1.10f, 0.60f), matPlant);
            plant.transform.position = new Vector3(2.65f, 0f, -1.85f);
            studioManager.RegisterPlacedObject(plant);

            Debug.Log("[StudioBootstrap] All 6 Scandinavian furniture models placed successfully.");
        }

        private GameObject CreatePrimitiveModel(string name, PrimitiveType type, Vector3 dimensions, Material mat)
        {
            GameObject obj = GameObject.CreatePrimitive(type);
            obj.name = name;
            obj.transform.localScale = dimensions;
            
            // Offset pivot so origin is at base (Y = 0)
            float halfHeight = dimensions.y * 0.5f;
            obj.transform.position = new Vector3(0f, halfHeight, 0f);

            var r = obj.GetComponent<MeshRenderer>();
            if (mat != null) r.material = mat;

            var col = obj.GetComponent<BoxCollider>();
            if (col == null && type != PrimitiveType.Cube)
            {
                Destroy(obj.GetComponent<Collider>());
                col = obj.AddComponent<BoxCollider>();
            }

            return obj;
        }

        private Material CreateMaterial(Shader shader, string name, Color color, string texFile, string norFile, float smoothness, float metallic = 0f)
        {
            Material mat = new Material(shader);
            mat.name = name;
            mat.color = color;
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);

            if (!string.IsNullOrEmpty(texFile))
            {
                Texture2D tex = Resources.Load<Texture2D>(texFile.Replace(".png", ""));
                if (tex != null)
                {
                    if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                    else mat.mainTexture = tex;
                }
            }

            if (!string.IsNullOrEmpty(norFile))
            {
                Texture2D nor = Resources.Load<Texture2D>(norFile.Replace(".png", ""));
                if (nor != null && mat.HasProperty("_BumpMap"))
                {
                    mat.SetTexture("_BumpMap", nor);
                    mat.EnableKeyword("_NORMALMAP");
                }
            }

            return mat;
        }
    }
}
