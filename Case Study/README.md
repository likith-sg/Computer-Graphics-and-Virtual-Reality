# AutoVerse — 3D Interactive Automotive Showroom & Customizer

[![Live WebGL Demo](https://img.shields.io/badge/Live_Demo-auto--verse--plum.vercel.app-00E5FF?style=for-the-badge&logo=vercel)](https://auto-verse-plum.vercel.app/)
[![Unity 6 Version](https://img.shields.io/badge/Unity_6-6000.6.7f1-000000?style=for-the-badge&logo=unity)](https://unity.com/)
[![Render Pipeline](https://img.shields.io/badge/Pipeline-Universal_Render_Pipeline_(URP)-4B5563?style=for-the-badge)](https://unity.com/srp/universal-render-pipeline)
[![Target Platform](https://img.shields.io/badge/Platform-WebGL_2.0_|_Windows_Standalone-059669?style=for-the-badge)](https://auto-verse-plum.vercel.app/)

> **Course**: CS4104 – Computer Graphics and Virtual Reality (CGVR)  
> **Student Name**: Likith S G  
> **USN**: 1RVU23CSE236  
> **Case Study 2**: Interactive 3D Real-Time Vehicle Showroom & Collaborative Customizer  
> **Live Production URL**: [https://auto-verse-plum.vercel.app/](https://auto-verse-plum.vercel.app/)  
> **Reference Architecture**: [bestorbust/cgvr_labs (AutoVerse)](https://github.com/bestorbust/cgvr_labs)

---

## 1. Executive Summary

**AutoVerse** is a production-grade, interactive 3D automotive design studio and vehicle customization system developed in **Unity 6.6 (`6000.6.7f1`)** using the **Universal Render Pipeline (URP)** and C#, with an accompanying standalone Python 3D OpenGL application.

Inspired by enterprise automotive design centers (Porsche Exclusive Manufaktur, Mercedes-AMG Studio, Tesla Design Studio), the application provides:
- **8 Interactive Vehicle Models** with instantaneous chassis switching and custom sub-mesh slot mappings.
- **Deep Multi-Layer Customization**: 18 automotive paint colors across 4 distinct surface finishes (Gloss, Metallic, Matte, Satin), 5 alloy finishes, 5 Brembo brake caliper colors, 6 window tint styles, 5 LED headlight styles, and 6 underglow neon colors with real-time ground bounce lighting.
- **Procedural Bounds-Fitted Accessories**: Roof light bars, aerodynamic rear downforce spoilers, dual racing stripes, and underglow neon tubes dynamically fitted to vehicle geometry bounds.
- **360° Architectural Luxury Showroom**: Polished obsidian reflective floor, illuminated turntable with dual ambient LED halo rings, acoustic feature wall with backlit 3D **AUTOVERSE** branding emblem, vertical architectural light columns, showcase window bay, and suspended overhead studio softbox luminaire.
- **4 Real-time Studio Lighting Presets**: Luxury Studio (warm champagne & tungsten), Midnight Studio (high-tech ice blue & cyan rim), Bright Gallery (pure daylight white high-key pavilion), and Neon Garage (cyberpunk magenta & electric cyan).
- **Responsive & Touch-Optimized UI**: Dynamic CanvasScaler with safe-area handling across Desktop, Tablet, Mobile Portrait, and Mobile Landscape with 100% camera orbit isolation.

---

## 2. Visual Demonstration

### 1. Interactive 3D Showroom & Turntable Demo (Animated GIF)
Demonstrates the complete interactive studio: real-time turntable auto-rotation, dynamic lighting response across studio presets, finish transitions (Metallic &rarr; Gloss &rarr; Satin), and alloy wheel reflections.

![AutoVerse Interactive Showroom Demo](demo.gif)

### 2. High-Resolution Hero Studio Capture (Static View)
High-resolution 1080p hero capture showcasing the Sports Coupe in Racing Red metallic finish on the illuminated halo turntable with polished obsidian reflections and architectural light columns.

![AutoVerse Studio Hero Shot](output.png)

---

## 3. Vehicle Topology & Compatibility Matrix

Every vehicle features unique sub-mesh slots, materials, and specialized customization handling:

| Index | Vehicle Identifier | Display Title | Body Paint Slot | Glass Tint Slot | Wheel Rim Slot | Headlights Slot | Specialized Custom Components |
| :---: | :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| `0` | `car_sports` | **SPORTS COUPE** | Slot 0 (`orange`) | Slot 1 (`glass`) | Slot 3 (`hub`) | Slot 4 (`headlight`) | Slot 2 (`bumper`), Slot 5 (`interior`), Rear Spoiler |
| `1` | `car_sedan` | **EXECUTIVE SEDAN** | Slot 0 (`carblue`) | Slot 1 (`glass`) | Slot 3 (`hub`) | Slot 4 (`headlight`) | Slot 2 (`bumper`), Slot 5 (`interior`) |
| `2` | `car_hatchback` | **COMPACT HATCH** | Slot 0 (`carred`) | Slot 1 (`glass`) | Slot 3 (`hub`) | Slot 4 (`headlight`) | Slot 2 (`bumper`), Slot 5 (`interior`) |
| `3` | `car_suv` | **URBAN SUV** | Slot 0 (`cargreen`) | Slot 1 (`glass`) | Slot 3 (`hub`) | Slot 4 (`headlight`) | Slot 2 (`bumper`), Slot 5 (`interior`), Roof Rails |
| `4` | `race_car` | **GT FORMULA RACER** | Slot 0 (`carred`) | *N/A (Open)* | Slot 5 (`hub`) | *N/A (Track)* | Slots 1 & 2 (`aero`), Slot 3 (`monocoque`) |
| `5` | `car_police` | **POLICE INTERCEPTOR** | Slot 0 (`white`) | Slot 1 (`glass`) | Slot 4 (`hub`) | Slot 5 (`headlight`) | Slot 2 (`navy`), Slot 8 (`emergency light bar`) |
| `6` | `car_taxi` | **CITY TAXI** | Slot 0 (`taxi`) | Slot 1 (`glass`) | Slot 3 (`hub`) | Slot 4 (`headlight`) | Slot 2 (`interior`), Slot 7 (`roof sign`) |
| `7` | `golf_cart` | **LUXURY GOLF CART** | Slot 3 (`lime`) | Slot 4 (`glass`) | Slot 8 (`hub`) | Slot 9 (`headlight`) | Slot 0 (`canopy`), Slot 1 (`seats`), Slot 2 (`chassis`) |

---

## 4. Customization Suite

### A. Body Paint & Surface Finishes
* **18 Automotive Colors**:
  1. Pearl White (`#F5F5FA`) &bull; 2. Midnight Black (`#141417`) &bull; 3. Graphite (`#40454D`) &bull; 4. Arctic Silver (`#D1D6DC`)  
  5. Racing Red (`#D91F1F`) &bull; 6. Crimson (`#8C0017`) &bull; 7. Sunset Orange (`#FF5C1F`) &bull; 8. Solar Yellow (`#FFCC00`)  
  9. Royal Blue (`#144DCC`) &bull; 10. Sky Blue (`#38B8FF`) &bull; 11. Emerald Green (`#00A666`) &bull; 12. Forest Green (`#1F522E`)  
  13. Violet (`#7314B8`) &bull; 14. Rose Gold (`#BF7380`) &bull; 15. Champagne (`#F7E8CF`) &bull; 16. Bronze (`#946133`)  
  17. Teal (`#008C8C`) &bull; 18. Candy Pink (`#F22673`)
* **4 Surface Finishes**:
  - **Gloss**: High smoothness ($0.90$), low metallic ($0.08$) with brilliant clearcoat reflections.
  - **Metallic**: High metallic ($0.90$), balanced micro-roughness ($0.85$) with deep flake luster.
  - **Matte**: Zero metallic ($0.00$), low smoothness ($0.15$) for stealth diffused absorption.
  - **Satin**: Mid metallic ($0.45$), velvety semi-specular sheen ($0.50$).

### B. Wheels & Calipers
* **5 Alloy Wheel Finishes**: Stealth Black, Polished Silver, Gunmetal, Liquid Gold, Burnt Bronze.
* **5 Brake Caliper Colors**: Brembo Racing Red, Acid Yellow, Electric Blue, Acid Lime Green, Stealth Gloss Black.

### C. Glass Tinting
* **6 Tint Styles**: Clear Glass ($35\%$), Light Smoke ($55\%$), Dark Charcoal ($82\%$), Limo Blackout ($96\%$), Sunset Gold Chameleon, Cyber Blue Mirror.

### D. Illumination & Neon Underglow
* **5 Headlight Styles**: Xenon White ($6500\text{K}$), Warm Halogen ($3200\text{K}$), Ice Blue LED ($8500\text{K}$), Cyber Violet LED, Amber Gold DRL.
* **6 Underglow Neon Colors**: Electric Cyan, Neon Magenta, Acid Green, Amber Gold, Ice White, Royal Violet with downward ground-illuminating point lights.

### E. Procedural Accessories
* **Roof Light Bar**: Dual high-intensity auxiliary off-road driving beam housing.
* **Rear Wing / Spoiler**: Aerodynamic downforce spoiler automatically scaled and positioned on vehicle rear deck lid.
* **Dual Racing Stripes**: Twin center racing livery stripes dynamically projected along hood, roof, and trunk.
* **Underglow Tubes**: Quad perimeter neon chassis tubes with real-time ground reflection.

---

## 5. Architectural Showroom & Lighting Presets

1. **Luxury Studio (Warm Champagne & Tungsten)**:
   - Primary Key: Warm tungsten sunlight ($45^\circ$, $1.8\times$ intensity).
   - Fill: Cool blue skylight fill to reveal body curve contours.
   - Best for: Champagne, Bronze, Pearl White, and luxury sedans.
2. **Midnight Studio (High-Tech Ice Blue & Cyan Rim)**:
   - Primary Key: Cool cyan directional key ($6000\text{K}$, high contrast).
   - Fill: Deep indigo fill with sharp specular edge highlighting.
   - Best for: Graphite, Midnight Black, Electric Cyan neon, and sports coupes.
3. **Bright Gallery (Pure Daylight White High-Key)**:
   - Primary Key: Pure white daylight ($6500\text{K}$, balanced omnidirectional lighting).
   - Minimal shadow falloff mimicking an art gallery pavilion.
   - Best for: Inspecting true paint pigment fidelity and formula race cars.
4. **Neon Garage (Cyberpunk Magenta & Electric Cyan)**:
   - High-contrast cross-lighting: Magenta key light paired with electric cyan fill.
   - Emphasizes underglow neon reflection on the obsidian floor.
   - Best for: Tuning culture, night drives, and street interceptors.

---

## 6. Automotive Case Study Technical Formulations

### 1. Matrix Transformations in Synchronized Collaborative CAD
In multi-location automotive design reviews (e.g., Bengaluru to Detroit sync), spatial transformations must be evaluated deterministically using standard $4 \times 4$ homogeneous matrix transformations:

$$\mathbf{M} = \mathbf{T}(t_x, t_y, t_z) \cdot \mathbf{R}_z(\theta) \cdot \mathbf{S}(s_x, s_y, s_z)$$

$$\mathbf{M} = \begin{bmatrix}
s_x \cos\theta & -s_y \sin\theta & 0 & t_x \\
s_x \sin\theta &  s_y \cos\theta & 0 & t_y \\
0              &  0              & s_z & t_z \\
0              &  0              & 0 & 1
\end{bmatrix}$$

- **Scale ($\mathbf{S}$)**: Uniform scaling ($s_x = s_y = s_z = 1.5$) expands components for ergonomic interior analysis.
- **Rotation ($\mathbf{R}$)**: Rotation around the chassis longitudinal/vertical axis aligns assembly components.
- **Translation ($\mathbf{T}$)**: Translates body panels outwards along their local surface normal vectors for an **Exploded Assembly View**.

### 2. Class-A Surface Paint Shading & B-Rep Reflections
Automotive Class-A surfaces require continuous $G^2$ (curvature) continuity across adjoining Bézier and B-Spline surface patches. To visualize highlight flow without visual faceting or stepping:
- **Cook-Torrance Microfacet Specular BRDF**:
  $$f_r(\mathbf{l}, \mathbf{v}) = \frac{D(\mathbf{h}) \cdot F(\mathbf{v}, \mathbf{h}) \cdot G(\mathbf{l}, \mathbf{v}, \mathbf{h})}{4 (\mathbf{n} \cdot \mathbf{l}) (\mathbf{n} \cdot \mathbf{v})}$$
- **GGX Normal Distribution Function ($D$)**: Produces the long specular tail characteristic of clearcoat automotive enamel.
- **Schlick-Fresnel Approximation ($F$)**:
  $$F(\theta) = F_0 + (1 - F_0)(1 - \cos\theta)^5$$
  Yields realistic clearcoat grazing reflections ($F \to 1.0$) along hood and fender contours while maintaining rich pigment saturation at normal incidence.

### 3. VR Stereoscopic Viewport & Real-Time Performance Budget
To maintain immersion and eliminate motion sickness in VR headsets:
- **Strict Frame Budget**: $T_{\text{frame}} \le 11.11\,\text{ms}$ (evaluating at $90\,\text{Hz}$ or $120\,\text{Hz}$).
- **Single-Pass Instanced (SPI) Rendering**: Draws both eyes in a single draw call with stereo viewport arrays, halving CPU driver overhead.
- **Window-to-Viewport Transformation**: Converts touch/cursor coordinates $(X_s, Y_s)$ to Normalized Device Coordinates (NDC) $[-1, 1]^2$ and unprojects via $(\mathbf{P} \cdot \mathbf{V})^{-1}$ to raycast against the turntable plane.

---

## 7. Controls & User Interaction

| Function | Keyboard Shortcut | Mouse / Touch Gesture |
| :--- | :---: | :---: |
| **Orbit Camera** | Left / Right Arrow | Left-Click & Drag (10px deadzone) / 1-Finger Drag |
| **Zoom View** | Up / Down Arrow | Mouse Scroll Wheel / 2-Finger Pinch In-Out |
| **Select Vehicle** | Keys `0` through `7` | `<` and `>` On-Screen Arrow Buttons |
| **Cycle Colors** | `C` | Click/Tap Circular Color Swatches |
| **Cycle Finishes** | `F` (`G`/`M`/`N`/`S`) | Click/Tap Finish Tab (Gloss/Metallic/Matte/Satin) |
| **Wheel Finishes** | `W` | Click/Tap Alloy Preset |
| **Window Tint** | `T` | Click/Tap Window Tint Level |
| **Toggle Underglow**| `U` | Click/Tap Underglow Toggle |
| **Cycle Underglow** | `N` | Click/Tap Neon Color Swatch |
| **Studio Lighting** | `1` &ndash; `4` | Click/Tap Top Bar "Studio" Pill |
| **Turntable Auto-Spin**| `Space` / `A` | Click/Tap "Spin: ON/OFF" Button |
| **Reset Vehicle** | `R` / `X` | Click/Tap "Reset" Button |
| **Toggle Help** | `H` / `?` / `Tab` | Click/Tap "Keys" Button |

---

## 8. How to Open and Run

### Option 1: Standalone Python 3D Viewer (Immediate Execution)
Run the complete interactive 3D AutoVerse showroom directly via Python:
```bash
python "Case Study/autoverse.py"
```

To re-generate the 1080p hero render and 30-frame animated GIF headlessly:
```bash
python "Case Study/autoverse.py" --headless --outdir "Case Study"
```

### Option 2: Unity 6 Editor
1. Open **Unity Hub**.
2. Click **Add > Add project from disk**.
3. Select the folder: `Case Study/AutoVerse` (or `Case Study/unity_project`).
4. Open the project with **Unity 6 (`6000.6.7f1`)** or newer.
5. In the Project window, double-click `Assets/Scenes/AutoVerse_Showwroom.unity`.
6. Press the **Play** button (▶) in the top toolbar to interact with the full showroom.

### Option 3: Automated Verification Test Suite
In the Unity Editor menu bar:
* Click: **`AutoVerse` > `Testing` > `Run Full Test Suite`**
* Validates all 8 vehicles, 18 body colors, 4 finishes, wheel alloys, window tints, accessories, and studio lighting styles automatically.

### Option 4: Live WebGL Deployment
Open the production deployment hosted on Vercel:  
👉 **[https://auto-verse-plum.vercel.app/](https://auto-verse-plum.vercel.app/)**
