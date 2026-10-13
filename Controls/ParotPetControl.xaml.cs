using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Parot.Models;
using Parot.Services;
using Color = System.Windows.Media.Color;
using UserControl = System.Windows.Controls.UserControl;

namespace Parot.Controls;

public partial class ParotPetControl : UserControl
{
    private PetState _currentState = PetState.Idle;
    private readonly Random _random = new();
    private readonly DispatcherTimer _blinkTimer;
    private readonly SpriteAnimationEngine _animEngine;

    private readonly Storyboard _breatheStoryboard;
    private readonly Storyboard _flapStoryboard;
    private readonly Storyboard _hopStoryboard;
    private readonly Storyboard _headTiltStoryboard;
    private readonly Storyboard _chirpStoryboard;
    private readonly Storyboard _danceStoryboard;
    private readonly Storyboard _peckStoryboard;
    private readonly Storyboard _preenStoryboard;
    private readonly Storyboard _stretchStoryboard;
    private readonly Storyboard _fluffStoryboard;
    private readonly Storyboard _yawnStoryboard;
    private readonly Storyboard _dizzyStoryboard;
    private readonly Storyboard _waddleStoryboard;
    private readonly Storyboard _perchFlapStoryboard;
    private readonly Storyboard _curiousLookStoryboard;
    private readonly Storyboard _beakWipeStoryboard;
    private readonly Storyboard _headScratchStoryboard;
    private readonly Storyboard _tailWagStoryboard;
    private DispatcherTimer? _currentActionTimer;

    private bool _isFacingLeft = false;
    private double _currentScale = 1.0;
    private string _currentSkin = "ScarletMacaw";
    private string _currentAccessory = "None";

    private DateTime _cursorOnOtherSideStart = DateTime.MinValue;
    private bool _pendingFaceLeft = false;

    public PetState CurrentState => _currentState;
    public SpriteAnimationEngine AnimationEngine => _animEngine;

    public ParotPetControl()
    {
        InitializeComponent();

        _breatheStoryboard = (Storyboard)Resources["BreatheStoryboard"];
        _flapStoryboard = (Storyboard)Resources["FlapStoryboard"];
        _hopStoryboard = (Storyboard)Resources["HopStoryboard"];
        _headTiltStoryboard = (Storyboard)Resources["HeadTiltStoryboard"];
        _chirpStoryboard = (Storyboard)Resources["ChirpStoryboard"];
        _danceStoryboard = (Storyboard)Resources["DanceStoryboard"];
        _peckStoryboard = (Storyboard)Resources["PeckStoryboard"];
        _preenStoryboard = (Storyboard)Resources["PreenStoryboard"];
        _stretchStoryboard = (Storyboard)Resources["StretchStoryboard"];
        _fluffStoryboard = (Storyboard)Resources["FluffStoryboard"];
        _yawnStoryboard = (Storyboard)Resources["YawnStoryboard"];
        _dizzyStoryboard = (Storyboard)Resources["DizzyStoryboard"];
        _waddleStoryboard = (Storyboard)Resources["WaddleStoryboard"];
        _perchFlapStoryboard = (Storyboard)Resources["PerchFlapStoryboard"];
        _curiousLookStoryboard = (Storyboard)Resources["CuriousLookStoryboard"];
        _beakWipeStoryboard = (Storyboard)Resources["BeakWipeStoryboard"];
        _headScratchStoryboard = (Storyboard)Resources["HeadScratchStoryboard"];
        _tailWagStoryboard = (Storyboard)Resources["TailWagStoryboard"];

        _animEngine = new SpriteAnimationEngine();
        _animEngine.FrameUpdated += OnAnimationFrameUpdated;
        _animEngine.AnimationCompleted += OnAnimationCompleted;

        // Load multi-frame animations
        string birdsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Birds", "Macaw");
        _animEngine.LoadClipsFromDirectory(birdsPath);

        _blinkTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(_random.Next(4, 9))
        };
        _blinkTimer.Tick += OnBlinkTimerTick;
        _blinkTimer.Start();

        SetState(PetState.Idle);
    }

    private void OnAnimationFrameUpdated(BitmapSource frame)
    {
        BirdSpriteImage.Source = frame;
    }

    private void OnAnimationCompleted(string clipName)
    {
        if (clipName == "Landing" || clipName == "Hop" || clipName == "Blink")
        {
            if (_currentState == PetState.Landing || _currentState == PetState.Hopping || _currentState == PetState.Blinking)
            {
                SetState(PetState.Idle);
            }
        }
    }

    public void SetBankAngle(double angle)
    {
        BirdRotateTransform.Angle = angle;
    }

    public void ApplySkin(string skin)
    {
        _currentSkin = string.IsNullOrWhiteSpace(skin) ? "ScarletMacaw" : skin;
        string birdsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Birds", _currentSkin);
        if (!Directory.Exists(birdsPath))
        {
            birdsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Birds", "Macaw");
        }
        if (Directory.Exists(birdsPath))
        {
            _animEngine.LoadClipsFromDirectory(birdsPath);
            SetState(_currentState);
        }
    }

    public void BobHead()
    {
        if (_currentState == PetState.Sleeping || _currentState == PetState.NightSleeping || _currentState == PetState.Eating) return;
        _chirpStoryboard.Begin();
    }

    public void SetAccessory(string accessory)
    {
        _currentAccessory = accessory;

        if (_currentState == PetState.Flying || _currentState == PetState.FollowCursor || _currentState == PetState.TakeOff || _currentState == PetState.Landing || _currentState == PetState.FlappingWings || _currentState == PetState.Dragged)
        {
            AccessoriesCanvas.Visibility = Visibility.Collapsed;
            return;
        }

        AccessoriesCanvas.Visibility = Visibility.Visible;
        AccessoryGlasses.Visibility = accessory == "Glasses" ? Visibility.Visible : Visibility.Collapsed;
        AccessorySunglasses.Visibility = accessory == "Sunglasses" ? Visibility.Visible : Visibility.Collapsed;
        AccessoryScarf.Visibility = accessory == "Scarf" ? Visibility.Visible : Visibility.Collapsed;
        AccessoryTopHat.Visibility = accessory == "TopHat" ? Visibility.Visible : Visibility.Collapsed;
        AccessoryPartyHat.Visibility = accessory == "PartyHat" ? Visibility.Visible : Visibility.Collapsed;
        AccessoryUmbrella.Visibility = accessory == "Umbrella" ? Visibility.Visible : Visibility.Collapsed;
        AccessoryNightcap.Visibility = accessory == "Nightcap" ? Visibility.Visible : Visibility.Collapsed;
    }

    private bool _isCageMode = false;
    public bool IsCageMode => _isCageMode;

    public void SetCageMode(bool isCaged)
    {
        _isCageMode = isCaged;
        BirdCageCanvas.Visibility = isCaged ? Visibility.Visible : Visibility.Collapsed;
    }

    public void SetFacing(bool faceLeft)
    {
        if (_isFacingLeft == faceLeft) return;
        _isFacingLeft = faceLeft;

        var anim = new DoubleAnimation
        {
            To = faceLeft ? -_currentScale : _currentScale,
            Duration = TimeSpan.FromMilliseconds(160)
        };
        RootScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
    }

    public void SetScale(double scale)
    {
        _currentScale = Math.Clamp(scale, 0.6, 2.0);
        RootScaleTransform.ScaleY = _currentScale;
        RootScaleTransform.ScaleX = _isFacingLeft ? -_currentScale : _currentScale;
    }

    private void OnBlinkTimerTick(object? sender, EventArgs e)
    {
        if (_currentState == PetState.Sleeping || _currentState == PetState.NightSleeping || _currentState == PetState.Flying) return;

        TriggerBlink();
        _blinkTimer.Interval = TimeSpan.FromSeconds(_random.Next(4, 9));
    }

    public void TriggerBlink()
    {
        if (_currentState == PetState.Idle)
        {
            _animEngine.Play("Blink");
        }
    }

    public void LookAt(double deltaX, double deltaY)
    {
        if (_currentState == PetState.Sleeping || _currentState == PetState.NightSleeping || _currentState == PetState.Dragged || _currentState == PetState.Flying || _currentState == PetState.Eating) return;

        // Subtle, natural head tilt towards cursor height (zero body jitter)
        BirdTranslateTransform.X = 0;
        double tiltAngle = Math.Clamp(deltaY * 0.03, -4.0, 4.0);
        BirdRotateTransform.Angle = tiltAngle;

        // Hysteresis facing: Only turn body around if cursor has stayed on the other side for > 3.5 seconds and is close
        bool wantsFaceLeft = deltaX < 0;
        double dist = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

        if (wantsFaceLeft != _isFacingLeft)
        {
            if (!_pendingFaceLeft)
            {
                _pendingFaceLeft = true;
                _cursorOnOtherSideStart = DateTime.Now;
            }
            else if ((DateTime.Now - _cursorOnOtherSideStart).TotalSeconds >= 3.5 && dist < 260)
            {
                SetFacing(wantsFaceLeft);
                _pendingFaceLeft = false;
            }
        }
        else
        {
            _pendingFaceLeft = false;
        }
    }

    public void ClearTransientEffects()
    {
        SleepParticles.BeginAnimation(OpacityProperty, null);
        SleepParticles.Opacity = 0;
        DreamBubble.BeginAnimation(OpacityProperty, null);
        DreamBubble.Opacity = 0;
        DizzyParticle.BeginAnimation(OpacityProperty, null);
        DizzyParticle.Opacity = 0;
        HeartParticle.BeginAnimation(OpacityProperty, null);
        HeartParticle.Opacity = 0;
        NoteParticle.BeginAnimation(OpacityProperty, null);
        NoteParticle.Opacity = 0;
    }

    public void StopAllActionStoryboards()
    {
        _currentActionTimer?.Stop();
        _currentActionTimer = null;

        _flapStoryboard.Stop();
        _danceStoryboard.Stop();
        _breatheStoryboard.Stop();
        _waddleStoryboard.Stop();
        _perchFlapStoryboard.Stop();
        _curiousLookStoryboard.Stop();
        _headTiltStoryboard.Stop();
        _peckStoryboard.Stop();
        _preenStoryboard.Stop();
        _stretchStoryboard.Stop();
        _fluffStoryboard.Stop();
        _yawnStoryboard.Stop();
        _dizzyStoryboard.Stop();
        _beakWipeStoryboard.Stop();
        _headScratchStoryboard.Stop();
        _tailWagStoryboard.Stop();

        BirdRotateTransform.Angle = 0;
        BirdTranslateTransform.X = 0;
        BirdTranslateTransform.Y = 0;
    }

    public void SetState(PetState state)
    {
        _currentState = state;

        if (GroundContactShadow != null)
        {
            GroundContactShadow.Visibility = (state == PetState.Flying || state == PetState.TakeOff || state == PetState.Dragged)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        // Clean up transient particle overlays unless actively sleeping
        if (state != PetState.Sleeping && state != PetState.NightSleeping)
        {
            ClearTransientEffects();
        }

        // Always stop conflicting storyboards when switching fundamental activity states
        if (state == PetState.Flying || state == PetState.TakeOff || state == PetState.Landing || state == PetState.Dragged || state == PetState.Idle || state == PetState.Perching || state == PetState.Sleeping || state == PetState.NightSleeping)
        {
            StopAllActionStoryboards();
        }

        switch (state)
        {
            case PetState.Idle:
            case PetState.Perching:
                SetAccessory(_currentAccessory);
                _animEngine.Play("Idle");
                break;

            case PetState.Blinking:
                _animEngine.Play("Blink");
                break;

            case PetState.Hopping:
                _animEngine.Play("Hop");
                _hopStoryboard.Begin();
                break;

            case PetState.TakeOff:
                AccessoriesCanvas.Visibility = Visibility.Collapsed;
                _animEngine.Play("TakeOff");
                break;

            case PetState.Flying:
            case PetState.FollowCursor:
                AccessoriesCanvas.Visibility = Visibility.Collapsed;
                _animEngine.Play("Flying");
                break;

            case PetState.Landing:
                AccessoriesCanvas.Visibility = Visibility.Collapsed;
                _animEngine.Play("Landing");
                break;

            case PetState.FlappingWings:
                AccessoriesCanvas.Visibility = Visibility.Collapsed;
                _animEngine.Play("Flying");
                _perchFlapStoryboard.Begin();
                break;

            case PetState.Dragged:
                AccessoriesCanvas.Visibility = Visibility.Collapsed;
                _animEngine.Play("Flying");
                break;

            case PetState.Dancing:
                _animEngine.Play("Idle");
                _danceStoryboard.Begin();
                AnimateMusicNote();
                break;

            case PetState.Sleeping:
                _animEngine.Play("Sleep");
                AnimateSleepZzz();
                break;

            case PetState.NightSleeping:
                _animEngine.Play("Sleep");
                SetAccessory("Nightcap");
                AnimateSleepZzz();
                break;

            case PetState.Happy:
                _animEngine.Play("Idle");
                AnimateHearts();
                _chirpStoryboard.Begin();
                break;

            case PetState.Eating:
                _chirpStoryboard.Begin();
                AnimateTreatEat();
                break;

            case PetState.Speaking:
                _animEngine.Play("Idle");
                _chirpStoryboard.Begin();
                break;

            case PetState.PomodoroFocus:
                _animEngine.Play("Idle");
                _breatheStoryboard.SetSpeedRatio(0.85);
                SetAccessory("Glasses");
                break;

            case PetState.PomodoroBreak:
                _animEngine.Play("Idle");
                _danceStoryboard.Begin();
                AnimateHearts();
                AnimateMusicNote();
                SetAccessory("PartyHat");
                break;

            case PetState.Walking:
                _animEngine.Play("Idle");
                _waddleStoryboard.Begin();
                break;

            case PetState.PeekingEdge:
                _animEngine.Play("Idle");
                _headTiltStoryboard.Begin();
                break;

            case PetState.Preening:
                _animEngine.Play("Idle");
                _preenStoryboard.Begin();
                break;

            case PetState.Stretching:
                _animEngine.Play("Idle");
                _stretchStoryboard.Begin();
                break;

            case PetState.Fluffing:
                _animEngine.Play("Idle");
                _fluffStoryboard.Begin();
                break;

            case PetState.Yawning:
                _animEngine.Play("Idle");
                _yawnStoryboard.Begin();
                break;

            case PetState.CuriousHeadTilt:
                _animEngine.Play("Idle");
                _curiousLookStoryboard.Begin();
                break;

            case PetState.BeakWiping:
                _animEngine.Play("Idle");
                _beakWipeStoryboard.Begin();
                break;

            case PetState.HeadScratching:
                _animEngine.Play("Idle");
                _headScratchStoryboard.Begin();
                break;

            case PetState.TailWagging:
                _animEngine.Play("Idle");
                _tailWagStoryboard.Begin();
                break;

            case PetState.Dizzy:
                _animEngine.Play("Idle");
                _dizzyStoryboard.Begin();
                AnimateDizzyStars();
                break;
        }
    }

    private void StartAction(PetState state, TimeSpan duration, Action? onFinish)
    {
        _currentActionTimer?.Stop();
        SetState(state);

        _currentActionTimer = new DispatcherTimer { Interval = duration };
        _currentActionTimer.Tick += (s, e) =>
        {
            _currentActionTimer?.Stop();
            _currentActionTimer = null;
            if (_currentState == state)
            {
                SetState(PetState.Idle);
                onFinish?.Invoke();
            }
        };
        _currentActionTimer.Start();
    }

    public void FlapWingsOnPerch(Action? onCompleted = null)
    {
        StartAction(PetState.FlappingWings, TimeSpan.FromSeconds(1.6), () =>
        {
            _perchFlapStoryboard.Stop();
            onCompleted?.Invoke();
        });
    }

    public void CuriousLookAround(Action? onCompleted = null)
    {
        StartAction(PetState.CuriousHeadTilt, TimeSpan.FromSeconds(2.0), () =>
        {
            _curiousLookStoryboard.Stop();
            onCompleted?.Invoke();
        });
    }

    public void PreenFeathers(Action? onCompleted = null)
    {
        StartAction(PetState.Preening, TimeSpan.FromSeconds(1.8), () =>
        {
            _preenStoryboard.Stop();
            onCompleted?.Invoke();
        });
    }

    public void StretchWing(Action? onCompleted = null)
    {
        StartAction(PetState.Stretching, TimeSpan.FromSeconds(2.0), () =>
        {
            _stretchStoryboard.Stop();
            onCompleted?.Invoke();
        });
    }

    public void FluffFeathers(Action? onCompleted = null)
    {
        StartAction(PetState.Fluffing, TimeSpan.FromSeconds(0.75), () =>
        {
            _fluffStoryboard.Stop();
            onCompleted?.Invoke();
        });
    }

    public void Yawn(Action? onCompleted = null)
    {
        StartAction(PetState.Yawning, TimeSpan.FromSeconds(1.9), () =>
        {
            _yawnStoryboard.Stop();
            onCompleted?.Invoke();
        });
    }

    public void BeakWipe(Action? onCompleted = null)
    {
        StartAction(PetState.BeakWiping, TimeSpan.FromSeconds(1.6), () =>
        {
            _beakWipeStoryboard.Stop();
            onCompleted?.Invoke();
        });
    }

    public void HeadScratch(Action? onCompleted = null)
    {
        StartAction(PetState.HeadScratching, TimeSpan.FromSeconds(1.4), () =>
        {
            _headScratchStoryboard.Stop();
            onCompleted?.Invoke();
        });
    }

    public void TailWag(Action? onCompleted = null)
    {
        StartAction(PetState.TailWagging, TimeSpan.FromSeconds(0.85), () =>
        {
            _tailWagStoryboard.Stop();
            onCompleted?.Invoke();
        });
    }

    public void MakeDizzy(Action? onCompleted = null)
    {
        StartAction(PetState.Dizzy, TimeSpan.FromSeconds(1.5), () =>
        {
            _dizzyStoryboard.Stop();
            onCompleted?.Invoke();
        });
    }

    public void AnimateDizzyStars()
    {
        DizzyParticle.Opacity = 1;
        var moveUp = new DoubleAnimation
        {
            From = 0,
            To = -18,
            Duration = TimeSpan.FromSeconds(1.4),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        var fade = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromSeconds(1.4)
        };
        fade.Completed += (s, e) => DizzyParticle.Opacity = 0;
        DizzyTranslate.BeginAnimation(TranslateTransform.YProperty, moveUp);
        DizzyParticle.BeginAnimation(OpacityProperty, fade);
    }

    public void StartWaddling()
    {
        _waddleStoryboard.Begin();
    }

    public void StopWaddling()
    {
        _waddleStoryboard.Stop();
        BirdRotateTransform.Angle = 0;
        BirdTranslateTransform.Y = 0;
    }

    public void ApplyTimeOfDayLighting(int hour)
    {
        // Pure transparent background: no ambient tint box overlay
    }

    public void AnimateHearts()
    {
        HeartParticle.Opacity = 1;
        var moveUp = new DoubleAnimation
        {
            From = 0,
            To = -35,
            Duration = TimeSpan.FromSeconds(1.2),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        var fade = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromSeconds(1.2)
        };
        fade.Completed += (s, e) => HeartParticle.Opacity = 0;

        HeartTranslate.BeginAnimation(TranslateTransform.YProperty, moveUp);
        HeartParticle.BeginAnimation(OpacityProperty, fade);
    }

    public void AnimateMusicNote()
    {
        NoteParticle.Opacity = 1;
        var moveUp = new DoubleAnimation
        {
            From = 0,
            To = -30,
            Duration = TimeSpan.FromSeconds(1.0),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        var fade = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromSeconds(1.0)
        };
        fade.Completed += (s, e) => NoteParticle.Opacity = 0;

        NoteTranslate.BeginAnimation(TranslateTransform.YProperty, moveUp);
        NoteParticle.BeginAnimation(OpacityProperty, fade);
    }

    private void AnimateSleepZzz()
    {
        var fade = new DoubleAnimation
        {
            From = 0,
            To = 0.9,
            Duration = TimeSpan.FromSeconds(0.8)
        };
        SleepParticles.BeginAnimation(OpacityProperty, fade);
        DreamBubble.BeginAnimation(OpacityProperty, fade);
    }

    public void PeckAndEat(string emoji = "🐞", Action? onPeckSound = null, Action? onCompleted = null)
    {
        _currentState = PetState.Eating;
        _flapStoryboard.Stop();
        _danceStoryboard.Stop();
        _breatheStoryboard.Stop();

        TreatItem.Text = emoji;
        TreatItem.Opacity = 1;
        TreatTranslate.Y = 0;

        // Perform 3 realistic downward pecks
        _peckStoryboard.Begin();
        onPeckSound?.Invoke();

        var peckTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(240) };
        int peckCount = 1;
        peckTimer.Tick += (s, e) =>
        {
            peckCount++;
            onPeckSound?.Invoke();
            if (peckCount >= 3)
            {
                peckTimer.Stop();
            }
        };
        peckTimer.Start();

        // Fade out food item right as 3rd peck finishes
        var fade = new DoubleAnimation
        {
            From = 1,
            To = 0,
            BeginTime = TimeSpan.FromMilliseconds(620),
            Duration = TimeSpan.FromMilliseconds(180)
        };
        fade.Completed += (s, e) =>
        {
            TreatItem.Opacity = 0;
            AnimateHearts();
            onCompleted?.Invoke();
            SetState(PetState.Idle);
        };

        TreatItem.BeginAnimation(OpacityProperty, fade);
    }

    private void AnimateTreatEat()
    {
        PeckAndEat("🌶️");
    }
}
