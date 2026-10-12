using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Parot.Models;
using UserControl = System.Windows.Controls.UserControl;

namespace Parot.Controls;

public partial class ParotPetControl : UserControl
{
    private PetState _currentState = PetState.Idle;
    private readonly Random _random = new();
    private readonly DispatcherTimer _blinkTimer;
    private readonly Storyboard _breatheStoryboard;
    private readonly Storyboard _flapStoryboard;
    private readonly Storyboard _hopStoryboard;
    private readonly Storyboard _headTiltStoryboard;
    private readonly Storyboard _chirpStoryboard;

    private bool _isFacingLeft = false;

    public PetState CurrentState => _currentState;

    public ParotPetControl()
    {
        InitializeComponent();

        _breatheStoryboard = (Storyboard)Resources["BreatheStoryboard"];
        _flapStoryboard = (Storyboard)Resources["FlapStoryboard"];
        _hopStoryboard = (Storyboard)Resources["HopStoryboard"];
        _headTiltStoryboard = (Storyboard)Resources["HeadTiltStoryboard"];
        _chirpStoryboard = (Storyboard)Resources["ChirpStoryboard"];

        _breatheStoryboard.Begin();

        _blinkTimer = new DispatcherTimer();
        _blinkTimer.Interval = TimeSpan.FromSeconds(_random.Next(3, 6));
        _blinkTimer.Tick += OnBlinkTimerTick;
        _blinkTimer.Start();
    }

    public void SetFacing(bool faceLeft)
    {
        if (_isFacingLeft == faceLeft) return;
        _isFacingLeft = faceLeft;

        var anim = new DoubleAnimation
        {
            To = faceLeft ? -1 : 1,
            Duration = TimeSpan.FromMilliseconds(180)
        };
        RootScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
    }

    public void SetScale(double scale)
    {
        scale = Math.Clamp(scale, 0.6, 2.0);
        RootScaleTransform.ScaleY = scale;
        if (!_isFacingLeft)
        {
            RootScaleTransform.ScaleX = scale;
        }
        else
        {
            RootScaleTransform.ScaleX = -scale;
        }
    }

    private void OnBlinkTimerTick(object? sender, EventArgs e)
    {
        if (_currentState == PetState.Sleeping || _currentState == PetState.Happy) return;

        TriggerBlink();
        _blinkTimer.Interval = TimeSpan.FromSeconds(_random.Next(3, 7));
    }

    public void TriggerBlink()
    {
        if (_currentState == PetState.Sleeping) return;

        var blinkAnim = new DoubleAnimationUsingKeyFrames();
        blinkAnim.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        blinkAnim.KeyFrames.Add(new LinearDoubleKeyFrame(22, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(70))));
        blinkAnim.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(160))));

        Eyelid.BeginAnimation(HeightProperty, blinkAnim);
    }

    public void LookAt(double deltaX, double deltaY)
    {
        if (_currentState == PetState.Sleeping || _currentState == PetState.Dragged) return;

        // Smooth pupil tracking within +/- 3 pixels
        double maxOffset = 3.5;
        double angle = Math.Atan2(deltaY, deltaX);
        double dist = Math.Min(Math.Sqrt(deltaX * deltaX + deltaY * deltaY), 300);
        double factor = dist / 300.0;

        double targetX = Math.Cos(angle) * maxOffset * factor;
        double targetY = Math.Sin(angle) * (maxOffset * 0.7) * factor;

        // Adjust if bird is flipped
        if (_isFacingLeft) targetX = -targetX;

        PupilTranslate.X = targetX;
        PupilTranslate.Y = targetY;
    }

    public void SetState(PetState state)
    {
        _currentState = state;

        switch (state)
        {
            case PetState.Idle:
                ShowAwakeEye();
                _flapStoryboard.Stop();
                _breatheStoryboard.SetSpeedRatio(1.0);
                SleepParticles.Opacity = 0;
                break;

            case PetState.CuriousHeadTilt:
                ShowAwakeEye();
                _headTiltStoryboard.Begin();
                break;

            case PetState.FlappingWings:
                ShowAwakeEye();
                _flapStoryboard.Begin();
                break;

            case PetState.Dragged:
                ShowAwakeEye();
                _flapStoryboard.Begin();
                // Pull feet up slightly
                break;

            case PetState.Hopping:
                ShowAwakeEye();
                _hopStoryboard.Begin();
                break;

            case PetState.Sleeping:
                ShowSleepEye();
                _flapStoryboard.Stop();
                _breatheStoryboard.SetSpeedRatio(0.5); // Slower, deeper breaths
                AnimateSleepZzz();
                break;

            case PetState.Happy:
                ShowHappyEye();
                _flapStoryboard.Stop();
                AnimateHearts();
                _chirpStoryboard.Begin();
                break;

            case PetState.Eating:
                ShowHappyEye();
                _chirpStoryboard.Begin();
                AnimateTreatEat();
                break;

            case PetState.Speaking:
                ShowAwakeEye();
                _chirpStoryboard.Begin();
                break;
        }
    }

    private void ShowAwakeEye()
    {
        AwakeEye.Visibility = Visibility.Visible;
        HappyEye.Visibility = Visibility.Collapsed;
        SleepEye.Visibility = Visibility.Collapsed;
        Eyelid.Height = 0;
    }

    private void ShowHappyEye()
    {
        AwakeEye.Visibility = Visibility.Collapsed;
        HappyEye.Visibility = Visibility.Visible;
        SleepEye.Visibility = Visibility.Collapsed;
    }

    private void ShowSleepEye()
    {
        AwakeEye.Visibility = Visibility.Collapsed;
        HappyEye.Visibility = Visibility.Collapsed;
        SleepEye.Visibility = Visibility.Visible;
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
    }

    private void AnimateTreatEat()
    {
        TreatItem.Opacity = 1;
        var drop = new DoubleAnimation
        {
            From = -25,
            To = 20,
            Duration = TimeSpan.FromMilliseconds(450),
            EasingFunction = new BounceEase { Bounces = 1, Bounciness = 2 }
        };
        var fade = new DoubleAnimation
        {
            From = 1,
            To = 0,
            BeginTime = TimeSpan.FromMilliseconds(400),
            Duration = TimeSpan.FromMilliseconds(200)
        };
        fade.Completed += (s, e) =>
        {
            TreatItem.Opacity = 0;
            AnimateHearts();
            SetState(PetState.Idle);
        };

        TreatTranslate.BeginAnimation(TranslateTransform.YProperty, drop);
        TreatItem.BeginAnimation(OpacityProperty, fade);
    }
}
