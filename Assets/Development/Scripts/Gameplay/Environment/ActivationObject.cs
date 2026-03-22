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

    private CinemachineCamera lastCinemachineCamera;
    private Hacker currentHacker;

    public void Activate(Hacker hacker)
    {
        currentHacker = hacker;
        StartCoroutine(ActivationCoroutine());
    }

    private IEnumerator ActivationCoroutine()
    {
        if (currentHacker == Hacker.Player)
        {
            InputManager.Instance.SetPlayerInputState(false);
        }
        else
        {
            InputManager.Instance.SetDroneInputState(false);
        }

        yield return new WaitForSeconds(1.5f);

        CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
        CinemachineCamera cinemachineCamera = brain.ActiveVirtualCamera as CinemachineCamera;
        lastCinemachineCamera = cinemachineCamera;
        lastCinemachineCamera.Priority = 0;
        objectCinemachineCamera.Priority = 1;

        //TODO : needs to wait for completion of object instead
        OnActivate.Invoke();
        yield return new WaitForSeconds(2f);

        objectCinemachineCamera.Priority = 0;
        lastCinemachineCamera.Priority = 1;

        yield return new WaitForSeconds(1f);

        if (currentHacker == Hacker.Player)
        {
            InputManager.Instance.SetPlayerInputState(true);
        }
        else
        {
            InputManager.Instance.SetDroneInputState(true);
        }
    }
}
