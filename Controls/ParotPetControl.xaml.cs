using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Parot.Models;
using UserControl = System.Windows.Controls.UserControl;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using ColorConverter = System.Windows.Media.ColorConverter;

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
    private readonly Storyboard _danceStoryboard;

    private bool _isFacingLeft = false;
    private string _currentSkin = "ScarletMacaw";
    private string _currentAccessory = "None";

    public PetState CurrentState => _currentState;

    public ParotPetControl()
    {
        InitializeComponent();

        _breatheStoryboard = (Storyboard)Resources["BreatheStoryboard"];
        _flapStoryboard = (Storyboard)Resources["FlapStoryboard"];
        _hopStoryboard = (Storyboard)Resources["HopStoryboard"];
        _headTiltStoryboard = (Storyboard)Resources["HeadTiltStoryboard"];
        _chirpStoryboard = (Storyboard)Resources["ChirpStoryboard"];
        _danceStoryboard = (Storyboard)Resources["DanceStoryboard"];

        _breatheStoryboard.Begin();

        _blinkTimer = new DispatcherTimer();
        _blinkTimer.Interval = TimeSpan.FromSeconds(_random.Next(3, 6));
        _blinkTimer.Tick += OnBlinkTimerTick;
        _blinkTimer.Start();

        ApplySkin("ScarletMacaw");
    }

    public void ApplySkin(string skin)
    {
        _currentSkin = skin;

        switch (skin)
        {
            case "GreenParakeet":
                CockatielCrest.Visibility = Visibility.Collapsed;
                MacawFaceMask.Visibility = Visibility.Collapsed;
                FaceStripes.Visibility = Visibility.Collapsed;

                BodyMainPath.Fill = CreateLinearBrush("#4ADE80", "#22C55E", "#15803D");
                HeadBasePath.Fill = CreateLinearBrush("#4ADE80", "#22C55E", "#16A34A");
                BellyPath.Fill = CreateRadialBrush("#BBF7D0", "#86EFAC");
                LeftWing.Fill = new SolidColorBrush(Color.FromRgb(21, 128, 61));
                WingRedCoverts.Fill = new SolidColorBrush(Color.FromRgb(34, 197, 94));
                WingYellowBand.Fill = new SolidColorBrush(Color.FromRgb(22, 163, 74));
                WingBluePrimaries.Fill = new SolidColorBrush(Color.FromRgb(21, 128, 61));
                UpperBeak.Fill = CreateLinearBrush("#F97316", "#EA580C", "#DC2626");
                LowerBeak.Fill = new SolidColorBrush(Color.FromRgb(194, 65, 12));
                TailFeatherPath.Fill = CreateLinearBrush("#059669", "#0D9488", "#0284C7");
                TailOuterBlueLeft.Fill = new SolidColorBrush(Color.FromRgb(5, 150, 105));
                TailOuterBlueRight.Fill = new SolidColorBrush(Color.FromRgb(2, 132, 199));
                Eyelid.Fill = new SolidColorBrush(Color.FromRgb(34, 197, 94));
                break;

            case "Cockatiel":
                CockatielCrest.Visibility = Visibility.Visible;
                MacawFaceMask.Visibility = Visibility.Collapsed;
                FaceStripes.Visibility = Visibility.Collapsed;

                BodyMainPath.Fill = CreateLinearBrush("#FEF08A", "#FACC15", "#EAB308");
                HeadBasePath.Fill = CreateLinearBrush("#FEF9C3", "#FDE047", "#FACC15");
                BellyPath.Fill = CreateRadialBrush("#FEFCE8", "#FEF08A");
                LeftWing.Fill = new SolidColorBrush(Color.FromRgb(202, 138, 4));
                WingRedCoverts.Fill = new SolidColorBrush(Color.FromRgb(234, 179, 8));
                WingYellowBand.Fill = new SolidColorBrush(Color.FromRgb(250, 204, 21));
                WingBluePrimaries.Fill = new SolidColorBrush(Color.FromRgb(148, 163, 184));
                UpperBeak.Fill = CreateLinearBrush("#94A3B8", "#64748B", "#475569");
                LowerBeak.Fill = new SolidColorBrush(Color.FromRgb(71, 85, 105));
                TailFeatherPath.Fill = CreateLinearBrush("#EAB308", "#CA8A04", "#A16207");
                TailOuterBlueLeft.Fill = new SolidColorBrush(Color.FromRgb(202, 138, 4));
                TailOuterBlueRight.Fill = new SolidColorBrush(Color.FromRgb(161, 98, 7));
                Eyelid.Fill = new SolidColorBrush(Color.FromRgb(250, 204, 21));
                break;

            case "BlueMacaw":
                CockatielCrest.Visibility = Visibility.Collapsed;
                MacawFaceMask.Visibility = Visibility.Visible;
                FaceStripes.Visibility = Visibility.Visible;
                FaceStripes.Stroke = new SolidColorBrush(Color.FromRgb(15, 23, 42));

                BodyMainPath.Fill = CreateLinearBrush("#2563EB", "#1D4ED8", "#1E40AF");
                HeadBasePath.Fill = CreateLinearBrush("#3B82F6", "#2563EB", "#1D4ED8");
                BellyPath.Fill = CreateRadialBrush("#FEF08A", "#FBBF24");
                LeftWing.Fill = new SolidColorBrush(Color.FromRgb(30, 64, 175));
                WingRedCoverts.Fill = new SolidColorBrush(Color.FromRgb(29, 78, 216));
                WingYellowBand.Fill = CreateLinearBrush("#60A5FA", "#3B82F6", "#2563EB");
                WingBluePrimaries.Fill = CreateLinearBrush("#1D4ED8", "#1E40AF", "#172554");
                UpperBeak.Fill = CreateLinearBrush("#334155", "#1E293B", "#0F172A");
                LowerBeak.Fill = new SolidColorBrush(Color.FromRgb(15, 23, 42));
                TailFeatherPath.Fill = CreateLinearBrush("#2563EB", "#1D4ED8", "#0284C7");
                TailOuterBlueLeft.Fill = new SolidColorBrush(Color.FromRgb(29, 78, 216));
                TailOuterBlueRight.Fill = new SolidColorBrush(Color.FromRgb(2, 132, 199));
                Eyelid.Fill = new SolidColorBrush(Color.FromRgb(37, 99, 235));
                break;

            default: // 🦜 REALISTIC SCARLET MACAW (Ara macao) - EXACT TO USER PHOTOS!
                CockatielCrest.Visibility = Visibility.Collapsed;
                MacawFaceMask.Visibility = Visibility.Visible;
                FaceStripes.Visibility = Visibility.Visible;
                FaceStripes.Stroke = new SolidColorBrush(Color.FromRgb(220, 38, 38));

                BodyMainPath.Fill = CreateLinearBrush("#F87171", "#EF4444", "#DC2626");
                HeadBasePath.Fill = CreateLinearBrush("#F87171", "#EF4444", "#DC2626");
                BellyPath.Fill = CreateRadialBrush("#FCA5A5", "#EF4444");
                LeftWing.Fill = new SolidColorBrush(Color.FromRgb(185, 28, 28));

                // Layered Wing Coverts: Scarlet Red -> Canary Yellow -> Cobalt Blue
                WingRedCoverts.Fill = new SolidColorBrush(Color.FromRgb(220, 38, 38));
                WingYellowBand.Fill = CreateLinearBrush("#FDE047", "#FACC15", "#EAB308");
                WingBluePrimaries.Fill = CreateLinearBrush("#3B82F6", "#2563EB", "#1D4ED8");

                // Beak: Ivory/Pale Horn upper, Charcoal lower
                UpperBeak.Fill = CreateLinearBrush("#FFFFFF", "#FEF3C7", "#FDE68A");
                LowerBeak.Fill = new SolidColorBrush(Color.FromRgb(15, 23, 42));

                // Tail: Long Scarlet Central, Royal Blue Outer
                TailFeatherPath.Fill = CreateLinearBrush("#EF4444", "#DC2626", "#B91C1C");
                TailOuterBlueLeft.Fill = new SolidColorBrush(Color.FromRgb(29, 78, 216));
                TailOuterBlueRight.Fill = new SolidColorBrush(Color.FromRgb(37, 99, 235));

                Eyelid.Fill = new SolidColorBrush(Color.FromRgb(220, 38, 38));
                break;
        }
    }

    public void SetAccessory(string accessory)
    {
        _currentAccessory = accessory;

        AccessoryGlasses.Visibility = accessory == "Glasses" ? Visibility.Visible : Visibility.Collapsed;
        AccessorySunglasses.Visibility = accessory == "Sunglasses" ? Visibility.Visible : Visibility.Collapsed;
        AccessoryScarf.Visibility = accessory == "Scarf" ? Visibility.Visible : Visibility.Collapsed;
        AccessoryTopHat.Visibility = accessory == "TopHat" ? Visibility.Visible : Visibility.Collapsed;
        AccessoryPartyHat.Visibility = accessory == "PartyHat" ? Visibility.Visible : Visibility.Collapsed;
        AccessoryUmbrella.Visibility = accessory == "Umbrella" ? Visibility.Visible : Visibility.Collapsed;
        AccessoryNightcap.Visibility = accessory == "Nightcap" ? Visibility.Visible : Visibility.Collapsed;
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
        RootScaleTransform.ScaleX = _isFacingLeft ? -scale : scale;
    }

    private void OnBlinkTimerTick(object? sender, EventArgs e)
    {
        if (_currentState == PetState.Sleeping || _currentState == PetState.NightSleeping || _currentState == PetState.Happy) return;

        TriggerBlink();
        _blinkTimer.Interval = TimeSpan.FromSeconds(_random.Next(3, 7));
    }

    public void TriggerBlink()
    {
        if (_currentState == PetState.Sleeping || _currentState == PetState.NightSleeping) return;

        var blinkAnim = new DoubleAnimationUsingKeyFrames();
        blinkAnim.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        blinkAnim.KeyFrames.Add(new LinearDoubleKeyFrame(22, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(70))));
        blinkAnim.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(160))));

        Eyelid.BeginAnimation(HeightProperty, blinkAnim);
    }

    public void LookAt(double deltaX, double deltaY)
    {
        if (_currentState == PetState.Sleeping || _currentState == PetState.NightSleeping || _currentState == PetState.Dragged) return;

        double maxOffset = 3.5;
        double angle = Math.Atan2(deltaY, deltaX);
        double dist = Math.Min(Math.Sqrt(deltaX * deltaX + deltaY * deltaY), 300);
        double factor = dist / 300.0;

        double targetX = Math.Cos(angle) * maxOffset * factor;
        double targetY = Math.Sin(angle) * (maxOffset * 0.7) * factor;

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
                _danceStoryboard.Stop();
                _breatheStoryboard.SetSpeedRatio(1.0);
                SleepParticles.Opacity = 0;
                SetAccessory(_currentAccessory);
                break;

            case PetState.Dancing:
                ShowHappyEye();
                _flapStoryboard.Stop();
                _danceStoryboard.Begin();
                AnimateMusicNote();
                break;

            case PetState.PomodoroFocus:
                ShowAwakeEye();
                _flapStoryboard.Stop();
                _danceStoryboard.Stop();
                SetAccessory("Glasses");
                break;

            case PetState.PomodoroBreak:
                ShowHappyEye();
                _flapStoryboard.Begin();
                AnimateHearts();
                AnimateMusicNote();
                SetAccessory("PartyHat");
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
                break;

            case PetState.Hopping:
                ShowAwakeEye();
                _hopStoryboard.Begin();
                break;

            case PetState.Sleeping:
                ShowSleepEye();
                _flapStoryboard.Stop();
                _danceStoryboard.Stop();
                _breatheStoryboard.SetSpeedRatio(0.5);
                AnimateSleepZzz();
                break;

            case PetState.NightSleeping:
                ShowSleepEye();
                _flapStoryboard.Stop();
                _danceStoryboard.Stop();
                _breatheStoryboard.SetSpeedRatio(0.4);
                SetAccessory("Nightcap");
                AnimateSleepZzz();
                break;

            case PetState.Happy:
                ShowHappyEye();
                _flapStoryboard.Stop();
                _danceStoryboard.Stop();
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

    private static LinearGradientBrush CreateLinearBrush(string c1, string c2, string c3)
    {
        var b = new LinearGradientBrush { StartPoint = new Point(0.2, 0), EndPoint = new Point(0.8, 1) };
        b.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(c1), 0));
        b.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(c2), 0.5));
        b.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(c3), 1));
        return b;
    }

    private static RadialGradientBrush CreateRadialBrush(string c1, string c2)
    {
        var b = new RadialGradientBrush { Center = new Point(0.6, 0.6), RadiusX = 0.6, RadiusY = 0.6 };
        b.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(c1), 0));
        b.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(c2), 1));
        return b;
    }
}
