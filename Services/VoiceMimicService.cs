using System;
using System.Net.Http;
using System.Speech.Synthesis;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Parot.Services;

public class VoiceMimicService : IDisposable
{
    private SpeechSynthesizer? _synth;
    private static readonly HttpClient _httpClient = new();

    public bool Enabled { get; set; } = true;
    public int VoiceRate { get; set; } = 4; // Cartoon fast chirp

    public VoiceMimicService(bool enabled = true, int voiceRate = 4)
    {
        Enabled = enabled;
        VoiceRate = voiceRate;

        try
        {
            _synth = new SpeechSynthesizer();
            _synth.Rate = Math.Clamp(voiceRate, -10, 10);
            _synth.Volume = 85;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Speech synthesis init failed: {ex.Message}");
        }
    }

    public void Speak(string text)
    {
        if (!Enabled || _synth == null || string.IsNullOrWhiteSpace(text)) return;

        Task.Run(() =>
        {
            try
            {
                _synth.SpeakAsyncCancelAll();
                _synth.Rate = VoiceRate;
                _synth.SpeakAsync(text);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Speak error: {ex.Message}");
            }
        });
    }

    public void Stop()
    {
        _synth?.SpeakAsyncCancelAll();
    }

    public async Task<string> AskParotAsync(string prompt, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return "কিচিরমিচির! কিছু জানতে চাইলে আমাকে জিজ্ঞেস করো! 🦜";
        }

        // If Gemini API Key is provided, query Google Gemini API
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            try
            {
                var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = $"You are Parot, a cute, enthusiastic parakeet desktop companion. Respond warmly, concisely (under 2 sentences), in the language the user asked (Bengali or English), with bird flair (Squawk! / কিচিরমিচির!). User question: {prompt}" }
                            }
                        }
                    }
                };

                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(responseJson);
                    var text = doc.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();

                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        return text.Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Gemini API error: {ex.Message}");
            }
        }

        // Built-in offline witty bird knowledge base
        prompt = prompt.Trim().ToLowerInvariant();

        if (prompt.Contains("কেমন") || prompt.Contains("how are you"))
            return "কিচিরমিচির! ডানা ঝাপটে একদম দারুণ আছি! তুমি কেমন আছো? ✨";

        if (prompt.Contains("সময়") || prompt.Contains("time"))
            return $"এখন সময় হলো {DateTime.Now:hh:mm tt}! কাজ শেষ করার দারুণ সময়! ⏰";

        if (prompt.Contains("তারিখ") || prompt.Contains("date"))
            return $"আজকের তারিখ {DateTime.Now:dd MMMM, yyyy}! দিনটি সফল হোক! 📅";

        if (prompt.Contains("পানি") || prompt.Contains("water"))
            return "একদম ঠিক কথা! এক গ্লাস ঠান্ডা পানি খেয়ে শরীর রিফ্রেশ করে নাও! 💧";

        if (prompt.Contains("বিরতি") || prompt.Contains("break"))
            return "চোখ বন্ধ করে গভীর শ্বাস নাও অথবা ২০ সেকেন্ড দূরের দিকে তাকাও! 👁️✨";

        if (prompt.Contains("গান") || prompt.Contains("music") || prompt.Contains("sing"))
            return "কুহু কুহু কিচিরমিচির! 🎶🎶🎶 আমার গানের গলা কেমন বলো? 🦜";

        if (prompt.Contains("কৌতুক") || prompt.Contains("joke"))
            return "টিয়া পাখির সবচেয়ে প্রিয় সোশ্যাল মিডিয়া কোনটা জানো? টুইট-টার (Twitter/X)! 🤣";

        return $"Squawk! '{prompt}' নিয়ে ভাবছি... তুমি মন দিয়ে কাজ করো, আমি পাশে আছি! 🚀";
    }

    public void Dispose()
    {
        _synth?.Dispose();
        _synth = null;
    }
}
