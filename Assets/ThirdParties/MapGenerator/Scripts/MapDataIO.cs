using System;
using System.IO;
using UnityEngine;

public static class MapDataIO
{
    // 파일명은 마음대로 바꿔도 됨
    public static string DefaultFilePath =>
#if UNITY_EDITOR
        Path.Combine(Application.streamingAssetsPath, "run_map.json");
#elif UNITY_STANDALONE
        Path.Combine(Application.persistentDataPath, "run_map.json");
#endif

    public static void SaveToFile(MapData data, string filePath = null)
    {
        if (data == null) 
        {
            throw new ArgumentNullException(nameof(data));
        }
        filePath ??= DefaultFilePath;

        string json = JsonUtility.ToJson(data, prettyPrint: true);

        string dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        { 
            Directory.CreateDirectory(dir);
        }

        File.WriteAllText(filePath, json);
#if UNITY_EDITOR
        Debug.Log($"[MapSaveIO] Saved: {filePath}");
#endif
    }

    public static bool TryLoadFromFile(out MapData data, string filePath = null)
    {
        filePath ??= DefaultFilePath;

        if (!File.Exists(filePath))
        {
            data = null;
            return false;
        }

        try
        {
            string json = File.ReadAllText(filePath);
            data = JsonUtility.FromJson<MapData>(json);
            return data != null;
        }
        catch (Exception e)
        {
            Debug.LogError($"[MapSaveIO] Load failed: {e.Message}");
            data = null;
            return false;
        }
    }

    public static bool TryRemoveFromFile(string filePath = null)
    {
        filePath ??= DefaultFilePath;

        if (!File.Exists(filePath))
        {
            return false;
        }

        try
        {
            File.Delete(filePath);
            return true;
        }
        catch (Exception e)
        {
            Debug.Log("삭제 실패 -> 덮어쓰기");
            File.WriteAllText(filePath, string.Empty);
            return true;
        }
    }
}
