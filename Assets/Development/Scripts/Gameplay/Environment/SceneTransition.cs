using Unity.Cinemachine;
using UnityEngine;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private SceneDestination destination;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        SceneTransitionManager.Instance.LoadSceneAndSpawn(destination, other.gameObject);
    }
}
