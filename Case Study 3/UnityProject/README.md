# Virtual Interior Design Studio ? Unity 6 URP Project
## CS4104 Case Study 3: Retail and Remote Commerce
**Course:** CS4104 â€“ Computer Graphics and Virtual Reality  
**Student Name:** Likith S G | **USN:** `1RVU23CSE236` | **Institution:** RV University, Bengaluru  

---

## 1. Project Overview

This folder contains the complete, production-grade **Unity 6 Universal Render Pipeline (URP 17.6)** project implementing the **Virtual Interior Design Studio**.

### Verified Unity Environment
- **Unity Editor Version:** `6000.6.7f1` (Unity 6 LTS)
- **Render Pipeline:** Universal Render Pipeline (URP 17.6.0)
- **Input System:** Unity Input System (1.20.1) & Legacy Input fallback
- **UI Framework:** Universal GUI (2.6.0) & OnGUI Responsive HUD
- **Target Platform:** Windows x86_64 / WebXR compatible

---

## 2. Project Asset Directory Structure

```
Assets/
├── Scenes/
│   └── InteriorStudio.unity           <- Master living room diorama scene
├── Scripts/
│   ├── Core/
│   │   ├── StudioManager.cs           <- Central orchestrator & event bus
│   │   ├── FurnitureData.cs           <- Metadata data model for catalogue items
│   │   ├── MaterialPreset.cs          <- PBR material preset configuration
│   │   └── RoomSaveData.cs            <- JSON serialization data contract
│   ├── Environment/
│   │   ├── RoomEnvironmentBuilder.cs  <- Procedural 8x6x3.2m Scandinavian architecture diorama
│   │   ├── LightingPresetManager.cs   <- Sun, Sky ambient, floor lamp point light, pot lights
│   │   └── IlluminationVisualizer.cs  <- Question 1 component isolation (Diffuse, Specular, Ambient)
│   ├── Interaction/
│   │   ├── ScreenToWorldRaycaster.cs  <- Question 3 Window-to-Viewport & ray unprojection
│   │   ├── FurnitureSelector.cs       <- Raycast selection & cyan bounding halo gizmo
│   │   ├── FurnitureManipulator.cs    <- Continuous planar move, axial rot Q/E, scale
│   │   ├── PlacementController.cs     <- Catalogue spawn & preview ghost placement
│   │   └── CameraNavigation.cs        <- WASD walk, right-click orbit, scroll zoom, F focus
│   ├── UI/
│   │   └── StudioUIManager.cs         <- Top bar telemetry, catalogue accordion, inspector
│   └── Editor/
│       ├── StudioAssetGenerator.cs    <- Automatic procedural asset & scene generation
│       └── CaptureInteriorStudioOutputs.cs <- Batchmode high-resolution capture harness
├── Prefabs/                           <- 6 configured furniture prefabs with colliders
├── Materials/                         <- 15 PBR Lit materials (Velvet, Leather, Oak, Marble, etc.)
├── Textures/                          <- 17 procedural 2048x2048 PBR albedo and normal maps
└── Settings/                          <- URP pipeline assets and renderers
```

---

## 3. How to Open and Run

1. Open **Unity Hub** and click **Add project from disk**.
2. Select this directory: `Case Study 3/UnityProject/`.
3. Open with Unity `6000.6.7f1`.
4. Open `Assets/Scenes/InteriorStudio.unity`.
5. Press **Play** to enter interactive evaluation mode.
