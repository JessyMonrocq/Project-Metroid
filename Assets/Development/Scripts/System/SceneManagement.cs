using Unity.Cinemachine;
using UnityEngine;

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

public class SceneManagement : MonoBehaviour
{
    public static SceneManagement Instance { get; private set; }

    [Header("Configuration")]
    public SceneReferenceSO sceneReferenceAsset;

    public SpawnPointData[] spawnPoints;
    public Interactable[] interactables;

    private SceneData runtimeSceneData;

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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (sceneReferenceAsset != null && !string.IsNullOrEmpty(sceneReferenceAsset.sceneName))
        {
            runtimeSceneData = SceneDataPersistence.LoadSceneData(sceneReferenceAsset.sceneName);

            if (runtimeSceneData == null || runtimeSceneData.interactables == null || runtimeSceneData.interactables.Length != interactables.Length)
            {
                runtimeSceneData = new SceneData
                {
                    interactables = new InteractableData[interactables.Length]
                };

                for (int i = 0; i < interactables.Length; i++)
                {
                    runtimeSceneData.interactables[i] = new InteractableData
                    {
                        id = i,
                        wasInteractedWith = false
                    };

                    interactables[i].InteractableID = i;
                    interactables[i].InitializeObject(false);
                }

                SceneDataPersistence.SaveSceneData(runtimeSceneData, sceneReferenceAsset.sceneName);
            }
            else
            {
                for (int i = 0; i < interactables.Length; i++)
                {
                    InteractableData data = runtimeSceneData.interactables[i];
                    interactables[i].InteractableID = data.id;
                    interactables[i].InitializeObject(data.wasInteractedWith);
                }
            }
        }
    }

    public void UpdateInteractableState(int interactableID, bool interacted)
    {
        if (runtimeSceneData != null && runtimeSceneData.interactables != null)
        {
            int id = interactableID;
            InteractableData[] data = runtimeSceneData.interactables;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].id == id)
                {
                    data[i].wasInteractedWith = interacted;
                    break;
                }
            }

            if (sceneReferenceAsset != null && !string.IsNullOrEmpty(sceneReferenceAsset.sceneName))
            {
                SceneDataPersistence.SaveSceneData(runtimeSceneData, sceneReferenceAsset.sceneName);
            }
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (sceneReferenceAsset != null && spawnPoints != null)
        {
            string[] updatedIds = spawnPoints.Select(sp => sp.id).ToArray();

            if (sceneReferenceAsset.sceneSpawnPoints == null || !sceneReferenceAsset.sceneSpawnPoints.SequenceEqual(updatedIds))
            {
                sceneReferenceAsset.sceneSpawnPoints = updatedIds;
                EditorUtility.SetDirty(sceneReferenceAsset);
            }

            interactables = FindObjectsByType<Interactable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            System.Array.Sort(interactables, (a, b) => string.Compare(a.name, b.name));
        }
    }
#endif
}
