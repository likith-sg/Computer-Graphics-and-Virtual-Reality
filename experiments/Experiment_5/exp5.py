"""
Experiment 5: Cohen-Sutherland Line Clipping Algorithm
Student Name: Likith S G
USN: 1RVU23CSE236
Course: Computer Graphics and Virtual Reality Laboratory

Description:
This program implements the classic Cohen-Sutherland 2D Line Clipping Algorithm.
It divides 2D space into 9 regions using 4-bit outcodes [TOP, BOTTOM, RIGHT, LEFT],
rapidly identifying trivially accepted and rejected lines, and iteratively calculating
exact intersection coordinates with rectangular window boundaries.
"""

import sys
import os
from OpenGL.GL import *
from OpenGL.GLUT import *
from OpenGL.GLU import *
from PIL import Image

WINDOW_WIDTH = 1250
WINDOW_HEIGHT = 720

# Local Window Clipping Boundaries inside panels
X_MIN = 80.0
X_MAX = 280.0
Y_MIN = 80.0
Y_MAX = 280.0

# 4-bit Region Codes (Outcodes)
INSIDE = 0  # 0000
LEFT   = 1  # 0001
RIGHT  = 2  # 0010
BOTTOM = 4  # 0100
TOP    = 8  # 1000

# Diverse set of test lines covering all algorithm branches:
# [x1, y1, x2, y2, label]
TEST_LINES = [
    [100, 120, 260, 240, "Line 1: Trivial Accept (Inside)"],
    [20, 310, 150, 350,  "Line 2: Trivial Reject (Top)"],
    [310, 50, 360, 220,  "Line 3: Trivial Reject (Right)"],
    [40, 60, 310, 290,   "Line 4: Clips Left & Top/Right"],
    [180, 40, 180, 320,  "Line 5: Vertical Clipping"],
    [50, 190, 320, 190,  "Line 6: Horizontal Clipping"],
    [210, 220, 330, 340, "Line 7: Partial Clip (Right/Top)"]
]


def compute_outcode(x, y, xmin, ymin, xmax, ymax):
    """Computes the 4-bit region code for coordinate point (x, y)."""
    code = INSIDE
    if x < xmin:
        code |= LEFT
    elif x > xmax:
        code |= RIGHT

    if y < ymin:
        code |= BOTTOM
    elif y > ymax:
        code |= TOP

    return code


def cohen_sutherland_clip(x1, y1, x2, y2, xmin, ymin, xmax, ymax):
    """
    Performs Cohen-Sutherland line clipping against [xmin, ymin, xmax, ymax].
    Returns (accepted, (cx1, cy1, cx2, cy2)).
    """
    code1 = compute_outcode(x1, y1, xmin, ymin, xmax, ymax)
    code2 = compute_outcode(x2, y2, xmin, ymin, xmax, ymax)

    cx1, cy1 = float(x1), float(y1)
    cx2, cy2 = float(x2), float(y2)

    while True:
        # Trivial Accept: Both endpoints inside window (0000 & 0000)
        if code1 == 0 and code2 == 0:
            return True, (cx1, cy1, cx2, cy2)

        # Trivial Reject: Both endpoints share an outside half-plane
        if (code1 & code2) != 0:
            return False, None

        # At least one endpoint is outside: select it
        code_out = code1 if code1 != 0 else code2

        x, y = 0.0, 0.0

        # Calculate intersection with respective boundary line
        if code_out & TOP:
            x = cx1 + (cx2 - cx1) * (ymax - cy1) / (cy2 - cy1)
            y = ymax
        elif code_out & BOTTOM:
            x = cx1 + (cx2 - cx1) * (ymin - cy1) / (cy2 - cy1)
            y = ymin
        elif code_out & RIGHT:
            y = cy1 + (cy2 - cy1) * (xmax - cx1) / (cx2 - cx1)
            x = xmax
        elif code_out & LEFT:
            y = cy1 + (cy2 - cy1) * (xmin - cx1) / (cx2 - cx1)
            x = xmin

        # Replace outside endpoint with computed intersection point
        if code_out == code1:
            cx1, cy1 = x, y
            code1 = compute_outcode(cx1, cy1, xmin, ymin, xmax, ymax)
        else:
            cx2, cy2 = x, y
            code2 = compute_outcode(cx2, cy2, xmin, ymin, xmax, ymax)


def render_text(x, y, text, font=GLUT_BITMAP_HELVETICA_12):
    """Renders bitmap text at screen coordinates (x, y)."""
    glRasterPos2i(int(x), int(y))
    for ch in text:
        glutBitmapCharacter(font, ord(ch))


def draw_panel_box(x, y, w, h, title, subtitle):
    """Draws background panel container."""
    glColor3f(0.10, 0.12, 0.16)
    glBegin(GL_QUADS)
    glVertex2i(x, y); glVertex2i(x + w, y); glVertex2i(x + w, y + h); glVertex2i(x, y + h)
    glEnd()

    glColor3f(0.24, 0.28, 0.38)
    glLineWidth(1.5)
    glBegin(GL_LINE_LOOP)
    glVertex2i(x, y); glVertex2i(x + w, y); glVertex2i(x + w, y + h); glVertex2i(x, y + h)
    glEnd()

    glColor3f(0.95, 0.95, 1.0)
    render_text(x + 15, y + h - 25, title, GLUT_BITMAP_HELVETICA_18)
    glColor3f(0.65, 0.72, 0.85)
    render_text(x + 15, y + h - 45, subtitle, GLUT_BITMAP_HELVETICA_12)


def draw_clipping_window(ox, oy, xmin, ymin, xmax, ymax):
    """Draws the rectangular clipping window boundary with soft interior fill."""
    # Window Fill
    glColor4f(0.15, 0.22, 0.32, 0.35)
    glBegin(GL_QUADS)
    glVertex2i(int(ox + xmin), int(oy + ymin))
    glVertex2i(int(ox + xmax), int(oy + ymin))
    glVertex2i(int(ox + xmax), int(oy + ymax))
    glVertex2i(int(ox + xmin), int(oy + ymax))
    glEnd()

    # Window Outline (Cyan)
    glColor3f(0.25, 0.75, 1.0)
    glLineWidth(2.0)
    glBegin(GL_LINE_LOOP)
    glVertex2i(int(ox + xmin), int(oy + ymin))
    glVertex2i(int(ox + xmax), int(oy + ymin))
    glVertex2i(int(ox + xmax), int(oy + ymax))
    glVertex2i(int(ox + xmin), int(oy + ymax))
    glEnd()

    # Labels
    glColor3f(0.4, 0.8, 1.0)
    render_text(int(ox + xmin + 8), int(oy + ymin + 10), f"({int(xmin)}, {int(ymin)})", GLUT_BITMAP_HELVETICA_10)
    render_text(int(ox + xmax - 75), int(oy + ymax - 18), f"({int(xmax)}, {int(ymax)})", GLUT_BITMAP_HELVETICA_10)


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
        assets_path = os.path.join(assets_dir, "exp5.png")
        image.save(assets_path)
        print(f"[SUCCESS] Mirrored screenshot to {assets_path}")


def display():
    glClear(GL_COLOR_BUFFER_BIT)

    # Header Banner
    glColor3f(0.95, 0.98, 1.0)
    render_text(35, WINDOW_HEIGHT - 30, "Experiment 5: Cohen-Sutherland Line Clipping Algorithm", GLUT_BITMAP_HELVETICA_18)
    glColor3f(0.65, 0.72, 0.85)
    render_text(35, WINDOW_HEIGHT - 50, "Student: Likith S G  |  USN: 1RVU23CSE236  |  4-Bit Region Outcode Window Clipping", GLUT_BITMAP_HELVETICA_12)

    pw, ph = 370, 580
    margin = 25
    base_y = 50

    # --------------------------------------------------------
    # Panel 1: Original Lines (Before Clipping)
    # --------------------------------------------------------
    p1_x = margin
    draw_panel_box(p1_x, base_y, pw, ph, "1. Before Clipping", "Unprocessed input line segments")
    ox1 = p1_x + 10
    oy1 = base_y + 110
    draw_clipping_window(ox1, oy1, X_MIN, Y_MIN, X_MAX, Y_MAX)

    # Draw all original test lines in coral red
    glLineWidth(2.2)
    for line in TEST_LINES:
        x1, y1, x2, y2, _ = line
        glColor3f(1.0, 0.35, 0.35)
        glBegin(GL_LINES)
        glVertex2i(int(ox1 + x1), int(oy1 + y1))
        glVertex2i(int(ox1 + x2), int(oy1 + y2))
        glEnd()

        # Endpoints
        glPointSize(5.0)
        glColor3f(1.0, 0.85, 0.2)
        glBegin(GL_POINTS)
        glVertex2i(int(ox1 + x1), int(oy1 + y1))
        glVertex2i(int(ox1 + x2), int(oy1 + y2))
        glEnd()

    # Panel 1 Legend
    glColor3f(0.8, 0.85, 0.9)
    render_text(p1_x + 15, base_y + 60, "Red Lines: Original candidate segments", GLUT_BITMAP_HELVETICA_10)
    render_text(p1_x + 15, base_y + 40, "Cyan Box: Rectangular clipping viewport [80, 280]^2", GLUT_BITMAP_HELVETICA_10)
    render_text(p1_x + 15, base_y + 20, "Total Test Lines: 7 lines with varying slopes", GLUT_BITMAP_HELVETICA_10)

    # --------------------------------------------------------
    # Panel 2: Clipped Lines (After Clipping)
    # --------------------------------------------------------
    p2_x = margin * 2 + pw
    draw_panel_box(p2_x, base_y, pw, ph, "2. After Clipping", "Retained visible segments inside window")
    ox2 = p2_x + 10
    oy2 = base_y + 110
    draw_clipping_window(ox2, oy2, X_MIN, Y_MIN, X_MAX, Y_MAX)

    # First draw discarded fragments as faint dashed/gray lines
    glLineWidth(1.0)
    glColor3f(0.3, 0.35, 0.42)
    for line in TEST_LINES:
        x1, y1, x2, y2, _ = line
        glBegin(GL_LINES)
        glVertex2i(int(ox2 + x1), int(oy2 + y1))
        glVertex2i(int(ox2 + x2), int(oy2 + y2))
        glEnd()

    # Draw clipped segments in bright Emerald Green
    glLineWidth(3.0)
    for line in TEST_LINES:
        x1, y1, x2, y2, _ = line
        accepted, clipped_coords = cohen_sutherland_clip(x1, y1, x2, y2, X_MIN, Y_MIN, X_MAX, Y_MAX)
        if accepted and clipped_coords:
            cx1, cy1, cx2, cy2 = clipped_coords
            glColor3f(0.2, 0.95, 0.45)
            glBegin(GL_LINES)
            glVertex2i(int(ox2 + cx1), int(oy2 + cy1))
            glVertex2i(int(ox2 + cx2), int(oy2 + cy2))
            glEnd()

            # Highlight clipped boundary intersection endpoints
            glPointSize(6.0)
            glColor3f(0.2, 1.0, 0.8)
            glBegin(GL_POINTS)
            glVertex2i(int(ox2 + cx1), int(oy2 + cy1))
            glVertex2i(int(ox2 + cx2), int(oy2 + cy2))
            glEnd()

    # Panel 2 Legend
    glColor3f(0.2, 0.95, 0.45)
    render_text(p2_x + 15, base_y + 60, "Green Lines: Successfully clipped visible segments", GLUT_BITMAP_HELVETICA_10)
    glColor3f(0.45, 0.5, 0.6)
    render_text(p2_x + 15, base_y + 40, "Faint Gray: Discarded outside segments (culled)", GLUT_BITMAP_HELVETICA_10)
    glColor3f(0.2, 1.0, 0.8)
    render_text(p2_x + 15, base_y + 20, "Cyan Dots: Exact boundary intersection points", GLUT_BITMAP_HELVETICA_10)

    # --------------------------------------------------------
    # Panel 3: 4-Bit Region Outcodes Diagram & Logic
    # --------------------------------------------------------
    p3_x = margin * 3 + pw * 2
    draw_panel_box(p3_x, base_y, pw, ph, "3. Outcode Architecture", "9-Region classification & bitwise logic")

    # Draw 3x3 Outcode Grid
    gx, gy = p3_x + 40, base_y + 350
    cw, ch = 90, 45

    outcodes_matrix = [
        ["1001 (TL)", "1000 (T)", "1010 (TR)"],
        ["0001 (L)",  "0000 (IN)", "0010 (R)"],
        ["0101 (BL)", "0100 (B)", "0110 (BR)"]
    ]

    for r in range(3):
        for c in range(3):
            cell_x = gx + c * cw
            cell_y = gy - r * ch
            # Center region is highlighted
            if r == 1 and c == 1:
                glColor3f(0.18, 0.28, 0.38)
            else:
                glColor3f(0.13, 0.16, 0.22)
            glBegin(GL_QUADS)
            glVertex2i(cell_x, cell_y); glVertex2i(cell_x + cw, cell_y)
            glVertex2i(cell_x + cw, cell_y + ch); glVertex2i(cell_x, cell_y + ch)
            glEnd()

            glColor3f(0.35, 0.42, 0.55)
            glLineWidth(1.0)
            glBegin(GL_LINE_LOOP)
            glVertex2i(cell_x, cell_y); glVertex2i(cell_x + cw, cell_y)
            glVertex2i(cell_x + cw, cell_y + ch); glVertex2i(cell_x, cell_y + ch)
            glEnd()

            if r == 1 and c == 1:
                glColor3f(0.3, 0.95, 0.5)
            else:
                glColor3f(0.85, 0.88, 0.95)
            render_text(cell_x + 15, cell_y + 16, outcodes_matrix[r][c], GLUT_BITMAP_HELVETICA_12)

    # Outcode Legend & Rules
    tx = p3_x + 20
    ty = base_y + 195
    glColor3f(0.95, 0.95, 1.0)
    render_text(tx, ty, "Algorithmic Decision Rules:", GLUT_BITMAP_HELVETICA_12)
    glColor3f(0.3, 0.95, 0.5)
    render_text(tx + 10, ty - 22, "- Trivial Accept: (code1 == 0 and code2 == 0)", GLUT_BITMAP_HELVETICA_10)
    glColor3f(1.0, 0.4, 0.4)
    render_text(tx + 10, ty - 42, "- Trivial Reject: (code1 & code2) != 0", GLUT_BITMAP_HELVETICA_10)
    glColor3f(0.7, 0.75, 0.85)
    render_text(tx + 10, ty - 62, "- Clip Intersection Equations:", GLUT_BITMAP_HELVETICA_10)
    render_text(tx + 20, ty - 80, "x = x1 + (x2 - x1) * (Y_bound - y1) / (y2 - y1)", GLUT_BITMAP_HELVETICA_10)
    render_text(tx + 20, ty - 98, "y = y1 + (y2 - y1) * (X_bound - x1) / (x2 - x1)", GLUT_BITMAP_HELVETICA_10)

    glColor3f(1.0, 0.85, 0.2)
    render_text(tx, base_y + 25, "Student: Likith S G (1RVU23CSE236)", GLUT_BITMAP_HELVETICA_12)

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
    glutCreateWindow(b"Experiment 5 - Cohen-Sutherland Line Clipping (Likith S G)")

    init()
    glutDisplayFunc(display)
    glutMainLoop()


if __name__ == "__main__":
    main()
