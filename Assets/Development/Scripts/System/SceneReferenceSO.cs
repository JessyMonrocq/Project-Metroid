using System.Collections.Generic;
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

    public SceneDataSO sceneData;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (sceneAsset != null)
        {
            sceneName = sceneAsset.name;
        }
    }
#endif
}
