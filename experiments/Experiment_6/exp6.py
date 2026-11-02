"""
Experiment 6: Z-Buffer Algorithm for Hidden Surface Removal
Student Name: Likith S G
USN: 1RVU23CSE236
Course: Computer Graphics and Virtual Reality Laboratory

Description:
This program implements and demonstrates the Z-Buffer (Depth-Buffer) Algorithm for
hidden surface removal. It presents a side-by-side comparative analysis:
1. Left Viewport: Depth Testing DISABLED (Painter's algorithm failure where draw order causes occlusion artifacts).
2. Right Viewport: Depth Testing ENABLED (Z-buffer correctly resolves visibility per fragment based on depth values).
"""

import sys
import os
import glfw
from OpenGL.GL import *
from OpenGL.GLU import *
from PIL import Image, ImageDraw, ImageFont

WINDOW_WIDTH = 1200
WINDOW_HEIGHT = 700


def draw_overlapping_primitives():
    """
    Renders three overlapping planar triangles at distinct depths:
    1. Red Triangle:   Z = +0.5 (Closest to camera)
    2. Green Triangle: Z = -0.2 (Intermediate depth)
    3. Blue Triangle:  Z = -0.6 (Furthest from camera)
    """
    glBegin(GL_TRIANGLES)

    # 1. Red Triangle (Closest: Z = +0.5) - Drawn FIRST
    glColor3f(0.92, 0.24, 0.24)
    glVertex3f(-0.7, -0.6, 0.5)
    glVertex3f(0.5, -0.6, 0.5)
    glVertex3f(-0.1, 0.7, 0.5)

    # 2. Green Triangle (Intermediate: Z = -0.2) - Drawn SECOND
    glColor3f(0.20, 0.85, 0.38)
    glVertex3f(-0.4, -0.4, -0.2)
    glVertex3f(0.8, -0.4, -0.2)
    glVertex3f(0.3, 0.8, -0.2)

    # 3. Blue Triangle (Furthest: Z = -0.6) - Drawn THIRD
    glColor3f(0.25, 0.52, 0.95)
    glVertex3f(-0.8, 0.2, -0.6)
    glVertex3f(0.2, 0.2, -0.6)
    glVertex3f(-0.3, -0.8, -0.6)

    glEnd()


def draw_viewport_border(w, h):
    """Draws an outline border around the active viewport."""
    glMatrixMode(GL_PROJECTION)
    glPushMatrix()
    glLoadIdentity()
    glOrtho(0, w, 0, h, -1, 1)

    glMatrixMode(GL_MODELVIEW)
    glPushMatrix()
    glLoadIdentity()

    glColor3f(0.28, 0.32, 0.42)
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


def render_scene():
    """Renders dual side-by-side viewports comparing depth test OFF vs ON."""
    glClearColor(0.06, 0.07, 0.10, 1.0)
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)

    half_width = WINDOW_WIDTH // 2

    # ------------------------------------------------------------
    # LEFT VIEWPORT: DEPTH TEST DISABLED
    # ------------------------------------------------------------
    glViewport(0, 0, half_width, WINDOW_HEIGHT)
    glDisable(GL_DEPTH_TEST)

    glMatrixMode(GL_PROJECTION)
    glLoadIdentity()
    glOrtho(-1.3, 1.3, -1.3, 1.3, -2.0, 2.0)
    glMatrixMode(GL_MODELVIEW)
    glLoadIdentity()

    draw_overlapping_primitives()
    draw_viewport_border(half_width, WINDOW_HEIGHT)

    # ------------------------------------------------------------
    # RIGHT VIEWPORT: DEPTH TEST ENABLED (Z-BUFFER)
    # ------------------------------------------------------------
    glViewport(half_width, 0, half_width, WINDOW_HEIGHT)
    glEnable(GL_DEPTH_TEST)
    glDepthFunc(GL_LESS)

    glMatrixMode(GL_PROJECTION)
    glLoadIdentity()
    glOrtho(-1.3, 1.3, -1.3, 1.3, -2.0, 2.0)
    glMatrixMode(GL_MODELVIEW)
    glLoadIdentity()

    draw_overlapping_primitives()
    draw_viewport_border(half_width, WINDOW_HEIGHT)


def add_labels_and_save(output_path):
    """Captures framebuffer and superimposes comprehensive visual annotations."""
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
        font_tag = ImageFont.truetype("C:/Windows/Fonts/segoeuib.ttf", 13)
        font_head = ImageFont.truetype("C:/Windows/Fonts/segoeuib.ttf", 14)
    except Exception:
        font_title = font_sub = font_tag = font_head = ImageFont.load_default()

    half_width = WINDOW_WIDTH // 2

    # Global Top Header
    draw.rectangle([(0, 0), (WINDOW_WIDTH, 45)], fill=(18, 22, 30))
    draw.text((25, 12), "Experiment 6: Z-Buffer Algorithm for Hidden Surface Removal", fill=(240, 245, 255), font=font_title)
    draw.text((half_width + 120, 15), "Student: Likith S G | USN: 1RVU23CSE236", fill=(170, 185, 205), font=font_head)

    # Left Viewport Banner (Depth Test Disabled)
    draw.rectangle([(20, 55), (half_width - 20, 110)], fill=(28, 24, 28), outline=(220, 60, 60), width=2)
    draw.text((35, 62), "Without Z-Buffer (Depth Test DISABLED)", fill=(255, 90, 90), font=font_title)
    draw.text((35, 87), "Naive Painter's failure: draw order determines visibility (Red drawn first)", fill=(200, 200, 200), font=font_sub)

    # Left Bottom Callout Box
    draw.rectangle([(30, WINDOW_HEIGHT - 65), (half_width - 30, WINDOW_HEIGHT - 25)], fill=(38, 20, 20), outline=(190, 50, 50))
    draw.text((45, WINDOW_HEIGHT - 55), "INCORRECT: Green (Z=-0.2) and Blue (Z=-0.6) wrongly occlude Red (Z=+0.5)", fill=(255, 120, 120), font=font_tag)

    # Right Viewport Banner (Depth Test Enabled)
    draw.rectangle([(half_width + 20, 55), (WINDOW_WIDTH - 20, 110)], fill=(20, 30, 24), outline=(50, 200, 100), width=2)
    draw.text((half_width + 35, 62), "With Z-Buffer (Depth Test ENABLED)", fill=(70, 230, 120), font=font_title)
    draw.text((half_width + 35, 87), "Hardware per-pixel depth buffer: if frag_Z < buffer_Z -> update pixel", fill=(200, 200, 200), font=font_sub)

    # Right Bottom Callout Box
    draw.rectangle([(half_width + 30, WINDOW_HEIGHT - 65), (WINDOW_WIDTH - 30, WINDOW_HEIGHT - 25)], fill=(18, 35, 25), outline=(50, 160, 80))
    draw.text((half_width + 45, WINDOW_HEIGHT - 55), "CORRECT: Closer Red triangle (Z=+0.5) stays properly in front of Green & Blue", fill=(120, 255, 160), font=font_tag)

    # Center Divider Line
    draw.line([(half_width, 45), (half_width, WINDOW_HEIGHT)], fill=(80, 90, 110), width=2)

    image.save(output_path)
    print(f"[SUCCESS] Saved framebuffer to {output_path}")

    assets_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", "assets", "screenshots"))
    if os.path.exists(assets_dir):
        assets_path = os.path.join(assets_dir, "exp6.png")
        image.save(assets_path)
        print(f"[SUCCESS] Mirrored screenshot to {assets_path}")


def main():
    headless = "--headless" in sys.argv or "--save" in sys.argv or "--screenshot" in sys.argv

    if not glfw.init():
        raise RuntimeError("GLFW initialization failed")

    glfw.window_hint(glfw.DEPTH_BITS, 24)
    if headless:
        glfw.window_hint(glfw.VISIBLE, glfw.FALSE)

    window = glfw.create_window(WINDOW_WIDTH, WINDOW_HEIGHT, "Experiment 6 - Z-Buffer Algorithm (Likith S G)", None, None)
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
