using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class DroneManager : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerWeapon playerWeapon;
    [SerializeField] private GameObject dronePrefab;
    [SerializeField] private InputActionReference IA_SpawnDrone;
    [SerializeField] private bool canSpawnDrone;

    private GameObject drone;
    private CinemachineCamera cinemachineCamera;
    private bool droneSpawned;

    private void Start()
    {
        droneSpawned = false;
        drone = null;
        cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
        cinemachineCamera.Follow = playerMovement.gameObject.transform;
    }

    private void OnEnable()
    {
        IA_SpawnDrone.action.performed += (ctx) => HandleDrone();
    }

    private void OnDisable()
    {
        IA_SpawnDrone.action.performed -= (ctx) => HandleDrone();
    }

    private void HandleDrone()
    {
        if (!canSpawnDrone || !playerMovement.IsPlayerGrounded)
        {
            return;
        }

        droneSpawned = !droneSpawned;
        if (droneSpawned)
        {
            drone = Instantiate(dronePrefab, this.transform.position, Quaternion.identity);
        }
        else
        {
            Destroy(drone);
        }

        playerMovement.EnableInput(!droneSpawned);
        playerWeapon.EnableInput(!droneSpawned);
        cinemachineCamera.Follow = droneSpawned ? drone.transform : playerMovement.transform;
    }
}
