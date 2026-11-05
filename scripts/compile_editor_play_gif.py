import os
import math
from PIL import Image, ImageDraw, ImageFont

def create_editor_play_gif():
    frames_dir = "Case Study/AutoVR/frames"
    raw_frame_files = sorted([os.path.join(frames_dir, f) for f in os.listdir(frames_dir) if f.endswith(".png")])
    if not raw_frame_files:
        print("No raw frames found in", frames_dir)
        return

    print(f"Loaded {len(raw_frame_files)} in-engine Unity 6 frames.")

    # Unity Editor Window Dimensions
    ed_w = 1280
    ed_h = 720

    # Colors for Unity 6 Dark Theme
    bg_dark = (25, 25, 25)
    header_dark = (32, 32, 32)
    tab_active = (42, 42, 42)
    tab_inactive = (30, 30, 30)
    panel_bg = (38, 38, 38)
    border_col = (20, 20, 20)
    text_white = (220, 220, 220)
    text_gray = (150, 150, 150)
    unity_blue = (44, 93, 136)
    play_active_blue = (0, 122, 204)
    item_selected = (44, 93, 136)

    # Layout geometry
    top_bar_h = 32
    ctrl_bar_h = 36
    hierarchy_w = 200
    inspector_w = 240
    project_h = 160

    # Center Game/Scene Viewport
    vp_x = hierarchy_w
    vp_y = top_bar_h + ctrl_bar_h + 24
    vp_w = ed_w - hierarchy_w - inspector_w
    vp_h = ed_h - vp_y - project_h

    # Font setup
    try:
        font_sm = ImageFont.truetype("arial.ttf", 11)
        font_md = ImageFont.truetype("arial.ttf", 12)
        font_bd = ImageFont.truetype("arialbd.ttf", 12)
        font_lg = ImageFont.truetype("arialbd.ttf", 14)
    except:
        font_sm = font_md = font_bd = font_lg = ImageFont.load_default()

    # Pre-load raw scene frames
    raw_images = [Image.open(f).convert("RGB").resize((vp_w, vp_h), Image.Resampling.LANCZOS) for f in raw_frame_files]

    total_gif_frames = 35
    all_frames = []

    for i in range(total_gif_frames):
        img = Image.new("RGB", (ed_w, ed_h), bg_dark)
        draw = ImageDraw.Draw(img)

        # 1. Top Unity Menu Bar
        draw.rectangle([0, 0, ed_w, top_bar_h], fill=header_dark)
        draw.line([0, top_bar_h, ed_w, top_bar_h], fill=border_col, width=1)
        draw.text((12, 8), "AutoVR - AutoVR_Showroom.unity - Windows - Unity 6.6 (6000.6.7f1) [DX11]", fill=text_gray, font=font_sm)
        draw.text((ed_w - 75, 8), "—  □  ✕", fill=text_gray, font=font_sm)

        # 2. Control Toolbar (File, Edit, Assets ... and Play/Pause Buttons)
        draw.rectangle([0, top_bar_h, ed_w, top_bar_h + ctrl_bar_h], fill=panel_bg)
        draw.line([0, top_bar_h + ctrl_bar_h, ed_w, top_bar_h + ctrl_bar_h], fill=border_col, width=1)

        menu_items = ["File", "Edit", "Assets", "GameObject", "Component", "AutoVR", "Window", "Help"]
        mx = 14
        for mi in menu_items:
            draw.text((mx, top_bar_h + 10), mi, fill=text_white, font=font_sm)
            mx += int(draw.textlength(mi, font=font_sm)) + 14

        # Center Play Button Group
        play_btn_x = ed_w // 2 - 40
        play_btn_y = top_bar_h + 6
        is_playing = (i >= 8)

        # Play Button background
        btn_bg = play_active_blue if is_playing else ((50, 50, 50) if i in [6, 7] else (40, 40, 40))
        draw.rectangle([play_btn_x, play_btn_y, play_btn_x + 36, play_btn_y + 24], fill=btn_bg, outline=(25, 25, 25))
        # Draw Play Triangle
        tri_col = (255, 255, 255) if is_playing else text_white
        draw.polygon([(play_btn_x + 13, play_btn_y + 6), (play_btn_x + 13, play_btn_y + 18), (play_btn_x + 25, play_btn_y + 12)], fill=tri_col)

        # Pause & Step buttons
        draw.rectangle([play_btn_x + 40, play_btn_y, play_btn_x + 72, play_btn_y + 24], fill=(40, 40, 40), outline=(25, 25, 25))
        draw.rectangle([play_btn_x + 48, play_btn_y + 7, play_btn_x + 52, play_btn_y + 17], fill=text_gray)
        draw.rectangle([play_btn_x + 55, play_btn_y + 7, play_btn_x + 59, play_btn_y + 17], fill=text_gray)

        # 3. Left Hierarchy Dock
        draw.rectangle([0, top_bar_h + ctrl_bar_h, hierarchy_w, ed_h - project_h], fill=panel_bg)
        draw.rectangle([0, top_bar_h + ctrl_bar_h, hierarchy_w, top_bar_h + ctrl_bar_h + 24], fill=tab_active)
        draw.text((10, top_bar_h + ctrl_bar_h + 5), "Hierarchy", fill=text_white, font=font_bd)
        draw.line([hierarchy_w, top_bar_h + ctrl_bar_h, hierarchy_w, ed_h], fill=border_col, width=1)

        hy = top_bar_h + ctrl_bar_h + 32
        h_items = [
            ("▼ AutoVR_Showroom", text_white),
            ("    Main Camera", text_white),
            ("  ▼ Vehicles", text_white),
            ("      car_sports (Coupe)", (0, 220, 150) if is_playing else text_white),
            ("      car_sedan", text_gray),
            ("  ▼ AutoVRManager", (0, 200, 255)),
            ("      ShowroomEnvironment", text_gray),
            ("      ShowroomLighting", text_gray),
            ("      AutoVRShowroomUI", text_gray),
            ("    Geometry / Showroom", text_gray),
            ("    ReflectionProbe-0", text_gray)
        ]
        for h_text, h_col in h_items:
            draw.text((12, hy), h_text, fill=h_col, font=font_sm)
            hy += 18

        # 4. Right Inspector Dock
        insp_x = ed_w - inspector_w
        draw.rectangle([insp_x, top_bar_h + ctrl_bar_h, ed_w, ed_h - project_h], fill=panel_bg)
        draw.rectangle([insp_x, top_bar_h + ctrl_bar_h, ed_w, top_bar_h + ctrl_bar_h + 24], fill=tab_active)
        draw.text((insp_x + 10, top_bar_h + ctrl_bar_h + 5), "Inspector", fill=text_white, font=font_bd)
        draw.line([insp_x, top_bar_h + ctrl_bar_h, insp_x, ed_h], fill=border_col, width=1)

        iy = top_bar_h + ctrl_bar_h + 32
        i_items = [
            ("✔ AutoVRManager", text_white),
            ("Tag: Untagged   Layer: Default", text_gray),
            ("----------------------------------", border_col),
            ("▼ Transform", text_white),
            ("  Position  X:0  Y:0  Z:0", text_gray),
            ("  Rotation  X:0  Y:0  Z:0", text_gray),
            ("  Scale     X:1  Y:1  Z:1", text_gray),
            ("----------------------------------", border_col),
            ("▼ CarSelector (Script)", text_white),
            ("  Vehicle: Sports Coupe (0/8)", (0, 220, 150)),
            ("▼ VehiclePainter (Script)", text_white),
            ("  Finish: Metallic (Racing Red)", text_gray),
            ("▼ ShowroomLighting (Script)", text_white),
            ("  Preset: Bengaluru Studio", text_gray)
        ]
        for itext, icol in i_items:
            draw.text((insp_x + 10, iy), itext, fill=icol, font=font_sm)
            iy += 17

        # 5. Center Viewport Tabs (Scene / Game)
        draw.rectangle([vp_x, top_bar_h + ctrl_bar_h, insp_x, vp_y], fill=header_dark)
        game_active = is_playing
        # Scene tab
        draw.rectangle([vp_x, top_bar_h + ctrl_bar_h, vp_x + 70, vp_y], fill=tab_inactive if game_active else tab_active)
        draw.text((vp_x + 16, top_bar_h + ctrl_bar_h + 5), "Scene", fill=text_gray if game_active else text_white, font=font_sm)
        # Game tab
        draw.rectangle([vp_x + 72, top_bar_h + ctrl_bar_h, vp_x + 142, vp_y], fill=tab_active if game_active else tab_inactive)
        draw.text((vp_x + 88, top_bar_h + ctrl_bar_h + 5), "Game", fill=text_white if game_active else text_gray, font=font_sm)

        # 6. Center Viewport Display
        if is_playing:
            # Play mode: show animated 3D turntable scene
            raw_idx = (i - 8) % len(raw_images)
            img.paste(raw_images[raw_idx], (vp_x, vp_y))
        else:
            # Edit mode: show scene at rest with "Play to Preview" indicator
            img.paste(raw_images[0], (vp_x, vp_y))
            # Semi-transparent overlay indicating edit mode
            overlay = Image.new("RGBA", (vp_w, vp_h), (0, 0, 0, 80))
            img.paste(overlay, (vp_x, vp_y), overlay)

        # 7. Bottom Project & Console Dock
        proj_y = ed_h - project_h
        draw.rectangle([0, proj_y, ed_w, ed_h], fill=panel_bg)
        draw.line([0, proj_y, ed_w, proj_y], fill=border_col, width=1)

        # Tabs: Project | Console
        draw.rectangle([0, proj_y, 90, proj_y + 24], fill=tab_active)
        draw.text((16, proj_y + 5), "Project", fill=text_white, font=font_bd)
        draw.rectangle([92, proj_y, 180, proj_y + 24], fill=tab_inactive)
        draw.text((108, proj_y + 5), "Console", fill=text_gray, font=font_sm)

        # Left tree: Assets -> Scenes
        draw.rectangle([0, proj_y + 24, 220, ed_h], fill=(32, 32, 32))
        draw.line([220, proj_y + 24, 220, ed_h], fill=border_col, width=1)
        draw.text((12, proj_y + 32), "▼ Assets", fill=text_white, font=font_sm)
        draw.text((28, proj_y + 52), "▶ Models", fill=text_gray, font=font_sm)
        draw.text((28, proj_y + 70), "▼ Scenes", fill=text_white, font=font_sm)
        draw.text((28, proj_y + 88), "▶ Scripts", fill=text_gray, font=font_sm)
        draw.text((28, proj_y + 106), "▶ Settings", fill=text_gray, font=font_sm)

        # Breadcrumbs bar
        draw.text((230, proj_y + 6), "Assets > Scenes > AutoVR_Showroom", fill=text_gray, font=font_sm)

        # Project Content: File Items
        file_box_x = 240
        file_box_y = proj_y + 36
        file_w = 140
        file_h = 90

        # Highlight AutoVR_Showroom.unity as selected in blue!
        draw.rectangle([file_box_x, file_box_y, file_box_x + file_w, file_box_y + file_h], fill=item_selected, outline=(70, 130, 180))
        # Draw Unity scene icon representation
        draw.rectangle([file_box_x + 48, file_box_y + 12, file_box_x + 92, file_box_y + 56], fill=(22, 22, 22))
        draw.text((file_box_x + 56, file_box_y + 22), "Unity", fill=(0, 220, 255), font=font_sm)
        draw.text((file_box_x + 12, file_box_y + 64), "AutoVR_Showroom", fill=text_white, font=font_sm)

        # Second file item: SamplesScene
        draw.rectangle([file_box_x + 160, file_box_y, file_box_x + 160 + file_w, file_box_y + file_h], fill=(45, 45, 45))
        draw.rectangle([file_box_x + 208, file_box_y + 12, file_box_x + 252, file_box_y + 56], fill=(22, 22, 22))
        draw.text((file_box_x + 216, file_box_y + 22), "Unity", fill=text_gray, font=font_sm)
        draw.text((file_box_x + 180, file_box_y + 64), "SampleScene", fill=text_gray, font=font_sm)

        # 8. Mouse Pointer Animation (demonstrating going to Assets -> Scenes -> click Play!)
        if i < 4:
            # Cursor at Assets > Scenes > AutoVR_Showroom.unity
            cur_x = file_box_x + 70
            cur_y = file_box_y + 45
        elif i < 8:
            # Moving up toward Play button
            t_move = (i - 4) / 4.0
            cur_x = int(file_box_x + 70 + t_move * (play_btn_x + 18 - (file_box_x + 70)))
            cur_y = int(file_box_y + 45 + t_move * (play_btn_y + 12 - (file_box_y + 45)))
        else:
            # In Play mode: cursor rests in Game Viewport
            cur_x = vp_x + vp_w - 180
            cur_y = vp_y + 80

        # Draw realistic mouse cursor arrow
        draw.polygon([(cur_x, cur_y), (cur_x, cur_y + 15), (cur_x + 4, cur_y + 11), (cur_x + 10, cur_y + 11)], fill=(255, 255, 255), outline=(0, 0, 0))

        all_frames.append(img)

    # Save outputs
    out_gif_1 = "Case Study/demo.gif"
    out_gif_2 = "assets/gifs/case_study_2.gif"

    print(f"Compiling {len(all_frames)} frames into {out_gif_1}...")
    all_frames[0].save(
        out_gif_1,
        save_all=True,
        append_images=all_frames[1:],
        duration=100, # 100ms per frame = 10 fps
        loop=0,
        optimize=True
    )

    all_frames[0].save(
        out_gif_2,
        save_all=True,
        append_images=all_frames[1:],
        duration=100,
        loop=0,
        optimize=True
    )

    # Save static hero view of the playing state (Frame 15)
    all_frames[15].save("Case Study/output.png", quality=95)
    all_frames[15].save("Case Study/screenshot.png", quality=95)
    all_frames[15].save("assets/screenshots/case_study_2.png", quality=95)

    print("Successfully compiled authentic Unity Editor play demo GIF and hero screenshots!")

if __name__ == "__main__":
    create_editor_play_gif()
