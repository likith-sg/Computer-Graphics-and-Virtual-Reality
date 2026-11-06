# Installation and Setup Guide
## CS4104 Case Study 3: Retail and Remote Commerce
**Virtual Interior Design Studio: An Interactive 3D Furniture Placement and Customisation WebXR Application**  
**Student Name:** Likith S G | **USN:** `1RVU23CSE236` | **Institution:** RV University, Bengaluru  

---

## 1. System Requirements

### Recommended Hardware
- **Operating System:** Windows 10 / Windows 11 (64-bit)
- **Processor:** Intel Core i5 / AMD Ryzen 5 or higher
- **Memory (RAM):** 8 GB minimum (16 GB recommended)
- **Graphics Card (GPU):** NVIDIA GeForce GTX 1060 / RTX 2060 or AMD Radeon RX 580 or higher (DirectX 12 / Vulkan / OpenGL 4.5 compatible)
- **Storage:** ~2 GB free disk space

### Software Prerequisites
1. **Unity 6 LTS (Version `6000.6.7f1`)**
   - Installed via Unity Hub with the **Universal Windows Platform Build Support** or **Windows Build Support (IL2CPP / Mono)**.
   - Package manager access to Universal Render Pipeline (URP 17.6.0), Input System (1.20.1), and uGUI (2.6.0).
2. **Python 3.10+ (Optional for Standalone Viewer & Tools)**
   - Required libraries: `PyOpenGL`, `glfw`, `numpy`, `pillow`, `reportlab`, `python-docx`, `matplotlib`.
   - Install command:
     ```bash
     pip install PyOpenGL glfw numpy pillow reportlab python-docx matplotlib
     ```

---

## 2. Opening the Unity 6 URP Project

1. Launch **Unity Hub**.
2. Click **Add** -> **Add project from disk**.
3. Navigate to your local repository directory and select:
   ```
   <Repository-Root>/Case Study 3/UnityProject/
   ```
4. Confirm that the Editor Version is set to **6000.6.7f1**.
5. Click the project to open it in Unity Editor.
6. Once the Editor completes package resolution and asset indexing, navigate to the Project window:
   ```
   Assets / Scenes / InteriorStudio.unity
   ```
7. Double-click **`InteriorStudio.unity`** to load the master living room scene.
8. Press the **Play** button at the top of the Unity Editor to enter interactive mode.

---

## 3. Running the Standalone Interactive 3D Viewer (Python / OpenGL)

For immediate interactive evaluation without opening the Unity Editor:
1. Open PowerShell or Command Prompt in the repository root.
2. Run:
   ```bash
   python "Case Study 3/interior_studio.py"
   ```
3. A hardware-accelerated 3D window (1280x720) will initialize, rendering the complete Scandinavian living room with real-time lighting presets, PBR materials, normal mapping, and coordinate transformation telemetry.

---

## 4. Re-Generating Academic Renders, Figures, and Reports

All procedural textures, in-engine frame captures, and university reports can be regenerated from source via the bundled scripts in `Case Study 3/Tools/`:

1. **Regenerate PBR Textures (2048x2048):**
   ```bash
   python "Case Study 3/Tools/generate_textures.py"
   ```
2. **Regenerate Technical Figures (Figures 10â€“17):**
   ```bash
   python "Case Study 3/Tools/generate_academic_diagrams.py"
   ```
3. **Capture All In-Engine 3D Scenarios & Turntable GIF:**
   ```bash
   python "Case Study 3/interior_studio.py"
   ```
4. **Compile Official 16-Page DOCX & PDF University Reports:**
   ```bash
   python "Case Study 3/Tools/generate_reports.py"
   ```
