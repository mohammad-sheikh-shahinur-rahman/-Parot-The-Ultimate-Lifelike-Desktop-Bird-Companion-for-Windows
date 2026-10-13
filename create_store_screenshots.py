import os
from PIL import Image, ImageDraw, ImageFont

output_dir = r"K:\Parot\Store-Screenshots"
os.makedirs(output_dir, exist_ok=True)

WIDTH, HEIGHT = 1920, 1080

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

def draw_header(draw, title, subtitle):
    # Top banner bar
    draw.rectangle([0, 0, WIDTH, 120], fill=(15, 23, 42, 230))
    draw.rectangle([0, 118, WIDTH, 122], fill=(59, 130, 246, 255))
    
    font_title = get_font(42, bold=True)
    font_sub = get_font(24, bold=False)
    
    draw.text((70, 25), title, fill=(255, 255, 255), font=font_title)
    draw.text((70, 75), subtitle, fill=(148, 163, 184), font=font_sub)
    
    # Parot branding badge right side
    draw.rounded_rectangle([WIDTH - 300, 30, WIDTH - 70, 90], radius=16, fill=(30, 41, 59), outline=(71, 85, 105), width=2)
    font_badge = get_font(28, bold=True)
    draw.text((WIDTH - 250, 42), "Parot 🦜", fill=(56, 189, 248), font=font_badge)

def draw_taskbar(draw):
    # Windows 11 style taskbar at bottom
    tb_top = HEIGHT - 70
    draw.rectangle([0, tb_top, WIDTH, HEIGHT], fill=(18, 22, 33, 245))
    draw.line([0, tb_top, WIDTH, tb_top], fill=(45, 55, 72), width=1)
    
    # Taskbar icons center
    center_x = WIDTH // 2
    icons = ["🪟", "🔍", "📂", "🌐", "💻", "🎵", "🦜"]
    for i, ico in enumerate(icons):
        ix = center_x - 150 + (i * 45)
        bg_col = (45, 55, 75) if ico == "🦜" else (28, 35, 48)
        draw.rounded_rectangle([ix - 16, tb_top + 12, ix + 20, tb_top + 50], radius=8, fill=bg_col)
        draw.text((ix - 8, tb_top + 16), ico, font=get_font(20))

def draw_speech_bubble(draw, x, y, text, title="Parot", width=360, height=110):
    # Bubble background
    draw.rounded_rectangle([x, y, x + width, y + height], radius=18, fill=(255, 255, 255), outline=(226, 232, 240), width=2)
    # Triangle pointer
    draw.polygon([(x + width // 2 - 15, y + height), (x + width // 2 + 15, y + height), (x + width // 2, y + height + 16)], fill=(255, 255, 255))
    
    # Header inside bubble
    draw.text((x + 20, y + 14), title, fill=(225, 29, 72), font=get_font(20, bold=True))
    draw.text((x + 20, y + 42), text, fill=(30, 41, 59), font=get_font(22, bold=True))

def create_desktop_base():
    img = Image.new("RGBA", (WIDTH, HEIGHT), (11, 15, 25, 255))
    draw = ImageDraw.Draw(img)
    # Background gradient
    for y in range(HEIGHT):
        r = int(12 + (y / HEIGHT) * 16)
        g = int(18 + (y / HEIGHT) * 24)
        b = int(32 + (y / HEIGHT) * 38)
        draw.line([(0, y), (WIDTH, y)], fill=(r, g, b))
    
    # Glow circle
    for rad in range(350, 0, -20):
        alpha = int((1.0 - (rad / 350.0)) * 25)
        draw.ellipse([WIDTH//2 - rad, HEIGHT//2 - rad + 80, WIDTH//2 + rad, HEIGHT//2 + rad + 80], outline=(59, 130, 246, alpha), width=10)
    
    return img

# ----------------- SCREENSHOT 1: Desktop Companion -----------------
def make_screenshot_1():
    img = create_desktop_base()
    draw = ImageDraw.Draw(img)
    draw_header(draw, "Lifelike Desktop Bird Companion", "Multi-Frame Photographic Animation • Autonomous Exploration • Taskbar & Titlebar Perching")
    draw_taskbar(draw)
    
    # Bird on taskbar
    macaw_path = r"K:\Parot\Assets\Birds\ScarletMacaw\Idle\frame_001.png"
    if os.path.exists(macaw_path):
        bird = Image.open(macaw_path).convert("RGBA")
        bird = bird.resize((260, 260), Image.Resampling.LANCZOS)
        bx, by = WIDTH - 420, HEIGHT - 315
        img.paste(bird, (bx, by), bird)
        
        # Speech bubble above bird
        draw_speech_bubble(draw, bx - 140, by - 140, "Hello! I am Parot, your lively\ndesktop friend! 🦜✨", title="Parot Companion", width=380, height=110)
    
    # Feature Pills
    features = [
        ("✨ 100% Lifelike Avian Behaviors", "Preening, stretching wings, scratching head & graceful walking"),
        ("🐾 Dual-Frequency Footsteps", "Audible left and right claw taps as it explores your screen"),
        ("🛌 Restful Habits & Power Naps", "Natural daytime naps, yawns, and nocturnal sleeping schedule"),
        ("⚡ Lightweight & Smooth", "Hardware-accelerated 60 FPS graphics with ultra-low CPU footprint")
    ]
    py = 200
    for title, desc in features:
        draw.rounded_rectangle([120, py, 880, py + 95], radius=16, fill=(24, 32, 47, 220), outline=(51, 65, 85), width=2)
        draw.text((150, py + 16), title, fill=(56, 189, 248), font=get_font(26, bold=True))
        draw.text((150, py + 52), desc, fill=(203, 213, 225), font=get_font(20))
        py += 120

    img.save(os.path.join(output_dir, "Screenshot1_DesktopCompanion.png"))
    print("Saved Screenshot 1")

# ----------------- SCREENSHOT 2: Window Perching & Wellness -----------------
def make_screenshot_2():
    img = create_desktop_base()
    draw = ImageDraw.Draw(img)
    draw_header(draw, "Smart Window Perching & Wellness Reminders", "Perches on Active Windows • Pomodoro Focus Mode • 20-20-20 Eye Breaks & Hydration")
    draw_taskbar(draw)
    
    # Draw simulated application window
    wx, wy, ww, wh = 140, 180, 1050, 720
    # Window shadow
    draw.rounded_rectangle([wx - 10, wy - 10, wx + ww + 10, wy + wh + 10], radius=16, fill=(0, 0, 0, 80))
    # Window body
    draw.rounded_rectangle([wx, wy, wx + ww, wy + wh], radius=12, fill=(30, 41, 59), outline=(71, 85, 105), width=2)
    # Window titlebar
    draw.rounded_rectangle([wx, wy, wx + ww, wy + 48], radius=12, fill=(15, 23, 42))
    # Window controls
    draw.ellipse([wx + 18, wy + 16, wx + 32, wy + 30], fill=(239, 68, 68))
    draw.ellipse([wx + 38, wy + 16, wx + 52, wy + 30], fill=(234, 179, 8))
    draw.ellipse([wx + 58, wy + 16, wx + 72, wy + 30], fill=(34, 197, 94))
    draw.text((wx + 90, wy + 14), "Visual Studio Code — Project Workspace", fill=(148, 163, 184), font=get_font(18))
    
    # Code content inside window
    code_lines = [
        "// Parot is happily perching right on this titlebar!",
        "public async Task WorkWithFocus()",
        "{",
        "    await PomodoroTimer.StartFocusSessionAsync(minutes: 25);",
        "    Parot.CheerAndAccompany(mode: CodingCompanion);",
        "    Logger.LogInfo(\"Healthy posture & eye break reminder active!\");",
        "}"
    ]
    cy = wy + 80
    for line in code_lines:
        color = (147, 197, 253) if "await" in line or "public" in line else (226, 232, 240)
        if "//" in line: color = (100, 116, 139)
        draw.text((wx + 40, cy), line, fill=color, font=get_font(22))
        cy += 40

    # Bird perching on top of the window titlebar!
    macaw_path = r"K:\Parot\Assets\Birds\ScarletMacaw\Idle\frame_001.png"
    if os.path.exists(macaw_path):
        bird = Image.open(macaw_path).convert("RGBA")
        bird = bird.resize((230, 230), Image.Resampling.LANCZOS)
        bx, by = wx + ww - 260, wy - 195
        img.paste(bird, (bx, by), bird)
        draw_speech_bubble(draw, bx - 160, by - 120, "25m Focus done! Look 20ft\naway & drink water! 🍅💧", title="Pomodoro & Eye Rest", width=360, height=105)

    # Right side wellness cards
    cards = [
        ("🍅 Pomodoro Focus Timer", "25-minute productivity sprints with 5-minute dancing breaks."),
        ("👁️ 20-20-20 Eye Rest Rule", "Prevents computer eye strain with timely gentle prompts."),
        ("💧 Hydration & Posture Checks", "Ensures you drink enough water and maintain healthy spine posture."),
        ("📊 Daily Activity Summary", "Tracks active screen time and friendship levels in beautiful charts.")
    ]
    ry = 200
    for title, desc in cards:
        draw.rounded_rectangle([1250, ry, 1850, ry + 120], radius=16, fill=(24, 32, 47, 220), outline=(51, 65, 85), width=2)
        draw.text((1280, ry + 20), title, fill=(244, 63, 94), font=get_font(24, bold=True))
        draw.text((1280, ry + 60), desc, fill=(203, 213, 225), font=get_font(18))
        ry += 150

    img.save(os.path.join(output_dir, "Screenshot2_WindowPerching_Pomodoro.png"))
    print("Saved Screenshot 2")

# ----------------- SCREENSHOT 3: Flight Physics -----------------
def make_screenshot_3():
    img = create_desktop_base()
    draw = ImageDraw.Draw(img)
    draw_header(draw, "60 FPS Curved Flight Physics & Multi-Monitor Roaming", "Dynamic Bezier Curves • Dynamic Banking Angle • Natural Take-Off & Soft Brake Landings")
    draw_taskbar(draw)
    
    # Draw Bezier arc flight path
    arc_points = [
        (250, 750), (450, 480), (700, 320), (1000, 280), (1300, 340), (1550, 560), (1700, 780)
    ]
    for i in range(len(arc_points) - 1):
        draw.line([arc_points[i], arc_points[i+1]], fill=(56, 189, 248, 160), width=4)
    
    # Flight frames along the arc
    flying_files = [
        (r"K:\Parot\Assets\Birds\ScarletMacaw\TakeOff\frame_001.png", (220, 680), -15),
        (r"K:\Parot\Assets\Birds\ScarletMacaw\Flying\frame_002.png", (650, 260), 10),
        (r"K:\Parot\Assets\Birds\ScarletMacaw\Flying\frame_005.png", (980, 220), 0),
        (r"K:\Parot\Assets\Birds\ScarletMacaw\Landing\frame_001.png", (1480, 500), -20),
        (r"K:\Parot\Assets\Birds\ScarletMacaw\Idle\frame_001.png", (1630, 700), 0),
    ]
    
    for fpath, pos, rot in flying_files:
        if os.path.exists(fpath):
            bird = Image.open(fpath).convert("RGBA")
            bird = bird.resize((210, 210), Image.Resampling.LANCZOS)
            if rot != 0:
                bird = bird.rotate(rot, expand=True, resample=Image.Resampling.BICUBIC)
            img.paste(bird, pos, bird)

    # Info boxes
    draw.rounded_rectangle([150, 200, 600, 380], radius=16, fill=(15, 23, 42, 230), outline=(56, 189, 248), width=2)
    draw.text((180, 225), "✈️ Physics Flight Highlights", fill=(56, 189, 248), font=get_font(26, bold=True))
    draw.text((180, 275), "• True 8-frame aerodynamic cycle", fill=(226, 232, 240), font=get_font(20))
    draw.text((180, 310), "• Dynamic banking (-25° to +25°)", fill=(226, 232, 240), font=get_font(20))
    draw.text((180, 345), "• Seamless multi-monitor traversal", fill=(226, 232, 240), font=get_font(20))

    img.save(os.path.join(output_dir, "Screenshot3_Flight_Physics.png"))
    print("Saved Screenshot 3")

# ----------------- SCREENSHOT 4: Species & Accessories -----------------
def make_screenshot_4():
    img = create_desktop_base()
    draw = ImageDraw.Draw(img)
    draw_header(draw, "5 Beautiful Bird Species & Fun Custom Accessories", "Scarlet Macaw • Green Parakeet • Blue Macaw • Cockatiel • White Cockatoo")
    draw_taskbar(draw)
    
    species = [
        ("Scarlet Macaw", r"K:\Parot\Assets\Birds\ScarletMacaw\Idle\frame_001.png", "👑 Royal Crown"),
        ("Green Parakeet", r"K:\Parot\Assets\Birds\GreenParakeet\Idle\frame_001.png", "🏴‍☠️ Pirate Hat"),
        ("Blue & Gold", r"K:\Parot\Assets\Birds\BlueMacaw\Idle\frame_001.png", "😎 Sunglasses"),
        ("Cockatiel", r"K:\Parot\Assets\Birds\Cockatiel\Idle\frame_001.png", "🎀 Cute Bowtie"),
        ("White Cockatoo", r"K:\Parot\Assets\Birds\Cockatoo\Idle\frame_001.png", "🎩 Gentleman Top Hat")
    ]
    
    card_width = 310
    start_x = 110
    for i, (name, path, acc) in enumerate(species):
        cx = start_x + (i * 350)
        cy = 240
        # Card background
        draw.rounded_rectangle([cx, cy, cx + card_width, cy + 540], radius=18, fill=(24, 32, 47, 230), outline=(71, 85, 105), width=2)
        draw.text((cx + 25, cy + 25), name, fill=(255, 255, 255), font=get_font(26, bold=True))
        draw.text((cx + 25, cy + 65), acc, fill=(56, 189, 248), font=get_font(20))
        
        if os.path.exists(path):
            bird = Image.open(path).convert("RGBA")
            bird = bird.resize((240, 240), Image.Resampling.LANCZOS)
            img.paste(bird, (cx + 35, cy + 160), bird)

    img.save(os.path.join(output_dir, "Screenshot4_Species_Accessories.png"))
    print("Saved Screenshot 4")

# ----------------- SCREENSHOT 5: Mini-Games, Birdcage & Voice -----------------
def make_screenshot_5():
    img = create_desktop_base()
    draw = ImageDraw.Draw(img)
    draw_header(draw, "Interactive Mini-Games, Voice Control & Birdcage Mode", "Treat Catching • Bouncy Football • Laser Pointer • Voice Commands • Golden Birdcage")
    draw_taskbar(draw)
    
    # Feature Showcase Columns
    columns = [
        ("🏰 Golden Birdcage Mode", [
            "• Keep Parot securely perched in a golden cage",
            "• Peaceful in-cage swings, singing & eating",
            "• Drag and place anywhere on multi-monitors",
            "• One-click lock & release via menu or voice"
        ]),
        ("🎮 Desktop Mini-Games", [
            "• 🌶️ Throw Treats: Toss chilis & seeds to catch",
            "• ⚽ Bouncy Ball: Push & kick football with bird",
            "• 🔴 Laser Pointer: Chase glowing red laser",
            "• 🪞 Desktop Mirror: Dances to its own reflection"
        ]),
        ("🎙️ Voice & Sound System", [
            "• Offline Voice Commands: 'fly', 'dance', 'sleep'",
            "• Microphone Echo: Mimics speech in parrot pitch",
            "• Live Music Beats: Dances to Spotify & YouTube",
            "• 5 Languages: English, Bengali, Hindi, Spanish"
        ])
    ]
    
    col_width = 520
    cx = 120
    for title, items in columns:
        draw.rounded_rectangle([cx, 220, cx + col_width, 820], radius=18, fill=(24, 32, 47, 230), outline=(71, 85, 105), width=2)
        draw.text((cx + 30, 260), title, fill=(251, 191, 36), font=get_font(28, bold=True))
        
        iy = 330
        for item in items:
            draw.text((cx + 30, iy), item, fill=(226, 232, 240), font=get_font(22))
            iy += 60
            
        cx += 580

    img.save(os.path.join(output_dir, "Screenshot5_MiniGames_Voice.png"))
    print("Saved Screenshot 5")

make_screenshot_1()
make_screenshot_2()
make_screenshot_3()
make_screenshot_4()
make_screenshot_5()
print("All 5 store screenshots generated successfully in K:\\Parot\\Store-Screenshots!")
