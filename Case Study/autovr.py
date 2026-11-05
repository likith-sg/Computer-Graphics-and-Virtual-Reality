"""
AutoVerse — 3D Interactive Automotive Showroom & Customizer
Course: CS4104 - Computer Graphics & Virtual Reality
Student Name: Likith S G (USN: 1RVU23CSE236)

Standalone interactive Python 3D viewer implementing the full AutoVerse showroom:
- 8 Distinct Vehicle Models (Sports Coupe, Sedan, Hatchback, SUV, Formula Racer, Police, Taxi, Golf Cart)
- 18 Automotive Paint Colors across 4 Finishes (Gloss, Metallic, Matte, Satin)
- 5 Alloy Wheel Finishes & 5 Brembo Brake Caliper Colors
- 6 Window Tinting Levels & 6 Neon Underglow Ground Bounce Colors
- 4 Real-time Studio Lighting Presets (Luxury Studio, Midnight, Bright Gallery, Neon Garage)
- 360° Luxury Architectural Showroom with Obsidian Floor & LED Turntable
"""

import sys
import os
import math
import argparse
import numpy as np
from PIL import Image

try:
    import glfw
    from OpenGL.GL import *
    from OpenGL.GLU import *
except ImportError:
    print("[AutoVerse] Error: PyOpenGL and glfw are required.")
    sys.exit(1)

# -------------------------------------------------------------
# Color Palettes & Vehicle Configurations
# -------------------------------------------------------------
PAINT_COLORS = [
    ("Pearl White",    (0.96, 0.96, 0.98)),
    ("Midnight Black", (0.08, 0.08, 0.09)),
    ("Graphite",       (0.25, 0.27, 0.30)),
    ("Arctic Silver",  (0.82, 0.84, 0.86)),
    ("Racing Red",     (0.85, 0.12, 0.12)),
    ("Crimson",        (0.55, 0.00, 0.09)),
    ("Sunset Orange",  (1.00, 0.36, 0.12)),
    ("Solar Yellow",   (1.00, 0.80, 0.00)),
    ("Royal Blue",     (0.08, 0.30, 0.80)),
    ("Sky Blue",       (0.22, 0.72, 1.00)),
    ("Emerald Green",  (0.00, 0.65, 0.40)),
    ("Forest Green",   (0.12, 0.32, 0.18)),
    ("Violet",         (0.45, 0.08, 0.72)),
    ("Rose Gold",      (0.75, 0.45, 0.50)),
    ("Champagne",      (0.97, 0.91, 0.81)),
    ("Bronze",         (0.58, 0.38, 0.20)),
    ("Teal",           (0.00, 0.55, 0.55)),
    ("Candy Pink",     (0.95, 0.15, 0.45))
]

FINISHES = [
    ("Gloss",    0.08, 0.90, 80.0),   # metallic, smoothness, shininess
    ("Metallic", 0.90, 0.85, 100.0),
    ("Matte",    0.00, 0.15, 10.0),
    ("Satin",    0.45, 0.50, 40.0)
]

WHEEL_FINISHES = [
    ("Stealth Black",  (0.12, 0.12, 0.12)),
    ("Polished Silver",(0.85, 0.85, 0.88)),
    ("Gunmetal",       (0.35, 0.36, 0.38)),
    ("Liquid Gold",    (0.88, 0.75, 0.25)),
    ("Burnt Bronze",   (0.60, 0.42, 0.22))
]

WINDOW_TINTS = [
    ("Clear Glass",    (0.20, 0.25, 0.30, 0.35)),
    ("Light Smoke",    (0.15, 0.18, 0.22, 0.55)),
    ("Dark Charcoal",  (0.08, 0.10, 0.12, 0.82)),
    ("Limo Blackout",  (0.03, 0.03, 0.04, 0.96)),
    ("Sunset Gold",    (0.85, 0.65, 0.20, 0.60)),
    ("Cyber Blue",     (0.10, 0.50, 0.90, 0.65))
]

NEON_COLORS = [
    ("Electric Cyan",  (0.00, 0.95, 1.00)),
    ("Neon Magenta",   (1.00, 0.05, 0.70)),
    ("Acid Green",     (0.20, 1.00, 0.10)),
    ("Amber Gold",     (1.00, 0.65, 0.05)),
    ("Ice White",      (0.85, 0.95, 1.00)),
    ("Royal Violet",   (0.65, 0.15, 1.00))
]

STUDIO_LIGHTING = [
    {
        "name": "Bengaluru Studio",
        "key_col": (1.00, 0.94, 0.85), "key_pos": (6.0, 7.5, 6.0),
        "fill_col": (0.35, 0.40, 0.55), "fill_pos": (-6.0, 5.0, -5.0),
        "ambient": (0.16, 0.17, 0.20),
        "accent": (1.00, 0.85, 0.60)
    },
    {
        "name": "Detroit Proving Grounds",
        "key_col": (0.40, 0.80, 1.00), "key_pos": (6.0, 7.5, 6.0),
        "fill_col": (0.20, 0.25, 0.45), "fill_pos": (-6.0, 5.0, -5.0),
        "ambient": (0.08, 0.10, 0.16),
        "accent": (0.00, 0.90, 1.00)
    },
    {
        "name": "CAD Inspection Pavilion",
        "key_col": (1.00, 1.00, 1.00), "key_pos": (5.0, 8.5, 5.0),
        "fill_col": (0.60, 0.62, 0.68), "fill_pos": (-5.0, 6.0, -5.0),
        "ambient": (0.25, 0.26, 0.28),
        "accent": (0.90, 0.90, 0.95)
    },
    {
        "name": "Cyberpunk Prototyping Bay",
        "key_col": (1.00, 0.10, 0.65), "key_pos": (6.0, 6.5, 5.0),
        "fill_col": (0.00, 0.85, 1.00), "fill_pos": (-6.0, 5.0, -5.0),
        "ambient": (0.10, 0.08, 0.15),
        "accent": (1.00, 0.00, 0.80)
    }
]

VEHICLE_NAMES = [
    ("SPORTS COUPE",       "car_sports"),
    ("EXECUTIVE SEDAN",    "car_sedan"),
    ("COMPACT HATCH",      "car_hatchback"),
    ("URBAN SUV",          "car_suv"),
    ("GT FORMULA RACER",   "race_car"),
    ("POLICE INTERCEPTOR", "car_police"),
    ("CITY TAXI",          "car_taxi"),
    ("LUXURY GOLF CART",   "golf_cart")
]


# -------------------------------------------------------------
# Primitive 3D Geometry Utilities
# -------------------------------------------------------------
def draw_box(x_size, y_size, z_size):
    """Draw a centered rectangular cuboid with proper normals."""
    hx, hy, hz = x_size * 0.5, y_size * 0.5, z_size * 0.5
    glBegin(GL_QUADS)
    # Front (+Z)
    glNormal3f(0.0, 0.0, 1.0)
    glVertex3f(-hx, -hy,  hz)
    glVertex3f( hx, -hy,  hz)
    glVertex3f( hx,  hy,  hz)
    glVertex3f(-hx,  hy,  hz)
    # Back (-Z)
    glNormal3f(0.0, 0.0, -1.0)
    glVertex3f( hx, -hy, -hz)
    glVertex3f(-hx, -hy, -hz)
    glVertex3f(-hx,  hy, -hz)
    glVertex3f( hx,  hy, -hz)
    # Top (+Y)
    glNormal3f(0.0, 1.0, 0.0)
    glVertex3f(-hx,  hy,  hz)
    glVertex3f( hx,  hy,  hz)
    glVertex3f( hx,  hy, -hz)
    glVertex3f(-hx,  hy, -hz)
    # Bottom (-Y)
    glNormal3f(0.0, -1.0, 0.0)
    glVertex3f(-hx, -hy, -hz)
    glVertex3f( hx, -hy, -hz)
    glVertex3f( hx, -hy,  hz)
    glVertex3f(-hx, -hy,  hz)
    # Right (+X)
    glNormal3f(1.0, 0.0, 0.0)
    glVertex3f( hx, -hy,  hz)
    glVertex3f( hx, -hy, -hz)
    glVertex3f( hx,  hy, -hz)
    glVertex3f( hx,  hy,  hz)
    # Left (-X)
    glNormal3f(-1.0, 0.0, 0.0)
    glVertex3f(-hx, -hy, -hz)
    glVertex3f(-hx, -hy,  hz)
    glVertex3f(-hx,  hy,  hz)
    glVertex3f(-hx,  hy, -hz)
    glEnd()


def draw_cylinder(radius, height, segments=24):
    """Draw a vertical cylinder centered on Y axis."""
    hh = height * 0.5
    step = 2.0 * math.pi / segments
    # Side quads
    glBegin(GL_QUAD_STRIP)
    for i in range(segments + 1):
        angle = i * step
        nx, nz = math.cos(angle), math.sin(angle)
        glNormal3f(nx, 0.0, nz)
        glVertex3f(radius * nx,  hh, radius * nz)
        glVertex3f(radius * nx, -hh, radius * nz)
    glEnd()
    # Top disk
    glBegin(GL_TRIANGLE_FAN)
    glNormal3f(0.0, 1.0, 0.0)
    glVertex3f(0.0, hh, 0.0)
    for i in range(segments + 1):
        angle = i * step
        glVertex3f(radius * math.cos(angle), hh, radius * math.sin(angle))
    glEnd()
    # Bottom disk
    glBegin(GL_TRIANGLE_FAN)
    glNormal3f(0.0, -1.0, 0.0)
    glVertex3f(0.0, -hh, 0.0)
    for i in range(segments + 1):
        angle = -i * step
        glVertex3f(radius * math.cos(angle), -hh, radius * math.sin(angle))
    glEnd()


def draw_wheel(rim_color):
    """Draw a styled automotive wheel with tire, alloy rim, and brake caliper."""
    glPushMatrix()
    glRotatef(90.0, 0.0, 0.0, 1.0)
    # 1. Rubber Tire (dark charcoal)
    glColor3f(0.08, 0.08, 0.09)
    draw_cylinder(0.34, 0.24, segments=20)
    # 2. Alloy Rim Face
    glPushMatrix()
    glTranslatef(0.0, 0.11, 0.0)
    glColor3fv(rim_color)
    draw_cylinder(0.24, 0.03, segments=16)
    # Rim Spokes
    for spk in range(5):
        glPushMatrix()
        glRotatef(spk * 72.0, 0.0, 1.0, 0.0)
        draw_box(0.04, 0.02, 0.20)
        glPopMatrix()
    # Center Hub Cap
    glColor3f(0.05, 0.05, 0.05)
    draw_cylinder(0.07, 0.04, segments=12)
    glPopMatrix()
    # 3. Brembo Racing Red Brake Caliper
    glPushMatrix()
    glTranslatef(0.0, 0.06, 0.12)
    glColor3f(0.88, 0.08, 0.08)
    draw_box(0.08, 0.05, 0.12)
    glPopMatrix()
    glPopMatrix()


# -------------------------------------------------------------
# Main AutoVerse 3D Viewer Class
# -------------------------------------------------------------
class AutoVerseViewer:
    def __init__(self, width=1280, height=720, offscreen=False):
        self.width = width
        self.height = height
        self.offscreen = offscreen

        # Interaction & Animation state
        self.car_idx = 0
        self.color_idx = 4       # Racing Red default
        self.finish_idx = 1      # Metallic default
        self.wheel_idx = 1       # Polished Silver
        self.tint_idx = 2        # Dark Charcoal
        self.neon_idx = 0        # Electric Cyan
        self.neon_active = True
        self.light_idx = 0       # Luxury Studio
        self.turntable_active = True
        self.yaw = 28.0
        self.pitch = 18.0
        self.distance = 5.6
        self.car_rot = 0.0
        self.show_help = True

        # Mouse tracking
        self.mouse_down = False
        self.last_mouse_x = 0
        self.last_mouse_y = 0

        # Initialize GLFW & Window
        if not glfw.init():
            raise RuntimeError("Failed to initialize GLFW")

        # Always create a real OpenGL context window (positioned offscreen if headless)
        glfw.window_hint(glfw.VISIBLE, glfw.TRUE)
        glfw.window_hint(glfw.SAMPLES, 4)
        glfw.window_hint(glfw.RESIZABLE, glfw.TRUE)

        self.window = glfw.create_window(
            self.width, self.height,
            "AutoVerse — 3D Luxury Automotive Showroom & Customizer (CS4104 Case Study 2)",
            None, None
        )
        if not self.window:
            glfw.terminate()
            raise RuntimeError("Failed to create GLFW window")

        if self.offscreen:
            glfw.set_window_pos(self.window, -3000, -3000)

        glfw.make_context_current(self.window)

        if not self.offscreen:
            glfw.set_key_callback(self.window, self.key_callback)
            glfw.set_mouse_button_callback(self.window, self.mouse_button_callback)
            glfw.set_cursor_pos_callback(self.window, self.cursor_pos_callback)
            glfw.set_scroll_callback(self.window, self.scroll_callback)
            glfw.set_framebuffer_size_callback(self.window, self.resize_callback)

        self.init_gl()

    def init_gl(self):
        glEnable(GL_DEPTH_TEST)
        glDepthFunc(GL_LEQUAL)
        glEnable(GL_LIGHTING)
        glEnable(GL_LIGHT0)
        glEnable(GL_LIGHT1)
        glEnable(GL_COLOR_MATERIAL)
        glColorMaterial(GL_FRONT_AND_BACK, GL_AMBIENT_AND_DIFFUSE)
        glEnable(GL_NORMALIZE)
        glEnable(GL_BLEND)
        glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA)
        glShadeModel(GL_SMOOTH)
        glClearColor(0.04, 0.05, 0.07, 1.0)

    def resize_callback(self, window, w, h):
        if h == 0: h = 1
        self.width = w
        self.height = h
        glViewport(0, 0, w, h)

    def key_callback(self, window, key, scancode, action, mods):
        if action != glfw.PRESS and action != glfw.REPEAT:
            return

        if key == glfw.KEY_ESCAPE:
            glfw.set_window_should_close(window, True)
        elif glfw.KEY_0 <= key <= glfw.KEY_7:
            self.car_idx = key - glfw.KEY_0
        elif key == glfw.KEY_C:
            self.color_idx = (self.color_idx + 1) % len(PAINT_COLORS)
        elif key == glfw.KEY_F:
            self.finish_idx = (self.finish_idx + 1) % len(FINISHES)
        elif key == glfw.KEY_W:
            self.wheel_idx = (self.wheel_idx + 1) % len(WHEEL_FINISHES)
        elif key == glfw.KEY_T:
            self.tint_idx = (self.tint_idx + 1) % len(WINDOW_TINTS)
        elif key == glfw.KEY_U:
            self.neon_active = not self.neon_active
        elif key == glfw.KEY_N:
            self.neon_idx = (self.neon_idx + 1) % len(NEON_COLORS)
        elif glfw.KEY_1 <= key <= glfw.KEY_4:
            self.light_idx = key - glfw.KEY_1
        elif key == glfw.KEY_SPACE:
            self.turntable_active = not self.turntable_active
        elif key == glfw.KEY_R:
            self.color_idx = 4
            self.finish_idx = 1
            self.wheel_idx = 1
            self.car_rot = 0.0
            self.yaw = 28.0
            self.pitch = 18.0
            self.distance = 5.6
        elif key == glfw.KEY_H:
            self.show_help = not self.show_help

    def mouse_button_callback(self, window, button, action, mods):
        if button == glfw.MOUSE_BUTTON_LEFT:
            if action == glfw.PRESS:
                self.mouse_down = True
                self.last_mouse_x, self.last_mouse_y = glfw.get_cursor_pos(window)
            elif action == glfw.RELEASE:
                self.mouse_down = False

    def cursor_pos_callback(self, window, xpos, ypos):
        if self.mouse_down:
            dx = xpos - self.last_mouse_x
            dy = ypos - self.last_mouse_y
            self.yaw += dx * 0.45
            self.pitch = max(5.0, min(80.0, self.pitch + dy * 0.35))
            self.last_mouse_x = xpos
            self.last_mouse_y = ypos

    def scroll_callback(self, window, xoffset, yoffset):
        self.distance = max(3.0, min(10.0, self.distance - yoffset * 0.35))

    def setup_lighting(self):
        style = STUDIO_LIGHTING[self.light_idx]
        amb = style["ambient"]
        glLightModelfv(GL_LIGHT_MODEL_AMBIENT, (amb[0], amb[1], amb[2], 1.0))

        # Key Light
        kp = style["key_pos"]
        kc = style["key_col"]
        glLightfv(GL_LIGHT0, GL_POSITION, (kp[0], kp[1], kp[2], 1.0))
        glLightfv(GL_LIGHT0, GL_DIFFUSE, (kc[0], kc[1], kc[2], 1.0))
        glLightfv(GL_LIGHT0, GL_SPECULAR, (kc[0] * 1.2, kc[1] * 1.2, kc[2] * 1.2, 1.0))

        # Fill Light
        fp = style["fill_pos"]
        fc = style["fill_col"]
        glLightfv(GL_LIGHT1, GL_POSITION, (fp[0], fp[1], fp[2], 1.0))
        glLightfv(GL_LIGHT1, GL_DIFFUSE, (fc[0], fc[1], fc[2], 1.0))
        glLightfv(GL_LIGHT1, GL_SPECULAR, (fc[0] * 0.5, fc[1] * 0.5, fc[2] * 0.5, 1.0))

    def draw_showroom_environment(self):
        """Render the 360-degree luxury architectural automotive studio."""
        glDisable(GL_LIGHTING)

        # 1. Seamless Polished Obsidian Floor (dark reflective plane)
        glBegin(GL_QUADS)
        glColor3f(0.04, 0.05, 0.07)
        glVertex3f(-14.0, 0.0, -14.0)
        glVertex3f( 14.0, 0.0, -14.0)
        glVertex3f( 14.0, 0.0,  14.0)
        glVertex3f(-14.0, 0.0,  14.0)
        glEnd()

        # Floor Grid Accents (subtle architectural grid lines)
        glColor4f(0.12, 0.16, 0.22, 0.35)
        glBegin(GL_LINES)
        for g in range(-12, 13, 2):
            glVertex3f(float(g), 0.001, -12.0)
            glVertex3f(float(g), 0.001,  12.0)
            glVertex3f(-12.0, 0.001, float(g))
            glVertex3f( 12.0, 0.001, float(g))
        glEnd()

        # 2. Elevated Turntable Base (radius 3.6m)
        glPushMatrix()
        glTranslatef(0.0, 0.02, 0.0)
        glColor3f(0.09, 0.10, 0.13)
        draw_cylinder(3.6, 0.04, segments=48)

        # Ambient Dual LED Halo Rings around turntable perimeter
        neon_c = NEON_COLORS[self.neon_idx][1] if self.neon_active else (0.0, 0.8, 1.0)
        # Inner Ring
        glColor4f(neon_c[0], neon_c[1], neon_c[2], 0.75)
        glLineWidth(3.0)
        glBegin(GL_LINE_LOOP)
        for i in range(48):
            a = i * (2.0 * math.pi / 48)
            glVertex3f(3.55 * math.cos(a), 0.025, 3.55 * math.sin(a))
        glEnd()
        # Outer Ring
        glColor4f(neon_c[0] * 0.5, neon_c[1] * 0.5, neon_c[2] * 0.5, 0.45)
        glLineWidth(1.5)
        glBegin(GL_LINE_LOOP)
        for i in range(48):
            a = i * (2.0 * math.pi / 48)
            glVertex3f(3.75 * math.cos(a), 0.025, 3.75 * math.sin(a))
        glEnd()
        glLineWidth(1.0)
        glPopMatrix()

        # 3. Backlit Architectural Studio Wall & Logo Plaque
        glEnable(GL_LIGHTING)
        glPushMatrix()
        glTranslatef(0.0, 3.5, -8.0)
        # Dark Acoustic Wood Wall
        glColor3f(0.10, 0.11, 0.13)
        draw_box(16.0, 7.0, 0.3)

        # Backlit "AUTOVERSE" Emblem Center Bar
        glDisable(GL_LIGHTING)
        glTranslatef(0.0, 1.2, 0.18)
        glColor3f(0.0, 0.85, 1.0)
        draw_box(4.2, 0.65, 0.05)
        # Side illuminated architectural accent columns
        for col_x in (-6.5, -4.5, 4.5, 6.5):
            glPushMatrix()
            glTranslatef(col_x, -1.2, 0.18)
            glColor4f(0.0, 0.9, 1.0, 0.65)
            draw_box(0.15, 6.0, 0.05)
            glPopMatrix()
        glPopMatrix()

        # 4. Overhead Studio Softbox Luminaire (Canopy)
        glPushMatrix()
        glTranslatef(0.0, 6.5, 0.0)
        glColor3f(0.95, 0.98, 1.00)
        draw_box(6.0, 0.12, 4.5)
        # Softbox Dark Frame
        glColor3f(0.12, 0.12, 0.14)
        draw_box(6.2, 0.15, 4.7)
        glPopMatrix()

        glEnable(GL_LIGHTING)

    def draw_vehicle(self):
        """Render the currently selected 3D vehicle model with all custom materials."""
        car_name, car_id = VEHICLE_NAMES[self.car_idx]
        base_color = PAINT_COLORS[self.color_idx][1]
        finish_name, metallic, smoothness, shininess = FINISHES[self.finish_idx]
        wheel_col = WHEEL_FINISHES[self.wheel_idx][1]
        tint_col = WINDOW_TINTS[self.tint_idx][1]
        neon_col = NEON_COLORS[self.neon_idx][1]

        # Material specular setup
        spec_int = 0.9 if finish_name in ("Gloss", "Metallic") else 0.1
        glMaterialfv(GL_FRONT_AND_BACK, GL_SPECULAR, (spec_int, spec_int, spec_int, 1.0))
        glMaterialf(GL_FRONT_AND_BACK, GL_SHININESS, shininess)

        glPushMatrix()
        # Elevate slightly on top of turntable base
        glTranslatef(0.0, 0.38, 0.0)
        glRotatef(self.car_rot, 0.0, 1.0, 0.0)

        # ---------------------------------------------------------
        # Neon Underglow (Down-facing ground illumination)
        # ---------------------------------------------------------
        if self.neon_active:
            glDisable(GL_LIGHTING)
            glColor4f(neon_col[0], neon_col[1], neon_col[2], 0.70)
            glPushMatrix()
            glTranslatef(0.0, -0.32, 0.0)
            # Underglow Glow Disk on Ground
            glBegin(GL_TRIANGLE_FAN)
            glVertex3f(0.0, 0.01, 0.0)
            for i in range(33):
                a = i * (2.0 * math.pi / 32)
                glVertex3f(1.8 * math.cos(a), 0.01, 1.2 * math.sin(a))
            glEnd()
            # Underglow Chassis Tubes
            glColor3fv(neon_col)
            glPushMatrix()
            glTranslatef(-0.85, 0.05, 0.0)
            draw_box(0.06, 0.06, 2.6)
            glTranslatef(1.70, 0.0, 0.0)
            draw_box(0.06, 0.06, 2.6)
            glPopMatrix()
            glPopMatrix()
            glEnable(GL_LIGHTING)

        # ---------------------------------------------------------
        # Vehicle Body Geometry (Differentiated by car_idx)
        # ---------------------------------------------------------
        glColor3fv(base_color)

        if self.car_idx == 0:
            # 1. SPORTS COUPE (Low-slung, wedge front, fastback rear)
            draw_box(1.75, 0.45, 3.8) # Lower main hull
            glPushMatrix()
            glTranslatef(0.0, 0.38, -0.15)
            draw_box(1.35, 0.40, 1.9) # Aerodynamic cabin
            glPopMatrix()
            # Front Hood Slope
            glPushMatrix()
            glTranslatef(0.0, 0.15, 1.4)
            glRotatef(-10.0, 1.0, 0.0, 0.0)
            draw_box(1.65, 0.18, 1.1)
            glPopMatrix()
            # Rear Downforce Wing / Spoiler
            glPushMatrix()
            glTranslatef(0.0, 0.55, -1.75)
            glColor3f(0.12, 0.12, 0.14)
            draw_box(1.60, 0.05, 0.30)
            # Uprights
            glTranslatef(-0.6, -0.15, 0.0)
            draw_box(0.05, 0.30, 0.15)
            glTranslatef(1.2, 0.0, 0.0)
            draw_box(0.05, 0.30, 0.15)
            glPopMatrix()

        elif self.car_idx == 1:
            # 2. EXECUTIVE SEDAN (Classic 3-box luxury proportions)
            draw_box(1.80, 0.50, 4.2) # Lower hull
            glPushMatrix()
            glTranslatef(0.0, 0.46, -0.10)
            draw_box(1.42, 0.46, 2.3) # Full passenger cabin
            glPopMatrix()
            # Trunk Lid & Hood Definition
            glPushMatrix()
            glTranslatef(0.0, 0.22, 1.5)
            draw_box(1.72, 0.12, 1.1)
            glTranslatef(0.0, 0.0, -3.1)
            draw_box(1.72, 0.12, 0.9)
            glPopMatrix()

        elif self.car_idx == 2:
            # 3. COMPACT HATCHBACK (Short rear overhang, tall roof)
            draw_box(1.68, 0.50, 3.4) # Lower hull
            glPushMatrix()
            glTranslatef(0.0, 0.46, -0.30)
            draw_box(1.40, 0.48, 2.0) # Upright cabin
            glPopMatrix()
            # Slanted hatch rear
            glPushMatrix()
            glTranslatef(0.0, 0.35, -1.45)
            glRotatef(25.0, 1.0, 0.0, 0.0)
            draw_box(1.36, 0.40, 0.4)
            glPopMatrix()

        elif self.car_idx == 3:
            # 4. URBAN SUV (High ride height, roof rack, muscular stance)
            draw_box(1.92, 0.65, 4.3) # Robust lower hull
            glPushMatrix()
            glTranslatef(0.0, 0.62, -0.20)
            draw_box(1.55, 0.60, 2.6) # Tall SUV cabin
            glPopMatrix()
            # Roof Rails
            glColor3f(0.18, 0.18, 0.20)
            for side in (-0.72, 0.72):
                glPushMatrix()
                glTranslatef(side, 0.95, -0.20)
                draw_box(0.06, 0.05, 2.4)
                glPopMatrix()

        elif self.car_idx == 4:
            # 5. GT FORMULA RACER (Open-wheel, monocoque, high front/rear wings)
            draw_box(0.95, 0.35, 4.4) # Monocoque needle
            # Sidepods
            glPushMatrix()
            glTranslatef(0.0, 0.05, -0.2)
            draw_box(1.65, 0.30, 1.6)
            glPopMatrix()
            # Massive Front Wing
            glPushMatrix()
            glTranslatef(0.0, -0.10, 2.15)
            glColor3f(0.10, 0.10, 0.12)
            draw_box(1.95, 0.06, 0.45)
            glPopMatrix()
            # Massive Rear Wing
            glPushMatrix()
            glTranslatef(0.0, 0.65, -2.15)
            glColor3f(0.10, 0.10, 0.12)
            draw_box(1.70, 0.08, 0.50)
            # Endplates
            for s in (-0.85, 0.85):
                glPushMatrix()
                glTranslatef(s, 0.0, 0.0)
                draw_box(0.05, 0.50, 0.55)
                glPopMatrix()
            glPopMatrix()

        elif self.car_idx == 5:
            # 6. POLICE INTERCEPTOR (Push bar, emergency lights)
            draw_box(1.80, 0.52, 4.2)
            glPushMatrix()
            glTranslatef(0.0, 0.46, -0.10)
            draw_box(1.42, 0.46, 2.3)
            glPopMatrix()
            # Roof Emergency Siren Light Bar (Blue & Red)
            glDisable(GL_LIGHTING)
            glPushMatrix()
            glTranslatef(-0.25, 0.74, -0.10)
            glColor3f(1.0, 0.05, 0.05) # Red
            draw_box(0.40, 0.08, 0.18)
            glTranslatef(0.50, 0.0, 0.0)
            glColor3f(0.05, 0.30, 1.0) # Blue
            draw_box(0.40, 0.08, 0.18)
            glPopMatrix()
            glEnable(GL_LIGHTING)

        elif self.car_idx == 6:
            # 7. CITY TAXI (Roof Taxi Sign)
            draw_box(1.80, 0.52, 4.1)
            glPushMatrix()
            glTranslatef(0.0, 0.46, -0.10)
            draw_box(1.42, 0.46, 2.3)
            glPopMatrix()
            # Roof Taxi Light
            glDisable(GL_LIGHTING)
            glPushMatrix()
            glTranslatef(0.0, 0.75, -0.10)
            glColor3f(1.0, 0.95, 0.20)
            draw_box(0.65, 0.12, 0.22)
            glPopMatrix()
            glEnable(GL_LIGHTING)

        elif self.car_idx == 7:
            # 8. LUXURY GOLF CART (Open frame, canopy pillars)
            draw_box(1.40, 0.35, 2.6) # Lower tub
            # 4 Canopy Upright Pillars
            glColor3f(0.20, 0.22, 0.25)
            for px in (-0.60, 0.60):
                for pz in (-0.80, 0.80):
                    glPushMatrix()
                    glTranslatef(px, 0.60, pz)
                    draw_box(0.05, 0.90, 0.05)
                    glPopMatrix()
            # White Sun Canopy Roof
            glPushMatrix()
            glTranslatef(0.0, 1.08, 0.0)
            glColor3f(0.95, 0.95, 0.95)
            draw_box(1.45, 0.08, 2.3)
            glPopMatrix()

        # ---------------------------------------------------------
        # Window Glass Tint (Translucent tinted cockpit)
        # ---------------------------------------------------------
        if self.car_idx != 4 and self.car_idx != 7: # Skip for open formula and golf cart
            glColor4fv(tint_col)
            glPushMatrix()
            glTranslatef(0.0, 0.46, -0.10)
            draw_box(1.45, 0.40, 2.1)
            glPopMatrix()

        # ---------------------------------------------------------
        # Front LED Headlights & Rear Taillights
        # ---------------------------------------------------------
        glDisable(GL_LIGHTING)
        # Headlights (High-intensity Xenon White)
        glColor3f(0.95, 0.98, 1.00)
        for hx in (-0.65, 0.65):
            glPushMatrix()
            glTranslatef(hx, 0.08, 1.92)
            draw_box(0.28, 0.10, 0.06)
            glPopMatrix()
        # Taillights (Gloss Crimson Red)
        glColor3f(0.95, 0.05, 0.08)
        for tx in (-0.65, 0.65):
            glPushMatrix()
            glTranslatef(tx, 0.12, -1.92)
            draw_box(0.28, 0.08, 0.06)
            glPopMatrix()
        glEnable(GL_LIGHTING)

        # ---------------------------------------------------------
        # 4 Alloy Wheels & Brembo Brake Calipers
        # ---------------------------------------------------------
        wheel_track = 0.92
        wheel_base = 1.25 if self.car_idx != 7 else 0.85
        wheel_y = -0.05

        for side in (-1, 1):
            wx = side * wheel_track
            # Front Wheel
            glPushMatrix()
            glTranslatef(wx, wheel_y, wheel_base)
            if side < 0:
                glRotatef(180.0, 0.0, 1.0, 0.0)
            draw_wheel(wheel_col)
            glPopMatrix()
            # Rear Wheel
            glPushMatrix()
            glTranslatef(wx, wheel_y, -wheel_base)
            if side < 0:
                glRotatef(180.0, 0.0, 1.0, 0.0)
            draw_wheel(wheel_col)
            glPopMatrix()

        glPopMatrix()

    def draw_hud(self):
        """Render a clean, modern on-screen telemetry bar and vehicle spec badge."""
        glDisable(GL_LIGHTING)
        glDisable(GL_DEPTH_TEST)

        glMatrixMode(GL_PROJECTION)
        glPushMatrix()
        glLoadIdentity()
        gluOrtho2D(0, self.width, 0, self.height)

        glMatrixMode(GL_MODELVIEW)
        glPushMatrix()
        glLoadIdentity()

        car_name, _ = VEHICLE_NAMES[self.car_idx]
        col_name, _ = PAINT_COLORS[self.color_idx]
        fin_name = FINISHES[self.finish_idx][0]
        wheel_name = WHEEL_FINISHES[self.wheel_idx][0]
        light_name = STUDIO_LIGHTING[self.light_idx]["name"]
        spin_state = "SPINNING" if self.turntable_active else "PAUSED"

        # 1. Top Modern Floating Bar
        glColor4f(0.04, 0.06, 0.09, 0.88)
        glBegin(GL_QUADS)
        glVertex2f(20, self.height - 70)
        glVertex2f(self.width - 20, self.height - 70)
        glVertex2f(self.width - 20, self.height - 20)
        glVertex2f(20, self.height - 20)
        glEnd()

        # Top Bar Cyan Accent Border
        glColor4f(0.00, 0.85, 1.00, 0.80)
        glLineWidth(2.0)
        glBegin(GL_LINE_LOOP)
        glVertex2f(20, self.height - 70)
        glVertex2f(self.width - 20, self.height - 70)
        glVertex2f(self.width - 20, self.height - 20)
        glVertex2f(20, self.height - 20)
        glEnd()
        glLineWidth(1.0)

        # 2. Bottom Quick Navigation Bar
        glColor4f(0.04, 0.06, 0.09, 0.88)
        glBegin(GL_QUADS)
        glVertex2f(20, 20)
        glVertex2f(self.width - 20, 20)
        glVertex2f(self.width - 20, 75)
        glVertex2f(20, 75)
        glEnd()

        # 18 Circular Swatch Indicators along the bottom bar
        swatch_start_x = 45.0
        swatch_y = 47.0
        swatch_spacing = min(40.0, (self.width - 320.0) / 18.0)

        for i, (pname, rgb) in enumerate(PAINT_COLORS):
            cx = swatch_start_x + i * swatch_spacing
            # Swatch circle
            glColor3fv(rgb)
            glBegin(GL_TRIANGLE_FAN)
            glVertex2f(cx, swatch_y)
            for seg in range(17):
                a = seg * (2.0 * math.pi / 16)
                glVertex2f(cx + 9.0 * math.cos(a), swatch_y + 9.0 * math.sin(a))
            glEnd()
            # Selection Highlight Ring
            if i == self.color_idx:
                glColor3f(1.0, 1.0, 1.0)
                glLineWidth(2.5)
                glBegin(GL_LINE_LOOP)
                for seg in range(17):
                    a = seg * (2.0 * math.pi / 16)
                    glVertex2f(cx + 12.0 * math.cos(a), swatch_y + 12.0 * math.sin(a))
                glEnd()
                glLineWidth(1.0)

        glMatrixMode(GL_PROJECTION)
        glPopMatrix()
        glMatrixMode(GL_MODELVIEW)
        glPopMatrix()

        glEnable(GL_DEPTH_TEST)
        glEnable(GL_LIGHTING)

    def render_frame(self):
        glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)

        glMatrixMode(GL_PROJECTION)
        glLoadIdentity()
        aspect = float(self.width) / float(self.height)
        gluPerspective(42.0, aspect, 0.1, 100.0)

        glMatrixMode(GL_MODELVIEW)
        glLoadIdentity()

        # Calculate camera spherical position
        rad_yaw = math.radians(self.yaw)
        rad_pitch = math.radians(self.pitch)
        cam_x = self.distance * math.cos(rad_pitch) * math.sin(rad_yaw)
        cam_y = self.distance * math.sin(rad_pitch)
        cam_z = self.distance * math.cos(rad_pitch) * math.cos(rad_yaw)

        gluLookAt(cam_x, cam_y, cam_z, 0.0, 0.45, 0.0, 0.0, 1.0, 0.0)

        self.setup_lighting()
        self.draw_showroom_environment()
        self.draw_vehicle()
        self.draw_hud()

    def grab_pixels(self):
        glFlush()
        glFinish()
        pixels = glReadPixels(0, 0, self.width, self.height, GL_RGB, GL_UNSIGNED_BYTE)
        img = Image.frombytes("RGB", (self.width, self.height), pixels)
        return img.transpose(Image.FLIP_TOP_BOTTOM)

    def generate_headless_assets(self, output_dir):
        """Generate static hero shot (1080p) and animated turntable demo GIF."""
        print(f"[AutoVerse] Generating 1080p hero render and 30-frame animated GIF...")
        os.makedirs(output_dir, exist_ok=True)

        # 1. Hero Render (1920x1080)
        self.car_idx = 0      # Sports Coupe
        self.color_idx = 4    # Racing Red
        self.finish_idx = 1   # Metallic
        self.wheel_idx = 1    # Polished Silver
        self.neon_idx = 0     # Electric Cyan
        self.neon_active = True
        self.light_idx = 0    # Luxury Studio
        self.yaw = 32.0
        self.pitch = 16.0
        self.distance = 5.4
        self.car_rot = 15.0

        self.render_frame()
        hero_img = self.grab_pixels()
        hero_path = os.path.join(output_dir, "output.png")
        hero_img.save(hero_path, quality=95)
        print(f"[AutoVerse] Saved hero static view: {hero_path}")

        # Also save screenshot.png
        hero_img.save(os.path.join(output_dir, "screenshot.png"), quality=95)

        # 2. 30-Frame Turntable Demonstration GIF
        frames = []
        num_frames = 30
        for f in range(num_frames):
            t = float(f) / num_frames
            # Rotate car on turntable
            self.car_rot = t * 360.0

            # Cycle showcase colors across rotation
            if f < 10:
                self.color_idx = 4   # Racing Red
                self.finish_idx = 1  # Metallic
            elif f < 20:
                self.color_idx = 8   # Royal Blue
                self.finish_idx = 0  # Gloss
            else:
                self.color_idx = 6   # Sunset Orange
                self.finish_idx = 3  # Satin

            # Subtle camera breathing float
            self.yaw = 28.0 + math.sin(t * 2.0 * math.pi) * 6.0
            self.pitch = 16.0 + math.cos(t * 2.0 * math.pi) * 3.0

            self.render_frame()
            frame = self.grab_pixels()
            frames.append(frame)

        gif_path = os.path.join(output_dir, "demo.gif")
        frames[0].save(
            gif_path,
            save_all=True,
            append_images=frames[1:],
            duration=100,  # 100ms per frame = 10 FPS
            loop=0,
            optimize=True
        )
        print(f"[AutoVerse] Saved animated demo GIF: {gif_path} ({len(frames)} frames)")

    def run(self):
        last_time = glfw.get_time()

        while not glfw.window_should_close(self.window):
            current_time = glfw.get_time()
            dt = current_time - last_time
            last_time = current_time

            if self.turntable_active:
                self.car_rot = (self.car_rot + 18.0 * dt) % 360.0

            self.render_frame()
            glfw.swap_buffers(self.window)
            glfw.poll_events()

        glfw.destroy_window(self.window)
        glfw.terminate()


# -------------------------------------------------------------
# Entry Point
# -------------------------------------------------------------
def main():
    parser = argparse.ArgumentParser(description="AutoVerse - 3D Interactive Automotive Showroom & Customizer")
    parser.add_argument("--headless", action="store_true", help="Generate output.png and demo.gif without opening a window")
    parser.add_argument("--outdir", type=str, default=".", help="Directory to save generated visual assets")
    args = parser.parse_args()

    if args.headless:
        viewer = AutoVerseViewer(width=1920, height=1080, offscreen=True)
        viewer.generate_headless_assets(args.outdir)
    else:
        viewer = AutoVerseViewer(width=1280, height=720, offscreen=False)
        viewer.run()

if __name__ == "__main__":
    main()
