using System;

namespace Parot.Models;

public class DailyStatistics
{
    public string Date { get; set; } = DateTime.Today.ToString("yyyy-MM-dd");
    public int ActiveMinutes { get; set; } = 0;
    public int EyeBreaksCompleted { get; set; } = 0;
    public int WaterGlassesLogged { get; set; } = 0;
    public int PomodoroCyclesCompleted { get; set; } = 0;
    public int TimesPetted { get; set; } = 0;
    public int TreatsFed { get; set; } = 0;
    public int XPEarnedToday { get; set; } = 0;
}
