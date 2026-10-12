using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
    public event Action<string>? QuestionAsked;
    public event Action? WaterLogged;

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
        AskParotPanel.Visibility = Visibility.Collapsed;
        BtnLogWater.Visibility = Visibility.Collapsed;

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
            ReminderType.PomodoroAlert => "🍅",
            _ => "🦜"
        };

        string title = type switch
        {
            ReminderType.ScreenBreak => "চোখের বিশ্রাম (20-20-20)",
            ReminderType.Hydration => "পানি খাওয়ার সময় (Hydrate)",
            ReminderType.Posture => "শরীর সোজা করার সময় (Posture)",
            ReminderType.PomodoroAlert => "পোমোডোরো সেশন (Pomodoro)",
            _ => "Parot Reminder"
        };

        TxtIcon.Text = icon;
        TxtTitle.Text = title;
        TxtMessage.Text = message;
        ActionPanel.Visibility = Visibility.Visible;
        AskParotPanel.Visibility = Visibility.Collapsed;

        BtnLogWater.Visibility = (type == ReminderType.Hydration) ? Visibility.Visible : Visibility.Collapsed;

        Appear();

        _autoHideTimer.Interval = TimeSpan.FromSeconds(40);
        _autoHideTimer.Stop();
        _autoHideTimer.Start();
    }

    public void PromptAskParot()
    {
        _currentReminderType = null;
        TxtIcon.Text = "✨";
        TxtTitle.Text = "Ask Parot (এআই বাডি)";
        TxtMessage.Text = "আমাকে যেকোনো প্রশ্ন করো অথবা বাংলায় অনুবাদ করতে বলো:";
        ActionPanel.Visibility = Visibility.Collapsed;
        AskParotPanel.Visibility = Visibility.Visible;
        BtnLogWater.Visibility = Visibility.Collapsed;

        TxtAskInput.Text = "";
        Appear();

        _autoHideTimer.Stop(); // Stays until user sends or closes
        TxtAskInput.Focus();
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

    private void BtnLogWater_Click(object sender, RoutedEventArgs e)
    {
        WaterLogged?.Invoke();
        ShowMessage("এক গ্লাস পানি পান রেকর্ড করা হয়েছে! 💧 (+15 XP)", "💧", "Hydrated!", 4);
    }

    private void BtnSendAsk_Click(object sender, RoutedEventArgs e)
    {
        SubmitQuestion();
    }

    private void TxtAskInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter)
        {
            SubmitQuestion();
        }
    }

    private void SubmitQuestion()
    {
        var query = TxtAskInput.Text?.Trim();
        if (!string.IsNullOrWhiteSpace(query))
        {
            AskParotPanel.Visibility = Visibility.Collapsed;
            QuestionAsked?.Invoke(query);
        }
    }
}
