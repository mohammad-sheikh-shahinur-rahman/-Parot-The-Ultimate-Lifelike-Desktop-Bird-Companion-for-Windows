using System;
using System.IO;
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
    private readonly FlightMovementService _flightMovementService;
    private readonly WeatherService _weatherService;
    private readonly DevWatcherService _devWatcherService;
    private readonly TypingDetectorService _typingDetectorService;
    private readonly MicrophoneMimicService _micMimicService;
    private readonly MoodService _moodService;
    private readonly OsAwarenessService _osAwarenessService;

    private BouncyBallWindow? _bouncyBallWindow;
    private CompanionWindow? _companionWindow;
    private TreatItemWindow? _activeTreatWindow;
    private LaserPointerWindow? _laserWindow;
    private DesktopMirrorWindow? _mirrorWindow;
    private bool _isPerchedOnCursor = false;
    private DateTime _lastHungerDecayTime = DateTime.Now;
    private DateTime _lastHungerWarningTime = DateTime.MinValue;

    // Window Perch & Drag Tracking & Titlebar Walking
    private IntPtr _perchedWindowHwnd = IntPtr.Zero;
    private WindowPerchService.RECT _lastPerchedRect;
    private bool _isWalkingAlongTitlebar = false;
    private double _walkDirection = 1.0;
    private DateTime _lastWalkTime = DateTime.Now;
    private DateTime _lastAvianBehaviorTime = DateTime.Now;

    // Dizzy Cursor Circles Tracking
    private readonly System.Collections.Generic.List<(DateTime Time, double Angle)> _recentMouseAngles = new();
    private DateTime _lastDizzyTime = DateTime.MinValue;

    private readonly DispatcherTimer _loopTimer;
    private readonly Random _random = new();

    private bool _isDragging = false;
    private Point _dragStartCursorPos;
    private Point _dragStartWindowPos;

    private DateTime _lastInteractionTime = DateTime.Now;
    private Point _lastKnownMousePos;
    private DateTime _lastRoamTime = DateTime.Now;
    private DateTime _lastFlightTime = DateTime.Now;
    private DateTime _lastActiveMinuteTick = DateTime.Now;
    private DateTime _lastDanceTime = DateTime.Now;
    private DateTime _lastStretchTime = DateTime.Now;
    private DateTime _lastHuntTime = DateTime.Now.AddSeconds(-20);
    private DateTime _lastAutonomousActionTime = DateTime.Now;
    private int _nextAutonomousIntervalSeconds = 7;
    private bool _isPlayingTag = false;

    private bool CanPerformAutonomousAction
    {
        get
        {
            if (_flightMovementService.IsFlying || _isDragging || _isPlayingTag || _isWalkingAlongTitlebar)
                return false;

            if (Pet.CurrentState == PetState.Sleeping || Pet.CurrentState == PetState.NightSleeping ||
                Pet.CurrentState == PetState.PomodoroFocus || Pet.CurrentState == PetState.Eating ||
                Pet.CurrentState == PetState.Dizzy)
                return false;

            // Auto-recovery: if a transient visual action has been active for >= 3.5 seconds, safely reset to Idle
            if (Pet.CurrentState != PetState.Idle && Pet.CurrentState != PetState.Perching)
            {
                if ((DateTime.Now - _lastAutonomousActionTime).TotalSeconds >= 3.5)
                {
                    Pet.SetState(PetState.Idle);
                    return true;
                }
                return false;
            }

            return true;
        }
    }

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
        _flightMovementService = new FlightMovementService();
        _weatherService = new WeatherService();
        _weatherService.WeatherUpdated += OnWeatherUpdated;

        _devWatcherService = new DevWatcherService();
        _devWatcherService.BuildSucceeded += OnBuildSucceeded;
        _devWatcherService.BuildFailed += OnBuildFailed;

        _typingDetectorService = new TypingDetectorService();
        _typingDetectorService.FastTypingDetected += OnFastTypingDetected;

        _micMimicService = new MicrophoneMimicService(s.VoiceCommandsEnabled);
        _micMimicService.SoundEnabled = s.SoundEnabled;
        _voiceMimicService.MasterSoundEnabled = s.SoundEnabled;
        _micMimicService.RecordingStarted += () =>
        {
            Pet.SetState(PetState.CuriousHeadTilt);
            _soundService.PlayChirp();
            SpeechBubble.ShowMessage("🎙️ কান পেতে শুনছি... কিছু একটা বলো! (Listening...)", "👂", "Voice Mimic", 3);
        };
        _micMimicService.MimicPlaybackStarted += () =>
        {
            Pet.SetState(PetState.Speaking);
            _soundService.PlayTrill();
            SpeechBubble.ShowMessage("🦜 কিচিরমিচির! অবিকল টিয়া পাখির গলায় রিপিট করছি!", "🎶", "Voice Mimic", 4);
        };
        _micMimicService.MimicPlaybackCompleted += () =>
        {
            if (Pet.CurrentState == PetState.Speaking)
            {
                Pet.SetState(PetState.Idle);
            }
        };
        _micMimicService.CommandRecognized += OnVoiceCommandRecognized;

        // Flight Movement Events
        _flightMovementService.TakeOffInitiated += OnFlightTakeOff;
        _flightMovementService.FlyingInitiated += () => Pet.SetState(PetState.Flying);
        _flightMovementService.PositionUpdated += OnFlightPositionUpdated;
        _flightMovementService.LandingInitiated += OnFlightLanding;
        _flightMovementService.FlightCompleted += OnFlightCompleted;

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
        _trayIconService.FlyRequested += TriggerAutonomousFlight;
        _trayIconService.FollowCursorToggled += () =>
        {
            var curS = _settingsService.CurrentSettings;
            curS.FollowCursor = !curS.FollowCursor;
            _settingsService.SaveSettings(curS);
            SpeechBubble.ShowMessage(curS.FollowCursor ? "আমি এখন তোমার কার্সারের পিছু নেব! 🎯" : "কার্সার অনুসরণ বন্ধ করা হয়েছে।", "🦜", "Follow Cursor", 4);
        };
        _trayIconService.SoundToggled += ToggleSound;
        _trayIconService.PerchModeToggled += TogglePerchMode;
        _trayIconService.AskParotRequested += () => SpeechBubble.PromptAskParot();
        _trayIconService.GhostModeToggled += ToggleGhostMode;
        _trayIconService.SettingsRequested += OpenSettingsWindow;
        _trayIconService.ExitRequested += ExitApplication;

        // Mood & Emotion System
        _moodService = new MoodService();
        _moodService.MoodChanged += (oldM, newM) => Dispatcher.Invoke(UpdateMoodDisplay);

        // OS & Hardware Awareness
        _osAwarenessService = new OsAwarenessService();
        _osAwarenessService.ChargerPluggedIn += () =>
        {
            Dispatcher.Invoke(() =>
            {
                WakeUp();
                Pet.SetState(PetState.Happy);
                _soundService.PlayHappy();
                Pet.AnimateHearts();
                SpeechBubble.ShowMessage(LocalizationService.Get("ChargerConnected", _settingsService.CurrentSettings.Language), "⚡", "Battery Charged", 5);
            });
        };
        _osAwarenessService.ChargerUnplugged += () =>
        {
            Dispatcher.Invoke(() =>
            {
                SpeechBubble.ShowMessage("🔋 চার্জার খোলা হয়েছে! ব্যাটারিতে চলছি!", "🔋", "Battery", 3);
            });
        };
        _osAwarenessService.LowBatteryWarning += (pct) =>
        {
            Dispatcher.Invoke(() =>
            {
                Pet.Yawn();
                SpeechBubble.ShowMessage(LocalizationService.Get("BatteryLow", _settingsService.CurrentSettings.Language), "🪫", "Low Battery", 6);
            });
        };
        _osAwarenessService.ClipboardTextCopied += (snippet) =>
        {
            Dispatcher.Invoke(() =>
            {
                Pet.BobHead();
                _soundService.PlayChirp();
                _moodService.RegisterCuriosity();
                SpeechBubble.ShowMessage($"কপি করেছো: \"{snippet}\" 📋", "📋", "Clipboard Watcher", 3);
            });
        };
        _osAwarenessService.NetworkAvailabilityChanged += (isAvailable) =>
        {
            Dispatcher.Invoke(() =>
            {
                if (isAvailable)
                {
                    _soundService.PlayDoubleChirp();
                    SpeechBubble.ShowMessage("ইন্টারনেট ফিরে এসেছে! কিচিরমিচির! 🌐✨", "🌐", "Internet", 4);
                }
                else
                {
                    Pet.SetState(PetState.CuriousHeadTilt);
                    SpeechBubble.ShowMessage("ইন্টারনেট সংযোগ চলে গেছে! 📡❌", "❓", "Internet Offline", 5);
                }
            });
        };

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
        Pet.SetCageMode(s.BirdcageMode);
        UpdateBirdcageMenuItem(s.BirdcageMode);

        Topmost = s.AlwaysOnTop;
        SoundService.MasterMute = !s.SoundEnabled;
        _soundService.Enabled = s.SoundEnabled;
        _soundService.Volume = s.SoundVolume;
        _voiceMimicService.MasterSoundEnabled = s.SoundEnabled;
        _micMimicService.SoundEnabled = s.SoundEnabled;
        if (!s.SoundEnabled)
        {
            _soundService.StopAll();
            _voiceMimicService.Stop();
            _micMimicService.StopPlayback();
        }
        _trayIconService.UpdateSoundState(s.SoundEnabled);
        UpdateSoundMenuItems(s.SoundEnabled);
        _voiceMimicService.Enabled = s.VoiceMimicEnabled;
        _voiceMimicService.VoiceRate = s.VoicePitchRate;
        _audioDetectorService.Enabled = s.DanceToMusic;

        _reminderService.UpdateSettings(s);
        _pomodoroService.FocusMinutes = s.PomodoroFocusMinutes;
        _pomodoroService.BreakMinutes = s.PomodoroBreakMinutes;

        WindowInteropHelperNative.SetClickThrough(this, s.GhostMode);
        _trayIconService.UpdateGhostState(s.GhostMode);
        _trayIconService.UpdatePerchMode(s.PerchMode == "ActiveWindow");

        ApplyLanguage(s.Language);
    }

    private Rect GetCurrentWorkArea()
    {
        var centerPoint = new Point(Left + Width / 2.0, Top + Height / 2.0);
        return WindowInteropHelperNative.GetMonitorWorkAreaFromPoint(centerPoint);
    }

    private void PositionWindowInitially()
    {
        var workArea = GetCurrentWorkArea();
        var s = _settingsService.CurrentSettings;

        if (s.LastPosX >= workArea.Left && s.LastPosY >= workArea.Top &&
            s.LastPosX < workArea.Right && s.LastPosY < workArea.Bottom)
        {
            Left = s.LastPosX;
            Top = s.LastPosY;
        }
        else
        {
            Left = workArea.Right - Width - 30;
            Top = workArea.Bottom - Height + 6;
        }

        // Clamp safely within visible screen
        Left = Math.Clamp(Left, workArea.Left, Math.Max(workArea.Left, workArea.Right - Width));
        Top = Math.Clamp(Top, workArea.Top, Math.Max(workArea.Top, workArea.Bottom - Height));
    }

    private void OnBehaviorLoopTick(object? sender, EventArgs e)
    {
        try
        {
            if (_isDragging || _flightMovementService.IsFlying) return;

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

                if (Pet.CurrentState == PetState.Sleeping && dist < 65)
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

            // Mouse gaze / eye tracking
            if (Pet.CurrentState != PetState.Sleeping && Pet.CurrentState != PetState.NightSleeping && Pet.CurrentState != PetState.Dragged && Pet.CurrentState != PetState.Flying && Pet.CurrentState != PetState.Eating)
            {
                double birdCenterX = Left + (Width / 2);
                double birdCenterY = Top + Height - 50;
                double deltaX = currentMouse.X - birdCenterX;
                double deltaY = currentMouse.Y - birdCenterY;

                Pet.LookAt(deltaX, deltaY);

                double dist = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
                if (dist < 50 && CanPerformAutonomousAction && _random.Next(100) < 6)
                {
                    _soundService.PlayCurious();
                    Pet.CuriousLookAround();
                }

                // Dizzy Cursor Circles Tracking (Rapid circular mouse motion around bird)
                if (Pet.CurrentState != PetState.Dizzy)
                {
                    double currentAngle = Math.Atan2(deltaY, deltaX);
                    _recentMouseAngles.Add((DateTime.Now, currentAngle));
                    _recentMouseAngles.RemoveAll(a => (DateTime.Now - a.Time).TotalSeconds > 1.2);

                    if (_recentMouseAngles.Count >= 7 && (DateTime.Now - _lastDizzyTime).TotalSeconds >= 10)
                    {
                        double cumulativeRotation = 0;
                        for (int i = 1; i < _recentMouseAngles.Count; i++)
                        {
                            double diff = _recentMouseAngles[i].Angle - _recentMouseAngles[i - 1].Angle;
                            while (diff > Math.PI) diff -= 2 * Math.PI;
                            while (diff < -Math.PI) diff += 2 * Math.PI;
                            cumulativeRotation += diff;
                        }

                        if (Math.Abs(cumulativeRotation) >= 5 * Math.PI)
                        {
                            _lastDizzyTime = DateTime.Now;
                            _recentMouseAngles.Clear();
                            _soundService.PlaySquawk();
                            SpeechBubble.ShowMessage("উহুহু... মাথা ঘুরছে! এত দ্রুত মাউস ঘোরাচ্ছো কেন! 😵💫", "😵💫", "Dizzy!", 4);
                            Pet.MakeDizzy();
                        }
                    }
                }
            }

            var s = _settingsService.CurrentSettings;

            // Auto-Sleep Check
            if (s.AutoSleepMinutes > 0 && Pet.CurrentState != PetState.Sleeping && Pet.CurrentState != PetState.NightSleeping && Pet.CurrentState != PetState.PomodoroFocus && Pet.CurrentState != PetState.Eating)
            {
                var idleSeconds = (DateTime.Now - _lastInteractionTime).TotalSeconds;
                if (idleSeconds >= s.AutoSleepMinutes * 60)
                {
                    PutToSleep();
                    return;
                }
            }

            // Daytime Nap Natural Wake-Up (after 90s of sleep during daytime)
            if (Pet.CurrentState == PetState.Sleeping)
            {
                int hour = DateTime.Now.Hour;
                bool isDaytime = hour >= 6 && hour < 22;
                var sleepDuration = (DateTime.Now - _lastInteractionTime).TotalSeconds - (s.AutoSleepMinutes * 60);
                if (isDaytime && sleepDuration >= 90 && _random.Next(100) < 15)
                {
                    WakeUp();
                    SpeechBubble.ShowMessage("মিষ্টি একটা ঘুম দিয়ে উঠে পড়লাম! একদম চাঙ্গা! ☀️🥱", "☀️", "Woke Up", 4);
                    _soundService.PlayDoubleChirp();
                    Pet.FluffFeathers();
                    return;
                }
            }

            // Perched Window Tracking (Window Drag Tracking, Minimizing Startle)
            if (_perchedWindowHwnd != IntPtr.Zero && !_flightMovementService.IsFlying && !_isDragging)
            {
                if (!WindowPerchService.TryGetWindowRect(_perchedWindowHwnd, out var currentRect))
                {
                    // Window closed or minimized! Startled flight!
                    _perchedWindowHwnd = IntPtr.Zero;
                    WakeUp();
                    SpeechBubble.ShowMessage("আরে! বসার ডালটা গেল কই! 😲", "😲", "Startled!", 4);
                    _soundService.PlaySquawk();
                    Pet.SetState(PetState.FlappingWings);
                    TriggerAutonomousFlight();
                    return;
                }

                // Window moved by user (Window Drag Tracking)
                double dx = currentRect.Left - _lastPerchedRect.Left;
                double dy = currentRect.Top - _lastPerchedRect.Top;
                if (Math.Abs(dx) > 1 || Math.Abs(dy) > 1)
                {
                    Left += dx;
                    Top += dy;
                    _lastPerchedRect = currentRect;
                }
            }

            // Unified Autonomous Living Avian Decision Engine (পাখির স্বয়ংক্রিয় জীবনচক্র)
            if (CanPerformAutonomousAction)
            {
                var timeSinceAction = (DateTime.Now - _lastAutonomousActionTime).TotalSeconds;
                if (timeSinceAction >= _nextAutonomousIntervalSeconds)
                {
                    _lastAutonomousActionTime = DateTime.Now;
                    _nextAutonomousIntervalSeconds = _random.Next(6, 12);
                    PerformNextAutonomousAction();
                    return;
                }
            }

            // Follow Cursor Mode Flight (only when explicitly enabled)
            if (s.FollowCursor && !_flightMovementService.IsFlying && Pet.CurrentState != PetState.Sleeping && Pet.CurrentState != PetState.NightSleeping && !_isDragging)
            {
                double birdCenterX = Left + (Width / 2);
                double birdCenterY = Top + Height - 50;
                double dist = Math.Sqrt(Math.Pow(currentMouse.X - birdCenterX, 2) + Math.Pow(currentMouse.Y - birdCenterY, 2));
                if (dist > 450)
                {
                    var workArea = GetCurrentWorkArea();
                    double targetX = Math.Clamp(currentMouse.X - (Width / 2), workArea.Left, workArea.Right - Width);
                    double targetY = Math.Clamp(currentMouse.Y - Height + 60, workArea.Top, workArea.Bottom - Height);
                    StartFlightTo(new Point(targetX, targetY));
                    return;
                }
            }

            // Perch on Cursor Mode
            if (_isPerchedOnCursor && !_isDragging && !_flightMovementService.IsFlying)
            {
                double targetLeft = currentMouse.X - (Width / 2);
                double targetTop = currentMouse.Y - Height + 14;
                double dX = targetLeft - Left;
                double dY = targetTop - Top;
                if (Math.Abs(dX) > 2 || Math.Abs(dY) > 2)
                {
                    Left += dX * 0.35;
                    Top += dY * 0.35;
                    Pet.SetFacing(dX < 0);
                }
            }

            // Ball Push Mini-Game Interaction
            if (_bouncyBallWindow != null && _bouncyBallWindow.IsLoaded && !_flightMovementService.IsFlying && (Pet.CurrentState == PetState.Idle || Pet.CurrentState == PetState.Perching))
            {
                var ballPos = _bouncyBallWindow.CurrentPosition;
                double birdCenterX = Left + (Width / 2);
                double distToBall = Math.Abs(ballPos.X - birdCenterX);
                double verticalDist = Math.Abs(ballPos.Y - (Top + Height));

                if (distToBall < 110 && verticalDist < 80 && Math.Abs(_bouncyBallWindow.VelocityX) < 45)
                {
                    Pet.SetFacing(ballPos.X < birdCenterX);
                    Pet.PeckAndEat("⚽", () => _soundService.PlayChirp(), () =>
                    {
                        double pushForce = (birdCenterX < ballPos.X) ? 380 : -380;
                        _bouncyBallWindow.ParotPush(pushForce);
                        _soundService.PlayDoubleChirp();
                        SpeechBubble.ShowMessage("গোল! তোমার দিকে ঠেলে দিয়েছি! ⚽💨", "⚽", "Goal!", 3);
                    });
                }
            }

            // Companion Sync
            if (_companionWindow != null && _companionWindow.IsLoaded)
            {
                _companionWindow.SetLeaderTarget(Left, Top, Pet.CurrentState);
            }

            // Hunger Decay (every 2 minutes -0.8%)
            if ((DateTime.Now - _lastHungerDecayTime).TotalMinutes >= 2)
            {
                _lastHungerDecayTime = DateTime.Now;
                s.HungerLevel = Math.Max(0, s.HungerLevel - 0.8);
                _settingsService.SaveSettings(s);

                if (s.HungerLevel < 25 && (DateTime.Now - _lastHungerWarningTime).TotalMinutes >= 8)
                {
                    _lastHungerWarningTime = DateTime.Now;
                    _soundService.PlayCurious();
                    SpeechBubble.ShowMessage("পেটে ভীষণ খিদে! আমাকে একটা মরিচ বা বাদাম খাওয়াও না প্লিজ! 🌶️🥜 (Hungry!)", "🍽️", "Hungry", 6);
                }
            }

            if (MenuHungerDisplay != null)
            {
                MenuHungerDisplay.Header = $"❤️ ক্ষুধা লেভেল: {s.HungerLevel:F0}%";
            }

            // Dynamic Mood Updates
            _moodService.UpdateMoodFromInactivity(DateTime.Now - _lastInteractionTime, DateTime.Now.Hour >= 22 || DateTime.Now.Hour < 6);
            UpdateMoodDisplay();
        }
        catch (Exception ex)
        {
            try
            {
                File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tick_exception.log"), $"[{DateTime.Now}] Tick error:\n{ex}\n\n");
            }
            catch { }
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
        }
    }

    #region Flight Engine Integration

    private void OnFlightTakeOff()
    {
        Pet.SetState(PetState.TakeOff);
        _soundService.PlayChirp();
    }

    private void OnFlightPositionUpdated(Point pos, double bankAngle, bool faceLeft)
    {
        Left = pos.X;
        Top = pos.Y;
        Pet.SetFacing(faceLeft);
        // Correct bank angle for flipped X coordinate
        Pet.SetBankAngle(faceLeft ? -bankAngle : bankAngle);


        // Check if catching treat mid-air
        if (_activeTreatWindow != null && !_activeTreatWindow.IsCaught)
        {
            var treatPos = _activeTreatWindow.CurrentPosition;
            double beakX = Left + (faceLeft ? 40 : Width - 40);
            double beakY = Top + Height - 120;
            double dist = Math.Sqrt(Math.Pow(beakX - treatPos.X, 2) + Math.Pow(beakY - treatPos.Y, 2));

            if (dist < 85)
            {
                _activeTreatWindow.Catch();
                _activeTreatWindow = null;
                Pet.PeckAndEat("🌶️",
                    () => _soundService.PlayMunch(),
                    () =>
                    {
                        _soundService.PlayTrill();
                        Pet.AnimateHearts();
                        _settingsService.CurrentSettings.FriendshipXP += 30;
                        _settingsService.CurrentSettings.HungerLevel = 100;
                        _settingsService.SaveSettings(_settingsService.CurrentSettings);
                        SpeechBubble.ShowMessage("আকাশে উড়ন্ত অবস্থায় ক্যাচ ধরে ফেলেছি! 🎯✨ (+30 XP)", "🏆", "Mid-Air Catch!", 4);
                    });
            }
        }

        // Keep companion bird updated
        if (_companionWindow != null && _companionWindow.IsLoaded)
        {
            _companionWindow.SetLeaderTarget(pos.X, pos.Y, Pet.CurrentState);
        }
    }

    private void OnFlightLanding()
    {
        Pet.SetState(PetState.Landing);
    }

    private void OnFlightCompleted()
    {
        Pet.SetBankAngle(0);
        Pet.SetState(PetState.Perching);
        _soundService.PlayDoubleChirp();
    }

    public void StartFlightTo(Point targetPoint)
    {
        if (_isDragging || Pet.CurrentState == PetState.Sleeping || Pet.CurrentState == PetState.NightSleeping) return;
        _flightMovementService.FlyTo(new Point(Left, Top), targetPoint);
    }

    public void TriggerAutonomousFlight()
    {
        var s = _settingsService.CurrentSettings;
        if (s.BirdcageMode || !s.AutonomousFlight) return;

        var workArea = GetCurrentWorkArea();
        var myHwnd = new WindowInteropHelper(this).Handle;

        bool hasCandidateWindow = WindowPerchService.GetRandomWindowPerchPosition(myHwnd, Width, Height, out var windowPerch, out var perchedHwnd, out var windowTitle);

        // Check whether currently sitting on or near taskbar bottom
        bool isNearBottom = Top >= workArea.Bottom - Height - 60;

        // 1. If on bottom and a candidate window is available, 75% chance to fly directly to window titlebar!
        if (isNearBottom && hasCandidateWindow && _random.Next(100) < 75)
        {
            _perchedWindowHwnd = perchedHwnd;
            WindowPerchService.TryGetWindowRect(perchedHwnd, out _lastPerchedRect);
            StartFlightTo(windowPerch);
            return;
        }

        // 2. If already perched on a window, 50% chance to fly to another window if another window exists, or fly down to taskbar
        if (!isNearBottom)
        {
            if (hasCandidateWindow && perchedHwnd != _perchedWindowHwnd && _random.Next(100) < 50)
            {
                _perchedWindowHwnd = perchedHwnd;
                WindowPerchService.TryGetWindowRect(perchedHwnd, out _lastPerchedRect);
                StartFlightTo(windowPerch);
                return;
            }

            _perchedWindowHwnd = IntPtr.Zero;
            double targetX = _random.Next(80, (int)Math.Max(120, workArea.Right - Width - 80));
            double targetY = workArea.Bottom - Height + 6;
            StartFlightTo(new Point(targetX, targetY));
            return;
        }

        // 3. Otherwise, pick between window or taskbar
        if (hasCandidateWindow && _random.Next(100) < 55)
        {
            _perchedWindowHwnd = perchedHwnd;
            WindowPerchService.TryGetWindowRect(perchedHwnd, out _lastPerchedRect);
            StartFlightTo(windowPerch);
        }
        else
        {
            _perchedWindowHwnd = IntPtr.Zero;
            double targetX = _random.Next(60, (int)Math.Max(120, workArea.Right - Width - 60));
            double targetY = workArea.Bottom - Height + 6;
            StartFlightTo(new Point(targetX, targetY));
        }
    }

    #endregion

    #region Hunting & Foraging Feature ("শিকার করে খায়")

    public void TriggerAutonomousHunt(string? preyType = null)
    {
        if (_flightMovementService.IsFlying || Pet.CurrentState == PetState.Sleeping || Pet.CurrentState == PetState.NightSleeping || _isDragging) return;

        (string emoji, string nameBn, string nameEn)[] preys =
        [
            ("🐞", "লাল লেডিবাগ পোকা", "Ladybug"),
            ("🦗", "ঘাসফড়িং", "Grasshopper"),
            ("🌻", "সূর্যমুখী বীজ", "Sunflower Seed"),
            ("🌶️", "ঝাল কাঁচা লঙ্কা", "Chili Pepper")
        ];

        var selectedPrey = preyType switch
        {
            "Ladybug" or "🐞" => preys[0],
            "Grasshopper" or "🦗" => preys[1],
            "Seed" or "🌻" => preys[2],
            "Chili" or "🌶️" => preys[3],
            _ => preys[_random.Next(preys.Length)]
        };

        _lastInteractionTime = DateTime.Now;

        // 1. Announce prey spotted
        var spotMsg = _settingsService.CurrentSettings.Language switch
        {
            "Bengali" => $"👀 একি! একটা {selectedPrey.nameBn} দেখা যাচ্ছে!",
            "English" => $"👀 Look! A fresh {selectedPrey.nameEn} appeared!",
            _ => $"👀 শিকার দেখতে পেয়েছি! {selectedPrey.nameBn} ({selectedPrey.nameEn})!"
        };
        SpeechBubble.ShowMessage(spotMsg, selectedPrey.emoji, "Hunter", 3);
        _soundService.PlayChirp();

        // 2. Perform realistic downward pecks with munch sounds
        Pet.PeckAndEat(selectedPrey.emoji,
            onPeckSound: () =>
            {
                _soundService.PlayMunch();
            },
            onCompleted: () =>
            {
                _soundService.PlayDoubleChirp();
                _settingsService.CurrentSettings.FriendshipXP += 25;
                _settingsService.SaveSettings(_settingsService.CurrentSettings);
                _statsService.LogTreat();

                var eatMsg = _settingsService.CurrentSettings.Language switch
                {
                    "Bengali" => $"আহ্! দারুণ শিকার! মচমচে ও সুস্বাদু {selectedPrey.nameBn}! 😋❤️",
                    "English" => $"Delicious! Gobbled up the {selectedPrey.nameEn}! 😋❤️",
                    _ => $"ইয়াম্মি! সুস্বাদু {selectedPrey.nameBn} সাবাড় করলাম! 😋❤️"
                };
                SpeechBubble.ShowMessage(eatMsg, "😋", "Yummy!", 4);
            });
    }

    #endregion

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

        var speakTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(8) };
        speakTimer.Tick += (s, e) =>
        {
            speakTimer.Stop();
            if (Pet.CurrentState == PetState.Speaking) Pet.SetState(PetState.Idle);
        };
        speakTimer.Start();
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

    private void Pet_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Middle)
        {
            ToggleSound();
            e.Handled = true;
        }
    }

    private void Pet_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        string lang = _settingsService.CurrentSettings.Language;
        bool isSleeping = (Pet.CurrentState == PetState.Sleeping || Pet.CurrentState == PetState.NightSleeping);
        MenuSleepWake.Header = isSleeping
            ? LocalizationService.Get("SleepWake_Wake", lang)
            : LocalizationService.Get("SleepWake_Sleep", lang);

        MenuPomodoro.Header = _pomodoroService.CurrentPhase == PomodoroPhase.Stopped
            ? "🍅 পোমোডোরো শুরু করুন (Start Focus)"
            : "🍅 পোমোডোরো বন্ধ করুন (Stop Focus)";

        MenuPerchMode.Header = _settingsService.CurrentSettings.PerchMode == "ActiveWindow"
            ? "🖥️ ডেস্কটপে ফিরে এসো (Perch on Desktop)"
            : LocalizationService.Get("PerchActiveWindow", lang);

        UpdateSoundMenuItems(_soundService.Enabled);
        UpdateBirdcageMenuItem(_settingsService.CurrentSettings.BirdcageMode);

        if (MenuAutoStartup != null)
        {
            MenuAutoStartup.IsChecked = AutoStartupService.IsAutoStartupEnabled();
        }

        PetContextMenu.IsOpen = true;
    }

    private void OnDragFinished()
    {
        var workArea = GetCurrentWorkArea();
        var s = _settingsService.CurrentSettings;

        // Custom Placement: clear perched window tracking so user's manual placement is locked
        _perchedWindowHwnd = IntPtr.Zero;

        // Clamp safely anywhere on visible monitor
        Left = Math.Clamp(Left, workArea.Left, Math.Max(workArea.Left, workArea.Right - Width));
        Top = Math.Clamp(Top, workArea.Top, Math.Max(workArea.Top, workArea.Bottom - Height));

        s.LastPosX = Left;
        s.LastPosY = Top;
        _settingsService.SaveSettings(s);

        _lastInteractionTime = DateTime.Now;
        _lastAutonomousActionTime = DateTime.Now.AddSeconds(60);

        Pet.SetState(PetState.Idle);
        _soundService.PlayTrill();
        Pet.AnimateHearts();

        if (s.BirdcageMode)
        {
            SpeechBubble.ShowMessage(LocalizationService.Get("Birdcage_PlacedBubble", s.Language), "🏰", "Placed", 4);
        }
        else
        {
            SpeechBubble.ShowMessage(
                s.Language == "English"
                    ? "You placed me here! Happily perched in this spot 🦜💖"
                    : "আমাকে এখানে বসিয়ে দিয়েছো! এখানেই সুন্দর করে বসে থাকছি 🦜💖",
                "📌", "Placed", 4);
        }
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
        OpenSettingsWindow();
    }

    #endregion

    #region State Controls

    public void WakeUp()
    {
        Pet.ClearTransientEffects();
        Pet.SetState(PetState.Idle);
        _trayIconService.UpdateSleepState(false);
        _soundService.PlayDoubleChirp();

        // Natural avian waking reaction
        int wakeReaction = _random.Next(3);
        if (wakeReaction == 0)
        {
            Pet.Yawn();
        }
        else if (wakeReaction == 1)
        {
            Pet.FluffFeathers();
        }
        else
        {
            Pet.StretchWing();
        }

        var wakeQuote = Dialogues.GetWakeUpDialogue(_settingsService.CurrentSettings.Language);
        SpeechBubble.ShowMessage(wakeQuote, "☀️", "Parot", 5);
        _lastInteractionTime = DateTime.Now;
    }

    public void PutToSleep()
    {
        // Cancel active flight or walking so bird doesn't sleep mid-air or mid-stride
        if (_flightMovementService.IsFlying)
        {
            _flightMovementService.CancelFlight();
            Pet.SetBankAngle(0);
        }
        _isWalkingAlongTitlebar = false;

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
        TriggerAutonomousHunt("Chili");
    }

    public void FeedSunflowerSeeds()
    {
        WakeUp();
        TriggerAutonomousHunt("Seed");
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
        var workArea = GetCurrentWorkArea();
        double minX = workArea.Left + 20;
        double maxX = workArea.Right - Width - 20;

        if (_perchedWindowHwnd != IntPtr.Zero && WindowPerchService.TryGetWindowRect(_perchedWindowHwnd, out var curRect))
        {
            minX = curRect.Left + 20;
            maxX = curRect.Right - Width - 20;
        }

        if (maxX <= minX) return;

        bool hopLeft = _random.Next(2) == 0;
        double hopDistance = _random.Next(35, 75);

        if (hopLeft && (Left - hopDistance) < minX) hopLeft = false;
        else if (!hopLeft && (Left + hopDistance) > maxX) hopLeft = true;

        double targetLeft = Math.Clamp(hopLeft ? Left - hopDistance : Left + hopDistance, minX, maxX);
        double currentTop = Top;

        Pet.SetFacing(hopLeft);
        Pet.SetState(PetState.Hopping);

        var moveAnim = new DoubleAnimation
        {
            To = targetLeft,
            Duration = TimeSpan.FromMilliseconds(320),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };

        var jumpAnim = new DoubleAnimationUsingKeyFrames
        {
            Duration = TimeSpan.FromMilliseconds(320)
        };
        jumpAnim.KeyFrames.Add(new SplineDoubleKeyFrame(currentTop - 14, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(160)), new KeySpline(0.2, 0.8, 0.4, 1.0)));
        jumpAnim.KeyFrames.Add(new SplineDoubleKeyFrame(currentTop, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(320)), new KeySpline(0.6, 0.0, 0.9, 0.4)));

        moveAnim.Completed += (s, e) =>
        {
            double finalLeft = targetLeft;
            BeginAnimation(LeftProperty, null);
            BeginAnimation(TopProperty, null);
            Left = finalLeft;
            Top = currentTop;
            Pet.SetState(PetState.Idle);
            _soundService.PlayStep();
            _settingsService.CurrentSettings.LastPosX = Left;
            _settingsService.CurrentSettings.LastPosY = Top;
        };

        BeginAnimation(LeftProperty, moveAnim);
        BeginAnimation(TopProperty, jumpAnim);
        _soundService.PlayChirp();
    }

    private void PerformWingStretch()
    {
        Pet.SetState(PetState.FlappingWings);
        _soundService.PlayTrill();

        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1100) };
        timer.Tick += (s, e) =>
        {
            timer.Stop();
            if (Pet.CurrentState == PetState.FlappingWings)
            {
                Pet.SetState(PetState.Idle);
            }
        };
        timer.Start();
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

        var remTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(6) };
        remTimer.Tick += (s, e) =>
        {
            remTimer.Stop();
            if (Pet.CurrentState == PetState.Speaking) Pet.SetState(PetState.Idle);
        };
        remTimer.Start();
    }

    private void OnReminderActionTaken(ReminderType type, bool isDone)
    {
        if (isDone)
        {
            _reminderService.ResetTimer(type);
            Pet.SetState(PetState.Happy);
            Pet.AnimateHearts();
            _soundService.PlayTrill();

            var happyTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            happyTimer.Tick += (s, e) =>
            {
                happyTimer.Stop();
                if (Pet.CurrentState == PetState.Happy) Pet.SetState(PetState.Idle);
            };
            happyTimer.Start();

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
        _osAwarenessService.Stop();
        _weatherService.Dispose();
        _devWatcherService.Dispose();
        _typingDetectorService.Dispose();
        _micMimicService.Dispose();
        _audioDetectorService.Dispose();
        _voiceMimicService.Dispose();
        _trayIconService.Dispose();

        try { _bouncyBallWindow?.Close(); } catch { }
        try { _companionWindow?.CloseCompanion(); } catch { }
        try { _activeTreatWindow?.Catch(); } catch { }
        try { _laserWindow?.Close(); } catch { }
        try { _mirrorWindow?.Close(); } catch { }

        Application.Current.Shutdown();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        _osAwarenessService.Stop();
        _weatherService.Dispose();
        _devWatcherService.Dispose();
        _typingDetectorService.Dispose();
        _micMimicService.Dispose();
        _audioDetectorService.Dispose();
        _voiceMimicService.Dispose();
        _trayIconService.Dispose();

        try { _bouncyBallWindow?.Close(); } catch { }
        try { _companionWindow?.CloseCompanion(); } catch { }
        try { _activeTreatWindow?.Catch(); } catch { }
        try { _laserWindow?.Close(); } catch { }
        try { _mirrorWindow?.Close(); } catch { }
    }

    #endregion

    #region Interactive Mini-Games Handlers

    public void ThrowTreat(string emoji = "🌶️")
    {
        WakeUp();
        if (_activeTreatWindow != null)
        {
            try { _activeTreatWindow.Catch(); } catch { }
            _activeTreatWindow = null;
        }

        var workArea = GetCurrentWorkArea();
        _activeTreatWindow = new TreatItemWindow(emoji);

        var mouse = WindowInteropHelperNative.GetCurrentMousePosition();
        Point startPos = new Point(
            Math.Clamp(mouse.X, workArea.Left + 150, workArea.Right - 150),
            Math.Clamp(mouse.Y - 100, workArea.Top + 80, workArea.Bottom - 300)
        );

        double throwVx = (startPos.X < Left) ? 140 : -140;
        double throwVy = -320;
        _activeTreatWindow.Throw(startPos, new Vector(throwVx, throwVy));

        SpeechBubble.ShowMessage($"খাবার ছুড়ে দেওয়া হয়েছে! ধরছি! {emoji}", "🎯", "Catch!", 2);

        // Fly to intercept treat
        StartFlightTo(new Point(startPos.X - (Width / 2), startPos.Y + 20));

        _activeTreatWindow.Landed += (landPos) =>
        {
            if (_activeTreatWindow != null && !_activeTreatWindow.IsCaught)
            {
                _activeTreatWindow.Catch();
                _activeTreatWindow = null;
                Pet.PeckAndEat(emoji, () => _soundService.PlayMunch(), () =>
                {
                    _soundService.PlayTrill();
                    Pet.AnimateHearts();
                    _settingsService.CurrentSettings.FriendshipXP += 25;
                    _settingsService.CurrentSettings.HungerLevel = 100;
                    _settingsService.SaveSettings(_settingsService.CurrentSettings);
                    SpeechBubble.ShowMessage($"মাটিতে পড়ে যাওয়ার আগেই খাবার ধরে খেয়ে ফেললাম! 😋🎉 (+25 XP)", "🏆", "Catch!", 4);
                });
            }
        };
    }

    private void TogglePerchOnCursor()
    {
        _isPerchedOnCursor = !_isPerchedOnCursor;
        _settingsService.CurrentSettings.PerchOnCursorActive = _isPerchedOnCursor;
        _settingsService.SaveSettings(_settingsService.CurrentSettings);

        if (MenuPerchCursor != null)
        {
            MenuPerchCursor.Header = _isPerchedOnCursor ? "🎯 কার্সার থেকে নামো" : "🎯 কার্সারের পিঠে চড়ো (Perch on Cursor)";
        }

        if (_isPerchedOnCursor)
        {
            WakeUp();
            SpeechBubble.ShowMessage("আমি তোমার মাউস কার্সারের পিঠে চড়ে বসেছি! 🎯", "🎯", "Cursor Perch", 4);
        }
        else
        {
            var workArea = GetCurrentWorkArea();
            StartFlightTo(new Point(Left, workArea.Bottom - Height + 6));
            SpeechBubble.ShowMessage("কার্সার থেকে নেমে ডেস্কটপে বসেছি। 🖥️", "🖥️", "Desktop", 3);
        }
    }

    private void ToggleBallPushGame()
    {
        if (_bouncyBallWindow == null || !_bouncyBallWindow.IsLoaded)
        {
            _bouncyBallWindow = new BouncyBallWindow();
            _bouncyBallWindow.Closed += (s, e) => { _bouncyBallWindow = null; };
            _bouncyBallWindow.UserKicked += (pos) =>
            {
                if (Pet.CurrentState == PetState.Idle || Pet.CurrentState == PetState.Perching)
                {
                    Pet.SetFacing(pos.X < Left);
                    Pet.BobHead();
                    _soundService.PlayChirp();
                }
            };
            _bouncyBallWindow.Show();
            SpeechBubble.ShowMessage("বল এসে গেছে! মাউস দিয়ে ধাক্কা দাও, আমি ঠেলে দেব! ⚽", "⚽", "Ball Game", 5);
        }
        else
        {
            _bouncyBallWindow.Close();
            _bouncyBallWindow = null;
            SpeechBubble.ShowMessage("বল খেলা শেষ! ⚽", "👍", "Ball Game", 3);
        }
    }

    #endregion

    #region Species, Weather, Voice & Companion Features

    public void SwitchSkin(string skin)
    {
        var s = _settingsService.CurrentSettings;
        s.Skin = skin;
        _settingsService.SaveSettings(s);
        Pet.ApplySkin(skin);

        string name = skin switch
        {
            "GreenParakeet" => "সবুজ দেশি টিয়া (Rose-ringed Parakeet)",
            "BlueMacaw" => "নীল-হলুদ ম্যাকাও (Blue & Gold Macaw)",
            "Cockatiel" => "হলুদ ককাটেল (Cockatiel)",
            "Cockatoo" => "সাদা কাকাতুয়া (White Cockatoo)",
            _ => "লাল স্কারলেট ম্যাকাও (Scarlet Macaw)"
        };
        _soundService.PlayDoubleChirp();
        Pet.AnimateHearts();
        SpeechBubble.ShowMessage($"নতুন প্রজাতি নির্বাচন: {name} 🦜✨", "🎨", "Species", 4);
    }

    public void ToggleCompanion()
    {
        var s = _settingsService.CurrentSettings;
        s.DualBirdMode = !s.DualBirdMode;
        _settingsService.SaveSettings(s);

        if (s.DualBirdMode)
        {
            if (_companionWindow == null || !_companionWindow.IsLoaded)
            {
                _companionWindow = new CompanionWindow(s.CompanionSkin);
                _companionWindow.Closed += (s, e) => { _companionWindow = null; };
                _companionWindow.Show();
                _soundService.PlayTrill();
                SpeechBubble.ShowMessage("দ্বিতীয় সঙ্গী পাখি চলে এসেছে! কিচিরমিচির! 🦜🦜💕", "🦜", "Bird Buddy", 5);
            }
        }
        else
        {
            _companionWindow?.CloseCompanion();
            _companionWindow = null;
            SpeechBubble.ShowMessage("সঙ্গী পাখি এখন বিদায় নিয়েছে। 🦜👋", "👋", "Bird Buddy", 3);
        }
    }

    private void OnWeatherUpdated(WeatherInfo info)
    {
        Dispatcher.Invoke(() =>
        {
            var s = _settingsService.CurrentSettings;
            if (s.WeatherSyncEnabled && s.Accessory != "None")
            {
                s.Accessory = info.RecommendedAccessory;
                Pet.SetAccessory(info.RecommendedAccessory);
                _settingsService.SaveSettings(s);
            }

            if (!string.IsNullOrWhiteSpace(info.Dialogue))
            {
                SpeechBubble.ShowMessage(info.Dialogue, "🌦️", "Live Weather", 6);
            }
        });
    }

    private void OnBuildSucceeded(string summary)
    {
        Dispatcher.Invoke(() =>
        {
            WakeUp();
            Pet.SetState(PetState.Dancing);
            _soundService.PlayHappy();
            Pet.AnimateHearts();
            _settingsService.CurrentSettings.FriendshipXP += 15;
            _settingsService.SaveSettings(_settingsService.CurrentSettings);
            SpeechBubble.ShowMessage("🎉 বিল্ড সফল! দারুণ কোড লিখেছো! / Build Succeeded! Amazing job! 🚀 (+15 XP)", "✨", "Dev Companion", 6);

            var buildDanceTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3.5) };
            buildDanceTimer.Tick += (s, e) =>
            {
                buildDanceTimer.Stop();
                if (Pet.CurrentState == PetState.Dancing) Pet.SetState(PetState.Idle);
            };
            buildDanceTimer.Start();
        });
    }

    private void OnBuildFailed(string error)
    {
        Dispatcher.Invoke(() =>
        {
            WakeUp();
            Pet.CuriousLookAround();
            _soundService.PlayCurious();
            SpeechBubble.ShowMessage("🤦‍♂️ আহারে, বিল্ড এরর! মন খারাপ কোরো না, চলো ডিবাগ করে ফেলি! / Build failed, you got this!", "💻", "Dev Companion", 6);
        });
    }

    private void OnFastTypingDetected(int count)
    {
        if (!_settingsService.CurrentSettings.TypingSyncBobbing) return;
        if (Pet.CurrentState == PetState.Sleeping || Pet.CurrentState == PetState.NightSleeping || Pet.CurrentState == PetState.Dragged) return;

        Dispatcher.Invoke(() =>
        {
            Pet.BobHead();
            if (_random.Next(100) < 15 && _settingsService.CurrentSettings.IdleChirping)
            {
                _soundService.PlayChirp();
            }
        });
    }

    private void OnVoiceCommandRecognized(string cmd)
    {
        Dispatcher.Invoke(() =>
        {
            switch (cmd)
            {
                case "fly" or "take off" or "flying":
                    TriggerAutonomousFlight();
                    SpeechBubble.ShowMessage("উড়ে চললাম আকাশে! 🚀", "🚀", "Voice Command", 3);
                    break;
                case "sleep" or "go to sleep" or "good night":
                    PutToSleep();
                    break;
                case "wake up" or "wake":
                    WakeUp();
                    break;
                case "eat" or "food" or "hungry" or "feed":
                    TriggerAutonomousHunt();
                    break;
                case "dance" or "party":
                    Pet.SetState(PetState.Dancing);
                    Pet.AnimateMusicNote();
                    _soundService.PlayTrill();
                    SpeechBubble.ShowMessage("নাচো সবাই! ডানা মেলে নাচছি! 💃🎶", "🎵", "Voice Command", 4);
                    var voiceDanceTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3.5) };
                    voiceDanceTimer.Tick += (s, e) =>
                    {
                        voiceDanceTimer.Stop();
                        if (Pet.CurrentState == PetState.Dancing) Pet.SetState(PetState.Idle);
                    };
                    voiceDanceTimer.Start();
                    break;
                case "come here" or "perch":
                    var cur = WindowInteropHelperNative.GetCurrentMousePosition();
                    StartFlightTo(new Point(cur.X - Width / 2, cur.Y - Height + 10));
                    SpeechBubble.ShowMessage("তোমার কার্সারের কাছে চলে এসেছি! 🎯", "🎯", "Voice Command", 3);
                    break;
                case "cage" or "birdcage" or "lock":
                    ToggleBirdcage();
                    break;
            }
        });
    }

    #endregion

    #region Context Menu Handlers

    private void MenuPomodoro_Click(object sender, RoutedEventArgs e) => TogglePomodoro();
    private void MenuAskParot_Click(object sender, RoutedEventArgs e) => SpeechBubble.PromptAskParot();
    private void MenuDailyReport_Click(object sender, RoutedEventArgs e) => OpenDailyReportWindow();
    private void MenuFlyAround_Click(object sender, RoutedEventArgs e) => TriggerAutonomousFlight();
    private void MenuFollowCursor_Click(object sender, RoutedEventArgs e)
    {
        var s = _settingsService.CurrentSettings;
        s.FollowCursor = !s.FollowCursor;
        _settingsService.SaveSettings(s);
        MenuFollowCursor.Header = s.FollowCursor ? "🎯 কার্সার অনুসরণ বন্ধ করো" : "🎯 কার্সার অনুসরণ করো (Follow Cursor)";
        SpeechBubble.ShowMessage(s.FollowCursor ? "আমি এখন তোমার কার্সারের পিছু নেব! 🎯" : "কার্সার অনুসরণ বন্ধ করা হয়েছে।", "🦜", "Follow Cursor", 4);
    }
    private void MenuPerchMode_Click(object sender, RoutedEventArgs e) => TogglePerchMode();
    private void MenuPlayTag_Click(object sender, RoutedEventArgs e) => StartPlayTag();
    private void MenuSquawk_Click(object sender, RoutedEventArgs e) => OnPetDoubleClicked();

    // Mini-Games
    private void MenuThrowChili_Click(object sender, RoutedEventArgs e) => ThrowTreat("🌶️");
    private void MenuThrowPeanut_Click(object sender, RoutedEventArgs e) => ThrowTreat("🥜");
    private void MenuThrowSeed_Click(object sender, RoutedEventArgs e) => ThrowTreat("🌻");
    private void MenuPerchCursor_Click(object sender, RoutedEventArgs e) => TogglePerchOnCursor();
    private void MenuBallPush_Click(object sender, RoutedEventArgs e) => ToggleBallPushGame();

    // Species
    private void MenuSkinScarletMacaw_Click(object sender, RoutedEventArgs e) => SwitchSkin("ScarletMacaw");
    private void MenuSkinGreenParakeet_Click(object sender, RoutedEventArgs e) => SwitchSkin("GreenParakeet");
    private void MenuSkinBlueMacaw_Click(object sender, RoutedEventArgs e) => SwitchSkin("BlueMacaw");
    private void MenuSkinCockatiel_Click(object sender, RoutedEventArgs e) => SwitchSkin("Cockatiel");
    private void MenuSkinCockatoo_Click(object sender, RoutedEventArgs e) => SwitchSkin("Cockatoo");

    // Accessories
    private void MenuAccessorySunglasses_Click(object sender, RoutedEventArgs e) => SetAccessory("Sunglasses");
    private void MenuAccessoryGlasses_Click(object sender, RoutedEventArgs e) => SetAccessory("Glasses");
    private void MenuAccessoryScarf_Click(object sender, RoutedEventArgs e) => SetAccessory("Scarf");
    private void MenuAccessoryTopHat_Click(object sender, RoutedEventArgs e) => SetAccessory("TopHat");
    private void MenuAccessoryPartyHat_Click(object sender, RoutedEventArgs e) => SetAccessory("PartyHat");
    private void MenuAccessoryUmbrella_Click(object sender, RoutedEventArgs e) => SetAccessory("Umbrella");
    private void MenuAccessoryNone_Click(object sender, RoutedEventArgs e) => SetAccessory("None");

    public void SetAccessory(string acc)
    {
        var s = _settingsService.CurrentSettings;
        s.Accessory = acc;
        _settingsService.SaveSettings(s);
        Pet.SetAccessory(acc);
        SpeechBubble.ShowMessage(acc == "None" ? "সব সাজসজ্জা খুলে ফেলা হয়েছে!" : $"নতুন সাজসজ্জা পরা হয়েছে: {acc} ✨", "✨", "Accessory", 3);
    }

    // Voice & Mimicry
    private async void MenuVoiceMimic_Click(object sender, RoutedEventArgs e) => await _micMimicService.RecordAndEchoParrotVoiceAsync(3);
    private void MenuVoiceCommands_Click(object sender, RoutedEventArgs e)
    {
        var s = _settingsService.CurrentSettings;
        s.VoiceCommandsEnabled = !s.VoiceCommandsEnabled;
        _settingsService.SaveSettings(s);
        MenuVoiceCommands.Header = s.VoiceCommandsEnabled ? "🗣️ ভয়েস কমান্ড (Voice Commands: Active)" : "🗣️ ভয়েস কমান্ড বন্ধ (Voice Commands: Inactive)";
        SpeechBubble.ShowMessage(s.VoiceCommandsEnabled ? "ভয়েস কমান্ড চালু আছে! 'fly', 'sleep', 'eat', 'dance' বলুন!" : "ভয়েস কমান্ড বন্ধ করা হয়েছে।", "🗣️", "Voice Command", 4);
    }

    // Weather & Companion
    private async void MenuWeather_Click(object sender, RoutedEventArgs e)
    {
        SpeechBubble.ShowMessage("লাইভ আবহাওয়া পরীক্ষা করছি... 🌦️", "🌦️", "Weather", 3);
        await _weatherService.CheckWeatherAsync();
    }
    private void MenuCompanion_Click(object sender, RoutedEventArgs e) => ToggleCompanion();

    // Feeding & Hunting
    private void MenuHuntPrey_Click(object sender, RoutedEventArgs e) => TriggerAutonomousHunt("Ladybug");
    private void MenuHuntGrasshopper_Click(object sender, RoutedEventArgs e) => TriggerAutonomousHunt("Grasshopper");
    private void MenuFeedChili_Click(object sender, RoutedEventArgs e) => FeedChiliTreat();
    private void MenuFeedSeeds_Click(object sender, RoutedEventArgs e) => FeedSunflowerSeeds();

    // System & Health
    private void MenuSleepWake_Click(object sender, RoutedEventArgs e) => ToggleSleepWake();
    private void MenuEyeBreak_Click(object sender, RoutedEventArgs e) => _reminderService.TriggerBreakNow();
    private void MenuHydrate_Click(object sender, RoutedEventArgs e) => _reminderService.TriggerHydrationNow();
    private void MenuGhostMode_Click(object sender, RoutedEventArgs e) => ToggleGhostMode();
    private void MenuSettings_Click(object sender, RoutedEventArgs e) => OpenSettingsWindow();
    private void MenuExit_Click(object sender, RoutedEventArgs e) => ExitApplication();

    // Sound Toggle System
    public void ToggleSound()
    {
        var s = _settingsService.CurrentSettings;
        s.SoundEnabled = !s.SoundEnabled;
        SoundService.MasterMute = !s.SoundEnabled;
        _soundService.Enabled = s.SoundEnabled;
        _voiceMimicService.MasterSoundEnabled = s.SoundEnabled;
        _micMimicService.SoundEnabled = s.SoundEnabled;

        if (!s.SoundEnabled)
        {
            _soundService.StopAll();
            _voiceMimicService.Stop();
            _micMimicService.StopPlayback();
        }

        _settingsService.SaveSettings(s);

        _trayIconService.UpdateSoundState(s.SoundEnabled);
        UpdateSoundMenuItems(s.SoundEnabled);

        if (s.SoundEnabled)
        {
            _soundService.PlayDoubleChirp();
            SpeechBubble.ShowMessage(LocalizationService.Get("SoundOnBubble", s.Language), "🔊", "Sound", 3);
        }
        else
        {
            SpeechBubble.ShowMessage(LocalizationService.Get("SoundOffBubble", s.Language), "🔇", "Muted", 3);
        }
    }

    private void UpdateSoundMenuItems(bool isEnabled)
    {
        string lang = _settingsService.CurrentSettings.Language;
        if (MenuSoundToggle != null)
        {
            MenuSoundToggle.Header = isEnabled
                ? LocalizationService.Get("SoundOn", lang)
                : LocalizationService.Get("SoundOff", lang);
        }

        if (MenuSoundQuickToggle != null)
        {
            MenuSoundQuickToggle.Header = isEnabled
                ? "🔊 সাউন্ড বন্ধ করো (Mute Sound)"
                : "🔇 সাউন্ড চালু করো (Unmute Sound)";
        }
    }

    private void MenuSoundToggle_Click(object sender, RoutedEventArgs e) => ToggleSound();

    // Auto-Startup & Multi-Language Handlers
    private void MenuAutoStartup_Click(object sender, RoutedEventArgs e)
    {
        var s = _settingsService.CurrentSettings;
        s.StartWithWindows = MenuAutoStartup.IsChecked;
        AutoStartupService.SetAutoStartup(s.StartWithWindows);
        _settingsService.SaveSettings(s);

        string msg = s.StartWithWindows
            ? LocalizationService.Get("AutoStartupOnBubble", s.Language)
            : LocalizationService.Get("AutoStartupOffBubble", s.Language);

        SpeechBubble.ShowMessage(msg, "🚀", "Startup", 4);
    }

    private void MenuLangBengali_Click(object sender, RoutedEventArgs e) => SetLanguage("Bengali");
    private void MenuLangEnglish_Click(object sender, RoutedEventArgs e) => SetLanguage("English");
    private void MenuLangHindi_Click(object sender, RoutedEventArgs e) => SetLanguage("Hindi");
    private void MenuLangSpanish_Click(object sender, RoutedEventArgs e) => SetLanguage("Spanish");
    private void MenuLangBilingual_Click(object sender, RoutedEventArgs e) => SetLanguage("Bilingual");

    public void SetLanguage(string lang)
    {
        var s = _settingsService.CurrentSettings;
        s.Language = LocalizationService.NormalizeLanguage(lang);
        _settingsService.SaveSettings(s);
        ApplyLanguage(s.Language);

        string msg = LocalizationService.Get("LanguageChangedBubble", s.Language);
        SpeechBubble.ShowMessage(msg, "🌐", "Language", 4);
    }

    public void ApplyLanguage(string? language = null)
    {
        string lang = LocalizationService.NormalizeLanguage(language ?? _settingsService.CurrentSettings.Language);

        if (MenuLanguage != null) MenuLanguage.Header = LocalizationService.Get("LanguageMenu", lang);
        if (MenuAutoStartup != null)
        {
            MenuAutoStartup.Header = LocalizationService.Get("AutoStartup", lang);
            MenuAutoStartup.IsChecked = AutoStartupService.IsAutoStartupEnabled();
        }

        if (MenuLangBengali != null) MenuLangBengali.IsChecked = (lang == "Bengali");
        if (MenuLangEnglish != null) MenuLangEnglish.IsChecked = (lang == "English");
        if (MenuLangHindi != null) MenuLangHindi.IsChecked = (lang == "Hindi");
        if (MenuLangSpanish != null) MenuLangSpanish.IsChecked = (lang == "Spanish");
        if (MenuLangBilingual != null) MenuLangBilingual.IsChecked = (lang == "Bilingual");

        if (MenuMiniGames != null) MenuMiniGames.Header = LocalizationService.Get("MiniGamesMenu", lang);
        if (MenuAvianBehaviors != null) MenuAvianBehaviors.Header = LocalizationService.Get("AvianBehaviorsMenu", lang);
        if (MenuBirdSpecies != null) MenuBirdSpecies.Header = LocalizationService.Get("BirdSpeciesMenu", lang);
        if (MenuAccessories != null) MenuAccessories.Header = LocalizationService.Get("AccessoriesMenu", lang);
        if (MenuVoiceAudio != null) MenuVoiceAudio.Header = LocalizationService.Get("AudioMenu", lang);
        if (MenuHungerFeeding != null) MenuHungerFeeding.Header = LocalizationService.Get("FeedingMenu", lang);
        if (MenuFlyAround != null) MenuFlyAround.Header = LocalizationService.Get("TakeFlight", lang);
        if (MenuFollowCursor != null) MenuFollowCursor.Header = LocalizationService.Get("FollowCursor", lang);
        if (MenuPerchMode != null) MenuPerchMode.Header = LocalizationService.Get("PerchActiveWindow", lang);
        if (MenuSettings != null) MenuSettings.Header = LocalizationService.Get("SettingsMenu", lang);
        if (MenuExit != null) MenuExit.Header = LocalizationService.Get("ExitMenu", lang);

        UpdateSoundMenuItems(_settingsService.CurrentSettings.SoundEnabled);
        UpdateBirdcageMenuItem(_settingsService.CurrentSettings.BirdcageMode);
    }

    // Dynamic Features: Window Walking, Edge Peeking & Avian Behaviors
    public void WalkAlongTitlebar()
    {
        if (_perchedWindowHwnd == IntPtr.Zero || !WindowPerchService.TryGetWindowRect(_perchedWindowHwnd, out var rect))
        {
            WalkAlongFloor();
            return;
        }

        _isWalkingAlongTitlebar = true;
        Pet.SetState(PetState.Walking);
        Pet.SetFacing(_walkDirection < 0);

        int stepsRemaining = _random.Next(5, 10);
        int stepCount = 0;
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(180) };
        timer.Tick += (s, e) =>
        {
            if (_perchedWindowHwnd == IntPtr.Zero || !WindowPerchService.TryGetWindowRect(_perchedWindowHwnd, out var curRect))
            {
                timer.Stop();
                _isWalkingAlongTitlebar = false;
                Pet.SetState(PetState.Idle);
                return;
            }

            double stepSize = 14 * _walkDirection;
            double newLeft = Left + stepSize;

            bool nearLeftEdge = newLeft <= curRect.Left + 25;
            bool nearRightEdge = newLeft >= curRect.Right - Width - 25;

            if (nearLeftEdge || nearRightEdge)
            {
                timer.Stop();
                _isWalkingAlongTitlebar = false;

                // Edge Peeking animation!
                Pet.SetFacing(nearLeftEdge);
                Pet.SetState(PetState.PeekingEdge);
                _soundService.PlayCurious();
                SpeechBubble.ShowMessage("উইন্ডোর কিনারায় উঁকি মারছি... নিচে কত গভীর! 🪟👀", "👀", "Edge Peeking", 4);

                _walkDirection = -_walkDirection;

                var peekTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.4) };
                peekTimer.Tick += (ps, pe) =>
                {
                    peekTimer.Stop();
                    if (Pet.CurrentState == PetState.PeekingEdge)
                    {
                        Pet.SetState(PetState.Idle);
                    }
                };
                peekTimer.Start();
            }
            else
            {
                Left = newLeft;
                stepCount++;
                _soundService.PlayStep(stepCount % 2 == 0);
                stepsRemaining--;
                if (stepsRemaining <= 0)
                {
                    timer.Stop();
                    _isWalkingAlongTitlebar = false;
                    Pet.SetState(PetState.Idle);
                }
            }
        };
        timer.Start();
    }

    public void WalkAlongFloor()
    {
        var workArea = GetCurrentWorkArea();
        _isWalkingAlongTitlebar = true;
        Pet.SetState(PetState.Walking);
        Pet.SetFacing(_walkDirection < 0);

        int stepsRemaining = _random.Next(6, 12);
        int stepCount = 0;
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(180) };
        timer.Tick += (s, e) =>
        {
            double stepSize = 14 * _walkDirection;
            double newLeft = Left + stepSize;

            bool nearLeftEdge = newLeft <= workArea.Left + 25;
            bool nearRightEdge = newLeft >= workArea.Right - Width - 25;

            if (nearLeftEdge || nearRightEdge)
            {
                timer.Stop();
                _isWalkingAlongTitlebar = false;
                _walkDirection = -_walkDirection;
                Pet.SetFacing(_walkDirection < 0);
                Pet.SetState(PetState.PeekingEdge);
                _soundService.PlayCurious();
                SpeechBubble.ShowMessage("স্ক্রিনের কিনারায় এসে দাঁড়িয়েছি! 🖥️👀", "👀", "Desktop Edge", 3);
                var edgeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.8) };
                edgeTimer.Tick += (es, ee) =>
                {
                    edgeTimer.Stop();
                    if (Pet.CurrentState == PetState.PeekingEdge)
                    {
                        Pet.SetState(PetState.Idle);
                    }
                };
                edgeTimer.Start();
            }
            else
            {
                Left = newLeft;
                stepCount++;
                _soundService.PlayStep(stepCount % 2 == 0);
                stepsRemaining--;
                if (stepsRemaining <= 0)
                {
                    timer.Stop();
                    _isWalkingAlongTitlebar = false;
                    Pet.SetState(PetState.Idle);
                }
            }
        };
        timer.Start();
    }

    public void TriggerRandomAvianBehavior()
    {
        int choice = _random.Next(11);
        switch (choice)
        {
            case 0:
                Pet.PreenFeathers();
                SpeechBubble.ShowMessage("পালকগুলো সুন্দর করে ঠোঁট দিয়ে গুছিয়ে নিচ্ছি! ✨🪶", "✨", "Preening", 3);
                _soundService.PlayPreen();
                break;
            case 1:
                Pet.StretchWing();
                SpeechBubble.ShowMessage("এক পা ও ডানা মেলে বার্ড-যোগা স্ট্রেচ করছি! 🧘🪶", "🧘", "Yoga Stretch", 3);
                _soundService.PlayChirp();
                break;
            case 2:
                Pet.FluffFeathers();
                SpeechBubble.ShowMessage("গা ঝাড়া দিয়ে পালক ফুলিয়ে নিলাম! 💨🪶", "💨", "Fluff & Shake", 3);
                _soundService.PlayDoubleChirp();
                break;
            case 3:
                Pet.Yawn();
                SpeechBubble.ShowMessage("মিষ্টি একটা হাই তুললাম! 🥱", "🥱", "Yawn", 3);
                break;
            case 4:
                Pet.CuriousLookAround();
                SpeechBubble.ShowMessage("চারপাশে সজাগ চোখে মাথা ঘুরিয়ে দেখছি! 👀🔍", "👀", "Observing", 3);
                _soundService.PlayCurious();
                break;
            case 5:
                Pet.FlapWingsOnPerch();
                SpeechBubble.ShowMessage("ডালে বসে ডানা মেলে ঝাপটানোর ব্যায়াম করছি! 🪶💨", "🪶", "Perch Flap", 3);
                _soundService.PlayWingFlap();
                break;
            case 6:
                Pet.BeakWipe();
                SpeechBubble.ShowMessage("ডালে ঠোঁট ঘষে পরিষ্কার করে ধারালো করছি! 🪶🧼", "🧼", "Beak Wipe", 3);
                _soundService.PlayRustle();
                break;
            case 7:
                Pet.HeadScratch();
                SpeechBubble.ShowMessage("পা দিয়ে মাথা চুলকে নিচ্ছি... আহ্ আরাম! 💆🪶", "💆", "Head Scratch", 3);
                _soundService.PlayRustle();
                break;
            case 8:
                Pet.TailWag();
                SpeechBubble.ShowMessage("লেজ নাড়িয়ে মনের খুশি প্রকাশ করছি! 💃✨", "✨", "Happy Tail", 3);
                _soundService.PlayChirp();
                break;
            case 9:
                Pet.SetState(PetState.Dancing);
                Pet.AnimateMusicNote();
                SpeechBubble.ShowMessage("মনটা দারুণ প্রফুল্ল! তালে তালে নাচছি! 🎶💃", "🎶", "Happy Dance", 3);
                _soundService.PlayHappy();
                var danceTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.8) };
                danceTimer.Tick += (s, e) =>
                {
                    danceTimer.Stop();
                    if (Pet.CurrentState == PetState.Dancing)
                    {
                        Pet.SetState(PetState.Idle);
                    }
                };
                danceTimer.Start();
                break;
            case 10:
                Pet.PeckAndEat("🌻",
                    () => _soundService.PlayMunch(),
                    () =>
                    {
                        _soundService.PlayDoubleChirp();
                        SpeechBubble.ShowMessage("পকেট থেকে একটা সূর্যমুখী বীজ বের করে খেয়ে নিলাম! 🌻😋", "🌻", "Snack", 3);
                    });
                break;
        }
    }

    public void PerformNextAutonomousAction()
    {
        if (!CanPerformAutonomousAction) return;

        var s = _settingsService.CurrentSettings;

        // In Birdcage Mode: stays safely in place, performs natural in-cage activities
        if (s.BirdcageMode)
        {
            int cageRoll = _random.Next(100);
            if (cageRoll < 40)
            {
                TriggerRandomAvianBehavior();
            }
            else if (cageRoll < 65)
            {
                Pet.PeckAndEat("🌻", () => _soundService.PlayMunch(), () =>
                {
                    _soundService.PlayDoubleChirp();
                    SpeechBubble.ShowMessage("খাঁচার পাত্র থেকে সুস্বাদু বীজ খেয়ে নিলাম! 🌻😋", "🏰", "Snack", 3);
                });
            }
            else if (cageRoll < 85)
            {
                TakeDaytimePowerNap(14);
            }
            else
            {
                Pet.SetState(PetState.Dancing);
                Pet.AnimateMusicNote();
                _soundService.PlayHappy();
                SpeechBubble.ShowMessage("খাঁচার দোলনায় বসে মিষ্টি গান গাইছি! 🎶✨", "🎶", "Singing", 3);
                var danceTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.8) };
                danceTimer.Tick += (s, e) =>
                {
                    danceTimer.Stop();
                    if (Pet.CurrentState == PetState.Dancing) Pet.SetState(PetState.Idle);
                };
                danceTimer.Start();
            }
            return;
        }

        // Environment-Aware Behavioral Adaptation (নিজের পরিবেশ অনুযায়ী আচরণ বদলানো)
        string activeWindow = WindowPerchService.GetActiveWindowTitle().ToLowerInvariant();
        int hour = DateTime.Now.Hour;

        // Environment Context 1: Active Code Editor or Terminal -> Coding Companion
        if (activeWindow.Contains("visual studio") || activeWindow.Contains("code") || activeWindow.Contains("terminal") || activeWindow.Contains("powershell") || activeWindow.Contains("git"))
        {
            if (_random.Next(100) < 30)
            {
                Pet.CuriousLookAround();
                _soundService.PlayCurious();
                string[] codingQuotes = [
                    "কোডিং চলছে পুরোদমে! মন দিয়ে কাজ করো! 💻✨",
                    "কোনো বাগ দেখলেই বলো, ঠোকর দিয়ে ঠিক করে দেব! 🐞🔍",
                    "চমৎকার প্রোগ্রাম লিখছ! আমি পাশে বসে দেখছি! ☕🪶"
                ];
                SpeechBubble.ShowMessage(codingQuotes[_random.Next(codingQuotes.Length)], "💻", "Code Companion", 4);
                return;
            }
        }
        // Environment Context 2: Music or Media Player -> Dance and sway
        else if (activeWindow.Contains("spotify") || activeWindow.Contains("music") || activeWindow.Contains("youtube") || activeWindow.Contains("vlc"))
        {
            if (_random.Next(100) < 35)
            {
                Pet.SetState(PetState.Dancing);
                Pet.AnimateMusicNote();
                _soundService.PlayHappy();
                SpeechBubble.ShowMessage("কী মিষ্টি গান! তালে তালে মাথা দোলাচ্ছি! 🎶🕺", "🎵", "Jamming", 3);
                var musicTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.8) };
                musicTimer.Tick += (s, e) =>
                {
                    musicTimer.Stop();
                    if (Pet.CurrentState == PetState.Dancing) Pet.SetState(PetState.Idle);
                };
                musicTimer.Start();
                return;
            }
        }
        // Environment Context 3: Web Browser -> Curious edge-peeking & observing
        else if (activeWindow.Contains("chrome") || activeWindow.Contains("edge") || activeWindow.Contains("firefox") || activeWindow.Contains("brave"))
        {
            if (_random.Next(100) < 25)
            {
                Pet.CuriousLookAround();
                _soundService.PlayCurious();
                SpeechBubble.ShowMessage("ইন্টারনেটে কী দারুণ জিনিস খুঁজছ! 🌐👀", "🌐", "Browsing", 3);
                return;
            }
        }

        // Time of Day Context: Early morning energy vs relaxing evening
        if (hour >= 6 && hour < 11 && _random.Next(100) < 22)
        {
            Pet.StretchWing();
            _soundService.PlayDoubleChirp();
            SpeechBubble.ShowMessage("সুপ্রভাত! আজকের দিনটা দারুণ শুরু হোক! ☀️🪶", "☀️", "Morning Cheer", 3);
            return;
        }

        var currentWorkArea = GetCurrentWorkArea();
        bool isOnFloorOrWindow = (_perchedWindowHwnd != IntPtr.Zero && WindowPerchService.TryGetWindowRect(_perchedWindowHwnd, out _))
                                 || (Top >= currentWorkArea.Bottom - Height - 50);

        if (!isOnFloorOrWindow && !s.BirdcageMode)
        {
            // Parot is floating in mid-air (custom user placement or perch closed)
            // Real birds do not walk or hop on thin air; they groom, stretch, look around, or fly down gracefully
            int midAirRoll = _random.Next(100);
            if (midAirRoll < 45 && s.AutonomousFlight)
            {
                TriggerAutonomousFlight();
            }
            else if (midAirRoll < 75)
            {
                TriggerRandomAvianBehavior();
            }
            else
            {
                Pet.CuriousLookAround();
                _soundService.PlayCurious();
            }
            return;
        }

        int roll = _random.Next(100);

        // 1. Walking with graceful footsteps along titlebar or taskbar floor (22%)
        if (roll < 22 && s.AllowWalking)
        {
            if (_perchedWindowHwnd != IntPtr.Zero && WindowPerchService.TryGetWindowRect(_perchedWindowHwnd, out _))
            {
                WalkAlongTitlebar();
            }
            else
            {
                WalkAlongFloor();
            }
            return;
        }

        // 2. Natural Hopping along perch/floor (18%)
        if (roll < 40 && s.AllowWalking)
        {
            PerformRandomHop();
            return;
        }

        // 3. Avian Grooming & Body Care (Preen, Stretch, Fluff, Beak wipe, Head scratch, Tail wag) (26%)
        if (roll < 66)
        {
            TriggerRandomAvianBehavior();
            return;
        }

        // 4. Head rotation & Curious Observing (10%)
        if (roll < 76)
        {
            Pet.CuriousLookAround();
            _soundService.PlayCurious();
            return;
        }

        // 5. Autonomous Flight between windows and taskbar (12%)
        if (roll < 88 && s.AutonomousFlight)
        {
            TriggerAutonomousFlight();
            return;
        }

        // 6. Autonomous Foraging / Hunting treats and seeds (8%)
        if (roll < 96)
        {
            TriggerAutonomousHunt();
            return;
        }

        // 7. Daytime Power Nap (4%)
        TakeDaytimePowerNap();
    }

    public void TakeDaytimePowerNap(int napSeconds = 16)
    {
        if (Pet.CurrentState == PetState.Sleeping || Pet.CurrentState == PetState.NightSleeping || Pet.CurrentState == PetState.PomodoroFocus) return;

        Pet.SetState(PetState.Sleeping);
        _soundService.PlaySnore();
        SpeechBubble.ShowMessage("একটু অলস লাগছে... ছোট একটা মিষ্টি পাওয়ার ন্যাপ নিচ্ছি! 🥱💤", "💤", "Power Nap", 4);

        var napTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(napSeconds) };
        napTimer.Tick += (s, e) =>
        {
            napTimer.Stop();
            if (Pet.CurrentState == PetState.Sleeping)
            {
                WakeUp();
                if (_random.Next(2) == 0)
                {
                    Pet.Yawn(() =>
                    {
                        SpeechBubble.ShowMessage("আহ্! দারুণ ঘুম হলো! এখন সম্পূর্ণ চাঙ্গা! ☀️✨", "☀️", "Refreshed", 3);
                    });
                }
                else
                {
                    Pet.FluffFeathers(() =>
                    {
                        SpeechBubble.ShowMessage("গা ঝাড়া দিয়ে উঠে পড়লাম! 💨☀️", "☀️", "Refreshed", 3);
                    });
                }
            }
        };
        napTimer.Start();
    }

    public void TriggerAutonomousMirrorTrick()
    {
        if (_mirrorWindow != null && _mirrorWindow.IsLoaded) return;
        ToggleDesktopMirror();
        var mirrorCloseTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(4.5) };
        mirrorCloseTimer.Tick += (s, e) =>
        {
            mirrorCloseTimer.Stop();
            if (_mirrorWindow != null && _mirrorWindow.IsLoaded)
            {
                _mirrorWindow.Close();
                _mirrorWindow = null;
            }
        };
        mirrorCloseTimer.Start();
    }

    // Interactive Toys: Laser Pointer & Desktop Mirror
    public void ToggleLaserPointer()
    {
        if (_laserWindow == null || !_laserWindow.IsLoaded)
        {
            _laserWindow = new LaserPointerWindow();
            _laserWindow.Closed += (s, e) => { _laserWindow = null; };
            _laserWindow.LaserPositionUpdated += OnLaserPositionUpdated;
            _laserWindow.Show();
            _soundService.PlayTrill();
            SpeechBubble.ShowMessage("🔴 লাল লেজার পয়েন্টার সক্রিয়! মাউস নাড়াও, আমি তাড়া করছি!", "🔴", "Laser Toy", 5);
        }
        else
        {
            _laserWindow.Close();
            _laserWindow = null;
            SpeechBubble.ShowMessage("লেজার পয়েন্টার খেলা শেষ।", "👍", "Laser Toy", 3);
        }
    }

    private void OnLaserPositionUpdated(Point laserPos)
    {
        if (_flightMovementService.IsFlying || Pet.CurrentState == PetState.Sleeping || Pet.CurrentState == PetState.NightSleeping || _isDragging) return;

        double birdCenterX = Left + (Width / 2);
        double birdCenterY = Top + Height - 50;
        double dist = Math.Sqrt(Math.Pow(laserPos.X - birdCenterX, 2) + Math.Pow(laserPos.Y - birdCenterY, 2));

        Pet.SetFacing(laserPos.X < birdCenterX);

        if (dist < 45)
        {
            _laserWindow?.AnimateCatch();
            _soundService.PlayMunch();
            _soundService.PlayHappy();
            Pet.AnimateHearts();
            _settingsService.CurrentSettings.FriendshipXP += 10;
            _settingsService.SaveSettings(_settingsService.CurrentSettings);
            SpeechBubble.ShowMessage("খপ করে লেজার ডটে ঠোকর দিয়েছি! 🔴💥 (+10 XP)", "🎉", "Laser Catch!", 3);
        }
        else if (dist < 340 && _random.Next(100) < 30)
        {
            double targetX = laserPos.X - (Width / 2);
            double targetY = laserPos.Y - Height + 30;
            var workArea = GetCurrentWorkArea();
            targetX = Math.Clamp(targetX, workArea.Left, workArea.Right - Width);
            targetY = Math.Clamp(targetY, workArea.Top, workArea.Bottom - Height);
            StartFlightTo(new Point(targetX, targetY));
        }
    }

    public void ToggleDesktopMirror()
    {
        if (_mirrorWindow == null || !_mirrorWindow.IsLoaded)
        {
            _mirrorWindow = new DesktopMirrorWindow(_settingsService.CurrentSettings.Skin);
            _mirrorWindow.Closed += (s, e) => { _mirrorWindow = null; };

            var workArea = GetCurrentWorkArea();
            double mirrorX = Left > workArea.Left + (workArea.Width / 2) ? Left - 190 : Left + Width + 10;
            mirrorX = Math.Clamp(mirrorX, workArea.Left, workArea.Right - 180);
            _mirrorWindow.Left = mirrorX;
            _mirrorWindow.Top = Math.Clamp(Top + 20, workArea.Top, workArea.Bottom - 240);
            _mirrorWindow.Show();

            Pet.SetFacing(_mirrorWindow.Left < Left);
            Pet.FluffFeathers(() =>
            {
                Pet.SetState(PetState.Dancing);
                _soundService.PlayHappy();
                Pet.AnimateHearts();
                SpeechBubble.ShowMessage("আরে! আয়নায় এত রূপবান পাখিটা কে? 🦜🪞✨", "🪞", "Mirror Trick", 6);
            });
        }
        else
        {
            _mirrorWindow.Close();
            _mirrorWindow = null;
            SpeechBubble.ShowMessage("আয়না সরিয়ে নিয়েছি। 🪞👋", "👍", "Mirror Trick", 3);
        }
    }

    // Dynamic Mood Display
    private void UpdateMoodDisplay()
    {
        if (MenuMoodDisplay != null)
        {
            MenuMoodDisplay.Header = $"🎭 মেজাজ: {_moodService.GetMoodTitle()}";
        }
    }

    private void MenuMood_Click(object sender, RoutedEventArgs e)
    {
        var msg = _moodService.GetMoodDialogue(_settingsService.CurrentSettings.Language);
        SpeechBubble.ShowMessage(msg, "🎭", "Pet Mood", 4);
    }

    // Dynamic Context Menu Handlers
    private void MenuLaserPointer_Click(object sender, RoutedEventArgs e) => ToggleLaserPointer();
    private void MenuDesktopMirror_Click(object sender, RoutedEventArgs e) => ToggleDesktopMirror();
    private void MenuWalkTitlebar_Click(object sender, RoutedEventArgs e) => WalkAlongTitlebar();
    private void MenuFlapPerch_Click(object sender, RoutedEventArgs e)
    {
        Pet.FlapWingsOnPerch();
        _soundService.PlayWingFlap();
        SpeechBubble.ShowMessage("ডালে বসে ডানা মেলে ঝাপটানোর ব্যায়াম করছি! 🪶💨", "🪶", "Perch Flap", 3);
    }
    private void MenuCuriousLook_Click(object sender, RoutedEventArgs e)
    {
        Pet.CuriousLookAround();
        _soundService.PlayCurious();
        SpeechBubble.ShowMessage("আগ্রহ নিয়ে চারপাশের পরিবেশ লক্ষ্য করছি! 👀🔍", "👀", "Observing", 3);
    }
    private void MenuPreen_Click(object sender, RoutedEventArgs e)
    {
        Pet.PreenFeathers();
        _soundService.PlayPreen();
        SpeechBubble.ShowMessage("পালকগুলো খুঁটে নিখুঁত করে নিচ্ছি! ✨", "✨", "Preening", 3);
    }
    private void MenuStretch_Click(object sender, RoutedEventArgs e)
    {
        Pet.StretchWing();
        _soundService.PlayChirp();
        SpeechBubble.ShowMessage("ডানা ও পা টানটান করে চমৎকার স্ট্রেচ! 🧘", "🧘", "Stretch", 3);
    }
    private void MenuFluff_Click(object sender, RoutedEventArgs e)
    {
        Pet.FluffFeathers();
        _soundService.PlayDoubleChirp();
        SpeechBubble.ShowMessage("শরীর কাঁপিয়ে পুরো গা ঝাড়া দিয়ে নিলাম! 💨", "💨", "Fluff", 3);
    }
    private void MenuYawn_Click(object sender, RoutedEventArgs e)
    {
        Pet.Yawn();
        SpeechBubble.ShowMessage("হাঁআআআ... শান্তি করে একটা হাই তুললাম! 🥱", "🥱", "Yawn", 3);
    }
    private void MenuDizzy_Click(object sender, RoutedEventArgs e)
    {
        Pet.MakeDizzy();
        _soundService.PlaySquawk();
        SpeechBubble.ShowMessage("উহুহু... মাথা ঘুরছে! 😵💫", "😵💫", "Dizzy", 4);
    }

    private void MenuBirdcage_Click(object sender, RoutedEventArgs e) => ToggleBirdcage();

    public void ToggleBirdcage()
    {
        var s = _settingsService.CurrentSettings;
        s.BirdcageMode = !s.BirdcageMode;
        _settingsService.SaveSettings(s);

        Pet.SetCageMode(s.BirdcageMode);
        UpdateBirdcageMenuItem(s.BirdcageMode);

        if (s.BirdcageMode)
        {
            _perchedWindowHwnd = IntPtr.Zero;
            _flightMovementService.CancelFlight();
            _soundService.PlayDoubleChirp();
            Pet.SetState(PetState.Idle);
            SpeechBubble.ShowMessage(LocalizationService.Get("Birdcage_CagedBubble", s.Language), "🏰", "Birdcage", 6);
        }
        else
        {
            _soundService.PlayTrill();
            Pet.FlapWingsOnPerch();
            SpeechBubble.ShowMessage(LocalizationService.Get("Birdcage_FreeBubble", s.Language), "🕊️", "Free Flight", 5);
        }
    }

    private void UpdateBirdcageMenuItem(bool isCaged)
    {
        if (MenuBirdcage != null)
        {
            string lang = _settingsService.CurrentSettings.Language;
            MenuBirdcage.Header = LocalizationService.Get(isCaged ? "Birdcage_Release" : "Birdcage_PutIn", lang);
        }
    }

    #endregion
}