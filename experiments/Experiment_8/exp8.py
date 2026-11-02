"""
Experiment 8: OpenGL Vertex and Fragment Shaders
Student Name: Likith S G
USN: 1RVU23CSE236
Course: Computer Graphics and Virtual Reality Laboratory

Description:
This program demonstrates the modern programmable OpenGL graphics pipeline using GLSL 3.3 Core.
It showcases GPU buffer management with VAO/VBOs, vertex position transformation via a custom
vertex shader, and per-vertex color interpolation with dynamic sine-wave pulsation in the fragment shader.
"""

import sys
import os
import math
import ctypes
import numpy as np
import glfw
from OpenGL.GL import *
from PIL import Image, ImageDraw, ImageFont

WINDOW_WIDTH = 1200
WINDOW_HEIGHT = 700

# GLSL 3.3 Core Vertex Shader
VERTEX_SHADER_SRC = """#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aColor;

out vec3 vertexColor;

uniform float angle;

void main()
{
    float c = cos(angle);
    float s = sin(angle);

    mat2 rotation = mat2(
        c, -s,
        s,  c
    );

    vec2 rotatedPos = rotation * aPos.xy;
    gl_Position = vec4(rotatedPos, aPos.z, 1.0);
    vertexColor = aColor;
}
"""

# GLSL 3.3 Core Fragment Shader
FRAGMENT_SHADER_SRC = """#version 330 core
in vec3 vertexColor;
out vec4 FragColor;

uniform float time;

void main()
{
    float pulse = 0.5 + 0.5 * sin(time * 3.0);
    vec3 animatedColor = vertexColor * (0.75 + 0.25 * pulse);
    FragColor = vec4(animatedColor, 1.0);
}
"""


def compile_shader(source, shader_type):
    """Compiles GLSL shader source code and asserts success."""
    shader = glCreateShader(shader_type)
    glShaderSource(shader, source)
    glCompileShader(shader)

    if not glGetShaderiv(shader, GL_COMPILE_STATUS):
        info_log = glGetShaderInfoLog(shader).decode()
        raise RuntimeError(f"Shader compilation failed: {info_log}")
    return shader


def create_shader_program():
    """Links vertex and fragment shaders into a GPU shader program."""
    vs = compile_shader(VERTEX_SHADER_SRC, GL_VERTEX_SHADER)
    fs = compile_shader(FRAGMENT_SHADER_SRC, GL_FRAGMENT_SHADER)

    program = glCreateProgram()
    glAttachShader(program, vs)
    glAttachShader(program, fs)
    glLinkProgram(program)

    if not glGetProgramiv(program, GL_LINK_STATUS):
        info_log = glGetProgramInfoLog(program).decode()
        raise RuntimeError(f"Shader program linking failed: {info_log}")

    glDeleteShader(vs)
    glDeleteShader(fs)
    return program


def setup_mesh():
    """
    Sets up interleaved vertex data [x, y, z, r, g, b] into a VBO
    and configures attributes inside a Vertex Array Object (VAO).
    """
    vertices = np.array([
        # X,     Y,    Z,    R,    G,    B
        -0.55, -0.45, 0.0,  1.0,  0.2,  0.2,   # Bottom-Left: Vibrant Red
         0.55, -0.45, 0.0,  0.2,  1.0,  0.3,   # Bottom-Right: Vibrant Green
         0.00,  0.60, 0.0,  0.2,  0.5,  1.0    # Top: Vibrant Blue
    ], dtype=np.float32)

    vao = glGenVertexArrays(1)
    vbo = glGenBuffers(1)

    glBindVertexArray(vao)
    glBindBuffer(GL_ARRAY_BUFFER, vbo)
    glBufferData(GL_ARRAY_BUFFER, vertices.nbytes, vertices, GL_STATIC_DRAW)

    stride = 6 * vertices.itemsize
    # Attribute 0: Position (3 floats)
    glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, stride, ctypes.c_void_p(0))
    glEnableVertexAttribArray(0)

    # Attribute 1: Color (3 floats)
    glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, stride, ctypes.c_void_p(3 * vertices.itemsize))
    glEnableVertexAttribArray(1)

    glBindBuffer(GL_ARRAY_BUFFER, 0)
    glBindVertexArray(0)

    return vao, vbo


def capture_frame():
    """Captures the current OpenGL frame buffer as a PIL Image."""
    glReadBuffer(GL_BACK)
    glFinish()
    glPixelStorei(GL_PACK_ALIGNMENT, 1)
    data = glReadPixels(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT, GL_RGB, GL_UNSIGNED_BYTE)
    image = Image.frombytes("RGB", (WINDOW_WIDTH, WINDOW_HEIGHT), data)
    return image.transpose(Image.FLIP_TOP_BOTTOM)


def add_labels_and_save(output_path):
    """Adds informative pipeline diagrams and annotations to captured frame."""
    image = capture_frame()
    draw = ImageDraw.Draw(image)
    try:
        font_title = ImageFont.truetype("C:/Windows/Fonts/segoeuib.ttf", 20)
        font_sub = ImageFont.truetype("C:/Windows/Fonts/segoeui.ttf", 15)
        font_code = ImageFont.truetype("C:/Windows/Fonts/consola.ttf", 13)
        font_tag = ImageFont.truetype("C:/Windows/Fonts/segoeuib.ttf", 13)
    except Exception:
        font_title = font_sub = font_code = font_tag = ImageFont.load_default()

    # Top Header Banner
    draw.rectangle([(0, 0), (WINDOW_WIDTH, 48)], fill=(18, 22, 30))
    draw.text((25, 12), "Experiment 8: Modern OpenGL Programmable Pipeline (GLSL 3.3 Core)", fill=(240, 245, 255), font=font_title)
    draw.text((WINDOW_WIDTH - 420, 15), "Student: Likith S G | USN: 1RVU23CSE236", fill=(170, 185, 205), font=font_sub)

    # Left Info Card: Vertex Shader Pipeline
    draw.rectangle([(25, 65), (380, 320)], fill=(20, 24, 34), outline=(70, 140, 240), width=1)
    draw.text((40, 75), "1. Vertex Shader (#version 330)", fill=(100, 180, 255), font=font_title)
    draw.text((40, 105), "- Input: aPos (loc=0), aColor (loc=1)", fill=(210, 220, 235), font=font_sub)
    draw.text((40, 130), "- Computes 2D Rotation in GPU:", fill=(210, 220, 235), font=font_sub)
    draw.text((55, 155), "mat2(cos(a), -sin(a),", fill=(120, 230, 160), font=font_code)
    draw.text((55, 175), "     sin(a),  cos(a))", fill=(120, 230, 160), font=font_code)
    draw.text((40, 205), "- gl_Position = vec4(rotated, z, 1.0)", fill=(210, 220, 235), font=font_code)
    draw.text((40, 230), "- Passes vertexColor to Fragment stage", fill=(210, 220, 235), font=font_sub)
    draw.text((40, 260), "Buffer Config: VAO with 6-float stride", fill=(255, 200, 80), font=font_tag)

    # Left Info Card: Fragment Shader Pipeline
    draw.rectangle([(25, 335), (380, 570)], fill=(20, 24, 34), outline=(220, 80, 120), width=1)
    draw.text((40, 345), "2. Fragment Shader (#version 330)", fill=(255, 120, 150), font=font_title)
    draw.text((40, 375), "- Per-fragment smooth color interpolation", fill=(210, 220, 235), font=font_sub)
    draw.text((40, 400), "- Barycentric coordinate rasterization", fill=(210, 220, 235), font=font_sub)
    draw.text((40, 425), "- Dynamic harmonic sine-wave pulse:", fill=(210, 220, 235), font=font_sub)
    draw.text((55, 450), "pulse = 0.5 + 0.5*sin(time * 3.0)", fill=(255, 180, 100), font=font_code)
    draw.text((40, 480), "- FragColor = vec4(animatedColor, 1.0)", fill=(210, 220, 235), font=font_code)
    draw.text((40, 510), "Continuous hardware color modulation", fill=(120, 240, 160), font=font_tag)

    # Bottom Status Bar
    draw.rectangle([(25, WINDOW_HEIGHT - 65), (WINDOW_WIDTH - 25, WINDOW_HEIGHT - 20)], fill=(20, 25, 35), outline=(50, 60, 80))
    draw.text((40, WINDOW_HEIGHT - 52), "Programmable GPU Pipeline  |  Uniform Variable Binding: angle (theta), time (sec)  |  VAO/VBO Buffer Management", fill=(180, 195, 215), font=font_sub)

    image.save(output_path)
    print(f"[SUCCESS] Saved framebuffer to {output_path}")

    assets_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", "assets", "screenshots"))
    if os.path.exists(assets_dir):
        assets_path = os.path.join(assets_dir, "exp8.png")
        image.save(assets_path)
        print(f"[SUCCESS] Mirrored screenshot to {assets_path}")


def save_animated_gif(window, program, vao, angle_loc, time_loc, output_gif_path, num_frames=60):
    """Records real-time animation frames and produces a smooth animated GIF."""
    frames = []
    print(f"[INFO] Generating {num_frames} animation frames for GIF...")

    for i in range(num_frames):
        t = i * (2.0 * math.pi / float(num_frames))
        angle = t

        glClearColor(0.06, 0.07, 0.10, 1.0)
        glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)

        glUseProgram(program)
        glUniform1f(angle_loc, angle)
        glUniform1f(time_loc, t * 2.0)

        glBindVertexArray(vao)
        glDrawArrays(GL_TRIANGLES, 0, 3)
        glBindVertexArray(0)

        glfw.swap_buffers(window)
        frame = capture_frame()
        # Scale slightly down for compact, high-efficiency GIF
        frame_thumb = frame.resize((600, 350), Image.Resampling.LANCZOS)
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
            assets_gif_path = os.path.join(assets_gif_dir, "exp8.gif")
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
    save_gif = "--save-gif" in sys.argv or headless  # Generate GIF in headless run

    if not glfw.init():
        raise RuntimeError("GLFW initialization failed")

    glfw.window_hint(glfw.CONTEXT_VERSION_MAJOR, 3)
    glfw.window_hint(glfw.CONTEXT_VERSION_MINOR, 3)
    glfw.window_hint(glfw.OPENGL_PROFILE, glfw.OPENGL_CORE_PROFILE)
    glfw.window_hint(glfw.OPENGL_FORWARD_COMPAT, GL_TRUE)
    if headless:
        glfw.window_hint(glfw.VISIBLE, glfw.FALSE)

    window = glfw.create_window(WINDOW_WIDTH, WINDOW_HEIGHT, "Experiment 8 - Vertex and Fragment Shaders (Likith S G)", None, None)
    if not window:
        glfw.terminate()
        raise RuntimeError("Failed to create GLFW 3.3 Core window")

    glfw.make_context_current(window)

    program = create_shader_program()
    vao, vbo = setup_mesh()

    angle_loc = glGetUniformLocation(program, "angle")
    time_loc = glGetUniformLocation(program, "time")

    # Render a single representative frame for the static screenshot
    glClearColor(0.06, 0.07, 0.10, 1.0)
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)
    glUseProgram(program)
    glUniform1f(angle_loc, 0.45)
    glUniform1f(time_loc, 1.20)
    glBindVertexArray(vao)
    glDrawArrays(GL_TRIANGLES, 0, 3)
    glBindVertexArray(0)
    glfw.swap_buffers(window)

    output_png = os.path.join(os.path.dirname(__file__), "output.png")
    add_labels_and_save(output_png)

    if save_gif:
        output_gif = os.path.join(os.path.dirname(__file__), "anim.gif")
        save_animated_gif(window, program, vao, angle_loc, time_loc, output_gif, num_frames=45)

    if headless:
        glfw.destroy_window(window)
        glfw.terminate()
        return

    # Interactive render loop
    while not glfw.window_should_close(window):
        cur_time = glfw.get_time()
        glClearColor(0.06, 0.07, 0.10, 1.0)
        glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)

        glUseProgram(program)
        glUniform1f(angle_loc, cur_time * 0.8)
        glUniform1f(time_loc, cur_time)

        glBindVertexArray(vao)
        glDrawArrays(GL_TRIANGLES, 0, 3)
        glBindVertexArray(0)

        glfw.swap_buffers(window)
        glfw.poll_events()

    glfw.destroy_window(window)
    glfw.terminate()


if __name__ == "__main__":
    main()
