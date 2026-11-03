using UnityEngine;

/// <summary>
/// Lab 10: 3D Viewing Pipeline — Orthographic vs Perspective Projection Demo
///
/// Complete 3D Viewing Pipeline Demonstration:
///   [Object Space] -> Model Matrix -> [World Space] -> View Matrix -> [Eye / Camera Space]
///         -> Projection Matrix -> [Clip Space] -> NDC -> [Screen Viewport]
///
/// Features:
///   - Left Viewport  (0.0 -> 0.5): Orthographic projection (glOrtho parallel lines)
///   - Right Viewport (0.5 -> 1.0): Perspective projection (gluPerspective foreshortening)
///   - Shared view camera position: eye=(4, 3, 6) looking at (0, 0, 0)
///   - 6-face coloured test cube rotating continuously
///   - Telemetry overlay showing Model, View, and Projection matrices
///
/// Student: Likith S G   USN: 1RVU23CSE236
/// Course:  Computer Graphics and Virtual Reality Laboratory
/// </summary>
[DisallowMultipleComponent]
public sealed class Lab10ViewingPipelineDemo : MonoBehaviour
{
    private Transform cubeRoot;
    private Camera orthoCamera, perspCamera;

    private bool  playing   = true;
    private bool  showInfo  = true;
    private float angle     = 35f;
    private float speed     = 30f;
    private float fov       = 45f;
    private float orthoSize = 3.2f;

    private GUIStyle _boxStyle, _titleStyle, _monoStyle, _infoStyle;
    private bool _stylesInitialized = false;

    // Cube definition matching Python CUBE_VERTICES
    private static readonly Vector3[] V =
    {
        new Vector3(-1,-1,-1), new Vector3( 1,-1,-1),
        new Vector3( 1, 1,-1), new Vector3(-1, 1,-1),
        new Vector3(-1,-1, 1), new Vector3( 1,-1, 1),
        new Vector3( 1, 1, 1), new Vector3(-1, 1, 1)
    };

    private static readonly int[][] Faces =
    {
        new[]{0,1,2,3}, // Back
        new[]{4,5,6,7}, // Front
        new[]{0,1,5,4}, // Bottom
        new[]{2,3,7,6}, // Top
        new[]{1,2,6,5}, // Right
        new[]{0,3,7,4}  // Left
    };

    private static readonly Color[] FaceColors =
    {
        new Color(0.2f, 0.7f, 0.3f), // Green (Back)
        new Color(0.9f, 0.2f, 0.2f), // Red (Front)
        new Color(0.9f, 0.8f, 0.2f), // Yellow (Bottom)
        new Color(0.2f, 0.4f, 0.9f), // Blue (Top)
        new Color(0.2f, 0.8f, 0.9f), // Cyan (Right)
        new Color(0.8f, 0.2f, 0.8f)  // Magenta (Left)
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoBootstrap()
    {
        if (FindFirstObjectByType<Lab10ViewingPipelineDemo>() == null)
        {
            var runner = new GameObject("Lab10_ViewingPipeline_Runner");
            runner.AddComponent<Lab10ViewingPipelineDemo>();
            Debug.Log("[Lab 10] Auto-bootstrapped 3D Viewing Pipeline demo.");
        }
    }

    private void Awake()
    {
        // Disable conflicting scene cameras
        foreach (var cam in FindObjectsByType<Camera>(FindObjectsSortMode.None))
            if (cam != orthoCamera && cam != perspCamera) cam.enabled = false;

        BuildCube();
        BuildCameras();
    }

    private void BuildCube()
    {
        cubeRoot = new GameObject("ViewingPipeline_Cube").transform;
        cubeRoot.position = Vector3.zero;

        Mesh mesh = new Mesh { name = "ColouredCubeMesh" };

        Vector3[] vertices = new Vector3[24];
        Color[]   colors   = new Color[24];
        int[]     triangles = new int[36];

        int vIdx = 0, tIdx = 0;
        for (int f = 0; f < 6; f++)
        {
            int baseV = vIdx;
            int[] face = Faces[f];
            Color c = FaceColors[f];

            vertices[vIdx++] = V[face[0]];
            vertices[vIdx++] = V[face[1]];
            vertices[vIdx++] = V[face[2]];
            vertices[vIdx++] = V[face[3]];

            for (int k = baseV; k < baseV + 4; k++) colors[k] = c;

            // Two triangles per face
            triangles[tIdx++] = baseV;
            triangles[tIdx++] = baseV + 2;
            triangles[tIdx++] = baseV + 1;

            triangles[tIdx++] = baseV;
            triangles[tIdx++] = baseV + 3;
            triangles[tIdx++] = baseV + 2;
        }

        mesh.vertices  = vertices;
        mesh.colors    = colors;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        Shader s = Shader.Find("CGVR/Lab10/FaceColors")
                ?? Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Sprites/Default")
                ?? Shader.Find("Unlit/Color");
        Material mat = new Material(s);

        MeshFilter mf = cubeRoot.gameObject.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;

        MeshRenderer mr = cubeRoot.gameObject.AddComponent<MeshRenderer>();
        mr.sharedMaterial = mat;
    }

    private void BuildCameras()
    {
        Vector3 eyePos = new Vector3(4f, 3f, 6f);

        // Left viewport: Orthographic
        GameObject goOrtho = new GameObject("Camera_Left_Orthographic");
        orthoCamera = goOrtho.AddComponent<Camera>();
        orthoCamera.rect = new Rect(0f, 0f, 0.5f, 1f);
        orthoCamera.orthographic = true;
        orthoCamera.orthographicSize = orthoSize;
        orthoCamera.nearClipPlane = 0.1f;
        orthoCamera.farClipPlane = 50f;
        orthoCamera.clearFlags = CameraClearFlags.SolidColor;
        orthoCamera.backgroundColor = new Color(0.06f, 0.08f, 0.12f);
        goOrtho.transform.position = eyePos;
        goOrtho.transform.LookAt(Vector3.zero);

        // Right viewport: Perspective
        GameObject goPersp = new GameObject("Camera_Right_Perspective");
        perspCamera = goPersp.AddComponent<Camera>();
        perspCamera.rect = new Rect(0.5f, 0f, 0.5f, 1f);
        perspCamera.orthographic = false;
        perspCamera.fieldOfView = fov;
        perspCamera.nearClipPlane = 0.1f;
        perspCamera.farClipPlane = 50f;
        perspCamera.clearFlags = CameraClearFlags.SolidColor;
        perspCamera.backgroundColor = new Color(0.08f, 0.10f, 0.15f);
        goPersp.transform.position = eyePos;
        goPersp.transform.LookAt(Vector3.zero);

        goOrtho.AddComponent<AudioListener>();
    }

    private void Update()
    {
        // Space: Pause/resume
        if (Input.GetKeyDown(KeyCode.Space)) playing = !playing;

        // R: Reset
        if (Input.GetKeyDown(KeyCode.R))
        {
            angle = 35f; speed = 30f; fov = 45f; orthoSize = 3.2f; playing = true;
            if (perspCamera) perspCamera.fieldOfView = fov;
            if (orthoCamera) orthoCamera.orthographicSize = orthoSize;
        }

        // H: Toggle info
        if (Input.GetKeyDown(KeyCode.H)) showInfo = !showInfo;

        // Speed adjustment
        if (Input.GetKey(KeyCode.UpArrow))   speed += 15f * Time.deltaTime;
        if (Input.GetKey(KeyCode.DownArrow)) speed = Mathf.Max(0f, speed - 15f * Time.deltaTime);

        // FOV / Ortho zoom
        if (Input.GetKey(KeyCode.RightArrow))
        {
            fov = Mathf.Clamp(fov + 20f * Time.deltaTime, 15f, 90f);
            orthoSize = Mathf.Clamp(orthoSize + 1.5f * Time.deltaTime, 1f, 8f);
            if (perspCamera) perspCamera.fieldOfView = fov;
            if (orthoCamera) orthoCamera.orthographicSize = orthoSize;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            fov = Mathf.Clamp(fov - 20f * Time.deltaTime, 15f, 90f);
            orthoSize = Mathf.Clamp(orthoSize - 1.5f * Time.deltaTime, 1f, 8f);
            if (perspCamera) perspCamera.fieldOfView = fov;
            if (orthoCamera) orthoCamera.orthographicSize = orthoSize;
        }

        if (playing && cubeRoot != null)
        {
            angle += speed * Time.deltaTime;
            cubeRoot.rotation = Quaternion.Euler(angle * 0.7f, angle, angle * 0.3f);
        }
    }

    private void InitStyles()
    {
        if (_stylesInitialized) return;

        Texture2D darkBg = new Texture2D(1, 1);
        darkBg.SetPixel(0, 0, new Color(0.06f, 0.08f, 0.11f, 0.88f));
        darkBg.Apply();

        _boxStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = darkBg, textColor = Color.white },
            padding = new RectOffset(10, 10, 8, 8)
        };

        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.35f, 0.85f, 1f) }
        };

        _monoStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11,
            normal = { textColor = new Color(0.85f, 0.95f, 0.75f) }
        };

        _infoStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11,
            normal = { textColor = new Color(0.85f, 0.85f, 0.85f) }
        };

        _stylesInitialized = true;
    }

    private void OnGUI()
    {
        InitStyles();

        // Viewport Header Labels
        float halfW = Screen.width * 0.5f;

        // Left Header: Orthographic
        GUI.Box(new Rect(10, 10, halfW - 20, 36), GUIContent.none, _boxStyle);
        GUI.Label(new Rect(20, 14, halfW - 40, 26),
            string.Format("LEFT: ORTHOGRAPHIC PROJECTION (glOrtho) | Size: {0:F1}", orthoSize), _titleStyle);

        // Right Header: Perspective
        GUI.Box(new Rect(halfW + 10, 10, halfW - 20, 36), GUIContent.none, _boxStyle);
        GUI.Label(new Rect(halfW + 20, 14, halfW - 40, 26),
            string.Format("RIGHT: PERSPECTIVE PROJECTION (gluPerspective) | FOV: {0:F0}°", fov), _titleStyle);

        if (!showInfo) return;

        // Bottom Telemetry card
        float cardW = Mathf.Min(Screen.width - 40, 680);
        float cardH = 150;
        Rect cardRect = new Rect((Screen.width - cardW) * 0.5f, Screen.height - cardH - 15, cardW, cardH);

        GUILayout.BeginArea(cardRect, _boxStyle);
        GUILayout.Label("Experiment 10: 3D Viewing Pipeline  |  Likith S G (1RVU23CSE236)", _titleStyle);
        GUILayout.Label(string.Format("Cube Rotation: {0:F1}° (Speed: {1:F0}°/s)  |  Status: {2}",
            angle % 360f, speed, playing ? "RUNNING" : "PAUSED"), _infoStyle);

        GUILayout.Space(4);
        GUILayout.Label("3D Pipeline Stage: Object Space -> World Space -> Eye Space -> Clip Space -> Screen Viewport", _monoStyle);
        GUILayout.Label("View Transform: Eye=(4, 3, 6) -> Target=(0,0,0)  |  Parallel vs Converging Rays", _infoStyle);

        GUILayout.Space(4);
        GUILayout.Label("Controls: Space: Pause | Up/Down: Speed | Left/Right: FOV & Size | R: Reset | H: Hide UI", _monoStyle);
        GUILayout.EndArea();
    }
}

