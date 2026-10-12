using System;
using System.IO;
using System.Text.Json;
using Parot.Models;

namespace Parot.Services;

public class StatisticsService
{
    private static readonly string StatsFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Parot");

    private static readonly string StatsFilePath = Path.Combine(StatsFolder, "statistics.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public DailyStatistics TodayStats { get; private set; }
    public event Action<DailyStatistics>? StatisticsUpdated;

    public StatisticsService()
    {
        TodayStats = LoadTodayStats();
    }

    private DailyStatistics LoadTodayStats()
    {
        string todayStr = DateTime.Today.ToString("yyyy-MM-dd");
        try
        {
            if (File.Exists(StatsFilePath))
            {
                var json = File.ReadAllText(StatsFilePath);
                var stats = JsonSerializer.Deserialize<DailyStatistics>(json, JsonOptions);
                if (stats != null && stats.Date == todayStr)
                {
                    return stats;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load stats: {ex.Message}");
        }

        return new DailyStatistics { Date = todayStr };
    }

    public void Save()
    {
        try
        {
            if (!Directory.Exists(StatsFolder))
            {
                Directory.CreateDirectory(StatsFolder);
            }
            var json = JsonSerializer.Serialize(TodayStats, JsonOptions);
            File.WriteAllText(StatsFilePath, json);
            StatisticsUpdated?.Invoke(TodayStats);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save stats: {ex.Message}");
        }
    }

    public void AddActiveMinutes(int minutes)
    {
        TodayStats.ActiveMinutes += minutes;
        Save();
    }

    public void LogEyeBreak()
    {
        TodayStats.EyeBreaksCompleted++;
        TodayStats.XPEarnedToday += 20;
        Save();
    }

    public void LogWaterGlass()
    {
        TodayStats.WaterGlassesLogged++;
        TodayStats.XPEarnedToday += 15;
        Save();
    }

    public void LogPomodoro()
    {
        TodayStats.PomodoroCyclesCompleted++;
        TodayStats.XPEarnedToday += 50;
        Save();
    }

    public void LogPet()
    {
        TodayStats.TimesPetted++;
        TodayStats.XPEarnedToday += 5;
        Save();
    }

    public void LogTreat()
    {
        TodayStats.TreatsFed++;
        TodayStats.XPEarnedToday += 10;
        Save();
    }
}
