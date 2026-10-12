using System;
using System.Collections.Generic;

namespace Parot.Models;

public static class Dialogues
{
    private static readonly Random _random = new();

    private static readonly string[] IdleChatterBengali =
    [
        "কিচিরমিচির! আজ দিনটা কেমন যাচ্ছে? ✨",
        "মন দিয়ে কাজ করছো তো? আমি কিন্তু দেখছি! 👀",
        "টিয়া পাখি গান গায়, কাজ ফেলে বসে থেকো না ভাই! 🎶",
        "একটু সোজা হয়ে বসো, পিঠের যত্ন নাও! 🪑",
        "তুমি খুব ভালো কাজ করছো! চালিয়ে যাও! 🚀",
        "মাঝে মাঝে দীর্ঘ শ্বাস নাও, মন শান্ত থাকবে। 🌿",
        "আমি তোমার সাথে আছি, একা লাগলে আমাকে ক্লিক কোরো! 🦜",
        "আজকের কাজগুলো শেষ করে মিষ্টি ঘুম দিতে হবে! 🌙"
    ];

    private static readonly string[] IdleChatterEnglish =
    [
        "Chirp! How is your day going? ✨",
        "Focus on your goals, you've got this! 🚀",
        "Sitting upright? Don't forget your posture! 🪑",
        "Take a deep breath and relax your shoulders. 🌿",
        "I'm keeping you company! Click me if you want to play! 🦜",
        "Squawk! Great progress today!",
        "Every small step counts towards your success! ⭐",
        "Happy humming! Keep up the good work! 🎶"
    ];

    private static readonly string[] EyeBreakBengali =
    [
        "চোখকে একটু বিশ্রাম দাও! ২০ সেকেন্ড দূরের কোনো বস্তুর দিকে তাকাও (২০-২০-২০ নিয়ম) 👁️✨",
        "অনেকক্ষণ একনাগাড়ে স্ক্রিনে তাকিয়ে আছো! চোখ দুটোকে একটু আরাম দাও 🌿",
        "স্ক্রিন থেকে চোখ সরিয়ে একটু পলক ফেলো, চোখের ক্লান্তি দূর হবে! 👀"
    ];

    private static readonly string[] EyeBreakEnglish =
    [
        "Rest your eyes! Look at something 20 feet away for 20 seconds (20-20-20 Rule) 👁️✨",
        "Screen break time! Give your lovely eyes a gentle rest 🌿",
        "Blink gently and look out the window for a moment! 👀"
    ];

    private static readonly string[] HydrationBengali =
    [
        "একটু পানি খেয়ে নাও! শরীর ও মস্তিষ্ক সতেজ থাকবে 💧",
        "হাইড্রেটেড থাকা খুব দরকার! এক চুমুক পানি পান করো 🥤✨",
        "পানি খাওয়ার সময় হয়েছে! শরীর শুকিয়ে যেতে দিও না 🌊"
    ];

    private static readonly string[] HydrationEnglish =
    [
        "Time for some water! Stay refreshed and energized 💧",
        "Hydration check! Grab a sip of clean water now 🥤✨",
        "Water fuels your brain! Take a quick sip 🌊"
    ];

    private static readonly string[] PostureBengali =
    [
        "পিঠ সোজা করো এবং ডানা (হাত) দুটো একটু প্রসারিত করো! 🧘",
        "একটানা বসে থেকো না, উঠে একটু হেঁটে আসো! 🚶",
        "ঘাড় আর কাঁধ একটু ঘুরিয়ে নাও, টান কমে যাবে 🌿"
    ];

    private static readonly string[] PostureEnglish =
    [
        "Straighten your back and stretch your shoulders! 🧘",
        "Stand up and take a quick 1-minute walk around! 🚶",
        "Gently roll your neck and stretch those muscles 🌿"
    ];

    private static readonly string[] TreatsBengali =
    [
        "ইয়াম্মি! ঝাল কাঁচালঙ্কাটা দারুণ ছিল! 🌶️😋",
        "মচমচে সূর্যমুখীর বীজ! অনেক ধন্যবাদ! 🌻✨",
        "পেট ভরে গেল! এখন আবার পাহারা দেওয়ার পালা! 🦜❤️"
    ];

    private static readonly string[] TreatsEnglish =
    [
        "Yummy! Parrots love fresh spicy peppers! 🌶️😋",
        "Crunchy sunflower seeds! Thank you, friend! 🌻✨",
        "Mmm delicious! That made my day brighter! 🦜❤️"
    ];

    private static readonly string[] WakeUpBengali =
    [
        "উঁহু! ঘুম ভেঙে গেল! চলো আবার কাজে ফিরি! ☀️",
        "কিচিরমিচির! আমি প্রস্তুত! 🦜",
        "ডানা ঝাপটে জেগে উঠলাম! আজ কী করব? ✨"
    ];

    private static readonly string[] WakeUpEnglish =
    [
        "Squawk! Waking up with a stretch! ☀️",
        "Chirp! Ready to keep you company again! 🦜",
        "Good to see you! Let's get things done! ✨"
    ];

    public static string GetIdleDialogue(string language)
    {
        return Pick(language, IdleChatterBengali, IdleChatterEnglish);
    }

    public static string GetEyeBreakDialogue(string language)
    {
        return Pick(language, EyeBreakBengali, EyeBreakEnglish);
    }

    public static string GetHydrationDialogue(string language)
    {
        return Pick(language, HydrationBengali, HydrationEnglish);
    }

    public static string GetPostureDialogue(string language)
    {
        return Pick(language, PostureBengali, PostureEnglish);
    }

    public static string GetTreatDialogue(string language)
    {
        return Pick(language, TreatsBengali, TreatsEnglish);
    }

    public static string GetWakeUpDialogue(string language)
    {
        return Pick(language, WakeUpBengali, WakeUpEnglish);
    }

    private static string Pick(string language, string[] bengali, string[] english)
    {
        if (language == "Bengali")
        {
            return bengali[_random.Next(bengali.Length)];
        }
        if (language == "English")
        {
            return english[_random.Next(english.Length)];
        }

        // Bilingual: Randomly mix Bengali and English
        var pickBengali = _random.Next(2) == 0;
        return pickBengali
            ? bengali[_random.Next(bengali.Length)]
            : english[_random.Next(english.Length)];
    }
}
