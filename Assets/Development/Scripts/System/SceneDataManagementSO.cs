using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

[CreateAssetMenu(menuName = "SceneDataManagementSO")]
public class SceneDataManagementSO : ScriptableObject
{
    public List<SceneDataSO> sceneDataList = new List<SceneDataSO>();

#if UNITY_EDITOR
    private void OnValidate()
    {
        string path = AssetDatabase.GetAssetPath(this);
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        sceneDataList.Clear();
        string folderPath = Path.GetDirectoryName(path);
        
        string[] guids = AssetDatabase.FindAssets("t:SceneDataSO", new[] { folderPath });
        
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            SceneDataSO sceneData = AssetDatabase.LoadAssetAtPath<SceneDataSO>(assetPath);
            
            if (sceneData != null)
            {
                sceneDataList.Add(sceneData);
            }
        }
    }

    public void EraseAllData()
    {
        foreach (SceneDataSO sceneData in sceneDataList)
        {
            if (sceneData != null)
            {
                Undo.RecordObject(sceneData, "Erase Scene Data");
                
                sceneData.interactables = null; 
                
                EditorUtility.SetDirty(sceneData);
            }
        }
        
        AssetDatabase.SaveAssets();
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

        if (GUILayout.Button("Erase All Scene Data", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Warning", "This will erase all SceneData", "Yes", "Cancel"))
            {
                manager.EraseAllData();
                Debug.Log("All SceneData has been erased.");
            }
        }
    }
}
#endif
