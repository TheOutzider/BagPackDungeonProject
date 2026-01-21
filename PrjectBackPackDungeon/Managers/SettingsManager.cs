using System;
using System.IO;
using System.Text.Json;

namespace PrjectBackPackDungeon;

public class GameSettings
{
    public float MusicVolume { get; set; } = 0.5f;
    public float SfxVolume { get; set; } = 0.5f;
    public bool IsFullScreen { get; set; } = false;
}

public static class SettingsManager
{
    private static string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
    public static GameSettings Settings { get; private set; }

    public static void LoadSettings()
    {
        if (File.Exists(_filePath))
        {
            try
            {
                string json = File.ReadAllText(_filePath);
                Settings = JsonSerializer.Deserialize<GameSettings>(json);
            }
            catch
            {
                Settings = new GameSettings();
            }
        }
        else
        {
            Settings = new GameSettings();
        }
    }

    public static void SaveSettings()
    {
        try
        {
            string json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save settings: {e.Message}");
        }
    }
}