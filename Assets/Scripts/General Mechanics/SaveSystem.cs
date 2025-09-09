using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public static class SaveSystem
{
    private const string SAVE_ROOT = "saves";
    
    private static string GetUserFolder(string userId)
    {
        if (string.IsNullOrEmpty(userId)) return null;
        return Path.Combine(Application.persistentDataPath, SAVE_ROOT, userId);
    }
    
    public static bool SaveGame(string userId, SaveData data, out string outFileName)
    {
        outFileName = null;
        if(string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("SaveSystem: cannot save - userId is null/empty");
            return false;
        }

        try
        {
            var folder = GetUserFolder(userId);
            Directory.CreateDirectory(folder);

            data.timestamp = DateTime.UtcNow.ToString("O");
            var safeName = string.IsNullOrEmpty(data.saveName) ? "save" : SanitizeFileName(data.saveName);
            var time = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var fileName = $"{safeName}_{time}.json";
            var path = Path.Combine(folder, fileName);

            var json = JsonUtility.ToJson(data);
            File.WriteAllText(path, json);

            outFileName = fileName;
            Debug.Log($"SaveSystem: saved to {path}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"SaveSystem.SaveGame failed: {ex}");
            return false;
        }
    }

    public static List<SaveMeta> GetSaveMetas(string userId)
    {
        var list = new List<SaveMeta>();
        if (string.IsNullOrEmpty(userId)) return list;

        var folder = GetUserFolder(userId);
        if (!Directory.Exists(folder)) return list;

        var files = Directory.GetFiles(folder, "*.json");
        foreach (var f in files)
        {
            try
            {
                var json = File.ReadAllText(f);
                var data = JsonUtility.FromJson<SaveData>(json);
                var meta = new SaveMeta
                {
                    fileName = Path.GetFileName(f),
                    saveName = string.IsNullOrEmpty(data.saveName) ? Path.GetFileNameWithoutExtension(f) : data.saveName,
                    timestamp = data.timestamp ?? File.GetLastWriteTimeUtc(f).ToString("O"),
                    sceneName = data.sceneName,
                    health = data.health,
                    mana = data.mana,
                };
                list.Add(meta);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"SaveSystem: failed reading {f}: {ex}");
            }
        }

        //Sort descending by timestamp
        list.Sort((a, b) => string.Compare(b.timestamp, a.timestamp, StringComparison.Ordinal));
        return list;
    }

        public static SaveData LoadSave(string userId, string fileName)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(fileName)) return null;
        var path = Path.Combine(GetUserFolder(userId), fileName);
        if(!File.Exists(path)) return null;
        try
        {
            var json = File.ReadAllText(path);
            return JsonUtility.FromJson<SaveData>(json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"SaveSystem.LoadSave failed: {ex}");
            return null;
        }
    }

        public static bool DeleteSave(string userId, string fileName)
    {
        if (string.IsNullOrEmpty (userId) || string.IsNullOrEmpty(fileName)) return false;
        var path = Path.Combine (GetUserFolder(userId), fileName);
        if(!File.Exists (path)) return false;
        try
        {
            File.Delete(path);
            return true;
        }
        catch(Exception ex)
        {
            Debug.LogError($"SaveSystem.DeleteSave failed: {ex}");
            return false;
        }
    }

        private static string SanitizeFileName(string input)
    {
        foreach (var invalid in Path.GetInvalidFileNameChars())
            input = input.Replace(invalid, '_');
        return input;
    }
}
