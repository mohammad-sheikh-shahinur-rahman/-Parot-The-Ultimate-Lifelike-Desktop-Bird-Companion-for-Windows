using System;
using System.Windows;
using System.Windows.Threading;
using Point = System.Windows.Point;

namespace Parot.Services;

public class FlightMovementService
{
    private readonly DispatcherTimer _flightTimer;
    private Point _startPoint;
    private Point _controlPoint;
    private Point _targetPoint;

    private DateTime _flightStartTime;
    private TimeSpan _flightDuration;
    private bool _isFlying = false;

    private bool _flyingNotified = false;
    private bool _landingNotified = false;

    public event Action<Point, double, bool>? PositionUpdated; // (Position, BankAngle, FaceLeft)
    public event Action? TakeOffInitiated;
    public event Action? FlyingInitiated;
    public event Action? LandingInitiated;
    public event Action? FlightCompleted;

    public bool IsFlying => _isFlying;

    public FlightMovementService()
    {
        _flightTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS smooth motion
        };
        _flightTimer.Tick += OnFlightTick;
    }

    public void FlyTo(Point start, Point target, double speedPixelsPerSecond = 460.0)
    {
        if (_isFlying)
        {
            _flightTimer.Stop();
        }

        _startPoint = start;
        _targetPoint = target;

        double dx = target.X - start.X;
        double dy = target.Y - start.Y;
        double distance = Math.Sqrt(dx * dx + dy * dy);

        if (distance < 30)
        {
            // Too close for flight, just trigger completed
            FlightCompleted?.Invoke();
            return;
        }

        // Calculate natural curved flight path with apex lift
        double midX = (start.X + target.X) / 2.0;
        double lift = Math.Min(220, Math.Max(70, distance * 0.40));
        double midY = Math.Min(start.Y, target.Y) - lift;

        // Keep apex within reasonable top screen boundary
        if (midY < 30) midY = 30;

        _controlPoint = new Point(midX, midY);

        double durationSeconds = Math.Clamp(distance / speedPixelsPerSecond, 0.95, 3.2);
        _flightDuration = TimeSpan.FromSeconds(durationSeconds);
        _flightStartTime = DateTime.Now;

        _flyingNotified = false;
        _landingNotified = false;

        _isFlying = true;
        TakeOffInitiated?.Invoke();
        _flightTimer.Start();
    }

    public void CancelFlight()
    {
        if (!_isFlying) return;
        _isFlying = false;
        _flightTimer.Stop();
    }

    private void OnFlightTick(object? sender, EventArgs e)
    {
        if (!_isFlying)
        {
            _flightTimer.Stop();
            return;
        }

        double elapsedSec = (DateTime.Now - _flightStartTime).TotalSeconds;
        double t = Math.Clamp(elapsedSec / _flightDuration.TotalSeconds, 0.0, 1.0);

        // Smooth cubic ease-in-out
        double smoothT = t < 0.5 ? 2 * t * t : 1 - Math.Pow(-2 * t + 2, 2) / 2.0;

        // Quadratic Bezier interpolation: B(t) = (1-t)^2 * P0 + 2(1-t)t * P1 + t^2 * P2
        double u = 1.0 - smoothT;
        double curX = u * u * _startPoint.X + 2 * u * smoothT * _controlPoint.X + smoothT * smoothT * _targetPoint.X;
        double curY = u * u * _startPoint.Y + 2 * u * smoothT * _controlPoint.Y + smoothT * smoothT * _targetPoint.Y;

        // Calculate instantaneous tangent angle for banking/tilting
        double derivX = 2 * u * (_controlPoint.X - _startPoint.X) + 2 * smoothT * (_targetPoint.X - _controlPoint.X);
        double derivY = 2 * u * (_controlPoint.Y - _startPoint.Y) + 2 * smoothT * (_targetPoint.Y - _controlPoint.Y);

        bool faceLeft = derivX < 0;
        double rawAngle = Math.Atan2(derivY, Math.Abs(derivX)) * (180.0 / Math.PI);
        double bankAngle = Math.Clamp(rawAngle * 0.65, -28.0, 28.0);

        // Transition from TakeOff to full Wing-Flapping Flight
        if (t >= 0.12 && t < 0.85 && !_flyingNotified)
        {
            _flyingNotified = true;
            FlyingInitiated?.Invoke();
        }

        // Landing trigger when near target (at 85% progress)
        if (t >= 0.85 && t < 0.98 && !_landingNotified)
        {
            _landingNotified = true;
            LandingInitiated?.Invoke();
        }

        PositionUpdated?.Invoke(new Point(curX, curY), bankAngle, faceLeft);

        if (t >= 1.0)
        {
            _isFlying = false;
            _flightTimer.Stop();
            PositionUpdated?.Invoke(_targetPoint, 0, faceLeft);
            FlightCompleted?.Invoke();
        }
    }
}
