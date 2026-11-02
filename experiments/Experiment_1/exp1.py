"""
Experiment 1: DDA and Bresenham Line Drawing Algorithms
Student Name: Likith S G
USN: 1RVU23CSE236
Course: Computer Graphics and Virtual Reality Laboratory

Description:
This program implements and compares two fundamental line rasterization algorithms:
1. Digital Differential Analyzer (DDA) - using floating-point increments.
2. Bresenham's Line Algorithm - using efficient integer-only decision parameters.
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


def dda_line(x1, y1, x2, y2):
    """
    Digital Differential Analyzer (DDA) Line Drawing Algorithm.
    Computes increments based on the maximum coordinate differential.
    """
    dx = x2 - x1
    dy = y2 - y1

    steps = int(max(abs(dx), abs(dy)))
    if steps == 0:
        plot_pixel(x1, y1)
        return

    x_inc = dx / float(steps)
    y_inc = dy / float(steps)

    x = float(x1)
    y = float(y1)

    for _ in range(steps + 1):
        plot_pixel(x, y)
        x += x_inc
        y += y_inc


def bresenham_line(x1, y1, x2, y2):
    """
    Bresenham's Integer Line Drawing Algorithm.
    Operates strictly with integer arithmetic, handling all octants.
    """
    dx = abs(x2 - x1)
    dy = abs(y2 - y1)

    sx = 1 if x1 < x2 else -1
    sy = 1 if y1 < y2 else -1

    err = dx - dy

    curr_x, curr_y = x1, y1

    while True:
        plot_pixel(curr_x, curr_y)
        if curr_x == x2 and curr_y == y2:
            break

        e2 = 2 * err
        if e2 > -dy:
            err -= dy
            curr_x += sx
        if e2 < dx:
            err += dx
            curr_y += sy


def render_text(x, y, text, font=GLUT_BITMAP_HELVETICA_12):
    """Renders bitmap text at screen coordinate (x, y)."""
    glRasterPos2i(x, y)
    for ch in text:
        glutBitmapCharacter(font, ord(ch))


def draw_grid_and_axes():
    """Draws coordinate grid, axes, and reference annotations."""
    # Faint grid lines
    glColor3f(0.15, 0.18, 0.22)
    glLineWidth(1.0)
    glBegin(GL_LINES)
    for x in range(50, WINDOW_WIDTH - 50, 50):
        glVertex2i(x, 50)
        glVertex2i(x, WINDOW_HEIGHT - 70)
    for y in range(50, WINDOW_HEIGHT - 70, 50):
        glVertex2i(50, y)
        glVertex2i(WINDOW_WIDTH - 50, y)
    glEnd()

    # Outer border
    glColor3f(0.3, 0.35, 0.45)
    glLineWidth(2.0)
    glBegin(GL_LINE_LOOP)
    glVertex2i(45, 45)
    glVertex2i(WINDOW_WIDTH - 45, 45)
    glVertex2i(WINDOW_WIDTH - 45, WINDOW_HEIGHT - 65)
    glVertex2i(45, WINDOW_HEIGHT - 65)
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

    # Also mirror into assets/screenshots
    assets_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", "assets", "screenshots"))
    if os.path.exists(assets_dir):
        assets_path = os.path.join(assets_dir, "exp1.png")
        image.save(assets_path)
        print(f"[SUCCESS] Mirrored screenshot to {assets_path}")


def display():
    glClear(GL_COLOR_BUFFER_BIT)

    draw_grid_and_axes()

    # Title and Metadata Banner
    glColor3f(0.9, 0.95, 1.0)
    render_text(50, WINDOW_HEIGHT - 30, "Experiment 1: DDA and Bresenham Line Drawing Algorithms", GLUT_BITMAP_HELVETICA_18)
    glColor3f(0.65, 0.72, 0.82)
    render_text(50, WINDOW_HEIGHT - 50, "Student: Likith S G  |  USN: 1RVU23CSE236", GLUT_BITMAP_HELVETICA_12)

    # 1. DDA Line (Vibrant Coral Red)
    glPointSize(3.5)
    glBegin(GL_POINTS)
    glColor3f(1.0, 0.3, 0.3)
    dda_line(80, 130, 580, 500)
    glEnd()

    # 2. Bresenham Line (Vibrant Emerald Green)
    glBegin(GL_POINTS)
    glColor3f(0.2, 0.95, 0.45)
    bresenham_line(80, 520, 580, 110)
    glEnd()

    # Legends & Information Box
    glColor3f(1.0, 0.3, 0.3)
    render_text(85, WINDOW_HEIGHT - 95, "[DDA Line] (80, 130) -> (580, 500) | Red Points (dx/dy floating steps)")
    glColor3f(0.2, 0.95, 0.45)
    render_text(85, WINDOW_HEIGHT - 115, "[Bresenham Line] (80, 520) -> (580, 110) | Green Points (integer error decision)")

    glColor3f(0.7, 0.7, 0.7)
    render_text(50, 20, "DDA Formula: steps = max(|dx|, |dy|), x += dx/steps, y += dy/steps")
    render_text(400, 20, "Bresenham: err = dx - dy, e2 = 2*err, integer loop")

    glFlush()

    # Check for automated headless or screenshot mode
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
    glutCreateWindow(b"Experiment 1 - DDA & Bresenham Line Drawing (Likith S G)")

    init()
    glutDisplayFunc(display)

    if "--headless" in sys.argv or "--save" in sys.argv:
        glutMainLoop()
    else:
        glutMainLoop()


if __name__ == "__main__":
    main()
