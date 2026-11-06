#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using InteriorStudio.Core;
using InteriorStudio.Environment;
using InteriorStudio.Interaction;
using InteriorStudio.UI;

namespace InteriorStudio.Editor
{
    [InitializeOnLoad]
    public static class StudioAssetGenerator
    {
        private const string MaterialsDir = "Assets/Materials";
        private const string PrefabsDir = "Assets/Prefabs";
        private const string ScenesDir = "Assets/Scenes";
        private const string TexturesDir = "Assets/Textures";

        static StudioAssetGenerator()
        {
            EditorApplication.delayCall += OnEditorLaunch;
        }

        private static void OnEditorLaunch()
        {
            // Auto-check if materials or prefabs are missing on launch
            if (!File.Exists(Path.Combine(MaterialsDir, "M_Velvet_Red.mat")) ||
                !File.Exists(Path.Combine(PrefabsDir, "Furniture_Chair_Velvet.prefab")))
            {
                Debug.Log("[StudioAssetGenerator] Missing assets detected on project open. Auto-generating studio assets...");
                GenerateAllAndSetupScene();
            }
        }

        [MenuItem("InteriorStudio/Generate All Studio Assets & Setup Scene")]
        public static void GenerateAllAndSetupScene()
        {
            Debug.Log("[StudioAssetGenerator] Starting full project generation...");

            EnsureDirectories();
            ConfigureTextureImporterSettings();
            var materials = CreateAllMaterials();
            var prefabs = CreateAllFurniturePrefabs(materials);
            BuildAndSaveMasterScene(materials, prefabs);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[StudioAssetGenerator] SUCCESS: All materials, prefabs, and master scene generated cleanly!");
        }

        private static void EnsureDirectories()
        {
            if (!AssetDatabase.IsValidFolder(MaterialsDir)) AssetDatabase.CreateFolder("Assets", "Materials");
            if (!AssetDatabase.IsValidFolder(PrefabsDir)) AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder(ScenesDir)) AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        private static void ConfigureTextureImporterSettings()
        {
            string[] normalMapFiles = new string[]
            {
                "T_Leather_Normal.png",
                "T_Velvet_Normal.png",
                "T_Wood_Normal.png",
                "T_Floor_Normal.png",
                "T_Rug_Normal.png",
                "T_Wall_Normal.png",
                "T_Marble_Normal.png",
                "T_Fabric_Normal.png"
            };

            foreach (var file in normalMapFiles)
            {
                string path = Path.Combine(TexturesDir, file).Replace("\\", "/");
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    if (importer.textureType != TextureImporterType.NormalMap)
                    {
                        importer.textureType = TextureImporterType.NormalMap;
                        importer.SaveAndReimport();
                        Debug.Log($"[StudioAssetGenerator] Configured Normal Map importer for: {file}");
                    }
                }
            }
        }

        private static Dictionary<string, Material> CreateAllMaterials()
        {
            var dict = new Dictionary<string, Material>();
            Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
            if (urpShader == null) urpShader = Shader.Find("Standard");

            // 1. Red Velvet (Question 1)
            Material matVelvet = new Material(urpShader);
            matVelvet.name = "M_Velvet_Red";
            matVelvet.color = new Color(0.68f, 0.08f, 0.16f); // Crimson
            matVelvet.SetFloat("_Smoothness", 0.22f);
            matVelvet.SetFloat("_Metallic", 0.04f);
            AssignTexture(matVelvet, "_BaseMap", "T_Velvet_Albedo.png");
            AssignNormalMap(matVelvet, "T_Velvet_Normal.png", 1.2f);
            SaveOrUpdateMaterial(matVelvet);
            dict["Velvet_Red"] = matVelvet;

            // 2. Cognac Leather (Question 2)
            Material matLeather = new Material(urpShader);
            matLeather.name = "M_Leather_Cognac";
            matLeather.color = new Color(0.72f, 0.42f, 0.22f); // Cognac Saddle
            matLeather.SetFloat("_Smoothness", 0.58f);
            matLeather.SetFloat("_Metallic", 0.12f);
            AssignTexture(matLeather, "_BaseMap", "T_Leather_Albedo.png");
            AssignNormalMap(matLeather, "T_Leather_Normal.png", 1.5f);
            SaveOrUpdateMaterial(matLeather);
            dict["Leather_Cognac"] = matLeather;

            // 3. Scandinavian Natural Oak
            Material matOak = new Material(urpShader);
            matOak.name = "M_Wood_Oak";
            matOak.color = new Color(0.88f, 0.76f, 0.60f);
            matOak.SetFloat("_Smoothness", 0.42f);
            matOak.SetFloat("_Metallic", 0.0f);
            AssignTexture(matOak, "_BaseMap", "T_Wood_Albedo.png");
            AssignNormalMap(matOak, "T_Wood_Normal.png", 1.0f);
            SaveOrUpdateMaterial(matOak);
            dict["Wood_Oak"] = matOak;

            // 4. Dark Walnut
            Material matWalnut = new Material(urpShader);
            matWalnut.name = "M_Wood_Walnut";
            matWalnut.color = new Color(0.38f, 0.26f, 0.18f);
            matWalnut.SetFloat("_Smoothness", 0.48f);
            matWalnut.SetFloat("_Metallic", 0.0f);
            AssignTexture(matWalnut, "_BaseMap", "T_Wood_Albedo.png");
            AssignNormalMap(matWalnut, "T_Wood_Normal.png", 1.0f);
            SaveOrUpdateMaterial(matWalnut);
            dict["Wood_Walnut"] = matWalnut;

            // 5. Hardwood Floor Parquet
            Material matFloor = new Material(urpShader);
            matFloor.name = "M_Floor_Parquet";
            matFloor.color = Color.white;
            matFloor.SetFloat("_Smoothness", 0.62f);
            matFloor.SetFloat("_Metallic", 0.05f);
            AssignTexture(matFloor, "_BaseMap", "T_Floor_Albedo.png", new Vector2(4f, 3f));
            AssignNormalMap(matFloor, "T_Floor_Normal.png", 1.2f, new Vector2(4f, 3f));
            SaveOrUpdateMaterial(matFloor);
            dict["Floor"] = matFloor;

            // 6. Plaster Wall
            Material matWall = new Material(urpShader);
            matWall.name = "M_Wall_Plaster";
            matWall.color = Color.white;
            matWall.SetFloat("_Smoothness", 0.15f);
            matWall.SetFloat("_Metallic", 0.0f);
            AssignTexture(matWall, "_BaseMap", "T_Wall_Albedo.png", new Vector2(3f, 2f));
            AssignNormalMap(matWall, "T_Wall_Normal.png", 0.8f, new Vector2(3f, 2f));
            SaveOrUpdateMaterial(matWall);
            dict["Wall"] = matWall;

            // 7. Ceiling
            Material matCeiling = new Material(urpShader);
            matCeiling.name = "M_Ceiling";
            matCeiling.color = new Color(0.96f, 0.96f, 0.96f);
            matCeiling.SetFloat("_Smoothness", 0.10f);
            matCeiling.SetFloat("_Metallic", 0.0f);
            SaveOrUpdateMaterial(matCeiling);
            dict["Ceiling"] = matCeiling;

            // 8. Geometric Rug
            Material matRug = new Material(urpShader);
            matRug.name = "M_Rug_Geometric";
            matRug.color = Color.white;
            matRug.SetFloat("_Smoothness", 0.18f);
            matRug.SetFloat("_Metallic", 0.0f);
            AssignTexture(matRug, "_BaseMap", "T_Rug_Albedo.png", new Vector2(2f, 2f));
            AssignNormalMap(matRug, "T_Rug_Normal.png", 1.2f, new Vector2(2f, 2f));
            SaveOrUpdateMaterial(matRug);
            dict["Rug"] = matRug;

            // 9. Wall Art
            Material matArt = new Material(urpShader);
            matArt.name = "M_WallArt";
            matArt.color = Color.white;
            matArt.SetFloat("_Smoothness", 0.35f);
            matArt.SetFloat("_Metallic", 0.0f);
            AssignTexture(matArt, "_BaseMap", "T_WallArt.png");
            SaveOrUpdateMaterial(matArt);
            dict["WallArt"] = matArt;

            // 10. Carrara Marble
            Material matMarble = new Material(urpShader);
            matMarble.name = "M_Marble_Carrara";
            matMarble.color = Color.white;
            matMarble.SetFloat("_Smoothness", 0.85f);
            matMarble.SetFloat("_Metallic", 0.08f);
            AssignTexture(matMarble, "_BaseMap", "T_Marble_Albedo.png");
            AssignNormalMap(matMarble, "T_Marble_Normal.png", 0.9f);
            SaveOrUpdateMaterial(matMarble);
            dict["Marble"] = matMarble;

            // 11. Brushed Brass Metal
            Material matBrass = new Material(urpShader);
            matBrass.name = "M_Metal_Brass";
            matBrass.color = new Color(0.92f, 0.78f, 0.45f);
            matBrass.SetFloat("_Smoothness", 0.78f);
            matBrass.SetFloat("_Metallic", 0.92f);
            SaveOrUpdateMaterial(matBrass);
            dict["Brass"] = matBrass;

            // 12. Matte Black Steel
            Material matBlackSteel = new Material(urpShader);
            matBlackSteel.name = "M_Metal_BlackSteel";
            matBlackSteel.color = new Color(0.12f, 0.13f, 0.14f);
            matBlackSteel.SetFloat("_Smoothness", 0.35f);
            matBlackSteel.SetFloat("_Metallic", 0.80f);
            SaveOrUpdateMaterial(matBlackSteel);
            dict["BlackSteel"] = matBlackSteel;

            // 13. Window Glass
            Material matGlass = new Material(urpShader);
            matGlass.name = "M_Window_Glass";
            matGlass.color = new Color(0.85f, 0.92f, 0.98f, 0.28f);
            matGlass.SetFloat("_Smoothness", 0.95f);
            matGlass.SetFloat("_Metallic", 0.1f);
            // Configure transparent blend mode
            matGlass.SetFloat("_Surface", 1.0f); // 1 = Transparent
            matGlass.SetFloat("_Blend", 0.0f);   // Alpha blend
            matGlass.renderQueue = (int)RenderQueue.Transparent;
            matGlass.SetOverrideTag("RenderType", "Transparent");
            SaveOrUpdateMaterial(matGlass);
            dict["Glass"] = matGlass;

            // 14. Plant Foliage
            Material matPlant = new Material(urpShader);
            matPlant.name = "M_Plant_Foliage";
            matPlant.color = new Color(0.14f, 0.42f, 0.22f);
            matPlant.SetFloat("_Smoothness", 0.45f);
            matPlant.SetFloat("_Metallic", 0.0f);
            SaveOrUpdateMaterial(matPlant);
            dict["Plant"] = matPlant;

            // 15. Terracotta Planter Pot
            Material matPot = new Material(urpShader);
            matPot.name = "M_Pot_Terracotta";
            matPot.color = new Color(0.78f, 0.42f, 0.28f);
            matPot.SetFloat("_Smoothness", 0.25f);
            matPot.SetFloat("_Metallic", 0.0f);
            SaveOrUpdateMaterial(matPot);
            dict["Pot"] = matPot;

            return dict;
        }

        private static void AssignTexture(Material mat, string prop, string texFile, Vector2? tiling = null)
        {
            string path = Path.Combine(TexturesDir, texFile).Replace("\\", "/");
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null)
            {
                mat.SetTexture(prop, tex);
                if (tiling.HasValue) mat.SetTextureScale(prop, tiling.Value);
            }
        }

        private static void AssignNormalMap(Material mat, string texFile, float scale = 1.0f, Vector2? tiling = null)
        {
            string path = Path.Combine(TexturesDir, texFile).Replace("\\", "/");
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null)
            {
                mat.EnableKeyword("_NORMALMAP");
                mat.SetTexture("_BumpMap", tex);
                mat.SetFloat("_BumpScale", scale);
                if (tiling.HasValue) mat.SetTextureScale("_BumpMap", tiling.Value);
            }
        }

        private static void SaveOrUpdateMaterial(Material mat)
        {
            string path = Path.Combine(MaterialsDir, mat.name + ".mat").Replace("\\", "/");
            AssetDatabase.CreateAsset(mat, path);
        }

        private static Dictionary<string, GameObject> CreateAllFurniturePrefabs(Dictionary<string, Material> mats)
        {
            var prefabs = new Dictionary<string, GameObject>();

            // 1. NORDIC VELVET ARMCHAIR (Question 1 Hero Asset)
            GameObject chair = new GameObject("Nordic_Velvet_Armchair");
            BuildVelvetArmchairGeometry(chair, mats["Velvet_Red"], mats["Wood_Oak"], mats["Brass"]);
            SavePrefab(chair, "Chair_Velvet.prefab");
            prefabs["Chair_Velvet"] = chair;

            // 2. STOCKHOLM 3-SEATER LEATHER SOFA (Question 2 Hero Asset)
            GameObject sofa = new GameObject("Stockholm_Leather_Sofa");
            BuildLeatherSofaGeometry(sofa, mats["Leather_Cognac"], mats["Wood_Oak"]);
            SavePrefab(sofa, "Sofa_Leather.prefab");
            prefabs["Sofa_Leather"] = sofa;

            // 3. OSLO OAK COFFEE TABLE
            GameObject coffeeTable = new GameObject("Oslo_Oak_CoffeeTable");
            BuildCoffeeTableGeometry(coffeeTable, mats["Wood_Oak"]);
            SavePrefab(coffeeTable, "Table_Coffee.prefab");
            prefabs["Table_Coffee"] = coffeeTable;

            // 4. BERGEN PEDESTAL SIDE TABLE
            GameObject sideTable = new GameObject("Bergen_Marble_SideTable");
            BuildSideTableGeometry(sideTable, mats["Marble"], mats["BlackSteel"]);
            SavePrefab(sideTable, "Table_Side.prefab");
            prefabs["Table_Side"] = sideTable;

            // 5. KOBENHAVN ARC FLOOR LAMP
            GameObject floorLamp = new GameObject("Kobenhavn_FloorLamp");
            BuildFloorLampGeometry(floorLamp, mats["Brass"], mats["BlackSteel"]);
            SavePrefab(floorLamp, "Lamp_Floor.prefab");
            prefabs["Lamp_Floor"] = floorLamp;

            // 6. MONSTERA PLANT DECOR
            GameObject plant = new GameObject("Monstera_Plant_Planter");
            BuildPlantGeometry(plant, mats["Plant"], mats["Pot"], mats["Wood_Walnut"]);
            SavePrefab(plant, "Decor_Plant.prefab");
            prefabs["Decor_Plant"] = plant;

            return prefabs;
        }

        private static void BuildVelvetArmchairGeometry(GameObject root, Material velvetMat, Material woodMat, Material brassMat)
        {
            // Plush seat cushion
            GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seat.name = "Velvet_Seat_Cushion";
            seat.transform.SetParent(root.transform, false);
            seat.transform.localPosition = new Vector3(0f, 0.42f, 0f);
            seat.transform.localScale = new Vector3(0.72f, 0.22f, 0.70f);
            seat.GetComponent<MeshRenderer>().sharedMaterial = velvetMat;

            // Barrel curved backrest
            GameObject backrest = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            backrest.name = "Velvet_Barrel_Backrest";
            backrest.transform.SetParent(root.transform, false);
            backrest.transform.localPosition = new Vector3(0f, 0.68f, -0.16f);
            backrest.transform.localScale = new Vector3(0.76f, 0.36f, 0.48f);
            backrest.GetComponent<MeshRenderer>().sharedMaterial = velvetMat;

            // Left armrest
            GameObject leftArm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftArm.name = "Velvet_Armrest_Left";
            leftArm.transform.SetParent(root.transform, false);
            leftArm.transform.localPosition = new Vector3(-0.38f, 0.56f, 0.04f);
            leftArm.transform.localScale = new Vector3(0.14f, 0.24f, 0.62f);
            leftArm.GetComponent<MeshRenderer>().sharedMaterial = velvetMat;

            // Right armrest
            GameObject rightArm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightArm.name = "Velvet_Armrest_Right";
            rightArm.transform.SetParent(root.transform, false);
            rightArm.transform.localPosition = new Vector3(0.38f, 0.56f, 0.04f);
            rightArm.transform.localScale = new Vector3(0.14f, 0.24f, 0.62f);
            rightArm.GetComponent<MeshRenderer>().sharedMaterial = velvetMat;

            // 4 Tapered wooden legs with angled splay
            CreateTaperedLeg(root, new Vector3(-0.28f, 0.16f, -0.24f), new Vector3(12f, 0f, -12f), woodMat, brassMat);
            CreateTaperedLeg(root, new Vector3(0.28f, 0.16f, -0.24f), new Vector3(12f, 0f, 12f), woodMat, brassMat);
            CreateTaperedLeg(root, new Vector3(-0.28f, 0.16f, 0.24f), new Vector3(-12f, 0f, -12f), woodMat, brassMat);
            CreateTaperedLeg(root, new Vector3(0.28f, 0.16f, 0.24f), new Vector3(-12f, 0f, 12f), woodMat, brassMat);

            // Add BoxCollider to root for selection raycasting
            var col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.48f, 0f);
            col.size = new Vector3(0.92f, 0.96f, 0.88f);
        }

        private static void BuildLeatherSofaGeometry(GameObject root, Material leatherMat, Material legMat)
        {
            // Base frame
            GameObject baseFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseFrame.name = "Leather_Base_Plinth";
            baseFrame.transform.SetParent(root.transform, false);
            baseFrame.transform.localPosition = new Vector3(0f, 0.24f, 0f);
            baseFrame.transform.localScale = new Vector3(2.20f, 0.16f, 0.88f);
            baseFrame.GetComponent<MeshRenderer>().sharedMaterial = leatherMat;

            // 3 Seat Cushions
            float cushionW = 0.66f;
            for (int i = 0; i < 3; i++)
            {
                float x = (i - 1) * (cushionW + 0.02f);
                GameObject cushion = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cushion.name = $"Leather_SeatCushion_{i+1}";
                cushion.transform.SetParent(root.transform, false);
                cushion.transform.localPosition = new Vector3(x, 0.40f, 0.06f);
                cushion.transform.localScale = new Vector3(cushionW, 0.18f, 0.72f);
                cushion.GetComponent<MeshRenderer>().sharedMaterial = leatherMat;
            }

            // 3 Back Cushions
            for (int i = 0; i < 3; i++)
            {
                float x = (i - 1) * (cushionW + 0.02f);
                GameObject backCushion = GameObject.CreatePrimitive(PrimitiveType.Cube);
                backCushion.name = $"Leather_BackCushion_{i+1}";
                backCushion.transform.SetParent(root.transform, false);
                backCushion.transform.localPosition = new Vector3(x, 0.66f, -0.28f);
                backCushion.transform.localRotation = Quaternion.Euler(-8f, 0f, 0f);
                backCushion.transform.localScale = new Vector3(cushionW, 0.42f, 0.18f);
                backCushion.GetComponent<MeshRenderer>().sharedMaterial = leatherMat;
            }

            // Left armrest
            GameObject leftArm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftArm.name = "Leather_Armrest_Left";
            leftArm.transform.SetParent(root.transform, false);
            leftArm.transform.localPosition = new Vector3(-1.08f, 0.54f, 0f);
            leftArm.transform.localScale = new Vector3(0.18f, 0.44f, 0.88f);
            leftArm.GetComponent<MeshRenderer>().sharedMaterial = leatherMat;

            // Right armrest
            GameObject rightArm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightArm.name = "Leather_Armrest_Right";
            rightArm.transform.SetParent(root.transform, false);
            rightArm.transform.localPosition = new Vector3(1.08f, 0.54f, 0f);
            rightArm.transform.localScale = new Vector3(0.18f, 0.44f, 0.88f);
            rightArm.GetComponent<MeshRenderer>().sharedMaterial = leatherMat;

            // 4 Minimalist oak legs
            CreateSquareLeg(root, new Vector3(-0.95f, 0.08f, -0.35f), legMat);
            CreateSquareLeg(root, new Vector3(0.95f, 0.08f, -0.35f), legMat);
            CreateSquareLeg(root, new Vector3(-0.95f, 0.08f, 0.35f), legMat);
            CreateSquareLeg(root, new Vector3(0.95f, 0.08f, 0.35f), legMat);

            var col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.45f, 0f);
            col.size = new Vector3(2.36f, 0.90f, 0.96f);
        }

        private static void BuildCoffeeTableGeometry(GameObject root, Material woodMat)
        {
            // Oval / rounded pill tabletop
            GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            top.name = "CoffeeTable_Top";
            top.transform.SetParent(root.transform, false);
            top.transform.localPosition = new Vector3(0f, 0.42f, 0f);
            top.transform.localScale = new Vector3(1.20f, 0.035f, 0.65f);
            top.GetComponent<MeshRenderer>().sharedMaterial = woodMat;

            // 4 Splayed tapered wooden legs
            CreateTaperedLeg(root, new Vector3(-0.45f, 0.20f, -0.22f), new Vector3(10f, 0f, -10f), woodMat, null);
            CreateTaperedLeg(root, new Vector3(0.45f, 0.20f, -0.22f), new Vector3(10f, 0f, 10f), woodMat, null);
            CreateTaperedLeg(root, new Vector3(-0.45f, 0.20f, 0.22f), new Vector3(-10f, 0f, -10f), woodMat, null);
            CreateTaperedLeg(root, new Vector3(0.45f, 0.20f, 0.22f), new Vector3(-10f, 0f, 10f), woodMat, null);

            var col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.22f, 0f);
            col.size = new Vector3(1.25f, 0.46f, 0.70f);
        }

        private static void BuildSideTableGeometry(GameObject root, Material marbleMat, Material steelMat)
        {
            // Marble disc top
            GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            top.name = "SideTable_MarbleTop";
            top.transform.SetParent(root.transform, false);
            top.transform.localPosition = new Vector3(0f, 0.52f, 0f);
            top.transform.localScale = new Vector3(0.48f, 0.025f, 0.48f);
            top.GetComponent<MeshRenderer>().sharedMaterial = marbleMat;

            // Center steel stem
            GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stem.name = "SideTable_SteelStem";
            stem.transform.SetParent(root.transform, false);
            stem.transform.localPosition = new Vector3(0f, 0.26f, 0f);
            stem.transform.localScale = new Vector3(0.04f, 0.26f, 0.04f);
            stem.GetComponent<MeshRenderer>().sharedMaterial = steelMat;

            // Heavy circular base disc
            GameObject baseDisc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            baseDisc.name = "SideTable_BaseDisc";
            baseDisc.transform.SetParent(root.transform, false);
            baseDisc.transform.localPosition = new Vector3(0f, 0.015f, 0f);
            baseDisc.transform.localScale = new Vector3(0.36f, 0.015f, 0.36f);
            baseDisc.GetComponent<MeshRenderer>().sharedMaterial = steelMat;

            var col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.27f, 0f);
            col.size = new Vector3(0.52f, 0.55f, 0.52f);
        }

        private static void BuildFloorLampGeometry(GameObject root, Material brassMat, Material baseMat)
        {
            // Heavy base
            GameObject baseDisc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            baseDisc.name = "Lamp_Base";
            baseDisc.transform.SetParent(root.transform, false);
            baseDisc.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            baseDisc.transform.localScale = new Vector3(0.40f, 0.02f, 0.40f);
            baseDisc.GetComponent<MeshRenderer>().sharedMaterial = baseMat;

            // Vertical pole
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "Lamp_VerticalPole";
            pole.transform.SetParent(root.transform, false);
            pole.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            pole.transform.localScale = new Vector3(0.03f, 0.85f, 0.03f);
            pole.GetComponent<MeshRenderer>().sharedMaterial = brassMat;

            // Arch arm
            GameObject arch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            arch.name = "Lamp_ArchArm";
            arch.transform.SetParent(root.transform, false);
            arch.transform.localPosition = new Vector3(0.22f, 1.74f, 0f);
            arch.transform.localRotation = Quaternion.Euler(0f, 0f, -40f);
            arch.transform.localScale = new Vector3(0.025f, 0.35f, 0.025f);
            arch.GetComponent<MeshRenderer>().sharedMaterial = brassMat;

            // Conical shade
            GameObject shade = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shade.name = "Lamp_Shade";
            shade.transform.SetParent(root.transform, false);
            shade.transform.localPosition = new Vector3(0.44f, 1.62f, 0f);
            shade.transform.localScale = new Vector3(0.28f, 0.14f, 0.28f);
            shade.GetComponent<MeshRenderer>().sharedMaterial = brassMat;

            // Warm interior point light source
            GameObject lightObj = new GameObject("Lamp_PointLight");
            lightObj.transform.SetParent(root.transform, false);
            lightObj.transform.localPosition = new Vector3(0.44f, 1.55f, 0f);
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1.0f, 0.82f, 0.55f);
            light.intensity = 1.25f;
            light.range = 5.0f;
            light.shadows = LightShadows.Soft;

            var col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0.2f, 0.95f, 0f);
            col.size = new Vector3(0.75f, 1.90f, 0.45f);
        }

        private static void BuildPlantGeometry(GameObject root, Material leafMat, Material potMat, Material soilMat)
        {
            // Planter pot
            GameObject pot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pot.name = "Plant_Pot";
            pot.transform.SetParent(root.transform, false);
            pot.transform.localPosition = new Vector3(0f, 0.24f, 0f);
            pot.transform.localScale = new Vector3(0.44f, 0.24f, 0.44f);
            pot.GetComponent<MeshRenderer>().sharedMaterial = potMat;

            // Soil
            GameObject soil = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            soil.name = "Plant_Soil";
            soil.transform.SetParent(root.transform, false);
            soil.transform.localPosition = new Vector3(0f, 0.46f, 0f);
            soil.transform.localScale = new Vector3(0.40f, 0.02f, 0.40f);
            soil.GetComponent<MeshRenderer>().sharedMaterial = soilMat;

            // 6 Fan-out Monstera leaves with varied angles
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f + 15f;
                GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Quad);
                leaf.name = $"Plant_Leaf_{i+1}";
                leaf.transform.SetParent(root.transform, false);
                leaf.transform.localPosition = new Vector3(
                    Mathf.Sin(angle * Mathf.Deg2Rad) * 0.18f, 
                    0.68f + (i % 2) * 0.12f, 
                    Mathf.Cos(angle * Mathf.Deg2Rad) * 0.18f);
                leaf.transform.localRotation = Quaternion.Euler(35f + (i % 3) * 10f, angle, 0f);
                leaf.transform.localScale = new Vector3(0.28f, 0.42f, 1f);
                leaf.GetComponent<MeshRenderer>().sharedMaterial = leafMat;
            }

            var col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.52f, 0f);
            col.size = new Vector3(0.65f, 1.05f, 0.65f);
        }

        private static void CreateTaperedLeg(GameObject root, Vector3 pos, Vector3 rot, Material woodMat, Material brassMat)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leg.name = "Tapered_Leg";
            leg.transform.SetParent(root.transform, false);
            leg.transform.localPosition = pos;
            leg.transform.localRotation = Quaternion.Euler(rot);
            leg.transform.localScale = new Vector3(0.045f, 0.16f, 0.045f);
            leg.GetComponent<MeshRenderer>().sharedMaterial = woodMat;
        }

        private static void CreateSquareLeg(GameObject root, Vector3 pos, Material mat)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leg.name = "Square_Leg";
            leg.transform.SetParent(root.transform, false);
            leg.transform.localPosition = pos;
            leg.transform.localScale = new Vector3(0.06f, 0.16f, 0.06f);
            leg.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }

        private static void SavePrefab(GameObject obj, string filename)
        {
            string path = Path.Combine(PrefabsDir, filename).Replace("\\", "/");
            PrefabUtility.SaveAsPrefabAsset(obj, path);
            GameObject.DestroyImmediate(obj);
        }

        private static void BuildAndSaveMasterScene(Dictionary<string, Material> mats, Dictionary<string, GameObject> prefabs)
        {
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Camera & Navigation
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            var camera = camObj.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.fieldOfView = 60f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            camObj.AddComponent<UniversalAdditionalCameraData>();
            camObj.AddComponent<AudioListener>();

            var nav = camObj.AddComponent<CameraNavigation>();
            var raycaster = camObj.AddComponent<ScreenToWorldRaycaster>();
            raycaster.targetCamera = camera;

            // Camera initial position: elevated perspective inside living room looking towards sofa and velvet chair
            camObj.transform.position = new Vector3(-2.8f, 1.65f, -2.1f);
            camObj.transform.rotation = Quaternion.Euler(18f, 48f, 0f);

            // 2. Lighting Presets Setup
            GameObject lightingRoot = new GameObject("Lighting_Rig");
            var lightingManager = lightingRoot.AddComponent<LightingPresetManager>();

            // Directional Sun Light
            GameObject sunObj = new GameObject("Directional_SunLight");
            sunObj.transform.SetParent(lightingRoot.transform, false);
            var sun = sunObj.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1.0f, 0.96f, 0.90f);
            sun.intensity = 1.35f;
            sun.shadows = LightShadows.Soft;
            sunObj.transform.rotation = Quaternion.Euler(38f, 32f, 0f);
            lightingManager.directionalSunLight = sun;

            // 4 Ceiling Pot Lights
            List<Light> potLights = new List<Light>();
            Vector2[] potPositions = new Vector2[]
            {
                new Vector2(-2.2f, -1.2f),
                new Vector2(2.2f, -1.2f),
                new Vector2(-2.2f, 1.4f),
                new Vector2(2.2f, 1.4f)
            };

            foreach (var pos in potPositions)
            {
                GameObject pot = new GameObject($"Ceiling_PotLight_({pos.x},{pos.y})");
                pot.transform.SetParent(lightingRoot.transform, false);
                pot.transform.position = new Vector3(pos.x, 3.1f, pos.y);
                pot.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                var pl = pot.AddComponent<Light>();
                pl.type = LightType.Spot;
                pl.spotAngle = 65f;
                pl.range = 4.2f;
                pl.intensity = 0.15f;
                pl.color = new Color(1.0f, 0.92f, 0.82f);
                potLights.Add(pl);
            }
            lightingManager.ceilingPotLights = potLights.ToArray();

            // 3. Room Environment Builder
            GameObject envObj = new GameObject("Room_Environment");
            var envBuilder = envObj.AddComponent<RoomEnvironmentBuilder>();
            envBuilder.floorMaterial = mats["Floor"];
            envBuilder.wallMaterial = mats["Wall"];
            envBuilder.ceilingMaterial = mats["Ceiling"];
            envBuilder.windowFrameMaterial = mats["BlackSteel"];
            envBuilder.glassMaterial = mats["Glass"];
            envBuilder.rugMaterial = mats["Rug"];
            envBuilder.wallArtMaterial = mats["WallArt"];
            envBuilder.BuildLivingRoom();

            // 4. Studio Manager & Interaction Setup
            GameObject managerObj = new GameObject("StudioManager");
            var studioManager = managerObj.AddComponent<StudioManager>();
            var selector = managerObj.AddComponent<FurnitureSelector>();
            selector.raycaster = raycaster;
            var manipulator = managerObj.AddComponent<FurnitureManipulator>();
            manipulator.raycaster = raycaster;
            manipulator.selector = selector;
            var placement = managerObj.AddComponent<FurniturePlacementController>();
            placement.raycaster = raycaster;
            placement.selector = selector;

            studioManager.environmentBuilder = envBuilder;
            studioManager.lightingManager = lightingManager;
            studioManager.raycaster = raycaster;
            studioManager.selector = selector;
            studioManager.placementController = placement;
            studioManager.cameraNavigation = nav;

            // Illumination Visualizer for Question 1
            var visualizer = managerObj.AddComponent<IlluminationVisualizer>();

            // UI Manager
            var ui = managerObj.AddComponent<StudioUIManager>();
            ui.studioManager = studioManager;
            ui.manipulator = manipulator;
            ui.placementController = placement;
            ui.lightingManager = lightingManager;
            ui.illuminationVisualizer = visualizer;
            ui.raycaster = raycaster;

            // 5. Initial Furniture Arrangement
            string prefabsRoot = "Assets/Prefabs";
            GameObject sofaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabsRoot}/Sofa_Leather.prefab");
            GameObject chairPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabsRoot}/Chair_Velvet.prefab");
            GameObject coffeePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabsRoot}/Table_Coffee.prefab");
            GameObject sidePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabsRoot}/Table_Side.prefab");
            GameObject lampPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabsRoot}/Lamp_Floor.prefab");
            GameObject plantPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabsRoot}/Decor_Plant.prefab");

            // Update catalogue prefab references in StudioManager
            UpdateCataloguePrefabRefs(studioManager, sofaPrefab, chairPrefab, coffeePrefab, sidePrefab, lampPrefab, plantPrefab);

            // Spawn Stockholm Leather Sofa
            if (sofaPrefab != null)
            {
                var s = (GameObject)PrefabUtility.InstantiatePrefab(sofaPrefab);
                s.name = "Stockholm 3-Seater Leather Sofa";
                s.transform.position = new Vector3(0f, 0f, 1.2f);
                s.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                studioManager.RegisterPlacedObject(s);
            }

            // Spawn Nordic Velvet Armchair (Prominently near sunny window - Question 1)
            if (chairPrefab != null)
            {
                var c = (GameObject)PrefabUtility.InstantiatePrefab(chairPrefab);
                c.name = "Nordic Velvet Armchair";
                c.transform.position = new Vector3(-1.45f, 0f, -0.45f);
                c.transform.rotation = Quaternion.Euler(0f, 42f, 0f);
                studioManager.RegisterPlacedObject(c);
                studioManager.SelectFurniture(c);

                // Wire to visualizer
                var meshRend = c.GetComponentInChildren<MeshRenderer>();
                visualizer.targetChairRenderer = meshRend;
            }

            // Spawn Oslo Oak Coffee Table
            if (coffeePrefab != null)
            {
                var t = (GameObject)PrefabUtility.InstantiatePrefab(coffeePrefab);
                t.name = "Oslo Oak Coffee Table";
                t.transform.position = new Vector3(0f, 0f, 0.25f);
                studioManager.RegisterPlacedObject(t);
            }

            // Spawn Bergen Pedestal Side Table
            if (sidePrefab != null)
            {
                var st = (GameObject)PrefabUtility.InstantiatePrefab(sidePrefab);
                st.name = "Bergen Pedestal Side Table";
                st.transform.position = new Vector3(-2.25f, 0f, -0.35f);
                studioManager.RegisterPlacedObject(st);
            }

            // Spawn Kobenhavn Arc Floor Lamp
            if (lampPrefab != null)
            {
                var l = (GameObject)PrefabUtility.InstantiatePrefab(lampPrefab);
                l.name = "Kobenhavn Arc Floor Lamp";
                l.transform.position = new Vector3(1.75f, 0f, 1.45f);
                l.transform.rotation = Quaternion.Euler(0f, -125f, 0f);
                studioManager.RegisterPlacedObject(l);

                // Wire lamp point light to lighting manager
                var lampLight = l.GetComponentInChildren<Light>();
                lightingManager.floorLampLight = lampLight;
            }

            // Spawn Monstera Plant
            if (plantPrefab != null)
            {
                var p = (GameObject)PrefabUtility.InstantiatePrefab(plantPrefab);
                p.name = "Monstera in Ceramic Planter";
                p.transform.position = new Vector3(2.65f, 0f, -1.85f);
                studioManager.RegisterPlacedObject(p);
            }

            // Initial lighting preset apply
            lightingManager.ApplyPreset(0);

            // Save scene
            string scenePath = Path.Combine(ScenesDir, "InteriorStudio.unity").Replace("\\", "/");
            EditorSceneManager.SaveScene(newScene, scenePath);
            Debug.Log($"[StudioAssetGenerator] Master scene saved to: {scenePath}");

            // Update EditorBuildSettings to include InteriorStudio.unity
            var scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(scenePath, true)
            };
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void UpdateCataloguePrefabRefs(StudioManager mgr, 
            GameObject sofa, GameObject chair, GameObject coffee, GameObject side, GameObject lamp, GameObject plant)
        {
            foreach (var item in mgr.catalogue)
            {
                if (item.id == "chair_velvet") item.prefab = chair;
                else if (item.id == "sofa_leather") item.prefab = sofa;
                else if (item.id == "table_coffee") item.prefab = coffee;
                else if (item.id == "table_side") item.prefab = side;
                else if (item.id == "lamp_floor") item.prefab = lamp;
                else if (item.id == "decor_plant") item.prefab = plant;
            }
        }
    }
}
#endif
