using UnityEngine;
using UnityEngine.SceneManagement;

public class FirstSceneLoader : MonoBehaviour
{
    [SerializeField] private SceneDestination destination;

    private void Start()
    {
        SceneTransitionManager.Instance.LoadSceneAndSpawn(destination, PlayerController.Instance.gameObject);
    }
}
