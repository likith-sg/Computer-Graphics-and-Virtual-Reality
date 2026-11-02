"""
Experiment 7: Phong Illumination Model for a 3D Object
Student Name: Likith S G
USN: 1RVU23CSE236
Course: Computer Graphics and Virtual Reality Laboratory

Description:
This program implements the Phong Illumination Model, demonstrating how local lighting
is decomposed into Ambient, Diffuse (Lambertian), and Specular components.
It renders four synchronized viewports showing:
1. Ambient Component (uniform background illumination)
2. Diffuse Component (directional cosine-law scattering)
3. Specular Component (glossy mirror reflection highlight)
4. Full Combined Phong Illumination Model
"""

import sys
import os
from math import sin, cos, pi, sqrt
import glfw
from OpenGL.GL import *
from OpenGL.GLU import *
from PIL import Image, ImageDraw, ImageFont

WINDOW_WIDTH = 1400
WINDOW_HEIGHT = 800

# Phong Material & Light Coefficients
KA = 0.18           # Ambient reflection coefficient
KD = 0.72           # Diffuse reflection coefficient
KS = 0.65           # Specular reflection coefficient
SHININESS = 32.0    # Specular shininess exponent (alpha)

# Material Base Color (Rich Coral/Amber tone)
BASE_COLOR = (0.92, 0.38, 0.22)
SPECULAR_COLOR = (1.0, 1.0, 1.0)

# Light direction angles in spherical space
LIGHT_THETA = 0.85
LIGHT_PHI = 0.65


def get_light_direction():
    """Computes normalized light direction vector L pointing towards the light source."""
    lx = cos(LIGHT_PHI) * sin(LIGHT_THETA)
    ly = sin(LIGHT_PHI)
    lz = cos(LIGHT_PHI) * cos(LIGHT_THETA)
    length = sqrt(lx * lx + ly * ly + lz * lz)
    return (lx / length, ly / length, lz / length)


def draw_lit_sphere(mode="full"):
    """
    Renders a 3D sphere tessellated with triangle strips and computes
    Phong lighting at each vertex according to mode ('ambient', 'diffuse', 'specular', 'full').
    """
    lx, ly, lz = get_light_direction()
    vx, vy, vz = 0.0, 0.0, 1.0  # Camera vector V looking straight along +Z

    stacks = 55
    sectors = 110

    for i in range(stacks):
        lat0 = -pi / 2.0 + i * pi / float(stacks)
        lat1 = -pi / 2.0 + (i + 1) * pi / float(stacks)

        glBegin(GL_TRIANGLE_STRIP)
        for j in range(sectors + 1):
            lng = j * 2.0 * pi / float(sectors)

            for lat in (lat0, lat1):
                # Normal vector N for unit sphere equals its vertex coordinates
                nx = cos(lat) * cos(lng)
                ny = sin(lat)
                nz = cos(lat) * sin(lng)

                # Dot product (N . L)
                n_dot_l = nx * lx + ny * ly + nz * lz

                # 1. Ambient Lighting (ka * Ia)
                amb_r = KA * BASE_COLOR[0]
                amb_g = KA * BASE_COLOR[1]
                amb_b = KA * BASE_COLOR[2]

                # 2. Diffuse Lighting (kd * max(0, N . L) * Id)
                diff_factor = max(0.0, n_dot_l)
                diff_r = KD * diff_factor * BASE_COLOR[0]
                diff_g = KD * diff_factor * BASE_COLOR[1]
                diff_b = KD * diff_factor * BASE_COLOR[2]

                # 3. Specular Highlight (ks * max(0, R . V)^alpha * Is)
                spec_r = spec_g = spec_b = 0.0
                if n_dot_l > 0.0:
                    # Reflection vector R = 2(N.L)N - L
                    rx = 2.0 * n_dot_l * nx - lx
                    ry = 2.0 * n_dot_l * ny - ly
                    rz = 2.0 * n_dot_l * nz - lz

                    # (R . V)
                    r_dot_v = rx * vx + ry * vy + rz * vz
                    if r_dot_v > 0.0:
                        spec_factor = r_dot_v ** SHININESS
                        spec_r = KS * spec_factor * SPECULAR_COLOR[0]
                        spec_g = KS * spec_factor * SPECULAR_COLOR[1]
                        spec_b = KS * spec_factor * SPECULAR_COLOR[2]

                # Output final color based on selected panel mode
                if mode == "ambient":
                    glColor3f(amb_r, amb_g, amb_b)
                elif mode == "diffuse":
                    glColor3f(diff_r, diff_g, diff_b)
                elif mode == "specular":
                    glColor3f(spec_r, spec_g, spec_b)
                else:  # Full Phong Model
                    final_r = min(1.0, amb_r + diff_r + spec_r)
                    final_g = min(1.0, amb_g + diff_g + spec_g)
                    final_b = min(1.0, amb_b + diff_b + spec_b)
                    glColor3f(final_r, final_g, final_b)

                glVertex3f(nx * 0.95, ny * 0.95, nz * 0.95)

        glEnd()


def draw_viewport_border(w, h):
    """Draws a neat boundary box around a viewport."""
    glMatrixMode(GL_PROJECTION)
    glPushMatrix()
    glLoadIdentity()
    glOrtho(0, w, 0, h, -1, 1)

    glMatrixMode(GL_MODELVIEW)
    glPushMatrix()
    glLoadIdentity()

    glColor3f(0.24, 0.28, 0.38)
    glLineWidth(1.5)
    glBegin(GL_LINE_LOOP)
    glVertex2f(8, 8)
    glVertex2f(w - 8, 8)
    glVertex2f(w - 8, h - 8)
    glVertex2f(8, h - 8)
    glEnd()

    glPopMatrix()
    glMatrixMode(GL_PROJECTION)
    glPopMatrix()
    glMatrixMode(GL_MODELVIEW)


def render_scene():
    """Renders 4 synchronized quadrants displaying Phong model decomposition."""
    glClearColor(0.06, 0.07, 0.10, 1.0)
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)
    glEnable(GL_DEPTH_TEST)

    qw = WINDOW_WIDTH // 2
    qh = WINDOW_HEIGHT // 2

    viewports = [
        # (x, y, mode, label)
        (0,  qh, "ambient",  "1. Ambient Component"),
        (qw, qh, "diffuse",  "2. Diffuse Component"),
        (0,  0,  "specular", "3. Specular Highlight"),
        (qw, 0,  "full",     "4. Full Phong Model")
    ]

    for vx, vy, mode, _ in viewports:
        glViewport(vx, vy, qw, qh)

        glMatrixMode(GL_PROJECTION)
        glLoadIdentity()
        glOrtho(-1.4, 1.4, -1.4, 1.4, -2.0, 2.0)

        glMatrixMode(GL_MODELVIEW)
        glLoadIdentity()

        draw_lit_sphere(mode)
        draw_viewport_border(qw, qh)


def add_labels_and_save(output_path):
    """Captures framebuffer and superimposes high-clarity mathematical documentation."""
    glReadBuffer(GL_BACK)
    glFinish()
    glPixelStorei(GL_PACK_ALIGNMENT, 1)
    data = glReadPixels(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT, GL_RGB, GL_UNSIGNED_BYTE)
    image = Image.frombytes("RGB", (WINDOW_WIDTH, WINDOW_HEIGHT), data)
    image = image.transpose(Image.FLIP_TOP_BOTTOM)

    draw = ImageDraw.Draw(image)
    try:
        font_title = ImageFont.truetype("C:/Windows/Fonts/segoeuib.ttf", 20)
        font_sub = ImageFont.truetype("C:/Windows/Fonts/segoeui.ttf", 15)
        font_math = ImageFont.truetype("C:/Windows/Fonts/segoeuib.ttf", 13)
        font_tag = ImageFont.truetype("C:/Windows/Fonts/segoeui.ttf", 13)
    except Exception:
        font_title = font_sub = font_math = font_tag = ImageFont.load_default()

    qw = WINDOW_WIDTH // 2
    qh = WINDOW_HEIGHT // 2

    # Global Top Header Bar
    draw.rectangle([(0, 0), (WINDOW_WIDTH, 48)], fill=(18, 22, 30))
    draw.text((25, 12), "Experiment 7: Phong Illumination Model for a 3D Object", fill=(240, 245, 255), font=font_title)
    draw.text((WINDOW_WIDTH - 420, 15), "Student: Likith S G | USN: 1RVU23CSE236", fill=(170, 185, 205), font=font_sub)

    # Panel 1: Ambient
    draw.rectangle([(20, 60), (qw - 20, 115)], fill=(25, 28, 38), outline=(80, 140, 240), width=1)
    draw.text((35, 66), "Panel 1: Ambient Lighting (I_amb)", fill=(100, 180, 255), font=font_title)
    draw.text((35, 92), "Formula: I_amb = ka * Ia (Uniform non-directional environmental light)", fill=(200, 210, 225), font=font_math)

    # Panel 2: Diffuse
    draw.rectangle([(qw + 20, 60), (WINDOW_WIDTH - 20, 115)], fill=(25, 28, 38), outline=(50, 200, 120), width=1)
    draw.text((qw + 35, 66), "Panel 2: Diffuse Reflection (I_diff)", fill=(70, 230, 130), font=font_title)
    draw.text((qw + 35, 92), "Formula: I_diff = kd * (N . L) * Id (Lambert's cosine scattering law)", fill=(200, 210, 225), font=font_math)

    # Panel 3: Specular
    draw.rectangle([(20, qh + 20), (qw - 20, qh + 75)], fill=(25, 28, 38), outline=(240, 180, 50), width=1)
    draw.text((35, qh + 26), "Panel 3: Specular Highlight (I_spec)", fill=(255, 200, 70), font=font_title)
    draw.text((35, qh + 52), "Formula: I_spec = ks * (R . V)^alpha * Is (Glossy specular shine, alpha=32)", fill=(200, 210, 225), font=font_math)

    # Panel 4: Full Phong Model
    draw.rectangle([(qw + 20, qh + 20), (WINDOW_WIDTH - 20, qh + 75)], fill=(25, 28, 38), outline=(240, 80, 80), width=1)
    draw.text((qw + 35, qh + 26), "Panel 4: Complete Phong Model (I_total)", fill=(255, 100, 100), font=font_title)
    draw.text((qw + 35, qh + 52), "Formula: I = I_amb + I_diff + I_spec (Photorealistic 3D synthesis)", fill=(200, 210, 225), font=font_math)

    # Center Cross Dividers
    draw.line([(qw, 48), (qw, WINDOW_HEIGHT)], fill=(60, 70, 90), width=2)
    draw.line([(0, qh), (WINDOW_WIDTH, qh)], fill=(60, 70, 90), width=2)

    image.save(output_path)
    print(f"[SUCCESS] Saved framebuffer to {output_path}")

    assets_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", "assets", "screenshots"))
    if os.path.exists(assets_dir):
        assets_path = os.path.join(assets_dir, "exp7.png")
        image.save(assets_path)
        print(f"[SUCCESS] Mirrored screenshot to {assets_path}")


def main():
    headless = "--headless" in sys.argv or "--save" in sys.argv or "--screenshot" in sys.argv

    if not glfw.init():
        raise RuntimeError("GLFW initialization failed")

    glfw.window_hint(glfw.DEPTH_BITS, 24)
    if headless:
        glfw.window_hint(glfw.VISIBLE, glfw.FALSE)

    window = glfw.create_window(WINDOW_WIDTH, WINDOW_HEIGHT, "Experiment 7 - Phong Illumination Model (Likith S G)", None, None)
    if not window:
        glfw.terminate()
        raise RuntimeError("Failed to create GLFW window")

    glfw.make_context_current(window)

    render_scene()
    output_file = os.path.join(os.path.dirname(__file__), "output.png")
    add_labels_and_save(output_file)
    glfw.swap_buffers(window)

    if headless:
        glfw.destroy_window(window)
        glfw.terminate()
        return

    while not glfw.window_should_close(window):
        render_scene()
        glfw.swap_buffers(window)
        glfw.poll_events()

    glfw.destroy_window(window)
    glfw.terminate()


if __name__ == "__main__":
    main()
