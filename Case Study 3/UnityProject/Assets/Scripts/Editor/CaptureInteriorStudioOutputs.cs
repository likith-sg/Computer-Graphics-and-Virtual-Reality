#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using InteriorStudio.Core;
using InteriorStudio.Environment;
using InteriorStudio.Interaction;
using InteriorStudio.UI;

namespace InteriorStudio.Editor
{
    public static class CaptureInteriorStudioOutputs
    {
        [MenuItem("InteriorStudio/Capture/Generate Authentic Unity 6 Scene Outputs")]
        public static void RunCapture()
        {
            Debug.Log("[CaptureInteriorStudio] Starting authentic Unity 6 scene capture...");

            // First ensure assets and master scene are built
            StudioAssetGenerator.GenerateAllAndSetupScene();

            const string scenePath = "Assets/Scenes/InteriorStudio.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            var studioManager = UnityEngine.Object.FindAnyObjectByType<StudioManager>();
            var lightingManager = UnityEngine.Object.FindAnyObjectByType<LightingPresetManager>();
            var visualizer = UnityEngine.Object.FindAnyObjectByType<IlluminationVisualizer>();
            var ui = UnityEngine.Object.FindAnyObjectByType<StudioUIManager>();
            var camera = Camera.main;
            if (camera == null) camera = UnityEngine.Object.FindAnyObjectByType<Camera>();

            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string cs3Root = Path.GetFullPath(Path.Combine(projectRoot, ".."));
            string screenshotsDir = Path.Combine(cs3Root, "Screenshots");
            string figuresDir = Path.Combine(cs3Root, "Report", "Figures");
            string framesDir = Path.Combine(projectRoot, "frames");

            EnsureDirectory(screenshotsDir);
            EnsureDirectory(figuresDir);
            if (Directory.Exists(framesDir)) Directory.Delete(framesDir, true);
            Directory.CreateDirectory(framesDir);

            int width = 1600;
            int height = 900;

            // SCENARIO 1: HERO INTERIOR (Wide Scandinavian Living Room in Morning Sunlight)
            lightingManager?.ApplyPreset(0);
            camera.transform.position = new Vector3(-2.85f, 1.70f, -2.15f);
            camera.transform.LookAt(new Vector3(0.15f, 0.65f, 0.55f));
            byte[] heroBytes = RenderCameraView(camera, width, height);
            SaveImage(heroBytes, Path.Combine(screenshotsDir, "01_hero_interior_studio.png"));
            SaveImage(heroBytes, Path.Combine(figuresDir, "fig1_hero_interior.png"));
            SaveImage(heroBytes, Path.Combine(cs3Root, "output.png"));
            SaveImage(heroBytes, Path.Combine(cs3Root, "screenshot.png"));
            Debug.Log("[CaptureInteriorStudio] Captured Hero Interior view.");

            // SCENARIO 2: RED VELVET CHAIR (Question 1 - Illumination & Velvet Material)
            camera.transform.position = new Vector3(-1.15f, 1.05f, -1.65f);
            camera.transform.LookAt(new Vector3(-1.45f, 0.55f, -0.45f));
            byte[] chairBytes = RenderCameraView(camera, width, height);
            SaveImage(chairBytes, Path.Combine(screenshotsDir, "02_red_velvet_chair_illumination.png"));
            SaveImage(chairBytes, Path.Combine(figuresDir, "fig2_red_velvet_chair.png"));
            Debug.Log("[CaptureInteriorStudio] Captured Red Velvet Chair Illumination closeup.");

            // SCENARIO 3: LEATHER SOFA WITH NORMAL MAPPING (Question 2 - Tangent Space Normal Map Detail)
            camera.transform.position = new Vector3(-0.85f, 1.15f, -0.15f);
            camera.transform.LookAt(new Vector3(0f, 0.55f, 1.2f));
            byte[] sofaBytes = RenderCameraView(camera, width, height);
            SaveImage(sofaBytes, Path.Combine(screenshotsDir, "03_leather_sofa_normal_map.png"));
            SaveImage(sofaBytes, Path.Combine(figuresDir, "fig3_leather_sofa_normal.png"));
            Debug.Log("[CaptureInteriorStudio] Captured Leather Sofa Normal Map closeup.");

            // SCENARIO 4: COZY GOLDEN EVENING LIGHTING PRESET
            lightingManager?.ApplyPreset(1);
            camera.transform.position = new Vector3(-2.45f, 1.65f, -1.85f);
            camera.transform.LookAt(new Vector3(0.2f, 0.7f, 0.6f));
            byte[] eveningBytes = RenderCameraView(camera, width, height);
            SaveImage(eveningBytes, Path.Combine(screenshotsDir, "04_cozy_evening_lighting.png"));
            SaveImage(eveningBytes, Path.Combine(figuresDir, "fig4_cozy_evening.png"));
            Debug.Log("[CaptureInteriorStudio] Captured Cozy Evening lighting preset.");

            // SCENARIO 5: NEUTRAL STUDIO DAYLIGHT PRESET
            lightingManager?.ApplyPreset(2);
            byte[] studioDayBytes = RenderCameraView(camera, width, height);
            SaveImage(studioDayBytes, Path.Combine(screenshotsDir, "05_studio_daylight_lighting.png"));
            SaveImage(studioDayBytes, Path.Combine(figuresDir, "fig5_studio_daylight.png"));
            Debug.Log("[CaptureInteriorStudio] Captured Neutral Studio Daylight preset.");

            // SCENARIO 6: FURNITURE INTERACTION & SELECTION (Question 3 - Window-to-Viewport Raycast Telemetry)
            lightingManager?.ApplyPreset(0);
            camera.transform.position = new Vector3(-2.2f, 1.45f, -1.5f);
            camera.transform.LookAt(new Vector3(-0.5f, 0.5f, 0.3f));
            byte[] interactBytes = RenderCameraView(camera, width, height);
            SaveImage(interactBytes, Path.Combine(screenshotsDir, "06_furniture_selection_interaction.png"));
            SaveImage(interactBytes, Path.Combine(figuresDir, "fig6_interaction_raycasting.png"));
            Debug.Log("[CaptureInteriorStudio] Captured Furniture Interaction & Raycasting view.");

            // SCENARIO 7: MATERIAL CUSTOMISATION PALETTE
            camera.transform.position = new Vector3(-1.2f, 1.15f, -0.6f);
            camera.transform.LookAt(new Vector3(-0.3f, 0.45f, 0.25f));
            byte[] materialBytes = RenderCameraView(camera, width, height);
            SaveImage(materialBytes, Path.Combine(screenshotsDir, "07_material_customisation.png"));
            SaveImage(materialBytes, Path.Combine(figuresDir, "fig7_material_palette.png"));
            Debug.Log("[CaptureInteriorStudio] Captured Material Customisation view.");

            // SCENARIO 8: SAVE & LOAD JSON PERSISTENCE
            studioManager?.SaveRoomLayoutToJson();
            byte[] saveBytes = RenderCameraView(camera, width, height);
            SaveImage(saveBytes, Path.Combine(screenshotsDir, "08_save_load_persistence.png"));
            SaveImage(saveBytes, Path.Combine(figuresDir, "fig8_save_load.png"));
            Debug.Log("[CaptureInteriorStudio] Captured Save/Load layout view.");

            // SCENARIO 9: 30 ORBITAL FRAMES FOR ANIMATED DEMO GIF
            int totalFrames = 30;
            Vector3 center = new Vector3(0f, 0.55f, 0.45f);
            float orbitRadius = 3.65f;
            float orbitHeight = 1.65f;

            Debug.Log($"[CaptureInteriorStudio] Rendering {totalFrames} orbital frames for animated turntable GIF...");
            for (int f = 0; f < totalFrames; f++)
            {
                float t = (float)f / totalFrames;
                float angle = t * Mathf.PI * 2f + Mathf.PI * 0.75f;

                Vector3 camPos = center + new Vector3(Mathf.Sin(angle) * orbitRadius, orbitHeight + Mathf.Sin(t * Mathf.PI * 2f) * 0.2f, Mathf.Cos(angle) * orbitRadius);
                // Clamp within room
                camPos.x = Mathf.Clamp(camPos.x, -3.5f, 3.5f);
                camPos.z = Mathf.Clamp(camPos.z, -2.5f, 2.5f);

                camera.transform.position = camPos;
                camera.transform.LookAt(center);

                byte[] frameBytes = RenderCameraView(camera, 1280, 720);
                string framePath = Path.Combine(framesDir, $"frame_{f:D2}.png");
                File.WriteAllBytes(framePath, frameBytes);
            }

            Debug.Log($"[CaptureInteriorStudio] ALL CAPTURES COMPLETE! {totalFrames} frames and all screenshots generated.");
        }

        private static byte[] RenderCameraView(Camera cam, int width, int height)
        {
            RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            rt.antiAliasing = 8;
            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            cam.targetTexture = null;
            RenderTexture.active = null;
            UnityEngine.Object.DestroyImmediate(rt);

            byte[] bytes = tex.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(tex);
            return bytes;
        }

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }

        private static void SaveImage(byte[] data, string path)
        {
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllBytes(path, data);
        }
    }
}
#endif
