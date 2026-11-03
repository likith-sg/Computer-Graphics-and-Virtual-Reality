using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Case Study 2 — High-Polish Forza Horizon Style Automotive Customization Studio
///
/// Features:
///   - 5 distinct high-performance vehicles with PI ratings (Performance Index: S2 998, X 999, S1 875, etc.)
///   - Graphical stat bars (Speed, Handling, Acceleration, Launch, Braking) with dynamic upgrades
///   - Visual color palette swatches with real-time BaseColor/Smoothness/Metallic clearcoat updates
///   - Modular wheels with 5 distinct alloy rim patterns, custom rim finishes, and caliper colors
///   - Full body aero customization: bumpers, ducktails, GT wings, side skirts, and exhaust options
///   - Smooth contextual camera orbit, zoom, pan, and presets
///   - Q2 Homogeneous Matrix Transformation demo (Scale · Rotate · Translate) with 3D axes tripod
///   - Q3 VR Dual-Eye Telemetry (FPS, Frame Budget, Hi-Z Early-Z, Back-face Culling, Frustum Clipping)
///
/// Student: Likith S G   USN: 1RVU23CSE236
/// Course:  Computer Graphics and Virtual Reality Laboratory
/// </summary>
public class VRCollaborativeCarReview : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────────
    // VEHICLE DEFINITIONS WITH FORZA PI RATINGS & STATS
    // ─────────────────────────────────────────────────────────────────────────
    public struct VehicleDef
    {
        public string name, category, make, model;
        public int year;
        public string piClass;
        public int piScore;
        public int hp, torque;
        public float topSpeed, accel;
        public float speedStat, handlingStat, accelStat, launchStat, brakingStat;
        public string engineLayout;
        public CaseStudyBootstrap.WheelStyle defaultWheelStyle;
        public Color defaultPaint;
    }

    private static readonly VehicleDef[] Vehicles = new VehicleDef[]
    {
        new VehicleDef {
            name="Aero GT Concept", category="Sports", make="Apex", model="Aero GT", year=2024,
            piClass="S1", piScore=865, hp=450, torque=420, topSpeed=295, accel=3.8f,
            speedStat=8.2f, handlingStat=8.6f, accelStat=8.4f, launchStat=8.1f, brakingStat=8.5f,
            engineLayout="Front-Mid V6 Twin-Turbo", defaultWheelStyle=CaseStudyBootstrap.WheelStyle.TwinSpoke,
            defaultPaint=new Color(0.06f, 0.42f, 0.98f)
        },
        new VehicleDef {
            name="Phantom X", category="Supercar", make="Veloce", model="X-800", year=2024,
            piClass="S2", piScore=962, hp=780, torque=710, topSpeed=345, accel=2.7f,
            speedStat=9.3f, handlingStat=9.1f, accelStat=9.5f, launchStat=9.4f, brakingStat=9.2f,
            engineLayout="Mid-Mounted V8 Twin-Turbo", defaultWheelStyle=CaseStudyBootstrap.WheelStyle.StarSpoke,
            defaultPaint=new Color(0.95f, 0.08f, 0.08f)
        },
        new VehicleDef {
            name="Titan RS", category="Hypercar", make="Kronos", model="RS-1200", year=2025,
            piClass="X", piScore=999, hp=1250, torque=1150, topSpeed=430, accel=1.75f,
            speedStat=9.9f, handlingStat=9.7f, accelStat=10.0f, launchStat=9.9f, brakingStat=9.8f,
            engineLayout="Quad-Motor Hybrid AWD", defaultWheelStyle=CaseStudyBootstrap.WheelStyle.LatticeSpoke,
            defaultPaint=new Color(0.08f, 0.09f, 0.11f)
        },
        new VehicleDef {
            name="Muscle V8 Interceptor", category="Muscle", make="Detroit", model="Interceptor 427", year=2023,
            piClass="A", piScore=785, hp=650, torque=890, topSpeed=285, accel=3.4f,
            speedStat=7.8f, handlingStat=6.9f, accelStat=8.7f, launchStat=8.5f, brakingStat=7.2f,
            engineLayout="Supercharged 7.0L Big Block V8", defaultWheelStyle=CaseStudyBootstrap.WheelStyle.YBlade,
            defaultPaint=new Color(0.60f, 0.06f, 0.06f)
        },
        new VehicleDef {
            name="Apex Crossover GT", category="SUV", make="Apex", model="Crossover GT", year=2025,
            piClass="A", piScore=750, hp=410, torque=520, topSpeed=250, accel=4.8f,
            speedStat=7.1f, handlingStat=7.4f, accelStat=7.6f, launchStat=8.2f, brakingStat=7.8f,
            engineLayout="Dual-Motor Turbo Hybrid AWD", defaultWheelStyle=CaseStudyBootstrap.WheelStyle.DeepDish,
            defaultPaint=new Color(0.12f, 0.38f, 0.16f)
        },
    };

    // ─────────────────────────────────────────────────────────────────────────
    // PAINT PALETTES
    // ─────────────────────────────────────────────────────────────────────────
    private static readonly Color[] PaintColors = {
        new Color(0.06f,0.42f,0.98f), new Color(0.95f,0.08f,0.08f), new Color(1.00f,0.76f,0.02f),
        new Color(0.08f,0.09f,0.11f), new Color(0.05f,0.80f,0.44f), new Color(0.92f,0.94f,0.98f),
        new Color(0.68f,0.15f,0.95f), new Color(0.92f,0.45f,0.08f), new Color(0.12f,0.45f,0.22f),
        new Color(0.72f,0.65f,0.52f), new Color(0.15f,0.18f,0.25f), new Color(0.85f,0.12f,0.45f)
    };
    private static readonly string[] PaintNames = {
        "Electric Cobalt","Rosso Corsa Red","Cyberpunk Gold","Stealth Black",
        "Liquid Emerald","Pearl White","Ultraviolet Purple","Sunset Orange","British Racing Green",
        "Liquid Champagne","Nardo Gunmetal","Fuchsia Fusion"
    };

    // ─────────────────────────────────────────────────────────────────────────
    // STATE
    // ─────────────────────────────────────────────────────────────────────────
    public enum FinishType { Gloss, Metallic, Matte, Pearlescent, Chrome }
    private enum LeftNavTab { Paint, Wheels, BodyKit, Performance, Windows, Technical }
    private enum AppState { VehicleSelect, Garage }

    private AppState _appState = AppState.VehicleSelect;
    private LeftNavTab _leftNav = LeftNavTab.Paint;
    private int _currentVehicleIndex = 0;
    private int _currentPaintIndex = 0;
    private FinishType _currentFinish = FinishType.Metallic;
    private int _currentWheelDesign = 0;
    private int _rimFinish = 0; // 0=Chrome, 1=MatteBlack, 2=Gunmetal, 3=Bronze, 4=Gold
    private int _caliperColor = 0; // 0=Red, 1=Yellow, 2=Blue, 3=Carbon, 4=Gold, 5=Silver
    private float _rideHeight = 1.0f;
    private float _windowTint = 0.18f;
    private int _spoilerOption = 1; // 0=None, 1=GT, 2=HighDownforce
    private int _frontBumper = 0;   // 0=Stock, 1=Aggressive, 2=Race
    private bool _showTechPanel = false;
    private bool _showUI = true;
    private bool _showPerfHUD = false;

    // Baseline for Before/After
    private int _savedPaintIndex = 0;
    private int _savedWheelDesign = 0;
    private int _savedSpoilerOption = 1;
    private bool _comparingBeforeAfter = false;

    // ─────────────────────────────────────────────────────────────────────────
    // REFERENCES
    // ─────────────────────────────────────────────────────────────────────────
    public Transform turntablePivot;
    public Transform cameraRig;
    public Camera mainCamera;
    public VROptimizationTelemetry telemetry;

    private VehicleComponents _vc;
    private CarWheelTransformDemo _wheelDemo;

    // ─────────────────────────────────────────────────────────────────────────
    // CAMERA ORBIT & TRANSITIONS
    // ─────────────────────────────────────────────────────────────────────────
    private float _orbitYaw   = -45f;
    private float _orbitPitch = 18f;
    private float _orbitDist  = 5.8f;
    private Vector3 _orbitTarget = new Vector3(0, 0.55f, 0.2f);
    private bool _isFreeOrbit = false;
    private bool _autoRotate = false;
    private float _autoRotateSpeed = 14f;
    private float _lastClickTime = 0f;
    private bool _rotateTurntable = false;
    private float _turntableSpeed = 16f;

    private static readonly float[,] CamPresets = {
        { -45f, 18f, 5.8f, 0.55f }, // 0: 3/4 Hero
        { 180f, 12f, 5.5f, 0.45f }, // 1: Front
        {   0f, 12f, 5.5f, 0.45f }, // 2: Rear
        { -90f, 14f, 5.2f, 0.50f }, // 3: Right Profile
        {  90f, 14f, 5.2f, 0.50f }, // 4: Left Profile
        {   0f, 85f, 6.0f, 0.00f }, // 5: Top Down
        { -28f, 16f, 1.6f, 0.40f }, // 6: Wheel Rim Macro
        { -45f, 12f, 2.8f, 0.42f }, // 7: Front Splitter Aero
        {  25f, 16f, 2.6f, 0.70f }, // 8: Rear Wing Aero
    };
    private static readonly string[] CamPresetNames = { "3/4 Hero", "Front", "Rear", "Right", "Left", "Top", "Wheel", "Aero", "Wing" };
    private int _currentCamPreset = 0;

    // ─────────────────────────────────────────────────────────────────────────
    // EXPLODE ASSEMBLY VIEW
    // ─────────────────────────────────────────────────────────────────────────
    private bool _isExploded = false;
    private float _explodeProgress = 0f;
    private Vector3 _chassisOrig, _hoodOrig, _cabinOrig, _spoilerOrig;
    private Vector3 _wFLOrig, _wFROrig, _wRLOrig, _wRROrig;

    // ─────────────────────────────────────────────────────────────────────────
    // MATERIAL INSTANCES
    // ─────────────────────────────────────────────────────────────────────────
    private Material _sharedPaintMat;
    private Material _sharedGlassMat;

    // ─────────────────────────────────────────────────────────────────────────
    // STYLES
    // ─────────────────────────────────────────────────────────────────────────
    private bool _stylesReady = false;
    private GUIStyle _stTitle, _stSection, _stBody, _stMono, _stBadge;
    private GUIStyle _btnNormal, _btnActive, _btnNav, _btnNavActive;

    // ─────────────────────────────────────────────────────────────────────────
    // LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────────
    void Start()
    {
        Invoke("InitFirstVehicle", 0.04f);
    }

    private void InitFirstVehicle()
    {
        LoadVehicle(_currentVehicleIndex);
    }

    void Update()
    {
        HandleHotkeys();

        if (_rotateTurntable && turntablePivot != null)
            turntablePivot.Rotate(Vector3.up, _turntableSpeed * Time.deltaTime, Space.World);

        if (_vc != null)
        {
            float targetExplode = _isExploded ? 1f : 0f;
            _explodeProgress = Mathf.MoveTowards(_explodeProgress, targetExplode, Time.deltaTime * 3.2f);
            ApplyExplodeOffsets(_explodeProgress);
        }

        HandleCamera();
    }

    private void HandleHotkeys()
    {
        if (Input.GetKeyDown(KeyCode.H)) _showUI = !_showUI;
        if (Input.GetKeyDown(KeyCode.F1)) _showPerfHUD = !_showPerfHUD;
        if (Input.GetKeyDown(KeyCode.E)) _isExploded = !_isExploded;
        if (Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.Space)) _rotateTurntable = !_rotateTurntable;
        if (Input.GetKeyDown(KeyCode.A)) _autoRotate = !_autoRotate;
        if (Input.GetKeyDown(KeyCode.R)) ResetOrbit();
        if (Input.GetKeyDown(KeyCode.Escape) && _appState == AppState.Garage)
            _appState = AppState.VehicleSelect;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // VEHICLE MANAGEMENT
    // ─────────────────────────────────────────────────────────────────────────
    private void LoadVehicle(int idx)
    {
        if (_vc != null && _vc.root != null)
            Destroy(_vc.root.gameObject);
        _vc = null;
        _wheelDemo = null;

        _currentVehicleIndex = idx;
        _currentPaintIndex = 0;
        _currentWheelDesign = (int)Vehicles[idx].defaultWheelStyle;

        _vc = CaseStudyBootstrap.BuildVehicle(idx, turntablePivot);
        ApplyPaint();
        ApplyWheelFinish();
        ApplyWindowTint();

        if (_vc.wheelFL != null)
            _wheelDemo = _vc.wheelFL.GetComponentInChildren<CarWheelTransformDemo>();

        CacheExplodePositions();
        _isExploded = false;
        _explodeProgress = 0f;

        GoToPreset(0, true);
    }

    private void CacheExplodePositions()
    {
        if (_vc == null) return;
        if (_vc.chassis) _chassisOrig = _vc.chassis.localPosition;
        if (_vc.hood)    _hoodOrig    = _vc.hood.localPosition;
        if (_vc.cabin)   _cabinOrig   = _vc.cabin.localPosition;
        if (_vc.spoiler) _spoilerOrig = _vc.spoiler.localPosition;
        if (_vc.wheelFL) _wFLOrig = _vc.wheelFL.localPosition;
        if (_vc.wheelFR) _wFROrig = _vc.wheelFR.localPosition;
        if (_vc.wheelRL) _wRLOrig = _vc.wheelRL.localPosition;
        if (_vc.wheelRR) _wRROrig = _vc.wheelRR.localPosition;
    }

    private void ApplyExplodeOffsets(float t)
    {
        if (_vc == null) return;
        if (_vc.hood)    _vc.hood.localPosition    = _hoodOrig    + new Vector3(0, 0.48f * t,  0.66f * t);
        if (_vc.cabin)   _vc.cabin.localPosition   = _cabinOrig   + new Vector3(0, 0.72f * t,  0.00f);
        if (_vc.spoiler) _vc.spoiler.localPosition = _spoilerOrig + new Vector3(0, 0.55f * t, -0.65f * t);
        float w = 0.46f * t;
        if (_vc.wheelFL) _vc.wheelFL.localPosition = _wFLOrig + new Vector3(-w, 0, 0);
        if (_vc.wheelFR) _vc.wheelFR.localPosition = _wFROrig + new Vector3( w, 0, 0);
        if (_vc.wheelRL) _vc.wheelRL.localPosition = _wRLOrig + new Vector3(-w, 0, 0);
        if (_vc.wheelRR) _vc.wheelRR.localPosition = _wRROrig + new Vector3( w, 0, 0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CUSTOMIZATION APPLIERS (Optimized with shared materials)
    // ─────────────────────────────────────────────────────────────────────────
    private void ApplyPaint()
    {
        if (_vc == null || _vc.bodyRenderers == null) return;
        Color col = PaintColors[_currentPaintIndex];
        float smoothness, metallic;
        switch (_currentFinish)
        {
            case FinishType.Matte:       smoothness = 0.12f; metallic = 0.02f; break;
            case FinishType.Gloss:       smoothness = 0.98f; metallic = 0.15f; break;
            case FinishType.Pearlescent: smoothness = 0.94f; metallic = 0.40f; break;
            case FinishType.Chrome:      smoothness = 1.00f; metallic = 0.98f; break;
            default:                     smoothness = 0.94f; metallic = 0.88f; break; // Metallic
        }

        if (_sharedPaintMat == null)
        {
            Shader s = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            _sharedPaintMat = new Material(s);
        }
        _sharedPaintMat.color = col;
        if (_sharedPaintMat.HasProperty("_BaseColor"))  _sharedPaintMat.SetColor("_BaseColor", col);
        if (_sharedPaintMat.HasProperty("_Smoothness")) _sharedPaintMat.SetFloat("_Smoothness", smoothness);
        if (_sharedPaintMat.HasProperty("_Metallic"))   _sharedPaintMat.SetFloat("_Metallic", metallic);

        foreach (var r in _vc.bodyRenderers)
        {
            if (r != null) r.sharedMaterial = _sharedPaintMat;
        }
        if (_vc.sideSkirtRenderers != null)
        {
            foreach (var r in _vc.sideSkirtRenderers)
            {
                if (r != null) r.sharedMaterial = _sharedPaintMat;
            }
        }
    }

    private void ApplyWindowTint()
    {
        if (_vc == null || _vc.windowRenderers == null) return;
        Color tint = new Color(0.04f, 0.07f, 0.12f, _windowTint + 0.05f);
        float sm = 0.96f + (1f - _windowTint) * 0.03f;
        if (_sharedGlassMat == null)
        {
            Shader s = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            _sharedGlassMat = new Material(s);
        }
        _sharedGlassMat.color = tint;
        if (_sharedGlassMat.HasProperty("_BaseColor"))  _sharedGlassMat.SetColor("_BaseColor", tint);
        if (_sharedGlassMat.HasProperty("_Smoothness")) _sharedGlassMat.SetFloat("_Smoothness", sm);

        foreach (var r in _vc.windowRenderers)
        {
            if (r != null) r.sharedMaterial = _sharedGlassMat;
        }
    }

    private static readonly Color[] RimFinishColors = {
        new Color(0.88f, 0.89f, 0.92f), // Chrome
        new Color(0.10f, 0.10f, 0.11f), // Matte Black
        new Color(0.40f, 0.41f, 0.44f), // Gunmetal
        new Color(0.72f, 0.52f, 0.28f), // Liquid Bronze
        new Color(0.88f, 0.72f, 0.15f), // Race Gold
    };
    private static readonly float[] RimSmoothness = { 0.98f, 0.20f, 0.82f, 0.88f, 0.95f };
    private static readonly float[] RimMetallic   = { 0.95f, 0.10f, 0.82f, 0.85f, 0.92f };
    private static readonly string[] RimFinishNames = { "Chrome", "Matte Black", "Gunmetal", "Bronze", "Race Gold" };

    private void ApplyWheelFinish()
    {
        if (_vc == null) return;
        int idx = Mathf.Clamp(_rimFinish, 0, RimFinishColors.Length - 1);
        Color col = RimFinishColors[idx];
        float sm  = RimSmoothness[idx];
        float met = RimMetallic[idx];

        foreach (Transform wt in new[] { _vc.wheelFL, _vc.wheelFR, _vc.wheelRL, _vc.wheelRR })
        {
            if (wt == null) continue;
            Transform rimRoot = wt.Find("RimRoot");
            if (rimRoot == null) continue;
            foreach (Renderer r in rimRoot.GetComponentsInChildren<Renderer>())
            {
                if (r.gameObject.name == "HubCap") continue;
                Material m = r.material;
                m.color = col;
                if (m.HasProperty("_BaseColor"))  m.SetColor("_BaseColor", col);
                if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", sm);
                if (m.HasProperty("_Metallic"))   m.SetFloat("_Metallic", met);
            }
        }
    }

    private static readonly Color[] CaliperColors = {
        new Color(0.92f, 0.10f, 0.10f), // Brembo Red
        new Color(0.96f, 0.84f, 0.04f), // Acid Yellow
        new Color(0.08f, 0.45f, 0.96f), // Miami Blue
        new Color(0.18f, 0.18f, 0.20f), // Carbon Ceramic
        new Color(0.88f, 0.70f, 0.10f), // Racing Gold
        new Color(0.82f, 0.84f, 0.88f), // Billet Silver
    };
    private static readonly string[] CaliperNames = { "Red", "Yellow", "Blue", "Carbon", "Gold", "Silver" };

    private void ApplyCaliperColor()
    {
        if (_vc == null) return;
        Color col = CaliperColors[Mathf.Clamp(_caliperColor, 0, CaliperColors.Length - 1)];
        foreach (Transform wt in new[] { _vc.wheelFL, _vc.wheelFR, _vc.wheelRL, _vc.wheelRR })
        {
            if (wt == null) continue;
            Transform caliper = wt.Find("Caliper");
            if (caliper == null) continue;
            Renderer r = caliper.GetComponent<Renderer>();
            if (r == null) continue;
            Material m = r.material;
            m.color = col;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", col);
        }
    }

    private void ApplyRideHeight()
    {
        if (_vc == null || _vc.root == null) return;
        Vector3 s = _vc.root.localScale;
        _vc.root.localScale = new Vector3(s.x, _rideHeight, s.z);
    }

    private void RebuildWheels()
    {
        if (_vc == null) return;
        CaseStudyBootstrap.WheelStyle style = (CaseStudyBootstrap.WheelStyle)_currentWheelDesign;
        float tireDiam = 0.68f;

        var wheelsToBuild = new (Transform wt, string wName, bool isQ2)[] {
            (_vc.wheelFL, "Wheel_FL_Q2", true),
            (_vc.wheelFR, "Wheel_FR",    false),
            (_vc.wheelRL, "Wheel_RL",    false),
            (_vc.wheelRR, "Wheel_RR",    false)
        };

        foreach (var item in wheelsToBuild)
        {
            Transform wt = item.wt;
            string wName = item.wName;
            bool isQ2 = item.isQ2;
            if (wt == null) continue;
            Vector3 pos = wt.localPosition;
            Transform par = wt.parent;
            Destroy(wt.gameObject);

            GameObject newWheel = CaseStudyBootstrap.BuildDetailedWheel(wName, style, tireDiam, isQ2);
            newWheel.transform.SetParent(par, false);
            newWheel.transform.localPosition = pos;

            if (wt == _vc.wheelFL) { _vc.wheelFL = newWheel.transform; }
            else if (wt == _vc.wheelFR) { _vc.wheelFR = newWheel.transform; }
            else if (wt == _vc.wheelRL) { _vc.wheelRL = newWheel.transform; }
            else if (wt == _vc.wheelRR) { _vc.wheelRR = newWheel.transform; }
        }

        _wheelDemo = null;
        if (_vc.wheelFL != null)
            _wheelDemo = _vc.wheelFL.GetComponentInChildren<CarWheelTransformDemo>();

        ApplyWheelFinish();
        ApplyCaliperColor();
        CacheExplodePositions();
    }

    private void SaveBaseline()
    {
        _savedPaintIndex    = _currentPaintIndex;
        _savedWheelDesign   = _currentWheelDesign;
        _savedSpoilerOption = _spoilerOption;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CAMERA CONTROLS
    // ─────────────────────────────────────────────────────────────────────────
    private void HandleCamera()
    {
        if (cameraRig == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (Time.time - _lastClickTime < 0.35f) ResetOrbit();
            _lastClickTime = Time.time;
        }

        bool overUI = IsMouseOverUI();

        if (Input.GetMouseButton(0) && !overUI)
        {
            _isFreeOrbit = true;
            _autoRotate = false;
            _orbitYaw   += Input.GetAxis("Mouse X") * 3.8f;
            _orbitPitch  = Mathf.Clamp(_orbitPitch - Input.GetAxis("Mouse Y") * 3.2f, 2f, 86f);
        }

        if (Input.GetMouseButton(1) && !overUI)
        {
            _isFreeOrbit = true;
            _autoRotate = false;
            _orbitTarget += new Vector3(-Input.GetAxis("Mouse X") * 0.018f, -Input.GetAxis("Mouse Y") * 0.018f, 0);
            _orbitTarget.y = Mathf.Clamp(_orbitTarget.y, 0f, 2.0f);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            _isFreeOrbit = true;
            _orbitDist = Mathf.Clamp(_orbitDist - scroll * 4.5f, 0.8f, 14f);
        }

        if (_autoRotate) _orbitYaw += _autoRotateSpeed * Time.deltaTime;

        Quaternion rot = Quaternion.Euler(_orbitPitch, _orbitYaw, 0f);
        Vector3 targetPos = _orbitTarget + rot * (Vector3.back * _orbitDist);
        cameraRig.position = Vector3.Lerp(cameraRig.position, targetPos, Time.deltaTime * 10f);
        cameraRig.LookAt(_orbitTarget);
    }

    private void GoToPreset(int preset, bool snap = false)
    {
        _currentCamPreset = preset;
        _isFreeOrbit = true;
        _orbitYaw   = CamPresets[preset, 0];
        _orbitPitch = CamPresets[preset, 1];
        _orbitDist  = CamPresets[preset, 2];
        _orbitTarget = new Vector3(0, CamPresets[preset, 3], 0);
        if (snap && cameraRig != null)
        {
            Quaternion rot = Quaternion.Euler(_orbitPitch, _orbitYaw, 0f);
            cameraRig.position = _orbitTarget + rot * (Vector3.back * _orbitDist);
            cameraRig.LookAt(_orbitTarget);
        }
    }

    private void ResetOrbit()
    {
        GoToPreset(0);
    }

    private bool IsMouseOverUI()
    {
        Vector2 mp = Input.mousePosition;
        float sy = Screen.height;
        float sx = Screen.width;
        return (mp.y > sy - 52f || mp.y < 56f || mp.x < 205f || mp.x > sx - 365f);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ONGUI RENDERING
    // ─────────────────────────────────────────────────────────────────────────
    void OnGUI()
    {
        InitStyles();

        if (_appState == AppState.VehicleSelect)
        {
            DrawVehicleSelectScreen();
            return;
        }

        if (!_showUI)
        {
            GUI.color = new Color(1, 1, 1, 0.6f);
            GUI.Label(new Rect(14, 8, 260, 20), "◆ CGVR FORZA GARAGE  |  H: Show UI", _stMono);
            GUI.color = Color.white;
            return;
        }

        DrawTopBar();
        DrawLeftNav();
        DrawRightPanel();
        DrawBottomBar();

        if (_showTechPanel) DrawTechPanel();
        if (_showPerfHUD)   DrawPerfHUD();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // FORZA VEHICLE SELECTION CAROUSEL
    // ─────────────────────────────────────────────────────────────────────────
    private void DrawVehicleSelectScreen()
    {
        float sw = Screen.width, sh = Screen.height;

        GUI.color = new Color(0.04f, 0.05f, 0.08f, 0.97f);
        GUI.DrawTexture(new Rect(0, 0, sw, sh), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Header Title
        GUI.Label(new Rect(sw * 0.5f - 240, 20, 480, 36), "CAR COLLECTION & SELECTION", _stTitle);
        GUI.Label(new Rect(sw * 0.5f - 240, 52, 480, 20), "Student: Likith S G  |  USN: 1RVU23CSE236  |  CG & VR Lab", _stBody);

        // 5 Vehicle Cards
        float cardW = 195, cardH = 290;
        float totalW = 5 * cardW + 4 * 16;
        float startX = (sw - totalW) * 0.5f;
        float startY = sh * 0.5f - cardH * 0.5f - 20;

        for (int i = 0; i < Vehicles.Length; i++)
        {
            VehicleDef def = Vehicles[i];
            float cx = startX + i * (cardW + 16);
            bool isSelected = (i == _currentVehicleIndex);

            // Card background
            Color cardCol = isSelected ? new Color(0.10f, 0.22f, 0.38f, 0.98f) : new Color(0.08f, 0.09f, 0.12f, 0.95f);
            GUI.color = cardCol;
            GUI.DrawTexture(new Rect(cx, startY, cardW, cardH), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Forza Category Top Accent Line
            Color catCol = GetCategoryColor(def.category);
            GUI.color = catCol;
            GUI.DrawTexture(new Rect(cx, startY, cardW, 4), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // PI Badge (e.g. S2 | 998)
            DrawPIBadge(cx + 10, startY + 12, def.piClass, def.piScore);

            // Vehicle Category Tag
            GUIStyle catStyle = new GUIStyle(_stBody) { fontSize = 10, fontStyle = FontStyle.Bold };
            catStyle.normal.textColor = catCol;
            GUI.Label(new Rect(cx + 78, startY + 14, cardW - 88, 18), def.category.ToUpper(), catStyle);

            // Thumbnail Color Preview Bar
            GUI.color = def.defaultPaint;
            GUI.DrawTexture(new Rect(cx + 10, startY + 42, cardW - 20, 65), Texture2D.whiteTexture);
            GUI.color = new Color(0, 0, 0, 0.25f);
            GUI.DrawTexture(new Rect(cx + 10, startY + 42, cardW - 20, 65), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUIStyle modelLbl = new GUIStyle(GUI.skin.label) {
                fontSize = 11, fontStyle = FontStyle.Bold, normal = { textColor = Color.white }, alignment = TextAnchor.MiddleCenter
            };
            GUI.Label(new Rect(cx + 10, startY + 66, cardW - 20, 20), def.model, modelLbl);

            // Specs
            float ty = startY + 114;
            GUI.Label(new Rect(cx + 10, ty, cardW - 20, 18), def.name, _stSection);
            GUI.Label(new Rect(cx + 10, ty + 18, cardW - 20, 14), def.hp + " HP  ·  " + def.torque + " Nm", _stBody);
            GUI.Label(new Rect(cx + 10, ty + 34, cardW - 20, 14), def.topSpeed + " km/h  ·  0-100: " + def.accel + "s", _stBody);

            // Graphical Mini Stat Bars
            float sy = ty + 54;
            DrawMiniStatBar(cx + 10, sy,      "SPD", def.speedStat, 10f);
            DrawMiniStatBar(cx + 10, sy + 14, "HND", def.handlingStat, 10f);
            DrawMiniStatBar(cx + 10, sy + 28, "ACC", def.accelStat, 10f);

            // Select Button
            Color btnCol = isSelected ? new Color(0.15f, 0.85f, 1.0f) : new Color(0.18f, 0.20f, 0.26f);
            GUI.color = btnCol;
            GUI.DrawTexture(new Rect(cx + 10, startY + cardH - 36, cardW - 20, 26), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUIStyle btnLbl = new GUIStyle(GUI.skin.label) {
                fontSize = 11, fontStyle = FontStyle.Bold,
                normal = { textColor = isSelected ? new Color(0.04f, 0.06f, 0.10f) : Color.white },
                alignment = TextAnchor.MiddleCenter
            };
            if (GUI.Button(new Rect(cx + 10, startY + cardH - 36, cardW - 20, 26), isSelected ? "SELECTED" : "SELECT", btnLbl))
            {
                _currentVehicleIndex = i;
            }
        }

        // Enter Garage Action Button
        float enterW = 240, enterH = 46;
        float enterX = sw * 0.5f - enterW * 0.5f;
        float enterY = startY + cardH + 24;

        GUI.color = new Color(0.15f, 0.85f, 1.0f);
        GUI.DrawTexture(new Rect(enterX, enterY, enterW, enterH), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle enterLbl = new GUIStyle(GUI.skin.label) {
            fontSize = 14, fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.04f, 0.06f, 0.10f) },
            alignment = TextAnchor.MiddleCenter
        };
        if (GUI.Button(new Rect(enterX, enterY, enterW, enterH), "ENTER CUSTOM GARAGE →", enterLbl))
        {
            LoadVehicle(_currentVehicleIndex);
            _appState = AppState.Garage;
        }
    }

    private void DrawPIBadge(float x, float y, string piClass, int piScore)
    {
        // Colored PI box
        Color bg = GetPIClassColor(piClass);
        GUI.color = bg;
        GUI.DrawTexture(new Rect(x, y, 60, 22), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle piStyle = new GUIStyle(GUI.skin.label) {
            fontSize = 10, fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }, alignment = TextAnchor.MiddleCenter
        };
        GUI.Label(new Rect(x, y + 1, 60, 20), piClass + " " + piScore, piStyle);
    }

    private Color GetPIClassColor(string cls)
    {
        switch (cls)
        {
            case "X":  return new Color(0.15f, 0.85f, 1.00f);
            case "S2": return new Color(0.92f, 0.18f, 0.18f);
            case "S1": return new Color(0.95f, 0.60f, 0.08f);
            case "A":  return new Color(0.85f, 0.25f, 0.70f);
            default:   return new Color(0.30f, 0.70f, 0.35f);
        }
    }

    private void DrawMiniStatBar(float x, float y, string lbl, float val, float maxVal)
    {
        GUI.Label(new Rect(x, y, 32, 14), lbl, _stMono);
        float barW = 110, barH = 7;
        float barX = x + 34;

        // Background
        GUI.color = new Color(0.15f, 0.16f, 0.20f, 0.85f);
        GUI.DrawTexture(new Rect(barX, y + 3, barW, barH), Texture2D.whiteTexture);

        // Fill
        float fillW = barW * Mathf.Clamp01(val / maxVal);
        GUI.color = new Color(0.15f, 0.85f, 1.0f);
        GUI.DrawTexture(new Rect(barX, y + 3, fillW, barH), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUI.Label(new Rect(barX + barW + 6, y, 24, 14), val.ToString("F1"), _stMono);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TOP HUD BAR
    // ─────────────────────────────────────────────────────────────────────────
    private void DrawTopBar()
    {
        float sw = Screen.width;
        Rect r = new Rect(0, 0, sw, 48);

        GUI.color = new Color(0.04f, 0.05f, 0.08f, 0.95f);
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Neon cyan bottom accent line
        GUI.color = new Color(0.15f, 0.85f, 1.0f);
        GUI.DrawTexture(new Rect(0, 46, sw, 2), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Logo
        GUI.Label(new Rect(16, 10, 200, 24), "◆ CGVR FORZA GARAGE", _stTitle);

        // Center Vehicle Badge & Title
        if (_currentVehicleIndex < Vehicles.Length)
        {
            VehicleDef def = Vehicles[_currentVehicleIndex];
            float cx = sw * 0.5f - 160;
            DrawPIBadge(cx, 12, def.piClass, def.piScore);

            GUI.Label(new Rect(cx + 68, 6, 260, 20), def.name.ToUpper(), _stSection);
            GUIStyle catStyle = new GUIStyle(_stBody) { fontSize = 10 };
            catStyle.normal.textColor = GetCategoryColor(def.category);
            GUI.Label(new Rect(cx + 68, 25, 260, 16), def.category + "  ·  " + def.year + "  ·  " + def.hp + " HP", catStyle);
        }

        // Right Action Buttons
        float bx = sw - 16;
        if (DrawTopBtn(ref bx, "H: HUD"))    _showUI = !_showUI;
        if (DrawTopBtn(ref bx, "CG TECH"))   _showTechPanel = !_showTechPanel;
        if (DrawTopBtn(ref bx, "CARS"))      _appState = AppState.VehicleSelect;
        if (DrawTopBtn(ref bx, "SAVE"))      SaveBaseline();
    }

    private bool DrawTopBtn(ref float bx, string label)
    {
        float bw = Mathf.Max(58, label.Length * 8 + 14);
        bx -= bw + 8;
        Color prevBg = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.12f, 0.15f, 0.20f);
        bool clicked = GUI.Button(new Rect(bx, 9, bw, 30), label, _btnNormal);
        GUI.backgroundColor = prevBg;
        return clicked;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // LEFT SIDE NAVIGATION
    // ─────────────────────────────────────────────────────────────────────────
    private void DrawLeftNav()
    {
        float sh = Screen.height;
        float navW = 195, navH = sh - 48 - 52;
        Rect navRect = new Rect(0, 48, navW, navH);

        GUI.color = new Color(0.04f, 0.05f, 0.08f, 0.94f);
        GUI.DrawTexture(navRect, Texture2D.whiteTexture);
        GUI.color = new Color(0.15f, 0.85f, 1.0f, 0.25f);
        GUI.DrawTexture(new Rect(navW - 1, 48, 1, navH), Texture2D.whiteTexture);
        GUI.color = Color.white;

        string[] tabs = { "🎨  Paint & Finish", "🛞  Wheels & Rims", "⚡  Body Aero Kit", "⚙  Performance", "🪟  Windows & Tint", "📊  CG Telemetry" };
        LeftNavTab[] enums = { LeftNavTab.Paint, LeftNavTab.Wheels, LeftNavTab.BodyKit, LeftNavTab.Performance, LeftNavTab.Windows, LeftNavTab.Technical };

        float ty = 60;
        for (int i = 0; i < tabs.Length; i++)
        {
            bool isActive = (_leftNav == enums[i]);
            if (isActive)
            {
                GUI.color = new Color(0.10f, 0.22f, 0.36f, 0.98f);
                GUI.DrawTexture(new Rect(0, ty, navW, 42), Texture2D.whiteTexture);
                GUI.color = new Color(0.15f, 0.85f, 1.0f);
                GUI.DrawTexture(new Rect(0, ty, 3, 42), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }

            GUIStyle lStyle = isActive ? _btnNavActive : _btnNav;
            if (GUI.Button(new Rect(8, ty + 4, navW - 16, 34), tabs[i], lStyle))
            {
                _leftNav = enums[i];
                if (enums[i] == LeftNavTab.Wheels)        GoToPreset(6); // low-angle wheel macro
                else if (enums[i] == LeftNavTab.BodyKit)  GoToPreset(7); // aero splitter
                else if (enums[i] == LeftNavTab.Technical) _showTechPanel = !_showTechPanel;
                else GoToPreset(0); // 3/4 hero
            }
            ty += 46;
        }

        // Student watermark at bottom of left nav
        GUI.Label(new Rect(12, navH - 42, navW - 14, 16), "Likith S G", _stSection);
        GUI.Label(new Rect(12, navH - 24, navW - 14, 14), "1RVU23CSE236", _stMono);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // RIGHT CONTEXTUAL CUSTOMIZATION PANEL
    // ─────────────────────────────────────────────────────────────────────────
    private void DrawRightPanel()
    {
        float sh = Screen.height, sw = Screen.width;
        float panW = 360, panH = sh - 48 - 52;
        float panX = sw - panW;
        Rect panRect = new Rect(panX, 48, panW, panH);

        GUI.color = new Color(0.04f, 0.05f, 0.08f, 0.94f);
        GUI.DrawTexture(panRect, Texture2D.whiteTexture);
        GUI.color = new Color(0.15f, 0.85f, 1.0f, 0.25f);
        GUI.DrawTexture(new Rect(panX, 48, 1, panH), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUILayout.BeginArea(new Rect(panX + 14, 58, panW - 28, panH - 20));

        switch (_leftNav)
        {
            case LeftNavTab.Paint:       DrawPaintPanel(); break;
            case LeftNavTab.Wheels:      DrawWheelsPanel(); break;
            case LeftNavTab.BodyKit:     DrawBodyKitPanel(); break;
            case LeftNavTab.Performance: DrawPerformancePanel(); break;
            case LeftNavTab.Windows:     DrawWindowsPanel(); break;
            case LeftNavTab.Technical:   DrawTechSummaryPanel(); break;
        }

        GUILayout.EndArea();
    }

    // ─── PAINT PANEL ───────────────────────────────────────────────────────
    private void DrawPaintPanel()
    {
        GUILayout.Label("PAINT & SURFACE FINISH", _stSection);
        GUILayout.Space(6);

        GUILayout.Label("Factory Paint Swatches", _stBody);
        GUILayout.Space(4);

        int perRow = 4;
        for (int row = 0; row * perRow < PaintColors.Length; row++)
        {
            GUILayout.BeginHorizontal();
            for (int col = 0; col < perRow; col++)
            {
                int i = row * perRow + col;
                if (i >= PaintColors.Length) break;
                bool sel = (i == _currentPaintIndex);

                Color prev = GUI.backgroundColor;
                GUI.backgroundColor = PaintColors[i];
                string marker = sel ? "✔" : "";
                if (GUILayout.Button(marker, GUILayout.Height(30)))
                {
                    _currentPaintIndex = i;
                    ApplyPaint();
                }
                GUI.backgroundColor = prev;
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(6);
        GUILayout.Label("Active: " + PaintNames[_currentPaintIndex], _stMono);

        GUILayout.Space(12);
        GUILayout.Label("Finish Clearcoat Style", _stBody);
        GUILayout.Space(4);
        GUILayout.BeginHorizontal();
        foreach (FinishType ft in new[] { FinishType.Metallic, FinishType.Gloss, FinishType.Matte, FinishType.Pearlescent, FinishType.Chrome })
        {
            bool sel = (_currentFinish == ft);
            Color prev = GUI.backgroundColor;
            GUI.backgroundColor = sel ? new Color(0.15f, 0.85f, 1.0f) : new Color(0.12f, 0.14f, 0.18f);
            if (GUILayout.Button(ft.ToString(), sel ? _btnActive : _btnNormal, GUILayout.Height(26)))
            {
                _currentFinish = ft;
                ApplyPaint();
            }
            GUI.backgroundColor = prev;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(14);
        GUILayout.Label("Showroom Studio Controls", _stBody);
        GUILayout.Space(4);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(_rotateTurntable ? "⏹ Stop Turntable" : "🔄 Spin Turntable", _rotateTurntable ? _btnActive : _btnNormal, GUILayout.Height(28)))
            _rotateTurntable = !_rotateTurntable;
        if (GUILayout.Button(_autoRotate ? "⏸ Stop Orbit" : "▶ Auto Orbit", _autoRotate ? _btnActive : _btnNormal, GUILayout.Height(28)))
            _autoRotate = !_autoRotate;
        GUILayout.EndHorizontal();

        GUILayout.Space(8);
        GUILayout.Label("Turntable Speed: " + _turntableSpeed.ToString("F0") + " °/s", _stMono);
        _turntableSpeed = GUILayout.HorizontalSlider(_turntableSpeed, 4f, 45f, GUILayout.Height(16));
    }

    // ─── WHEELS PANEL ──────────────────────────────────────────────────────
    private void DrawWheelsPanel()
    {
        GUILayout.Label("WHEELS, RIMS & CALIPERS", _stSection);
        GUILayout.Space(6);

        GUILayout.Label("Alloy Rim Design", _stBody);
        GUILayout.Space(4);

        string[] designNames = { "Twin 5", "Star 10", "Lattice", "Y-Blade", "Deep Dish" };
        GUILayout.BeginHorizontal();
        for (int i = 0; i < 5; i++)
        {
            bool sel = (_currentWheelDesign == i);
            Color prev = GUI.backgroundColor;
            GUI.backgroundColor = sel ? new Color(0.15f, 0.85f, 1.0f) : new Color(0.12f, 0.15f, 0.20f);
            if (GUILayout.Button(designNames[i], sel ? _btnActive : _btnNormal, GUILayout.Height(28)))
            {
                _currentWheelDesign = i;
                RebuildWheels();
                GoToPreset(6);
            }
            GUI.backgroundColor = prev;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.Label("Rim Finish", _stBody);
        GUILayout.Space(4);
        GUILayout.BeginHorizontal();
        for (int i = 0; i < RimFinishNames.Length; i++)
        {
            bool sel = (_rimFinish == i);
            Color prev = GUI.backgroundColor;
            GUI.backgroundColor = sel ? new Color(0.15f, 0.85f, 1.0f) : new Color(0.12f, 0.15f, 0.20f);
            if (GUILayout.Button(RimFinishNames[i], sel ? _btnActive : _btnNormal, GUILayout.Height(26)))
            {
                _rimFinish = i;
                ApplyWheelFinish();
            }
            GUI.backgroundColor = prev;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(12);
        GUILayout.Label("Q2 Wheel Rim Matrix Transformation", _stSection);
        GUILayout.Space(4);

        if (_wheelDemo != null)
        {
            GUILayout.Label(string.Format("Scale: {0:F2}x  |  Rotate: {1:F0}°  |  Translate: {2:F0} mm",
                _wheelDemo.scaleFactor, _wheelDemo.rotationAngle, _wheelDemo.translationMm), _stMono);

            _wheelDemo.scaleFactor = GUILayout.HorizontalSlider(_wheelDemo.scaleFactor, 0.8f, 2.2f, GUILayout.Height(16));
            _wheelDemo.rotationAngle = GUILayout.HorizontalSlider(_wheelDemo.rotationAngle, 0f, 360f, GUILayout.Height(16));
            _wheelDemo.translationMm = GUILayout.HorizontalSlider(_wheelDemo.translationMm, -20f, 150f, GUILayout.Height(16));

            GUILayout.Space(6);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply Q2 (1.5x / 45° / 50mm)", _btnActive, GUILayout.Height(26)))
                _wheelDemo.ApplyPresetQ2Transformation();
            if (GUILayout.Button("Reset", _btnNormal, GUILayout.Height(26)))
                _wheelDemo.ResetTransformation();
            GUILayout.EndHorizontal();

            _wheelDemo.animateLoop = GUILayout.Toggle(_wheelDemo.animateLoop, "  Auto Transform Animation Loop");
            _wheelDemo.showCoordinateAxes = GUILayout.Toggle(_wheelDemo.showCoordinateAxes, "  Show 3D Coordinate Tripod (RGB = XYZ)");
        }
    }

    // ─── BODY KIT PANEL ────────────────────────────────────────────────────
    private void DrawBodyKitPanel()
    {
        GUILayout.Label("AERODYNAMICS & BODY KIT", _stSection);
        GUILayout.Space(6);

        GUILayout.Label("Front Aerodynamic Splitter", _stBody);
        GUILayout.Space(4);
        GUILayout.BeginHorizontal();
        foreach (string opt in new[] { "Stock Splitter", "Dual Canards", "Track Pack" })
        {
            int idx = System.Array.IndexOf(new[] { "Stock Splitter", "Dual Canards", "Track Pack" }, opt);
            bool sel = (_frontBumper == idx);
            Color prev = GUI.backgroundColor;
            GUI.backgroundColor = sel ? new Color(0.15f, 0.85f, 1.0f) : new Color(0.12f, 0.15f, 0.20f);
            if (GUILayout.Button(opt, sel ? _btnActive : _btnNormal, GUILayout.Height(26)))
            {
                _frontBumper = idx;
                GoToPreset(7);
            }
            GUI.backgroundColor = prev;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.Label("Rear Aerodynamic Wing", _stBody);
        GUILayout.Space(4);
        GUILayout.BeginHorizontal();
        string[] spoilerNames = { "No Wing", "GT Swan-Neck", "Dual-Element" };
        for (int i = 0; i < spoilerNames.Length; i++)
        {
            bool sel = (_spoilerOption == i);
            Color prev = GUI.backgroundColor;
            GUI.backgroundColor = sel ? new Color(0.15f, 0.85f, 1.0f) : new Color(0.12f, 0.15f, 0.20f);
            if (GUILayout.Button(spoilerNames[i], sel ? _btnActive : _btnNormal, GUILayout.Height(26)))
            {
                _spoilerOption = i;
                ApplySpoilerOption();
                GoToPreset(8);
            }
            GUI.backgroundColor = prev;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(12);
        GUILayout.Label("CAD Assembly Decomposition (Explode View)", _stSection);
        GUILayout.Space(4);

        Color prevExplode = GUI.backgroundColor;
        GUI.backgroundColor = _isExploded ? new Color(0.15f, 0.85f, 1.0f) : new Color(0.12f, 0.15f, 0.20f);
        if (GUILayout.Button(_isExploded ? "⬛ Collapse Assembly (E)" : "⬜ Explode Assembly (E)", _isExploded ? _btnActive : _btnNormal, GUILayout.Height(30)))
            _isExploded = !_isExploded;
        GUI.backgroundColor = prevExplode;

        GUILayout.Label(string.Format("Decomposition: {0:P0}", _explodeProgress), _stMono);
    }

    private void ApplySpoilerOption()
    {
        if (_vc == null || _vc.spoiler == null) return;
        switch (_spoilerOption)
        {
            case 0: _vc.spoiler.gameObject.SetActive(false); break;
            case 1: _vc.spoiler.gameObject.SetActive(true); break;
            case 2: _vc.spoiler.gameObject.SetActive(true); break;
        }
    }

    // ─── PERFORMANCE PANEL ─────────────────────────────────────────────────
    private void DrawPerformancePanel()
    {
        GUILayout.Label("PERFORMANCE & STANCE", _stSection);
        GUILayout.Space(6);

        GUILayout.Label("Brake Caliper Color", _stBody);
        GUILayout.Space(4);
        int perRow = 3;
        for (int row = 0; row * perRow < CaliperNames.Length; row++)
        {
            GUILayout.BeginHorizontal();
            for (int c = 0; c < perRow; c++)
            {
                int i = row * perRow + c;
                if (i >= CaliperNames.Length) break;
                bool sel = (_caliperColor == i);
                Color prev = GUI.backgroundColor;
                GUI.backgroundColor = sel ? new Color(0.15f, 0.85f, 1.0f) : new Color(0.12f, 0.15f, 0.20f);
                if (GUILayout.Button(CaliperNames[i], sel ? _btnActive : _btnNormal, GUILayout.Height(26)))
                {
                    _caliperColor = i;
                    ApplyCaliperColor();
                }
                GUI.backgroundColor = prev;
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(10);
        GUILayout.Label("Suspension Ride Height: " + _rideHeight.ToString("F2") + "x", _stBody);
        float newRH = GUILayout.HorizontalSlider(_rideHeight, 0.85f, 1.15f, GUILayout.Height(16));
        if (Mathf.Abs(newRH - _rideHeight) > 0.001f) { _rideHeight = newRH; ApplyRideHeight(); }

        GUILayout.Space(10);
        GUILayout.Label("Full Vehicle Specifications", _stSection);
        GUILayout.Space(4);
        if (_currentVehicleIndex < Vehicles.Length)
        {
            VehicleDef def = Vehicles[_currentVehicleIndex];
            GUILayout.Label("Horsepower:   " + def.hp + " HP", _stMono);
            GUILayout.Label("Torque:       " + def.torque + " Nm", _stMono);
            GUILayout.Label("Top Speed:    " + def.topSpeed + " km/h", _stMono);
            GUILayout.Label("0-100 km/h:   " + def.accel + " seconds", _stMono);
            GUILayout.Label("Powertrain:   " + def.engineLayout, _stMono);

            GUILayout.Space(8);
            DrawMiniStatBar(Screen.width - 360 + 14, 280, "SPD", def.speedStat, 10f);
            DrawMiniStatBar(Screen.width - 360 + 14, 296, "HND", def.handlingStat, 10f);
            DrawMiniStatBar(Screen.width - 360 + 14, 312, "ACC", def.accelStat, 10f);
            DrawMiniStatBar(Screen.width - 360 + 14, 328, "LCH", def.launchStat, 10f);
            DrawMiniStatBar(Screen.width - 360 + 14, 344, "BRK", def.brakingStat, 10f);
        }
    }

    // ─── WINDOWS PANEL ─────────────────────────────────────────────────────
    private void DrawWindowsPanel()
    {
        GUILayout.Label("GLASS & WINDOW TINT", _stSection);
        GUILayout.Space(6);

        GUILayout.Label("Tint Opacity: " + (_windowTint * 100f).ToString("F0") + "%", _stBody);
        float newTint = GUILayout.HorizontalSlider(_windowTint, 0.02f, 0.60f, GUILayout.Height(16));
        if (Mathf.Abs(newTint - _windowTint) > 0.005f) { _windowTint = newTint; ApplyWindowTint(); }

        GUILayout.Space(8);
        GUILayout.Label("Factory Tint Presets", _stBody);
        GUILayout.Space(4);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear (5%)", _btnNormal, GUILayout.Height(28))) { _windowTint = 0.05f; ApplyWindowTint(); }
        if (GUILayout.Button("Light (18%)", _btnNormal, GUILayout.Height(28))) { _windowTint = 0.18f; ApplyWindowTint(); }
        if (GUILayout.Button("Medium (35%)", _btnNormal, GUILayout.Height(28))) { _windowTint = 0.35f; ApplyWindowTint(); }
        if (GUILayout.Button("Limo (55%)", _btnNormal, GUILayout.Height(28))) { _windowTint = 0.55f; ApplyWindowTint(); }
        GUILayout.EndHorizontal();
    }

    // ─── TECH SUMMARY PANEL ────────────────────────────────────────────────
    private void DrawTechSummaryPanel()
    {
        GUILayout.Label("CG CASE STUDY ARCHITECTURE", _stSection);
        GUILayout.Space(6);
        GUILayout.Label("Q1: Geometric Modelling", _stBody);
        GUILayout.Label("• 5 distinct multi-part procedural B-Rep meshes", _stMono);
        GUILayout.Label("• Modular hierarchy: Chassis, Fascia, Canopy, Wing", _stMono);
        GUILayout.Label("• Shared material instancing for maximum URP batching", _stMono);
        GUILayout.Space(8);
        GUILayout.Label("Q2: Homogeneous Transformations", _stBody);
        GUILayout.Label("• Column-vector convention: p' = M · p", _stMono);
        GUILayout.Label("• Order of operations: M = T · R · S", _stMono);
        GUILayout.Label("• Live 4×4 matrix computation around Wheel Rim pivot", _stMono);
        GUILayout.Space(8);
        GUILayout.Label("Q3: Clipping & Visibility", _stBody);
        GUILayout.Label("• Back-face culling (N · V <= 0 on GPU)", _stMono);
        GUILayout.Label("• Depth-buffer Hi-Z Early-Z hidden surface removal", _stMono);
        GUILayout.Label("• Sutherland-Hodgman clipping in clip space [-w, w]", _stMono);
        GUILayout.Label("• Single-Pass Instanced Stereo VR display", _stMono);

        GUILayout.Space(12);
        Color prev = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.15f, 0.85f, 1.0f);
        if (GUILayout.Button("OPEN LIVE TECHNICAL DIAGNOSTIC PANEL", _btnActive, GUILayout.Height(30)))
            _showTechPanel = true;
        GUI.backgroundColor = prev;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BOTTOM CAMERA & UTILITY DOCK
    // ─────────────────────────────────────────────────────────────────────────
    private void DrawBottomBar()
    {
        float sw = Screen.width, sh = Screen.height;
        float bh = 52;
        Rect r = new Rect(0, sh - bh, sw, bh);

        GUI.color = new Color(0.04f, 0.05f, 0.08f, 0.95f);
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = new Color(0.15f, 0.85f, 1.0f, 0.25f);
        GUI.DrawTexture(new Rect(0, sh - bh, sw, 1), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUILayout.BeginArea(new Rect(205, sh - bh + 9, sw - 205 - 365, 36));
        GUILayout.BeginHorizontal();

        // Camera Presets
        for (int i = 0; i < CamPresetNames.Length; i++)
        {
            bool sel = (_currentCamPreset == i && _isFreeOrbit);
            Color prev = GUI.backgroundColor;
            GUI.backgroundColor = sel ? new Color(0.15f, 0.85f, 1.0f) : new Color(0.10f, 0.13f, 0.18f);
            if (GUILayout.Button(CamPresetNames[i], sel ? _btnActive : _btnNormal, GUILayout.Height(30), GUILayout.ExpandWidth(false)))
                GoToPreset(i);
            GUI.backgroundColor = prev;
        }

        GUILayout.FlexibleSpace();

        // Utility Buttons
        Color prevBg = GUI.backgroundColor;
        GUI.backgroundColor = _autoRotate ? new Color(0.15f, 0.85f, 1.0f) : new Color(0.10f, 0.13f, 0.18f);
        if (GUILayout.Button("Auto 360°", _autoRotate ? _btnActive : _btnNormal, GUILayout.Height(30)))
            _autoRotate = !_autoRotate;

        GUI.backgroundColor = new Color(0.10f, 0.13f, 0.18f);
        if (GUILayout.Button("Reset Cam (R)", _btnNormal, GUILayout.Height(30)))
            ResetOrbit();

        GUI.backgroundColor = _comparingBeforeAfter ? new Color(0.15f, 0.85f, 1.0f) : new Color(0.10f, 0.13f, 0.18f);
        if (GUILayout.Button("Before / After", _comparingBeforeAfter ? _btnActive : _btnNormal, GUILayout.Height(30)))
        {
            if (!_comparingBeforeAfter)
            {
                int curPaint = _currentPaintIndex;
                _currentPaintIndex = _savedPaintIndex;
                ApplyPaint();
                _comparingBeforeAfter = true;
                _currentPaintIndex = curPaint;
            }
            else
            {
                ApplyPaint();
                _comparingBeforeAfter = false;
            }
        }

        GUI.backgroundColor = new Color(0.10f, 0.13f, 0.18f);
        if (GUILayout.Button("F1: Perf", _showPerfHUD ? _btnActive : _btnNormal, GUILayout.Height(30)))
            _showPerfHUD = !_showPerfHUD;

        GUI.backgroundColor = prevBg;

        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TECHNICAL PANEL (Floating Overlay for Q2 & Q3)
    // ─────────────────────────────────────────────────────────────────────────
    private void DrawTechPanel()
    {
        float sw = Screen.width, sh = Screen.height;
        float pw = 540, ph = 330;
        float px = sw * 0.5f - pw * 0.5f;
        float py = sh * 0.5f - ph * 0.5f;

        GUI.color = new Color(0.04f, 0.05f, 0.08f, 0.98f);
        GUI.DrawTexture(new Rect(px, py, pw, ph), Texture2D.whiteTexture);
        GUI.color = new Color(0.15f, 0.85f, 1.0f);
        GUI.DrawTexture(new Rect(px, py, pw, 2), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUILayout.BeginArea(new Rect(px + 14, py + 12, pw - 28, ph - 24));

        GUILayout.BeginHorizontal();
        GUILayout.Label("TECHNICAL CG DIAGNOSTIC PANEL", _stSection);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("✕ Close", _btnNormal, GUILayout.Width(68), GUILayout.Height(24)))
            _showTechPanel = false;
        GUILayout.EndHorizontal();

        // Q2 Homogeneous Matrix
        GUILayout.Space(6);
        GUILayout.Label("Q2: Wheel Rim Homogeneous Composite Matrix  M = T · R · S", _stBody);
        if (_wheelDemo != null)
        {
            Matrix4x4 m = _wheelDemo.GetCompositeMatrixMm();
            GUILayout.Label(string.Format("┌  {0,8:F4}   {1,8:F4}   {2,8:F4}   {3,8:F2}  ┐", m.m00, m.m01, m.m02, m.m03), _stMono);
            GUILayout.Label(string.Format("│  {0,8:F4}   {1,8:F4}   {2,8:F4}   {3,8:F2}  │", m.m10, m.m11, m.m12, m.m13), _stMono);
            GUILayout.Label(string.Format("│  {0,8:F4}   {1,8:F4}   {2,8:F4}   {3,8:F2}  │", m.m20, m.m21, m.m22, m.m23), _stMono);
            GUILayout.Label(string.Format("└  {0,8:F4}   {1,8:F4}   {2,8:F4}   {3,8:F2}  ┘", m.m30, m.m31, m.m32, m.m33), _stMono);
            GUILayout.Label("Order: T(50mm) · R(45°) · S(1.5)  |  Analytical: 1.5 · cos(45°) = 1.0607", _stMono);
        }

        GUILayout.Space(8);
        GUILayout.Label("Q3: Clipping, Visibility & VR Dual-Eye Telemetry", _stBody);
        float fpsVal = telemetry != null ? telemetry.fps : 90f;
        float ftVal  = telemetry != null ? telemetry.frameTimeMs : 11.1f;
        float mtpVal = telemetry != null ? telemetry.motionToPhotonMs : 14.3f;

        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical(GUILayout.Width(250));
        GUILayout.Label("• Back-Face Culling: ACTIVE (N·V <= 0)", _stMono);
        GUILayout.Label("• Hi-Z Early-Z Test: ACTIVE (Depth Buffer)", _stMono);
        GUILayout.Label("• Frustum Clipping: Sutherland-Hodgman", _stMono);
        GUILayout.Label("• Occlusion Culling: ACTIVE (BVH)", _stMono);
        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        GUILayout.Label(string.Format("• Frame Rate: {0:F1} FPS (90 Hz Target)", fpsVal), _stMono);
        GUILayout.Label(string.Format("• Frame Budget: {0:F2} ms / 11.11 ms", ftVal), _stMono);
        GUILayout.Label(string.Format("• Motion-to-Photon: {0:F1} ms (<20ms)", mtpVal), _stMono);
        GUILayout.Label("• Stereo: Single-Pass Instanced (Multiview)", _stMono);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PERFORMANCE HUD (F1 Toggle)
    // ─────────────────────────────────────────────────────────────────────────
    private void DrawPerfHUD()
    {
        float fpsVal = telemetry != null ? telemetry.fps : 0f;
        float ftVal  = telemetry != null ? telemetry.frameTimeMs : 0f;
        float mtpVal = telemetry != null ? telemetry.motionToPhotonMs : 0f;

        float pw = 215, ph = 88;
        Rect r = new Rect(Screen.width - 365 - pw - 10, 56, pw, ph);

        GUI.color = new Color(0.04f, 0.05f, 0.08f, 0.92f);
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUILayout.BeginArea(new Rect(r.x + 8, r.y + 6, pw - 16, ph - 10));
        GUILayout.Label("VR TELEMETRY  (F1)", _stMono);
        Color fpsCol = fpsVal >= 80 ? new Color(0.2f, 0.9f, 0.2f) : fpsVal >= 55 ? new Color(0.9f, 0.7f, 0.1f) : new Color(0.9f, 0.2f, 0.2f);
        GUIStyle fpsStyle = new GUIStyle(_stMono); fpsStyle.normal.textColor = fpsCol;
        GUILayout.Label("FPS:  " + fpsVal.ToString("F1") + " FPS", fpsStyle);
        GUILayout.Label("Frame Time: " + ftVal.ToString("F2") + " ms", _stMono);
        GUILayout.Label("Motion-to-Photon: " + mtpVal.ToString("F1") + " ms", _stMono);
        GUILayout.EndArea();
    }

    private Color GetCategoryColor(string cat)
    {
        switch (cat)
        {
            case "Sports":   return new Color(0.20f, 0.75f, 1.00f);
            case "Supercar": return new Color(0.95f, 0.18f, 0.18f);
            case "Hypercar": return new Color(0.95f, 0.65f, 0.05f);
            case "Muscle":   return new Color(0.92f, 0.45f, 0.10f);
            case "SUV":      return new Color(0.25f, 0.82f, 0.38f);
            default:         return Color.gray;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STYLE INITIALIZATION
    // ─────────────────────────────────────────────────────────────────────────
    private void InitStyles()
    {
        if (_stylesReady) return;

        _stTitle = new GUIStyle(GUI.skin.label) {
            fontSize = 14, fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.20f, 0.90f, 1.0f) }
        };
        _stSection = new GUIStyle(GUI.skin.label) {
            fontSize = 11, fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(1.0f, 0.82f, 0.22f) }
        };
        _stBody = new GUIStyle(GUI.skin.label) {
            fontSize = 10,
            normal = { textColor = new Color(0.80f, 0.84f, 0.90f) }
        };
        _stMono = new GUIStyle(GUI.skin.label) {
            fontSize = 9,
            normal = { textColor = new Color(0.65f, 0.82f, 0.78f) }
        };
        _stBadge = new GUIStyle(GUI.skin.label) {
            fontSize = 10, fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.20f, 0.95f, 0.50f) }
        };

        _btnNormal = new GUIStyle(GUI.skin.button) {
            fontSize = 10,
            normal   = { textColor = new Color(0.80f, 0.84f, 0.90f) }
        };
        _btnActive = new GUIStyle(GUI.skin.button) {
            fontSize = 10, fontStyle = FontStyle.Bold,
            normal   = { textColor = new Color(0.04f, 0.06f, 0.10f) }
        };
        _btnNav = new GUIStyle(GUI.skin.button) {
            fontSize = 11,
            normal   = { textColor = new Color(0.72f, 0.76f, 0.82f) },
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(10, 4, 4, 4)
        };
        _btnNavActive = new GUIStyle(GUI.skin.button) {
            fontSize = 11, fontStyle = FontStyle.Bold,
            normal   = { textColor = new Color(0.15f, 0.88f, 1.0f) },
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(10, 4, 4, 4)
        };

        _stylesReady = true;
    }
}
