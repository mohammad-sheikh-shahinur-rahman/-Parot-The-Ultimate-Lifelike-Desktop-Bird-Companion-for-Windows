using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Point = System.Windows.Point;

namespace Parot.Views;

public partial class TreatItemWindow : Window
{
    private readonly DispatcherTimer _physicsTimer;
    private double _vx = 0;
    private double _vy = 0;
    private const double Gravity = 980.0; // px/s^2
    private DateTime _lastTick = DateTime.Now;
    private bool _isCaught = false;

    public bool IsCaught => _isCaught;
    public Point CurrentPosition => new(Left + Width / 2, Top + Height / 2);
    public event Action<Point>? Landed;

    public TreatItemWindow(string treatEmoji = "🌶️")
    {
        InitializeComponent();
        TreatIcon.Text = treatEmoji;

        _physicsTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _physicsTimer.Tick += OnPhysicsTick;
    }

    public void Throw(Point startPos, Vector initialVelocity)
    {
        Left = startPos.X - Width / 2;
        Top = startPos.Y - Height / 2;
        _vx = initialVelocity.X;
        _vy = initialVelocity.Y;
        _lastTick = DateTime.Now;
        Show();
        _physicsTimer.Start();
    }

    private void OnPhysicsTick(object? sender, EventArgs e)
    {
        if (_isCaught) return;

        double dt = (DateTime.Now - _lastTick).TotalSeconds;
        _lastTick = DateTime.Now;
        if (dt > 0.05) dt = 0.016;

        _vy += Gravity * dt;
        Left += _vx * dt;
        Top += _vy * dt;
        TreatRotate.Angle += _vx * dt * 0.5;

        // Ground check
        var workArea = SystemParameters.WorkArea;
        double groundY = workArea.Bottom - Height;
        if (Top >= groundY)
        {
            Top = groundY;
            _physicsTimer.Stop();
            Landed?.Invoke(CurrentPosition);
        }

        // Screen boundary clamp
        if (Left < workArea.Left) Left = workArea.Left;
        if (Left > workArea.Right - Width) Left = workArea.Right - Width;
    }

    public void Catch()
    {
        if (_isCaught) return;
        _isCaught = true;
        _physicsTimer.Stop();

        // Shrink & Pop animation
        var scaleAnim = new DoubleAnimation
        {
            To = 0,
            Duration = TimeSpan.FromMilliseconds(180),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };
        scaleAnim.Completed += (s, e) => Close();
        TreatScale.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty, scaleAnim);
        TreatScale.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty, scaleAnim);
    }
}
