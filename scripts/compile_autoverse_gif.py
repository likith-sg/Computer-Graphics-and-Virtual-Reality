import os
from PIL import Image

frames_dir = "Case Study/AutoVerse/frames"
frame_files = sorted([os.path.join(frames_dir, f) for f in os.listdir(frames_dir) if f.endswith(".png")])

print(f"Loading {len(frame_files)} frames...")
images = []
for f in frame_files:
    im = Image.open(f).convert("RGB")
    # Resize slightly to 960x540 for fast loading and optimal GitHub preview size
    im_resized = im.resize((960, 540), Image.Resampling.LANCZOS)
    images.append(im_resized)

# Save demo.gif
out1 = "Case Study/demo.gif"
out2 = "assets/gifs/case_study_2.gif"

print(f"Saving {out1}...")
images[0].save(
    out1,
    save_all=True,
    append_images=images[1:],
    duration=80,  # 80ms = 12.5 fps smooth turntable rotation
    loop=0,
    optimize=True
)

print(f"Saving {out2}...")
images[0].save(
    out2,
    save_all=True,
    append_images=images[1:],
    duration=80,
    loop=0,
    optimize=True
)

size1 = os.path.getsize(out1)
size2 = os.path.getsize(out2)
print(f"Compiled successfully! {out1} size: {size1} bytes, {out2} size: {size2} bytes")
