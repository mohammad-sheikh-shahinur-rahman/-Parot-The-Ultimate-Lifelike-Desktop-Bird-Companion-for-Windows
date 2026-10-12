using System;

namespace Parot.Models;

public class AppSettings
{
    // Appearance & Scale
    public double Scale { get; set; } = 1.0; // 0.7 to 1.6
    public bool AlwaysOnTop { get; set; } = true;
    public bool GhostMode { get; set; } = false; // Click-through

    // Bird Skin & Accessories
    public string Skin { get; set; } = "ScarletMacaw"; // "ScarletMacaw", "GreenParakeet", "Cockatiel", "BlueMacaw"
    public string Accessory { get; set; } = "None"; // "None", "Glasses", "Sunglasses", "Scarf", "TopHat", "PartyHat", "Umbrella"

    // Pet Behavior & Perching
    public bool AllowWalking { get; set; } = true;
    public string PerchMode { get; set; } = "Desktop"; // "Desktop", "ActiveWindow"
    public int AutoSleepMinutes { get; set; } = 3; // 0 = disabled
    public bool IdleChirping { get; set; } = true;
    public bool DanceToMusic { get; set; } = true;
    public bool NightModeAuto { get; set; } = true;
    public string Language { get; set; } = "Bilingual"; // "Bilingual", "Bengali", "English"

    // Sound & Voice
    public bool SoundEnabled { get; set; } = true;
    public double SoundVolume { get; set; } = 0.8; // 0.0 to 1.0
    public bool VoiceMimicEnabled { get; set; } = true;
    public int VoicePitchRate { get; set; } = 5; // Fast bird-like voice

    // Health & Screen-Time Reminders
    public bool ScreenBreakEnabled { get; set; } = true;
    public int ScreenBreakIntervalMinutes { get; set; } = 25; // 20-20-20 rule
    public int EyeBreakDurationSeconds { get; set; } = 20;

    public bool HydrationReminderEnabled { get; set; } = true;
    public int HydrationIntervalMinutes { get; set; } = 45;

    public bool PostureReminderEnabled { get; set; } = true;
    public int PostureIntervalMinutes { get; set; } = 60;

    // Pomodoro Mode
    public int PomodoroFocusMinutes { get; set; } = 25;
    public int PomodoroBreakMinutes { get; set; } = 5;

    // AI & Gemini
    public string GeminiApiKey { get; set; } = "";

    // Gamification & Friendship
    public int FriendshipXP { get; set; } = 150;
    public int FriendshipLevel => Math.Max(1, (FriendshipXP / 100) + 1);

    // System
    public bool StartWithWindows { get; set; } = false;
    public double LastPosX { get; set; } = -1;
    public double LastPosY { get; set; } = -1;
}
