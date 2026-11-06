# Developer Architecture and Technical Guide
## CS4104 Case Study 3: Retail and Remote Commerce
**Virtual Interior Design Studio: An Interactive 3D Furniture Placement and Customisation WebXR Application**  
**Student Name:** Likith S G | **USN:** `1RVU23CSE236` | **Institution:** RV University, Bengaluru  

---

## 1. Project Organization & Codebase Architecture

The project is structured under `Case Study 3/` with complete independence from other repository files:

```
Case Study 3/
├── README.md                          <- Main project overview & academic summary
├── demo.gif                           <- 30-frame animated turntable demonstration GIF
├── output.png                         <- Authentic hero static render
├── screenshot.png                     <- High-res capture for GitHub display
├── interior_studio.py                 <- Standalone interactive Python/OpenGL 3D engine
├── room_layout.json                   <- Sample serialized room layout configuration
│
├── UnityProject/                      <- Independent Unity 6 LTS URP Project
│   ├── Assets/
│   │   ├── Scenes/InteriorStudio.unity
│   │   ├── Scripts/
│   │   │   ├── Core/                  <- StudioManager, FurnitureData, MaterialPreset, RoomSaveData
│   │   │   ├── Environment/           <- RoomEnvironmentBuilder, LightingPresetManager, IlluminationVisualizer
│   │   │   ├── Interaction/           <- ScreenToWorldRaycaster, FurnitureSelector, FurnitureManipulator, PlacementController, CameraNavigation
│   │   │   ├── UI/                    <- StudioUIManager (OnGUI responsive layer)
│   │   │   └── Editor/                <- StudioAssetGenerator, CaptureInteriorStudioOutputs
│   │   ├── Prefabs/                   <- 6 configured furniture prefabs with colliders
│   │   ├── Materials/                 <- 15 configured URP PBR materials
│   │   ├── Textures/                  <- 17 procedural 2048x2048 PBR maps
│   │   └── Settings/                  <- URP Pipeline assets (PC_RPAsset, Mobile_RPAsset)
│   ├── Packages/manifest.json         <- URP 17.6, Input System 1.20, uGUI 2.6
│   └── ProjectSettings/               <- ProjectVersion.txt (6000.6.7f1), GraphicsSettings
│
├── Report/
│   ├── CS4104_Case_Study_3.docx       <- Official 16-page university submission report (DOCX)
│   ├── CS4104_Case_Study_3.pdf        <- Official 16-page university submission report (PDF)
│   ├── Figures/                       <- 17 numbered figures and diagrams
│   └── rvu_logo.png                   <- RV University crest
│
├── Screenshots/                       <- 9 numbered in-engine evaluation screenshots
│
├── Documentation/
│   ├── INSTALLATION.md                <- Setup, dependencies, and launch guide
│   ├── USER_GUIDE.md                  <- Navigation, controls, and features
│   ├── TESTING.md                     <- QA matrix, benchmarks, and normal proof
│   ├── ASSET_CREDITS.md               <- Model and texture provenance
│   └── DEVELOPMENT.md                 <- Architecture, math pipelines, and WebXR bridge
│
├── Tools/                             <- Asset & report generation toolchain
│   ├── generate_textures.py           <- Sobel gradient normal map generator
│   ├── generate_academic_diagrams.py  <- Figures 10-17 generator (matplotlib)
│   └── generate_reports.py            <- DOCX & PDF report compiler
│
└── Builds/
    └── README.md                      <- Build targets and standalone instructions
```

---

## 2. Core Mathematical Pipelines

### A. Window-to-Viewport Coordinate Mapping (Question 3)
```csharp
public Vector2 ComputeWindowToViewport(Vector2 screenCoord, Rect winRect, Rect vpRect)
{
    float xw = screenCoord.x;
    float yw = screenCoord.y;
    float xv = vpRect.xMin + ((xw - winRect.xMin) / (winRect.xMax - winRect.xMin)) * (vpRect.xMax - vpRect.xMin);
    float yv = vpRect.yMin + ((yw - winRect.yMin) / (winRect.yMax - winRect.yMin)) * (vpRect.yMax - vpRect.yMin);
    return new Vector2(xv, yv);
}
```

### B. Normalized Device Coordinates (NDC) to Camera Ray Unprojection
```csharp
public Ray GenerateWorldRayFromScreen(Vector2 screenPos)
{
    // 1. Screen to Viewport mapping
    Vector2 vp = ComputeWindowToViewport(screenPos, new Rect(0,0,Screen.width,Screen.height), new Rect(0,0,1,1));
    // 2. Viewport to NDC in [-1, 1]^3
    Vector3 ndc = new Vector3(vp.x * 2.0f - 1.0f, vp.y * 2.0f - 1.0f, -1.0f);
    // 3. Unproject via Camera Inverse View-Projection (P * V)^(-1)
    Ray ray = targetCamera.ScreenPointToRay(screenPos);
    return ray;
}
```

### C. Floor Plane ($Y = 0$) Intersection
```csharp
public bool RaycastFloorPlane(Vector2 screenPos, out Vector3 worldHitPoint)
{
    Ray ray = GenerateWorldRayFromScreen(screenPos);
    // Mathematical ray-plane intersection for horizontal plane Y = 0
    // Ray: P(t) = O + t*D.  Floor: n*(P - P0) = 0 where n = (0,1,0), P0 = (0,0,0)
    // t = -O.y / D.y
    if (Mathf.Abs(ray.direction.y) > 0.0001f)
    {
        float t = -ray.origin.y / ray.direction.y;
        if (t > 0f)
        {
            worldHitPoint = ray.origin + ray.direction * t;
            return true;
        }
    }
    worldHitPoint = Vector3.zero;
    return false;
}
```

---

## 3. WebXR Architecture & Integration Pathway

While the assignment scenario describes a WebXR retail application, building and testing pure WebXR in native desktop terminals without connected headset hardware requires a **hybrid simulation bridge**:
1. **Input Abstraction Layer:** The interaction architecture maps standard mouse/touch coordinates through the exact same raycasting pipeline that the WebXR Device API (`XRInputSource.targetRaySpace`) delivers.
2. **WebXR Deployment Roadmap:**
   - Package: Install `com.unity.xr.webxr` or the official Unity WebXR Export package.
   - Graphics API: Switch to WebGL2 in Player Settings.
   - WebXR Session: Call `navigator.xr.requestSession('immersive-ar')` with `hit-test` feature flag enabled.
   - Spatial Hit-Testing: Replace our mathematical floor plane test with `XRHitTestSource` against real-world horizontal surfaces detected by ARCore / ARKit.
