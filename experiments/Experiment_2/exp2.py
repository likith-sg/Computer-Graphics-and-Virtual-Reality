"""
Experiment 2: Midpoint Circle Drawing Algorithm
Student Name: Likith S G
USN: 1RVU23CSE236
Course: Computer Graphics and Virtual Reality Laboratory

Description:
This program implements the Midpoint Circle Drawing Algorithm using 8-way symmetry
and integer-only decision parameters to rasterize circles efficiently without trigonometry.
"""

import sys
import os
from OpenGL.GL import *
from OpenGL.GLUT import *
from OpenGL.GLU import *
from PIL import Image

WINDOW_WIDTH = 700
WINDOW_HEIGHT = 650


def plot_pixel(x, y):
    """Plots a single pixel at integer coordinates (x, y)."""
    glVertex2i(int(round(x)), int(round(y)))


def plot_8_symmetric_points(xc, yc, x, y):
    """Plots all 8 symmetric points derived from a single octant point (x, y)."""
    plot_pixel(xc + x, yc + y)  # Octant 1
    plot_pixel(xc - x, yc + y)  # Octant 4
    plot_pixel(xc + x, yc - y)  # Octant 8
    plot_pixel(xc - x, yc - y)  # Octant 5
    plot_pixel(xc + y, yc + x)  # Octant 2
    plot_pixel(xc - y, yc + x)  # Octant 3
    plot_pixel(xc + y, yc - x)  # Octant 7
    plot_pixel(xc - y, yc - x)  # Octant 6


def midpoint_circle(xc, yc, radius):
    """
    Midpoint Circle Drawing Algorithm.
    Uses initial decision parameter p = 1 - r and updates iteratively.
    """
    x = 0
    y = radius
    p = 1 - radius

    # Plot initial points at cardinal axes
    plot_8_symmetric_points(xc, yc, x, y)

    while x < y:
        x += 1
        if p < 0:
            # Midpoint is inside the perimeter: choose East (E)
            p += 2 * x + 1
        else:
            # Midpoint is outside or on perimeter: choose South-East (SE)
            y -= 1
            p += 2 * (x - y) + 1

        plot_8_symmetric_points(xc, yc, x, y)


def render_text(x, y, text, font=GLUT_BITMAP_HELVETICA_12):
    """Renders bitmap text at screen coordinate (x, y)."""
    glRasterPos2i(x, y)
    for ch in text:
        glutBitmapCharacter(font, ord(ch))


def draw_decorations(xc, yc, radius):
    """Draws grid, center crosshair, radius line, and symmetry guide lines."""
    # Faint grid lines
    glColor3f(0.14, 0.17, 0.22)
    glLineWidth(1.0)
    glBegin(GL_LINES)
    for x in range(50, WINDOW_WIDTH - 50, 50):
        glVertex2i(x, 50)
        glVertex2i(x, WINDOW_HEIGHT - 70)
    for y in range(50, WINDOW_HEIGHT - 70, 50):
        glVertex2i(50, y)
        glVertex2i(WINDOW_WIDTH - 50, y)
    glEnd()

    # Border frame
    glColor3f(0.3, 0.35, 0.45)
    glLineWidth(2.0)
    glBegin(GL_LINE_LOOP)
    glVertex2i(45, 45)
    glVertex2i(WINDOW_WIDTH - 45, 45)
    glVertex2i(WINDOW_WIDTH - 45, WINDOW_HEIGHT - 65)
    glVertex2i(45, WINDOW_HEIGHT - 65)
    glEnd()

    # Symmetry reference lines through center (dotted feel)
    glColor3f(0.28, 0.34, 0.44)
    glLineWidth(1.5)
    glBegin(GL_LINES)
    # Horizontal axis
    glVertex2i(xc - radius - 40, yc)
    glVertex2i(xc + radius + 40, yc)
    # Vertical axis
    glVertex2i(xc, yc - radius - 40)
    glVertex2i(xc, yc + radius + 40)
    # Diagonal 45 deg lines
    d = int(radius * 0.85)
    glVertex2i(xc - d - 20, yc - d - 20)
    glVertex2i(xc + d + 20, yc + d + 20)
    glVertex2i(xc - d - 20, yc + d + 20)
    glVertex2i(xc + d + 20, yc - d - 20)
    glEnd()

    # Center marker
    glPointSize(6.0)
    glColor3f(1.0, 0.85, 0.2)
    glBegin(GL_POINTS)
    plot_pixel(xc, yc)
    glEnd()

    # Radius vector line (Center to 45 degree point)
    rx = int(xc + radius * 0.7071)
    ry = int(yc + radius * 0.7071)
    glColor3f(1.0, 0.5, 0.2)
    glLineWidth(2.0)
    glBegin(GL_LINES)
    glVertex2i(xc, yc)
    glVertex2i(rx, ry)
    glEnd()


def save_framebuffer(output_path):
    """Reads back the OpenGL framebuffer and saves as an image."""
    glReadBuffer(GL_FRONT)
    glPixelStorei(GL_PACK_ALIGNMENT, 1)
    data = glReadPixels(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT, GL_RGB, GL_UNSIGNED_BYTE)
    image = Image.frombytes("RGB", (WINDOW_WIDTH, WINDOW_HEIGHT), data)
    image = image.transpose(Image.FLIP_TOP_BOTTOM)
    image.save(output_path)
    print(f"[SUCCESS] Saved framebuffer to {output_path}")

    # Mirror to assets/screenshots
    assets_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", "assets", "screenshots"))
    if os.path.exists(assets_dir):
        assets_path = os.path.join(assets_dir, "exp2.png")
        image.save(assets_path)
        print(f"[SUCCESS] Mirrored screenshot to {assets_path}")


def display():
    glClear(GL_COLOR_BUFFER_BIT)

    xc, yc = 350, 310
    primary_radius = 175
    inner_radius = 90

    draw_decorations(xc, yc, primary_radius)

    # Title and Metadata Banner
    glColor3f(0.9, 0.95, 1.0)
    render_text(50, WINDOW_HEIGHT - 30, "Experiment 2: Midpoint Circle Drawing Algorithm", GLUT_BITMAP_HELVETICA_18)
    glColor3f(0.65, 0.72, 0.82)
    render_text(50, WINDOW_HEIGHT - 50, "Student: Likith S G  |  USN: 1RVU23CSE236", GLUT_BITMAP_HELVETICA_12)

    # Draw Primary Circle (Vibrant Spring Green)
    glPointSize(3.5)
    glColor3f(0.2, 0.95, 0.45)
    glBegin(GL_POINTS)
    midpoint_circle(xc, yc, primary_radius)
    glEnd()

    # Draw Inner Concentric Circle (Vibrant Cyan)
    glPointSize(2.8)
    glColor3f(0.25, 0.85, 0.95)
    glBegin(GL_POINTS)
    midpoint_circle(xc, yc, inner_radius)
    glEnd()

    # Annotations and Formula details
    glColor3f(1.0, 0.85, 0.2)
    render_text(xc + 10, yc - 15, f"Center C({xc}, {yc})")

    glColor3f(1.0, 0.5, 0.2)
    render_text(xc + 55, yc + 75, f"Radius R = {primary_radius}")

    glColor3f(0.25, 0.85, 0.95)
    render_text(xc + 15, yc + 105, f"Inner R = {inner_radius}")

    glColor3f(0.85, 0.88, 0.95)
    render_text(60, WINDOW_HEIGHT - 85, "Algorithm: p0 = 1 - r | If p < 0: p += 2x + 1 | Else: y -= 1, p += 2(x - y) + 1")
    render_text(60, WINDOW_HEIGHT - 105, "Exploits 8-way symmetry: compute (x, y) for 1 octant -> map to all 8 octants")

    glColor3f(0.6, 0.65, 0.75)
    render_text(50, 20, "Complexity: O(R) integer additions & bit-shifts  |  Zero square root / trigonometric calculations")

    glFlush()

    if "--headless" in sys.argv or "--save" in sys.argv or "--screenshot" in sys.argv:
        output_file = os.path.join(os.path.dirname(__file__), "output.png")
        save_framebuffer(output_file)
        glutLeaveMainLoop()


def init():
    glClearColor(0.08, 0.09, 0.12, 1.0)
    gluOrtho2D(0, WINDOW_WIDTH, 0, WINDOW_HEIGHT)


def main():
    glutInit()
    glutInitDisplayMode(GLUT_SINGLE | GLUT_RGB)
    glutInitWindowSize(WINDOW_WIDTH, WINDOW_HEIGHT)
    glutCreateWindow(b"Experiment 2 - Midpoint Circle Drawing Algorithm (Likith S G)")

    init()
    glutDisplayFunc(display)
    glutMainLoop()


if __name__ == "__main__":
    main()
