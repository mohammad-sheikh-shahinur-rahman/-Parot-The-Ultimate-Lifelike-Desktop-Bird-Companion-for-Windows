import os
import glob
import numpy as np
import cv2
from PIL import Image

OUT_ROOT = r'K:\Parot\Assets\Birds\Macaw'

def ensure_dirs():
    for sub in ['Idle', 'Blink', 'Hop', 'TakeOff', 'Flying', 'Landing', 'Sleep']:
        p = os.path.join(OUT_ROOT, sub)
        os.makedirs(p, exist_ok=True)

def floodfill_remove_bg(bgr_img, tolerance=15):
    """Accurately floodfills pure/near-white backgrounds from borders, producing RGBA with anti-aliasing."""
    gray = cv2.cvtColor(bgr_img, cv2.COLOR_BGR2GRAY)
    h, w = gray.shape
    mask = np.zeros((h + 2, w + 2), np.uint8)
    
    # Floodfill from borders
    flood = gray.copy()
    for pt in [(0, 0), (w-1, 0), (0, h-1), (w-1, h-1), (w//2, 0), (w//2, h-1), (0, h//2), (w-1, h//2)]:
        if pt[0] < w and pt[1] < h and gray[pt[1], pt[0]] > 220:
            cv2.floodFill(flood, mask, pt, 255, tolerance, tolerance, cv2.FLOODFILL_FIXED_RANGE)
            
    bg_mask = mask[1:-1, 1:-1] == 1
    
    # Also remove any isolated high-luminance border pixels
    bg_mask |= (gray > 250) & ((bgr_img[:,:,0] > 245) & (bgr_img[:,:,1] > 245) & (bgr_img[:,:,2] > 245))
    
    rgba = cv2.cvtColor(bgr_img, cv2.COLOR_BGR2RGBA)
    rgba[bg_mask, 3] = 0
    
    # Smooth edges slightly to remove aliasing
    alpha = rgba[:, :, 3].astype(np.float32)
    alpha = cv2.GaussianBlur(alpha, (3, 3), 0)
    rgba[:, :, 3] = np.clip(alpha, 0, 255).astype(np.uint8)
    return rgba

def fit_to_canvas(rgba_img, target_size=(240, 240), align_bottom=True):
    """Centers and scales bird into canvas, aligning feet to bottom or center."""
    pil_img = Image.fromarray(rgba_img)
    bbox = pil_img.getbbox()
    if not bbox:
        return pil_img.resize(target_size, Image.Resampling.LANCZOS)
        
    cropped = pil_img.crop(bbox)
    w, h = cropped.size
    
    # Scale to fit comfortably
    max_w = target_size[0] - 16
    max_h = target_size[1] - 16
    scale = min(max_w / w, max_h / h)
    new_w = max(1, int(w * scale))
    new_h = max(1, int(h * scale))
    resized = cropped.resize((new_w, new_h), Image.Resampling.LANCZOS)
    
    canvas = Image.new('RGBA', target_size, (0, 0, 0, 0))
    paste_x = (target_size[0] - new_w) // 2
    if align_bottom:
        paste_y = target_size[1] - new_h - 4
    else:
        paste_y = (target_size[1] - new_h) // 2
        
    canvas.paste(resized, (paste_x, paste_y), resized)
    return canvas

def make_blink_frame(rgba_head):
    """Draws a closed eyelid over the eye at row 215, col 191."""
    blink = rgba_head.copy()
    # Eye center in 1024x1024 image is (191, 215)
    # Draw eyelid matching surrounding pale skin (#F4EFEB) with subtle crease line
    cv2.circle(blink, (191, 215), 11, (244, 238, 233, 255), -1)
    cv2.ellipse(blink, (191, 215), (10, 4), 0, 0, 360, (210, 195, 185, 255), 2)
    return blink

def main():
    ensure_dirs()
    print("=== Processing Realistic Photographic Macaw Assets ===")
    
    # 1. STANDING / IDLE
    # Load the hyper-realistic standing macaw (has ZERO shelf, pure white bg)
    realistic_path = r'C:\Users\shahi\.gemini\antigravity\brain\de9d3e6e-0b67-4ac1-be9b-d03149d6aace\macaw_standing_realistic_1791789006213.jpg'
    bgr_standing = cv2.imread(realistic_path)
    rgba_standing = floodfill_remove_bg(bgr_standing, tolerance=14)
    
    # Idle Frame 1: Standard proud standing
    idle1 = fit_to_canvas(rgba_standing, (240, 240), align_bottom=True)
    idle1.save(os.path.join(OUT_ROOT, 'Idle', 'frame_001.png'))
    
    # Idle Frame 2: Slight breathing movement (subtle vertical pulse)
    # Warping chest slightly for realistic breathing
    h, w, _ = rgba_standing.shape
    breathe_arr = rgba_standing.copy()
    # Slight upward/downward chest breathing shift
    breathe_arr = cv2.resize(breathe_arr, (w, int(h * 1.015)))
    idle2 = fit_to_canvas(breathe_arr, (240, 240), align_bottom=True)
    idle2.save(os.path.join(OUT_ROOT, 'Idle', 'frame_002.png'))
    print("Saved Idle frames (100% transparent, NO shelf)")
    
    # 2. BLINK
    blink_arr = make_blink_frame(rgba_standing)
    blink1 = idle1 # Open eye
    blink2 = fit_to_canvas(blink_arr, (240, 240), align_bottom=True)
    blink1.save(os.path.join(OUT_ROOT, 'Blink', 'frame_001.png'))
    blink2.save(os.path.join(OUT_ROOT, 'Blink', 'frame_002.png'))
    print("Saved Blink frames (natural closed eye)")
    
    # 3. SLEEP
    # Real sleeping macaw pose: closed eye + head tucked down + gentle breathing
    # Use sleep pose with shelf completely cropped out (row < 590)
    sleep_flat_path = r'C:\Users\shahi\.gemini\antigravity\brain\de9d3e6e-0b67-4ac1-be9b-d03149d6aace\macaw_sleeping_flat_1791787229647.jpg'
    bgr_sleep = cv2.imread(sleep_flat_path)
    # Crop strictly above shelf (row 585)
    bgr_sleep_clean = bgr_sleep[:585, :, :]
    rgba_sleep1 = floodfill_remove_bg(bgr_sleep_clean, tolerance=15)
    sleep_frame1 = fit_to_canvas(rgba_sleep1, (240, 240), align_bottom=True)
    sleep_frame1.save(os.path.join(OUT_ROOT, 'Sleep', 'frame_001.png'))
    
    # Sleep Frame 2: Slow breathing cycle (+1.5% height scale)
    sh, sw, _ = rgba_sleep1.shape
    rgba_sleep2 = cv2.resize(rgba_sleep1, (sw, int(sh * 1.018)))
    sleep_frame2 = fit_to_canvas(rgba_sleep2, (240, 240), align_bottom=True)
    sleep_frame2.save(os.path.join(OUT_ROOT, 'Sleep', 'frame_002.png'))
    print("Saved Sleep frames (peaceful closed eyes, zero shelf)")
    
    # 4. HOP
    hop_path = r'C:\Users\shahi\.gemini\antigravity\brain\de9d3e6e-0b67-4ac1-be9b-d03149d6aace\macaw_hopping_up_1791787251091.jpg'
    bgr_hop = cv2.imread(hop_path)
    # Crop above row 712 (strictly above shelf)
    bgr_hop_clean = bgr_hop[:712, :, :]
    rgba_hop = floodfill_remove_bg(bgr_hop_clean, tolerance=15)
    hop1 = fit_to_canvas(rgba_standing, (240, 240), align_bottom=True)
    hop2 = fit_to_canvas(rgba_hop, (240, 240), align_bottom=False) # In air
    hop3 = fit_to_canvas(rgba_standing, (240, 240), align_bottom=True) # Land
    hop1.save(os.path.join(OUT_ROOT, 'Hop', 'frame_001.png'))
    hop2.save(os.path.join(OUT_ROOT, 'Hop', 'frame_002.png'))
    hop3.save(os.path.join(OUT_ROOT, 'Hop', 'frame_003.png'))
    print("Saved Hop frames")
    
    # 5. TAKEOFF
    takeoff_path = r'C:\Users\shahi\.gemini\antigravity\brain\de9d3e6e-0b67-4ac1-be9b-d03149d6aace\macaw_takeoff_wings_1791787182515.jpg'
    bgr_takeoff = cv2.imread(takeoff_path)
    bgr_takeoff_clean = bgr_takeoff[:815, :, :] # Above shelf
    rgba_takeoff = floodfill_remove_bg(bgr_takeoff_clean, tolerance=15)
    to1 = fit_to_canvas(rgba_takeoff, (240, 240), align_bottom=True)
    to2 = fit_to_canvas(rgba_takeoff, (240, 240), align_bottom=False)
    to1.save(os.path.join(OUT_ROOT, 'TakeOff', 'frame_001.png'))
    to2.save(os.path.join(OUT_ROOT, 'TakeOff', 'frame_002.png'))
    print("Saved TakeOff frames")
    
    # 6. LANDING
    landing_path = r'C:\Users\shahi\.gemini\antigravity\brain\de9d3e6e-0b67-4ac1-be9b-d03149d6aace\macaw_landing_brake_1791787205794.jpg'
    bgr_landing = cv2.imread(landing_path)
    bgr_landing_clean = bgr_landing[:780, :, :] # Above bottom shelf
    rgba_landing = floodfill_remove_bg(bgr_landing_clean, tolerance=15)
    land1 = fit_to_canvas(rgba_landing, (240, 240), align_bottom=False)
    land2 = fit_to_canvas(rgba_landing, (240, 240), align_bottom=True)
    land1.save(os.path.join(OUT_ROOT, 'Landing', 'frame_001.png'))
    land2.save(os.path.join(OUT_ROOT, 'Landing', 'frame_002.png'))
    print("Saved Landing frames")
    
    # 7. FLYING (8 frames wing flapping cycle)
    # Ensure all 8 frames have pure alpha transparency and smooth centering
    sheet_path = r'C:\Users\shahi\.gemini\antigravity\brain\de9d3e6e-0b67-4ac1-be9b-d03149d6aace\macaw_flight_cycle_1791787099931.jpg'
    bgr_sheet = cv2.imread(sheet_path)
    # The sheet has 8 birds on white background with zero shelf
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
        crop = bgr_sheet[slc[0].start:slc[0].stop, slc[1].start:slc[1].stop]
        rgba_flight = floodfill_remove_bg(crop, tolerance=15)
        flight_frame = fit_to_canvas(rgba_flight, (240, 240), align_bottom=False)
        flight_frame.save(os.path.join(OUT_ROOT, 'Flying', f'frame_{i+1:03d}.png'))
        print(f"Saved Flying/frame_{i+1:03d}.png")
        
    print("=== All Realistic Assets Processed Successfully ===")

if __name__ == '__main__':
    main()
