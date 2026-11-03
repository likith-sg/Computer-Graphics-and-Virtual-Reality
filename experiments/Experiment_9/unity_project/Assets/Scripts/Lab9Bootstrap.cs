using UnityEngine;

/// <summary>
/// Auto-Bootstrapper for Experiment 9:
/// Automatically builds the full interactive 3D scene (arena, lighting, player, collectibles, obstacles, HUD)
/// if loaded in an empty scene.
/// </summary>
public static class Lab9Bootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void EnsureSceneSetup()
    {
        if (Object.FindFirstObjectByType<PlayerController>() != null) return;

        Debug.Log("[Lab 9] Auto-building interactive 3D scene...");

        // 1. Lighting
        GameObject lightGO = new GameObject("Directional Light");
        Light dirLight = lightGO.AddComponent<Light>();
        dirLight.type = LightType.Directional;
        dirLight.intensity = 1.2f;
        dirLight.color = new Color(0.95f, 0.95f, 0.90f);
        lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // 2. Checkerboard Arena Ground
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "ArenaGround";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(3f, 1f, 3f);
        ApplyCheckerboard(ground);

        // 3. Player Avatar
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 1f, 0f);

        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 2f; cc.radius = 0.5f; cc.center = Vector3.zero;

        // Torso
        GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Cube);
        torso.name = "PlayerTorso";
        Object.Destroy(torso.GetComponent<BoxCollider>());
        torso.transform.SetParent(player.transform, false);
        torso.transform.localScale = new Vector3(0.8f, 1.0f, 0.5f);
        ApplyColor(torso, new Color(0.20f, 0.55f, 0.95f));

        // Head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
        head.name = "PlayerHead";
        Object.Destroy(head.GetComponent<BoxCollider>());
        head.transform.SetParent(player.transform, false);
        head.transform.localPosition = new Vector3(0f, 0.75f, 0f);
        head.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
        ApplyColor(head, new Color(0.95f, 0.75f, 0.35f));

        // Visor
        GameObject visor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visor.name = "PlayerVisor";
        Object.Destroy(visor.GetComponent<BoxCollider>());
        visor.transform.SetParent(player.transform, false);
        visor.transform.localPosition = new Vector3(0f, 0.75f, 0.25f);
        visor.transform.localScale = new Vector3(0.35f, 0.15f, 0.10f);
        ApplyColor(visor, new Color(0.15f, 0.95f, 1.0f));

        player.AddComponent<PlayerController>();

        // 4. Follow Camera
        GameObject camGO = Camera.main != null ? Camera.main.gameObject : new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        Camera cam = camGO.GetComponent<Camera>() ?? camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.10f, 0.14f);
        camGO.transform.position = new Vector3(0f, 3.8f, -5.2f);
        camGO.transform.LookAt(player.transform);
        CameraFollow cf = camGO.GetComponent<CameraFollow>() ?? camGO.AddComponent<CameraFollow>();
        cf.target = player.transform;

        // 5. Rotating Obstacles
        CreateObstacle("Obstacle_NW", new Vector3(-6f, 1f,  6f), new Color(0.85f, 0.25f, 0.25f));
        CreateObstacle("Obstacle_NE", new Vector3( 6f, 1f,  6f), new Color(0.25f, 0.75f, 0.35f));
        CreateObstacle("Obstacle_SW", new Vector3(-6f, 1f, -6f), new Color(0.90f, 0.65f, 0.15f));
        CreateObstacle("Obstacle_SE", new Vector3( 6f, 1f, -6f), new Color(0.65f, 0.30f, 0.85f));

        // 6. Collectible Gems
        CreateCollectible("Gem_Center", new Vector3( 0f, 0.8f,  5f), new Color(1.0f, 0.85f, 0.1f));
        CreateCollectible("Gem_Left",   new Vector3(-8f, 0.8f,  0f), new Color(0.2f, 0.95f, 0.9f));
        CreateCollectible("Gem_Right",  new Vector3( 8f, 0.8f,  0f), new Color(0.95f, 0.3f, 0.7f));
        CreateCollectible("Gem_FarNW",  new Vector3(-9f, 0.8f,  9f), new Color(0.4f, 1.0f, 0.3f));
        CreateCollectible("Gem_FarSE",  new Vector3( 9f, 0.8f, -9f), new Color(1.0f, 0.45f, 0.2f));

        // 7. HUD
        GameObject hudGO = new GameObject("GameHUD");
        hudGO.AddComponent<GameHUD>();
    }

    private static void CreateObstacle(string name, Vector3 pos, Color col)
    {
        GameObject obs = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obs.name = name;
        obs.transform.position = pos;
        obs.transform.localScale = new Vector3(1.6f, 1.6f, 1.6f);
        ApplyColor(obs, col);
        obs.AddComponent<RotatingObject>();
    }

    private static void CreateCollectible(string name, Vector3 pos, Color col)
    {
        GameObject gem = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        gem.name = name;
        gem.transform.position = pos;
        gem.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        ApplyColor(gem, col);
        SphereCollider sc = gem.GetComponent<SphereCollider>();
        if (sc != null) sc.isTrigger = true;
        gem.AddComponent<Collectible>();
    }

    public static Material CreateMaterial(Color col)
    {
        Shader s = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard")
                ?? Shader.Find("Diffuse")
                ?? Shader.Find("Unlit/Color");
        Material mat = new Material(s);
        mat.color = col;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", col);
        return mat;
    }

    private static void ApplyColor(GameObject go, Color col)
    {
        Renderer r = go.GetComponent<Renderer>();
        if (r != null) r.material = CreateMaterial(col);
    }

    private static void ApplyCheckerboard(GameObject ground)
    {
        const int RES = 256; const int TILES = 18;
        Texture2D tex = new Texture2D(RES, RES, TextureFormat.RGB24, false);
        tex.filterMode = FilterMode.Point;
        Color dark1 = new Color(0.14f, 0.17f, 0.22f);
        Color dark2 = new Color(0.09f, 0.11f, 0.15f);
        int cellPx = RES / TILES;
        for (int y = 0; y < RES; y++)
            for (int x = 0; x < RES; x++)
                tex.SetPixel(x, y, ((x / cellPx) + (y / cellPx)) % 2 == 0 ? dark1 : dark2);
        tex.Apply();

        Shader s = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        Material mat = new Material(s);
        mat.mainTexture = tex;
        if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
        ground.GetComponent<Renderer>().material = mat;
    }
}

