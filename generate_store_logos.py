import os
from PIL import Image, ImageDraw, ImageFont

output_dir = r"K:\Parot\Store-Screenshots"
os.makedirs(output_dir, exist_ok=True)

def get_font(size, bold=False):
    font_names = [
        "C:\\Windows\\Fonts\\segoeuib.ttf" if bold else "C:\\Windows\\Fonts\\segoeui.ttf",
        "C:\\Windows\\Fonts\\arialbd.ttf" if bold else "C:\\Windows\\Fonts\\arial.ttf"
    ]
    for fn in font_names:
        if os.path.exists(fn):
            try:
                return ImageFont.truetype(fn, size)
            except:
                pass
    return ImageFont.load_default()

macaw_path = r"K:\Parot\Assets\Birds\ScarletMacaw\Idle\frame_001.png"
bird_img = Image.open(macaw_path).convert("RGBA") if os.path.exists(macaw_path) else None

# 1. 1:1 Box Art (1080 x 1080)
box = Image.new("RGBA", (1080, 1080), (15, 23, 42, 255))
draw_box = ImageDraw.Draw(box)
# Gradient & radial glow
for r in range(450, 0, -25):
    alpha = int((1.0 - (r / 450.0)) * 60)
    draw_box.ellipse([540 - r, 540 - r, 540 + r, 540 + r], outline=(59, 130, 246, alpha), width=15)

if bird_img:
    b_resized = bird_img.resize((560, 560), Image.Resampling.LANCZOS)
    box.paste(b_resized, (260, 180), b_resized)

draw_box.text((540, 780), "Parot", fill=(255, 255, 255), font=get_font(84, bold=True), anchor="mt")
draw_box.text((540, 880), "Desktop Bird Companion", fill=(56, 189, 248), font=get_font(36, bold=True), anchor="mt")
box.save(os.path.join(output_dir, "BoxArt_1080x1080.png"))
print("Saved BoxArt_1080x1080.png")

# 2. 9:16 Poster Art (720 x 1080)
poster = Image.new("RGBA", (720, 1080), (15, 23, 42, 255))
draw_poster = ImageDraw.Draw(poster)
for r in range(350, 0, -20):
    alpha = int((1.0 - (r / 350.0)) * 50)
    draw_poster.ellipse([360 - r, 500 - r, 360 + r, 500 + r], outline=(225, 29, 72, alpha), width=12)

if bird_img:
    b_resized = bird_img.resize((480, 480), Image.Resampling.LANCZOS)
    poster.paste(b_resized, (120, 240), b_resized)

draw_poster.text((360, 760), "Parot", fill=(255, 255, 255), font=get_font(72, bold=True), anchor="mt")
draw_poster.text((360, 850), "Desktop Companion", fill=(251, 191, 36), font=get_font(32, bold=True), anchor="mt")
poster.save(os.path.join(output_dir, "PosterArt_720x1080.png"))
print("Saved PosterArt_720x1080.png")

# 3. 1:1 App Tile Icon (300 x 300)
tile = Image.new("RGBA", (300, 300), (15, 23, 42, 255))
if bird_img:
    b_resized = bird_img.resize((220, 220), Image.Resampling.LANCZOS)
    tile.paste(b_resized, (40, 40), b_resized)
tile.save(os.path.join(output_dir, "AppTile_300x300.png"))
print("Saved AppTile_300x300.png")
