# Builds & Standalone Distribution Guide
## CS4104 Case Study 3: Retail and Remote Commerce
**Virtual Interior Design Studio: An Interactive 3D Furniture Placement and Customisation WebXR Application**  
**Student Name:** Likith S G | **USN:** `1RVU23CSE236` | **Institution:** RV University, Bengaluru  

---

## 1. Standalone Executable Build Options

The project supports two verified execution pipelines:

### Pipeline A: Unity 6 URP Standalone Windows Build
To build a standalone Windows 64-bit `.exe` binary:
1. Open the project in Unity Editor: `Case Study 3/UnityProject/`.
2. Go to **File** -> **Build Profiles** (or **Build Settings**).
3. Ensure `Assets/Scenes/InteriorStudio.unity` is included and checked.
4. Set **Target Platform:** `Windows, Mac, Linux` -> `Windows (x86_64)`.
5. Set **Architecture:** `Intel 64-bit`.
6. Click **Build**, and select an output folder (e.g., `Builds/StandaloneWindows64/`).
7. Unity compiles the executable with full Universal Render Pipeline lighting, soft shadows, and audio.

*Note on Git Tracking:* Standalone binary builds (`.exe`, `.data`, `.pck`) typically exceed 100 MB and are excluded by the repository `.gitignore` rules (`**/[Bb]uild/`, `**/[Bb]uilds/`) to keep the Git repository lightweight.

---

### Pipeline B: Standalone Interactive Python/OpenGL Viewer
For instant cross-platform review without opening Unity:
```bash
python "Case Study 3/interior_studio.py"
```
Features:
- Instant 60 FPS hardware-accelerated 3D rendering via PyOpenGL and GLFW.
- Full camera navigation, lighting presets (Morning Sun, Cozy Evening, Studio Daylight), PBR material switching, normal mapping comparison, and HUD telemetry.
- Runs without any external editor licenses or cloud dependencies.

---

### Pipeline C: WebXR / WebGL Deployment Pathway
To compile for web browsers supporting the WebXR Device API:
1. In Unity Editor, switch active platform to **WebGL**.
2. Enable WebXR export template (`com.unity.xr.webxr` or DeWebXR).
3. Set WebGL Memory Size to 512 MB and Compression Format to Gzip / Brotli.
4. Build to `Builds/WebGL/` and host over HTTPS (WebXR requires secure origin `https://` for camera and spatial tracking permissions).
