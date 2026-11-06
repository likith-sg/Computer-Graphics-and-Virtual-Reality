import os

readme_path = "README.md"
with open(readme_path, "r", encoding="utf-8") as f:
    content = f.read()

target = "\n---\n\n## Installation and Setup\n"
cs3_content = """
---

## Case Study 3: Retail and Remote Commerce

**Location:** [`Case Study 3/`](Case%20Study%203/) &bull; **Documentation:** [`Case Study 3/README.md`](Case%20Study%203/README.md) &bull; **Interactive Program:** [`Case Study 3/interior_studio.py`](Case%20Study%203/interior_studio.py) &bull; **Unity Project:** [`Case Study 3/UnityProject/`](Case%20Study%203/UnityProject/) &bull; **Report:** [DOCX](Case%20Study%203/Report/CS4104_Case_Study_3.docx) | [PDF](Case%20Study%203/Report/CS4104_Case_Study_3.pdf)

### Scenario: Retail and Remote Commerce (Virtual Interior Design Studio)
An interior design startup developing an interactive WebXR-ready 3D application allowing customers to place, inspect, and customize furniture in a photorealistic Scandinavian living room.

### Key Computer Graphics Concepts
1. **Illumination and Colour Models (Question 1):** Multi-component Phong ($I = k_a I_a + k_d I_l(\\mathbf{N}\\cdot\\mathbf{L}) + k_s I_l(\\mathbf{R}\\cdot\\mathbf{V})^n$) and Microfacet PBR, investigating optical retro-reflection, cut-pile fiber light trapping, and grazing-angle sheen on a virtual Red Velvet Armchair across Morning Sunlight, Cozy Evening, and Studio Daylight presets.
2. **Texture and Bump/Normal Mapping (Question 2):** Tangent-space normal mapping ($\\mathbf{N}_{\\text{world}} = \\mathbf{M}_{\\text{TBN}} \\cdot \\mathbf{n}_{\\text{tangent}}$) on a 3-seater Stockholm Leather Sofa, achieving rich pebble grain, creases, and stitching detail with 2,450 triangles instead of 520,000 (99.5% polygon budget reduction).
3. **User Interaction & Coordinate Transforms (Question 3):** Window-to-viewport linear coordinate mapping, Normalized Device Coordinates (NDC), and unprojected camera ray generation ($\\mathbf{P}(t) = \\mathbf{O} + t\\mathbf{D}$) intersecting the floor plane ($Y = 0$) at $t = -O_y / D_y$ for planar translation, axial rotation, and scaling.
4. **Layout Persistence:** Full JSON room state serialization and restoration (`room_layout.json`).

### Execution Command
Run the standalone interactive 3D Virtual Interior Design Studio:
```bash
python "Case Study 3/interior_studio.py"
```

To run in Unity 6 Editor, open `Case Study 3/UnityProject/` and load `Assets/Scenes/InteriorStudio.unity`.

### Visual Result

#### Virtual Interior Design Studio (Hero Static View)
<img src="Case Study 3/output.png" alt="Virtual Interior Design Studio Hero View" width="700">

#### Interactive 3D Turntable Demo (GIF)
<img src="Case Study 3/demo.gif" alt="Virtual Interior Design Studio 360 Turntable Demo" width="650">

*Demonstrates the Scandinavian living room: Nordic Velvet Armchair in morning sunlight with grazing sheen, Stockholm Leather Sofa with normal-mapped creases, Oslo Oak Coffee Table, Kobenhavn Floor Lamp, and interactive selection HUD.*

---

## Installation and Setup
"""

tree_target = """├── Case Study/
│   ├── README.md
│   ├── verify_viewport.py
│   └── unity_project/
│       ├── Assets/
│       │   ├── Scripts/
│       │   ├── Editor/
│       │   └── Scenes/
│       ├── Packages/
│       └── ProjectSettings/
│"""

tree_replacement = """├── Case Study/
│   ├── README.md
│   ├── verify_viewport.py
│   └── unity_project/
│       ├── Assets/
│       │   ├── Scripts/
│       │   ├── Editor/
│       │   └── Scenes/
│       ├── Packages/
│       └── ProjectSettings/
│
├── Case Study 3/
│   ├── README.md
│   ├── interior_studio.py
│   ├── demo.gif
│   ├── output.png
│   ├── UnityProject/
│   ├── Report/
│   │   ├── CS4104_Case_Study_3.docx
│   │   └── CS4104_Case_Study_3.pdf
│   ├── Screenshots/
│   └── Documentation/
│"""

if target in content:
    content = content.replace(target, cs3_content, 1)
else:
    print("Warning: target string not found!")

if tree_target in content:
    content = content.replace(tree_target, tree_replacement, 1)

with open(readme_path, "w", encoding="utf-8") as f:
    f.write(content)

print("Root README.md updated with minimal Case Study 3 link successfully!")
