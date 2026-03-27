#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using System.Collections.Generic;
using System.IO;

[CreateAssetMenu(menuName = "SceneDataManagementSO")]
public class SceneDataManagementSO : ScriptableObject
{
    public List<string> sceneDataFileList = new List<string>();

    public void EraseAllData()
    {
        RefreshFileList();

        foreach (string filename in sceneDataFileList)
        {
            if (string.IsNullOrEmpty(filename))
            {
                continue;
            }

            string sceneName = Path.GetFileNameWithoutExtension(filename);
            if (sceneName.EndsWith("_scenedata"))
            {
                sceneName = sceneName.Substring(0, sceneName.Length - "_scenedata".Length);
            }

            SceneDataPersistence.DeletePersistentSceneData(sceneName);

#if UNITY_EDITOR
            string streamingPath = Path.Combine(Application.dataPath, "StreamingAssets", filename);
            if (File.Exists(streamingPath))
            {
                File.Delete(streamingPath);
            }
#endif
        }

        Debug.Log("Deleted persistent scene data for all known scene JSONs.");
    }

    private void RefreshFileList()
    {
        sceneDataFileList.Clear();

        string persistentFolder = Application.persistentDataPath;
        if (Directory.Exists(persistentFolder))
        {
            string[] persistentFiles = Directory.GetFiles(persistentFolder, "*_scenedata.json", SearchOption.TopDirectoryOnly);
            foreach (string file in persistentFiles)
            {
                sceneDataFileList.Add(Path.GetFileName(file));
            }
        }

#if UNITY_EDITOR
        string streamingFolder = Path.Combine(Application.dataPath, "StreamingAssets");
        if (Directory.Exists(streamingFolder))
        {
            string[] streamingFiles = Directory.GetFiles(streamingFolder, "*_scenedata.json", SearchOption.TopDirectoryOnly);
            foreach (string file in streamingFiles)
            {
                string fn = Path.GetFileName(file);
                if (!sceneDataFileList.Contains(fn))
                {
                    sceneDataFileList.Add(fn);
                }
            }
        }
#endif
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        RefreshFileList();
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(SceneDataManagementSO))]
public class SceneDataManagementSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SceneDataManagementSO manager = (SceneDataManagementSO)target;

        GUILayout.Space(15);

        if (GUILayout.Button("Erase All Scene Data (persistent)", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Warning", "This will erase all persistent Scene JSON data", "Yes", "Cancel"))
            {
                manager.EraseAllData();
                Debug.Log("All persistent Scene JSON data erased.");
            }
        }
    }
}
#endif
