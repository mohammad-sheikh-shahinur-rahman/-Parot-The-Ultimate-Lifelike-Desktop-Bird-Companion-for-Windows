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
    public event Action? PerchModeToggled;
    public event Action? AskParotRequested;
    public event Action? GhostModeToggled;
    public event Action? SettingsRequested;
    public event Action? ExitRequested;

    private readonly ToolStripMenuItem _itemShowHide;
    private readonly ToolStripMenuItem _itemSleepWake;
    private readonly ToolStripMenuItem _itemPomodoro;
    private readonly ToolStripMenuItem _itemPerchMode;
    private readonly ToolStripMenuItem _itemGhost;

    public TrayIconService()
    {
        _contextMenu = new ContextMenuStrip();

        _itemShowHide = new ToolStripMenuItem("🦜 Hide Parot", null, (s, e) => ShowHideToggled?.Invoke());
        _itemSleepWake = new ToolStripMenuItem("💤 Go to Sleep", null, (s, e) => SleepWakeToggled?.Invoke());

        _itemPomodoro = new ToolStripMenuItem("🍅 Start Pomodoro Focus (25m)", null, (s, e) => PomodoroToggled?.Invoke());
        var askItem = new ToolStripMenuItem("✨ Ask Parot (AI Chat)", null, (s, e) => AskParotRequested?.Invoke());
        var reportItem = new ToolStripMenuItem("📊 Daily Wellness Report", null, (s, e) => DailyReportRequested?.Invoke());

        var breakItem = new ToolStripMenuItem("👁️ Rest Eyes Now (20-20-20)", null, (s, e) => TakeBreakRequested?.Invoke());
        var waterItem = new ToolStripMenuItem("💧 Drink Water Now", null, (s, e) => DrinkWaterRequested?.Invoke());
        var feedItem = new ToolStripMenuItem("🌶️ Feed Chili Treat", null, (s, e) => FeedTreatRequested?.Invoke());

        _itemPerchMode = new ToolStripMenuItem("🪟 Perch on Active Window", null, (s, e) => PerchModeToggled?.Invoke());
        _itemGhost = new ToolStripMenuItem("👻 Ghost Mode (Click-Through)", null, (s, e) => GhostModeToggled?.Invoke());

        var settingsItem = new ToolStripMenuItem("⚙️ Settings...", null, (s, e) => SettingsRequested?.Invoke());
        var exitItem = new ToolStripMenuItem("❌ Exit Parot", null, (s, e) => ExitRequested?.Invoke());

        _contextMenu.Items.AddRange(
        [
            _itemShowHide,
            _itemSleepWake,
            new ToolStripSeparator(),
            _itemPomodoro,
            askItem,
            reportItem,
            new ToolStripSeparator(),
            breakItem,
            waterItem,
            feedItem,
            new ToolStripSeparator(),
            _itemPerchMode,
            _itemGhost,
            settingsItem,
            new ToolStripSeparator(),
            exitItem
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

    public void ShowNotification(string title, string text, ToolTipIcon icon = ToolTipIcon.Info)
    {
        _notifyIcon.ShowBalloonTip(3000, title, text, icon);
    }

    private static Icon CreateParrotIcon()
    {
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
