"""
Experiment 3: 2D Transformations Using Homogeneous Coordinates
Student Name: Likith S G
USN: 1RVU23CSE236
Course: Computer Graphics and Virtual Reality Laboratory

Description:
This program implements 2D geometric transformations using 3x3 homogeneous coordinate matrices.
It demonstrates Translation, Rotation, Scaling, Reflection, and Shearing applied to a 2D polygon,
displaying original and transformed geometry side-by-side with matrix equations.
"""

import sys
import os
import math
from OpenGL.GL import *
from OpenGL.GLUT import *
from OpenGL.GLU import *
from PIL import Image

WINDOW_WIDTH = 1200
WINDOW_HEIGHT = 720

# Original test polygon (Triangle) in homogeneous coordinates [x, y, 1]
ORIGINAL_TRIANGLE = [
    [0.0, 0.0, 1.0],
    [80.0, 0.0, 1.0],
    [40.0, 70.0, 1.0]
]


def matrix_mult_3x3(A, B):
    """Multiplies two 3x3 matrices."""
    result = [[0.0 for _ in range(3)] for _ in range(3)]
    for i in range(3):
        for j in range(3):
            for k in range(3):
                result[i][j] += A[i][k] * B[k][j]
    return result


def transform_points(matrix, points):
    """Applies a 3x3 transformation matrix to a list of [x, y, 1] vertices."""
    transformed = []
    for pt in points:
        # [x', y', 1]^T = M * [x, y, 1]^T
        nx = matrix[0][0] * pt[0] + matrix[0][1] * pt[1] + matrix[0][2] * pt[2]
        ny = matrix[1][0] * pt[0] + matrix[1][1] * pt[1] + matrix[1][2] * pt[2]
        nw = matrix[2][0] * pt[0] + matrix[2][1] * pt[1] + matrix[2][2] * pt[2]
        transformed.append([nx / nw, ny / nw, 1.0])
    return transformed


def get_translation_matrix(tx, ty):
    return [
        [1.0, 0.0, tx],
        [0.0, 1.0, ty],
        [0.0, 0.0, 1.0]
    ]


def get_rotation_matrix(angle_deg):
    theta = math.radians(angle_deg)
    c = math.cos(theta)
    s = math.sin(theta)
    return [
        [c, -s, 0.0],
        [s,  c, 0.0],
        [0.0, 0.0, 1.0]
    ]


def get_scaling_matrix(sx, sy):
    return [
        [sx,  0.0, 0.0],
        [0.0, sy,  0.0],
        [0.0, 0.0, 1.0]
    ]


def get_reflection_x_matrix():
    return [
        [1.0,  0.0, 0.0],
        [0.0, -1.0, 0.0],
        [0.0,  0.0, 1.0]
    ]


def get_shearing_matrix(shx, shy):
    return [
        [1.0, shx, 0.0],
        [shy, 1.0, 0.0],
        [0.0, 0.0, 1.0]
    ]


def render_text(x, y, text, font=GLUT_BITMAP_HELVETICA_12):
    """Renders bitmap text at screen coordinates (x, y)."""
    glRasterPos2i(int(x), int(y))
    for ch in text:
        glutBitmapCharacter(font, ord(ch))


def draw_panel(px, py, pw, ph, title, subtitle, orig_pts, trans_pts, trans_color, matrix_str):
    """Draws a dedicated panel showing coordinate grid, original and transformed shapes, and matrix."""
    # Panel background & border
    glColor3f(0.11, 0.13, 0.17)
    glBegin(GL_QUADS)
    glVertex2i(px, py)
    glVertex2i(px + pw, py)
    glVertex2i(px + pw, py + ph)
    glVertex2i(px, py + ph)
    glEnd()

    glColor3f(0.24, 0.28, 0.38)
    glLineWidth(1.5)
    glBegin(GL_LINE_LOOP)
    glVertex2i(px, py)
    glVertex2i(px + pw, py)
    glVertex2i(px + pw, py + ph)
    glVertex2i(px, py + ph)
    glEnd()

    # Panel Title & Parameter Subtitle
    glColor3f(0.95, 0.95, 1.0)
    render_text(px + 15, py + ph - 24, title, GLUT_BITMAP_HELVETICA_18)
    glColor3f(*trans_color)
    render_text(px + 15, py + ph - 42, subtitle, GLUT_BITMAP_HELVETICA_12)

    # Local origin inside the panel
    ox = px + 120
    oy = py + 110

    # Local axes
    glColor3f(0.32, 0.36, 0.46)
    glLineWidth(1.0)
    glBegin(GL_LINES)
    glVertex2i(px + 15, oy); glVertex2i(px + pw - 15, oy) # X axis
    glVertex2i(ox, py + 15); glVertex2i(ox, py + ph - 55) # Y axis
    glEnd()

    render_text(px + pw - 25, oy - 15, "+X", GLUT_BITMAP_HELVETICA_10)
    render_text(ox + 5, py + ph - 65, "+Y", GLUT_BITMAP_HELVETICA_10)
    render_text(ox - 15, oy - 15, "(0,0)", GLUT_BITMAP_HELVETICA_10)

    # 1. Original Shape (Faint Gray dotted line)
    glColor3f(0.55, 0.58, 0.65)
    glLineWidth(1.5)
    glBegin(GL_LINE_LOOP)
    for pt in orig_pts:
        glVertex2i(int(ox + pt[0]), int(oy + pt[1]))
    glEnd()

    # 2. Transformed Shape (Thick Vibrant Line)
    glColor3f(*trans_color)
    glLineWidth(2.5)
    glBegin(GL_LINE_LOOP)
    for pt in trans_pts:
        glVertex2i(int(ox + pt[0]), int(oy + pt[1]))
    glEnd()

    # Transformed Vertices
    glPointSize(5.0)
    glBegin(GL_POINTS)
    for pt in trans_pts:
        glVertex2i(int(ox + pt[0]), int(oy + pt[1]))
    glEnd()

    # Matrix formula block
    glColor3f(0.75, 0.80, 0.90)
    my = py + ph - 70
    for line in matrix_str:
        render_text(px + pw - 145, my, line, GLUT_BITMAP_8_BY_13)
        my -= 16


def save_framebuffer(output_path):
    """Reads back the OpenGL framebuffer and saves as an image."""
    glReadBuffer(GL_FRONT)
    glPixelStorei(GL_PACK_ALIGNMENT, 1)
    data = glReadPixels(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT, GL_RGB, GL_UNSIGNED_BYTE)
    image = Image.frombytes("RGB", (WINDOW_WIDTH, WINDOW_HEIGHT), data)
    image = image.transpose(Image.FLIP_TOP_BOTTOM)
    image.save(output_path)
    print(f"[SUCCESS] Saved framebuffer to {output_path}")

    assets_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", "assets", "screenshots"))
    if os.path.exists(assets_dir):
        assets_path = os.path.join(assets_dir, "exp3.png")
        image.save(assets_path)
        print(f"[SUCCESS] Mirrored screenshot to {assets_path}")


def display():
    glClear(GL_COLOR_BUFFER_BIT)

    # Header Banner
    glColor3f(0.95, 0.98, 1.0)
    render_text(30, WINDOW_HEIGHT - 30, "Experiment 3: 2D Transformations Using Homogeneous Coordinates", GLUT_BITMAP_HELVETICA_18)
    glColor3f(0.65, 0.72, 0.85)
    render_text(30, WINDOW_HEIGHT - 50, "Student: Likith S G  |  USN: 1RVU23CSE236  |  Gray: Original  |  Colored: Transformed", GLUT_BITMAP_HELVETICA_12)

    # Panel dimensions: 3 columns x 2 rows
    margin_x = 25
    margin_y = 20
    header_space = 75
    usable_h = WINDOW_HEIGHT - header_space - margin_y
    pw = (WINDOW_WIDTH - 4 * margin_x) // 3
    ph = (usable_h - margin_y) // 2

    # Transformations definitions
    # 1. Translation
    T = get_translation_matrix(50, 40)
    pts_trans = transform_points(T, ORIGINAL_TRIANGLE)
    t_mat = ["[ 1  0  50 ]", "[ 0  1  40 ]", "[ 0  0   1 ]"]
    draw_panel(margin_x, margin_y + ph + margin_y, pw, ph, "1. Translation", "tx = 50, ty = 40", ORIGINAL_TRIANGLE, pts_trans, (0.2, 0.9, 0.4), t_mat)

    # 2. Rotation
    R = get_rotation_matrix(45)
    pts_rot = transform_points(R, ORIGINAL_TRIANGLE)
    r_mat = ["[ cos  -sin  0 ]", "[ sin   cos  0 ]", "[  0     0   1 ]"]
    draw_panel(margin_x * 2 + pw, margin_y + ph + margin_y, pw, ph, "2. Rotation", "angle = +45 deg (CCW)", ORIGINAL_TRIANGLE, pts_rot, (1.0, 0.4, 0.4), r_mat)

    # 3. Scaling
    S = get_scaling_matrix(1.5, 0.7)
    pts_scale = transform_points(S, ORIGINAL_TRIANGLE)
    s_mat = ["[ 1.5   0   0 ]", "[  0   0.7  0 ]", "[  0    0   1 ]"]
    draw_panel(margin_x * 3 + pw * 2, margin_y + ph + margin_y, pw, ph, "3. Scaling", "sx = 1.5, sy = 0.7", ORIGINAL_TRIANGLE, pts_scale, (0.3, 0.75, 1.0), s_mat)

    # 4. Reflection (about X-axis, offset in Y so visible)
    RefX = get_reflection_x_matrix()
    pts_ref = transform_points(RefX, ORIGINAL_TRIANGLE)
    ref_mat = ["[ 1   0  0 ]", "[ 0  -1  0 ]", "[ 0   0  1 ]"]
    draw_panel(margin_x, margin_y, pw, ph, "4. Reflection", "About X-Axis (y' = -y)", ORIGINAL_TRIANGLE, pts_ref, (1.0, 0.8, 0.2), ref_mat)

    # 5. Shearing
    Sh = get_shearing_matrix(0.5, 0.0)
    pts_shear = transform_points(Sh, ORIGINAL_TRIANGLE)
    sh_mat = ["[ 1  0.5  0 ]", "[ 0   1   0 ]", "[ 0   0   1 ]"]
    draw_panel(margin_x * 2 + pw, margin_y, pw, ph, "5. Shearing", "shx = 0.5, shy = 0.0", ORIGINAL_TRIANGLE, pts_shear, (0.9, 0.4, 1.0), sh_mat)

    # 6. Theory & Homogeneous Coordinates Summary
    px = margin_x * 3 + pw * 2
    py = margin_y
    glColor3f(0.11, 0.13, 0.17)
    glBegin(GL_QUADS)
    glVertex2i(px, py); glVertex2i(px + pw, py); glVertex2i(px + pw, py + ph); glVertex2i(px, py + ph)
    glEnd()
    glColor3f(0.24, 0.28, 0.38)
    glLineWidth(1.5)
    glBegin(GL_LINE_LOOP)
    glVertex2i(px, py); glVertex2i(px + pw, py); glVertex2i(px + pw, py + ph); glVertex2i(px, py + ph)
    glEnd()

    glColor3f(0.95, 0.95, 1.0)
    render_text(px + 15, py + ph - 24, "Homogeneous Coordinates", GLUT_BITMAP_HELVETICA_18)
    glColor3f(0.7, 0.75, 0.85)
    render_text(px + 15, py + ph - 50, "- Point representation: [x, y, 1]^T", GLUT_BITMAP_HELVETICA_12)
    render_text(px + 15, py + ph - 70, "- Unified 3x3 matrix framework", GLUT_BITMAP_HELVETICA_12)
    render_text(px + 15, py + ph - 90, "- Enables matrix composability:", GLUT_BITMAP_HELVETICA_12)
    glColor3f(0.3, 0.9, 0.5)
    render_text(px + 30, py + ph - 110, "P' = M * P", GLUT_BITMAP_HELVETICA_12)
    glColor3f(0.7, 0.75, 0.85)
    render_text(px + 15, py + ph - 135, "- Invertible affine operations", GLUT_BITMAP_HELVETICA_12)
    render_text(px + 15, py + ph - 155, "- Eliminates special-case translation", GLUT_BITMAP_HELVETICA_12)
    glColor3f(1.0, 0.85, 0.2)
    render_text(px + 15, py + 25, "Likith S G (1RVU23CSE236)", GLUT_BITMAP_HELVETICA_12)

    glFlush()

    if "--headless" in sys.argv or "--save" in sys.argv or "--screenshot" in sys.argv:
        output_file = os.path.join(os.path.dirname(__file__), "output.png")
        save_framebuffer(output_file)
        glutLeaveMainLoop()


def init():
    glClearColor(0.06, 0.07, 0.10, 1.0)
    gluOrtho2D(0, WINDOW_WIDTH, 0, WINDOW_HEIGHT)


def main():
    glutInit()
    glutInitDisplayMode(GLUT_SINGLE | GLUT_RGB)
    glutInitWindowSize(WINDOW_WIDTH, WINDOW_HEIGHT)
    glutCreateWindow(b"Experiment 3 - 2D Transformations (Likith S G)")

    init()
    glutDisplayFunc(display)
    glutMainLoop()


if __name__ == "__main__":
    main()
