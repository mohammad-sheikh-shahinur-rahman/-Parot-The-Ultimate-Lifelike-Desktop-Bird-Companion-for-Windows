using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Parot.Services;

public class WeatherInfo
{
    public double Temperature { get; set; }
    public int WeatherCode { get; set; }
    public bool IsDay { get; set; }
    public string ConditionDescription { get; set; } = "Sunny";
    public string RecommendedAccessory { get; set; } = "None";
    public string Dialogue { get; set; } = "";
}

public class WeatherService : IDisposable
{
    private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(8) };
    private readonly DispatcherTimer _timer;
    private bool _disposed = false;

    public event Action<WeatherInfo>? WeatherUpdated;
    public WeatherInfo? LastWeather { get; private set; }

    public WeatherService()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(30)
        };
        _timer.Tick += async (s, e) => await CheckWeatherAsync();
        _timer.Start();

        // Initial check in background after short delay
        Task.Run(async () =>
        {
            await Task.Delay(2000);
            await CheckWeatherAsync();
        });
    }

    public async Task<WeatherInfo?> CheckWeatherAsync()
    {
        try
        {
            // Default coords: Dhaka (23.81, 90.41), can be queried or fallback
            double lat = 23.8103;
            double lon = 90.4125;

            // Attempt IP-based geolocation
            try
            {
                var geoResp = await _httpClient.GetStringAsync("http://ip-api.com/json");
                using var geoDoc = JsonDocument.Parse(geoResp);
                if (geoDoc.RootElement.TryGetProperty("lat", out var latProp) &&
                    geoDoc.RootElement.TryGetProperty("lon", out var lonProp))
                {
                    lat = latProp.GetDouble();
                    lon = lonProp.GetDouble();
                }
            }
            catch
            {
                // Fallback to default coordinates
            }

            string url = $"https://api.open-meteo.com/v1/forecast?latitude={lat:F4}&longitude={lon:F4}&current=temperature_2m,weather_code,is_day";
            var resp = await _httpClient.GetStringAsync(url);
            using var doc = JsonDocument.Parse(resp);

            if (doc.RootElement.TryGetProperty("current", out var current))
            {
                double temp = current.GetProperty("temperature_2m").GetDouble();
                int code = current.GetProperty("weather_code").GetInt32();
                bool isDay = current.GetProperty("is_day").GetInt32() == 1;

                var info = EvaluateWeather(temp, code, isDay);
                LastWeather = info;
                WeatherUpdated?.Invoke(info);
                return info;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Weather check failed: {ex.Message}");
        }

        return null;
    }

    private static WeatherInfo EvaluateWeather(double temp, int code, bool isDay)
    {
        var info = new WeatherInfo
        {
            Temperature = temp,
            WeatherCode = code,
            IsDay = isDay
        };

        // Determine condition, accessory and dialogue
        if ((code >= 51 && code <= 67) || (code >= 80 && code <= 82) || (code >= 95 && code <= 99))
        {
            info.ConditionDescription = "Rainy";
            info.RecommendedAccessory = "Umbrella";
            info.Dialogue = $"বাইরে বৃষ্টি হচ্ছে ({temp:F0}°C)! আমি ছাতা নিয়েছি ☂️ / It's raining outside!";
        }
        else if (temp < 16.0 || (code >= 71 && code <= 77))
        {
            info.ConditionDescription = "Cold";
            info.RecommendedAccessory = "Scarf";
            info.Dialogue = $"বাইরে বেশ ঠাণ্ডা ({temp:F0}°C)! গলায় মাফলার জড়িয়েছি 🧣 / Brrr, it's chilly outside!";
        }
        else if (temp > 28.0 && isDay)
        {
            info.ConditionDescription = "Sunny & Warm";
            info.RecommendedAccessory = "Sunglasses";
            info.Dialogue = $"রোদ ঝলমলে চমৎকার দিন ({temp:F0}°C)! সানগ্লাস পরেছি 🕶️ / Sunny day vibes!";
        }
        else
        {
            info.ConditionDescription = "Pleasant";
            info.RecommendedAccessory = "None";
            info.Dialogue = $"আজকের আবহাওয়া বেশ মনোরম ({temp:F0}°C)! 🌤️ / Pleasant weather today!";
        }

        return info;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _timer.Stop();
    }
}
