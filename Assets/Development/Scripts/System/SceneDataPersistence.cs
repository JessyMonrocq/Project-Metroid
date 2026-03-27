using System.IO;
using UnityEngine;

public static class SceneDataPersistence
{
    public static string GetFileNameForScene(string sceneName)
    {
        return $"{sceneName}_scenedata.json";
    }

    public static string GetPersistentPath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    public static string GetStreamingAssetsPath(string fileName)
    {
#if UNITY_EDITOR
        return Path.Combine(Application.dataPath, "StreamingAssets", fileName);
#else
        return Path.Combine(Application.streamingAssetsPath, fileName);
#endif
    }

    public static SceneData LoadSceneData(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            return null;
        }

        string fileName = GetFileNameForScene(sceneName);

        string persistentPath = GetPersistentPath(fileName);
        if (File.Exists(persistentPath))
        {
            string json = File.ReadAllText(persistentPath);
            return JsonUtility.FromJson<SceneData>(json);
        }

        string streamingPath = GetStreamingAssetsPath(fileName);
        if (File.Exists(streamingPath))
        {
            string json = File.ReadAllText(streamingPath);
            return JsonUtility.FromJson<SceneData>(json);
        }

        return null;
    }

    public static void SaveSceneData(SceneData data, string sceneName)
    {
        if (data == null || string.IsNullOrEmpty(sceneName))
        {
            return;
        }

        string fileName = GetFileNameForScene(sceneName);
        string targetPath = GetPersistentPath(fileName);

        string json = JsonUtility.ToJson(data, true);
        string dir = Path.GetDirectoryName(targetPath);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllText(targetPath, json);
    }

    public static void DeletePersistentSceneData(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            return;
        }

        string fileName = GetFileNameForScene(sceneName);
        string path = GetPersistentPath(fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}