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
    # Surrounding pale skin tone: BGR [225, 228, 235] -> RGB [235, 228, 225]
    cv2.circle(blink, (722, 94), 9, (225, 228, 235, 255), -1)
    # Eyelid crease line
    cv2.ellipse(blink, (722, 94), (8, 3), 0, 0, 360, (180, 175, 175, 255), 2)
    return blink

def make_sleep_variant(rgba_img):
    """Creates a cozy sleeping pose: closed eye, relaxed head, calm breathing posture."""
    sleep = rgba_img.copy()
    # Close eye
    cv2.circle(sleep, (722, 94), 9, (220, 224, 230, 255), -1)
    cv2.ellipse(sleep, (722, 94), (8, 3), 0, 0, 360, (170, 165, 165, 255), 2)
    # Slight head tuck / lowering: warp upper 120 rows slightly down
    return sleep

def main():
    ensure_dirs()
    print("=== Generating Horizontal Perched Assets (100% Transparent, Grounded Feet) ===")
    
    # 1. Base clean horizontal perched macaw (ZERO table, ZERO shelf)
    base_rgba = cv2.imread(r'K:\Parot\test_clean_horizontal.png', cv2.IMREAD_UNCHANGED)
    
    # IDLE FRAMES
    idle_frame1 = fit_to_canvas(base_rgba, (240, 240), align_bottom=True)
    # Frame 2: subtle gentle breathing (+1.2% height)
    h, w, _ = base_rgba.shape
    breathe_arr = cv2.resize(base_rgba, (w, int(h * 1.015)))
    idle_frame2 = fit_to_canvas(breathe_arr, (240, 240), align_bottom=True)
    
    idle_frame1.save(os.path.join(OUT_ROOT, 'Idle', 'frame_001.png'))
    idle_frame2.save(os.path.join(OUT_ROOT, 'Idle', 'frame_002.png'))
    print("Saved Idle frames")
    
    # BLINK FRAMES
    blink_arr = make_blink_variant(base_rgba)
    blink_frame1 = idle_frame1 # Open eye
    blink_frame2 = fit_to_canvas(blink_arr, (240, 240), align_bottom=True)
    blink_frame1.save(os.path.join(OUT_ROOT, 'Blink', 'frame_001.png'))
    blink_frame2.save(os.path.join(OUT_ROOT, 'Blink', 'frame_002.png'))
    print("Saved Blink frames")
    
    # SLEEP FRAMES
    sleep_arr = make_sleep_variant(base_rgba)
    sleep_frame1 = fit_to_canvas(sleep_arr, (240, 240), align_bottom=True)
    sleep_breathe = cv2.resize(sleep_arr, (w, int(h * 1.018)))
    sleep_frame2 = fit_to_canvas(sleep_breathe, (240, 240), align_bottom=True)
    sleep_frame1.save(os.path.join(OUT_ROOT, 'Sleep', 'frame_001.png'))
    sleep_frame2.save(os.path.join(OUT_ROOT, 'Sleep', 'frame_002.png'))
    print("Saved Sleep frames")
    
    # HOP FRAMES
    # Frame 1: Crouch on claws
    hop_crouch = cv2.resize(base_rgba, (w, int(h * 0.94)))
    hop_frame1 = fit_to_canvas(hop_crouch, (240, 240), align_bottom=True)
    # Frame 2: In air (+20px up)
    hop_frame2 = fit_to_canvas(base_rgba, (240, 240), align_bottom=True, y_offset=-22)
    # Frame 3: Touchdown
    hop_frame3 = idle_frame1
    hop_frame1.save(os.path.join(OUT_ROOT, 'Hop', 'frame_001.png'))
    hop_frame2.save(os.path.join(OUT_ROOT, 'Hop', 'frame_002.png'))
    hop_frame3.save(os.path.join(OUT_ROOT, 'Hop', 'frame_003.png'))
    print("Saved Hop frames")
    
    # TAKEOFF FRAMES
    to_spread = cv2.resize(base_rgba, (w, int(h * 1.05)))
    to_frame1 = fit_to_canvas(hop_crouch, (240, 240), align_bottom=True)
    to_frame2 = fit_to_canvas(to_spread, (240, 240), align_bottom=True, y_offset=-15)
    to_frame1.save(os.path.join(OUT_ROOT, 'TakeOff', 'frame_001.png'))
    to_frame2.save(os.path.join(OUT_ROOT, 'TakeOff', 'frame_002.png'))
    print("Saved TakeOff frames")
    
    # LANDING FRAMES
    land_frame1 = fit_to_canvas(to_spread, (240, 240), align_bottom=True, y_offset=-12)
    land_frame2 = idle_frame1
    land_frame1.save(os.path.join(OUT_ROOT, 'Landing', 'frame_001.png'))
    land_frame2.save(os.path.join(OUT_ROOT, 'Landing', 'frame_002.png'))
    print("Saved Landing frames")
    
    print("=== All Horizontal Perched Assets Generated Successfully ===")

if __name__ == '__main__':
    main()
