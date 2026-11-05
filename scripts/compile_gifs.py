import os
import glob
from PIL import Image

def make_gif(frames_dir, output_gif_path, duration=130, target_size=(960, 540)):
    frame_files = sorted(glob.glob(os.path.join(frames_dir, "frame_*.png")))
    if not frame_files:
        print(f"Warning: No frames found in {frames_dir}")
        return

    frames = []
    for f in frame_files:
        im = Image.open(f).convert("RGB")
        if target_size:
            im = im.resize(target_size, Image.Resampling.LANCZOS)
        # Convert to 256-color palette with adaptive palette
        im_p = im.convert("P", palette=Image.Palette.ADAPTIVE)
        frames.append(im_p)

    out_dir = os.path.dirname(output_gif_path)
    if out_dir and not os.path.exists(out_dir):
        os.makedirs(out_dir, exist_ok=True)

    frames[0].save(
        output_gif_path,
        save_all=True,
        append_images=frames[1:],
        duration=duration,
        loop=0,
        optimize=True
    )
    print(f"[GIF] Created {output_gif_path} ({len(frames)} frames, {duration}ms/frame, size={os.path.getsize(output_gif_path)} bytes)")

if __name__ == "__main__":
    base = r"C:\Users\LIKITH S G\Desktop\CGVR LAB"

    # Case Study 3: WebXR AR/VR Furniture Design (Retail and Remote Commerce)
    # Turntable rotation of Red Velvet Chair & Leather Sofa, sunlit illumination & raycast AR reticle
    dirCS = os.path.join(base, "Case Study", "frames")
    make_gif(dirCS, os.path.join(base, "Case Study", "demo.gif"), duration=130)
    make_gif(dirCS, os.path.join(base, "assets", "gifs", "case_study_3.gif"), duration=130)
    make_gif(dirCS, os.path.join(base, "assets", "gifs", "case_study_2.gif"), duration=130)

    # Clean up test.gif if exists
    if os.path.exists(os.path.join(base, "test.gif")):
        os.remove(os.path.join(base, "test.gif"))
