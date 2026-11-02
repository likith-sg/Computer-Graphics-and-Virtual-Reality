"""
Experiment 4: Composite 2D Transformations Using Matrix Representation
Student Name: Likith S G
USN: 1RVU23CSE236
Course: Computer Graphics and Virtual Reality Laboratory

Description:
This program implements composite 2D geometric transformations using homogeneous coordinate matrices.
It demonstrates the sequential transformation pipeline (Scale -> Rotate -> Translate) computed via
matrix multiplication M = T x R x S, highlighting matrix non-commutativity and intermediate states.
"""

import sys
import os
import math
from OpenGL.GL import *
from OpenGL.GLUT import *
from OpenGL.GLU import *
from PIL import Image

WINDOW_WIDTH = 1250
WINDOW_HEIGHT = 720

# Original test polygon (Triangle)
ORIGINAL_TRIANGLE = [
    [0.0, 0.0, 1.0],
    [70.0, 0.0, 1.0],
    [35.0, 60.0, 1.0]
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
        nx = matrix[0][0] * pt[0] + matrix[0][1] * pt[1] + matrix[0][2] * pt[2]
        ny = matrix[1][0] * pt[0] + matrix[1][1] * pt[1] + matrix[1][2] * pt[2]
        nw = matrix[2][0] * pt[0] + matrix[2][1] * pt[1] + matrix[2][2] * pt[2]
        transformed.append([nx / nw, ny / nw, 1.0])
    return transformed


def get_translation_matrix(tx, ty):
    return [
        [1.0, 0.0, float(tx)],
        [0.0, 1.0, float(ty)],
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
        [float(sx), 0.0,       0.0],
        [0.0,       float(sy), 0.0],
        [0.0,       0.0,       1.0]
    ]


def render_text(x, y, text, font=GLUT_BITMAP_HELVETICA_12):
    """Renders bitmap text at screen coordinates (x, y)."""
    glRasterPos2i(int(x), int(y))
    for ch in text:
        glutBitmapCharacter(font, ord(ch))


def draw_axes_and_grid(ox, oy, w, h):
    """Draws grid lines and axes around a local origin."""
    # Faint grid
    glColor3f(0.15, 0.18, 0.23)
    glLineWidth(1.0)
    glBegin(GL_LINES)
    for x in range(int(ox - w // 2), int(ox + w // 2), 40):
        glVertex2i(x, int(oy - h // 2))
        glVertex2i(x, int(oy + h // 2))
    for y in range(int(oy - h // 2), int(oy + h // 2), 40):
        glVertex2i(int(ox - w // 2), y)
        glVertex2i(int(ox + w // 2), y)
    glEnd()

    # Main Axes
    glColor3f(0.35, 0.40, 0.50)
    glLineWidth(1.5)
    glBegin(GL_LINES)
    glVertex2i(int(ox - w // 2), int(oy))
    glVertex2i(int(ox + w // 2), int(oy))
    glVertex2i(int(ox), int(oy - h // 2))
    glVertex2i(int(ox), int(oy + h // 2))
    glEnd()

    render_text(int(ox + w // 2 - 25), int(oy - 15), "+X", GLUT_BITMAP_HELVETICA_10)
    render_text(int(ox + 5), int(oy + h // 2 - 15), "+Y", GLUT_BITMAP_HELVETICA_10)
    render_text(int(ox - 15), int(oy - 15), "(0,0)", GLUT_BITMAP_HELVETICA_10)


def draw_polygon(points, color, line_width=2.5, filled=False):
    """Draws a 2D polygon with vertices."""
    if filled:
        glColor4f(color[0], color[1], color[2], 0.25)
        glBegin(GL_TRIANGLES)
        for pt in points:
            glVertex2i(int(pt[0]), int(pt[1]))
        glEnd()

    glColor3f(*color)
    glLineWidth(line_width)
    glBegin(GL_LINE_LOOP)
    for pt in points:
        glVertex2i(int(pt[0]), int(pt[1]))
    glEnd()

    glPointSize(5.0)
    glBegin(GL_POINTS)
    for pt in points:
        glVertex2i(int(pt[0]), int(pt[1]))
    glEnd()


def draw_matrix_box(x, y, title, mat, color):
    """Draws a stylized box showing a 3x3 matrix."""
    w, h = 260, 125
    glColor3f(0.12, 0.14, 0.18)
    glBegin(GL_QUADS)
    glVertex2i(x, y)
    glVertex2i(x + w, y)
    glVertex2i(x + w, y + h)
    glVertex2i(x, y + h)
    glEnd()

    glColor3f(0.25, 0.30, 0.40)
    glLineWidth(1.0)
    glBegin(GL_LINE_LOOP)
    glVertex2i(x, y)
    glVertex2i(x + w, y)
    glVertex2i(x + w, y + h)
    glVertex2i(x, y + h)
    glEnd()

    glColor3f(*color)
    render_text(x + 12, y + h - 20, title, GLUT_BITMAP_HELVETICA_12)

    glColor3f(0.85, 0.88, 0.95)
    for r in range(3):
        row_str = f"| {mat[r][0]:6.2f}  {mat[r][1]:6.2f}  {mat[r][2]:6.2f} |"
        render_text(x + 15, y + h - 45 - r * 22, row_str, GLUT_BITMAP_8_BY_13)


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
        assets_path = os.path.join(assets_dir, "exp4.png")
        image.save(assets_path)
        print(f"[SUCCESS] Mirrored screenshot to {assets_path}")


def display():
    glClear(GL_COLOR_BUFFER_BIT)

    # Header Banner
    glColor3f(0.95, 0.98, 1.0)
    render_text(35, WINDOW_HEIGHT - 30, "Experiment 4: Composite 2D Transformations Using Matrix Representation", GLUT_BITMAP_HELVETICA_18)
    glColor3f(0.65, 0.72, 0.85)
    render_text(35, WINDOW_HEIGHT - 50, "Student: Likith S G  |  USN: 1RVU23CSE236  |  Pipeline: Scale -> Rotate -> Translate (M = T x R x S)", GLUT_BITMAP_HELVETICA_12)

    # Transformation Parameters
    sx, sy = 1.6, 1.6
    rot_angle = 45.0
    tx, ty = 140.0, 100.0

    # 1. Compute Individual Transformation Matrices
    S = get_scaling_matrix(sx, sy)
    R = get_rotation_matrix(rot_angle)
    T = get_translation_matrix(tx, ty)

    # 2. Composite Matrix Computation: M = T x (R x S)
    RS = matrix_mult_3x3(R, S)
    M = matrix_mult_3x3(T, RS)

    # 3. Intermediate Geometric States (offset to center viewport)
    ox, oy = 320, 310
    viewport_w, viewport_h = 560, 520

    # Draw Stage Outline and Local Coordinate System
    glColor3f(0.09, 0.11, 0.15)
    glBegin(GL_QUADS)
    glVertex2i(ox - viewport_w // 2, oy - viewport_h // 2)
    glVertex2i(ox + viewport_w // 2, oy - viewport_h // 2)
    glVertex2i(ox + viewport_w // 2, oy + viewport_h // 2)
    glVertex2i(ox - viewport_w // 2, oy + viewport_h // 2)
    glEnd()

    glColor3f(0.25, 0.30, 0.42)
    glLineWidth(1.5)
    glBegin(GL_LINE_LOOP)
    glVertex2i(ox - viewport_w // 2, oy - viewport_h // 2)
    glVertex2i(ox + viewport_w // 2, oy - viewport_h // 2)
    glVertex2i(ox + viewport_w // 2, oy + viewport_h // 2)
    glVertex2i(ox - viewport_w // 2, oy + viewport_h // 2)
    glEnd()

    draw_axes_and_grid(ox, oy, viewport_w, viewport_h)

    # A. Original Shape at Origin (Gray)
    pts_orig = [[ox + p[0], oy + p[1]] for p in ORIGINAL_TRIANGLE]
    draw_polygon(pts_orig, (0.55, 0.58, 0.65), line_width=1.5)

    # B. Step 1: Scaled Geometry (Cyan)
    scaled_raw = transform_points(S, ORIGINAL_TRIANGLE)
    pts_scaled = [[ox + p[0], oy + p[1]] for p in scaled_raw]
    draw_polygon(pts_scaled, (0.2, 0.8, 1.0), line_width=1.5)

    # C. Step 2: Scaled + Rotated Geometry (Yellow/Orange)
    rot_raw = transform_points(RS, ORIGINAL_TRIANGLE)
    pts_rotated = [[ox + p[0], oy + p[1]] for p in rot_raw]
    draw_polygon(pts_rotated, (1.0, 0.75, 0.2), line_width=1.5)

    # D. Step 3: Final Composite Transformed Geometry (Vibrant Red-Coral)
    comp_raw = transform_points(M, ORIGINAL_TRIANGLE)
    pts_comp = [[ox + p[0], oy + p[1]] for p in comp_raw]
    draw_polygon(pts_comp, (1.0, 0.28, 0.28), line_width=3.0)

    # Viewport Legend
    render_text(ox - viewport_w // 2 + 15, oy + viewport_h // 2 - 25, "Visual Pipeline Progression:", GLUT_BITMAP_HELVETICA_12)
    glColor3f(0.55, 0.58, 0.65); render_text(ox - viewport_w // 2 + 20, oy + viewport_h // 2 - 45, "[1] Gray: Original Triangle", GLUT_BITMAP_HELVETICA_10)
    glColor3f(0.2, 0.8, 1.0);    render_text(ox - viewport_w // 2 + 20, oy + viewport_h // 2 - 65, "[2] Cyan: Scaled (sx=1.6, sy=1.6)", GLUT_BITMAP_HELVETICA_10)
    glColor3f(1.0, 0.75, 0.2);   render_text(ox - viewport_w // 2 + 20, oy + viewport_h // 2 - 85, "[3] Yellow: Rotated (+45 deg)", GLUT_BITMAP_HELVETICA_10)
    glColor3f(1.0, 0.28, 0.28);  render_text(ox - viewport_w // 2 + 20, oy + viewport_h // 2 - 105, "[4] Red: Final Composite (T x R x S)", GLUT_BITMAP_HELVETICA_10)

    # 4. Matrix Display Panel on the Right Side
    rx = 640
    col1 = rx
    col2 = rx + 280

    draw_matrix_box(col1, 440, "1. Scaling Matrix S (1.6x, 1.6x)", S, (0.2, 0.8, 1.0))
    draw_matrix_box(col2, 440, "2. Rotation Matrix R (+45 deg)", R, (1.0, 0.75, 0.2))
    draw_matrix_box(col1, 290, "3. Translation Matrix T (140, 100)", T, (0.4, 0.9, 0.5))

    # Composite Matrix Box (Highlighted)
    draw_matrix_box(col2, 290, "Composite Matrix M = T x R x S", M, (1.0, 0.35, 0.35))

    # Explanation and Theory Box at Bottom Right
    bx, by, bw, bh = rx, 50, 560, 215
    glColor3f(0.11, 0.13, 0.17)
    glBegin(GL_QUADS)
    glVertex2i(bx, by); glVertex2i(bx + bw, by); glVertex2i(bx + bw, by + bh); glVertex2i(bx, by + bh)
    glEnd()

    glColor3f(0.25, 0.30, 0.42)
    glLineWidth(1.5)
    glBegin(GL_LINE_LOOP)
    glVertex2i(bx, by); glVertex2i(bx + bw, by); glVertex2i(bx + bw, by + bh); glVertex2i(bx, by + bh)
    glEnd()

    glColor3f(0.95, 0.95, 1.0)
    render_text(bx + 15, by + bh - 24, "Mathematical Formulation & Analysis", GLUT_BITMAP_HELVETICA_18)
    glColor3f(0.7, 0.75, 0.85)
    render_text(bx + 15, by + bh - 50, "- Matrix Order Matters: Matrix multiplication is non-commutative.", GLUT_BITMAP_HELVETICA_12)
    glColor3f(1.0, 0.4, 0.4)
    render_text(bx + 30, by + bh - 70, "T x R x S != S x R x T", GLUT_BITMAP_HELVETICA_12)
    glColor3f(0.7, 0.75, 0.85)
    render_text(bx + 15, by + bh - 95, "- Right-to-Left Evaluation: The rightmost matrix applies first to point P:", GLUT_BITMAP_HELVETICA_12)
    glColor3f(0.3, 0.9, 0.5)
    render_text(bx + 30, by + bh - 115, "P' = M * P = T * (R * (S * P))", GLUT_BITMAP_HELVETICA_12)
    glColor3f(0.7, 0.75, 0.85)
    render_text(bx + 15, by + bh - 140, "- Single Concatenated Matrix: Any sequence of N affine transformations", GLUT_BITMAP_HELVETICA_12)
    render_text(bx + 15, by + bh - 160, "  collapses into a single 3x3 matrix, achieving O(1) transform cost per vertex.", GLUT_BITMAP_HELVETICA_12)

    glColor3f(1.0, 0.85, 0.2)
    render_text(bx + 15, by + 18, "Laboratory Submission - Likith S G (1RVU23CSE236)", GLUT_BITMAP_HELVETICA_12)

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
    glutCreateWindow(b"Experiment 4 - Composite 2D Transformations (Likith S G)")

    init()
    glutDisplayFunc(display)
    glutMainLoop()


if __name__ == "__main__":
    main()
