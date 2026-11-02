"""
Experiment 10: 3D Viewing Pipeline, Projection, and Viewport Mapping
Student Name: Likith S G
USN: 1RVU23CSE236
Course: Computer Graphics and Virtual Reality Laboratory

Description:
This program implements and demonstrates the complete 3D Viewing Pipeline:
[Object Coordinates] -> [World] -> [Eye / Camera] -> [Clip] -> [NDC] -> [Screen Viewports].
It presents a synchronized dual-viewport comparison displaying an animated 3D cube under:
1. Left Viewport: Orthographic Projection (parallel projectors, distance-invariant CAD view).
2. Right Viewport: Perspective Projection (converging projectors with natural depth foreshortening).
"""

import sys
import os
import math
import glfw
from OpenGL.GL import *
from OpenGL.GLU import *
from PIL import Image, ImageDraw, ImageFont

WINDOW_WIDTH = 1300
WINDOW_HEIGHT = 700

# 3D Unit Cube Geometry Definition
CUBE_VERTICES = [
    (-1.0, -1.0, -1.0),
    ( 1.0, -1.0, -1.0),
    ( 1.0,  1.0, -1.0),
    (-1.0,  1.0, -1.0),
    (-1.0, -1.0,  1.0),
    ( 1.0, -1.0,  1.0),
    ( 1.0,  1.0,  1.0),
    (-1.0,  1.0,  1.0),
]

CUBE_FACES = [
    (0, 1, 2, 3),  # Back Face
    (4, 5, 6, 7),  # Front Face
    (0, 1, 5, 4),  # Bottom Face
    (2, 3, 7, 6),  # Top Face
    (1, 2, 6, 5),  # Right Face
    (0, 3, 7, 4),  # Left Face
]

CUBE_EDGES = [
    (0, 1), (1, 2), (2, 3), (3, 0),
    (4, 5), (5, 6), (6, 7), (7, 4),
    (0, 4), (1, 5), (2, 6), (3, 7),
]

FACE_COLORS = [
    (0.95, 0.25, 0.25),  # Red
    (0.20, 0.85, 0.35),  # Green
    (0.25, 0.50, 0.95),  # Blue
    (1.00, 0.82, 0.20),  # Yellow
    (0.85, 0.25, 0.95),  # Magenta
    (0.20, 0.90, 0.90),  # Cyan
]

cube_angle = 35.0


def draw_colored_cube():
    """Renders a solid colored 3D cube with crisp wireframe silhouette edges."""
    glEnable(GL_DEPTH_TEST)

    # 1. Solid Faces
    glBegin(GL_QUADS)
    for i, face in enumerate(CUBE_FACES):
        glColor3fv(FACE_COLORS[i])
        for v_idx in face:
            glVertex3fv(CUBE_VERTICES[v_idx])
    glEnd()

    # 2. Black Wireframe Edges (Geometric clarity)
    glColor3f(0.06, 0.07, 0.10)
    glLineWidth(2.5)
    glBegin(GL_LINES)
    for edge in CUBE_EDGES:
        for v_idx in edge:
            glVertex3fv(CUBE_VERTICES[v_idx])
    glEnd()


def draw_viewport_border(w, h):
    """Draws an outline border around the viewport."""
    glMatrixMode(GL_PROJECTION)
    glPushMatrix()
    glLoadIdentity()
    glOrtho(0, w, 0, h, -1, 1)

    glMatrixMode(GL_MODELVIEW)
    glPushMatrix()
    glLoadIdentity()

    glColor3f(0.28, 0.32, 0.44)
    glLineWidth(2.0)
    glBegin(GL_LINE_LOOP)
    glVertex2f(10, 10)
    glVertex2f(w - 10, 10)
    glVertex2f(w - 10, h - 10)
    glVertex2f(10, h - 10)
    glEnd()

    glPopMatrix()
    glMatrixMode(GL_PROJECTION)
    glPopMatrix()
    glMatrixMode(GL_MODELVIEW)


def render_scene(angle):
    """Renders dual synchronized viewports: Left Orthographic vs Right Perspective."""
    glClearColor(0.06, 0.07, 0.10, 1.0)
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)

    half_width = WINDOW_WIDTH // 2

    # ------------------------------------------------------------
    # LEFT VIEWPORT: ORTHOGRAPHIC PROJECTION (glOrtho)
    # ------------------------------------------------------------
    glViewport(0, 0, half_width, WINDOW_HEIGHT)

    glMatrixMode(GL_PROJECTION)
    glLoadIdentity()
    aspect = half_width / float(WINDOW_HEIGHT)
    ortho_scale = 3.2
    glOrtho(-ortho_scale * aspect, ortho_scale * aspect, -ortho_scale, ortho_scale, 1.0, 30.0)

    glMatrixMode(GL_MODELVIEW)
    glLoadIdentity()
    # Camera View Transformation: Eye=(4, 3, 6), Target=(0,0,0), Up=(0,1,0)
    gluLookAt(4.0, 3.2, 6.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0)

    # Model World Transformations
    glRotatef(angle, 0.0, 1.0, 0.0)
    glRotatef(angle * 0.6, 1.0, 0.0, 0.0)

    draw_colored_cube()
    draw_viewport_border(half_width, WINDOW_HEIGHT)

    # ------------------------------------------------------------
    # RIGHT VIEWPORT: PERSPECTIVE PROJECTION (gluPerspective)
    # ------------------------------------------------------------
    glViewport(half_width, 0, half_width, WINDOW_HEIGHT)

    glMatrixMode(GL_PROJECTION)
    glLoadIdentity()
    gluPerspective(45.0, aspect, 0.1, 100.0)

    glMatrixMode(GL_MODELVIEW)
    glLoadIdentity()
    gluLookAt(4.0, 3.2, 6.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0)

    glRotatef(angle, 0.0, 1.0, 0.0)
    glRotatef(angle * 0.6, 1.0, 0.0, 0.0)

    draw_colored_cube()
    draw_viewport_border(half_width, WINDOW_HEIGHT)


def capture_frame():
    """Reads back the active framebuffer as a PIL image."""
    glReadBuffer(GL_BACK)
    glFinish()
    glPixelStorei(GL_PACK_ALIGNMENT, 1)
    data = glReadPixels(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT, GL_RGB, GL_UNSIGNED_BYTE)
    image = Image.frombytes("RGB", (WINDOW_WIDTH, WINDOW_HEIGHT), data)
    return image.transpose(Image.FLIP_TOP_BOTTOM)


def add_labels_and_save(output_path):
    """Overlays rich comparative pipeline documentation onto the captured frame."""
    image = capture_frame()
    draw = ImageDraw.Draw(image)
    try:
        font_title = ImageFont.truetype("C:/Windows/Fonts/segoeuib.ttf", 20)
        font_sub = ImageFont.truetype("C:/Windows/Fonts/segoeui.ttf", 15)
        font_tag = ImageFont.truetype("C:/Windows/Fonts/segoeuib.ttf", 13)
        font_math = ImageFont.truetype("C:/Windows/Fonts/consola.ttf", 13)
    except Exception:
        font_title = font_sub = font_tag = font_math = ImageFont.load_default()

    half_width = WINDOW_WIDTH // 2

    # Global Top Header Banner
    draw.rectangle([(0, 0), (WINDOW_WIDTH, 48)], fill=(18, 22, 30))
    draw.text((25, 12), "Experiment 10: 3D Viewing Pipeline, Projection, and Viewport Mapping", fill=(240, 245, 255), font=font_title)
    draw.text((WINDOW_WIDTH - 420, 15), "Student: Likith S G | USN: 1RVU23CSE236", fill=(170, 185, 205), font=font_sub)

    # Left Viewport Card: Orthographic Projection
    draw.rectangle([(20, 60), (half_width - 20, 120)], fill=(22, 26, 36, 230), outline=(60, 160, 240), width=2)
    draw.text((35, 68), "ORTHOGRAPHIC PROJECTION (glOrtho)", fill=(90, 190, 255), font=font_title)
    draw.text((35, 94), "Parallel projectors: Object size is distance-invariant (Zero foreshortening)", fill=(200, 215, 235), font=font_sub)

    # Left Bottom Feature Card
    draw.rectangle([(30, WINDOW_HEIGHT - 70), (half_width - 30, WINDOW_HEIGHT - 25)], fill=(20, 26, 38), outline=(60, 130, 200))
    draw.text((45, WINDOW_HEIGHT - 60), "Use Cases: Engineering drawings, CAD blueprints, Architectural floorplans", fill=(140, 210, 255), font=font_tag)

    # Right Viewport Card: Perspective Projection
    draw.rectangle([(half_width + 20, 60), (WINDOW_WIDTH - 20, 120)], fill=(22, 26, 36, 230), outline=(50, 210, 120), width=2)
    draw.text((half_width + 35, 68), "PERSPECTIVE PROJECTION (gluPerspective)", fill=(80, 235, 140), font=font_title)
    draw.text((half_width + 35, 94), "Converging projectors: Natural depth foreshortening (Distant objects appear smaller)", fill=(200, 215, 235), font=font_sub)

    # Right Bottom Feature Card
    draw.rectangle([(half_width + 30, WINDOW_HEIGHT - 70), (WINDOW_WIDTH - 30, WINDOW_HEIGHT - 25)], fill=(18, 36, 26), outline=(40, 180, 90))
    draw.text((half_width + 45, WINDOW_HEIGHT - 60), "Use Cases: Human eye vision, Real-time 3D Games, Virtual Reality Simulations", fill=(130, 250, 170), font=font_tag)

    # Center Pipeline Dividing Strip
    draw.line([(half_width, 48), (half_width, WINDOW_HEIGHT)], fill=(80, 90, 115), width=2)

    image.save(output_path)
    print(f"[SUCCESS] Saved framebuffer to {output_path}")

    assets_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", "assets", "screenshots"))
    if os.path.exists(assets_dir):
        assets_path = os.path.join(assets_dir, "exp10.png")
        image.save(assets_path)
        print(f"[SUCCESS] Mirrored screenshot to {assets_path}")


def save_animated_demo_gif(window, output_gif_path, num_frames=60):
    """Generates an animated GIF capturing real-time dual-projection cube rotation."""
    frames = []
    print(f"[INFO] Generating {num_frames} animation frames for Experiment 10 GIF...")

    for i in range(num_frames):
        angle = i * (360.0 / float(num_frames))
        render_scene(angle)
        glfw.swap_buffers(window)

        frame = capture_frame()
        frame_thumb = frame.resize((600, 323), Image.Resampling.LANCZOS)
        frames.append(frame_thumb)

    if frames:
        frames[0].save(
            output_gif_path,
            save_all=True,
            append_images=frames[1:],
            duration=45,
            loop=0,
            optimize=True
        )
        print(f"[SUCCESS] Saved animated GIF to {output_gif_path}")

        assets_gif_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", "assets", "gifs"))
        if os.path.exists(assets_gif_dir):
            assets_gif_path = os.path.join(assets_gif_dir, "exp10.gif")
            frames[0].save(
                assets_gif_path,
                save_all=True,
                append_images=frames[1:],
                duration=45,
                loop=0,
                optimize=True
            )
            print(f"[SUCCESS] Mirrored animated GIF to {assets_gif_path}")


def main():
    headless = "--headless" in sys.argv or "--save" in sys.argv or "--screenshot" in sys.argv
    save_gif = "--save-gif" in sys.argv or headless

    if not glfw.init():
        raise RuntimeError("GLFW initialization failed")

    glfw.window_hint(glfw.DEPTH_BITS, 24)
    if headless:
        glfw.window_hint(glfw.VISIBLE, glfw.FALSE)

    window = glfw.create_window(WINDOW_WIDTH, WINDOW_HEIGHT, "Experiment 10 - 3D Viewing Pipeline (Likith S G)", None, None)
    if not window:
        glfw.terminate()
        raise RuntimeError("Failed to create GLFW window")

    glfw.make_context_current(window)

    # Initial frame capture
    render_scene(35.0)
    output_png = os.path.join(os.path.dirname(__file__), "output.png")
    add_labels_and_save(output_png)

    if save_gif:
        output_gif = os.path.join(os.path.dirname(__file__), "demo.gif")
        save_animated_demo_gif(window, output_gif, num_frames=50)

    if headless:
        glfw.destroy_window(window)
        glfw.terminate()
        return

    # Interactive Render Loop
    cur_angle = 0.0
    last_time = glfw.get_time()
    while not glfw.window_should_close(window):
        now = glfw.get_time()
        dt = now - last_time
        last_time = now

        cur_angle = (cur_angle + dt * 45.0) % 360.0
        render_scene(cur_angle)
        glfw.swap_buffers(window)
        glfw.poll_events()

    glfw.destroy_window(window)
    glfw.terminate()


if __name__ == "__main__":
    main()
