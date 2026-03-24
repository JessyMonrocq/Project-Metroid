using UnityEngine;
using System.Collections.Generic;
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

public class SceneManagement : MonoBehaviour
{
    public static SceneManagement Instance { get; private set; }

    [Header("Configuration")]
    public SceneReferenceSO sceneReferenceAsset;

    public SpawnPointData[] spawnPoints;
    public Interactable[] interactables;

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
        if (sceneReferenceAsset != null && sceneReferenceAsset.sceneData != null)
        {
            if (sceneReferenceAsset.sceneData.interactables == null || sceneReferenceAsset.sceneData.interactables.Length != interactables.Length)
            {
                sceneReferenceAsset.sceneData.interactables = new InteractablesData[interactables.Length];
                for (int i = 0; i < interactables.Length; i++)
                {
                    sceneReferenceAsset.sceneData.interactables[i] = new InteractablesData
                    {
                        id = i,
                        wasInteractedWith = false
                    };
                    interactables[i].InteractableID = i;
                    interactables[i].InitializeObject(false);
                }
            }
            else
            {
                for (int i = 0; i < interactables.Length; i++)
                {
                    InteractablesData data = sceneReferenceAsset.sceneData.interactables[i];
                    interactables[i].InitializeObject(data.wasInteractedWith);
                }
            }
        }
    }

    public void UpdateInteractableState(int interactableID, bool interacted)
    {
        if (sceneReferenceAsset != null && sceneReferenceAsset.sceneData != null)
        {
            int id = interactableID;
            InteractablesData[] data = sceneReferenceAsset.sceneData.interactables;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].id == id)
                {
                    data[i].wasInteractedWith = interacted;
                    break;
                }
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

            interactables = FindObjectsByType<Interactable>(FindObjectsInactive.Include ,FindObjectsSortMode.None);
            System.Array.Sort(interactables, (a, b) => string.Compare(a.name, b.name));
        }
    }
#endif
}
