#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CaptureLab10Output
{
    public static void RunCapture()
    {
        Debug.Log("[CaptureLab10] Initializing Experiment 10 Scene...");
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");

        // Clear scene
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(go);
        }

        // 1. Build Multi-Colored 6-Face Cube
        GameObject cubeRoot = new GameObject("ViewingPipeline_Cube");
        cubeRoot.transform.position = Vector3.zero;

        Mesh cubeMesh = BuildColouredCubeMesh();
        MeshFilter mf = cubeRoot.AddComponent<MeshFilter>();
        mf.sharedMesh = cubeMesh;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                     ?? Shader.Find("Universal Render Pipeline/Unlit")
                     ?? Shader.Find("Standard")
                     ?? Shader.Find("Unlit/Color");
        Material mat = new Material(shader);
        MeshRenderer mr = cubeRoot.AddComponent<MeshRenderer>();
        mr.sharedMaterial = mat;

        // 2. Build Studio Lighting
        GameObject keyLight = new GameObject("DirLight");
        Light l = keyLight.AddComponent<Light>();
        l.type = LightType.Directional;
        l.intensity = 1.2f;
        l.color = Color.white;
        keyLight.transform.rotation = Quaternion.Euler(45f, -30f, 0f);

        // 3. Build Dual Cameras
        // Shared eye position looking at origin (matching gluLookAt(4, 3, 6, 0, 0, 0))
        Vector3 eyePos = new Vector3(3.8f, 2.8f, 5.5f);

        // Orthographic Camera (Left Viewport)
        GameObject orthoGO = new GameObject("OrthoCam");
        Camera orthoCam = orthoGO.AddComponent<Camera>();
        orthoCam.orthographic = true;
        orthoCam.orthographicSize = 2.6f;
        orthoCam.nearClipPlane = 0.1f;
        orthoCam.farClipPlane = 40f;
        orthoCam.clearFlags = CameraClearFlags.SolidColor;
        orthoCam.backgroundColor = new Color(0.05f, 0.07f, 0.11f);
        orthoGO.transform.position = eyePos;
        orthoGO.transform.LookAt(Vector3.zero);

        // Perspective Camera (Right Viewport)
        GameObject perspGO = new GameObject("PerspCam");
        Camera perspCam = perspGO.AddComponent<Camera>();
        perspCam.orthographic = false;
        perspCam.fieldOfView = 48f;
        perspCam.nearClipPlane = 0.1f;
        perspCam.farClipPlane = 40f;
        perspCam.clearFlags = CameraClearFlags.SolidColor;
        perspCam.backgroundColor = new Color(0.06f, 0.09f, 0.13f);
        perspGO.transform.position = eyePos;
        perspGO.transform.LookAt(Vector3.zero);

        int totalWidth = 1600;
        int totalHeight = 900;
        int halfWidth = 800;

        string framesDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../frames"));
        if (!Directory.Exists(framesDir)) Directory.CreateDirectory(framesDir);

        int totalFrames = 16;
        for (int frame = 0; frame < totalFrames; frame++)
        {
            float angle = (float)frame / totalFrames * 360f;
            cubeRoot.transform.rotation = Quaternion.Euler(angle * 0.7f, angle, angle * 0.35f);

            // Render Left Half: Orthographic
            RenderTexture rtLeft = new RenderTexture(halfWidth, totalHeight, 24, RenderTextureFormat.ARGB32);
            rtLeft.antiAliasing = 8;
            orthoCam.targetTexture = rtLeft;
            orthoCam.Render();

            RenderTexture.active = rtLeft;
            Texture2D texLeft = new Texture2D(halfWidth, totalHeight, TextureFormat.RGB24, false);
            texLeft.ReadPixels(new Rect(0, 0, halfWidth, totalHeight), 0, 0);
            texLeft.Apply();

            // Render Right Half: Perspective
            RenderTexture rtRight = new RenderTexture(halfWidth, totalHeight, 24, RenderTextureFormat.ARGB32);
            rtRight.antiAliasing = 8;
            perspCam.targetTexture = rtRight;
            perspCam.Render();

            RenderTexture.active = rtRight;
            Texture2D texRight = new Texture2D(halfWidth, totalHeight, TextureFormat.RGB24, false);
            texRight.ReadPixels(new Rect(0, 0, halfWidth, totalHeight), 0, 0);
            texRight.Apply();

            // Composite Left + Right into Total 1600x900 Texture
            Texture2D finalTex = new Texture2D(totalWidth, totalHeight, TextureFormat.RGB24, false);

            Color[] leftPixels = texLeft.GetPixels();
            Color[] rightPixels = texRight.GetPixels();

            for (int y = 0; y < totalHeight; y++)
            {
                for (int x = 0; x < halfWidth; x++)
                {
                    finalTex.SetPixel(x, y, leftPixels[y * halfWidth + x]);
                }
                for (int x = 0; x < halfWidth; x++)
                {
                    finalTex.SetPixel(halfWidth + x, y, rightPixels[y * halfWidth + x]);
                }
            }

            // Cleanup RenderTextures
            orthoCam.targetTexture = null;
            perspCam.targetTexture = null;
            RenderTexture.active = null;
            Object.DestroyImmediate(rtLeft);
            Object.DestroyImmediate(rtRight);
            Object.DestroyImmediate(texLeft);
            Object.DestroyImmediate(texRight);

            // Draw center dividing line & mathematical HUD overlay
            DrawLab10Overlay(finalTex, angle);
            finalTex.Apply();

            byte[] pngBytes = finalTex.EncodeToPNG();
            Object.DestroyImmediate(finalTex);

            string framePath = Path.Combine(framesDir, $"frame_{frame:D2}.png");
            File.WriteAllBytes(framePath, pngBytes);

            if (frame == 0)
            {
                string target1 = Path.GetFullPath(Path.Combine(Application.dataPath, "../../output_unity.png"));
                string target2 = Path.GetFullPath(Path.Combine(Application.dataPath, "../../../../assets/screenshots/exp10_unity.png"));
                EnsureDir(target1);
                EnsureDir(target2);
                File.WriteAllBytes(target1, pngBytes);
                File.WriteAllBytes(target2, pngBytes);
                Debug.Log("[CaptureLab10] Static screenshot saved to: " + target1);
            }
        }

        Debug.Log("[CaptureLab10] All 16 frames generated successfully.");
    }

    private static void EnsureDir(string filePath)
    {
        string dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
    }

    private static Mesh BuildColouredCubeMesh()
    {
        Mesh mesh = new Mesh { name = "ColouredCube" };
        Vector3[] v = new Vector3[]
        {
            new Vector3(-1,-1,-1), new Vector3( 1,-1,-1), new Vector3( 1, 1,-1), new Vector3(-1, 1,-1), // Back
            new Vector3(-1,-1, 1), new Vector3( 1,-1, 1), new Vector3( 1, 1, 1), new Vector3(-1, 1, 1)  // Front
        };

        int[][] faces = new int[][]
        {
            new int[]{0,1,2,3}, // Back
            new int[]{4,5,6,7}, // Front
            new int[]{0,1,5,4}, // Bottom
            new int[]{2,3,7,6}, // Top
            new int[]{1,2,6,5}, // Right
            new int[]{0,3,7,4}  // Left
        };

        Color[] faceColors = new Color[]
        {
            new Color(0.20f, 0.75f, 0.35f), // Green (Back)
            new Color(0.95f, 0.20f, 0.20f), // Red (Front)
            new Color(0.95f, 0.85f, 0.15f), // Yellow (Bottom)
            new Color(0.20f, 0.50f, 0.95f), // Blue (Top)
            new Color(0.20f, 0.85f, 0.95f), // Cyan (Right)
            new Color(0.85f, 0.20f, 0.85f)  // Magenta (Left)
        };

        Vector3[] vertices = new Vector3[24];
        Color[] colors = new Color[24];
        int[] triangles = new int[36];

        int vIdx = 0, tIdx = 0;
        for (int f = 0; f < 6; f++)
        {
            int baseV = vIdx;
            int[] face = faces[f];

            vertices[vIdx] = v[face[0]]; colors[vIdx++] = faceColors[f];
            vertices[vIdx] = v[face[1]]; colors[vIdx++] = faceColors[f];
            vertices[vIdx] = v[face[2]]; colors[vIdx++] = faceColors[f];
            vertices[vIdx] = v[face[3]]; colors[vIdx++] = faceColors[f];

            triangles[tIdx++] = baseV + 0; triangles[tIdx++] = baseV + 2; triangles[tIdx++] = baseV + 1;
            triangles[tIdx++] = baseV + 0; triangles[tIdx++] = baseV + 3; triangles[tIdx++] = baseV + 2;
        }

        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    private static void DrawLab10Overlay(Texture2D tex, float angle)
    {
        int midX = tex.width / 2;

        // Draw vertical neon cyan divider line (width = 4px)
        for (int y = 0; y < tex.height; y++)
        {
            for (int dx = -2; dx <= 2; dx++)
            {
                tex.SetPixel(midX + dx, y, new Color(0.00f, 0.89f, 1.0f, 0.95f));
            }
        }

        // Left Header: Orthographic CAD Blueprint Mode
        DrawRect(tex, 24, 20, 480, 85, new Color(0.05f, 0.07f, 0.11f, 0.88f), new Color(0.00f, 0.90f, 0.46f));

        // Right Header: Perspective Human Eye Mode
        DrawRect(tex, midX + 24, 20, 480, 85, new Color(0.05f, 0.07f, 0.11f, 0.88f), new Color(0.95f, 0.35f, 0.15f));

        // Bottom Telemetry Bar: 3D Viewing Transformation Pipeline
        DrawRect(tex, 24, tex.height - 130, tex.width - 48, 110, new Color(0.05f, 0.07f, 0.11f, 0.90f), new Color(0.00f, 0.89f, 1.0f));
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
