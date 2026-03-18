using Unity.Cinemachine;
using UnityEngine;

public class RoomCameraSwitch : MonoBehaviour
{
    [SerializeField] private Collider2D roomBounds;
    [SerializeField] private LayerMask layer;

    private CinemachineCamera cinemachineCamera;

    private void Start()
    {
        cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & layer) == 0)
        {
            return;
        }

        CinemachineConfiner2D confiner = cinemachineCamera.GetComponent<CinemachineConfiner2D>();
        if (confiner != null && confiner.BoundingShape2D != roomBounds)
        {
            Debug.Log("Switching camera to room bounds: " + roomBounds.name);
            confiner.BoundingShape2D = roomBounds;
        }
    }
}
