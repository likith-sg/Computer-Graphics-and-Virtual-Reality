import os
import math
import numpy as np
from PIL import Image, ImageDraw, ImageFilter

def ensure_dir(d):
    if not os.path.exists(d):
        os.makedirs(d)

tex_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "UnityProject", "Assets", "Textures"))
ensure_dir(tex_dir)
print(f"Generating photorealistic Scandinavian PBR textures in: {tex_dir}")

def height_to_normal(h, strength=3.0):
    # Sobel filter to compute gradients dh/dx and dh/dy
    sobel_x = np.array([[-1, 0, 1], [-2, 0, 2], [-1, 0, 1]], dtype=np.float32)
    sobel_y = np.array([[-1, -2, -1], [0, 0, 0], [1, 2, 1]], dtype=np.float32)
    
    pad_h = np.pad(h, 1, mode='wrap')
    # Manual 2D convolution with numpy slicing
    dx = (pad_h[0:-2, 2:] + 2*pad_h[1:-1, 2:] + pad_h[2:, 2:] - 
          pad_h[0:-2, 0:-2] - 2*pad_h[1:-1, 0:-2] - pad_h[2:, 0:-2]) / 8.0
    dy = (pad_h[2:, 0:-2] + 2*pad_h[2:, 1:-1] + pad_h[2:, 2:] - 
          pad_h[0:-2, 0:-2] - 2*pad_h[0:-2, 1:-1] - pad_h[0:-2, 2:]) / 8.0
    
    # Invert dy for Unity DirectX/OpenGL tangent convention (Unity expects +Y up)
    nx = -dx * strength
    ny = -dy * strength
    nz = np.ones_like(nx)
    
    length = np.sqrt(nx**2 + ny**2 + nz**2)
    nx /= length
    ny /= length
    nz /= length
    
    r = np.clip((nx * 0.5 + 0.5) * 255.0, 0, 255).astype(np.uint8)
    g = np.clip((ny * 0.5 + 0.5) * 255.0, 0, 255).astype(np.uint8)
    b = np.clip((nz * 0.5 + 0.5) * 255.0, 0, 255).astype(np.uint8)
    
    return np.stack([r, g, b], axis=-1)

# 1. LEATHER (Cognac Brown Leather for Sofa - Question 2)
def gen_leather(size=1024):
    print("Generating Leather textures...")
    np.random.seed(42)
    # Voronoi / cellular-like grain + fractal micro noise
    x = np.linspace(0, 16 * math.pi, size)
    y = np.linspace(0, 16 * math.pi, size)
    xx, yy = np.meshgrid(x, y)
    
    # Pebble cells
    grain = np.sin(xx * 2.5 + np.cos(yy * 2.2)) * np.cos(yy * 2.7 + np.sin(xx * 2.1))
    grain += 0.5 * np.sin(xx * 5.0 + yy * 4.5) * np.cos(yy * 5.5 - xx * 4.2)
    grain += 0.25 * np.sin(xx * 12.0) * np.sin(yy * 12.0)
    # Normalize grain 0..1
    grain = (grain - grain.min()) / (grain.max() - grain.min())
    
    # Micro creases
    creases = np.sin(xx * 0.7 + np.cos(yy * 1.1) * 2.0) + np.sin(yy * 0.8 + np.cos(xx * 1.3) * 1.8)
    creases = np.clip((creases - creases.min()) / (creases.max() - creases.min()), 0, 1)
    
    height = grain * 0.7 + creases * 0.3
    normal_map = height_to_normal(height, strength=4.5)
    
    # Albedo: rich cognac brown with natural patina and micro-tonal depth
    # Base cognac: RGB(168, 92, 45) -> (195, 115, 60)
    r = (145 + grain * 45 + creases * 20).clip(0, 255).astype(np.uint8)
    g = (75 + grain * 35 + creases * 15).clip(0, 255).astype(np.uint8)
    b = (35 + grain * 20 + creases * 10).clip(0, 255).astype(np.uint8)
    albedo = np.stack([r, g, b], axis=-1)
    
    Image.fromarray(albedo).save(os.path.join(tex_dir, "T_Leather_Albedo.png"))
    Image.fromarray(normal_map).save(os.path.join(tex_dir, "T_Leather_Normal.png"))
    
    # Roughness: leather is semi-glossy, creases are more rough
    roughness = (110 + (1.0 - grain) * 50).clip(0, 255).astype(np.uint8)
    Image.fromarray(roughness).save(os.path.join(tex_dir, "T_Leather_Roughness.png"))

# 2. RED VELVET (Velvet Chair - Question 1)
def gen_velvet(size=1024):
    print("Generating Velvet textures...")
    np.random.seed(101)
    x = np.linspace(0, 32 * math.pi, size)
    y = np.linspace(0, 32 * math.pi, size)
    xx, yy = np.meshgrid(x, y)
    
    # High frequency pile weave
    pile = np.sin(xx * 4.0) * np.cos(yy * 4.0) * 0.5 + 0.5
    pile += np.sin(xx * 8.0 + yy * 8.0) * 0.25
    # Normalize 0..1
    pile = (pile - pile.min()) / (pile.max() - pile.min())
    
    # Soft brush direction variation
    brush = np.sin(xx * 0.3 + yy * 0.2) * 0.5 + 0.5
    
    # Velvet normal map has very subtle, soft micro-variation
    normal_map = height_to_normal(pile, strength=1.2)
    
    # Crimson velvet albedo: Deep ruby red (165, 18, 38) with soft luminance gradients
    r = (140 + brush * 35 + pile * 15).clip(0, 255).astype(np.uint8)
    g = (10 + brush * 12).clip(0, 255).astype(np.uint8)
    b = (25 + brush * 18).clip(0, 255).astype(np.uint8)
    albedo = np.stack([r, g, b], axis=-1)
    
    Image.fromarray(albedo).save(os.path.join(tex_dir, "T_Velvet_Albedo.png"))
    Image.fromarray(normal_map).save(os.path.join(tex_dir, "T_Velvet_Normal.png"))

# 3. SCANDINAVIAN OAK WOOD (Coffee Table, Side Table, Legs)
def gen_wood(size=1024):
    print("Generating Scandinavian Oak textures...")
    np.random.seed(77)
    x = np.linspace(0, 10, size)
    y = np.linspace(0, 40, size)
    xx, yy = np.meshgrid(x, y)
    
    # Linear grain along Y axis with gentle natural waviness
    grain = np.sin(yy * 3.5 + np.sin(xx * 1.5) * 1.8)
    grain += 0.4 * np.sin(yy * 9.0 + np.sin(xx * 3.0))
    grain += 0.2 * np.sin(yy * 25.0)
    grain = (grain - grain.min()) / (grain.max() - grain.min())
    
    normal_map = height_to_normal(grain, strength=2.2)
    
    # Scandinavian natural white oak: light blonde honey tones (218, 185, 148)
    r = (195 + grain * 38).clip(0, 255).astype(np.uint8)
    g = (162 + grain * 34).clip(0, 255).astype(np.uint8)
    b = (125 + grain * 30).clip(0, 255).astype(np.uint8)
    albedo = np.stack([r, g, b], axis=-1)
    
    Image.fromarray(albedo).save(os.path.join(tex_dir, "T_Wood_Albedo.png"))
    Image.fromarray(normal_map).save(os.path.join(tex_dir, "T_Wood_Normal.png"))

# 4. HARDWOOD FLOOR (Plank parquet flooring)
def gen_floor(size=1024):
    print("Generating Oak Hardwood Flooring textures...")
    np.random.seed(88)
    x = np.linspace(0, 8, size)
    y = np.linspace(0, 32, size)
    xx, yy = np.meshgrid(x, y)
    
    # Plank seams: 4 planks across width
    plank_idx = np.floor(xx).astype(int)
    seam_x = np.abs(np.sin(xx * math.pi))
    seam_mask = np.where(seam_x < 0.04, 0.0, 1.0)
    
    # Grain inside each plank with varied tint
    grain = np.sin(yy * 2.8 + np.sin(xx * 2.0)) * 0.5 + 0.5
    grain += 0.25 * np.sin(yy * 7.5)
    
    # Height map has distinct bevels at seams
    height = grain * 0.4 + seam_mask * 0.6
    normal_map = height_to_normal(height, strength=4.0)
    
    # Base floor color with subtle plank variation
    tint = (np.sin(plank_idx * 1.7) * 15).astype(int)
    r = (190 + grain * 28 + tint) * seam_mask
    g = (155 + grain * 24 + tint) * seam_mask
    b = (118 + grain * 20 + tint) * seam_mask
    albedo = np.stack([r.clip(0, 255).astype(np.uint8), 
                       g.clip(0, 255).astype(np.uint8), 
                       b.clip(0, 255).astype(np.uint8)], axis=-1)
    
    Image.fromarray(albedo).save(os.path.join(tex_dir, "T_Floor_Albedo.png"))
    Image.fromarray(normal_map).save(os.path.join(tex_dir, "T_Floor_Normal.png"))

# 5. SCANDINAVIAN AREA RUG (Geometric woven diamond textile)
def gen_rug(size=1024):
    print("Generating Scandinavian Rug textures...")
    x = np.linspace(0, 8 * math.pi, size)
    y = np.linspace(0, 8 * math.pi, size)
    xx, yy = np.meshgrid(x, y)
    
    # Diamond lattice
    d1 = np.sin(xx + yy)
    d2 = np.sin(xx - yy)
    diamonds = np.where((d1 > 0.3) & (d2 > 0.3), 1.0, 0.0)
    
    # Wool weave noise
    weave = np.sin(xx * 16.0) * np.sin(y * 16.0) * 0.3
    height = diamonds * 0.5 + weave * 0.5
    normal_map = height_to_normal(height, strength=3.0)
    
    # Cream / Charcoal geometric pattern
    # Cream: (230, 226, 218), Charcoal: (58, 62, 65)
    r = np.where(diamonds > 0.5, 62, 230) + (weave * 15).astype(int)
    g = np.where(diamonds > 0.5, 66, 226) + (weave * 15).astype(int)
    b = np.where(diamonds > 0.5, 70, 218) + (weave * 15).astype(int)
    albedo = np.stack([r.clip(0, 255).astype(np.uint8), 
                       g.clip(0, 255).astype(np.uint8), 
                       b.clip(0, 255).astype(np.uint8)], axis=-1)
    
    Image.fromarray(albedo).save(os.path.join(tex_dir, "T_Rug_Albedo.png"))
    Image.fromarray(normal_map).save(os.path.join(tex_dir, "T_Rug_Normal.png"))

# 6. ARCHITECTURAL PLASTER WALL
def gen_wall(size=1024):
    print("Generating Plaster Wall textures...")
    np.random.seed(55)
    # Fine stipple
    stipple = np.random.normal(0.5, 0.08, (size, size))
    stipple = np.clip(stipple, 0, 1)
    
    normal_map = height_to_normal(stipple, strength=1.5)
    
    # Warm Nordic off-white wall (242, 240, 235)
    r = (240 + stipple * 8).clip(0, 255).astype(np.uint8)
    g = (238 + stipple * 8).clip(0, 255).astype(np.uint8)
    b = (233 + stipple * 8).clip(0, 255).astype(np.uint8)
    albedo = np.stack([r, g, b], axis=-1)
    
    Image.fromarray(albedo).save(os.path.join(tex_dir, "T_Wall_Albedo.png"))
    Image.fromarray(normal_map).save(os.path.join(tex_dir, "T_Wall_Normal.png"))

# 7. WHITE CARRARA MARBLE (Tabletop option)
def gen_marble(size=1024):
    print("Generating Marble textures...")
    np.random.seed(99)
    x = np.linspace(0, 6 * math.pi, size)
    y = np.linspace(0, 6 * math.pi, size)
    xx, yy = np.meshgrid(x, y)
    
    # Turbulent veins
    turb = np.sin(xx + np.sin(yy * 1.5) * 2.0) + 0.5 * np.sin(yy * 2.0 + np.sin(xx * 2.0))
    turb = (turb - turb.min()) / (turb.max() - turb.min())
    veins = np.exp(-((turb - 0.5)**2) / 0.015)
    
    normal_map = height_to_normal(veins, strength=1.0)
    
    # White with soft smoke-grey veins
    r = (245 - veins * 85).clip(0, 255).astype(np.uint8)
    g = (245 - veins * 80).clip(0, 255).astype(np.uint8)
    b = (248 - veins * 75).clip(0, 255).astype(np.uint8)
    albedo = np.stack([r, g, b], axis=-1)
    
    Image.fromarray(albedo).save(os.path.join(tex_dir, "T_Marble_Albedo.png"))
    Image.fromarray(normal_map).save(os.path.join(tex_dir, "T_Marble_Normal.png"))

# 8. LINEN FABRIC (Sofa upholstery alternative)
def gen_fabric(size=1024):
    print("Generating Linen Fabric textures...")
    x = np.linspace(0, 32 * math.pi, size)
    y = np.linspace(0, 32 * math.pi, size)
    xx, yy = np.meshgrid(x, y)
    
    weave = (np.sin(xx * 4.0) + np.cos(yy * 4.0)) * 0.5
    weave = (weave - weave.min()) / (weave.max() - weave.min())
    
    normal_map = height_to_normal(weave, strength=2.8)
    
    # Warm Oat / Natural Linen (210, 204, 192)
    r = (202 + weave * 18).clip(0, 255).astype(np.uint8)
    g = (196 + weave * 18).clip(0, 255).astype(np.uint8)
    b = (185 + weave * 16).clip(0, 255).astype(np.uint8)
    albedo = np.stack([r, g, b], axis=-1)
    
    Image.fromarray(albedo).save(os.path.join(tex_dir, "T_Fabric_Albedo.png"))
    Image.fromarray(normal_map).save(os.path.join(tex_dir, "T_Fabric_Normal.png"))

# 9. SCANDINAVIAN ARTWORK (Framed Wall Art)
def gen_art(size=1024):
    print("Generating Scandinavian Wall Art...")
    img = Image.new("RGB", (size, size), (245, 243, 238))
    draw = ImageDraw.Draw(img)
    # Minimalist abstract geometric art: arch, terracotta circle, sage green wedge
    # Large neutral terracotta circle
    draw.ellipse([size * 0.25, size * 0.2, size * 0.75, size * 0.7], fill=(204, 119, 88))
    # Elegant sage green arch
    draw.chord([size * 0.35, size * 0.45, size * 0.85, size * 0.95], 180, 360, fill=(128, 145, 126))
    # Charcoal minimalist arc
    draw.arc([size * 0.15, size * 0.15, size * 0.85, size * 0.85], 0, 180, fill=(40, 42, 44), width=12)
    img.save(os.path.join(tex_dir, "T_WallArt.png"))

if __name__ == "__main__":
    gen_leather()
    gen_velvet()
    gen_wood()
    gen_floor()
    gen_rug()
    gen_wall()
    gen_marble()
    gen_fabric()
    gen_art()
    print("ALL 17 PBR TEXTURES GENERATED SUCCESSFULLY!")
