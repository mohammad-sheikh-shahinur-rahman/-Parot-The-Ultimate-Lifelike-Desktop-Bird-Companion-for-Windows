using System;
using Parot.Models;

namespace Parot.Services;

public class MoodService
{
    private PetMood _currentMood = PetMood.Cheerful;
    private readonly Random _random = new();

    public PetMood CurrentMood
    {
        get => _currentMood;
        private set
        {
            if (_currentMood != value)
            {
                var old = _currentMood;
                _currentMood = value;
                MoodChanged?.Invoke(old, value);
            }
        }
    }

    public event Action<PetMood, PetMood>? MoodChanged;

    public void SetMood(PetMood mood)
    {
        CurrentMood = mood;
    }

    public void RegisterInteraction()
    {
        if (CurrentMood == PetMood.Needy || CurrentMood == PetMood.Sleepy)
        {
            CurrentMood = PetMood.Cheerful;
        }
    }

    public void RegisterCuriosity()
    {
        if (CurrentMood != PetMood.Sleepy)
        {
            CurrentMood = PetMood.Curious;
        }
    }

    public void UpdateMoodFromInactivity(TimeSpan inactivityTime, bool isNight)
    {
        if (isNight)
        {
            CurrentMood = PetMood.Sleepy;
            return;
        }

        if (inactivityTime.TotalMinutes >= 8)
        {
            CurrentMood = PetMood.Needy;
        }
        else if (inactivityTime.TotalMinutes >= 4)
        {
            if (CurrentMood == PetMood.Cheerful)
            {
                CurrentMood = PetMood.Curious;
            }
        }
    }

    public string GetMoodDialogue(string language)
    {
        return CurrentMood switch
        {
            PetMood.Cheerful => language switch
            {
                "Bengali" => "আজ মনটা খুব ভালো! ডানা মেলে শিস বাজাচ্ছি! 🌟🎶",
                "English" => "I'm feeling so cheerful and happy today! 🌟🎶",
                _ => "আজ দারুণ প্রফুল্ল লাগছে! কিচিরমিচির! 🌟🎶 / Cheerful mood!"
            },
            PetMood.Curious => language switch
            {
                "Bengali" => "তুমি কী করছো? আমি মন দিয়ে গভীর চোখে দেখছি! 🧐✨",
                "English" => "Watching everything you do with keen curiosity! 🧐✨",
                _ => "কৌতূহলী চোখে তোমার কাজ দেখছি! 🧐✨"
            },
            PetMood.Needy => language switch
            {
                "Bengali" => "আমাকে কি ভুলে গেলে? 🥺 একটু আদর করো বা লঙ্কা খাওয়াও!",
                "English" => "Did you forget about me? 🥺 Please pet me or give me a treat!",
                _ => "একটু আদর বা খাবার চাই! অভিমান করছি কিন্তু! 🥺❤️"
            },
            PetMood.Sleepy => language switch
            {
                "Bengali" => "উফ্, চোখ বুজে আসছে... একটু ঝিমিয়ে নেই! 🥱💤",
                "English" => "So drowsy... feeling sleepy and peaceful! 🥱💤",
                _ => "চোখে ঘুম জড়িয়ে আসছে... 🥱💤"
            },
            _ => "কিচিরমিচির! 🦜"
        };
    }

    public string GetMoodTitle(string? language = null)
    {
        string key = CurrentMood switch
        {
            PetMood.Cheerful => "MenuMood_Cheerful",
            PetMood.Curious => "MenuMood_Curious",
            PetMood.Needy => "MenuMood_Needy",
            PetMood.Sleepy => "MenuMood_Sleepy",
            _ => "MenuMood_Normal"
        };
        return LocalizationService.Get(key, language);
    }
}
