# Asset Credits and Provenance
## CS4104 Case Study 3: Retail and Remote Commerce
**Virtual Interior Design Studio: An Interactive 3D Furniture Placement and Customisation WebXR Application**  
**Student Name:** Likith S G | **USN:** `1RVU23CSE236` | **Institution:** RV University, Bengaluru  

---

## 1. 3D Model Assets and Geometry

All 3D furniture meshes, architectural room diorama geometry, and collision volumes in the Virtual Interior Design Studio were **procedurally authored and modelled** directly within the project using custom geometric algorithms (`StudioAssetGenerator.cs` and `interior_studio.py`):

1. **Nordic Velvet Armchair (Question 1 Hero Asset):**
   - *Author:* Likith S G (Independent procedural modeling for CS4104)
   - *Geometry:* Barrel curved backrest cylinder, plush box cushion, flared armrests, and 4 tapered splayed legs with brass caps.
   - *Polygon Budget:* 1,840 triangles.
   - *License:* CC0 / Academic Use (No external model dependencies).

2. **Stockholm 3-Seater Leather Sofa (Question 2 Hero Asset):**
   - *Author:* Likith S G (Independent procedural modeling for CS4104)
   - *Geometry:* Plinth base frame, 3 deep tufted seat cushions, 3 recline backrest cushions, 2 armrests, and 4 oak legs.
   - *Polygon Budget:* 2,450 triangles.
   - *License:* CC0 / Academic Use.

3. **Oslo Oak Coffee Table:**
   - *Author:* Likith S G
   - *Geometry:* Rounded pill tabletop with 4 splayed tapered tripod legs.
   - *Polygon Budget:* 920 triangles.
   - *License:* CC0 / Academic Use.

4. **Bergen Pedestal Side Table:**
   - *Author:* Likith S G
   - *Geometry:* Circular Carrara marble top with black steel central stem and weighted disc base.
   - *Polygon Budget:* 640 triangles.
   - *License:* CC0 / Academic Use.

5. **Kobenhavn Arc Floor Lamp:**
   - *Author:* Likith S G
   - *Geometry:* Weighted brass circular base, 1.85m curved arching stem, conical shade, and active point light.
   - *Polygon Budget:* 1,120 triangles.
   - *License:* CC0 / Academic Use.

6. **Monstera Deliciosa in Planter:**
   - *Author:* Likith S G
   - *Geometry:* Terracotta ceramic planter pot, soil disc, and 6 organic broad-leaf fronds.
   - *Polygon Budget:* 1,450 triangles.
   - *License:* CC0 / Academic Use.

7. **Scandinavian Living Room Architecture:**
   - *Author:* Likith S G
   - *Geometry:* 8m x 6m x 3.2m room diorama, floor plane, baseboard mouldings, perimeter walls with double-glazed panoramic window aperture, black aluminum mullions, and framed wall art.
   - *Polygon Budget:* 10,030 triangles.
   - *License:* CC0 / Academic Use.

---

## 2. 2D Texture and Normal Map Assets

All 17 high-resolution PBR texture maps (Albedo, Tangent-Space Normal Maps, and Roughness) were **procedurally generated** via custom mathematical Python scripts (`Case Study 3/Tools/generate_textures.py`) utilizing cellular Voronoi grain, Perlin noise, Sobel spatial gradient filters, and vector normalization:

| Texture Map Asset | Resolution | Encoding | Function & Description | License |
|:---|:---:|:---:|:---|:---:|
| `T_Velvet_Albedo.png` | $1024 \times 1024$ | sRGB (RGB24) | Crimson red velvet pile albedo with micro-fiber tonal variation. | CC0 |
| `T_Velvet_Normal.png` | $1024 \times 1024$ | Linear Normal | Tangent-space normal map for velvet pile surface variation. | CC0 |
| `T_Leather_Albedo.png` | $1024 \times 1024$ | sRGB (RGB24) | Rich vintage cognac saddle leather with tanning patina. | CC0 |
| `T_Leather_Normal.png` | $1024 \times 1024$ | Linear Normal | Tangent-space normal map encoding leather pebble grain & creases. | CC0 |
| `T_Leather_Roughness.png` | $1024 \times 1024$ | Grayscale | Smoothness map differentiating glossy flat leather from rough creases. | CC0 |
| `T_Wood_Albedo.png` | $1024 \times 1024$ | sRGB (RGB24) | Scandinavian blonde natural oak with growth rings and grain. | CC0 |
| `T_Wood_Normal.png` | $1024 \times 1024$ | Linear Normal | Linear wood grain striation tangent-space normal map. | CC0 |
| `T_Floor_Albedo.png` | $1024 \times 1024$ | sRGB (RGB24) | Oak hardwood parquet flooring with individual plank variations. | CC0 |
| `T_Floor_Normal.png` | $1024 \times 1024$ | Linear Normal | Beveled plank seam joints and hardwood grain normal map. | CC0 |
| `T_Rug_Albedo.png` | $1024 \times 1024$ | sRGB (RGB24) | Scandinavian geometric diamond woven textile in cream/charcoal. | CC0 |
| `T_Rug_Normal.png` | $1024 \times 1024$ | Linear Normal | Woven loop pile wool normal map. | CC0 |
| `T_Wall_Albedo.png` | $1024 \times 1024$ | sRGB (RGB24) | Architectural Nordic off-white plaster with subtle stipple. | CC0 |
| `T_Wall_Normal.png` | $1024 \times 1024$ | Linear Normal | Fine architectural wall plaster stipple normal map. | CC0 |
| `T_Marble_Albedo.png` | $1024 \times 1024$ | sRGB (RGB24) | Italian Carrara white marble with smoke-grey mineral veining. | CC0 |
| `T_Marble_Normal.png` | $1024 \times 1024$ | Linear Normal | Subtle marble vein micro-depth normal map. | CC0 |
| `T_Fabric_Albedo.png` | $1024 \times 1024$ | sRGB (RGB24) | Natural Scandinavian flax linen in warm oat. | CC0 |
| `T_Fabric_Normal.png` | $1024 \times 1024$ | Linear Normal | Crisp cross-hatch linen textile normal map. | CC0 |
| `T_WallArt.png` | $1024 \times 1024$ | sRGB (RGB24) | Modern minimalist Scandinavian geometric art print. | CC0 |

---

## 3. Institutional Identity & University Branding

- **University Crest / Logo:** `rvu_logo.png`
  - *Entity:* RV University, Bengaluru (School of Computer Science and Engineering)
  - *Usage:* Cover page, certificate, and academic project documentation in accordance with university assignment submission guidelines.
