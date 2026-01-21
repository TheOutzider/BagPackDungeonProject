using System;
using System.IO;
using System.Text.Json;

namespace PrjectBackPackDungeon;

public static class SaveManager
{
    private static string GetSavePath(int slotIndex)
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"save_{slotIndex}.json");
    }

    public static void SaveGame(SaveData data, int slotIndex)
    {
        try
        {
            // On met à jour la date de sauvegarde
            data.SaveDate = DateTime.Now;
            
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(GetSavePath(slotIndex), json);
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save slot {slotIndex}: {e.Message}");
        }
    }

    public static SaveData LoadGame(int slotIndex)
    {
        string path = GetSavePath(slotIndex);
        if (!File.Exists(path)) return null;

        try
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<SaveData>(json);
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load slot {slotIndex}: {e.Message}");
            return null;
        }
    }

    public static bool HasSave(int slotIndex)
    {
        return File.Exists(GetSavePath(slotIndex));
    }
    
    public static void DeleteSave(int slotIndex)
    {
        string path = GetSavePath(slotIndex);
        if (File.Exists(path)) File.Delete(path);
    }
}