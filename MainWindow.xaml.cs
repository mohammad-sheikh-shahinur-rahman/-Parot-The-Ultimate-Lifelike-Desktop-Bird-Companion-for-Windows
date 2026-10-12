using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Parot.Helpers;
using Parot.Models;
using Parot.Services;
using Parot.Views;
using Point = System.Windows.Point;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Application = System.Windows.Application;

namespace Parot;

public partial class MainWindow : Window
{
    private readonly SettingsService _settingsService;
    private readonly SoundService _soundService;
    private readonly ReminderService _reminderService;
    private readonly TrayIconService _trayIconService;

    private readonly DispatcherTimer _loopTimer;
    private readonly Random _random = new();

    private bool _isDragging = false;
    private Point _dragStartCursorPos;
    private Point _dragStartWindowPos;

    private DateTime _lastInteractionTime = DateTime.Now;
    private Point _lastKnownMousePos;
    private DateTime _lastRoamTime = DateTime.Now;

    public MainWindow()
    {
        InitializeComponent();

        _settingsService = new SettingsService();
        _soundService = new SoundService(_settingsService.CurrentSettings.SoundEnabled, _settingsService.CurrentSettings.SoundVolume);
        _reminderService = new ReminderService(_settingsService.CurrentSettings);
        _trayIconService = new TrayIconService();

        // Wire up reminders
        _reminderService.ReminderTriggered += OnReminderTriggered;

        // Wire up speech bubble actions
        SpeechBubble.ReminderActionTaken += OnReminderActionTaken;

        // Wire up tray icon actions
        _trayIconService.ShowHideToggled += ToggleVisibility;
        _trayIconService.SleepWakeToggled += ToggleSleepWake;
        _trayIconService.TakeBreakRequested += () => _reminderService.TriggerBreakNow();
        _trayIconService.DrinkWaterRequested += () => _reminderService.TriggerHydrationNow();
        _trayIconService.FeedTreatRequested += FeedChiliTreat;
        _trayIconService.GhostModeToggled += ToggleGhostMode;
        _trayIconService.SettingsRequested += OpenSettingsWindow;
        _trayIconService.ExitRequested += ExitApplication;

        // Settings changed event
        _settingsService.SettingsChanged += ApplySettings;

        // Main behavioral loop (runs every 150ms for smooth low-CPU responsiveness)
        _loopTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(150)
        };
        _loopTimer.Tick += OnBehaviorLoopTick;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        ApplySettings(_settingsService.CurrentSettings);
        PositionWindowInitially();
        _loopTimer.Start();

        // Play initial happy chirp & friendly greeting
        _soundService.PlayChirp();
        var greet = Dialogues.GetWakeUpDialogue(_settingsService.CurrentSettings.Language);
        SpeechBubble.ShowMessage(greet, "🦜", "Parot", 5);
    }

    private void ApplySettings(AppSettings s)
    {
        Pet.SetScale(s.Scale);
        Topmost = s.AlwaysOnTop;
        _soundService.Enabled = s.SoundEnabled;
        _soundService.Volume = s.SoundVolume;
        _reminderService.UpdateSettings(s);

        WindowInteropHelperNative.SetClickThrough(this, s.GhostMode);
        _trayIconService.UpdateGhostState(s.GhostMode);
    }

    private void PositionWindowInitially()
    {
        var workArea = SystemParameters.WorkArea;
        var s = _settingsService.CurrentSettings;

        if (s.LastPosX >= 0 && s.LastPosY >= 0 &&
            s.LastPosX < workArea.Right && s.LastPosY < workArea.Bottom)
        {
            Left = s.LastPosX;
            Top = s.LastPosY;
        }
        else
        {
            // Default: Perch comfortably near bottom-right, right above taskbar
            Left = workArea.Right - Width - 30;
            Top = workArea.Bottom - Height;
        }
    }

    private void OnBehaviorLoopTick(object? sender, EventArgs e)
    {
        if (_isDragging) return;

        var currentMouse = WindowInteropHelperNative.GetCurrentMousePosition();

        // Check if mouse moved anywhere on screen
        if (Math.Abs(currentMouse.X - _lastKnownMousePos.X) > 2 ||
            Math.Abs(currentMouse.Y - _lastKnownMousePos.Y) > 2)
        {
            _lastKnownMousePos = currentMouse;
            _lastInteractionTime = DateTime.Now;

            // If sleeping and mouse passes very close, pet may stir
            double birdCenterX = Left + (Width / 2);
            double birdCenterY = Top + Height - 50;
            double dist = Math.Sqrt(Math.Pow(currentMouse.X - birdCenterX, 2) + Math.Pow(currentMouse.Y - birdCenterY, 2));

            if (Pet.CurrentState == PetState.Sleeping && dist < 60)
            {
                WakeUp();
            }
        }

        // Mouse follow and eye tracking
        if (Pet.CurrentState != PetState.Sleeping && Pet.CurrentState != PetState.Dragged)
        {
            double birdCenterX = Left + (Width / 2);
            double birdCenterY = Top + Height - 50;
            double deltaX = currentMouse.X - birdCenterX;
            double deltaY = currentMouse.Y - birdCenterY;

            Pet.LookAt(deltaX, deltaY);

            // If cursor is close and pet is idle, occasional curious tilt
            double dist = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
            if (dist < 50 && Pet.CurrentState == PetState.Idle && _random.Next(100) < 4)
            {
                Pet.SetState(PetState.CuriousHeadTilt);
                _soundService.PlayChirp();
            }
        }

        // Auto-Sleep Check
        var s = _settingsService.CurrentSettings;
        if (s.AutoSleepMinutes > 0 && Pet.CurrentState != PetState.Sleeping)
        {
            var idleSeconds = (DateTime.Now - _lastInteractionTime).TotalSeconds;
            if (idleSeconds >= s.AutoSleepMinutes * 60)
            {
                PutToSleep();
            }
        }

        // Idle Roaming / Hopping along screen bottom
        if (s.AllowWalking && Pet.CurrentState == PetState.Idle)
        {
            var timeSinceRoam = (DateTime.Now - _lastRoamTime).TotalSeconds;
            if (timeSinceRoam >= 45 && _random.Next(100) < 15)
            {
                _lastRoamTime = DateTime.Now;
                PerformRandomHop();
            }
        }
    }

    private void PerformRandomHop()
    {
        var workArea = SystemParameters.WorkArea;
        bool isAtBottom = Math.Abs(Top - (workArea.Bottom - Height)) < 80;
        if (!isAtBottom) return;

        bool hopLeft = _random.Next(2) == 0;
        double hopDistance = _random.Next(35, 75);

        // Keep within work area bounds
        if (hopLeft && (Left - hopDistance) < workArea.Left + 20)
        {
            hopLeft = false;
        }
        else if (!hopLeft && (Left + hopDistance) > workArea.Right - Width - 20)
        {
            hopLeft = true;
        }

        double targetLeft = hopLeft ? Left - hopDistance : Left + hopDistance;
        Pet.SetFacing(hopLeft);
        Pet.SetState(PetState.Hopping);

        var moveAnim = new DoubleAnimation
        {
            To = targetLeft,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };

        moveAnim.Completed += (s, e) =>
        {
            Pet.SetState(PetState.Idle);
            _settingsService.CurrentSettings.LastPosX = Left;
            _settingsService.CurrentSettings.LastPosY = Top;
        };

        BeginAnimation(LeftProperty, moveAnim);
        _soundService.PlayChirp();
    }

    #region Dragging & Mouse Events

    private void Pet_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            OnPetDoubleClicked();
            return;
        }

        _dragStartCursorPos = WindowInteropHelperNative.GetCurrentMousePosition();
        _dragStartWindowPos = new Point(Left, Top);
        _lastInteractionTime = DateTime.Now;

        if (Pet.CurrentState == PetState.Sleeping)
        {
            WakeUp();
            return;
        }

        Pet.CaptureMouse();
    }

    private void Pet_MouseMove(object sender, MouseEventArgs e)
    {
        if (Pet.IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed)
        {
            var currentCursor = WindowInteropHelperNative.GetCurrentMousePosition();
            double diffX = currentCursor.X - _dragStartCursorPos.X;
            double diffY = currentCursor.Y - _dragStartCursorPos.Y;

            if (!_isDragging && (Math.Abs(diffX) > 4 || Math.Abs(diffY) > 4))
            {
                _isDragging = true;
                Pet.SetState(PetState.Dragged);
                _soundService.PlayChirp();
            }

            if (_isDragging)
            {
                Left = _dragStartWindowPos.X + diffX;
                Top = _dragStartWindowPos.Y + diffY;
            }
        }
    }

    private void Pet_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (Pet.IsMouseCaptured)
        {
            Pet.ReleaseMouseCapture();
        }

        if (_isDragging)
        {
            _isDragging = false;
            OnDragFinished();
        }
        else
        {
            // Simple single click!
            OnPetSingleClicked();
        }
    }

    private void Pet_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        MenuSleepWake.Header = Pet.CurrentState == PetState.Sleeping
            ? "☀️ জেগে ওঠো (Wake Up)"
            : "💤 ঘুমাতে যাও (Go to Sleep)";

        PetContextMenu.IsOpen = true;
    }

    private void OnDragFinished()
    {
        var workArea = SystemParameters.WorkArea;

        // Snap to bottom if dropped near taskbar / screen bottom
        if (Top > workArea.Bottom - Height - 70)
        {
            Top = workArea.Bottom - Height;
        }

        // Clamp inside screen bounds horizontally
        Left = Math.Clamp(Left, workArea.Left, workArea.Right - Width);

        // Save position
        _settingsService.CurrentSettings.LastPosX = Left;
        _settingsService.CurrentSettings.LastPosY = Top;
        _settingsService.SaveSettings(_settingsService.CurrentSettings);

        Pet.SetState(PetState.Idle);
        _soundService.PlayTrill();
        Pet.AnimateHearts();
    }

    private void OnPetSingleClicked()
    {
        _lastInteractionTime = DateTime.Now;

        if (Pet.CurrentState == PetState.Sleeping)
        {
            WakeUp();
        }
        else
        {
            Pet.SetState(PetState.Happy);
            _soundService.PlayDoubleChirp();

            // After a moment return to idle
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1200) };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                if (Pet.CurrentState == PetState.Happy)
                {
                    Pet.SetState(PetState.Idle);
                }
            };
            timer.Start();
        }
    }

    private void OnPetDoubleClicked()
    {
        _lastInteractionTime = DateTime.Now;
        if (Pet.CurrentState == PetState.Sleeping)
        {
            WakeUp();
            return;
        }

        _soundService.PlayTrill();
        Pet.SetState(PetState.Speaking);
        var dialogue = Dialogues.GetIdleDialogue(_settingsService.CurrentSettings.Language);
        SpeechBubble.ShowMessage(dialogue, "🦜", "Parot", 6);
    }

    #endregion

    #region State Controls (Sleep, Wake, Feed)

    public void WakeUp()
    {
        Pet.SetState(PetState.Idle);
        _trayIconService.UpdateSleepState(false);
        _soundService.PlayDoubleChirp();
        Pet.AnimateMusicNote();

        var wakeQuote = Dialogues.GetWakeUpDialogue(_settingsService.CurrentSettings.Language);
        SpeechBubble.ShowMessage(wakeQuote, "☀️", "Parot", 5);
        _lastInteractionTime = DateTime.Now;
    }

    public void PutToSleep()
    {
        Pet.SetState(PetState.Sleeping);
        _trayIconService.UpdateSleepState(true);
        _soundService.PlaySnore();
        SpeechBubble.HideBubble();
    }

    public void ToggleSleepWake()
    {
        if (Pet.CurrentState == PetState.Sleeping)
        {
            WakeUp();
        }
        else
        {
            PutToSleep();
        }
    }

    public void FeedChiliTreat()
    {
        WakeUp();
        Pet.SetState(PetState.Eating);
        _soundService.PlayMunch();
        var treatQuote = Dialogues.GetTreatDialogue(_settingsService.CurrentSettings.Language);
        SpeechBubble.ShowMessage(treatQuote, "🌶️", "Parot Treat", 6);
    }

    public void FeedSunflowerSeeds()
    {
        WakeUp();
        Pet.SetState(PetState.Eating);
        _soundService.PlayMunch();
        var treatQuote = Dialogues.GetTreatDialogue(_settingsService.CurrentSettings.Language);
        SpeechBubble.ShowMessage(treatQuote, "🌻", "Parot Treat", 6);
    }

    #endregion

    #region Reminders Handling

    private void OnReminderTriggered(object? sender, ReminderEventArgs e)
    {
        if (Pet.CurrentState == PetState.Sleeping)
        {
            Pet.SetState(PetState.Idle);
            _trayIconService.UpdateSleepState(false);
        }

        _soundService.PlayChime();
        Pet.SetState(PetState.Speaking);
        SpeechBubble.ShowReminder(e.Type, e.Message);
    }

    private void OnReminderActionTaken(ReminderType type, bool isDone)
    {
        if (isDone)
        {
            _reminderService.ResetTimer(type);
            Pet.SetState(PetState.Happy);
            Pet.AnimateHearts();
            _soundService.PlayTrill();
        }
        else
        {
            // Snooze for 5 minutes
            _reminderService.Snooze(type, 5);
            _soundService.PlayChirp();
            SpeechBubble.ShowMessage("৫ মিনিটের জন্য মনে করিয়ে দেব! ⏳", "⏳", "Snoozed", 3);
        }
    }

    #endregion

    #region Window & Tray Actions

    private void ToggleVisibility()
    {
        if (Visibility == Visibility.Visible)
        {
            Visibility = Visibility.Collapsed;
            _trayIconService.UpdateVisibilityState(false);
        }
        else
        {
            Visibility = Visibility.Visible;
            _trayIconService.UpdateVisibilityState(true);
            WakeUp();
        }
    }

    private void ToggleGhostMode()
    {
        var s = _settingsService.CurrentSettings;
        s.GhostMode = !s.GhostMode;
        _settingsService.SaveSettings(s);
        ApplySettings(s);

        string status = s.GhostMode ? "সক্রিয় (Clicks pass through)" : "নিষ্ক্রিয় (Normal clicks)";
        _trayIconService.ShowNotification("Ghost Mode", $"ক্লিক-থ্রু মোড এখন {status}");
    }

    private void OpenSettingsWindow()
    {
        var settingsWin = new SettingsWindow(_settingsService, _soundService)
        {
            Owner = this
        };
        settingsWin.FeedTreatRequested += FeedChiliTreat;
        settingsWin.TestReminderRequested += () => _reminderService.TriggerBreakNow();
        settingsWin.ShowDialog();
    }

    private void ExitApplication()
    {
        _loopTimer.Stop();
        _trayIconService.Dispose();
        Application.Current.Shutdown();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        _trayIconService.Dispose();
    }

    #endregion

    #region Context Menu Handlers

    private void MenuSquawk_Click(object sender, RoutedEventArgs e)
    {
        OnPetDoubleClicked();
    }

    private void MenuFeedChili_Click(object sender, RoutedEventArgs e)
    {
        FeedChiliTreat();
    }

    private void MenuFeedSeeds_Click(object sender, RoutedEventArgs e)
    {
        FeedSunflowerSeeds();
    }

    private void MenuSleepWake_Click(object sender, RoutedEventArgs e)
    {
        ToggleSleepWake();
    }

    private void MenuEyeBreak_Click(object sender, RoutedEventArgs e)
    {
        _reminderService.TriggerBreakNow();
    }

    private void MenuHydrate_Click(object sender, RoutedEventArgs e)
    {
        _reminderService.TriggerHydrationNow();
    }

    private void MenuGhostMode_Click(object sender, RoutedEventArgs e)
    {
        ToggleGhostMode();
    }

    private void MenuSettings_Click(object sender, RoutedEventArgs e)
    {
        OpenSettingsWindow();
    }

    private void MenuExit_Click(object sender, RoutedEventArgs e)
    {
        ExitApplication();
    }

    #endregion
}