using Unity.Cinemachine;
using UnityEngine;

public class RoomCameraSwitch : MonoBehaviour
{
    [SerializeField] private CinemachineCamera roomCamera;
    [SerializeField] private LayerMask layer;
    [SerializeField] private float zoomAmount = 3f;

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & layer) == 0)
        {
            return;
        }

        roomCamera.Priority = 1;
    }

    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & layer) == 0)
        {
            return;
        }

        roomCamera.Priority = 0;
    }
}
