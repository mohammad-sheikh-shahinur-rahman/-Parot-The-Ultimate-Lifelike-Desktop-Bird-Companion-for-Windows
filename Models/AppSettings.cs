using System;

namespace Parot.Models;

public class AppSettings
{
    // Appearance & Scale
    public double Scale { get; set; } = 1.0; // 0.7 to 1.6
    public bool AlwaysOnTop { get; set; } = true;
    public bool GhostMode { get; set; } = false; // Click-through

    // Pet Behavior
    public bool AllowWalking { get; set; } = true;
    public int AutoSleepMinutes { get; set; } = 3; // 0 = disabled
    public bool IdleChirping { get; set; } = true;
    public string Language { get; set; } = "Bilingual"; // "Bilingual", "Bengali", "English"

    // Sound
    public bool SoundEnabled { get; set; } = true;
    public double SoundVolume { get; set; } = 0.8; // 0.0 to 1.0

    // Health & Screen-Time Reminders
    public bool ScreenBreakEnabled { get; set; } = true;
    public int ScreenBreakIntervalMinutes { get; set; } = 25; // 20-20-20 rule default ~25 min
    public int EyeBreakDurationSeconds { get; set; } = 20;

    public bool HydrationReminderEnabled { get; set; } = true;
    public int HydrationIntervalMinutes { get; set; } = 45;

    public bool PostureReminderEnabled { get; set; } = true;
    public int PostureIntervalMinutes { get; set; } = 60;

    // System
    public bool StartWithWindows { get; set; } = false;
    public double LastPosX { get; set; } = -1; // -1 = auto-perch near bottom-right
    public double LastPosY { get; set; } = -1;
}
