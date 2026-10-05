using System;
using System.IO;
using UnityEngine;

namespace AlmaGame.Systems
{
    // Local JSON save (no server, no account): one file in Application.persistentDataPath.
    // Writes go to a temporary file first and then replace the save, so a crash mid-write can't
    // leave a half-written file. A missing or unreadable save simply means "new game".
    public static class SaveSystem
    {
        private const string FileName = "alma_save.json";

        public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        public static bool HasSave => File.Exists(FilePath);

        public static SaveData Load()
        {
            if (!HasSave) return null;
            try
            {
                var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath));
                if (data == null || data.Version > SaveData.CurrentVersion) return null;
                return data;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Save file could not be read, starting fresh: {exception.Message}");
                return null;
            }
        }

        public static void Save(SaveData data)
        {
            try
            {
                data.Version = SaveData.CurrentVersion;
                data.SavedAtUtc = DateTime.UtcNow.ToString("o");
                string temporary = FilePath + ".tmp";
                File.WriteAllText(temporary, JsonUtility.ToJson(data, true));
                if (File.Exists(FilePath)) File.Delete(FilePath);
                File.Move(temporary, FilePath);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Save file could not be written: {exception.Message}");
            }
        }

        public static void Delete()
        {
            if (HasSave) File.Delete(FilePath);
        }
    }
}
