import os
import cv2
import numpy as np
from PIL import Image

OUT_ROOT = r'K:\Parot\Assets\Birds\Macaw'

def ensure_dirs():
    for sub in ['Idle', 'Blink', 'Hop', 'TakeOff', 'Flying', 'Landing', 'Sleep']:
        p = os.path.join(OUT_ROOT, sub)
        os.makedirs(p, exist_ok=True)

def fit_to_canvas(rgba_img, target_size=(260, 220), align_bottom=True, y_offset=0):
    """Centers and scales bird into canvas with claws resting exactly on bottom edge."""
    pil_img = Image.fromarray(rgba_img) if isinstance(rgba_img, np.ndarray) else rgba_img
    bbox = pil_img.getbbox()
    if not bbox:
        return pil_img.resize(target_size, Image.Resampling.LANCZOS)
        
    cropped = pil_img.crop(bbox)
    w, h = cropped.size
    
    max_w = target_size[0] - 12
    max_h = target_size[1] - 12
    scale = min(max_w / w, max_h / h)
    new_w = max(1, int(w * scale))
    new_h = max(1, int(h * scale))
    resized = cropped.resize((new_w, new_h), Image.Resampling.LANCZOS)
    
    canvas = Image.new('RGBA', target_size, (0, 0, 0, 0))
    paste_x = (target_size[0] - new_w) // 2
    if align_bottom:
        # Claws firmly planted at the bottom with 5px margin
        paste_y = target_size[1] - new_h - 5 + y_offset
    else:
        paste_y = (target_size[1] - new_h) // 2 + y_offset
        
    canvas.paste(resized, (paste_x, paste_y), resized)
    return canvas

def make_blink_variant(rgba_img):
    """Draws a closed eyelid seamlessly matching the surrounding facial skin."""
    blink = rgba_img.copy()
    # Surrounding pale skin: RGB [232, 225, 220]
    cv2.circle(blink, (811, 39), 11, (232, 225, 220, 255), -1)
    # Delicate eyelid crease line
    cv2.ellipse(blink, (811, 39), (10, 3), 0, 0, 360, (190, 180, 175, 255), 2)
    return blink

def clean_cell(cell):
    alpha = (cell[:, :, 3] > 10).astype(np.uint8)
    num, labels, stats, _ = cv2.connectedComponentsWithStats(alpha)
    if num <= 1:
        return cell
    largest_idx = 1 + np.argmax(stats[1:, cv2.CC_STAT_AREA])
    cleaned = cell.copy()
    cleaned[labels != largest_idx] = 0
    return cleaned

def generate_assets():
    ensure_dirs()
    print("=== Generating High Definition Complete Red Macaw Assets ===")

    # 1. Base Standing Macaw with full intact legs and claws
    base_pil = Image.open(r'K:\Parot\cropped_perfect_macaw.png')
    base_rgba = np.array(base_pil)
    h, w, _ = base_rgba.shape

    # IDLE FRAMES
    idle_frame1 = fit_to_canvas(base_rgba, (260, 220), align_bottom=True)
    breathe_arr = cv2.resize(base_rgba, (w, int(h * 1.015)))
    idle_frame2 = fit_to_canvas(breathe_arr, (260, 220), align_bottom=True)

    idle_frame1.save(os.path.join(OUT_ROOT, 'Idle', 'frame_001.png'))
    idle_frame2.save(os.path.join(OUT_ROOT, 'Idle', 'frame_002.png'))
    print("Saved Idle frames")

    # BLINK FRAMES
    blink_arr = make_blink_variant(base_rgba)
    blink_frame1 = idle_frame1
    blink_frame2 = fit_to_canvas(blink_arr, (260, 220), align_bottom=True)
    blink_frame1.save(os.path.join(OUT_ROOT, 'Blink', 'frame_001.png'))
    blink_frame2.save(os.path.join(OUT_ROOT, 'Blink', 'frame_002.png'))
    print("Saved Blink frames")

    # SLEEP FRAMES
    sleep_frame1 = fit_to_canvas(blink_arr, (260, 220), align_bottom=True)
    sleep_breathe = cv2.resize(blink_arr, (w, int(h * 1.015)))
    sleep_frame2 = fit_to_canvas(sleep_breathe, (260, 220), align_bottom=True)
    sleep_frame1.save(os.path.join(OUT_ROOT, 'Sleep', 'frame_001.png'))
    sleep_frame2.save(os.path.join(OUT_ROOT, 'Sleep', 'frame_002.png'))
    print("Saved Sleep frames")

    # HOP FRAMES
    hop_crouch = cv2.resize(base_rgba, (w, int(h * 0.94)))
    hop_frame1 = fit_to_canvas(hop_crouch, (260, 220), align_bottom=True)
    hop_frame2 = fit_to_canvas(base_rgba, (260, 220), align_bottom=True, y_offset=-22)
    hop_frame3 = idle_frame1
    hop_frame1.save(os.path.join(OUT_ROOT, 'Hop', 'frame_001.png'))
    hop_frame2.save(os.path.join(OUT_ROOT, 'Hop', 'frame_002.png'))
    hop_frame3.save(os.path.join(OUT_ROOT, 'Hop', 'frame_003.png'))
    print("Saved Hop frames")

    # TAKEOFF FRAMES
    to_spread = cv2.resize(base_rgba, (w, int(h * 1.05)))
    to_frame1 = fit_to_canvas(hop_crouch, (260, 220), align_bottom=True)
    to_frame2 = fit_to_canvas(to_spread, (260, 220), align_bottom=True, y_offset=-16)
    to_frame1.save(os.path.join(OUT_ROOT, 'TakeOff', 'frame_001.png'))
    to_frame2.save(os.path.join(OUT_ROOT, 'TakeOff', 'frame_002.png'))
    print("Saved TakeOff frames")

    # LANDING FRAMES
    land_frame1 = fit_to_canvas(to_spread, (260, 220), align_bottom=True, y_offset=-14)
    land_frame2 = idle_frame1
    land_frame1.save(os.path.join(OUT_ROOT, 'Landing', 'frame_001.png'))
    land_frame2.save(os.path.join(OUT_ROOT, 'Landing', 'frame_002.png'))
    print("Saved Landing frames")

    # FLYING FRAMES (8-frame wing flap loop from rembg_flight_sheet)
    sheet = np.array(Image.open(r'K:\Parot\rembg_flight_sheet.png'))
    top = sheet[:450, :]

    # 4 clean birds:
    # Bird 1: [0, 275]
    # Bird 2: [275, 500]
    # Bird 3: [490, 735]
    # Bird 4: [735, 1024]
    flight_poses = []
    
    # Pose 1:
    c1 = clean_cell(top[:, 0:275])
    p1 = Image.fromarray(c1).crop(Image.fromarray(c1).getbbox()).transpose(Image.FLIP_LEFT_RIGHT)
    flight_poses.append(p1)

    # Pose 2:
    c2 = clean_cell(top[:, 275:500])
    p2 = Image.fromarray(c2).crop(Image.fromarray(c2).getbbox()).transpose(Image.FLIP_LEFT_RIGHT)
    flight_poses.append(p2)

    # Pose 3:
    c3 = clean_cell(top[:, 490:735])
    # trim neighbor tail tip if any
    p3_arr = np.array(Image.fromarray(c3).crop(Image.fromarray(c3).getbbox()).transpose(Image.FLIP_LEFT_RIGHT))
    if p3_arr.shape[1] > 208:
        p3_arr = p3_arr[:, :208]
    p3 = Image.fromarray(p3_arr)
    flight_poses.append(p3)

    # Pose 4:
    c4 = clean_cell(top[:, 735:1024])
    p4_arr = np.array(Image.fromarray(c4).crop(Image.fromarray(c4).getbbox()).transpose(Image.FLIP_LEFT_RIGHT))
    if p4_arr.shape[1] > 246:
        p4_arr = p4_arr[:, :246]
    p4 = Image.fromarray(p4_arr)
    flight_poses.append(p4)

    # 8-frame harmonic flapping sequence:
    # 0 -> 1 -> 2 -> 3 -> 3 -> 2 -> 1 -> 0
    seq = [0, 1, 2, 3, 3, 2, 1, 0]
    for frame_idx, pose_idx in enumerate(seq):
        f = fit_to_canvas(flight_poses[pose_idx], (260, 220), align_bottom=False)
        out_path = os.path.join(OUT_ROOT, 'Flying', f'frame_{frame_idx+1:03d}.png')
        f.save(out_path)
        print(f"Saved Flying frame {frame_idx+1:03d}")

    print("=== All Final Red Macaw Assets Successfully Generated! ===")

if __name__ == '__main__':
    generate_assets()
