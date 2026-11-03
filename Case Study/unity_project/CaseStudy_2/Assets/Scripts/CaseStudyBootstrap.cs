using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Case Study 2 — High-Polish Forza-Style VR Automotive Studio Bootstrapper
/// 
/// Constructs:
///   1. Multi-element automotive studio lighting (overhead LED grid + 3-point key/fill/rim + rim kickers)
///   2. Architectural showroom environment with illuminated wall light battens and reflective floor
///   3. Dual-tier neon illuminated showroom turntable platform
///   4. 5 distinct, highly sculpted procedural vehicles with Coke-bottle silhouettes, flared haunches,
///      angled aerodynamic splitters, louvers, glass fastbacks, and multi-element GT wings
///   5. High-fidelity modular alloy wheels with concave barrels, multi-piston calipers, and ceramic rotors
///
/// Student: Likith S G   USN: 1RVU23CSE236
/// Course:  Computer Graphics and Virtual Reality Laboratory
/// </summary>
public static class CaseStudyBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void EnsureSceneSetup()
    {
        CleanPreviousInstances();
        Debug.Log("[CaseStudy2] Building High-Polish Forza-Style Automotive Studio...");

        BuildStudioLighting();
        BuildStudioEnvironment();

        GameObject turntablePivot = BuildShowroomTurntable();
        VRCollaborativeCarReview ctrl = BuildGarageController(turntablePivot);

        BuildCameraRig(ctrl);

        GameObject telGO = new GameObject("CaseStudy_Telemetry");
        ctrl.telemetry = telGO.AddComponent<VROptimizationTelemetry>();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SCENE CLEANUP (Fast, non-blocking single pass)
    // ─────────────────────────────────────────────────────────────────────────
    private static void CleanPreviousInstances()
    {
        Camera existingCam = Camera.main;
        if (existingCam == null) existingCam = Object.FindFirstObjectByType<Camera>();
        if (existingCam != null)
        {
            existingCam.transform.SetParent(null);
            existingCam.enabled = true;
            existingCam.targetDisplay = 0;
            existingCam.gameObject.name = "Main Camera";
        }

        string[] targets = {
            "Studio_KeyLight", "Studio_FillLight", "Studio_RimLight", "Studio_OverheadLightGrid",
            "Studio_Floor", "Studio_BackWall", "Studio_WallAccentStrip", "Studio_LightBatten",
            "Showroom_TurntablePivot", "Showroom_Turntable",
            "Avatar_Bengaluru_LeadDesigner", "Avatar_Detroit_AeroEngineer",
            "Studio_CameraRig", "CaseStudy_Telemetry", "VRCollaborativeCarReview",
            "CarPrototype_1to1Scale", "VehicleRoot"
        };
        HashSet<string> targetSet = new HashSet<string>(targets);
        GameObject[] allSceneObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject go in allSceneObjects)
        {
            if (go != null && (targetSet.Contains(go.name) || go.name.StartsWith("Studio_LightBatten")))
            {
                go.name = "_Destroying_";
                if (Application.isPlaying) Object.Destroy(go);
                else Object.DestroyImmediate(go);
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STUDIO LIGHTING  (Overhead LED grid + Key / Fill / Rim / Floor kickers)
    // ─────────────────────────────────────────────────────────────────────────
    private static void BuildStudioLighting()
    {
        MakeDirectionalLight("Studio_KeyLight",  1.45f, new Color(0.98f, 0.98f, 1.00f), 48f, -35f);
        MakeDirectionalLight("Studio_FillLight", 0.55f, new Color(0.85f, 0.92f, 1.00f), 30f, 145f);
        MakeDirectionalLight("Studio_RimLight",  0.48f, new Color(0.80f, 0.90f, 1.00f), 18f,-165f);

        // Overhead geometric LED light grid (creates the distinctive Forza Horizon ceiling reflection streaks)
        GameObject gridRoot = new GameObject("Studio_OverheadLightGrid");
        gridRoot.transform.position = new Vector3(0, 5.2f, 0);

        // 4 longitudinal light bars
        for (int i = -2; i <= 2; i++)
        {
            if (i == 0) continue;
            GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bar.name = "LightBar_Longitudinal_" + i;
            bar.transform.SetParent(gridRoot.transform, false);
            bar.transform.localPosition = new Vector3(i * 1.35f, 0, 0);
            bar.transform.localScale = new Vector3(0.12f, 0.04f, 8.5f);
            Object.Destroy(bar.GetComponent<Collider>());
            ApplyEmissive(bar, new Color(0.96f, 0.98f, 1.0f), 3.2f);
        }

        // 3 lateral light bars across
        for (int z = -1; z <= 1; z++)
        {
            GameObject cross = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cross.name = "LightBar_Lateral_" + z;
            cross.transform.SetParent(gridRoot.transform, false);
            cross.transform.localPosition = new Vector3(0, 0.02f, z * 3.0f);
            cross.transform.localScale = new Vector3(5.8f, 0.04f, 0.12f);
            Object.Destroy(cross.GetComponent<Collider>());
            ApplyEmissive(cross, new Color(0.96f, 0.98f, 1.0f), 3.0f);
        }
    }

    private static void MakeDirectionalLight(string n, float intensity, Color col, float pitch, float yaw)
    {
        GameObject go = new GameObject(n);
        Light l = go.AddComponent<Light>();
        l.type = LightType.Directional;
        l.intensity = intensity;
        l.color = col;
        l.shadows = LightShadows.Soft;
        go.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STUDIO ENVIRONMENT  (Polished showroom floor, back wall & architectural battens)
    // ─────────────────────────────────────────────────────────────────────────
    private static void BuildStudioEnvironment()
    {
        // High-gloss reflective showroom floor
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Studio_Floor";
        floor.transform.position = Vector3.zero;
        floor.transform.localScale = new Vector3(8.5f, 1f, 8.5f);
        ApplyMat(floor, new Color(0.06f, 0.07f, 0.09f), 0.88f, 0.65f);

        // Dark architectural backdrop wall
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "Studio_BackWall";
        wall.transform.position = new Vector3(0, 4.2f, -9.5f);
        wall.transform.localScale = new Vector3(24f, 8.5f, 0.3f);
        Object.Destroy(wall.GetComponent<Collider>());
        ApplyMat(wall, new Color(0.035f, 0.040f, 0.055f), 0.25f, 0.12f);

        // Horizontal illuminated ambient neon strip across the back wall
        GameObject neonStrip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        neonStrip.name = "Studio_WallAccentStrip";
        neonStrip.transform.position = new Vector3(0, 2.2f, -9.32f);
        neonStrip.transform.localScale = new Vector3(22f, 0.05f, 0.04f);
        Object.Destroy(neonStrip.GetComponent<Collider>());
        ApplyEmissive(neonStrip, new Color(0.12f, 0.82f, 1.0f), 2.8f);

        // Vertical illuminated architectural light battens on backdrop
        for (int i = -4; i <= 4; i++)
        {
            if (i == 0) continue;
            GameObject batten = GameObject.CreatePrimitive(PrimitiveType.Cube);
            batten.name = "Studio_LightBatten_" + i;
            batten.transform.position = new Vector3(i * 2.5f, 4.2f, -9.34f);
            batten.transform.localScale = new Vector3(0.04f, 6.5f, 0.03f);
            Object.Destroy(batten.GetComponent<Collider>());
            ApplyEmissive(batten, new Color(0.35f, 0.45f, 0.60f), 1.6f);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TURNTABLE  (Dual-tier platform with illuminated neon ring)
    // ─────────────────────────────────────────────────────────────────────────
    private static GameObject BuildShowroomTurntable()
    {
        GameObject pivot = new GameObject("Showroom_TurntablePivot");
        pivot.transform.position = Vector3.zero;
        pivot.transform.localScale = Vector3.one;

        // Base turntable cylinder (Diameter: 7.6m, Height: 0.06m)
        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        platform.name = "Turntable_PlatformMesh";
        platform.transform.SetParent(pivot.transform, false);
        platform.transform.localPosition = new Vector3(0, 0.03f, 0);
        platform.transform.localScale = new Vector3(7.6f, 0.03f, 7.6f);
        Object.Destroy(platform.GetComponent<Collider>());
        ApplyMat(platform, new Color(0.11f, 0.12f, 0.15f), 0.92f, 0.78f);

        // Illuminated cyan neon ring recessed into turntable perimeter
        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "Turntable_NeonRing";
        ring.transform.SetParent(platform.transform, false);
        ring.transform.localPosition = new Vector3(0, 0.54f, 0);
        ring.transform.localScale = new Vector3(0.988f, 0.04f, 0.988f);
        Object.Destroy(ring.GetComponent<Collider>());
        ApplyEmissive(ring, new Color(0.15f, 0.85f, 1.0f), 3.5f);

        return pivot;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GARAGE CONTROLLER & CAMERA
    // ─────────────────────────────────────────────────────────────────────────
    private static VRCollaborativeCarReview BuildGarageController(GameObject turntablePivot)
    {
        GameObject ctrlGO = new GameObject("VRCollaborativeCarReview");
        VRCollaborativeCarReview ctrl = ctrlGO.AddComponent<VRCollaborativeCarReview>();
        ctrl.turntablePivot = turntablePivot.transform;
        return ctrl;
    }

    private static void BuildCameraRig(VRCollaborativeCarReview ctrl)
    {
        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindFirstObjectByType<Camera>();
        if (cam == null)
        {
            GameObject camGO = new GameObject("Main Camera");
            cam = camGO.AddComponent<Camera>();
            camGO.tag = "MainCamera";
        }

        cam.transform.SetParent(null);
        cam.gameObject.name = "Main Camera";
        cam.tag = "MainCamera";
        cam.enabled = true;
        cam.targetDisplay = 0;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.04f, 0.05f, 0.08f);
        cam.fieldOfView = 44f;
        cam.nearClipPlane = 0.08f;
        cam.farClipPlane = 200f;

        GameObject rig = new GameObject("Studio_CameraRig");
        rig.transform.position = new Vector3(-4.5f, 1.8f, 4.0f);
        rig.transform.LookAt(new Vector3(0, 0.55f, 0.2f));

        cam.transform.SetParent(rig.transform, false);
        cam.transform.localPosition = Vector3.zero;
        cam.transform.localRotation = Quaternion.identity;

        ctrl.cameraRig = rig.transform;
        ctrl.mainCamera = cam;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 5 HIGH-POLISH PROCEDURAL VEHICLES
    // ─────────────────────────────────────────────────────────────────────────
    public static VehicleComponents BuildVehicle(int vehicleIndex, Transform parentPivot)
    {
        switch (vehicleIndex)
        {
            case 0: return BuildSportsCar(parentPivot);
            case 1: return BuildSupercar(parentPivot);
            case 2: return BuildHypercar(parentPivot);
            case 3: return BuildMuscleCar(parentPivot);
            case 4: return BuildSUV(parentPivot);
            default: return BuildSportsCar(parentPivot);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CAR 0: Aero GT Concept (Sports Car — Front-Mid Engine, GT3 aero silhouette)
    // ─────────────────────────────────────────────────────────────────────────
    private static VehicleComponents BuildSportsCar(Transform parent)
    {
        VehicleComponents vc = new VehicleComponents();
        GameObject root = new GameObject("VehicleRoot");
        root.transform.SetParent(parent, false);
        root.transform.localPosition = new Vector3(0, 0.42f, 0);
        vc.root = root.transform;

        Color paint = Color.white;
        List<Renderer> body = new List<Renderer>();

        // 1. Lower Aerodynamic Undertray Chassis
        GameObject chassis = MakeCube(root, "Chassis", new Vector3(0, 0.04f, 0), new Vector3(1.85f, 0.16f, 4.55f),
            new Color(0.10f, 0.10f, 0.12f), 0.70f, 0.30f);
        vc.chassis = chassis.transform;

        // Front Carbon Splitter with Dual Aerodynamic Canards
        GameObject splitter = MakeCube(chassis, "FrontSplitter", new Vector3(0, -0.28f, 0.52f), new Vector3(1.06f, 0.18f, 0.14f),
            new Color(0.12f, 0.12f, 0.14f), 0.92f, 0.60f);
        vc.frontSplitter = splitter.transform;
        for (int s = -1; s <= 1; s += 2)
        {
            MakeCube(splitter, "CanardUpper_" + s, new Vector3(s * 0.51f, 0.55f, 0), new Vector3(0.04f, 1.20f, 0.85f),
                new Color(0.12f, 0.12f, 0.14f), 0.92f, 0.60f);
            MakeCube(splitter, "CanardLower_" + s, new Vector3(s * 0.52f, -0.10f, 0.05f), new Vector3(0.06f, 0.30f, 0.60f),
                new Color(0.12f, 0.12f, 0.14f), 0.92f, 0.60f);
        }

        // Rear Diffuser with 4 Vertical Aero Strakes
        GameObject diffuser = MakeCube(chassis, "RearDiffuser", new Vector3(0, -0.16f, -0.52f), new Vector3(1.02f, 0.30f, 0.14f),
            new Color(0.12f, 0.12f, 0.14f), 0.92f, 0.60f);
        vc.rearDiffuser = diffuser.transform;
        for (int k = -2; k <= 2; k++)
        {
            if (k == 0) continue;
            MakeCube(diffuser, "Strake_" + k, new Vector3(k * 0.20f, -0.15f, 0), new Vector3(0.02f, 0.40f, 0.70f),
                new Color(0.10f, 0.10f, 0.12f), 0.90f, 0.50f);
        }

        // Quad Polished Exhaust Tips
        for (int s = -1; s <= 1; s += 2)
        for (int e = 0; e < 2; e++)
        {
            GameObject pipe = MakeCylinder(diffuser, "Exhaust_" + s + "_" + e,
                new Vector3(s * (0.28f + e * 0.12f), -0.06f, -0.55f),
                Quaternion.Euler(90, 0, 0), new Vector3(0.085f, 0.14f, 0.085f),
                new Color(0.85f, 0.86f, 0.90f), 0.96f, 0.92f);
            vc.exhaustRenderers.Add(pipe.GetComponent<Renderer>());
        }

        // Sculpted Coke-Bottle Mid Body Core
        GameObject bodyCore = MakeCube(root, "BodyMidFascia", new Vector3(0, 0.28f, 0), new Vector3(1.88f, 0.36f, 4.30f), paint, 0.94f, 0.88f);
        body.Add(bodyCore.GetComponent<Renderer>());

        // Flared Rear Wheel Haunches (Gives broad, muscular stance)
        for (int s = -1; s <= 1; s += 2)
        {
            GameObject haunch = MakeCube(root, "RearHaunch_" + s, new Vector3(s * 0.98f, 0.28f, -1.25f), new Vector3(0.16f, 0.38f, 1.45f), paint, 0.94f, 0.88f);
            body.Add(haunch.GetComponent<Renderer>());
        }

        // Carbon Side Skirts (Ground effects with rear aero kick-up)
        for (int s = -1; s <= 1; s += 2)
        {
            GameObject sk = MakeCube(root, "SideSkirt_" + s, new Vector3(s * 0.97f, 0.05f, 0), new Vector3(0.08f, 0.08f, 2.70f),
                new Color(0.12f, 0.12f, 0.14f), 0.90f, 0.55f);
            vc.sideSkirtRenderers.Add(sk.GetComponent<Renderer>());
            MakeCube(sk, "AeroFlick_" + s, new Vector3(0, 0.60f, -0.45f), new Vector3(1.1f, 1.2f, 0.30f),
                new Color(0.12f, 0.12f, 0.14f), 0.90f, 0.55f);
        }

        // Sloping Sculpted Hood with Heat Extractor Louvers
        GameObject hood = MakeCube(root, "Hood", new Vector3(0, 0.36f, 1.25f), new Vector3(1.78f, 0.28f, 1.65f), paint, 0.94f, 0.88f);
        body.Add(hood.GetComponent<Renderer>());
        vc.hood = hood.transform;

        // Front Grille & Radiator Intake
        MakeCube(hood, "Grille", new Vector3(0, -0.15f, 0.52f), new Vector3(0.86f, 0.40f, 0.08f),
            new Color(0.08f, 0.08f, 0.10f), 0.40f, 0.10f);

        // Slanted Dual LED Projector Headlights
        for (int s = -1; s <= 1; s += 2)
        {
            GameObject hl = MakeCube(hood, "Headlight_" + s, new Vector3(s * 0.40f, 0.12f, 0.51f), new Vector3(0.24f, 0.10f, 0.05f),
                new Color(0.92f, 0.96f, 1.0f), 0.98f, 0.10f);
            ApplyEmissive(hl, new Color(0.92f, 0.96f, 1.0f), 4.5f);
            vc.headlightRenderers.Add(hl.GetComponent<Renderer>());
        }
        // Continuous Signature DRL Light Strip
        GameObject drl = MakeCube(hood, "SignatureDRL", new Vector3(0, 0.20f, 0.51f), new Vector3(0.94f, 0.04f, 0.04f), Color.white, 1f, 0f);
        ApplyEmissive(drl, new Color(0.85f, 0.95f, 1.0f), 3.2f);

        // Tapered Cabin Cockpit
        GameObject cabin = MakeCube(root, "Cabin", new Vector3(0, 0.72f, -0.22f), new Vector3(1.50f, 0.55f, 2.05f), paint, 0.94f, 0.88f);
        body.Add(cabin.GetComponent<Renderer>());
        vc.cabin = cabin.transform;

        // Aerodynamic Raked Windshield (24 deg)
        GameObject ws = MakeCube(cabin, "Windshield", new Vector3(0, 0.08f, 0.52f), new Vector3(0.92f, 0.75f, 0.06f),
            new Color(0.05f, 0.08f, 0.14f), 0.98f, 0.15f);
        ws.transform.localRotation = Quaternion.Euler(24f, 0, 0);
        vc.windowRenderers.Add(ws.GetComponent<Renderer>());

        // Fastback Rear Window (Angled -26 deg)
        GameObject rw = MakeCube(cabin, "RearWindow", new Vector3(0, 0.06f, -0.52f), new Vector3(0.88f, 0.72f, 0.06f),
            new Color(0.05f, 0.08f, 0.14f), 0.98f, 0.15f);
        rw.transform.localRotation = Quaternion.Euler(-26f, 0, 0);
        vc.windowRenderers.Add(rw.GetComponent<Renderer>());

        // Side aerodynamic mirrors
        for (int s = -1; s <= 1; s += 2)
            MakeCube(cabin, "Mirror_" + s, new Vector3(s * 0.58f, -0.05f, 0.35f), new Vector3(0.16f, 0.10f, 0.22f),
                new Color(0.12f, 0.12f, 0.14f), 0.92f, 0.55f);

        // GT Aerodynamic Spoiler Wing with Swan-Neck Pylons
        GameObject spoiler = MakeCube(root, "SpoilerWing", new Vector3(0, 0.82f, -2.15f), new Vector3(1.78f, 0.08f, 0.40f),
            new Color(0.12f, 0.12f, 0.14f), 0.94f, 0.60f);
        body.Add(spoiler.GetComponent<Renderer>());
        vc.spoiler = spoiler.transform;

        // Endplates
        for (int s = -1; s <= 1; s += 2)
            MakeCube(spoiler, "Endplate_" + s, new Vector3(s * 0.51f, 0.4f, 0), new Vector3(0.04f, 1.4f, 1.05f),
                new Color(0.12f, 0.12f, 0.14f), 0.92f, 0.55f);

        // Twin Mounting Pylons
        for (int s = -1; s <= 1; s += 2)
            MakeCube(spoiler, "Pylon_" + s, new Vector3(s * 0.32f, -1.8f, 0), new Vector3(0.04f, 2.5f, 0.45f),
                new Color(0.12f, 0.12f, 0.14f), 0.92f, 0.50f);

        // Full-Width OLED Cyber Taillight Lightbar
        GameObject tl = MakeCube(root, "TaillightBar", new Vector3(0, 0.44f, -2.18f), new Vector3(1.72f, 0.08f, 0.05f), Color.white, 1f, 0f);
        ApplyEmissive(tl, new Color(1.0f, 0.06f, 0.06f), 4.8f);

        AttachHighDetailWheels(root, vc, 0.94f, 1.35f, WheelStyle.TwinSpoke);
        vc.bodyRenderers = body.ToArray();
        return vc;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CAR 1: Phantom X (Mid-Engine Exotic Supercar — Low, Wide Haunches, Flying Buttresses)
    // ─────────────────────────────────────────────────────────────────────────
    private static VehicleComponents BuildSupercar(Transform parent)
    {
        VehicleComponents vc = new VehicleComponents();
        GameObject root = new GameObject("VehicleRoot");
        root.transform.SetParent(parent, false);
        root.transform.localPosition = new Vector3(0, 0.38f, 0);
        vc.root = root.transform;

        Color paint = Color.white;
        List<Renderer> body = new List<Renderer>();

        // Wide exotic chassis tub
        GameObject chassis = MakeCube(root, "Chassis", new Vector3(0, 0.03f, 0), new Vector3(2.06f, 0.14f, 4.35f),
            new Color(0.10f, 0.10f, 0.12f), 0.70f, 0.30f);
        vc.chassis = chassis.transform;

        // Wide sculpted lower fuselage
        GameObject body1 = MakeCube(root, "BodyLower", new Vector3(0, 0.25f, 0), new Vector3(2.14f, 0.30f, 4.10f), paint, 0.95f, 0.92f);
        body.Add(body1.GetComponent<Renderer>());

        // Aggressive rear haunches with cooling vents
        for (int s = -1; s <= 1; s += 2)
        {
            GameObject haunch = MakeCube(root, "SupercarHaunch_" + s, new Vector3(s * 1.10f, 0.24f, -1.15f), new Vector3(0.24f, 0.40f, 1.45f), paint, 0.95f, 0.92f);
            body.Add(haunch.GetComponent<Renderer>());
            // Intake vent
            MakeCube(haunch, "IntakeSlot_" + s, new Vector3(0, 0.15f, 0.52f), new Vector3(0.85f, 0.45f, 0.06f),
                new Color(0.06f, 0.06f, 0.08f), 0.30f, 0.10f);
        }

        // Low sloping wedge nose
        GameObject nose = MakeCube(root, "WedgeNose", new Vector3(0, 0.20f, 1.85f), new Vector3(1.55f, 0.22f, 0.75f), paint, 0.95f, 0.92f);
        body.Add(nose.GetComponent<Renderer>());
        vc.hood = nose.transform;

        // Sharp aerodynamic blade headlights
        for (int s = -1; s <= 1; s += 2)
        {
            GameObject hl = MakeCube(nose, "BladeHeadlight_" + s, new Vector3(s * 0.38f, 0.10f, 0.52f), new Vector3(0.32f, 0.06f, 0.05f), Color.white, 1f, 0f);
            ApplyEmissive(hl, new Color(0.95f, 0.99f, 1.0f), 5.2f);
            vc.headlightRenderers.Add(hl.GetComponent<Renderer>());
        }

        // Slim tapered fighter-jet cockpit
        GameObject cabin = MakeCube(root, "Cockpit", new Vector3(0, 0.62f, -0.08f), new Vector3(1.24f, 0.46f, 1.85f), paint, 0.95f, 0.92f);
        body.Add(cabin.GetComponent<Renderer>());
        vc.cabin = cabin.transform;

        // Aerodynamic raked canopy
        GameObject ws = MakeCube(cabin, "CanopyGlass", new Vector3(0, 0.08f, 0.52f), new Vector3(0.92f, 0.72f, 0.05f),
            new Color(0.04f, 0.07f, 0.13f), 0.99f, 0.10f);
        ws.transform.localRotation = Quaternion.Euler(30f, 0, 0);
        vc.windowRenderers.Add(ws.GetComponent<Renderer>());

        // Flying Buttresses over rear engine deck
        for (int s = -1; s <= 1; s += 2)
        {
            GameObject buttress = MakeCube(root, "FlyingButtress_" + s, new Vector3(s * 0.58f, 0.56f, -1.25f), new Vector3(0.14f, 0.65f, 1.15f), paint, 0.95f, 0.92f);
            body.Add(buttress.GetComponent<Renderer>());
        }

        // Active aero rear wing
        GameObject spoiler = MakeCube(root, "ActiveWing", new Vector3(0, 0.84f, -2.05f), new Vector3(1.88f, 0.10f, 0.38f),
            new Color(0.14f, 0.14f, 0.16f), 0.92f, 0.55f);
        vc.spoiler = spoiler.transform;

        // Front carbon track splitter
        GameObject fSplitter = MakeCube(root, "FrontSplitter", new Vector3(0, 0.06f, 2.12f), new Vector3(1.85f, 0.06f, 0.22f),
            new Color(0.10f, 0.10f, 0.12f), 0.90f, 0.50f);
        vc.frontSplitter = fSplitter.transform;

        // Center-exit quad exhaust tips
        for (int e = -1; e <= 1; e += 2)
        {
            GameObject ex = MakeCylinder(root, "SupercarExhaust_" + e, new Vector3(e * 0.10f, 0.36f, -2.18f),
                Quaternion.Euler(90, 0, 0), new Vector3(0.09f, 0.16f, 0.09f),
                new Color(0.85f, 0.86f, 0.90f), 0.96f, 0.90f);
            vc.exhaustRenderers.Add(ex.GetComponent<Renderer>());
        }

        // Carbon side skirts
        for (int s = -1; s <= 1; s += 2)
        {
            GameObject sk = MakeCube(root, "SideSkirt_" + s, new Vector3(s * 1.04f, 0.08f, 0), new Vector3(0.10f, 0.08f, 2.55f),
                new Color(0.10f, 0.10f, 0.12f), 0.88f, 0.45f);
            vc.sideSkirtRenderers.Add(sk.GetComponent<Renderer>());
        }

        AttachHighDetailWheels(root, vc, 1.02f, 1.25f, WheelStyle.StarSpoke);
        vc.bodyRenderers = body.ToArray();
        return vc;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CAR 2: Titan RS (Le Mans Hypercar — Ultra Low, Shark Fin, Dual-Tier Aero Wing)
    // ─────────────────────────────────────────────────────────────────────────
    private static VehicleComponents BuildHypercar(Transform parent)
    {
        VehicleComponents vc = new VehicleComponents();
        GameObject root = new GameObject("VehicleRoot");
        root.transform.SetParent(parent, false);
        root.transform.localPosition = new Vector3(0, 0.34f, 0); // Slammed hypercar stance
        vc.root = root.transform;

        Color paint = Color.white;
        List<Renderer> body = new List<Renderer>();

        // Monocoque carbon chassis
        GameObject chassis = MakeCube(root, "Chassis", new Vector3(0, 0.02f, 0), new Vector3(2.12f, 0.11f, 4.25f),
            new Color(0.08f, 0.08f, 0.10f), 0.70f, 0.25f);
        vc.chassis = chassis.transform;

        // Wide Le Mans aerodynamic body shell
        GameObject bodyMain = MakeCube(root, "HypercarFuselage", new Vector3(0, 0.22f, 0), new Vector3(2.20f, 0.26f, 4.05f), paint, 0.96f, 0.94f);
        body.Add(bodyMain.GetComponent<Renderer>());

        // Extreme aerodynamic wheel arch pontoons
        for (int s = -1; s <= 1; s += 2)
        {
            for (int lr = -1; lr <= 1; lr += 2)
            {
                GameObject arch = MakeCube(root, "Pontoon_" + s + "_" + lr, new Vector3(s * 1.12f, 0.23f, lr * 1.25f), new Vector3(0.26f, 0.46f, 0.95f), paint, 0.96f, 0.94f);
                body.Add(arch.GetComponent<Renderer>());
            }
        }

        // Cockpit bubble canopy
        GameObject cabin = MakeCube(root, "BubbleCanopy", new Vector3(0, 0.54f, 0.05f), new Vector3(1.10f, 0.40f, 1.85f), paint, 0.96f, 0.94f);
        body.Add(cabin.GetComponent<Renderer>());
        vc.cabin = cabin.transform;

        // Full wraparound glass bubble
        GameObject ws = MakeCube(cabin, "BubbleGlass", new Vector3(0, 0.10f, 0), new Vector3(0.96f, 0.82f, 0.96f),
            new Color(0.04f, 0.06f, 0.10f), 0.99f, 0.10f);
        vc.windowRenderers.Add(ws.GetComponent<Renderer>());

        // Central LMP-Style Shark Fin Stabilizer
        GameObject fin = MakeCube(root, "SharkFin", new Vector3(0, 0.65f, -0.90f), new Vector3(0.04f, 0.60f, 1.60f),
            new Color(0.12f, 0.12f, 0.14f), 0.94f, 0.60f);
        body.Add(fin.GetComponent<Renderer>());

        // Dual-tier high-downforce rear wing
        GameObject wing1 = MakeCube(root, "WingLower", new Vector3(0, 0.92f, -2.05f), new Vector3(1.98f, 0.10f, 0.40f),
            new Color(0.12f, 0.12f, 0.14f), 0.92f, 0.55f);
        GameObject wing2 = MakeCube(root, "WingUpperFlap", new Vector3(0, 1.12f, -2.02f), new Vector3(1.98f, 0.06f, 0.24f),
            new Color(0.12f, 0.12f, 0.14f), 0.92f, 0.55f);
        vc.spoiler = wing1.transform;

        // Wing endplates
        for (int s = -1; s <= 1; s += 2)
            MakeCube(wing1, "HyperEndplate_" + s, new Vector3(s * 0.51f, 0.8f, 0), new Vector3(0.04f, 2.2f, 1.1f),
                new Color(0.12f, 0.12f, 0.14f), 0.92f, 0.55f);

        // Low aero nose with vortex generator slots
        GameObject nose = MakeCube(root, "HyperNose", new Vector3(0, 0.15f, 1.90f), new Vector3(1.35f, 0.18f, 0.55f), paint, 0.96f, 0.94f);
        body.Add(nose.GetComponent<Renderer>());
        vc.hood = nose.transform;

        // Slit-style futuristic laser headlights
        for (int s = -1; s <= 1; s += 2)
        {
            GameObject hl = MakeCube(nose, "LaserHeadlight_" + s, new Vector3(s * 0.40f, 0.05f, 0.52f), new Vector3(0.42f, 0.04f, 0.04f), Color.white, 1f, 0f);
            ApplyEmissive(hl, new Color(0.88f, 0.96f, 1.0f), 6.0f);
            vc.headlightRenderers.Add(hl.GetComponent<Renderer>());
        }

        // Carbon ground tunnel skirts
        for (int s = -1; s <= 1; s += 2)
        {
            GameObject sk = MakeCube(root, "GroundTunnelSkirt_" + s, new Vector3(s * 1.06f, 0.08f, 0), new Vector3(0.12f, 0.08f, 2.45f),
                new Color(0.08f, 0.08f, 0.10f), 0.86f, 0.40f);
            vc.sideSkirtRenderers.Add(sk.GetComponent<Renderer>());
        }

        vc.frontSplitter = chassis.transform;
        vc.rearDiffuser = chassis.transform;

        AttachHighDetailWheels(root, vc, 1.04f, 1.20f, WheelStyle.LatticeSpoke);
        vc.bodyRenderers = body.ToArray();
        return vc;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CAR 3: Muscle V8 Interceptor (American Muscle — Long Hood, Shaker Scoop, Ducktail)
    // ─────────────────────────────────────────────────────────────────────────
    private static VehicleComponents BuildMuscleCar(Transform parent)
    {
        VehicleComponents vc = new VehicleComponents();
        GameObject root = new GameObject("VehicleRoot");
        root.transform.SetParent(parent, false);
        root.transform.localPosition = new Vector3(0, 0.48f, 0); // Sturdy muscle stance
        vc.root = root.transform;

        Color paint = Color.white;
        List<Renderer> body = new List<Renderer>();

        // Heavy-duty ladder-frame chassis
        GameObject chassis = MakeCube(root, "Chassis", new Vector3(0, 0.04f, 0), new Vector3(1.94f, 0.18f, 4.85f),
            new Color(0.10f, 0.10f, 0.12f), 0.65f, 0.25f);
        vc.chassis = chassis.transform;

        // Long muscular body shell
        GameObject bodyMain = MakeCube(root, "BodyMain", new Vector3(0, 0.32f, 0), new Vector3(1.98f, 0.42f, 4.75f), paint, 0.88f, 0.80f);
        body.Add(bodyMain.GetComponent<Renderer>());

        // Long prominent hood (Classic front V8 proportions)
        GameObject hood = MakeCube(root, "MuscleHood", new Vector3(0, 0.44f, 1.55f), new Vector3(1.92f, 0.30f, 1.95f), paint, 0.88f, 0.80f);
        body.Add(hood.GetComponent<Renderer>());
        vc.hood = hood.transform;

        // Functional Shaker Hood Scoop / Power Bulge
        GameObject scoop = MakeCube(hood, "PowerBulgeScoop", new Vector3(0, 0.20f, 0), new Vector3(0.32f, 0.18f, 0.75f),
            new Color(0.08f, 0.08f, 0.10f), 0.72f, 0.30f);
        MakeCube(scoop, "IntakeMouth", new Vector3(0, 0.4f, 0.52f), new Vector3(0.85f, 0.35f, 0.08f),
            new Color(0.05f, 0.05f, 0.07f), 0.20f, 0.05f);

        // Aggressive blunt nose with recessed bar grille
        GameObject nose = MakeCube(root, "BluntNose", new Vector3(0, 0.34f, 2.40f), new Vector3(1.90f, 0.30f, 0.22f), paint, 0.88f, 0.80f);
        body.Add(nose.GetComponent<Renderer>());

        // Deep recessed horizontal bar grille
        MakeCube(nose, "MuscleGrille", new Vector3(0, -0.06f, 0.52f), new Vector3(0.82f, 0.24f, 0.06f),
            new Color(0.06f, 0.06f, 0.08f), 0.25f, 0.05f);

        // Quad round halo headlights
        for (int s = -1; s <= 1; s += 2)
        for (int h = 0; h < 2; h++)
        {
            GameObject hl = MakeCylinder(nose, "HaloHeadlight_" + s + "_" + h,
                new Vector3(s * (0.32f + h * 0.14f), 0.04f, 0.52f),
                Quaternion.Euler(90, 0, 0), new Vector3(0.10f, 0.06f, 0.10f),
                new Color(1.0f, 0.98f, 0.90f), 0.98f, 0.10f);
            ApplyEmissive(hl, new Color(1.0f, 0.98f, 0.90f), 4.2f);
            vc.headlightRenderers.Add(hl.GetComponent<Renderer>());
        }

        // Fastback 2-door cabin
        GameObject cabin = MakeCube(root, "FastbackCabin", new Vector3(0, 0.78f, -0.42f), new Vector3(1.64f, 0.60f, 1.95f), paint, 0.88f, 0.80f);
        body.Add(cabin.GetComponent<Renderer>());
        vc.cabin = cabin.transform;

        // Angled muscle windshield
        GameObject ws = MakeCube(cabin, "Windshield", new Vector3(0, 0.08f, 0.52f), new Vector3(0.94f, 0.78f, 0.06f),
            new Color(0.05f, 0.08f, 0.14f), 0.98f, 0.12f);
        ws.transform.localRotation = Quaternion.Euler(20f, 0, 0);
        vc.windowRenderers.Add(ws.GetComponent<Renderer>());

        // Ducktail rear spoiler
        GameObject spoiler = MakeCube(root, "DucktailSpoiler", new Vector3(0, 0.52f, -2.22f), new Vector3(1.65f, 0.08f, 0.24f),
            new Color(0.12f, 0.12f, 0.14f), 0.88f, 0.45f);
        vc.spoiler = spoiler.transform;

        // Big-bore dual side-exit chrome exhausts
        for (int s = -1; s <= 1; s += 2)
        {
            GameObject pipe = MakeCylinder(root, "BigBoreExhaust_" + s,
                new Vector3(s * 0.60f, 0.15f, -2.25f),
                Quaternion.Euler(90, 0, 0), new Vector3(0.15f, 0.12f, 0.15f),
                new Color(0.85f, 0.86f, 0.90f), 0.96f, 0.88f);
            vc.exhaustRenderers.Add(pipe.GetComponent<Renderer>());
        }

        vc.frontSplitter = chassis.transform;
        vc.rearDiffuser = chassis.transform;

        AttachHighDetailWheels(root, vc, 0.96f, 1.45f, WheelStyle.YBlade);
        vc.bodyRenderers = body.ToArray();
        return vc;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CAR 4: Apex Crossover (Performance Luxury SUV — Elevated Stance, Cladding, Panorama)
    // ─────────────────────────────────────────────────────────────────────────
    private static VehicleComponents BuildSUV(Transform parent)
    {
        VehicleComponents vc = new VehicleComponents();
        GameObject root = new GameObject("VehicleRoot");
        root.transform.SetParent(parent, false);
        root.transform.localPosition = new Vector3(0, 0.58f, 0); // Elevated ground clearance
        vc.root = root.transform;

        Color paint = Color.white;
        List<Renderer> body = new List<Renderer>();

        // Rugged reinforced chassis with skid plates
        GameObject chassis = MakeCube(root, "Chassis", new Vector3(0, 0.04f, 0), new Vector3(1.98f, 0.22f, 4.65f),
            new Color(0.10f, 0.10f, 0.12f), 0.65f, 0.25f);
        vc.chassis = chassis.transform;

        // Skid plate front
        MakeCube(chassis, "FrontSkidPlate", new Vector3(0, -0.38f, 0.48f), new Vector3(0.85f, 0.14f, 0.22f),
            new Color(0.48f, 0.50f, 0.54f), 0.82f, 0.72f);

        // High luxury crossover body
        GameObject bodyMain = MakeCube(root, "SUVBody", new Vector3(0, 0.56f, 0), new Vector3(2.04f, 0.82f, 4.50f), paint, 0.84f, 0.74f);
        body.Add(bodyMain.GetComponent<Renderer>());

        // Matte black wheel arch protection cladding
        for (int s = -1; s <= 1; s += 2)
        {
            for (int lr = -1; lr <= 1; lr += 2)
            {
                GameObject cl = MakeCube(root, "ArchCladding_" + s + "_" + lr, new Vector3(s * 1.03f, 0.20f, lr * 1.25f), new Vector3(0.10f, 0.52f, 1.12f),
                    new Color(0.11f, 0.11f, 0.13f), 0.35f, 0.08f);
                body.Add(cl.GetComponent<Renderer>());
            }
        }

        // Flat muscular SUV hood
        GameObject hood = MakeCube(root, "SUVHood", new Vector3(0, 0.74f, 1.62f), new Vector3(1.96f, 0.26f, 1.55f), paint, 0.84f, 0.74f);
        body.Add(hood.GetComponent<Renderer>());
        vc.hood = hood.transform;

        // Upright luxury front grille
        GameObject frontFace = MakeCube(root, "FrontFascia", new Vector3(0, 0.48f, 2.30f), new Vector3(1.96f, 0.52f, 0.22f), paint, 0.84f, 0.74f);
        body.Add(frontFace.GetComponent<Renderer>());
        MakeCube(frontFace, "WaterfallGrille", new Vector3(0, -0.05f, 0.52f), new Vector3(0.92f, 0.30f, 0.06f),
            new Color(0.16f, 0.16f, 0.18f), 0.88f, 0.75f);

        // Modern matrix LED headlights with eyebrow DRLs
        for (int s = -1; s <= 1; s += 2)
        {
            GameObject hl = MakeCube(frontFace, "Headlight_" + s, new Vector3(s * 0.42f, 0.12f, 0.52f), new Vector3(0.32f, 0.16f, 0.05f), Color.white, 1f, 0f);
            ApplyEmissive(hl, new Color(0.95f, 0.98f, 1.0f), 4.5f);
            vc.headlightRenderers.Add(hl.GetComponent<Renderer>());
        }

        // Tall 5-door cabin with panoramic glass roof
        GameObject cabin = MakeCube(root, "Cabin", new Vector3(0, 1.10f, -0.20f), new Vector3(1.90f, 0.78f, 2.65f), paint, 0.84f, 0.74f);
        body.Add(cabin.GetComponent<Renderer>());
        vc.cabin = cabin.transform;

        // Large panoramic windshield
        GameObject ws = MakeCube(cabin, "Windshield", new Vector3(0, 0.06f, 0.52f), new Vector3(0.92f, 0.86f, 0.06f),
            new Color(0.05f, 0.08f, 0.14f), 0.98f, 0.12f);
        ws.transform.localRotation = Quaternion.Euler(14f, 0, 0);
        vc.windowRenderers.Add(ws.GetComponent<Renderer>());

        // Brushed aluminum roof rails
        for (int s = -1; s <= 1; s += 2)
            MakeCube(cabin, "RoofRail_" + s, new Vector3(s * 0.54f, 0.54f, 0), new Vector3(0.04f, 0.05f, 1.65f),
                new Color(0.60f, 0.62f, 0.66f), 0.90f, 0.85f);

        // Integrated tailgate roof spoiler
        GameObject spoiler = MakeCube(root, "RoofSpoiler", new Vector3(0, 1.54f, -2.12f), new Vector3(1.72f, 0.08f, 0.22f),
            new Color(0.12f, 0.12f, 0.14f), 0.85f, 0.45f);
        vc.spoiler = spoiler.transform;

        vc.frontSplitter = chassis.transform;
        vc.rearDiffuser = chassis.transform;

        AttachHighDetailWheels(root, vc, 0.98f, 1.50f, WheelStyle.DeepDish);
        vc.bodyRenderers = body.ToArray();
        return vc;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // HIGH-DETAIL WHEEL FACTORY
    // ─────────────────────────────────────────────────────────────────────────
    public enum WheelStyle { TwinSpoke, StarSpoke, LatticeSpoke, YBlade, DeepDish }

    private static void AttachHighDetailWheels(GameObject carRoot, VehicleComponents vc, float trackX, float wheelBaseZ, WheelStyle style)
    {
        bool isLarge = (style == WheelStyle.LatticeSpoke || style == WheelStyle.DeepDish);
        float tireDiam = isLarge ? 0.72f : 0.68f;

        GameObject wFL = BuildDetailedWheel("Wheel_FL_Q2", style, tireDiam, true);
        wFL.transform.SetParent(carRoot.transform, false);
        wFL.transform.localPosition = new Vector3(-trackX, 0, wheelBaseZ);
        vc.wheelFL = wFL.transform;

        GameObject wFR = BuildDetailedWheel("Wheel_FR", style, tireDiam, false);
        wFR.transform.SetParent(carRoot.transform, false);
        wFR.transform.localPosition = new Vector3(trackX, 0, wheelBaseZ);
        vc.wheelFR = wFR.transform;

        GameObject wRL = BuildDetailedWheel("Wheel_RL", style, tireDiam, false);
        wRL.transform.SetParent(carRoot.transform, false);
        wRL.transform.localPosition = new Vector3(-trackX, 0, -wheelBaseZ);
        vc.wheelRL = wRL.transform;

        GameObject wRR = BuildDetailedWheel("Wheel_RR", style, tireDiam, false);
        wRR.transform.SetParent(carRoot.transform, false);
        wRR.transform.localPosition = new Vector3(trackX, 0, -wheelBaseZ);
        vc.wheelRR = wRR.transform;
    }

    public static GameObject BuildDetailedWheel(string wName, WheelStyle style, float tireDiam, bool attachMatrixDemo)
    {
        GameObject wheelRoot = new GameObject(wName);

        // 1. Performance Compound Tire (Chamfered sidewall)
        MakeCylinder(wheelRoot, "Tire", Vector3.zero, Quaternion.Euler(0, 0, 90f),
            new Vector3(tireDiam, 0.13f, tireDiam), new Color(0.09f, 0.09f, 0.10f), 0.35f, 0.04f);

        // 2. Concave Rim Barrel Root
        GameObject rimRoot = new GameObject("RimRoot");
        rimRoot.transform.SetParent(wheelRoot.transform, false);

        // Outer machined rim lip
        MakeCylinder(rimRoot, "RimLip", Vector3.zero, Quaternion.Euler(0, 0, 90f),
            new Vector3(0.52f, 0.14f, 0.52f), new Color(0.86f, 0.87f, 0.90f), 0.94f, 0.88f);

        // Recessed inner barrel (dark depth)
        MakeCylinder(rimRoot, "InnerBarrel", new Vector3(-0.02f, 0, 0), Quaternion.Euler(0, 0, 90f),
            new Vector3(0.48f, 0.11f, 0.48f), new Color(0.18f, 0.19f, 0.22f), 0.88f, 0.70f);

        // Distinctive spoke patterns
        BuildSpokes(rimRoot, style);

        // Centerlock hub cap with brand accent
        MakeCylinder(rimRoot, "HubCap", Vector3.zero, Quaternion.Euler(0, 0, 90f),
            new Vector3(0.12f, 0.15f, 0.12f), new Color(0.15f, 0.48f, 0.95f), 0.96f, 0.92f);

        // 3. Ventilated Ceramic Brake Rotor
        MakeCylinder(wheelRoot, "BrakeDisc", new Vector3(0.045f, 0, 0), Quaternion.Euler(0, 0, 90f),
            new Vector3(0.44f, 0.04f, 0.44f), new Color(0.36f, 0.37f, 0.40f), 0.85f, 0.88f);

        // 4. Multi-Piston High Performance Brake Caliper (Brembo style)
        GameObject caliper = MakeCube(wheelRoot, "Caliper", new Vector3(0.055f, 0.13f, 0), new Vector3(0.07f, 0.18f, 0.12f),
            new Color(0.92f, 0.10f, 0.10f), 0.78f, 0.40f);

        if (attachMatrixDemo)
            rimRoot.AddComponent<CarWheelTransformDemo>();

        return wheelRoot;
    }

    private static void BuildSpokes(GameObject rimRoot, WheelStyle style)
    {
        switch (style)
        {
            case WheelStyle.TwinSpoke:    BuildTwinSpokes(rimRoot, 5); break;
            case WheelStyle.StarSpoke:    BuildStarSpokes(rimRoot, 10); break;
            case WheelStyle.LatticeSpoke: BuildLatticeSpokes(rimRoot); break;
            case WheelStyle.YBlade:       BuildYBlade(rimRoot); break;
            case WheelStyle.DeepDish:     BuildDeepDish(rimRoot); break;
        }
    }

    private static void BuildTwinSpokes(GameObject parent, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float deg = i * (360f / count);
            for (int s = -1; s <= 1; s += 2)
            {
                GameObject sp = MakeCube(parent, "Spoke_" + i + "_" + s, Vector3.zero, new Vector3(0.032f, 0.46f, 0.044f),
                    new Color(0.82f, 0.84f, 0.88f), 0.92f, 0.85f);
                sp.transform.localRotation = Quaternion.Euler(0, 0, deg + s * 7.5f);
            }
        }
    }

    private static void BuildStarSpokes(GameObject parent, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float deg = i * (360f / count);
            GameObject sp = MakeCube(parent, "StarSpoke_" + i, Vector3.zero, new Vector3(0.026f, 0.46f, 0.036f),
                new Color(0.80f, 0.82f, 0.86f), 0.92f, 0.85f);
            sp.transform.localRotation = Quaternion.Euler(0, 0, deg);
        }
    }

    private static void BuildLatticeSpokes(GameObject parent)
    {
        for (int i = 0; i < 8; i++)
        {
            float deg = i * 45f;
            GameObject sp = MakeCube(parent, "OuterSpoke_" + i, Vector3.zero, new Vector3(0.022f, 0.44f, 0.030f),
                new Color(0.78f, 0.80f, 0.84f), 0.92f, 0.85f);
            sp.transform.localRotation = Quaternion.Euler(0, 0, deg);

            GameObject sp2 = MakeCube(parent, "InnerCross_" + i, Vector3.zero, new Vector3(0.018f, 0.28f, 0.028f),
                new Color(0.72f, 0.74f, 0.78f), 0.90f, 0.80f);
            sp2.transform.localRotation = Quaternion.Euler(0, 0, deg + 22.5f);
        }
    }

    private static void BuildYBlade(GameObject parent)
    {
        for (int i = 0; i < 3; i++)
        {
            float deg = i * 120f;
            GameObject arm = MakeCube(parent, "YArm_" + i, Vector3.zero, new Vector3(0.055f, 0.44f, 0.048f),
                new Color(0.80f, 0.82f, 0.86f), 0.90f, 0.84f);
            arm.transform.localRotation = Quaternion.Euler(0, 0, deg);
            for (int t = -1; t <= 1; t += 2)
            {
                GameObject tip = MakeCube(parent, "YTip_" + i + "_" + t, Vector3.zero, new Vector3(0.038f, 0.22f, 0.038f),
                    new Color(0.78f, 0.80f, 0.84f), 0.90f, 0.82f);
                tip.transform.localRotation = Quaternion.Euler(0, 0, deg + t * 15f);
                tip.transform.localPosition = Quaternion.Euler(0, 0, deg) * new Vector3(0, 0.22f, 0);
            }
        }
    }

    private static void BuildDeepDish(GameObject parent)
    {
        MakeCylinder(parent, "DishBarrel", Vector3.zero, Quaternion.Euler(0, 0, 90f),
            new Vector3(0.42f, 0.06f, 0.42f), new Color(0.70f, 0.71f, 0.74f), 0.90f, 0.82f);
        for (int i = 0; i < 7; i++)
        {
            float deg = i * (360f / 7f);
            GameObject sp = MakeCube(parent, "DishSpoke_" + i, Vector3.zero, new Vector3(0.038f, 0.40f, 0.046f),
                new Color(0.75f, 0.77f, 0.80f), 0.90f, 0.84f);
            sp.transform.localRotation = Quaternion.Euler(0, 0, deg);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PRIMITIVE HELPERS & MATERIAL CACHING (Instant load, max SRP batching)
    // ─────────────────────────────────────────────────────────────────────────
    private static readonly Dictionary<int, Material> _matCache = new Dictionary<int, Material>();
    private static readonly Dictionary<int, Material> _emissiveCache = new Dictionary<int, Material>();
    private static Shader _litShader;
    private static Shader _unlitShader;

    public static GameObject MakeCube(GameObject parent, string name, Vector3 localPos, Vector3 localScale,
        Color col, float smoothness, float metallic)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = localScale;
        Object.Destroy(go.GetComponent<Collider>());
        ApplyMat(go, col, smoothness, metallic);
        return go;
    }

    public static GameObject MakeCylinder(GameObject parent, string name, Vector3 localPos,
        Quaternion localRot, Vector3 localScale, Color col, float smoothness, float metallic)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = localPos;
        go.transform.localRotation = localRot;
        go.transform.localScale = localScale;
        Object.Destroy(go.GetComponent<Collider>());
        ApplyMat(go, col, smoothness, metallic);
        return go;
    }

    public static void ApplyMat(GameObject go, Color col, float smoothness, float metallic)
    {
        if (_litShader == null) _litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        int hash = col.GetHashCode() ^ smoothness.GetHashCode() ^ (metallic.GetHashCode() << 2);
        Material mat;
        if (!_matCache.TryGetValue(hash, out mat) || mat == null)
        {
            mat = new Material(_litShader);
            mat.color = col;
            if (mat.HasProperty("_BaseColor"))  mat.SetColor("_BaseColor", col);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Metallic"))   mat.SetFloat("_Metallic", metallic);
            _matCache[hash] = mat;
        }
        Renderer r = go.GetComponent<Renderer>();
        if (r != null) r.sharedMaterial = mat;
    }

    public static void ApplyEmissive(GameObject go, Color col, float intensity)
    {
        if (_unlitShader == null) _unlitShader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
        int hash = col.GetHashCode() ^ intensity.GetHashCode();
        Material mat;
        if (!_emissiveCache.TryGetValue(hash, out mat) || mat == null)
        {
            mat = new Material(_unlitShader);
            Color hdr = col * intensity;
            mat.color = hdr;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", hdr);
            _emissiveCache[hash] = mat;
        }
        Renderer r = go.GetComponent<Renderer>();
        if (r != null) r.sharedMaterial = mat;
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// DATA CONTAINER — holds all built vehicle references
// ─────────────────────────────────────────────────────────────────────────────
[System.Serializable]
public class VehicleComponents
{
    public Transform root;
    public Transform chassis, hood, cabin, spoiler;
    public Transform frontSplitter, rearDiffuser;
    public Transform wheelFL, wheelFR, wheelRL, wheelRR;
    public Renderer[] bodyRenderers = new Renderer[0];
    public List<Renderer> windowRenderers = new List<Renderer>();
    public List<Renderer> sideSkirtRenderers = new List<Renderer>();
    public List<Renderer> exhaustRenderers = new List<Renderer>();
    public List<Renderer> headlightRenderers = new List<Renderer>();
    public List<Renderer> wheelRimRenderers = new List<Renderer>();
}
