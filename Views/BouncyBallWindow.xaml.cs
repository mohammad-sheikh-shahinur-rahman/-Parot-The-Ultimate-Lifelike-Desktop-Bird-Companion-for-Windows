using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Point = System.Windows.Point;

namespace Parot.Views;

public partial class BouncyBallWindow : Window
{
    private readonly DispatcherTimer _physicsTimer;
    private double _vx = 0;
    private double _vy = 0;
    private const double Gravity = 900.0;
    private const double Restitution = 0.72; // Bounciness
    private const double Friction = 0.985; // Rolling friction
    private DateTime _lastTick = DateTime.Now;

    public Point CurrentPosition => new(Left + Width / 2, Top + Height / 2);
    public double VelocityX => _vx;

    public event Action<Point>? UserKicked;
    public event Action<Point>? ParotKicked;

    public BouncyBallWindow()
    {
        InitializeComponent();

        _physicsTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _physicsTimer.Tick += OnPhysicsTick;

        // Position on taskbar initially
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Right - 350;
        Top = workArea.Bottom - Height - 2;

        _lastTick = DateTime.Now;
        _physicsTimer.Start();
    }

    private void OnPhysicsTick(object? sender, EventArgs e)
    {
        double dt = (DateTime.Now - _lastTick).TotalSeconds;
        _lastTick = DateTime.Now;
        if (dt > 0.05) dt = 0.016;

        var workArea = SystemParameters.WorkArea;
        double groundY = workArea.Bottom - Height;

        _vy += Gravity * dt;
        double newX = Left + _vx * dt;
        double newY = Top + _vy * dt;

        // Roll rotation
        BallRotation.Angle += (_vx * dt) * 3.5;

        // Ground collision & bouncing
        if (newY >= groundY)
        {
            newY = groundY;
            if (Math.Abs(_vy) > 80)
            {
                _vy = -_vy * Restitution;
            }
            else
            {
                _vy = 0;
            }

            // Ground friction
            _vx *= Friction;
            if (Math.Abs(_vx) < 3.0) _vx = 0;
        }

        // Left / Right wall bounce
        if (newX <= workArea.Left)
        {
            newX = workArea.Left;
            _vx = -_vx * Restitution;
        }
        else if (newX >= workArea.Right - Width)
        {
            newX = workArea.Right - Width;
            _vx = -_vx * Restitution;
        }

        Left = newX;
        Top = newY;
    }

    public void ParotPush(double forceX)
    {
        _vx = forceX;
        _vy = -180; // Small hop off ground
        ParotKicked?.Invoke(CurrentPosition);
    }

    private void Ball_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Kick ball away from mouse position
        var pos = e.GetPosition(this);
        double kickDir = (pos.X < Width / 2) ? 380.0 : -380.0;
        _vx = kickDir;
        _vy = -260; // Kick into the air
        UserKicked?.Invoke(CurrentPosition);
    }

    private void Ball_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        _physicsTimer.Stop();
        Close();
    }
}
