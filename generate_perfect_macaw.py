import os
import cv2
import numpy as np
from PIL import Image

OUT_ROOT = r'K:\Parot\Assets\Birds\Macaw'

def ensure_dirs():
    for sub in ['Idle', 'Blink', 'Hop', 'TakeOff', 'Flying', 'Landing', 'Sleep']:
        p = os.path.join(OUT_ROOT, sub)
        os.makedirs(p, exist_ok=True)

def fit_to_canvas(rgba_img, target_size=(240, 240), align_bottom=True, y_offset=0):
    """Centers and scales bird into 240x240 canvas with claws firmly resting on bottom edge."""
    pil_img = Image.fromarray(rgba_img)
    bbox = pil_img.getbbox()
    if not bbox:
        return pil_img.resize(target_size, Image.Resampling.LANCZOS)
        
    cropped = pil_img.crop(bbox)
    w, h = cropped.size
    
    # Scale to fill canvas comfortably while preserving aspect ratio
    max_w = target_size[0] - 12
    max_h = target_size[1] - 12
    scale = min(max_w / w, max_h / h)
    new_w = max(1, int(w * scale))
    new_h = max(1, int(h * scale))
    resized = cropped.resize((new_w, new_h), Image.Resampling.LANCZOS)
    
    canvas = Image.new('RGBA', target_size, (0, 0, 0, 0))
    paste_x = (target_size[0] - new_w) // 2
    if align_bottom:
        # Claws firmly planted at the bottom (leaving 4px margin)
        paste_y = target_size[1] - new_h - 4 + y_offset
    else:
        paste_y = (target_size[1] - new_h) // 2 + y_offset
        
    canvas.paste(resized, (paste_x, paste_y), resized)
    return canvas

def make_blink_variant(rgba_img):
    """Draws a closed eyelid at (col 722, row 94)."""
    blink = rgba_img.copy()
    # Surrounding pale skin tone in RGB: [240, 235, 230]
    cv2.circle(blink, (722, 94), 9, (240, 235, 230, 255), -1)
    # Eyelid crease line
    cv2.ellipse(blink, (722, 94), (8, 3), 0, 0, 360, (185, 175, 170, 255), 2)
    return blink

def make_sleep_variant(rgba_img):
    """Creates a cozy sleeping pose: closed eye, relaxed posture."""
    sleep = rgba_img.copy()
    # Close eye with gentle eyelid
    cv2.circle(sleep, (722, 94), 9, (238, 232, 228, 255), -1)
    cv2.ellipse(sleep, (722, 94), (8, 3), 0, 0, 360, (180, 170, 165, 255), 2)
    return sleep

def main():
    ensure_dirs()
    print("=== Generating Perfect Red Macaw Assets (Real Scarlet Colors, Level Perch) ===")
    
    # 1. Base clean horizontal perched macaw
    bgra = cv2.imread(r'K:\Parot\test_clean_horizontal.png', cv2.IMREAD_UNCHANGED)
    # CRUCIAL: Convert BGR to RGB so the bird is brilliant SCARLET RED, not blue!
    rgba_correct = cv2.cvtColor(bgra, cv2.COLOR_BGRA2RGBA)
    
    # Apply leveling rotation (-2.0 degrees) so the claws are perfectly horizontal and level
    pil_base = Image.fromarray(rgba_correct).rotate(-2.0, resample=Image.Resampling.BICUBIC, expand=True)
    base_rgba = np.array(pil_base)
    
    # IDLE FRAMES
    idle_frame1 = fit_to_canvas(base_rgba, (240, 240), align_bottom=True)
    idle_frame2 = idle_frame1 # Keep idle calm, stable, and still
    
    idle_frame1.save(os.path.join(OUT_ROOT, 'Idle', 'frame_001.png'))
    idle_frame2.save(os.path.join(OUT_ROOT, 'Idle', 'frame_002.png'))
    print("Saved Idle frames (Brilliant Scarlet Red, Level Claws)")
    
    # BLINK FRAMES
    blink_arr = make_blink_variant(rgba_correct)
    pil_blink = Image.fromarray(blink_arr).rotate(-2.0, resample=Image.Resampling.BICUBIC, expand=True)
    blink_frame1 = idle_frame1 # Open eye
    blink_frame2 = fit_to_canvas(np.array(pil_blink), (240, 240), align_bottom=True)
    blink_frame1.save(os.path.join(OUT_ROOT, 'Blink', 'frame_001.png'))
    blink_frame2.save(os.path.join(OUT_ROOT, 'Blink', 'frame_002.png'))
    print("Saved Blink frames")
    
    # SLEEP FRAMES
    sleep_arr = make_sleep_variant(rgba_correct)
    pil_sleep = Image.fromarray(sleep_arr).rotate(-2.0, resample=Image.Resampling.BICUBIC, expand=True)
    sleep_frame1 = fit_to_canvas(np.array(pil_sleep), (240, 240), align_bottom=True)
    sleep_frame2 = sleep_frame1
    sleep_frame1.save(os.path.join(OUT_ROOT, 'Sleep', 'frame_001.png'))
    sleep_frame2.save(os.path.join(OUT_ROOT, 'Sleep', 'frame_002.png'))
    print("Saved Sleep frames")
    
    # HOP FRAMES
    h, w, _ = base_rgba.shape
    hop_crouch = cv2.resize(base_rgba, (w, int(h * 0.94)))
    hop_frame1 = fit_to_canvas(hop_crouch, (240, 240), align_bottom=True)
    hop_frame2 = fit_to_canvas(base_rgba, (240, 240), align_bottom=True, y_offset=-24) # In air
    hop_frame3 = idle_frame1 # Landing
    hop_frame1.save(os.path.join(OUT_ROOT, 'Hop', 'frame_001.png'))
    hop_frame2.save(os.path.join(OUT_ROOT, 'Hop', 'frame_002.png'))
    hop_frame3.save(os.path.join(OUT_ROOT, 'Hop', 'frame_003.png'))
    print("Saved Hop frames")
    
    # TAKEOFF FRAMES
    to_spread = cv2.resize(base_rgba, (w, int(h * 1.05)))
    to_frame1 = fit_to_canvas(hop_crouch, (240, 240), align_bottom=True)
    to_frame2 = fit_to_canvas(to_spread, (240, 240), align_bottom=True, y_offset=-16)
    to_frame1.save(os.path.join(OUT_ROOT, 'TakeOff', 'frame_001.png'))
    to_frame2.save(os.path.join(OUT_ROOT, 'TakeOff', 'frame_002.png'))
    print("Saved TakeOff frames")
    
    # LANDING FRAMES
    land_frame1 = fit_to_canvas(to_spread, (240, 240), align_bottom=True, y_offset=-14)
    land_frame2 = idle_frame1
    land_frame1.save(os.path.join(OUT_ROOT, 'Landing', 'frame_001.png'))
    land_frame2.save(os.path.join(OUT_ROOT, 'Landing', 'frame_002.png'))
    print("Saved Landing frames")
    
    # FLYING (8 frames wing flap cycle - ensure correct RGB colors!)
    sheet_path = r'C:\Users\shahi\.gemini\antigravity\brain\de9d3e6e-0b67-4ac1-be9b-d03149d6aace\macaw_flight_cycle_1791787099931.jpg'
    bgr_sheet = cv2.imread(sheet_path)
    rgb_sheet = cv2.cvtColor(bgr_sheet, cv2.COLOR_BGR2RGB)
    
    from scipy.ndimage import label, find_objects
    gray_sheet = cv2.cvtColor(bgr_sheet, cv2.COLOR_BGR2GRAY)
    mask = gray_sheet < 240
    lbl, num = label(mask)
    objs = find_objects(lbl)
    birds = []
    for slc in objs:
        h_ = slc[0].stop - slc[0].start
        w_ = slc[1].stop - slc[1].start
        if h_ > 90 and w_ > 90:
            birds.append(slc)
    birds.sort(key=lambda s: (s[0].start // 400, s[1].start))
    print(f"Extracted {len(birds)} flight frames from sheet")
    
    for i, slc in enumerate(birds[:8]):
        crop_rgb = rgb_sheet[slc[0].start:slc[0].stop, slc[1].start:slc[1].stop]
        # remove white bg
        gray_crop = cv2.cvtColor(crop_rgb, cv2.COLOR_RGB2GRAY)
        alpha = np.where(gray_crop > 240, 0, 255).astype(np.uint8)
        alpha = cv2.GaussianBlur(alpha.astype(float), (3, 3), 0)
        rgba_flight = np.dstack([crop_rgb, np.clip(alpha, 0, 255).astype(np.uint8)])
        flight_frame = fit_to_canvas(rgba_flight, (240, 240), align_bottom=False)
        flight_frame.save(os.path.join(OUT_ROOT, 'Flying', f'frame_{i+1:03d}.png'))
        print(f"Saved Flying/frame_{i+1:03d}.png")
        
    print("=== All Perfect Macaw Assets Generated Successfully ===")

if __name__ == '__main__':
    main()
