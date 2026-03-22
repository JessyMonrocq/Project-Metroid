using UnityEngine;
using Unity.Cinemachine;

#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

[System.Serializable]
public struct SpawnPointData
{
    public string id;
    public Transform spawnLocation;
    public CinemachineCamera camera;
}

public class SceneSpawnPoints : MonoBehaviour
{
    [Header("Configuration")]
    public SceneReferenceSO sceneReferenceAsset;

    public SpawnPointData[] spawnPoints;

    public Transform GetSpawnPoint(string id)
    {
        foreach (var sp in spawnPoints)
        {
            if (sp.id == id)
            {
                return sp.spawnLocation;
            }
        }
        return null;
    }

    public CinemachineCamera GetSpawnCamera(string id)
    {
        foreach (var sp in spawnPoints)
        {
            if (sp.id == id)
            {
                return sp.camera;
            }
        }
        return null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (sceneReferenceAsset != null && spawnPoints != null)
        {
            string[] updatedIds = spawnPoints.Select(sp => sp.id).ToArray();

            if (sceneReferenceAsset.availableSpawnPoints == null || !sceneReferenceAsset.availableSpawnPoints.SequenceEqual(updatedIds))
            {
                sceneReferenceAsset.availableSpawnPoints = updatedIds;
                EditorUtility.SetDirty(sceneReferenceAsset);
            }
        }
    }
#endif
}
