using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
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
    private readonly StatisticsService _statsService;
    private readonly PomodoroService _pomodoroService;
    private readonly AudioBeatDetectorService _audioDetectorService;
    private readonly VoiceMimicService _voiceMimicService;

    private readonly DispatcherTimer _loopTimer;
    private readonly Random _random = new();

    private bool _isDragging = false;
    private Point _dragStartCursorPos;
    private Point _dragStartWindowPos;

    private DateTime _lastInteractionTime = DateTime.Now;
    private Point _lastKnownMousePos;
    private DateTime _lastRoamTime = DateTime.Now;
    private DateTime _lastActiveMinuteTick = DateTime.Now;
    private DateTime _lastDanceTime = DateTime.Now;
    private bool _isPlayingTag = false;

    public MainWindow()
    {
        InitializeComponent();

        _settingsService = new SettingsService();
        var s = _settingsService.CurrentSettings;

        _soundService = new SoundService(s.SoundEnabled, s.SoundVolume);
        _reminderService = new ReminderService(s);
        _trayIconService = new TrayIconService();
        _statsService = new StatisticsService();
        _pomodoroService = new PomodoroService(s.PomodoroFocusMinutes, s.PomodoroBreakMinutes);
        _audioDetectorService = new AudioBeatDetectorService();
        _voiceMimicService = new VoiceMimicService(s.VoiceMimicEnabled, s.VoicePitchRate);

        // Reminders & Speech Bubble
        _reminderService.ReminderTriggered += OnReminderTriggered;
        SpeechBubble.ReminderActionTaken += OnReminderActionTaken;
        SpeechBubble.QuestionAsked += OnQuestionAsked;
        SpeechBubble.WaterLogged += OnWaterLogged;

        // Pomodoro Events
        _pomodoroService.Tick += OnPomodoroTick;
        _pomodoroService.PhaseChanged += OnPomodoroPhaseChanged;

        // Audio Beat / Music Detection
        _audioDetectorService.BeatDetected += OnAudioBeatDetected;

        // Tray Icon Actions
        _trayIconService.ShowHideToggled += ToggleVisibility;
        _trayIconService.SleepWakeToggled += ToggleSleepWake;
        _trayIconService.TakeBreakRequested += () => _reminderService.TriggerBreakNow();
        _trayIconService.DrinkWaterRequested += () => _reminderService.TriggerHydrationNow();
        _trayIconService.FeedTreatRequested += FeedChiliTreat;
        _trayIconService.PomodoroToggled += TogglePomodoro;
        _trayIconService.DailyReportRequested += OpenDailyReportWindow;
        _trayIconService.PerchModeToggled += TogglePerchMode;
        _trayIconService.AskParotRequested += () => SpeechBubble.PromptAskParot();
        _trayIconService.GhostModeToggled += ToggleGhostMode;
        _trayIconService.SettingsRequested += OpenSettingsWindow;
        _trayIconService.ExitRequested += ExitApplication;

        // Settings Changed Event
        _settingsService.SettingsChanged += ApplySettings;

        // Main behavioral loop (150ms for low CPU and smooth tracking)
        _loopTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
        _loopTimer.Tick += OnBehaviorLoopTick;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        ApplySettings(_settingsService.CurrentSettings);
        PositionWindowInitially();
        _loopTimer.Start();

        _soundService.PlayChirp();
        var greet = Dialogues.GetWakeUpDialogue(_settingsService.CurrentSettings.Language);
        SpeechBubble.ShowMessage(greet, "🦜", "Parot", 5);

        // Check Night Mode
        CheckNightMode();
    }

    private void ApplySettings(AppSettings s)
    {
        Pet.ApplySkin(s.Skin);
        Pet.SetAccessory(s.Accessory);
        Pet.SetScale(s.Scale);

        Topmost = s.AlwaysOnTop;
        _soundService.Enabled = s.SoundEnabled;
        _soundService.Volume = s.SoundVolume;
        _voiceMimicService.Enabled = s.VoiceMimicEnabled;
        _voiceMimicService.VoiceRate = s.VoicePitchRate;
        _audioDetectorService.Enabled = s.DanceToMusic;

        _reminderService.UpdateSettings(s);
        _pomodoroService.FocusMinutes = s.PomodoroFocusMinutes;
        _pomodoroService.BreakMinutes = s.PomodoroBreakMinutes;

        WindowInteropHelperNative.SetClickThrough(this, s.GhostMode);
        _trayIconService.UpdateGhostState(s.GhostMode);
        _trayIconService.UpdatePerchMode(s.PerchMode == "ActiveWindow");
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
            Left = workArea.Right - Width - 30;
            Top = workArea.Bottom - Height;
        }
    }

    private void OnBehaviorLoopTick(object? sender, EventArgs e)
    {
        if (_isDragging) return;

        var currentMouse = WindowInteropHelperNative.GetCurrentMousePosition();

        // Active Screen Time Logging (every minute)
        if ((DateTime.Now - _lastActiveMinuteTick).TotalMinutes >= 1)
        {
            _lastActiveMinuteTick = DateTime.Now;
            _statsService.AddActiveMinutes(1);
        }

        // Check if mouse moved
        if (Math.Abs(currentMouse.X - _lastKnownMousePos.X) > 2 ||
            Math.Abs(currentMouse.Y - _lastKnownMousePos.Y) > 2)
        {
            _lastKnownMousePos = currentMouse;
            _lastInteractionTime = DateTime.Now;

            double birdCenterX = Left + (Width / 2);
            double birdCenterY = Top + Height - 50;
            double dist = Math.Sqrt(Math.Pow(currentMouse.X - birdCenterX, 2) + Math.Pow(currentMouse.Y - birdCenterY, 2));

            if (Pet.CurrentState == PetState.Sleeping && dist < 60)
            {
                WakeUp();
            }

            // Tag Game Logic: move towards cursor
            if (_isPlayingTag)
            {
                MoveTowards(currentMouse.X - 70, currentMouse.Y - 100, 18);
                if (dist < 45)
                {
                    _isPlayingTag = false;
                    Pet.SetState(PetState.Happy);
                    _soundService.PlayTrill();
                    Pet.AnimateHearts();
                    SpeechBubble.ShowMessage("খপ করে ধরে ফেলেছি! কিচিরমিচির! 🏃❤️", "🎉", "Tag!", 4);
                }
            }
        }

        // Mouse follow and eye tracking
        if (Pet.CurrentState != PetState.Sleeping && Pet.CurrentState != PetState.NightSleeping && Pet.CurrentState != PetState.Dragged)
        {
            double birdCenterX = Left + (Width / 2);
            double birdCenterY = Top + Height - 50;
            double deltaX = currentMouse.X - birdCenterX;
            double deltaY = currentMouse.Y - birdCenterY;

            Pet.LookAt(deltaX, deltaY);

            double dist = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
            if (dist < 50 && Pet.CurrentState == PetState.Idle && _random.Next(100) < 4)
            {
                Pet.SetState(PetState.CuriousHeadTilt);
                _soundService.PlayChirp();
            }
        }

        // Feature 4: Active Window Perching
        var s = _settingsService.CurrentSettings;
        if (s.PerchMode == "ActiveWindow" && !_isPlayingTag && Pet.CurrentState != PetState.Dragged)
        {
            var myHwnd = new WindowInteropHelper(this).Handle;
            if (WindowPerchService.GetActiveWindowPerchPosition(myHwnd, Width, Height, out var perchTarget))
            {
                MoveTowards(perchTarget.X, perchTarget.Y, 20);
            }
        }

        // Auto-Sleep Check
        if (s.AutoSleepMinutes > 0 && Pet.CurrentState != PetState.Sleeping && Pet.CurrentState != PetState.NightSleeping && Pet.CurrentState != PetState.PomodoroFocus)
        {
            var idleSeconds = (DateTime.Now - _lastInteractionTime).TotalSeconds;
            if (idleSeconds >= s.AutoSleepMinutes * 60)
            {
                PutToSleep();
            }
        }

        // Roaming / Hopping along bottom
        if (s.AllowWalking && s.PerchMode == "Desktop" && Pet.CurrentState == PetState.Idle && !_isPlayingTag)
        {
            var timeSinceRoam = (DateTime.Now - _lastRoamTime).TotalSeconds;
            if (timeSinceRoam >= 45 && _random.Next(100) < 15)
            {
                _lastRoamTime = DateTime.Now;
                PerformRandomHop();
            }
        }
    }

    private void MoveTowards(double targetX, double targetY, double maxStep)
    {
        double diffX = targetX - Left;
        double diffY = targetY - Top;
        double dist = Math.Sqrt(diffX * diffX + diffY * diffY);

        if (dist > 4)
        {
            double step = Math.Min(dist, maxStep);
            Left += (diffX / dist) * step;
            Top += (diffY / dist) * step;

            if (Math.Abs(diffX) > 10)
            {
                Pet.SetFacing(diffX < 0);
            }
        }
    }

    private void CheckNightMode()
    {
        var s = _settingsService.CurrentSettings;
        if (s.NightModeAuto)
        {
            int hour = DateTime.Now.Hour;
            if (hour >= 22 || hour < 6)
            {
                Pet.SetState(PetState.NightSleeping);
                _trayIconService.UpdateSleepState(true);
            }
        }
    }

    private void OnAudioBeatDetected(float peak)
    {
        if (!_settingsService.CurrentSettings.DanceToMusic) return;
        if (Pet.CurrentState == PetState.PomodoroFocus || Pet.CurrentState == PetState.Dragged) return;

        // When music beats are detected, dance!
        if (Pet.CurrentState != PetState.Dancing)
        {
            Pet.SetState(PetState.Dancing);
            _lastDanceTime = DateTime.Now;
        }
        else
        {
            _lastDanceTime = DateTime.Now;
        }

        // Return to idle 1.5 seconds after music stops
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1600) };
        timer.Tick += (s, e) =>
        {
            timer.Stop();
            if ((DateTime.Now - _lastDanceTime).TotalMilliseconds >= 1400 && Pet.CurrentState == PetState.Dancing)
            {
                Pet.SetState(PetState.Idle);
            }
        };
        timer.Start();
    }

    #region Pomodoro Handlers

    private void TogglePomodoro()
    {
        if (_pomodoroService.CurrentPhase == PomodoroPhase.Stopped)
        {
            _pomodoroService.StartFocus();
        }
        else
        {
            _pomodoroService.Stop();
        }
    }

    private void OnPomodoroTick(TimeSpan remaining, PomodoroPhase phase)
    {
        string phaseName = phase == PomodoroPhase.Focus ? "🎯 Focus" : "☕ Break";
        string timeStr = $"{phaseName}: {remaining.Minutes:D2}:{remaining.Seconds:D2}";
        _trayIconService.UpdatePomodoroState(true);
    }

    private void OnPomodoroPhaseChanged(PomodoroPhase phase)
    {
        switch (phase)
        {
            case PomodoroPhase.Focus:
                WakeUp();
                Pet.SetState(PetState.PomodoroFocus);
                _soundService.PlayChime();
                SpeechBubble.ShowMessage("পোমোডোরো শুরু! শান্তভাবে মনোযোগ দাও 🎯 👓", "🍅", "Pomodoro Focus", 6);
                _trayIconService.UpdatePomodoroState(true);
                break;

            case PomodoroPhase.ShortBreak:
            case PomodoroPhase.LongBreak:
                Pet.SetState(PetState.PomodoroBreak);
                _soundService.PlayTrill();
                _statsService.LogPomodoro();
                _settingsService.CurrentSettings.FriendshipXP += 50;
                _settingsService.SaveSettings(_settingsService.CurrentSettings);

                SpeechBubble.ShowMessage("🎉 ফোকাস শেষ! ডানা ঝাপটে ৫ মিনিট বিশ্রাম নাও! (+50 XP)", "🥳", "Break Time!", 8);
                break;

            case PomodoroPhase.Stopped:
                Pet.SetState(PetState.Idle);
                _trayIconService.UpdatePomodoroState(false);
                break;
        }
    }

    #endregion

    #region AI & Ask Parot

    private async void OnQuestionAsked(string query)
    {
        SpeechBubble.ShowMessage("টিয়া পাখি ভাবছে... 🤔", "🦜", "Parot AI", 0);
        Pet.SetState(PetState.CuriousHeadTilt);

        var answer = await _voiceMimicService.AskParotAsync(query, _settingsService.CurrentSettings.GeminiApiKey);

        SpeechBubble.ShowMessage(answer, "🦜", "Parot", 8);
        Pet.SetState(PetState.Speaking);

        if (_settingsService.CurrentSettings.VoiceMimicEnabled)
        {
            _voiceMimicService.Speak(answer);
        }
    }

    private void OnWaterLogged()
    {
        _statsService.LogWaterGlass();
        _settingsService.CurrentSettings.FriendshipXP += 15;
        _settingsService.SaveSettings(_settingsService.CurrentSettings);
        _soundService.PlayDoubleChirp();
        Pet.AnimateHearts();
    }

    #endregion

    #region Tag Mini-Game

    private void StartPlayTag()
    {
        WakeUp();
        _isPlayingTag = true;
        Pet.SetState(PetState.FlappingWings);
        _soundService.PlayTrill();
        SpeechBubble.ShowMessage("মাউস সরাও! দেখি কে কাকে ধরতে পারে! 🏃🦜", "🎮", "Play Tag!", 4);
    }

    #endregion

    #region Dragging & Interactions

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

        if (Pet.CurrentState == PetState.Sleeping || Pet.CurrentState == PetState.NightSleeping)
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
            OnPetSingleClicked();
        }
    }

    private void Pet_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        MenuSleepWake.Header = (Pet.CurrentState == PetState.Sleeping || Pet.CurrentState == PetState.NightSleeping)
            ? "☀️ জেগে ওঠো (Wake Up)"
            : "💤 ঘুমাতে যাও (Go to Sleep)";

        MenuPomodoro.Header = _pomodoroService.CurrentPhase == PomodoroPhase.Stopped
            ? "🍅 পোমোডোরো শুরু করুন (Start Focus)"
            : "🍅 পোমোডোরো বন্ধ করুন (Stop Focus)";

        MenuPerchMode.Header = _settingsService.CurrentSettings.PerchMode == "ActiveWindow"
            ? "🖥️ ডেস্কটপে ফিরে এসো (Perch on Desktop)"
            : "🪟 উইন্ডোর ওপরে বসো (Perch on Active Window)";

        PetContextMenu.IsOpen = true;
    }

    private void OnDragFinished()
    {
        var workArea = SystemParameters.WorkArea;

        if (Top > workArea.Bottom - Height - 70)
        {
            Top = workArea.Bottom - Height;
        }

        Left = Math.Clamp(Left, workArea.Left, workArea.Right - Width);

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

        if (Pet.CurrentState == PetState.Sleeping || Pet.CurrentState == PetState.NightSleeping)
        {
            WakeUp();
        }
        else
        {
            Pet.SetState(PetState.Happy);
            _soundService.PlayDoubleChirp();
            _statsService.LogPet();
            _settingsService.CurrentSettings.FriendshipXP += 5;
            _settingsService.SaveSettings(_settingsService.CurrentSettings);

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
        if (Pet.CurrentState == PetState.Sleeping || Pet.CurrentState == PetState.NightSleeping)
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

    #region State Controls

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
        int hour = DateTime.Now.Hour;
        if (_settingsService.CurrentSettings.NightModeAuto && (hour >= 22 || hour < 6))
        {
            Pet.SetState(PetState.NightSleeping);
        }
        else
        {
            Pet.SetState(PetState.Sleeping);
        }

        _trayIconService.UpdateSleepState(true);
        _soundService.PlaySnore();
        SpeechBubble.HideBubble();
    }

    public void ToggleSleepWake()
    {
        if (Pet.CurrentState == PetState.Sleeping || Pet.CurrentState == PetState.NightSleeping)
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
        _statsService.LogTreat();
        _settingsService.CurrentSettings.FriendshipXP += 10;
        _settingsService.SaveSettings(_settingsService.CurrentSettings);

        var treatQuote = Dialogues.GetTreatDialogue(_settingsService.CurrentSettings.Language);
        SpeechBubble.ShowMessage(treatQuote, "🌶️", "Parot Treat", 6);
    }

    public void FeedSunflowerSeeds()
    {
        WakeUp();
        Pet.SetState(PetState.Eating);
        _soundService.PlayMunch();
        _statsService.LogTreat();
        _settingsService.CurrentSettings.FriendshipXP += 10;
        _settingsService.SaveSettings(_settingsService.CurrentSettings);

        var treatQuote = Dialogues.GetTreatDialogue(_settingsService.CurrentSettings.Language);
        SpeechBubble.ShowMessage(treatQuote, "🌻", "Parot Treat", 6);
    }

    private void TogglePerchMode()
    {
        var s = _settingsService.CurrentSettings;
        s.PerchMode = s.PerchMode == "ActiveWindow" ? "Desktop" : "ActiveWindow";
        _settingsService.SaveSettings(s);
        ApplySettings(s);

        string modeName = s.PerchMode == "ActiveWindow" ? "সক্রিয় উইন্ডোর উপরে" : "ডেস্কটপ টাস্কবারে";
        SpeechBubble.ShowMessage($"বসার স্থান পরিবর্তন: {modeName} 🪟", "🪟", "Perch Mode", 4);
    }

    private void PerformRandomHop()
    {
        var workArea = SystemParameters.WorkArea;
        bool isAtBottom = Math.Abs(Top - (workArea.Bottom - Height)) < 80;
        if (!isAtBottom) return;

        bool hopLeft = _random.Next(2) == 0;
        double hopDistance = _random.Next(35, 75);

        if (hopLeft && (Left - hopDistance) < workArea.Left + 20) hopLeft = false;
        else if (!hopLeft && (Left + hopDistance) > workArea.Right - Width - 20) hopLeft = true;

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

    #endregion

    #region Reminders

    private void OnReminderTriggered(object? sender, ReminderEventArgs e)
    {
        if (Pet.CurrentState == PetState.Sleeping || Pet.CurrentState == PetState.NightSleeping)
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

            if (type == ReminderType.ScreenBreak)
            {
                _statsService.LogEyeBreak();
                _settingsService.CurrentSettings.FriendshipXP += 20;
                _settingsService.SaveSettings(_settingsService.CurrentSettings);
            }
        }
        else
        {
            _reminderService.Snooze(type, 5);
            _soundService.PlayChirp();
            SpeechBubble.ShowMessage("৫ মিনিটের জন্য মনে করিয়ে দেব! ⏳", "⏳", "Snoozed", 3);
        }
    }

    #endregion

    #region Window & Dialog Openers

    private void OpenSettingsWindow()
    {
        var settingsWin = new SettingsWindow(_settingsService, _soundService, _statsService)
        {
            Owner = this
        };
        settingsWin.FeedTreatRequested += FeedChiliTreat;
        settingsWin.OpenDailyReportRequested += OpenDailyReportWindow;
        settingsWin.ShowDialog();
    }

    private void OpenDailyReportWindow()
    {
        var reportWin = new DailyReportWindow(_statsService, _settingsService.CurrentSettings)
        {
            Owner = this
        };
        reportWin.ShowDialog();
    }

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

    private void ExitApplication()
    {
        _loopTimer.Stop();
        _audioDetectorService.Dispose();
        _voiceMimicService.Dispose();
        _trayIconService.Dispose();
        Application.Current.Shutdown();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        _audioDetectorService.Dispose();
        _voiceMimicService.Dispose();
        _trayIconService.Dispose();
    }

    #endregion

    #region Context Menu Handlers

    private void MenuPomodoro_Click(object sender, RoutedEventArgs e) => TogglePomodoro();
    private void MenuAskParot_Click(object sender, RoutedEventArgs e) => SpeechBubble.PromptAskParot();
    private void MenuDailyReport_Click(object sender, RoutedEventArgs e) => OpenDailyReportWindow();
    private void MenuPerchMode_Click(object sender, RoutedEventArgs e) => TogglePerchMode();
    private void MenuPlayTag_Click(object sender, RoutedEventArgs e) => StartPlayTag();
    private void MenuSquawk_Click(object sender, RoutedEventArgs e) => OnPetDoubleClicked();
    private void MenuFeedChili_Click(object sender, RoutedEventArgs e) => FeedChiliTreat();
    private void MenuFeedSeeds_Click(object sender, RoutedEventArgs e) => FeedSunflowerSeeds();
    private void MenuSleepWake_Click(object sender, RoutedEventArgs e) => ToggleSleepWake();
    private void MenuEyeBreak_Click(object sender, RoutedEventArgs e) => _reminderService.TriggerBreakNow();
    private void MenuHydrate_Click(object sender, RoutedEventArgs e) => _reminderService.TriggerHydrationNow();
    private void MenuGhostMode_Click(object sender, RoutedEventArgs e) => ToggleGhostMode();
    private void MenuSettings_Click(object sender, RoutedEventArgs e) => OpenSettingsWindow();
    private void MenuExit_Click(object sender, RoutedEventArgs e) => ExitApplication();

    #endregion
}