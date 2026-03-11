using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class DroneManager : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerWeapon playerWeapon;
    [SerializeField] private GameObject dronePrefab;
    [SerializeField] private InputActionReference IA_SpawnDrone;
    [SerializeField] private InputActionReference IA_DestroyDrone;
    [SerializeField] private bool canSpawnDrone;

    private GameObject drone;
    private CinemachineCamera cinemachineCamera;

    private bool registerInput = true;
    private float inputDelayDuration = 0.5f;
    private float inputDelayTimer = 0f;

    private void Start()
    {
        drone = null;
        cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
        cinemachineCamera.Follow = playerMovement.gameObject.transform;
    }

    private void OnEnable()
    {
        IA_SpawnDrone.action.performed += (ctx) => SpawnDrone();
        IA_DestroyDrone.action.performed += (ctx) => DestroyDrone();
    }

    private void OnDisable()
    {
        IA_SpawnDrone.action.performed -= (ctx) => SpawnDrone();
        IA_DestroyDrone.action.performed -= (ctx) => DestroyDrone();
    }

    private void Update()
    {
        if (!registerInput)
        {
            inputDelayTimer += Time.deltaTime;
            if (inputDelayTimer > inputDelayDuration)
            {
                registerInput = true;
                inputDelayTimer = 0f;
            }
        }
    }

    private void SpawnDrone()
    {
        if (!registerInput)
        {
            return;
        }

        if (!canSpawnDrone || !playerMovement.IsPlayerGrounded)
        {
            return;
        }

        drone = Instantiate(dronePrefab, this.transform.position, Quaternion.identity);

        InputSystemManager.Instance.SetPlayerInputState(false);
        InputSystemManager.Instance.SetDroneInputState(true);
        cinemachineCamera.Follow = drone.transform;

        registerInput = false;
    }

    private void DestroyDrone()
    {
        if (!registerInput)
        {
            return;
        }

        if (drone == null)
        {
            return;
        }

        Destroy(drone);

        InputSystemManager.Instance.SetPlayerInputState(true);
        InputSystemManager.Instance.SetDroneInputState(false);
        cinemachineCamera.Follow = playerMovement.transform;

        registerInput = false;
    }
}
