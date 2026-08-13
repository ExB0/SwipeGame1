using UnityEngine;

namespace SavingSystem
{
    public static class SaveSystem
    {
        private const string Key = "SAVE_DATA";

        public static void Save(SaveData data)
        {
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(Key, json);
            PlayerPrefs.Save();
        }

        public static SaveData Load()
        {
            if (!PlayerPrefs.HasKey(Key))
            {
                return new SaveData();
            }

            string json = PlayerPrefs.GetString(Key);

            SaveData data = null;

            try
            {
                data = JsonUtility.FromJson<SaveData>(json);
            }
            catch
            {
                Debug.LogWarning("Save corrupted, resetting...");
            }

            return data ?? new SaveData();
        }
    }
}