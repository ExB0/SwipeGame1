using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    private const string KEY = "SAVE_DATA";

    public static void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(KEY, json);
        PlayerPrefs.Save();
    }

    public static SaveData Load()
    {
        if (!PlayerPrefs.HasKey(KEY))
            return new SaveData();

        string json = PlayerPrefs.GetString(KEY);

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
