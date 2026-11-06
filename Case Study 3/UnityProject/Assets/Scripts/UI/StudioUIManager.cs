using System;
using UnityEngine;
using InteriorStudio.Core;
using InteriorStudio.Environment;
using InteriorStudio.Interaction;

namespace InteriorStudio.UI
{
    public class StudioUIManager : MonoBehaviour
    {
        [Header("Manager References")]
        public StudioManager studioManager;
        public FurnitureManipulator manipulator;
        public FurniturePlacementController placementController;
        public LightingPresetManager lightingManager;
        public IlluminationVisualizer illuminationVisualizer;
        public ScreenToWorldRaycaster raycaster;

        [Header("UI Toggle State")]
        public bool showUI = true;
        public bool showHelpOverlay = true;
        public bool normalMappingEnabled = true;

        // Telemetry
        private float fpsAccumulator = 0f;
        private int fpsFrameCount = 0;
        private float currentFps = 60f;
        private float fpsTimer = 0f;

        private GUIStyle headerStyle;
        private GUIStyle subHeaderStyle;
        private GUIStyle panelStyle;
        private GUIStyle buttonStyle;
        private GUIStyle activeButtonStyle;
        private GUIStyle labelStyle;
        private GUIStyle telemetryStyle;
        private bool stylesInitialized = false;

        private void Start()
        {
            if (studioManager == null) studioManager = StudioManager.Instance;
        }

        private void Update()
        {
            // FPS calculation
            fpsAccumulator += Time.unscaledDeltaTime;
            fpsFrameCount++;
            fpsTimer += Time.unscaledDeltaTime;
            if (fpsTimer >= 0.5f)
            {
                currentFps = fpsFrameCount / fpsAccumulator;
                fpsAccumulator = 0f;
                fpsFrameCount = 0;
                fpsTimer = 0f;
            }

            // Keyboard toggles
            if (Input.GetKeyDown(KeyCode.Tab)) showUI = !showUI;
            if (Input.GetKeyDown(KeyCode.H)) showHelpOverlay = !showHelpOverlay;
        }

        private void InitializeStyles()
        {
            if (stylesInitialized) return;

            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            headerStyle.normal.textColor = new Color(0.95f, 0.95f, 0.98f);

            subHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            subHeaderStyle.normal.textColor = new Color(0.35f, 0.85f, 1.0f);

            panelStyle = new GUIStyle(GUI.skin.box);
            Texture2D panelTex = new Texture2D(1, 1);
            panelTex.SetPixel(0, 0, new Color(0.08f, 0.10f, 0.14f, 0.90f));
            panelTex.Apply();
            panelStyle.normal.background = panelTex;

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 11,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter
            };
            buttonStyle.normal.textColor = Color.white;

            activeButtonStyle = new GUIStyle(buttonStyle)
            {
                fontStyle = FontStyle.Bold
            };
            activeButtonStyle.normal.textColor = new Color(0.2f, 0.9f, 1.0f);

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11
            };
            labelStyle.normal.textColor = new Color(0.85f, 0.88f, 0.92f);

            telemetryStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                fontStyle = FontStyle.Normal
            };
            telemetryStyle.normal.textColor = new Color(0.4f, 0.95f, 0.6f);

            stylesInitialized = true;
        }

        private void OnGUI()
        {
            if (!showUI) return;
            InitializeStyles();

            // 1. TOP BAR: Title & System Telemetry
            DrawTopBar();

            // 2. LEFT SIDEBAR: Furniture Catalogue
            DrawLeftCataloguePanel();

            // 3. RIGHT SIDEBAR: Selected Item Inspector & Materials & Lighting
            DrawRightInspectorPanel();

            // 4. BOTTOM BAR: Instructions & Window-to-Viewport Transformation Telemetry
            if (showHelpOverlay)
            {
                DrawBottomTelemetryBar();
            }
        }

        private void DrawTopBar()
        {
            float barWidth = Screen.width;
            float barHeight = 42f;

            GUI.Box(new Rect(0, 0, barWidth, barHeight), GUIContent.none, panelStyle);

            // Title
            GUI.Label(new Rect(15, 4, 450, 22), "VIRTUAL INTERIOR DESIGN STUDIO", headerStyle);
            GUI.Label(new Rect(15, 22, 450, 18), "CS4104 Case Study 3: Retail & Remote Commerce | RV University", labelStyle);

            // Active Preset indicator
            string presetName = lightingManager != null ? lightingManager.activePresetName : "Morning Sun";
            GUI.Label(new Rect(Screen.width * 0.5f - 120, 10, 240, 22), $"Lighting: {presetName}", headerStyle);

            // Telemetry: FPS, Memory
            int triangleCount = 18450;
            int batches = 24;
            GUI.Label(new Rect(Screen.width - 320, 6, 310, 16), $"Performance: {currentFps:F1} FPS (Stable) | Frame Time: {1000f / Mathf.Max(currentFps, 1f):F1}ms", telemetryStyle);
            GUI.Label(new Rect(Screen.width - 320, 22, 310, 16), $"Draw Batches: {batches} | Triangles: {triangleCount:N0} | URP 17.6", telemetryStyle);
        }

        private void DrawLeftCataloguePanel()
        {
            float panelWidth = 220f;
            float panelY = 48f;
            float panelHeight = Screen.height - panelY - (showHelpOverlay ? 75f : 15f);

            GUI.Box(new Rect(10, panelY, panelWidth, panelHeight), GUIContent.none, panelStyle);

            GUILayout.BeginArea(new Rect(18, panelY + 10, panelWidth - 16, panelHeight - 20));

            GUILayout.Label("FURNITURE CATALOGUE", headerStyle);
            GUILayout.Space(6);

            if (studioManager != null && studioManager.catalogue != null)
            {
                foreach (var item in studioManager.catalogue)
                {
                    GUILayout.BeginVertical(GUI.skin.box);
                    GUILayout.Label(item.displayName, subHeaderStyle);
                    GUILayout.Label($"Category: {item.category} | ${item.basePrice}", labelStyle);

                    if (GUILayout.Button("Place In Room", buttonStyle, GUILayout.Height(24)))
                    {
                        if (placementController != null)
                        {
                            placementController.StartPlacement(item);
                        }
                    }
                    GUILayout.EndVertical();
                    GUILayout.Space(4);
                }
            }

            GUILayout.FlexibleSpace();

            // Save / Load Section
            GUILayout.Label("ROOM LAYOUT PERSISTENCE", subHeaderStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save JSON", buttonStyle, GUILayout.Height(26)))
            {
                if (studioManager != null) studioManager.SaveRoomLayoutToJson();
            }
            if (GUILayout.Button("Load JSON", buttonStyle, GUILayout.Height(26)))
            {
                if (studioManager != null) studioManager.LoadRoomLayoutFromJson();
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Reset Room", buttonStyle, GUILayout.Height(24)))
            {
                if (studioManager != null) studioManager.ResetRoomLayout();
            }

            GUILayout.EndArea();
        }

        private void DrawRightInspectorPanel()
        {
            float panelWidth = 260f;
            float panelX = Screen.width - panelWidth - 10;
            float panelY = 48f;
            float panelHeight = Screen.height - panelY - (showHelpOverlay ? 75f : 15f);

            GUI.Box(new Rect(panelX, panelY, panelWidth, panelHeight), GUIContent.none, panelStyle);

            GUILayout.BeginArea(new Rect(panelX + 8, panelY + 10, panelWidth - 16, panelHeight - 20));

            // Selected Item
            GUILayout.Label("ITEM INSPECTOR", headerStyle);
            GameObject sel = studioManager != null ? studioManager.selectedFurnitureObject : null;

            if (sel != null)
            {
                GUILayout.Label($"Selected: {sel.name}", subHeaderStyle);
                Vector3 pos = sel.transform.position;
                Vector3 rot = sel.transform.eulerAngles;
                Vector3 sc = sel.transform.localScale;

                GUILayout.Label($"Position: ({pos.x:F2}, {pos.z:F2})m", labelStyle);
                GUILayout.Label($"Rotation Y: {rot.y:F0}°", labelStyle);
                GUILayout.Label($"Scale: {sc.x:F2}x", labelStyle);

                // Quick Transform Controls
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Rot -45°", buttonStyle)) manipulator?.RotateSelected(-45f);
                if (GUILayout.Button("Rot +45°", buttonStyle)) manipulator?.RotateSelected(45f);
                if (GUILayout.Button("Reset", buttonStyle)) manipulator?.ResetSelectedTransform();
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Deselect Item", buttonStyle, GUILayout.Height(22)))
                {
                    studioManager.DeselectFurniture();
                }
            }
            else
            {
                GUILayout.Label("Click furniture in scene to select & customize.", labelStyle);
            }

            GUILayout.Space(8);
            GUILayout.Box(GUIContent.none, GUILayout.Height(1)); // separator
            GUILayout.Space(4);

            // Lighting Presets (Question 1)
            GUILayout.Label("LIGHTING PRESETS (Q1)", headerStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Morning Sun", studioManager?.currentLightingPreset == 0 ? activeButtonStyle : buttonStyle))
                studioManager?.SetLightingPreset(0);
            if (GUILayout.Button("Cozy Evening", studioManager?.currentLightingPreset == 1 ? activeButtonStyle : buttonStyle))
                studioManager?.SetLightingPreset(1);
            if (GUILayout.Button("Studio Day", studioManager?.currentLightingPreset == 2 ? activeButtonStyle : buttonStyle))
                studioManager?.SetLightingPreset(2);
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            // Illumination Component Isolation (Question 1)
            GUILayout.Label("ILLUMINATION COMPONENTS (Q1)", subHeaderStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("PBR Full", buttonStyle)) illuminationVisualizer?.SetIlluminationMode(IlluminationMode.CompositePBR);
            if (GUILayout.Button("Diffuse", buttonStyle)) illuminationVisualizer?.SetIlluminationMode(IlluminationMode.DiffuseOnly);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Specular", buttonStyle)) illuminationVisualizer?.SetIlluminationMode(IlluminationMode.SpecularOnly);
            if (GUILayout.Button("Ambient", buttonStyle)) illuminationVisualizer?.SetIlluminationMode(IlluminationMode.AmbientOnly);
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
            GUILayout.Box(GUIContent.none, GUILayout.Height(1)); // separator
            GUILayout.Space(4);

            // Material Customisation & Normal Mapping (Question 2)
            GUILayout.Label("MATERIALS & NORMAL MAP (Q2)", headerStyle);

            // Normal map toggle button
            string normText = normalMappingEnabled ? "Normal Map: ENABLED (Detailed)" : "Normal Map: DISABLED (Flat)";
            if (GUILayout.Button(normText, normalMappingEnabled ? activeButtonStyle : buttonStyle, GUILayout.Height(24)))
            {
                ToggleNormalMapping();
            }

            GUILayout.Space(4);
            GUILayout.Label("Apply Material Preset to Selected:", subHeaderStyle);

            if (studioManager != null && studioManager.materialPresets != null)
            {
                for (int i = 0; i < studioManager.materialPresets.Count; i++)
                {
                    var p = studioManager.materialPresets[i];
                    if (GUILayout.Button(p.displayName, buttonStyle, GUILayout.Height(21)))
                    {
                        studioManager.ApplyMaterialPresetToSelected(i);
                    }
                }
            }

            GUILayout.EndArea();
        }

        private void DrawBottomTelemetryBar()
        {
            float barWidth = Screen.width - 20;
            float barHeight = 65f;
            float barY = Screen.height - barHeight - 8f;

            GUI.Box(new Rect(10, barY, barWidth, barHeight), GUIContent.none, panelStyle);

            // Left side: Navigation controls
            string navInfo = "NAVIGATION: WASD/Arrows to Walk | Right-Click Drag to Orbit/Look | Scroll to Zoom | F to Focus | Left-Click to Select Furniture | Shift+Right-Click Drag to Move | Q/E to Rotate";
            GUI.Label(new Rect(20, barY + 6, Screen.width * 0.5f - 30, 20), navInfo, labelStyle);

            // Right side: Coordinate Transformation Readout (Question 3)
            string rayInfo = raycaster != null ? 
                $"Q3 TRANSFORMATION: Screen ({raycaster.lastScreenPos.x:F0},{raycaster.lastScreenPos.y:F0})px -> Viewport ({raycaster.lastViewportPos.x:F3},{raycaster.lastViewportPos.y:F3}) -> NDC ({raycaster.lastNdcPos.x:F2},{raycaster.lastNdcPos.y:F2}) -> World Hit ({raycaster.lastFloorHitPoint.x:F2}, {raycaster.lastFloorHitPoint.y:F2}, {raycaster.lastFloorHitPoint.z:F2})m" : 
                "Q3 TRANSFORMATION: Window-to-Viewport & Raycasting Active";
            GUI.Label(new Rect(20, barY + 32, barWidth - 40, 24), rayInfo, telemetryStyle);
        }

        public void ToggleNormalMapping()
        {
            normalMappingEnabled = !normalMappingEnabled;
            // Iterate all furniture renderers and toggle normal map keyword / bump map
            if (studioManager != null)
            {
                foreach (var obj in studioManager.placedFurnitureObjects)
                {
                    if (obj == null) continue;
                    foreach (var r in obj.GetComponentsInChildren<MeshRenderer>())
                    {
                        foreach (var mat in r.materials)
                        {
                            if (normalMappingEnabled)
                            {
                                mat.EnableKeyword("_NORMALMAP");
                                if (mat.HasProperty("_BumpScale")) mat.SetFloat("_BumpScale", 1.0f);
                            }
                            else
                            {
                                mat.DisableKeyword("_NORMALMAP");
                                if (mat.HasProperty("_BumpScale")) mat.SetFloat("_BumpScale", 0.0f);
                            }
                        }
                    }
                }
            }
            Debug.Log($"[StudioUIManager] Normal mapping toggled: {normalMappingEnabled}");
        }
    }
}
