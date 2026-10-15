using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Parot.Models;

namespace Parot.Services;

public class TrayIconService : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _contextMenu;

    public event Action? ShowHideToggled;
    public event Action? SleepWakeToggled;
    public event Action? TakeBreakRequested;
    public event Action? DrinkWaterRequested;
    public event Action? FeedTreatRequested;
    public event Action? PomodoroToggled;
    public event Action? DailyReportRequested;
    public event Action? SoundToggled;
    public event Action? FlyRequested;
    public event Action? FollowCursorToggled;
    public event Action? PerchModeToggled;
    public event Action? AskParotRequested;
    public event Action? GhostModeToggled;
    public event Action? SettingsRequested;
    public event Action? ExitRequested;

    private readonly ToolStripMenuItem _itemShowHide;
    private readonly ToolStripMenuItem _itemSleepWake;
    private readonly ToolStripMenuItem _itemSoundToggle;
    private readonly ToolStripMenuItem _itemPomodoro;
    private readonly ToolStripMenuItem _itemFollowCursor;
    private readonly ToolStripMenuItem _itemPerchMode;
    private readonly ToolStripMenuItem _itemGhost;
    private readonly ToolStripMenuItem _flyItem;
    private readonly ToolStripMenuItem _askItem;
    private readonly ToolStripMenuItem _reportItem;
    private readonly ToolStripMenuItem _breakItem;
    private readonly ToolStripMenuItem _waterItem;
    private readonly ToolStripMenuItem _feedItem;
    private readonly ToolStripMenuItem _settingsItem;
    private readonly ToolStripMenuItem _exitItem;

    public TrayIconService()
    {
        _contextMenu = new ContextMenuStrip();

        _itemShowHide = new ToolStripMenuItem("🦜 Hide Parot", null, (s, e) => ShowHideToggled?.Invoke());
        _itemSleepWake = new ToolStripMenuItem("💤 Go to Sleep", null, (s, e) => SleepWakeToggled?.Invoke());
        _itemSoundToggle = new ToolStripMenuItem("🔊 Sound: ON", null, (s, e) => SoundToggled?.Invoke());

        _flyItem = new ToolStripMenuItem("🚀 Take Flight", null, (s, e) => FlyRequested?.Invoke());
        _itemFollowCursor = new ToolStripMenuItem("🎯 Follow Cursor", null, (s, e) => FollowCursorToggled?.Invoke());

        _itemPomodoro = new ToolStripMenuItem("🍅 Start Pomodoro Focus (25m)", null, (s, e) => PomodoroToggled?.Invoke());
        _askItem = new ToolStripMenuItem("✨ Ask Parot (AI Chat)", null, (s, e) => AskParotRequested?.Invoke());
        _reportItem = new ToolStripMenuItem("📊 Daily Wellness Report", null, (s, e) => DailyReportRequested?.Invoke());

        _breakItem = new ToolStripMenuItem("👁️ Rest Eyes Now (20-20-20)", null, (s, e) => TakeBreakRequested?.Invoke());
        _waterItem = new ToolStripMenuItem("💧 Drink Water Now", null, (s, e) => DrinkWaterRequested?.Invoke());
        _feedItem = new ToolStripMenuItem("🌶️ Feed Chili Treat", null, (s, e) => FeedTreatRequested?.Invoke());

        _itemPerchMode = new ToolStripMenuItem("🪟 Perch on Active Window", null, (s, e) => PerchModeToggled?.Invoke());
        _itemGhost = new ToolStripMenuItem("👻 Ghost Mode (Click-Through)", null, (s, e) => GhostModeToggled?.Invoke());

        _settingsItem = new ToolStripMenuItem("⚙️ Settings...", null, (s, e) => SettingsRequested?.Invoke());
        _exitItem = new ToolStripMenuItem("❌ Exit Parot", null, (s, e) => ExitRequested?.Invoke());

        _contextMenu.Items.AddRange(
        [
            _itemShowHide,
            _itemSleepWake,
            _itemSoundToggle,
            _flyItem,
            _itemFollowCursor,
            new ToolStripSeparator(),
            _itemPomodoro,
            _askItem,
            _reportItem,
            new ToolStripSeparator(),
            _breakItem,
            _waterItem,
            _feedItem,
            new ToolStripSeparator(),
            _itemPerchMode,
            _itemGhost,
            _settingsItem,
            new ToolStripSeparator(),
            _exitItem
        ]);

        _notifyIcon = new NotifyIcon
        {
            Icon = CreateParrotIcon(),
            Text = "Parot - Desktop Bird Companion",
            Visible = true,
            ContextMenuStrip = _contextMenu
        };

        _notifyIcon.DoubleClick += (s, e) => ShowHideToggled?.Invoke();
    }

    public void UpdateVisibilityState(bool isVisible)
    {
        _itemShowHide.Text = isVisible ? "🦜 Hide Parot" : "🦜 Show Parot";
    }

    public void UpdateSleepState(bool isSleeping)
    {
        _itemSleepWake.Text = isSleeping ? "☀️ Wake Up Parot" : "💤 Put to Sleep";
    }

    public void UpdateSoundState(bool soundEnabled)
    {
        _itemSoundToggle.Text = soundEnabled ? "🔊 Sound: ON" : "🔇 Sound: Muted (OFF)";
    }

    public void UpdatePomodoroState(bool isRunning)
    {
        _itemPomodoro.Text = isRunning ? "🍅 Stop Pomodoro" : "🍅 Start Pomodoro Focus (25m)";
    }

    public void UpdatePerchMode(bool onActiveWindow)
    {
        _itemPerchMode.Checked = onActiveWindow;
    }

    public void UpdateGhostState(bool isGhost)
    {
        _itemGhost.Checked = isGhost;
    }

    public void UpdateLanguage(string lang)
    {
        _flyItem.Text = LocalizationService.Get("TakeFlight", lang);
        _itemFollowCursor.Text = LocalizationService.Get("FollowCursor", lang);
        _askItem.Text = LocalizationService.Get("MenuAskParot", lang);
        _reportItem.Text = LocalizationService.Get("MenuDailyReport", lang);
        _breakItem.Text = LocalizationService.Get("MenuEyeBreak", lang);
        _waterItem.Text = LocalizationService.Get("MenuHydrate", lang);
        _feedItem.Text = LocalizationService.Get("MenuThrowChili", lang);
        _itemPerchMode.Text = LocalizationService.Get("PerchActiveWindow", lang);
        _itemGhost.Text = LocalizationService.Get("GhostModeMenu", lang);
        _settingsItem.Text = LocalizationService.Get("SettingsMenu", lang);
        _exitItem.Text = LocalizationService.Get("ExitMenu", lang);
    }

    public void ShowNotification(string title, string text, ToolTipIcon icon = ToolTipIcon.Info)
    {
        _notifyIcon.ShowBalloonTip(3000, title, text, icon);
    }

    private static Icon CreateParrotIcon()
    {
        try
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string[] possiblePaths =
            [
                System.IO.Path.Combine(baseDir, "Assets", "AppIcon.ico"),
                System.IO.Path.Combine(baseDir, "Assets", "WindowsAssets", "AppIcon.ico"),
                System.IO.Path.Combine(baseDir, "Parot-Windows-Assets", "AppIcon.ico")
            ];

            foreach (var path in possiblePaths)
            {
                if (System.IO.File.Exists(path))
                {
                    return new Icon(path);
                }
            }
        }
        catch { }

        using var bmp = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);

            using (var brush = new SolidBrush(Color.FromArgb(34, 197, 94)))
            {
                g.FillEllipse(brush, 4, 6, 22, 22);
            }

            using (var brush = new SolidBrush(Color.FromArgb(134, 239, 172)))
            {
                g.FillEllipse(brush, 10, 12, 14, 14);
            }

            using (var brush = new SolidBrush(Color.FromArgb(15, 23, 42)))
            {
                g.FillEllipse(brush, 16, 10, 4, 4);
            }

            using (var brush = new SolidBrush(Color.White))
            {
                g.FillEllipse(brush, 17, 10, 2, 2);
            }

            using (var brush = new SolidBrush(Color.FromArgb(249, 115, 22)))
            {
                Point[] beak = [new Point(24, 13), new Point(31, 16), new Point(23, 20)];
                g.FillPolygon(brush, beak);
            }

            using (var pen = new Pen(Color.FromArgb(225, 29, 72), 2))
            {
                g.DrawArc(pen, 8, 16, 14, 10, 30, 120);
            }
        }

        IntPtr hIcon = bmp.GetHicon();
        return Icon.FromHandle(hIcon);
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _contextMenu.Dispose();
    }
}
