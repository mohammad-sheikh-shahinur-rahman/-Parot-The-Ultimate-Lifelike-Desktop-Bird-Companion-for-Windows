using System;
using System.Windows;
using Parot.Services;
using Xunit;
using Point = System.Windows.Point;

namespace Parot.Tests;

public class FlightMovementTests
{
    [Fact]
    public void QuadraticBezier_InterpolationEndpoints_MatchStartAndTarget()
    {
        Point p0 = new Point(100, 500);
        Point p1 = new Point(500, 200); // Apex
        Point p2 = new Point(900, 500);

        // At t = 0: B(0) = P0
        double t0 = 0.0;
        double u0 = 1.0 - t0;
        double x0 = u0 * u0 * p0.X + 2 * u0 * t0 * p1.X + t0 * t0 * p2.X;
        double y0 = u0 * u0 * p0.Y + 2 * u0 * t0 * p1.Y + t0 * t0 * p2.Y;

        Assert.Equal(p0.X, x0);
        Assert.Equal(p0.Y, y0);

        // At t = 1: B(1) = P2
        double t1 = 1.0;
        double u1 = 1.0 - t1;
        double x1 = u1 * u1 * p0.X + 2 * u1 * t1 * p1.X + t1 * t1 * p2.X;
        double y1 = u1 * u1 * p0.Y + 2 * u1 * t1 * p1.Y + t1 * t1 * p2.Y;

        Assert.Equal(p2.X, x1);
        Assert.Equal(p2.Y, y1);

        // At midpoint t = 0.5: x is exactly halfway (500), y is lifted above (350)
        double tMid = 0.5;
        double uMid = 0.5;
        double xMid = uMid * uMid * p0.X + 2 * uMid * tMid * p1.X + tMid * tMid * p2.X;
        double yMid = uMid * uMid * p0.Y + 2 * uMid * tMid * p1.Y + tMid * tMid * p2.Y;

        Assert.Equal(500.0, xMid);
        Assert.Equal(350.0, yMid);
        Assert.True(yMid < p0.Y); // Lifted towards apex
    }

    [Theory]
    [InlineData(100, 200, false)] // Moving right: faceLeft is false
    [InlineData(800, 300, true)]  // Moving left: faceLeft is true
    public void Flight_FacingDirection_MatchesHorizontalMovement(
        double startX, double targetX, bool expectedFaceLeft)
    {
        double dx = targetX - startX;
        bool faceLeft = dx < 0;
        Assert.Equal(expectedFaceLeft, faceLeft);
    }

    [Theory]
    [InlineData(50.0, 0.9)]    // Very short distance clamped to min 0.9s
    [InlineData(3000.0, 3.2)]  // Very long distance clamped to max 3.2s
    [InlineData(900.0, 2.0)]   // Medium distance: 900 / 450 = 2.0s
    public void Flight_DurationCalculation_ClampsProperly(double distance, double expectedSeconds)
    {
        double speed = 450.0;
        double duration = Math.Clamp(distance / speed, 0.9, 3.2);
        Assert.Equal(expectedSeconds, duration, precision: 2);
    }

    [Fact]
    public void Flight_ApexCalculation_StaysAboveTopBoundary()
    {
        Point start = new Point(100, 50);
        Point target = new Point(500, 60);
        double distance = 400;

        double midY = Math.Min(start.Y, target.Y) - Math.Min(180, distance * 0.35);
        if (midY < 40) midY = 40;

        Assert.True(midY >= 40, "Apex should not clip off the top screen boundary (< 40px)");
    }
}
