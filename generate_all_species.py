import os
import cv2
import numpy as np
from PIL import Image

BASE_MACAW = r'K:\Parot\Assets\Birds\Macaw'
ROOT_BIRDS = r'K:\Parot\Assets\Birds'

def ensure_folders(species):
    for sub in ['Idle', 'Blink', 'Sleep', 'Hop', 'TakeOff', 'Landing', 'Flying']:
        p = os.path.join(ROOT_BIRDS, species, sub)
        os.makedirs(p, exist_ok=True)

def transform_green_parakeet(rgba):
    """Vibrant emerald green Indian Rose-ringed Parakeet."""
    hsv = cv2.cvtColor(rgba[:,:,:3], cv2.COLOR_RGB2HSV).astype(float)
    alpha = rgba[:,:,3]
    
    is_red = ((hsv[:,:,0] < 22) | (hsv[:,:,0] > 165)) & (hsv[:,:,1] > 45)
    is_blue = (hsv[:,:,0] >= 90) & (hsv[:,:,0] <= 130) & (hsv[:,:,1] > 40)
    is_yellow = (hsv[:,:,0] >= 22) & (hsv[:,:,0] <= 45) & (hsv[:,:,1] > 40)
    
    # Body green
    hsv[is_red, 0] = 58
    hsv[is_red, 1] = np.clip(hsv[is_red, 1] * 1.15, 60, 255)
    
    # Wings green
    hsv[is_blue, 0] = 74
    hsv[is_yellow, 0] = 50
    
    rgb = cv2.cvtColor(hsv.astype(np.uint8), cv2.COLOR_HSV2RGB)
    
    # Rose ring neck collar
    collar_mask = (alpha > 180)
    for cy in range(80, 88):
        for cx in range(195, 222):
            if cy < rgba.shape[0] and cx < rgba.shape[1] and collar_mask[cy, cx]:
                rgb[cy, cx] = (rgb[cy, cx] * 0.45 + np.array([230, 80, 115]) * 0.55).astype(np.uint8)
                
    return np.dstack([rgb, alpha])

def transform_blue_macaw(rgba):
    """Majestic Blue and Gold Macaw."""
    hsv = cv2.cvtColor(rgba[:,:,:3], cv2.COLOR_RGB2HSV).astype(float)
    alpha = rgba[:,:,3]
    
    is_red = ((hsv[:,:,0] < 22) | (hsv[:,:,0] > 165)) & (hsv[:,:,1] > 45)
    
    is_chest = np.zeros(rgba.shape[:2], dtype=bool)
    for y in range(90, min(rgba.shape[0], 180)):
        for x in range(160, min(rgba.shape[1], 240)):
            is_chest[y, x] = True
            
    is_back = is_red & (~is_chest)
    hsv[is_back, 0] = 108 # Azure blue
    hsv[is_back, 1] = np.clip(hsv[is_back, 1] * 1.1, 0, 255)
    
    is_belly = is_red & is_chest
    hsv[is_belly, 0] = 25 # Golden amber
    hsv[is_belly, 1] = np.clip(hsv[is_belly, 1] * 1.25, 100, 255)
    
    rgb = cv2.cvtColor(hsv.astype(np.uint8), cv2.COLOR_HSV2RGB)
    return np.dstack([rgb, alpha])

def transform_cockatiel(rgba):
    """Soft grey Cockatiel with sunny yellow face and orange cheek blush."""
    hsv = cv2.cvtColor(rgba[:,:,:3], cv2.COLOR_RGB2HSV).astype(float)
    alpha = rgba[:,:,3]
    
    # Desaturate body into warm soft charcoal / slate grey
    hsv[:, :, 1] = hsv[:, :, 1] * 0.15 # Mostly greyscale body
    hsv[:, :, 2] = np.clip(hsv[:, :, 2] * 0.9 + 25, 0, 245)
    
    # Face and crest region: sunny yellow
    for y in range(20, min(rgba.shape[0], 80)):
        for x in range(185, min(rgba.shape[1], 245)):
            if alpha[y, x] > 150:
                hsv[y, x, 0] = 25
                hsv[y, x, 1] = 175
                hsv[y, x, 2] = 240
                
    rgb = cv2.cvtColor(hsv.astype(np.uint8), cv2.COLOR_HSV2RGB)
    
    # Orange cheek circle at (212, 60)
    cv2.circle(rgb, (212, 60), 7, (238, 105, 30), -1)
    
    return np.dstack([rgb, alpha])

def transform_cockatoo(rgba):
    """Silky pure white Sulphur-crested Cockatoo with yellow crest."""
    hsv = cv2.cvtColor(rgba[:,:,:3], cv2.COLOR_RGB2HSV).astype(float)
    alpha = rgba[:,:,3]
    
    # Pure silky white feathers
    hsv[:, :, 1] = hsv[:, :, 1] * 0.08
    hsv[:, :, 2] = np.clip(hsv[:, :, 2] * 0.4 + 180, 0, 255)
    
    # Sulfur yellow crest at top head
    for y in range(15, min(rgba.shape[0], 45)):
        for x in range(185, min(rgba.shape[1], 230)):
            if alpha[y, x] > 150:
                hsv[y, x, 0] = 28
                hsv[y, x, 1] = 210
                hsv[y, x, 2] = 250
                
    rgb = cv2.cvtColor(hsv.astype(np.uint8), cv2.COLOR_HSV2RGB)
    return np.dstack([rgb, alpha])

def build_all_species():
    species_map = {
        'GreenParakeet': transform_green_parakeet,
        'BlueMacaw': transform_blue_macaw,
        'Cockatiel': transform_cockatiel,
        'Cockatoo': transform_cockatoo
    }
    
    # Also create ScarletMacaw folder pointing to Macaw
    ensure_folders('ScarletMacaw')
    for sub in ['Idle', 'Blink', 'Sleep', 'Hop', 'TakeOff', 'Landing', 'Flying']:
        src_dir = os.path.join(BASE_MACAW, sub)
        dst_dir = os.path.join(ROOT_BIRDS, 'ScarletMacaw', sub)
        for f in os.listdir(src_dir):
            if f.endswith('.png'):
                Image.open(os.path.join(src_dir, f)).save(os.path.join(dst_dir, f))
    print("Populated ScarletMacaw")

    for sp_name, sp_func in species_map.items():
        ensure_folders(sp_name)
        print(f"Generating species: {sp_name}...")
        for sub in ['Idle', 'Blink', 'Sleep', 'Hop', 'TakeOff', 'Landing', 'Flying']:
            src_dir = os.path.join(BASE_MACAW, sub)
            dst_dir = os.path.join(ROOT_BIRDS, sp_name, sub)
            for f in os.listdir(src_dir):
                if f.endswith('.png'):
                    im = np.array(Image.open(os.path.join(src_dir, f)))
                    res = sp_func(im)
                    Image.fromarray(res).save(os.path.join(dst_dir, f))
        print(f"Species {sp_name} successfully generated!")

if __name__ == '__main__':
    build_all_species()
