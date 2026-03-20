using DG.Tweening;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;

public class ActivationObject : MonoBehaviour
{
    public UnityEvent OnActivate;

    public enum Hacker
    {
        Player,
        Drone
    }

    [SerializeField] private CinemachineCamera objectCinemachineCamera;

    private CinemachineBrain cinemachineBrain;
    private CinemachineCamera lastCinemachineCamera;
    private Hacker currentHacker;

    private void Start()
    {
        cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
    }

    public void Activate(Hacker hacker)
    {
        currentHacker = hacker;
        StartCoroutine(ActivationCoroutine());
    }

    private IEnumerator ActivationCoroutine()
    {
        if (currentHacker == Hacker.Player)
        {
            InputSystemManager.Instance.SetPlayerInputState(false);
        }
        else
        {
            InputSystemManager.Instance.SetDroneInputState(false);
        }

        yield return new WaitForSeconds(1.5f);

        CinemachineCamera cinemachineCamera = cinemachineBrain.ActiveVirtualCamera as CinemachineCamera;
        lastCinemachineCamera = cinemachineCamera;
        lastCinemachineCamera.Priority = 0;
        objectCinemachineCamera.Priority = 1;

        // Needs to wait for completion of object instead
        OnActivate.Invoke();
        yield return new WaitForSeconds(2f);

        objectCinemachineCamera.Priority = 0;
        lastCinemachineCamera.Priority = 1;

        yield return new WaitForSeconds(1f);

        if (currentHacker == Hacker.Player)
        {
            InputSystemManager.Instance.SetPlayerInputState(true);
        }
        else
        {
            InputSystemManager.Instance.SetDroneInputState(true);
        }
    }
}
