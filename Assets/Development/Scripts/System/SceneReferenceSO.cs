using UnityEngine;

[CreateAssetMenu(menuName = "SceneReferenceSO")]
public class SceneReferenceSO : ScriptableObject
{
    [Header("Scene Reference")]
#if UNITY_EDITOR
    public UnityEditor.SceneAsset sceneAsset;
#endif
    public string sceneName;

    [HideInInspector]
    public string[] sceneSpawnPoints;

    [Header("Scene Data (JSON)")]
    public string sceneDataFileName;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (sceneAsset != null)
        {
            sceneName = sceneAsset.name;
        }

        if (string.IsNullOrEmpty(sceneDataFileName) && !string.IsNullOrEmpty(sceneName))
        {
            sceneDataFileName = sceneName + "_scenedata.json";
        }
    }
#endif
}
