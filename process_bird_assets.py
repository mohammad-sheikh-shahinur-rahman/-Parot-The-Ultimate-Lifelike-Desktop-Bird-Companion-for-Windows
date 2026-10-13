import os
import numpy as np
from PIL import Image
from scipy.ndimage import label, find_objects, gaussian_filter

OUT_ROOT = r'K:\Parot\Assets\Birds\Macaw'

def ensure_dirs():
    for sub in ['Idle', 'Blink', 'Hop', 'TakeOff', 'Flying', 'Landing', 'Sleep']:
        p = os.path.join(OUT_ROOT, sub)
        os.makedirs(p, exist_ok=True)

def remove_white_bg(img, thresh=240, feather=1.5):
    """Clean cutout of bird from white/near-white studio background with smooth anti-aliased alpha."""
    im = img.convert('RGBA')
    arr = np.array(im).astype(np.float32)
    r, g, b, a = arr[:,:,0], arr[:,:,1], arr[:,:,2], arr[:,:,3]
    
    # Distance from white (255, 255, 255)
    # White background has small distance to 255
    diff = np.maximum(np.maximum(255 - r, 255 - g), 255 - b)
    
    # Soft alpha ramp
    alpha = np.clip((diff - 12) / 20.0, 0.0, 1.0) * 255.0
    
    # Keep bird solid if colors are vivid
    is_saturated = (np.abs(r - g) > 20) | (np.abs(r - b) > 20) | (np.abs(g - b) > 20)
    alpha[is_saturated] = 255.0
    
    # White face patch of macaw has low saturation, keep if enclosed in bird or R/G/B < 240
    is_darker = (r < thresh) | (g < thresh) | (b < thresh)
    alpha[is_darker] = np.maximum(alpha[is_darker], 255.0 * np.clip((thresh - np.minimum(np.minimum(r, g), b))[is_darker] / 15.0, 0, 1))

    arr[:, :, 3] = alpha
    return Image.fromarray(arr.astype(np.uint8))

def crop_and_pad(img, target_size=(240, 240)):
    """Center cropped sprite with transparent padding."""
    bbox = img.getbbox()
    if not bbox:
        return img.resize(target_size, Image.Resampling.LANCZOS)
    cropped = img.crop(bbox)
    
    w, h = cropped.size
    scale = min((target_size[0] - 20) / w, (target_size[1] - 20) / h)
    new_w = max(1, int(w * scale))
    new_h = max(1, int(h * scale))
    resized = cropped.resize((new_w, new_h), Image.Resampling.LANCZOS)
    
    res = Image.new('RGBA', target_size, (0, 0, 0, 0))
    paste_x = (target_size[0] - new_w) // 2
    paste_y = (target_size[1] - new_h) // 2
    res.paste(resized, (paste_x, paste_y), resized)
    return res

def process_flight_sheet():
    sheet_path = r'C:\Users\shahi\.gemini\antigravity\brain\de9d3e6e-0b67-4ac1-be9b-d03149d6aace\macaw_flight_cycle_1791787099931.jpg'
    if not os.path.exists(sheet_path):
        print("Sheet not found")
        return
    im = Image.open(sheet_path)
    arr = np.array(im)
    # Mask of non-white areas
    mask = ~((arr[:,:,0] > 240) & (arr[:,:,1] > 240) & (arr[:,:,2] > 240))
    lbl, num = label(mask)
    objs = find_objects(lbl)
    
    birds = []
    for slc in objs:
        h = slc[0].stop - slc[0].start
        w = slc[1].stop - slc[1].start
        if h > 80 and w > 80:
            birds.append(slc)
            
    # Sort birds by vertical (row) then horizontal (col)
    birds.sort(key=lambda s: (s[0].start // 400, s[1].start))
    print(f"Extracted {len(birds)} flight frames")
    
    flying_dir = os.path.join(OUT_ROOT, 'Flying')
    for i, slc in enumerate(birds[:8]):
        cropped = im.crop((slc[1].start, slc[0].start, slc[1].stop, slc[0].stop))
        transparent = remove_white_bg(cropped)
        final_frame = crop_and_pad(transparent, (240, 240))
        frame_name = f"frame_{i+1:03d}.png"
        final_frame.save(os.path.join(flying_dir, frame_name))
        print(f"Saved {frame_name}")

def remove_pedestal_and_extract(img_path, out_folder, frame_names, crop_bottom_pct=0.0):
    im = Image.open(img_path)
    w, h = im.size
    if crop_bottom_pct > 0:
        im = im.crop((0, 0, w, int(h * (1.0 - crop_bottom_pct))))
    
    transparent = remove_white_bg(im)
    # Find bounding box of the bird
    bbox = transparent.getbbox()
    if bbox:
        # If there's pedestal below feet, detect where feet end
        transparent = transparent.crop(bbox)
        
    for name in frame_names:
        final_frame = crop_and_pad(transparent, (240, 240))
        final_frame.save(os.path.join(OUT_ROOT, out_folder, name))
        print(f"Saved {out_folder}/{name}")

def main():
    ensure_dirs()
    print("Processing Flight sheet...")
    process_flight_sheet()
    
    print("Processing Standing/Idle...")
    # Idle
    idle_path = r'C:\Users\shahi\.gemini\antigravity\brain\de9d3e6e-0b67-4ac1-be9b-d03149d6aace\macaw_standing_idle_1791787130659.jpg'
    remove_pedestal_and_extract(idle_path, 'Idle', ['frame_001.png', 'frame_002.png'], crop_bottom_pct=0.28)
    
    # Blink / Look
    look_path = r'C:\Users\shahi\.gemini\antigravity\brain\de9d3e6e-0b67-4ac1-be9b-d03149d6aace\macaw_standing_look_1791787153685.jpg'
    remove_pedestal_and_extract(look_path, 'Blink', ['frame_001.png', 'frame_002.png'], crop_bottom_pct=0.34)
    
    # Hop
    hop_path = r'C:\Users\shahi\.gemini\antigravity\brain\de9d3e6e-0b67-4ac1-be9b-d03149d6aace\macaw_hopping_up_1791787251091.jpg'
    remove_pedestal_and_extract(hop_path, 'Hop', ['frame_001.png', 'frame_002.png', 'frame_003.png'], crop_bottom_pct=0.24)
    
    # TakeOff
    takeoff_path = r'C:\Users\shahi\.gemini\antigravity\brain\de9d3e6e-0b67-4ac1-be9b-d03149d6aace\macaw_takeoff_wings_1791787182515.jpg'
    remove_pedestal_and_extract(takeoff_path, 'TakeOff', ['frame_001.png', 'frame_002.png'], crop_bottom_pct=0.18)
    
    # Landing
    landing_path = r'C:\Users\shahi\.gemini\antigravity\brain\de9d3e6e-0b67-4ac1-be9b-d03149d6aace\macaw_landing_brake_1791787205794.jpg'
    remove_pedestal_and_extract(landing_path, 'Landing', ['frame_001.png', 'frame_002.png'], crop_bottom_pct=0.18)
    
    # Sleep
    sleep_path = r'C:\Users\shahi\.gemini\antigravity\brain\de9d3e6e-0b67-4ac1-be9b-d03149d6aace\macaw_sleeping_flat_1791787229647.jpg'
    remove_pedestal_and_extract(sleep_path, 'Sleep', ['frame_001.png', 'frame_002.png'], crop_bottom_pct=0.20)
    
    print("All frames generated successfully!")

if __name__ == '__main__':
    main()
