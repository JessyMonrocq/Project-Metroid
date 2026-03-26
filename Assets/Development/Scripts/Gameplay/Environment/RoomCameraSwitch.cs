using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class RoomCameraSwitch : MonoBehaviour
{
    [SerializeField] private CinemachineCamera roomCamera;
    [SerializeField] private LayerMask layer;

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & layer) == 0)
        {
            return;
        }

        CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
        CinemachineCamera currentCamera = brain.ActiveVirtualCamera as CinemachineCamera;

        Transform followTarget = currentCamera.Follow;

        if (currentCamera != null && currentCamera != roomCamera)
        {
            currentCamera.Priority = 0;
        }
        roomCamera.Priority = 1;
        roomCamera.Follow = followTarget;        
    }
}
