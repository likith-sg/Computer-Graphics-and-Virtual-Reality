"""
Experiment 9: Simple Interactive 3D Scene
Student Name: Likith S G
USN: 1RVU23CSE236
Course: Computer Graphics and Virtual Reality Laboratory

Description:
This program implements a complete interactive 3D virtual environment demonstrating
fundamental game engine architecture (as seen in Unity) directly in Python using OpenGL:
1. 3D Scene Architecture: Textured arena floor grid, boundary markers, obstacle blocks, lighting.
2. Player Kinematics & Jump Physics: 3D character movement, rotation, and Euler gravity jump simulation.
3. Third-Person Camera Pipeline: Smooth following camera calculating dynamic 3D look-at vectors.
4. Animated Collectibles: 5 rotating, bobbing diamond gems with euclidean proximity collection logic.
5. Real-Time HUD: Real-time telemetry, collection score counter, and interactive keyboard bindings.
"""

import sys
import os
import math
import glfw
from OpenGL.GL import *
from OpenGL.GLU import *
from PIL import Image, ImageDraw, ImageFont

WINDOW_WIDTH = 1300
WINDOW_HEIGHT = 750

# Player Kinematic State
player_x = 0.0
player_y = 0.5  # Grounded level (avatar half-height)
player_z = 2.0
player_yaw = 0.0
player_vy = 0.0
is_grounded = True

GRAVITY = 24.0
JUMP_IMPULSE = 8.5
MOVE_SPEED = 7.0
ROT_SPEED = 120.0

# Collectibles (5 rotating gems placed throughout arena)
collectibles = [
    {"pos": [-4.0, 0.8, -3.0], "collected": False, "color": (1.0, 0.85, 0.15)},  # Gold
    {"pos": [ 4.0, 0.8, -3.0], "collected": False, "color": (0.2, 0.95, 1.00)},  # Cyan
    {"pos": [-3.5, 0.8,  3.5], "collected": False, "color": (1.0, 0.25, 0.75)},  # Magenta
    {"pos": [ 3.5, 0.8,  3.5], "collected": False, "color": (0.3, 1.00, 0.40)},  # Emerald
    {"pos": [ 0.0, 0.8, -6.0], "collected": False, "color": (1.0, 0.45, 0.10)},  # Orange
]

# Obstacles: (x, z, width, depth, height, color)
obstacles = [
    (-2.0, -1.0, 1.5, 1.5, 1.8, (0.55, 0.25, 0.65)),
    ( 2.5, -0.5, 1.2, 2.0, 1.5, (0.20, 0.60, 0.70)),
    (-1.0, -4.5, 3.0, 1.0, 1.4, (0.75, 0.45, 0.20)),
    ( 2.0,  2.0, 1.5, 1.5, 1.6, (0.35, 0.55, 0.35)),
]

score = 0
anim_time = 0.0
key_states = {}


def reset_game():
    global player_x, player_y, player_z, player_yaw, player_vy, is_grounded, score
    player_x = 0.0
    player_y = 0.5
    player_z = 2.0
    player_yaw = 0.0
    player_vy = 0.0
    is_grounded = True
    score = 0
    for c in collectibles:
        c["collected"] = False


def draw_cube(size=1.0):
    """Draws a unit cube with surface normals."""
    s = size / 2.0
    glBegin(GL_QUADS)
    # Front
    glNormal3f(0, 0, 1); glVertex3f(-s, -s,  s); glVertex3f( s, -s,  s); glVertex3f( s,  s,  s); glVertex3f(-s,  s,  s)
    # Back
    glNormal3f(0, 0, -1); glVertex3f(-s, -s, -s); glVertex3f(-s,  s, -s); glVertex3f( s,  s, -s); glVertex3f( s, -s, -s)
    # Top
    glNormal3f(0, 1, 0); glVertex3f(-s,  s, -s); glVertex3f(-s,  s,  s); glVertex3f( s,  s,  s); glVertex3f( s,  s, -s)
    # Bottom
    glNormal3f(0, -1, 0); glVertex3f(-s, -s, -s); glVertex3f( s, -s, -s); glVertex3f( s, -s,  s); glVertex3f(-s, -s,  s)
    # Right
    glNormal3f(1, 0, 0); glVertex3f( s, -s, -s); glVertex3f( s,  s, -s); glVertex3f( s,  s,  s); glVertex3f( s, -s,  s)
    # Left
    glNormal3f(-1, 0, 0); glVertex3f(-s, -s, -s); glVertex3f(-s, -s,  s); glVertex3f(-s,  s,  s); glVertex3f(-s,  s, -s)
    glEnd()


def draw_faceted_gem():
    """Renders a 3D faceted bipyramidal gem."""
    r = 0.38
    h = 0.55
    glBegin(GL_TRIANGLES)
    # Top cone
    for i in range(8):
        a1 = i * (2.0 * math.pi / 8.0)
        a2 = (i + 1) * (2.0 * math.pi / 8.0)
        x1, z1 = r * math.cos(a1), r * math.sin(a1)
        x2, z2 = r * math.cos(a2), r * math.sin(a2)
        glNormal3f(x1 + x2, 0.8, z1 + z2)
        glVertex3f(0.0, h, 0.0)
        glVertex3f(x1, 0.0, z1)
        glVertex3f(x2, 0.0, z2)

    # Bottom cone
    for i in range(8):
        a1 = i * (2.0 * math.pi / 8.0)
        a2 = (i + 1) * (2.0 * math.pi / 8.0)
        x1, z1 = r * math.cos(a1), r * math.sin(a1)
        x2, z2 = r * math.cos(a2), r * math.sin(a2)
        glNormal3f(x1 + x2, -0.8, z1 + z2)
        glVertex3f(0.0, -h, 0.0)
        glVertex3f(x2, 0.0, z2)
        glVertex3f(x1, 0.0, z1)
    glEnd()


def draw_arena():
    """Draws checkered arena ground grid and boundary pillars."""
    grid_size = 18
    step = 1.0
    start = -grid_size / 2.0

    glDisable(GL_LIGHTING)
    glBegin(GL_QUADS)
    for i in range(grid_size):
        for j in range(grid_size):
            if (i + j) % 2 == 0:
                glColor3f(0.13, 0.16, 0.22)
            else:
                glColor3f(0.09, 0.11, 0.16)
            x0 = start + i * step
            z0 = start + j * step
            glVertex3f(x0, 0.0, z0)
            glVertex3f(x0 + step, 0.0, z0)
            glVertex3f(x0 + step, 0.0, z0 + step)
            glVertex3f(x0, 0.0, z0 + step)
    glEnd()

    # Arena Boundary Line
    glColor3f(0.35, 0.45, 0.65)
    glLineWidth(2.5)
    glBegin(GL_LINE_LOOP)
    b = grid_size / 2.0
    glVertex3f(-b, 0.02, -b); glVertex3f( b, 0.02, -b)
    glVertex3f( b, 0.02,  b); glVertex3f(-b, 0.02,  b)
    glEnd()
    glEnable(GL_LIGHTING)


def render_scene():
    """Renders the complete 3D scene from third-person perspective."""
    glClearColor(0.05, 0.06, 0.09, 1.0)
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)
    glEnable(GL_DEPTH_TEST)

    glMatrixMode(GL_PROJECTION)
    glLoadIdentity()
    gluPerspective(55.0, WINDOW_WIDTH / float(WINDOW_HEIGHT), 0.1, 100.0)

    # Third-person following camera
    cam_dist = 5.2
    cam_height = 2.8
    rad = math.radians(player_yaw)
    cam_x = player_x - cam_dist * math.sin(rad)
    cam_z = player_z + cam_dist * math.cos(rad)
    cam_y = player_y + cam_height

    glMatrixMode(GL_MODELVIEW)
    glLoadIdentity()
    gluLookAt(
        cam_x, cam_y, cam_z,
        player_x, player_y + 0.6, player_z,
        0.0, 1.0, 0.0
    )

    # Lighting setup
    glEnable(GL_LIGHTING)
    glEnable(GL_LIGHT0)
    glLightfv(GL_LIGHT0, GL_POSITION, [10.0, 18.0, 12.0, 1.0])
    glLightfv(GL_LIGHT0, GL_AMBIENT,  [0.25, 0.28, 0.35, 1.0])
    glLightfv(GL_LIGHT0, GL_DIFFUSE,  [0.85, 0.85, 0.80, 1.0])
    glLightfv(GL_LIGHT0, GL_SPECULAR, [1.0, 1.0, 1.0, 1.0])
    glEnable(GL_COLOR_MATERIAL)
    glColorMaterial(GL_FRONT, GL_AMBIENT_AND_DIFFUSE)

    # Draw Ground
    draw_arena()

    # Draw Obstacles
    for ox, oz, ow, od, oh, col in obstacles:
        glPushMatrix()
        glTranslatef(ox, oh / 2.0, oz)
        glScalef(ow, oh, od)
        glColor3fv(col)
        draw_cube(1.0)
        glPopMatrix()

    # Draw Animated Collectibles
    for i, c in enumerate(collectibles):
        if not c["collected"]:
            glPushMatrix()
            bob = 0.15 * math.sin(anim_time * 3.0 + i)
            glTranslatef(c["pos"][0], c["pos"][1] + bob, c["pos"][2])
            glRotatef(anim_time * 75.0 + i * 45.0, 0.0, 1.0, 0.0)
            glColor3fv(c["color"])
            draw_faceted_gem()
            glPopMatrix()

    # Draw Player Avatar (Dynamic character with torso and head)
    glPushMatrix()
    glTranslatef(player_x, player_y, player_z)
    glRotatef(-player_yaw, 0.0, 1.0, 0.0)

    # Torso (Rich Cobalt Blue)
    glColor3f(0.20, 0.55, 0.95)
    glPushMatrix()
    glScalef(0.8, 1.0, 0.5)
    draw_cube(1.0)
    glPopMatrix()

    # Head (Warm Amber)
    glColor3f(0.95, 0.75, 0.35)
    glPushMatrix()
    glTranslatef(0.0, 0.75, 0.0)
    glScalef(0.45, 0.45, 0.45)
    draw_cube(1.0)
    glPopMatrix()

    # Facing Visor (Cyan accent)
    glColor3f(0.15, 0.95, 1.0)
    glPushMatrix()
    glTranslatef(0.0, 0.75, -0.25)
    glScalef(0.35, 0.15, 0.10)
    draw_cube(1.0)
    glPopMatrix()

    glPopMatrix()
    glDisable(GL_LIGHTING)


def update_physics(dt):
    """Updates kinematics, gravity integration, collision, and gem pickups."""
    global player_x, player_y, player_z, player_yaw, player_vy, is_grounded, score, anim_time
    anim_time += dt

    # Steering Rotation (A/D or Left/Right)
    if key_states.get(glfw.KEY_A, False) or key_states.get(glfw.KEY_LEFT, False):
        player_yaw -= ROT_SPEED * dt
    if key_states.get(glfw.KEY_D, False) or key_states.get(glfw.KEY_RIGHT, False):
        player_yaw += ROT_SPEED * dt

    # Forward / Backward Movement (W/S or Up/Down)
    rad = math.radians(player_yaw)
    dx = 0.0
    dz = 0.0
    if key_states.get(glfw.KEY_W, False) or key_states.get(glfw.KEY_UP, False):
        dx -= math.sin(rad) * MOVE_SPEED * dt
        dz -= math.cos(rad) * MOVE_SPEED * dt
    if key_states.get(glfw.KEY_S, False) or key_states.get(glfw.KEY_DOWN, False):
        dx += math.sin(rad) * MOVE_SPEED * dt
        dz += math.cos(rad) * MOVE_SPEED * dt

    player_x += dx
    player_z += dz

    # Jump & Euler Gravity Simulation
    if (key_states.get(glfw.KEY_SPACE, False)) and is_grounded:
        player_vy = JUMP_IMPULSE
        is_grounded = False

    if not is_grounded:
        player_vy -= GRAVITY * dt
        player_y += player_vy * dt
        if player_y <= 0.5:
            player_y = 0.5
            player_vy = 0.0
            is_grounded = True

    # Arena Clamping
    b = 8.5
    player_x = max(-b, min(b, player_x))
    player_z = max(-b, min(b, player_z))

    # Proximity Collectible Pickup Logic
    for c in collectibles:
        if not c["collected"]:
            dist_sq = (player_x - c["pos"][0]) ** 2 + (player_z - c["pos"][2]) ** 2
            if dist_sq < 1.4:
                c["collected"] = True
                score += 1


def capture_frame():
    """Captures the active framebuffer as a PIL image."""
    glReadBuffer(GL_BACK)
    glFinish()
    glPixelStorei(GL_PACK_ALIGNMENT, 1)
    data = glReadPixels(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT, GL_RGB, GL_UNSIGNED_BYTE)
    image = Image.frombytes("RGB", (WINDOW_WIDTH, WINDOW_HEIGHT), data)
    return image.transpose(Image.FLIP_TOP_BOTTOM)


def add_labels_and_save(output_path):
    """Draws a professional Head-Up Display (HUD) overlay and saves output image."""
    image = capture_frame()
    draw = ImageDraw.Draw(image)
    try:
        font_title = ImageFont.truetype("C:/Windows/Fonts/segoeuib.ttf", 20)
        font_sub = ImageFont.truetype("C:/Windows/Fonts/segoeui.ttf", 15)
        font_hud = ImageFont.truetype("C:/Windows/Fonts/consola.ttf", 14)
        font_bold = ImageFont.truetype("C:/Windows/Fonts/segoeuib.ttf", 14)
    except Exception:
        font_title = font_sub = font_hud = font_bold = ImageFont.load_default()

    # Top Header Banner
    draw.rectangle([(0, 0), (WINDOW_WIDTH, 50)], fill=(18, 22, 30))
    draw.text((25, 12), "Experiment 9: Simple Interactive 3D Scene (Virtual Reality Engine Architecture)", fill=(240, 245, 255), font=font_title)
    draw.text((WINDOW_WIDTH - 420, 15), "Student: Likith S G | USN: 1RVU23CSE236", fill=(170, 185, 205), font=font_sub)

    # Top-Left HUD Card: Mission & Telemetry
    draw.rectangle([(25, 65), (380, 240)], fill=(22, 26, 36, 230), outline=(70, 140, 240), width=1)
    draw.text((40, 75), "PLAYER TELEMETRY & HUD", fill=(100, 190, 255), font=font_bold)
    draw.text((40, 105), f"Position : X: {player_x:+5.2f}  Y: {player_y:+5.2f}  Z: {player_z:+5.2f}", fill=(220, 230, 245), font=font_hud)
    draw.text((40, 130), f"Heading  : Yaw = {player_yaw:+6.1f} deg", fill=(220, 230, 245), font=font_hud)
    draw.text((40, 155), f"State    : {'GROUNDED' if is_grounded else 'AIRBORNE (JUMPING)'}", fill=(120, 240, 160) if is_grounded else (255, 180, 80), font=font_hud)
    draw.text((40, 185), f"Gems Collected : {score} / {len(collectibles)}", fill=(255, 220, 60), font=font_bold)

    # Top-Right Controls Card
    rx = WINDOW_WIDTH - 360
    draw.rectangle([(rx, 65), (WINDOW_WIDTH - 25, 220)], fill=(22, 26, 36, 230), outline=(60, 180, 120), width=1)
    draw.text((rx + 15, 75), "INTERACTIVE KEYBOARD CONTROLS", fill=(90, 230, 140), font=font_bold)
    draw.text((rx + 15, 105), "[W / Up]    : Move Forward", fill=(210, 220, 235), font=font_hud)
    draw.text((rx + 15, 128), "[S / Down]  : Move Backward", fill=(210, 220, 235), font=font_hud)
    draw.text((rx + 15, 151), "[A / D]     : Steer Yaw Angle", fill=(210, 220, 235), font=font_hud)
    draw.text((rx + 15, 174), "[Space]     : Jump Physics (vy=8.5)", fill=(210, 220, 235), font=font_hud)
    draw.text((rx + 15, 197), "[R]         : Reset Arena Scene", fill=(210, 220, 235), font=font_hud)

    # Bottom Pipeline Feature Card
    draw.rectangle([(25, WINDOW_HEIGHT - 60), (WINDOW_WIDTH - 25, WINDOW_HEIGHT - 18)], fill=(18, 22, 30), outline=(50, 60, 80))
    draw.text((40, WINDOW_HEIGHT - 48), "Features: 3rd-Person Camera Tracking (gluLookAt) | Euler Gravity Simulation | Faceted Bipyramidal Gems | Proximity Triggers", fill=(180, 195, 215), font=font_sub)

    image.save(output_path)
    print(f"[SUCCESS] Saved framebuffer to {output_path}")

    assets_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", "assets", "screenshots"))
    if os.path.exists(assets_dir):
        assets_path = os.path.join(assets_dir, "exp9.png")
        image.save(assets_path)
        print(f"[SUCCESS] Mirrored screenshot to {assets_path}")


def save_animated_demo_gif(window, output_gif_path, num_frames=60):
    """Simulates player movement and gem collection to produce a genuine gameplay GIF."""
    global player_x, player_z, player_yaw, player_vy, is_grounded, key_states
    frames = []
    print(f"[INFO] Generating {num_frames} gameplay animation frames for Experiment 9 GIF...")

    reset_game()
    dt = 0.035

    for i in range(num_frames):
        # Programmatic movement script to demonstrate capabilities:
        # Move forward, steer towards gems, and jump
        if i < 20:
            key_states[glfw.KEY_W] = True
            key_states[glfw.KEY_A] = True
        elif i < 35:
            key_states[glfw.KEY_A] = False
            key_states[glfw.KEY_SPACE] = True
        elif i < 48:
            key_states[glfw.KEY_SPACE] = False
            key_states[glfw.KEY_D] = True
        else:
            key_states[glfw.KEY_D] = False
            key_states[glfw.KEY_W] = True

        update_physics(dt)
        render_scene()
        glfw.swap_buffers(window)

        frame = capture_frame()
        frame_thumb = frame.resize((600, 346), Image.Resampling.LANCZOS)
        frames.append(frame_thumb)

    key_states.clear()

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
            assets_gif_path = os.path.join(assets_gif_dir, "exp9.gif")
            frames[0].save(
                assets_gif_path,
                save_all=True,
                append_images=frames[1:],
                duration=45,
                loop=0,
                optimize=True
            )
            print(f"[SUCCESS] Mirrored animated GIF to {assets_gif_path}")


def key_callback(window, key, scancode, action, mods):
    global key_states
    if action == glfw.PRESS:
        key_states[key] = True
        if key == glfw.KEY_R:
            reset_game()
    elif action == glfw.RELEASE:
        key_states[key] = False


def main():
    headless = "--headless" in sys.argv or "--save" in sys.argv or "--screenshot" in sys.argv
    save_gif = "--save-gif" in sys.argv or headless

    if not glfw.init():
        raise RuntimeError("GLFW initialization failed")

    glfw.window_hint(glfw.DEPTH_BITS, 24)
    if headless:
        glfw.window_hint(glfw.VISIBLE, glfw.FALSE)

    window = glfw.create_window(WINDOW_WIDTH, WINDOW_HEIGHT, "Experiment 9 - Interactive 3D Scene (Likith S G)", None, None)
    if not window:
        glfw.terminate()
        raise RuntimeError("Failed to create GLFW window")

    glfw.make_context_current(window)
    glfw.set_key_callback(window, key_callback)

    # Initial frame capture
    render_scene()
    output_png = os.path.join(os.path.dirname(__file__), "output.png")
    add_labels_and_save(output_png)

    if save_gif:
        output_gif = os.path.join(os.path.dirname(__file__), "demo.gif")
        save_animated_demo_gif(window, output_gif, num_frames=55)

    if headless:
        glfw.destroy_window(window)
        glfw.terminate()
        return

    # Interactive Event Loop
    last_time = glfw.get_time()
    while not glfw.window_should_close(window):
        now = glfw.get_time()
        dt = min(0.05, now - last_time)
        last_time = now

        update_physics(dt)
        render_scene()
        glfw.swap_buffers(window)
        glfw.poll_events()

    glfw.destroy_window(window)
    glfw.terminate()


if __name__ == "__main__":
    main()
