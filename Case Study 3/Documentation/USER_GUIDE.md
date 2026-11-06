# User and Interaction Guide
## CS4104 Case Study 3: Retail and Remote Commerce
**Virtual Interior Design Studio: An Interactive 3D Furniture Placement and Customisation WebXR Application**  
**Student Name:** Likith S G | **USN:** `1RVU23CSE236` | **Institution:** RV University, Bengaluru  

---

## 1. Application Overview

The **Virtual Interior Design Studio** simulates a contemporary Scandinavian interior living room designed for remote spatial e-commerce. Users can freely walk around the 3D room, inspect furniture items, change materials and upholstery finishes, place new furniture from an interactive catalogue, adjust lighting conditions to simulate different times of day, and persist custom room layouts to JSON.

---

## 2. Interactive Navigation Controls

| Input Action | Keyboard / Mouse Binding | Description |
|:---|:---|:---|
| **Walk Forward / Back** | `W` / `S` or `Up` / `Down` Arrow | Translates camera along forward planar vector. |
| **Strafe Left / Right** | `A` / `D` or `Left` / `Right` Arrow | Translates camera sideways along right vector. |
| **Orbit / Look Around** | `Right-Click` + Mouse Drag | Controls camera pitch and yaw. |
| **Zoom / Dolly** | `Mouse Scroll Wheel` | Adjusts field of view (FOV) or camera distance. |
| **Focus on Selected** | `F` Key | Smoothly re-centers camera onto active furniture item. |
| **Toggle UI Overlay** | `Tab` or `Space` | Shows or hides the HUD and sidebar panels. |

---

## 3. Furniture Selection & Manipulation (Question 3)

### Selecting Furniture
- **Left-Click** any furniture item in the 3D living room.
- The active item is highlighted with a **luminous cyan bounding halo ring** around its base.
- The right-hand **Inspector Panel** displays real-time transform metrics: world position $(x, z)$, rotation $Y$, uniform scale, and polygon count.

### Moving Furniture Across the Floor Plane
- Select a furniture item.
- Hold **`Shift` + `Right-Click`** and drag across the floor.
- The system unprojects your screen-space cursor into a 3D camera ray $\mathbf{P}(t) = \mathbf{O} + t\mathbf{D}$, intersecting the horizontal floor plane ($Y = 0$).
- Movement is automatically constrained within room boundaries ($X \in [-3.5, 3.5]$ m, $Z \in [-2.5, 2.5]$ m).

### Rotating Furniture
- Select a furniture item.
- Press **`Q`** to rotate counter-clockwise ($-45^\circ / \text{sec}$).
- Press **`E`** to rotate clockwise ($+45^\circ / \text{sec}$).
- Alternatively, use the `Rot -45°` and `Rot +45°` buttons in the Inspector panel.

### Scaling Furniture
- Adjust the scale slider in the Inspector panel or use keyboard shortcuts.
- Scaling is uniformly clamped between $0.5\times$ and $2.0\times$ default dimensions.

---

## 4. Lighting Presets (Question 1)

The studio provides three switchable lighting presets designed to test material optics:
1. **Morning Sunlight (Preset 0 / Key `1`):**
   - Direct sunlight ($I = 1.35$, 6500K) streams through the South panoramic window at an angle of $38^\circ$.
   - Demonstrates diffuse radiance, sharp cast shadows, and grazing sheen on the **Red Velvet Chair**.
2. **Cozy Golden Evening (Preset 1 / Key `2`):**
   - Low-angle amber sunset ($I = 0.45$, 2700K) entering at $14^\circ$.
   - Activates the warm point light inside the **Kobenhavn Floor Lamp** and architectural ceiling pot lights.
3. **Neutral Studio Daylight (Preset 2 / Key `3`):**
   - High overhead balanced diffuse fill ($I = 1.10$, 5500K) simulating an overcast photographic studio.
   - Ideal for color-accurate material evaluation and client presentations.

---

## 5. Material Customisation & Normal Mapping (Question 2)

### Applying Material Presets
Select any placed furniture item and choose a material swatch from the right panel (or press **`M`** to cycle):
- **Royal Crimson Velvet:** Deep ruby red velvet with micro-pile sheen.
- **Nordic Emerald Velvet:** Luxurious jewel-tone dark green velvet.
- **Vintage Cognac Leather:** Full-grain saddle brown aniline leather with natural pebble grain.
- **Midnight Obsidian Leather:** Sleek contemporary black leather with crisp highlights.
- **Scandinavian Blonde Oak:** Natural light oak with satin varnish.
- **Smoked Dark Walnut:** Architectural dark walnut grain.
- **Polished Carrara Marble:** White Italian marble with smoke-grey mineral veins.

### Normal Mapping Toggle (Normal Map ON vs. OFF)
- Press **`N`** or click the **Normal Map Toggle** in the right panel.
- **Normal Map ENABLED:** Renders fine leather creases, surface pores, and stitching seams using tangent-space normal maps.
- **Normal Map DISABLED:** Shuts off normal perturbations, showing the underlying low-polygon geometric surface. This directly demonstrates the power of bump/normal mapping.

---

## 6. Room Layout Saving & Loading (JSON Persistence)

- **Save Room Layout:** Press **`S`** or click **`Save JSON`**. The room arrangement (furniture IDs, positions, rotations, scales, material IDs, and lighting preset) is serialized to `room_layout.json`.
- **Load Room Layout:** Press **`L`** or click **`Load JSON`**. Reconstructs the saved furniture arrangement instantly.
- **Reset Room:** Click **`Reset Room`** to clear placed furniture and revert to the default blank room.
