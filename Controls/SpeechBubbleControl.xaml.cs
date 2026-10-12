using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Parot.Models;
using UserControl = System.Windows.Controls.UserControl;

namespace Parot.Controls;

public partial class SpeechBubbleControl : UserControl
{
    private readonly DispatcherTimer _autoHideTimer;
    private ReminderType? _currentReminderType;

    public event Action<ReminderType, bool>? ReminderActionTaken; // bool: true = Done, false = Snoozed
    public event Action? BubbleClosed;

    public SpeechBubbleControl()
    {
        InitializeComponent();

        _autoHideTimer = new DispatcherTimer();
        _autoHideTimer.Tick += OnAutoHideTimerTick;
    }

    public void ShowMessage(string message, string icon = "🦜", string title = "Parot", int durationSeconds = 6)
    {
        _currentReminderType = null;
        TxtIcon.Text = icon;
        TxtTitle.Text = title;
        TxtMessage.Text = message;
        ActionPanel.Visibility = Visibility.Collapsed;

        Appear();

        if (durationSeconds > 0)
        {
            _autoHideTimer.Interval = TimeSpan.FromSeconds(durationSeconds);
            _autoHideTimer.Stop();
            _autoHideTimer.Start();
        }
        else
        {
            _autoHideTimer.Stop();
        }
    }

    public void ShowReminder(ReminderType type, string message)
    {
        _currentReminderType = type;
        string icon = type switch
        {
            ReminderType.ScreenBreak => "👁️",
            ReminderType.Hydration => "💧",
            ReminderType.Posture => "🧘",
            _ => "🦜"
        };

        string title = type switch
        {
            ReminderType.ScreenBreak => "চোখের বিশ্রাম (20-20-20)",
            ReminderType.Hydration => "পানি খাওয়ার সময় (Hydrate)",
            ReminderType.Posture => "শরীর সোজা করার সময় (Posture)",
            _ => "Parot Reminder"
        };

        TxtIcon.Text = icon;
        TxtTitle.Text = title;
        TxtMessage.Text = message;
        ActionPanel.Visibility = Visibility.Visible;

        Appear();

        // Stays up longer for reminders (40 seconds) if not interacted with
        _autoHideTimer.Interval = TimeSpan.FromSeconds(40);
        _autoHideTimer.Stop();
        _autoHideTimer.Start();
    }

    private void Appear()
    {
        Visibility = Visibility.Visible;
        if (Resources["ShowStoryboard"] is Storyboard sb)
        {
            sb.Begin(this);
        }
        else
        {
            Opacity = 1;
        }
    }

    public void HideBubble()
    {
        _autoHideTimer.Stop();
        if (Resources["HideStoryboard"] is Storyboard sb)
        {
            sb.Completed += (s, e) =>
            {
                Visibility = Visibility.Collapsed;
                BubbleClosed?.Invoke();
            };
            sb.Begin(this);
        }
        else
        {
            Visibility = Visibility.Collapsed;
            BubbleClosed?.Invoke();
        }
    }

    private void OnAutoHideTimerTick(object? sender, EventArgs e)
    {
        _autoHideTimer.Stop();
        HideBubble();
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        HideBubble();
    }

    private void BtnDone_Click(object sender, RoutedEventArgs e)
    {
        if (_currentReminderType.HasValue)
        {
            ReminderActionTaken?.Invoke(_currentReminderType.Value, true);
        }
        HideBubble();
    }

    private void BtnSnooze_Click(object sender, RoutedEventArgs e)
    {
        if (_currentReminderType.HasValue)
        {
            ReminderActionTaken?.Invoke(_currentReminderType.Value, false);
        }
        HideBubble();
    }
}
