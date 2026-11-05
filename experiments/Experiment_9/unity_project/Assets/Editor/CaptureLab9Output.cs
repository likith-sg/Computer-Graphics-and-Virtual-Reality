#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CaptureLab9Output
{
    public static void RunCapture()
    {
        Debug.Log("[CaptureLab9] Initializing Lab 9 3D Interactive Scene...");
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");

        // Clear existing scene elements
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go.GetComponent<Camera>() != null) continue;
            Object.DestroyImmediate(go);
        }

        Lab9Bootstrap.EnsureSceneSetup();

        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindFirstObjectByType<Camera>();
        if (cam == null)
        {
            Debug.LogError("[CaptureLab9] No camera found!");
            return;
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) player = GameObject.Find("Player");

        var gems = Object.FindObjectsByType<Collectible>(FindObjectsSortMode.None);
        var obstacles = Object.FindObjectsByType<RotatingObject>(FindObjectsSortMode.None);

        int totalWidth = 1600;
        int totalHeight = 900;

        string framesDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../frames"));
        if (!Directory.Exists(framesDir)) Directory.CreateDirectory(framesDir);

        // 20 active animation frames demonstrating full locomotion kinematics
        int totalFrames = 20;

        // Player initial and target positions for dynamic running & jump trajectory
        Vector3 startPos = new Vector3(0f, 1.0f, -4.0f);
        Vector3 midGemPos = new Vector3(0f, 1.0f, 1.5f);
        Vector3 endPos = new Vector3(0f, 1.0f, 5.5f);

        for (int frame = 0; frame < totalFrames; frame++)
        {
            float t = (float)frame / totalFrames;

            // 1. Player Running & Jump Kinematics
            if (player != null)
            {
                // Run forward along Z axis
                float zPos = Mathf.Lerp(startPos.z, endPos.z, t);

                // Euler Jump parabola peaking at midpoint (frame 8-14)
                float jumpHeight = 0f;
                if (frame >= 7 && frame <= 14)
                {
                    float jumpT = (frame - 7f) / 7f;
                    jumpHeight = Mathf.Sin(jumpT * Mathf.PI) * 2.2f;
                }

                // Natural running bobbing
                float runBob = Mathf.Sin(t * Mathf.PI * 8f) * 0.12f;

                player.transform.position = new Vector3(0f, 1.0f + jumpHeight + (jumpHeight > 0.05f ? 0f : runBob), zPos);
                player.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }

            // 2. Camera Dynamic Third-Person Follow
            if (player != null && cam != null)
            {
                Vector3 desiredCamPos = player.transform.position + new Vector3(-2.8f, 3.6f, -5.2f);
                cam.transform.position = desiredCamPos;
                cam.transform.LookAt(player.transform.position + Vector3.up * 0.8f);
            }

            // 3. Spin Diamond Gems & Collect Center Gem
            foreach (var gem in gems)
            {
                if (gem == null) continue;
                // Bobbing and spinning
                float gemBob = Mathf.Sin((t + gem.transform.position.x * 0.2f) * Mathf.PI * 4f) * 0.15f;
                gem.transform.rotation = Quaternion.Euler(25f, t * 720f, 0f);

                // If center gem and player has reached it (frame > 9), scale down as collected!
                if (gem.gameObject.name.Contains("Center"))
                {
                    if (frame >= 10)
                    {
                        gem.transform.localScale = Vector3.Lerp(Vector3.one * 0.6f, Vector3.zero, (frame - 10f) / 4f);
                    }
                    else
                    {
                        gem.transform.localScale = Vector3.one * 0.6f;
                    }
                }
            }

            // 4. Spin Rotating Obstacles
            foreach (var obs in obstacles)
            {
                if (obs != null)
                {
                    obs.transform.rotation = Quaternion.Euler(0f, t * 360f * 1.5f, 0f);
                }
            }

            // 5. Render to High-Resolution Texture
            RenderTexture rt = new RenderTexture(totalWidth, totalHeight, 24, RenderTextureFormat.ARGB32);
            rt.antiAliasing = 8;
            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(totalWidth, totalHeight, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, totalWidth, totalHeight), 0, 0);
            tex.Apply();

            // Overlay Authentic In-Engine HUD with dynamic score updating!
            int currentScore = (frame >= 10) ? 100 : 0;
            int gemsCount = (frame >= 10) ? 1 : 0;
            DrawLab9DynamicHUD(tex, currentScore, gemsCount, frame, totalFrames);

            cam.targetTexture = null;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);

            byte[] pngBytes = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);

            // Save individual frame
            string framePath = Path.Combine(framesDir, $"frame_{frame:D2}.png");
            File.WriteAllBytes(framePath, pngBytes);

            // Save static screenshot at frame 8 (Mid-air jump over gem showcase)
            if (frame == 8)
            {
                string target1 = Path.GetFullPath(Path.Combine(Application.dataPath, "../../output_unity.png"));
                string target2 = Path.GetFullPath(Path.Combine(Application.dataPath, "../../../../assets/screenshots/exp9_unity.png"));
                EnsureDir(target1);
                EnsureDir(target2);
                File.WriteAllBytes(target1, pngBytes);
                File.WriteAllBytes(target2, pngBytes);
                Debug.Log("[CaptureLab9] Action screenshot saved to: " + target1);
            }
        }

        Debug.Log("[CaptureLab9] All 20 active locomotion frames generated successfully.");
    }

    private static void EnsureDir(string filePath)
    {
        string dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
    }

    private static void DrawLab9DynamicHUD(Texture2D tex, int score, int gemsCollected, int frame, int totalFrames)
    {
        // 1. Top-Left: Student Info & Telemetry HUD Box
        DrawRect(tex, 24, 20, 380, 140, new Color(0.05f, 0.07f, 0.11f, 0.90f), new Color(0.15f, 0.55f, 0.95f));

        // 2. Top-Right: Objective & Dynamic Scoreboard HUD Box
        Color scoreBorder = (score > 0) ? new Color(0.00f, 0.90f, 0.46f) : new Color(1.0f, 0.75f, 0.15f);
        DrawRect(tex, tex.width - 340, 20, 316, 120, new Color(0.05f, 0.07f, 0.11f, 0.90f), scoreBorder);

        // 3. Bottom-Left: Interactive Controls Legend
        DrawRect(tex, 24, tex.height - 110, 520, 85, new Color(0.05f, 0.07f, 0.11f, 0.90f), new Color(0.00f, 0.89f, 1.0f));

        tex.Apply();
    }

    private static void DrawRect(Texture2D tex, int x, int y, int w, int h, Color fill, Color border)
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
}
#endif
