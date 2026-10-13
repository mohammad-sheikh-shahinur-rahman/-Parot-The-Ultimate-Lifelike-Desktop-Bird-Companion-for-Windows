using System;
using System.Collections.Generic;

namespace Parot.Services;

/// <summary>
/// Comprehensive localization service providing multi-language UI, dialogues, and notifications.
/// Supports Bengali, English, Hindi, Spanish, and Bilingual modes.
/// </summary>
public static class LocalizationService
{
    public static readonly string[] SupportedLanguages =
    [
        "Bilingual",
        "Bengali",
        "English",
        "Hindi",
        "Spanish"
    ];

    public static string NormalizeLanguage(string? lang)
    {
        return lang switch
        {
            "Bengali" or "বাংলা" => "Bengali",
            "English" or "ইংরেজি" => "English",
            "Hindi" or "हिंदी" => "Hindi",
            "Spanish" or "Español" => "Spanish",
            _ => "Bilingual"
        };
    }

    public static string GetLanguageDisplayName(string lang)
    {
        return lang switch
        {
            "Bengali" => "🇧🇩 বাংলা (Bengali)",
            "English" => "🇬🇧 English",
            "Hindi" => "🇮🇳 हिंदी (Hindi)",
            "Spanish" => "🇪🇸 Español (Spanish)",
            _ => "🌐 দ্বিভাষিক (Bilingual)"
        };
    }

    // Dialogue and UI string dictionaries
    private static readonly Dictionary<string, Dictionary<string, string>> Strings = new(StringComparer.OrdinalIgnoreCase)
    {
        // Sound toggles
        ["SoundOn"] = new()
        {
            ["Bengali"] = "🔊 সাউন্ড: চালু",
            ["English"] = "🔊 Sound: ON",
            ["Hindi"] = "🔊 आवाज़: चालू (Sound: ON)",
            ["Spanish"] = "🔊 Sonido: ACTIVADO",
            ["Bilingual"] = "🔊 সাউন্ড: চালু (Sound: ON)"
        },
        ["SoundOff"] = new()
        {
            ["Bengali"] = "🔇 সাউন্ড: বন্ধ (মিউট)",
            ["English"] = "🔇 Sound: OFF (Muted)",
            ["Hindi"] = "🔇 आवाज़: बंद (Muted)",
            ["Spanish"] = "🔇 Sonido: SILENCIADO",
            ["Bilingual"] = "🔇 সাউন্ড: বন্ধ (Sound: OFF)"
        },
        ["SoundOnBubble"] = new()
        {
            ["Bengali"] = "🔊 সাউন্ড চালু করা হয়েছে! কিচিরমিচির!",
            ["English"] = "🔊 Sound enabled! Chirp chirp!",
            ["Hindi"] = "🔊 आवाज़ चालू कर दी गई है! चीं-चीं!",
            ["Spanish"] = "🔊 ¡Sonido activado! ¡Pío pío!",
            ["Bilingual"] = "🔊 সাউন্ড চালু করা হয়েছে! (Sound is ON)"
        },
        ["SoundOffBubble"] = new()
        {
            ["Bengali"] = "🔇 সাউন্ড সম্পূর্ণ বন্ধ (Muted)। কোনো শব্দ হবে না!",
            ["English"] = "🔇 Sound completely muted. Absolute silence!",
            ["Hindi"] = "🔇 आवाज़ पूरी तरह बंद (Muted)। कोई आवाज़ नहीं होगी!",
            ["Spanish"] = "🔇 Sonido completamente silenciado. ¡Silencio absoluto!",
            ["Bilingual"] = "🔇 সাউন্ড সম্পূর্ণ বন্ধ (Muted)। Absolute silence!"
        },
        ["SoundQuick_Mute"] = new()
        {
            ["Bengali"] = "🔊 সাউন্ড বন্ধ করো (Mute Sound)",
            ["English"] = "🔊 Mute Sound",
            ["Hindi"] = "🔊 आवाज़ बंद करें (Mute Sound)",
            ["Spanish"] = "🔊 Silenciar Sonido",
            ["Bilingual"] = "🔊 সাউন্ড বন্ধ করো (Mute Sound)"
        },
        ["SoundQuick_Unmute"] = new()
        {
            ["Bengali"] = "🔇 সাউন্ড চালু করো (Unmute Sound)",
            ["English"] = "🔇 Unmute Sound",
            ["Hindi"] = "🔇 आवाज़ चालू करें (Unmute Sound)",
            ["Spanish"] = "🔇 Activar Sonido",
            ["Bilingual"] = "🔇 সাউন্ড চালু করো (Unmute Sound)"
        },

        // Birdcage Mode
        ["Birdcage_PutIn"] = new()
        {
            ["Bengali"] = "🏰 খাঁচাবন্দি করো (Put in Birdcage)",
            ["English"] = "🏰 Put in Birdcage",
            ["Hindi"] = "🏰 पिंजरे में रखें (Put in Birdcage)",
            ["Spanish"] = "🏰 Poner en Jaula (Birdcage)",
            ["Bilingual"] = "🏰 খাঁচাবন্দি করো (Put in Birdcage)"
        },
        ["Birdcage_Release"] = new()
        {
            ["Bengali"] = "🔓 খাঁচা থেকে মুক্ত করো (Release from Cage)",
            ["English"] = "🔓 Release from Cage",
            ["Hindi"] = "🔓 पिंजरे से मुक्त करें (Release from Cage)",
            ["Spanish"] = "🔓 Liberar de la Jaula",
            ["Bilingual"] = "🔓 খাঁচা থেকে মুক্ত করো (Release from Cage)"
        },
        ["Birdcage_CagedBubble"] = new()
        {
            ["Bengali"] = "পাখিটি এখন সুন্দর খাঁচায় সুরক্ষিত আছে! 🏰🦜 শান্তভাবে থাকবে ও কাজে বাধা দেবে না। মাউস দিয়ে খাঁচা যেকোনো জায়গায় সরাতে পারো!",
            ["English"] = "Parot is now safe and cozy inside the golden cage! 🏰🦜 Won't interrupt your work. Drag the cage anywhere you like!",
            ["Hindi"] = "तोता अब सुनहरे पिंजरे में सुरक्षित है! 🏰🦜 काम में बाधा नहीं डालेगा। माउस से कहीं भी रख सकते हैं!",
            ["Spanish"] = "¡El loro está seguro en su jaula dorada! 🏰🦜 No interrumpirá tu trabajo. ¡Arrástralo a donde quieras!",
            ["Bilingual"] = "পাখিটি এখন সুন্দর খাঁচায় সুরক্ষিত! 🏰🦜 (Safe inside Birdcage!)"
        },
        ["Birdcage_FreeBubble"] = new()
        {
            ["Bengali"] = "খাঁচা থেকে মুক্ত! এখন আবার স্বাধীনভাবে উড়বে ও ঘুরে বেড়াবে! 🕊️✨",
            ["English"] = "Released from the cage! Free to fly and roam across your screen! 🕊️✨",
            ["Hindi"] = "पिंजरे से आज़ाद! अब स्क्रीन पर खुलेआम उड़ेगा! 🕊️✨",
            ["Spanish"] = "¡Liberado de la jaula! ¡Libre para volar por tu pantalla! 🕊️✨",
            ["Bilingual"] = "খাঁচা থেকে মুক্ত! 🕊️✨ (Free to fly and roam!)"
        },
        ["Birdcage_PlacedBubble"] = new()
        {
            ["Bengali"] = "খাঁচাটি এই স্থানে সুন্দরভাবে স্থাপন করা হলো! 🏰✨",
            ["English"] = "The birdcage has been placed here nicely! 🏰✨",
            ["Hindi"] = "पिंजरा यहाँ सुंदरता से स्थापित किया गया है! 🏰✨",
            ["Spanish"] = "¡La jaula ha sido colocada aquí con éxito! 🏰✨",
            ["Bilingual"] = "খাঁচাটি এই স্থানে সুন্দরভাবে স্থাপন করা হলো! 🏰✨ (Placed)"
        },

        // Auto-Startup
        ["AutoStartup"] = new()
        {
            ["Bengali"] = "🚀 উইন্ডোজ চালুর সাথে অটো স্টার্ট",
            ["English"] = "🚀 Start with Windows",
            ["Hindi"] = "🚀 विंडोज़ शुरू होते ही ऑटो स्टार्ट",
            ["Spanish"] = "🚀 Iniciar con Windows",
            ["Bilingual"] = "🚀 উইন্ডোজ চালু হলে অটো স্টার্ট (Start with Windows)"
        },
        ["AutoStartupOnBubble"] = new()
        {
            ["Bengali"] = "🚀 অটো স্টার্ট চালু! পিসি অন হলেই আমি চলে আসব!",
            ["English"] = "🚀 Auto-startup enabled! I'll fly in whenever your PC boots!",
            ["Hindi"] = "🚀 ऑटो-स्टार्ट चालू! पीसी शुरू होते ही मैं आ जाऊंगा!",
            ["Spanish"] = "🚀 ¡Inicio automático activado! ¡Estaré aquí al encender tu PC!",
            ["Bilingual"] = "🚀 অটো স্টার্ট চালু! (Start with Windows Enabled!)"
        },
        ["AutoStartupOffBubble"] = new()
        {
            ["Bengali"] = "অটো স্টার্ট বন্ধ করা হয়েছে।",
            ["English"] = "Auto-startup disabled.",
            ["Hindi"] = "ऑटो-स्टार्ट बंद कर दिया गया है।",
            ["Spanish"] = "Inicio automático desactivado.",
            ["Bilingual"] = "অটো স্টার্ট বন্ধ (Auto-Startup Disabled)"
        },

        // Language changed
        ["LanguageChangedBubble"] = new()
        {
            ["Bengali"] = "🌐 ভাষা সফলভাবে পরিবর্তন করা হয়েছে: বাংলা 🇧🇩",
            ["English"] = "🌐 Language successfully updated: English 🇬🇧",
            ["Hindi"] = "🌐 भाषा सफलतापूर्वक बदल दी गई: हिंदी 🇮🇳",
            ["Spanish"] = "🌐 Idioma actualizado con éxito: Español 🇪🇸",
            ["Bilingual"] = "🌐 Language updated: দ্বিভাষিক (Bilingual) 🌐"
        },

        // Menus
        ["LanguageMenu"] = new()
        {
            ["Bengali"] = "🌐 ভাষা নির্বাচন (Select Language)",
            ["English"] = "🌐 Language (Select Language)",
            ["Hindi"] = "🌐 भाषा चुनें (Select Language)",
            ["Spanish"] = "🌐 Idioma (Select Language)",
            ["Bilingual"] = "🌐 ভাষা নির্বাচন (Language)"
        },
        ["MiniGamesMenu"] = new()
        {
            ["Bengali"] = "🎯 খেলাধুলো ও ট্রিকস",
            ["English"] = "🎯 Mini-Games & Toys",
            ["Hindi"] = "🎯 खेल और ट्रिक्स",
            ["Spanish"] = "🎯 Juegos y Juguetes",
            ["Bilingual"] = "🎯 খেলাধুলো ও ট্রিকস (Mini-Games & Toys)"
        },
        ["AvianBehaviorsMenu"] = new()
        {
            ["Bengali"] = "🕊️ পাখির স্বাভাবিক আচরণ",
            ["English"] = "🕊️ Avian Behaviors",
            ["Hindi"] = "🕊️ पक्षी का प्राकृतिक व्यवहार",
            ["Spanish"] = "🕊️ Comportamientos Naturales",
            ["Bilingual"] = "🕊️ পাখির স্বাভাবিক আচরণ (Avian Behaviors)"
        },
        ["BirdSpeciesMenu"] = new()
        {
            ["Bengali"] = "🎨 পাখির প্রজাতি",
            ["English"] = "🎨 Bird Species",
            ["Hindi"] = "🎨 पक्षियों की प्रजातियां",
            ["Spanish"] = "🎨 Especies de Aves",
            ["Bilingual"] = "🎨 পাখির প্রজাতি (Bird Species)"
        },
        ["AccessoriesMenu"] = new()
        {
            ["Bengali"] = "👓 পোশাক ও সাজসজ্জা",
            ["English"] = "👓 Costumes & Accessories",
            ["Hindi"] = "👓 वेशभूषा और साज-सज्जा",
            ["Spanish"] = "👓 Disfraces y Accesorios",
            ["Bilingual"] = "👓 পোশাক ও সাজসজ্জা (Accessories)"
        },
        ["AudioMenu"] = new()
        {
            ["Bengali"] = "🎙️ ভয়েস ও সাউন্ড",
            ["English"] = "🎙️ Voice & Audio",
            ["Hindi"] = "🎙️ आवाज़ और ध्वनि",
            ["Spanish"] = "🎙️ Voz y Sonido",
            ["Bilingual"] = "🎙️ ভয়েস ও সাউন্ড (Voice & Audio)"
        },
        ["FeedingMenu"] = new()
        {
            ["Bengali"] = "🍽️ ক্ষুধা ও খাবার",
            ["English"] = "🍽️ Hunger & Feeding",
            ["Hindi"] = "🍽️ भूख और भोजन",
            ["Spanish"] = "🍽️ Hambre y Alimentación",
            ["Bilingual"] = "🍽️ ক্ষুধা ও খাবার (Hunger & Feeding)"
        },
        ["TakeFlight"] = new()
        {
            ["Bengali"] = "🚀 আকাশে ডানা মেলে ওড়ো",
            ["English"] = "🚀 Take Flight",
            ["Hindi"] = "🚀 आकाश में उड़ो",
            ["Spanish"] = "🚀 Alzar el Vuelo",
            ["Bilingual"] = "🚀 আকাশে ডানা মেলে ওড়ো (Take Flight)"
        },
        ["FollowCursor"] = new()
        {
            ["Bengali"] = "🎯 কার্সার অনুসরণ করো",
            ["English"] = "🎯 Follow Cursor",
            ["Hindi"] = "🎯 कर्सर का पीछा करो",
            ["Spanish"] = "🎯 Seguir el Cursor",
            ["Bilingual"] = "🎯 কার্সার অনুসরণ করো (Follow Cursor)"
        },
        ["PerchActiveWindow"] = new()
        {
            ["Bengali"] = "🪟 উইন্ডোর ওপরে বসো",
            ["English"] = "🪟 Perch on Active Window",
            ["Hindi"] = "🪟 सक्रिय विंडो पर बैठो",
            ["Spanish"] = "🪟 Posarse en Ventana Activa",
            ["Bilingual"] = "🪟 উইন্ডোর ওপরে বসো (Perch on Window)"
        },
        ["SleepWake_Sleep"] = new()
        {
            ["Bengali"] = "💤 ঘুমাতে যাও (Go to Sleep)",
            ["English"] = "💤 Go to Sleep",
            ["Hindi"] = "💤 सो जाओ (Go to Sleep)",
            ["Spanish"] = "💤 Ir a Dormir",
            ["Bilingual"] = "💤 ঘুমাতে যাও (Go to Sleep)"
        },
        ["SleepWake_Wake"] = new()
        {
            ["Bengali"] = "☀️ জেগে ওঠো (Wake Up)",
            ["English"] = "☀️ Wake Up",
            ["Hindi"] = "☀️ जाग जाओ (Wake Up)",
            ["Spanish"] = "☀️ Despertar",
            ["Bilingual"] = "☀️ জেগে ওঠো (Wake Up)"
        },
        ["SettingsMenu"] = new()
        {
            ["Bengali"] = "⚙️ সেটিংস...",
            ["English"] = "⚙️ Settings...",
            ["Hindi"] = "⚙️ सेटिंग्स...",
            ["Spanish"] = "⚙️ Configuración...",
            ["Bilingual"] = "⚙️ সেটিংস (Settings...)"
        },
        ["ExitMenu"] = new()
        {
            ["Bengali"] = "❌ বন্ধ করুন (Exit)",
            ["English"] = "❌ Exit Parot",
            ["Hindi"] = "❌ बंद करें (Exit)",
            ["Spanish"] = "❌ Salir de Parot",
            ["Bilingual"] = "❌ বন্ধ করুন (Exit Parot)"
        },

        // Behaviors Dialogues
        ["Preen"] = new()
        {
            ["Bengali"] = "ডানার পালকগুলো ঠোঁট দিয়ে খুঁটে সাজিয়ে নিচ্ছি... ✨",
            ["English"] = "Grooming and preening my feathers neatly... ✨",
            ["Hindi"] = "चोंच से अपने पंखों को संवार रहा हूँ... ✨",
            ["Spanish"] = "Acalicalando mis plumas suavemente... ✨",
            ["Bilingual"] = "ডানার পালকগুলো খুঁটে সাজিয়ে নিচ্ছি... ✨ (Preening Feathers)"
        },
        ["Stretch"] = new()
        {
            ["Bengali"] = "আহ্! এক ডানা ও এক পা পেছনে টানটান করে বার্ড ইয়োগা! 🧘",
            ["English"] = "Ah! Stretching out one wing and leg for bird yoga! 🧘",
            ["Hindi"] = "आह! एक पंख और पैर फैलाकर बर्ड योगा! 🧘",
            ["Spanish"] = "¡Ah! ¡Estirando ala y pata en yoga de aves! 🧘",
            ["Bilingual"] = "এক ডানা ও এক পা পেছনে টানটান করে বার্ড ইয়োগা! 🧘 (Yoga Stretch)"
        },
        ["Fluff"] = new()
        {
            ["Bengali"] = "পুরো শরীর কাঁপিয়ে গা ঝাড়া দিয়ে লোম ফুলালাম! 💨",
            ["English"] = "Fluffed up my feathers and shook my whole body! 💨",
            ["Hindi"] = "पूरे शरीर को हिलाकर पंख फड़फड़ाए! 💨",
            ["Spanish"] = "¡Espiné mis plumas y sacudí todo el cuerpo! 💨",
            ["Bilingual"] = "গা ঝাড়া দিয়ে লোম ফুলালাম! 💨 (Fluff & Shake)"
        },
        ["Yawn"] = new()
        {
            ["Bengali"] = "হাঁআআআ... অলস সময়ে মিষ্টি একটা হাই তুললাম! 🥱",
            ["English"] = "Yaaawn... Taking a sweet lazy yawn! 🥱",
            ["Hindi"] = "जम्हाई... फुर्सत के समय में एक प्यारी जम्हाई! 🥱",
            ["Spanish"] = "Bostezooo... ¡Un perezoso bostezo! 🥱",
            ["Bilingual"] = "হাঁআআআ... অলস সময়ে মিষ্টি হাই তুললাম! 🥱 (Yawn)"
        },
        ["Observing"] = new()
        {
            ["Bengali"] = "উইন্ডোর দিকে কৌতূহল নিয়ে মাথা কাত করে দেখছি! 🦜👀",
            ["English"] = "Curiously tilting my head to inspect your work! 🦜👀",
            ["Hindi"] = "उत्सुकता से सिर झुकाकर आपका काम देख रहा हूँ! 🦜👀",
            ["Spanish"] = "¡Inclinando curiosamente la cabeza para ver tu trabajo! 🦜👀",
            ["Bilingual"] = "উইন্ডোর দিকে কৌতূহল নিয়ে তাকিয়ে দেখছি! 🦜👀 (Observing Screen)"
        },
        ["HappyDance"] = new()
        {
            ["Bengali"] = "খুশিতে ডানা নাচিয়ে একটু নেচে নিলাম! 🎶💃",
            ["English"] = "Flapping and dancing happily to the rhythm! 🎶💃",
            ["Hindi"] = "खुशी में पंख हिलाकर थोड़ा नाच लिया! 🎶💃",
            ["Spanish"] = "¡Bailando alegremente al ritmo! 🎶💃",
            ["Bilingual"] = "খুশিতে ডানা নাচিয়ে নেচে নিলাম! 🎶💃 (Happy Dance)"
        },
        ["EdgePeeking"] = new()
        {
            ["Bengali"] = "উইন্ডোর কিনারায় উঁকি মারছি... নিচে কত গভীর! 🪟👀",
            ["English"] = "Peeking over the window edge... wow, look down! 🪟👀",
            ["Hindi"] = "विंडो के किनारे से नीचे झांक रहा हूँ... कितनी गहराई है! 🪟👀",
            ["Spanish"] = "¡Asomándome al borde de la ventana... mira abajo! 🪟👀",
            ["Bilingual"] = "উইন্ডোর কিনারায় উঁকি মারছি... 🪟👀 (Peeking Over Edge)"
        },
        ["WokeUp"] = new()
        {
            ["Bengali"] = "মিষ্টি একটা ঘুম দিয়ে উঠে পড়লাম! একদম চাঙ্গা! ☀️🥱",
            ["English"] = "Woke up from a sweet little power nap! Fully refreshed! ☀️🥱",
            ["Hindi"] = "एक मीठी नींद से जाग गया! एकदम तरोताजा! ☀️🥱",
            ["Spanish"] = "¡Desperté de una rica siesta! ¡Lleno de energía! ☀️🥱",
            ["Bilingual"] = "মিষ্টি একটা ঘুম দিয়ে উঠে পড়লাম! ☀️🥱 (Woke Up Refreshed!)"
        },
        ["ChargerConnected"] = new()
        {
            ["Bengali"] = "⚡ আহ্! চার্জার কানেক্ট হয়েছে! ফুল এনার্জি আসছে!",
            ["English"] = "⚡ Ah! Charger plugged in! Full energy pumping in!",
            ["Hindi"] = "⚡ आह! चार्जर कनेक्ट हो गया! पूरी ऊर्जा आ रही है!",
            ["Spanish"] = "⚡ ¡Cargador conectado! ¡Batería recargándose!",
            ["Bilingual"] = "⚡ চার্জার কানেক্ট হয়েছে! ফুল এনার্জি! (Charger Plugged In!)"
        },
        ["BatteryLow"] = new()
        {
            ["Bengali"] = "🥱 ব্যাটারি কমে এসেছে... আমাকে চার্জে দাও প্লিজ!",
            ["English"] = "🥱 Battery is running low... please plug in the charger!",
            ["Hindi"] = "🥱 बैटरी कम हो रही है... कृपया मुझे चार्ज पर लगाएं!",
            ["Spanish"] = "🥱 La batería está baja... ¡por favor conéctame al cargador!",
            ["Bilingual"] = "🥱 ব্যাটারি কমে এসেছে... (Battery Running Low)"
        }
    };

    public static string Get(string key, string? currentLang = null)
    {
        string lang = NormalizeLanguage(currentLang);
        if (Strings.TryGetValue(key, out var dict))
        {
            if (dict.TryGetValue(lang, out var val)) return val;
            if (dict.TryGetValue("Bilingual", out var biVal)) return biVal;
            if (dict.TryGetValue("English", out var enVal)) return enVal;
        }
        return key;
    }
}
