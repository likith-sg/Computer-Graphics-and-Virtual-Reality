#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CaptureAutoVRUnityOutputs
{
    [MenuItem("AutoVR/Capture/Generate Authentic Unity 6 Scene Outputs")]
    public static void RunCapture()
    {
        Debug.Log("[CaptureAutoVR] Opening AutoVR Showroom Scene in Unity 6...");
        const string scenePath = "Assets/Scenes/AutoVR_Showroom.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        var carSelector = UnityEngine.Object.FindAnyObjectByType<CarSelector>();
        var envManager = UnityEngine.Object.FindAnyObjectByType<ShowroomEnvironmentManager>();
        var lightingManager = UnityEngine.Object.FindAnyObjectByType<ShowroomLightingManager>();
        var painter = UnityEngine.Object.FindAnyObjectByType<VehiclePainter>();
        var camera = Camera.main;
        if (camera == null) camera = UnityEngine.Object.FindAnyObjectByType<Camera>();

        if (envManager != null)
        {
            envManager.BuildShowroomArchitecture();
            Debug.Log("[CaptureAutoVR] Luxury Showroom architecture built.");
        }

        if (carSelector != null)
        {
            carSelector.ShowCar(0); // 0 = Sports Coupe
            Debug.Log("[CaptureAutoVR] Selected vehicle: Sports Coupe.");
        }

        if (painter != null)
        {
            painter.SetBodyColor(4);    // Racing Red
            painter.SetPaintFinish(1);  // Metallic
            painter.SetWheelColor(1);   // Polished Silver
            painter.SetUnderglowColor(0); // Cyan Underglow
            Debug.Log("[CaptureAutoVR] Metallic Racing Red and alloy finish configured.");
        }

        if (lightingManager != null)
        {
            lightingManager.SetLightingStyle(0); // Bengaluru Studio
            Debug.Log("[CaptureAutoVR] Studio lighting preset applied.");
        }

        Vector3 targetCenter = new Vector3(0f, 0.45f, 0f);
        if (carSelector != null && carSelector.CurrentCar != null)
        {
            var bounds = CalculateBounds(carSelector.CurrentCar);
            targetCenter = bounds.center;
        }

        int width = 1600;
        int height = 900;
        int totalFrames = 30;

        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string framesDir = Path.Combine(projectRoot, "frames");
        if (Directory.Exists(framesDir)) Directory.Delete(framesDir, true);
        Directory.CreateDirectory(framesDir);

        Debug.Log($"[CaptureAutoVR] Rendering {totalFrames} in-engine Unity 6 frames at {width}x{height}...");

        float baseDistance = 5.6f;
        float camHeight = 1.65f;

        for (int frame = 0; frame < totalFrames; frame++)
        {
            float t = (float)frame / totalFrames;
            // 360 degree smooth orbital spin around vehicle
            float angleDeg = t * 360f + 210f;
            float rad = angleDeg * Mathf.Deg2Rad;

            Vector3 camPos = targetCenter + new Vector3(Mathf.Sin(rad) * baseDistance, camHeight + Mathf.Sin(t * Mathf.PI * 2f) * 0.15f, Mathf.Cos(rad) * baseDistance);
            camera.transform.position = camPos;
            camera.transform.LookAt(targetCenter + Vector3.up * 0.25f);

            RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            rt.antiAliasing = 8;
            camera.targetTexture = rt;
            camera.Render();

            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            // Overlay in-engine RVU Collaborative CAD & Telemetry UI
            DrawInEngineShowroomHUD(tex, frame, totalFrames);

            camera.targetTexture = null;
            RenderTexture.active = null;
            UnityEngine.Object.DestroyImmediate(rt);

            byte[] pngBytes = tex.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(tex);

            string framePath = Path.Combine(framesDir, $"frame_{frame:D2}.png");
            File.WriteAllBytes(framePath, pngBytes);

            // Save static hero render at frame 0 (Front-3/4 angle)
            if (frame == 0)
            {
                string heroPath1 = Path.Combine(projectRoot, "../output.png");
                string heroPath2 = Path.Combine(projectRoot, "../screenshot.png");
                string heroPath3 = Path.Combine(projectRoot, "../../assets/screenshots/case_study_2.png");

                SaveSafe(heroPath1, pngBytes);
                SaveSafe(heroPath2, pngBytes);
                SaveSafe(heroPath3, pngBytes);
                Debug.Log($"[CaptureAutoVR] Hero static Unity 6 render saved to: {heroPath1}");
            }
        }

        Debug.Log($"[CaptureAutoVR] All {totalFrames} authentic Unity 6 frames captured to {framesDir}");
    }

    private static void SaveSafe(string path, byte[] data)
    {
        string full = Path.GetFullPath(path);
        string dir = Path.GetDirectoryName(full);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
        File.WriteAllBytes(full, data);
    }

    private static Bounds CalculateBounds(GameObject obj)
    {
        var renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(obj.transform.position, Vector3.one * 2f);
        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
        return b;
    }

    private static void DrawInEngineShowroomHUD(Texture2D tex, int frame, int totalFrames)
    {
        int w = tex.width;
        int h = tex.height;

        // 1. Top Modern Floating Navigation Bar (Dark Glass)
        DrawGlassRect(tex, 20, 20, w - 40, 56, new Color(0.04f, 0.06f, 0.09f, 0.88f), new Color(0.00f, 0.85f, 1.0f, 0.85f));

        // 2. Top-Right CAD Matrix & Optimization Telemetry Card (CS4104 Case Study 2)
        DrawGlassRect(tex, w - 460, 88, 440, 160, new Color(0.04f, 0.06f, 0.09f, 0.88f), new Color(1.0f, 0.75f, 0.15f, 0.85f));

        // 3. Bottom Customization Dock (18 Swatches)
        DrawGlassRect(tex, 20, h - 85, w - 40, 65, new Color(0.04f, 0.06f, 0.09f, 0.88f), new Color(0.15f, 0.20f, 0.28f, 0.90f));

        // Draw 18 Circular Color Swatches inside Bottom Dock
        Color[] palette = new Color[]
        {
            new Color(0.96f, 0.96f, 0.98f), new Color(0.08f, 0.08f, 0.09f), new Color(0.25f, 0.27f, 0.30f), new Color(0.82f, 0.84f, 0.86f),
            new Color(0.85f, 0.12f, 0.12f), new Color(0.55f, 0.00f, 0.09f), new Color(1.00f, 0.36f, 0.12f), new Color(1.00f, 0.80f, 0.00f),
            new Color(0.08f, 0.30f, 0.80f), new Color(0.22f, 0.72f, 1.00f), new Color(0.00f, 0.65f, 0.40f), new Color(0.12f, 0.32f, 0.18f),
            new Color(0.45f, 0.08f, 0.72f), new Color(0.75f, 0.45f, 0.50f), new Color(0.97f, 0.91f, 0.81f), new Color(0.58f, 0.38f, 0.20f),
            new Color(0.00f, 0.55f, 0.55f), new Color(0.95f, 0.15f, 0.45f)
        };

        int startX = 60;
        int dockY = h - 52;
        int spacing = (w - 380) / palette.Length;

        for (int i = 0; i < palette.Length; i++)
        {
            int cx = startX + i * spacing;
            bool selected = (i == 4); // Racing Red
            DrawCircle(tex, cx, dockY, 10, palette[i], selected ? Color.white : new Color(0.3f, 0.3f, 0.3f));
        }

        tex.Apply();
    }

    private static void DrawGlassRect(Texture2D tex, int x, int y, int w, int h, Color fill, Color border)
    {
        int texH = tex.height;
        int startY = texH - y - h;

        for (int px = x; px < x + w && px < tex.width; px++)
        {
            for (int py = startY; py < startY + h && py < texH; py++)
            {
                if (px < 0 || py < 0) continue;
                bool isBorder = (px == x || px == x + w - 1 || py == startY || py == startY + h - 1 ||
                                 px == x + 1 || px == x + w - 2 || py == startY + 1 || py == startY + h - 2);

                Color current = tex.GetPixel(px, py);
                Color blended = isBorder ? border : Color.Lerp(current, fill, fill.a);
                tex.SetPixel(px, py, blended);
            }
        }
    }

    private static void DrawCircle(Texture2D tex, int cx, int cy, int radius, Color col, Color ringCol)
    {
        int texH = tex.height;
        int actY = texH - cy;

        for (int x = cx - radius - 2; x <= cx + radius + 2; x++)
        {
            for (int y = actY - radius - 2; y <= actY + radius + 2; y++)
            {
                if (x < 0 || x >= tex.width || y < 0 || y >= tex.height) continue;
                float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - actY) * (y - actY));
                if (d <= radius)
                {
                    tex.SetPixel(x, y, col);
                }
                else if (d <= radius + 2.0f)
                {
                    tex.SetPixel(x, y, ringCol);
                }
            }
        }
    }
}
#endif
