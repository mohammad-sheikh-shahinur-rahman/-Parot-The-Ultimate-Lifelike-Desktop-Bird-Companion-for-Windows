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
        ["PerchDesktop"] = new()
        {
            ["Bengali"] = "🖥️ ডেস্কটপে ফিরে এসো",
            ["English"] = "🖥️ Perch on Desktop & Taskbar",
            ["Hindi"] = "🖥️ डेस्कटॉप पर लौटें",
            ["Spanish"] = "🖥️ Posarse en el Escritorio",
            ["Bilingual"] = "🖥️ ডেস্কটপে ফিরে এসো (Perch on Desktop)"
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
        },

        // Mood and Focus
        ["MenuMood_Cheerful"] = new() { ["English"] = "🎭 Mood: 🌟 Cheerful", ["Bengali"] = "🎭 মেজাজ: 🌟 প্রফুল্ল", ["Hindi"] = "🎭 मूड: 🌟 खुशमिज़ाज", ["Spanish"] = "🎭 Estado: 🌟 Alegre", ["Bilingual"] = "🎭 মেজাজ: 🌟 প্রফুল্ল (Mood: Cheerful)" },
        ["MenuMood_Curious"] = new() { ["English"] = "🎭 Mood: 🧐 Curious", ["Bengali"] = "🎭 মেজাজ: 🧐 কৌতূহলী", ["Hindi"] = "🎭 मूड: 🧐 उत्सुक", ["Spanish"] = "🎭 Estado: 🧐 Curioso", ["Bilingual"] = "🎭 মেজাজ: 🧐 কৌতূহলী (Mood: Curious)" },
        ["MenuMood_Needy"] = new() { ["English"] = "🎭 Mood: 🥺 Affection-seeking", ["Bengali"] = "🎭 মেজাজ: 🥺 আদরপ্রত্যাশী", ["Hindi"] = "🎭 मूड: 🥺 लाड़-प्यार चाहने वाला", ["Spanish"] = "🎭 Estado: 🥺 Cariñoso", ["Bilingual"] = "🎭 মেজাজ: 🥺 আদরপ্রত্যাশী (Needy)" },
        ["MenuMood_Sleepy"] = new() { ["English"] = "🎭 Mood: 💤 Sleepy", ["Bengali"] = "🎭 মেজাজ: 💤 অলস/ঘুমন্ত", ["Hindi"] = "🎭 मूड: 💤 नींद में", ["Spanish"] = "🎭 Estado: 💤 Soñoliento", ["Bilingual"] = "🎭 মেজাজ: 💤 ঘুমন্ত (Mood: Sleepy)" },
        ["MenuMood_Normal"] = new() { ["English"] = "🎭 Mood: 🦜 Content", ["Bengali"] = "🎭 মেজাজ: 🦜 স্বাভাবিক", ["Hindi"] = "🎭 मूड: 🦜 सामान्य", ["Spanish"] = "🎭 Estado: 🦜 Tranquilo", ["Bilingual"] = "🎭 মেজাজ: 🦜 স্বাভাবিক (Content)" },

        ["MenuPomodoro_Start"] = new() { ["English"] = "🍅 Start Pomodoro Focus (25m)", ["Bengali"] = "🍅 পোমোডোরো শুরু করুন", ["Hindi"] = "🍅 पोमोडोरो शुरू करें", ["Spanish"] = "🍅 Iniciar Enfoque Pomodoro", ["Bilingual"] = "🍅 পোমোডোরো শুরু করুন (Start Focus)" },
        ["MenuPomodoro_Stop"] = new() { ["English"] = "⏹️ Stop Pomodoro Focus", ["Bengali"] = "⏹️ পোমোডোরো বন্ধ করুন", ["Hindi"] = "⏹️ पोमोडोरो रोकें", ["Spanish"] = "⏹️ Detener Pomodoro", ["Bilingual"] = "⏹️ পোমোডোরো বন্ধ করুন (Stop Focus)" },
        ["MenuAskParot"] = new() { ["English"] = "✨ Ask Parot (AI Buddy Chat)", ["Bengali"] = "✨ পারটকে জিজ্ঞাসা করুন (এআই চ্যাট)", ["Hindi"] = "✨ पारट से पूछें (एआई चैट)", ["Spanish"] = "✨ Preguntar a Parot (Chat IA)", ["Bilingual"] = "✨ Ask Parot (এআই বাডি চ্যাট)" },
        ["MenuDailyReport"] = new() { ["English"] = "📊 Daily Wellness Report", ["Bengali"] = "📊 দৈনিক রিপোর্ট", ["Hindi"] = "📊 दैनिक रिपोर्ट", ["Spanish"] = "📊 Reporte Diario", ["Bilingual"] = "📊 দৈনিক রিপোর্ট (Daily Report)" },

        // Mini-Games subitems
        ["MenuLaserPointer"] = new() { ["English"] = "🔴 Laser Pointer Toy", ["Bengali"] = "🔴 লেজার পয়েন্টার খেলনা", ["Hindi"] = "🔴 लेज़र पॉइंटर खिलौना", ["Spanish"] = "🔴 Puntero Láser de Juguete", ["Bilingual"] = "🔴 লেজার পয়েন্টার খেলনা (Laser Pointer)" },
        ["MenuDesktopMirror"] = new() { ["English"] = "🪞 Desktop Mirror Trick", ["Bengali"] = "🪞 ডেস্কটপ আয়না", ["Hindi"] = "🪞 डेस्कटॉप आईना", ["Spanish"] = "🪞 Espejo de Escritorio", ["Bilingual"] = "🪞 ডেস্কটপ আয়না (Desktop Mirror)" },
        ["MenuThrowChili"] = new() { ["English"] = "🌶️ Throw Chili Treat", ["Bengali"] = "🌶️ লঙ্কা ছুড়ে দাও", ["Hindi"] = "🌶️ मिर्च फेंकें", ["Spanish"] = "🌶️ Lanzar Ají", ["Bilingual"] = "🌶️ লঙ্কা ছুড়ে দাও (Throw Chili Treat)" },
        ["MenuThrowPeanut"] = new() { ["English"] = "🥜 Throw Peanut Treat", ["Bengali"] = "🥜 বাদাম ছুড়ে দাও", ["Hindi"] = "🥜 मूंगफली फेंकें", ["Spanish"] = "🥜 Lanzar Cacahuate", ["Bilingual"] = "🥜 বাদাম ছুড়ে দাও (Throw Peanut Treat)" },
        ["MenuThrowSeed"] = new() { ["English"] = "🌻 Throw Sunflower Seeds", ["Bengali"] = "🌻 সূর্যমুখী বীজ ছুড়ে দাও", ["Hindi"] = "🌻 सूरजमुखी बीज फेंकें", ["Spanish"] = "🌻 Lanzar Semillas de Girasol", ["Bilingual"] = "🌻 সূর্যমুখী বীজ ছুড়ে দাও (Throw Seeds)" },
        ["MenuPerchCursor"] = new() { ["English"] = "🎯 Perch on Mouse Cursor", ["Bengali"] = "🎯 কার্সারের পিঠে চড়ো", ["Hindi"] = "🎯 कर्सर पर बैठो", ["Spanish"] = "🎯 Posarse en el Cursor", ["Bilingual"] = "🎯 কার্সারের পিঠে চড়ো (Perch on Cursor)" },
        ["MenuBallPush"] = new() { ["English"] = "⚽ Bouncy Ball Push Game", ["Bengali"] = "⚽ বল ঠেলার খেলা", ["Hindi"] = "⚽ गेंद धकेलने का खेल", ["Spanish"] = "⚽ Juego de Empujar la Pelota", ["Bilingual"] = "⚽ বল ঠেলার খেলা (Ball Push Game)" },
        ["MenuPlayTag"] = new() { ["English"] = "🎮 Play Tag with Cursor", ["Bengali"] = "🎮 মাউস নিয়ে খেলা", ["Hindi"] = "🎮 कर्सर के साथ खेलें", ["Spanish"] = "🎮 Jugar a las Traes con el Cursor", ["Bilingual"] = "🎮 মাউস নিয়ে খেলা (Play Tag)" },

        // Avian Behaviors subitems
        ["MenuWalkTitlebar"] = new() { ["English"] = "🚶 Walk on Titlebar", ["Bengali"] = "🚶 টাইটেলবারে হেঁটে বেড়াও", ["Hindi"] = "🚶 टाइटल बार पर टहलो", ["Spanish"] = "🚶 Caminar en Barra de Título", ["Bilingual"] = "🚶 টাইটেলবারে হেঁটে বেড়াও (Walk on Titlebar)" },
        ["MenuFlapPerch"] = new() { ["English"] = "🪶 Flap Wings on Perch", ["Bengali"] = "🪶 বসে ডানা ঝাপটাও", ["Hindi"] = "🪶 बैठे-बैठे पंख फड़फड़ाओ", ["Spanish"] = "🪶 Aletear en la Perca", ["Bilingual"] = "🪶 বসে ডানা ঝাপটাও (Flap Wings on Perch)" },
        ["MenuCuriousLook"] = new() { ["English"] = "👀 Curious Look Around", ["Bengali"] = "👀 মাথা ঘুরিয়ে তাকাও", ["Hindi"] = "👀 उत्सुकता से चारों ओर देखो", ["Spanish"] = "👀 Mirar con Curiosidad", ["Bilingual"] = "👀 মাথা ঘুরিয়ে তাকাও (Curious Look Around)" },
        ["MenuPreenFeathers"] = new() { ["English"] = "✨ Preen Feathers", ["Bengali"] = "✨ পালক আঁচড়াও", ["Hindi"] = "✨ पंख संवारो", ["Spanish"] = "✨ Acalicalar Plumas", ["Bilingual"] = "✨ পালক আঁচড়াও (Preen Feathers)" },
        ["MenuStretchWings"] = new() { ["English"] = "🧘 Wing & Leg Stretch", ["Bengali"] = "🧘 ডানা ও পা স্ট্রেচ করো", ["Hindi"] = "🧘 पंख और पैर फैलाओ", ["Spanish"] = "🧘 Estiramiento de Ala y Pata", ["Bilingual"] = "🧘 ডানা ও পা স্ট্রেচ করো (Stretch)" },
        ["MenuFluffFeathers"] = new() { ["English"] = "💨 Fluff & Shake Feathers", ["Bengali"] = "💨 গা ঝাড়া দাও", ["Hindi"] = "💨 पंख फुलाओ और हिलाओ", ["Spanish"] = "💨 Sacudir Plumas", ["Bilingual"] = "💨 গা ঝাড়া দাও (Fluff & Shake)" },
        ["MenuLazyYawn"] = new() { ["English"] = "🥱 Lazy Yawn", ["Bengali"] = "🥱 হাই তোলো", ["Hindi"] = "🥱 जम्हाई लो", ["Spanish"] = "🥱 Bostezo Perezoso", ["Bilingual"] = "🥱 হাই তোলো (Yawn)" },
        ["MenuDizzyTrick"] = new() { ["English"] = "😵💫 Make Dizzy Trick", ["Bengali"] = "😵💫 ডিজি ট্রিক", ["Hindi"] = "😵💫 चक्कर आने की ट्रिक", ["Spanish"] = "😵💫 Truco del Mareo", ["Bilingual"] = "😵💫 ডিজি ট্রিক (Make Dizzy)" },

        // Species subitems
        ["MenuSkin_Scarlet"] = new() { ["English"] = "🔴 Scarlet Macaw", ["Bengali"] = "🔴 স্কারলেট ম্যাকাও", ["Hindi"] = "🔴 लाल मकाऊ", ["Spanish"] = "🔴 Guacamayo Escarlata", ["Bilingual"] = "🔴 Scarlet Macaw (লাল ম্যাকাও)" },
        ["MenuSkin_Parakeet"] = new() { ["English"] = "🟢 Green Parakeet", ["Bengali"] = "🟢 সবুজ দেশি টিয়া", ["Hindi"] = "🟢 हरा तोता", ["Spanish"] = "🟢 Periquito Verde", ["Bilingual"] = "🟢 Green Parakeet (দেশি টিয়া)" },
        ["MenuSkin_BlueMacaw"] = new() { ["English"] = "🔵 Blue & Gold Macaw", ["Bengali"] = "🔵 নীল-হলুদ ম্যাকাও", ["Hindi"] = "🔵 नीला और सुनहरा मकाऊ", ["Spanish"] = "🔵 Guacamayo Azul y Amarillo", ["Bilingual"] = "🔵 Blue & Gold Macaw (নীল ম্যাকাও)" },
        ["MenuSkin_Cockatiel"] = new() { ["English"] = "🟡 Cockatiel", ["Bengali"] = "🟡 হলুদ ককাটেল", ["Hindi"] = "🟡 कॉकटेल", ["Spanish"] = "🟡 Ninfa / Cockatiel", ["Bilingual"] = "🟡 Cockatiel (হলুদ ককাটেল)" },
        ["MenuSkin_Cockatoo"] = new() { ["English"] = "⚪ White Cockatoo", ["Bengali"] = "⚪ সাদা কাকাতুয়া", ["Hindi"] = "⚪ सफेद काकातुआ", ["Spanish"] = "⚪ Cacatúa Blanca", ["Bilingual"] = "⚪ White Cockatoo (সাদা কাকাতুয়া)" },

        // Accessories subitems
        ["MenuAcc_Sunglasses"] = new() { ["English"] = "🕶️ Cool Sunglasses", ["Bengali"] = "🕶️ সানগ্লাস", ["Hindi"] = "🕶️ धूप का चश्मा", ["Spanish"] = "🕶️ Gafas de Sol", ["Bilingual"] = "🕶️ সানগ্লাস (Cool Sunglasses)" },
        ["MenuAcc_Glasses"] = new() { ["English"] = "👓 Reading Glasses", ["Bengali"] = "👓 রিডিং চশমা", ["Hindi"] = "👓 पढ़ने का चश्मा", ["Spanish"] = "👓 Gafas de Lectura", ["Bilingual"] = "👓 রিডিং চশমা (Reading Glasses)" },
        ["MenuAcc_Scarf"] = new() { ["English"] = "🧣 Winter Scarf", ["Bengali"] = "🧣 শীতের মাফলার", ["Hindi"] = "🧣 मफलर", ["Spanish"] = "🧣 Bufanda de Invierno", ["Bilingual"] = "🧣 শীতের মাফলার (Winter Scarf)" },
        ["MenuAcc_TopHat"] = new() { ["English"] = "🎩 Elegant Top Hat", ["Bengali"] = "🎩 অভিজাত হ্যাট", ["Hindi"] = "🎩 टॉप हैट", ["Spanish"] = "🎩 Sombrero de Copa", ["Bilingual"] = "🎩 অভিজাত হ্যাট (Top Hat)" },
        ["MenuAcc_PartyHat"] = new() { ["English"] = "🥳 Party Cone Hat", ["Bengali"] = "🥳 পার্টি হ্যাট", ["Hindi"] = "🥳 पार्टी टोपी", ["Spanish"] = "🥳 Gorro de Fiesta", ["Bilingual"] = "🥳 পার্টি হ্যাট (Party Hat)" },
        ["MenuAcc_Umbrella"] = new() { ["English"] = "☂️ Tiny Umbrella", ["Bengali"] = "☂️ ছাতা", ["Hindi"] = "☂️ छोटी छतरी", ["Spanish"] = "☂️ Paraguas Pequeño", ["Bilingual"] = "☂️ ছাতা (Umbrella)" },
        ["MenuAcc_None"] = new() { ["English"] = "🚫 Remove All Accessories", ["Bengali"] = "🚫 কোনো সাজসজ্জা নয়", ["Hindi"] = "🚫 कोई साज-सज्जा नहीं", ["Spanish"] = "🚫 Quitar Accesorios", ["Bilingual"] = "🚫 কোনো সাজসজ্জা নয় (Remove All)" },

        // Audio & Voice subitems
        ["MenuVoiceMimic"] = new() { ["English"] = "🎙️ Record & Echo Voice", ["Bengali"] = "🎙️ কথা শেখাও ও নকল শোনো", ["Hindi"] = "🎙️ शब्द सिखाएं और सुनें", ["Spanish"] = "🎙️ Grabar y Repetir Voz", ["Bilingual"] = "🎙️ কথা শেখাও ও নকল শোনো (Echo Voice)" },
        ["MenuVoiceCommands"] = new() { ["English"] = "🗣️ Voice Commands: Active", ["Bengali"] = "🗣️ ভয়েস কমান্ড: সক্রিয়", ["Hindi"] = "🗣️ वॉयस कमांड: सक्रिय", ["Spanish"] = "🗣️ Comandos de Voz: Activo", ["Bilingual"] = "🗣️ ভয়েস কমান্ড (Voice Commands)" },
        ["MenuSquawk"] = new() { ["English"] = "💬 Squawk Loudly!", ["Bengali"] = "💬 ডাক ছাড়ো (Squawk!)", ["Hindi"] = "💬 ज़ोर से चीखो!", ["Spanish"] = "💬 ¡Graznar Fuerte!", ["Bilingual"] = "💬 ডাক ছাড়ো (Squawk!)" },

        // Feeding subitems
        ["MenuHunger_Level"] = new() { ["English"] = "❤️ Fullness: {0}%", ["Bengali"] = "❤️ ক্ষুধা লেভেল: {0}%", ["Hindi"] = "❤️ पेट भरा: {0}%", ["Spanish"] = "❤️ Saciedad: {0}%", ["Bilingual"] = "❤️ ক্ষুধা লেভেল: {0}% (Fullness)" },
        ["MenuHunt_Bug"] = new() { ["English"] = "🐞 Hunt Ladybug", ["Bengali"] = "🐞 পোকা শিকার করো", ["Hindi"] = "🐞 कीड़ा शिकार करें", ["Spanish"] = "🐞 Cazar Mariquita", ["Bilingual"] = "🐞 পোকা শিকার করো (Hunt Bug)" },
        ["MenuHunt_Grasshopper"] = new() { ["English"] = "🦗 Hunt Grasshopper", ["Bengali"] = "🦗 ঘাসফড়িং শিকার করো", ["Hindi"] = "🦗 टिड्डा शिकार करें", ["Spanish"] = "🦗 Cazar Saltamontes", ["Bilingual"] = "🦗 ঘাসফড়িং শিকার করো (Hunt Grasshopper)" },
        ["MenuFeed_Chili"] = new() { ["English"] = "🌶️ Feed Spicy Chili", ["Bengali"] = "🌶️ ঝাল লঙ্কা খাওয়াও", ["Hindi"] = "🌶️ तीखी मिर्च खिलाएं", ["Spanish"] = "🌶️ Alimentar con Ají", ["Bilingual"] = "🌶️ ঝাল লঙ্কা খাওয়াও (Feed Chili)" },
        ["MenuFeed_Seeds"] = new() { ["English"] = "🌻 Feed Sunflower Seeds", ["Bengali"] = "🌻 সূর্যমুখী বীজ খাওয়াও", ["Hindi"] = "🌻 सूरजमुखी बीज खिलाएं", ["Spanish"] = "🌻 Alimentar con Semillas", ["Bilingual"] = "🌻 সূর্যমুখী বীজ খাওয়াও (Feed Seeds)" },

        // Other items
        ["MenuWeather"] = new() { ["English"] = "🌦️ Check Live Weather", ["Bengali"] = "🌦️ লাইভ আবহাওয়া চেক করো", ["Hindi"] = "🌦️ मौसम की जानकारी लें", ["Spanish"] = "🌦️ Consultar Clima en Vivo", ["Bilingual"] = "🌦️ লাইভ আবহাওয়া চেক করো (Check Weather)" },
        ["MenuCompanionBird"] = new() { ["English"] = "🦜🦜 Toggle Bird Buddy", ["Bengali"] = "🦜🦜 দ্বিতীয় সঙ্গী পাখি ডাকুন", ["Hindi"] = "🦜🦜 साथी पक्षी बुलाएं", ["Spanish"] = "🦜🦜 Alternar Ave Compañera", ["Bilingual"] = "🦜🦜 দ্বিতীয় সঙ্গী পাখি ডাকুন (Toggle Buddy)" },
        ["MenuEyeBreak"] = new() { ["English"] = "👁️ Rest Eyes Now (20-20-20)", ["Bengali"] = "👁️ চোখের বিশ্রাম (20-20-20)", ["Hindi"] = "👁️ आँखों को आराम दें", ["Spanish"] = "👁️ Descansar los Ojos (20-20-20)", ["Bilingual"] = "👁️ চোখের বিশ্রাম (20-20-20 Break)" },
        ["MenuHydrate"] = new() { ["English"] = "💧 Drink Water Reminder", ["Bengali"] = "💧 পানি খাওয়ার রিমাইন্ডার", ["Hindi"] = "💧 पानी पीने का रिमाइंडर", ["Spanish"] = "💧 Recordatorio de Beber Agua", ["Bilingual"] = "💧 পানি খাওয়ার রিমাইন্ডার (Hydrate)" },
        ["GhostModeMenu"] = new() { ["English"] = "👻 Ghost Mode (Click-Through)", ["Bengali"] = "👻 ঘোস্ট মোড (ক্লিক-থ্রু)", ["Hindi"] = "👻 घोस्ट मोड (क्लिक-थ्रू)", ["Spanish"] = "👻 Modo Fantasma (Clic Directo)", ["Bilingual"] = "👻 Ghost Mode (Click-Through)" },

        // Settings Window UI strings
        ["Settings_Title"] = new() { ["English"] = "Parot - Settings & Customization", ["Bengali"] = "Parot - সেটিংস ও কাস্টমাইজেশন", ["Hindi"] = "Parot - सेटिंग्स और कस्टमाइज़ेशन", ["Spanish"] = "Parot - Configuración y Personalización", ["Bilingual"] = "Parot - Settings & Customization" },
        ["Settings_Subtitle"] = new() { ["English"] = "Desktop companion, skins, dance & wellness controls", ["Bengali"] = "ডেস্কটপ বাডি, স্কিন, ড্যান্স ও ফোকাস কন্ট্রোল", ["Hindi"] = "डेस्कटॉप बडी, स्किन, डांस और वेलनेस कंट्रोल", ["Spanish"] = "Compañero de escritorio, aspectos, baile y bienestar", ["Bilingual"] = "ডেস্কটপ বাডি, স্কিন, ড্যান্স ও ফোকাস কন্ট্রোল" },
        
        ["Settings_CardSkins"] = new() { ["English"] = "🎨 Bird Species & Costumes", ["Bengali"] = "🎨 পাখির প্রজাতি ও পোশাক (Skins & Accessories)", ["Hindi"] = "🎨 पक्षी की प्रजाति और वेशभूषा", ["Spanish"] = "🎨 Especies y Disfraces", ["Bilingual"] = "🎨 পাখির প্রজাতি ও পোশাক (Skins & Costumes)" },
        ["Settings_LabelSpecies"] = new() { ["English"] = "Bird Species:", ["Bengali"] = "পাখির প্রজাতি:", ["Hindi"] = "पक्षी की प्रजाति:", ["Spanish"] = "Especie de Ave:", ["Bilingual"] = "পাখির প্রজাতি (Bird Species):" },
        ["Settings_LabelAccessories"] = new() { ["English"] = "Costumes / Accessories:", ["Bengali"] = "সাজসজ্জা / পোশাক:", ["Hindi"] = "वेशभूषा / साज-सज्जा:", ["Spanish"] = "Disfraces / Accesorios:", ["Bilingual"] = "সাজসজ্জা / পোশাক (Accessories):" },
        ["Settings_LabelPerchMode"] = new() { ["English"] = "Perch Position:", ["Bengali"] = "বসার স্থান:", ["Hindi"] = "बैठने का स्थान:", ["Spanish"] = "Posición de Perca:", ["Bilingual"] = "বসার স্থান (Perch Position):" },

        ["Settings_CardPomodoro"] = new() { ["English"] = "⏱️ Focus & Dance (Pomodoro)", ["Bengali"] = "⏱️ পোমোডোরো ও মিউজিক ড্যান্স (Focus & Dance)", ["Hindi"] = "⏱️ फ़ोकस और डांस (पोमोडोरो)", ["Spanish"] = "⏱️ Enfoque y Baile (Pomodoro)", ["Bilingual"] = "⏱️ পোমোডোরো ও মিউজিক ড্যান্স (Focus & Dance)" },
        ["Settings_LabelFocus"] = new() { ["English"] = "Pomodoro Focus Duration:", ["Bengali"] = "পোমোডোরো ফোকাস পর্ব:", ["Hindi"] = "पोमोडोरो फ़ोकस समय:", ["Spanish"] = "Duración de Enfoque Pomodoro:", ["Bilingual"] = "পোমোডোরো ফোকাস পর্ব (Focus Duration):" },
        ["Settings_LabelBreak"] = new() { ["English"] = "Pomodoro Break Duration:", ["Bengali"] = "পোমোডোরো বিরতি পর্ব:", ["Hindi"] = "पोमोडोरो ब्रेक समय:", ["Spanish"] = "Duración de Descanso Pomodoro:", ["Bilingual"] = "পোমোডোরো বিরতি পর্ব (Break Duration):" },
        ["Settings_ChkDanceMusic"] = new() { ["English"] = "🎶 Dance to system music beats (Beat-Sync)", ["Bengali"] = "🎶 গান বাজলে তালে তালে নাচবে (Beat-Sync Headbanging)", ["Hindi"] = "🎶 गाने की ताल पर सिर हिलाकर नाचेगा", ["Spanish"] = "🎶 Bailar al ritmo de la música del sistema", ["Bilingual"] = "🎶 গান বাজলে তালে তালে নাচবে (Beat-Sync)" },
        ["Settings_ChkNightMode"] = new() { ["English"] = "🌙 Night mode auto-sleep cap after 10 PM", ["Bengali"] = "🌙 রাত ১০টার পর ঘুমানোর ক্যাপ পরবে (Night Mode Auto)", ["Hindi"] = "🌙 रात 10 बजे के बाद नाइट मोड (स्लीपिंग कैप)", ["Spanish"] = "🌙 Modo nocturno automático después de las 10 PM", ["Bilingual"] = "🌙 রাত ১০টার পর ঘুমানোর ক্যাপ (Night Mode Auto)" },

        ["Settings_CardAI"] = new() { ["English"] = "🤖 AI Assistant & Voice Mimic", ["Bengali"] = "🤖 এআই ও ভয়েস (Gemini AI & Voice Mimic)", ["Hindi"] = "🤖 एआई सहायक और वॉयस मिमिक", ["Spanish"] = "🤖 Asistente IA y Eco de Voz", ["Bilingual"] = "🤖 এআই ও ভয়েস (Gemini AI & Voice Mimic)" },
        ["Settings_ChkVoiceMimic"] = new() { ["English"] = "Enable voice mimicry & high-pitch parrot speech", ["Bengali"] = "টিয়া পাখির কণ্ঠে কথা বলা সক্রিয় রাখুন (Voice Mimic)", ["Hindi"] = "तोते की आवाज़ में बोलना सक्रिय रखें", ["Spanish"] = "Activar imitación de voz en tono de loro", ["Bilingual"] = "টিয়া পাখির কণ্ঠে কথা বলা সক্রিয় রাখুন (Voice Mimic)" },
        ["Settings_LabelGeminiKey"] = new() { ["English"] = "Google Gemini API Key (optional — for online AI chat):", ["Bengali"] = "Google Gemini API Key (ঐচ্ছিক — অনলাইন এআই চ্যাটের জন্য):", ["Hindi"] = "Google Gemini API Key (वैकल्पिक — ऑनलाइन चैट के लिए):", ["Spanish"] = "Clave API de Google Gemini (opcional — para chat IA):", ["Bilingual"] = "Google Gemini API Key (ঐচ্ছিক — অনলাইন চ্যাটের জন্য):" },

        ["Settings_CardBehavior"] = new() { ["English"] = "🦜 Companion Size & Behavior", ["Bengali"] = "🦜 পাখির আকার ও আচরণ (Pet Behavior)", ["Hindi"] = "🦜 साथी का आकार और व्यवहार", ["Spanish"] = "🦜 Tamaño y Comportamiento", ["Bilingual"] = "🦜 পাখির আকার ও আচরণ (Pet Behavior)" },
        ["Settings_LabelScale"] = new() { ["English"] = "Companion Size:", ["Bengali"] = "Companion Size (আকার):", ["Hindi"] = "आकार (Scale):", ["Spanish"] = "Tamaño del Ave:", ["Bilingual"] = "Companion Size (আকার):" },
        ["Settings_ChkAlwaysOnTop"] = new() { ["English"] = "Always on Top of other windows", ["Bengali"] = "সব উইন্ডোর উপরে থাকবে (Always on Top)", ["Hindi"] = "हमेशा अन्य विंडो के ऊपर रखें", ["Spanish"] = "Siempre visible encima de otras ventanas", ["Bilingual"] = "সব উইন্ডোর উপরে থাকবে (Always on Top)" },
        ["Settings_ChkAllowWalking"] = new() { ["English"] = "Allow occasional desktop roaming & walking", ["Bengali"] = "ডেস্কটপে মাঝে মাঝে হাঁটাহাঁটি করবে (Allow Roaming)", ["Hindi"] = "डेस्कटॉप पर कभी-कभार टहलने दें", ["Spanish"] = "Permitir caminatas ocasionales por el escritorio", ["Bilingual"] = "মাঝে মাঝে হাঁটাহাঁটি করবে (Allow Roaming)" },
        ["Settings_ChkAutonomousFlight"] = new() { ["English"] = "🚀 Autonomous flight across screen", ["Bengali"] = "🚀 স্বাভাবিক উড়াউড়ি (Autonomous Flight Across Screen)", ["Hindi"] = "🚀 स्क्रीन पर स्वचालित उड़ान (Autonomous Flight)", ["Spanish"] = "🚀 Vuelo autónomo por la pantalla", ["Bilingual"] = "🚀 স্বাভাবিক উড়াউড়ি (Autonomous Flight)" },
        ["Settings_ChkBirdcage"] = new() { ["English"] = "🏰 Birdcage Mode (Keep bird calm inside cage)", ["Bengali"] = "🏰 খাঁচাবন্দি মোড (Birdcage Mode - পাখি খাঁচায় শান্ত থাকবে)", ["Hindi"] = "🏰 पिंजरा मोड (पक्षी पिंजरे में शांत रहेगा)", ["Spanish"] = "🏰 Modo Jaula (Mantener el ave tranquila en su jaula)", ["Bilingual"] = "🏰 খাঁচাবন্দি মোড (Birdcage Mode)" },
        ["Settings_ChkFollowCursor"] = new() { ["English"] = "🎯 Follow mouse cursor", ["Bengali"] = "🎯 মাউস কার্সার অনুসরণ (Follow Mouse Cursor)", ["Hindi"] = "🎯 माउस कर्सर का पीछा करें", ["Spanish"] = "🎯 Seguir el cursor del ratón", ["Bilingual"] = "🎯 মাউস কার্সার অনুসরণ (Follow Cursor)" },
        ["Settings_ChkGhostMode"] = new() { ["English"] = "Click-through Mode / Ghost Mode", ["Bengali"] = "ক্লিক-থ্রু মোড / Ghost Mode", ["Hindi"] = "क्लिक-थ्रू मोड / घोस्ट मोड", ["Spanish"] = "Modo Fantasma (Clic Directo sin estorbar)", ["Bilingual"] = "ক্লিক-থ্রু মোড / Ghost Mode" },

        ["Settings_CardWellness"] = new() { ["English"] = "🔊 Audio & Digital Wellness Reminders", ["Bengali"] = "🔊 সাউন্ড ও হেলথ রিমাইন্ডার (Audio & Wellness)", ["Hindi"] = "🔊 ऑडियो और वेलनेस रिमाइंडर", ["Spanish"] = "🔊 Audio y Recordatorios de Bienestar", ["Bilingual"] = "🔊 সাউন্ড ও হেলথ রিমাইন্ডার (Audio & Wellness)" },
        ["Settings_ChkSound"] = new() { ["English"] = "Enable bird chirps and audio effects", ["Bengali"] = "পাখির ডাক ও সাউন্ড চালু রাখুন", ["Hindi"] = "पक्षी की आवाज़ और ध्वनि प्रभाव चालू रखें", ["Spanish"] = "Activar sonidos y trinos del ave", ["Bilingual"] = "পাখির ডাক ও সাউন্ড চালু রাখুন (Sound Enabled)" },
        ["Settings_ChkScreenBreak"] = new() { ["English"] = "20-20-20 Eye break reminder", ["Bengali"] = "২০-২০-২০ চোখের বিশ্রাম রিমাইন্ডার", ["Hindi"] = "20-20-20 आँखों का ब्रेक रिमाइंडर", ["Spanish"] = "Recordatorio de descanso visual 20-20-20", ["Bilingual"] = "২০-২০-২০ চোখের বিশ্রাম (Eye Break)" },
        ["Settings_ChkHydration"] = new() { ["English"] = "Drink water hydration reminder", ["Bengali"] = "পানি পানের রিমাইন্ডার", ["Hindi"] = "पानी पीने का रिमाइंडर", ["Spanish"] = "Recordatorio de hidratación (beber agua)", ["Bilingual"] = "পানি পানের রিমাইন্ডার (Drink Water)" },
        ["Settings_BtnDailyReport"] = new() { ["English"] = "📊 View Daily Report", ["Bengali"] = "📊 দৈনিক রিপোর্ট দেখুন", ["Hindi"] = "📊 दैनिक रिपोर्ट देखें", ["Spanish"] = "📊 Ver Reporte Diario", ["Bilingual"] = "📊 View Daily Report" },
        ["Settings_BtnTestSound"] = new() { ["English"] = "🔊 Test Chirp", ["Bengali"] = "🔊 টেস্ট সাউন্ড", ["Hindi"] = "🔊 टेस्ट आवाज़", ["Spanish"] = "🔊 Probar Trino", ["Bilingual"] = "🔊 Test Chirp" },

        ["Settings_CardSystem"] = new() { ["English"] = "🌐 System & Language", ["Bengali"] = "🌐 সিস্টেম ও ভাষা (System & Language)", ["Hindi"] = "🌐 सिस्टम और भाषा", ["Spanish"] = "🌐 Sistema e Idioma", ["Bilingual"] = "🌐 সিস্টেম ও ভাষা (System & Language)" },
        ["Settings_LabelLanguage"] = new() { ["English"] = "Application Language:", ["Bengali"] = "অ্যাপের ভাষা (Language):", ["Hindi"] = "ऐप की भाषा:", ["Spanish"] = "Idioma de la Aplicación:", ["Bilingual"] = "অ্যাপের ভাষা (Language):" },
        ["Settings_ChkStartup"] = new() { ["English"] = "🚀 Start automatically with Windows", ["Bengali"] = "🚀 উইন্ডোজ চালুর সাথে অটো স্টার্ট (Start with Windows)", ["Hindi"] = "🚀 विंडोज़ शुरू होते ही ऑटो स्टार्ट", ["Spanish"] = "🚀 Iniciar automáticamente con Windows", ["Bilingual"] = "🚀 উইন্ডোজ চালুর সাথে অটো স্টার্ট (Start with Windows)" },

        ["Settings_BtnFeedTreat"] = new() { ["English"] = "🌶️ Feed Treat", ["Bengali"] = "🌶️ খাবার খাওয়াও", ["Hindi"] = "🌶️ ट्रीट खिलाएं", ["Spanish"] = "🌶️ Dar Premio", ["Bilingual"] = "🌶️ Feed Treat" },
        ["Settings_BtnCancel"] = new() { ["English"] = "Cancel", ["Bengali"] = "বাতিল", ["Hindi"] = "रद्द करें", ["Spanish"] = "Cancelar", ["Bilingual"] = "বাতিল (Cancel)" },
        ["Settings_BtnSave"] = new() { ["English"] = "Save Settings", ["Bengali"] = "সংরক্ষণ করুন", ["Hindi"] = "सेव करें", ["Spanish"] = "Guardar", ["Bilingual"] = "সংরক্ষণ করুন (Save)" }
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
