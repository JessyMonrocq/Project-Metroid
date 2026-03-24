using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    private float fadeDuration = 0.33f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void LoadSceneAndSpawn(SceneDestination destination, GameObject playerOrRoot)
    {
        StartCoroutine(TransitionRoutine(destination, playerOrRoot));
    }

    private IEnumerator TransitionRoutine(SceneDestination destination, GameObject player)
    {
        InputManager.Instance.SetPlayerInputState(false);

        SceneManagement currentSceneSpawnPoints = FindAnyObjectByType<SceneManagement>();

        yield return GameManager.Instance.FadeScreen(true, fadeDuration);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(destination.scene.sceneName);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        SceneManagement spawnPoints = FindAnyObjectByType<SceneManagement>();

        if (spawnPoints != null)
        {
            Transform spawnPoint = spawnPoints.GetSpawnPoint(destination.spawnPointId);
            if (spawnPoint != null)
            {
                CharacterController characterController = player.GetComponent<CharacterController>();
                characterController.enabled = false;

                player.transform.position = spawnPoint.position;
                player.transform.rotation = spawnPoint.rotation;

                characterController.enabled = true;
            }
            else
            {
                Debug.LogWarning($"SpawnPoint '{destination.spawnPointId}' missing !");
            }

            var spawnCamera = spawnPoints.GetSpawnCamera(destination.spawnPointId);
            if (spawnCamera != null)
            {
                spawnCamera.Follow = player.transform;
            }
        }
        else
        {
            Debug.LogError("No SceneSpawnPoints found in new scene !");
        }

        yield return GameManager.Instance.FadeScreen(false, fadeDuration);

        InputManager.Instance.SetPlayerInputState(true);
    }
}
