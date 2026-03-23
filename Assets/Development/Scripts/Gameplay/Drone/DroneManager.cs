using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class DroneManager : MonoBehaviour
{
    #region Inspector Fields
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerWeapon playerWeapon;
    [SerializeField] private GameObject dronePrefab;
    [SerializeField] private float droneCameraDistanceDifference = 2f;
    [SerializeField] private float droneCameraZoomTime = 1f;
    [SerializeField] private bool canSpawnDrone;

    private GameObject drone;
    private CinemachineBrain cinemachineBrain;
    private CinemachineCamera playerLastCinemachineCamera;

    private bool registerInput = true;
    private float inputDelayDuration = 0.5f;
    private float inputDelayTimer = 0f;
    #endregion

    #region Unity Methods
    private void Start()
    {
        drone = null;

        cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();

        InputManager.Instance.PlayerSpawnDrone.performed += OnSpawnDrone;
        InputManager.Instance.DroneDestroy.performed += OnDestroyDrone;
    }

    private void OnDisable()
    {
        InputManager.Instance.PlayerSpawnDrone.performed -= OnSpawnDrone;
        InputManager.Instance.DroneDestroy.performed -= OnDestroyDrone;
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
    #endregion

    #region Input Callbacks
    private void OnSpawnDrone(InputAction.CallbackContext context)
    {
        SpawnDrone();
    }

    private void OnDestroyDrone(InputAction.CallbackContext context)
    {
        DestroyDrone();
    }
    #endregion

    #region Custom Methods
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

        InputManager.Instance.SetPlayerInputState(false);
        InputManager.Instance.SetDroneInputState(true);

        CinemachineCamera cinemachineCamera = cinemachineBrain.ActiveVirtualCamera as CinemachineCamera;
        playerLastCinemachineCamera = cinemachineCamera;
        playerLastCinemachineCamera.Follow = drone.transform;

        /*
        CinemachineCamera[] cinemachineCameras = FindObjectsByType<CinemachineCamera>(UnityEngine.FindObjectsSortMode.None);
        foreach (CinemachineCamera camera in cinemachineCameras)
        {
            if (camera.Follow != drone.transform)
            {
                continue;
            }

            CinemachinePositionComposer composer = camera.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachinePositionComposer;
            composer.CameraDistance -= droneCameraDistanceDifference;
        }

        StartCoroutine(ZoomCamera(cinemachineCamera, -droneCameraDistanceDifference, droneCameraZoomTime));
        */

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

        InputManager.Instance.SetPlayerInputState(true);
        InputManager.Instance.SetDroneInputState(false);
        
        CinemachineCamera cinemachineCamera = cinemachineBrain.ActiveVirtualCamera as CinemachineCamera;
        if (cinemachineCamera != playerLastCinemachineCamera)
        {
            cinemachineCamera.Priority = 0;
            playerLastCinemachineCamera.Priority = 1;
            playerLastCinemachineCamera.Follow = playerMovement.transform;
        }
        else
        {
            cinemachineCamera.Follow = playerMovement.transform;
        }

        /*
        CinemachineCamera[] cinemachineCameras = FindObjectsByType<CinemachineCamera>(UnityEngine.FindObjectsSortMode.None);
        foreach (CinemachineCamera camera in cinemachineCameras)
        {
            if (camera.Follow != playerMovement.transform)
            {
                continue;
            }

            CinemachinePositionComposer composer = camera.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachinePositionComposer;
            composer.CameraDistance += droneCameraDistanceDifference;
        }

        StartCoroutine(ZoomCamera(cinemachineCamera, droneCameraDistanceDifference, droneCameraZoomTime));
        */

        registerInput = false;
    }
    #endregion

    #region Coroutine Methods
    private IEnumerator ZoomCamera(CinemachineCamera cinemachineCamera, float targetOffset, float duration)
    {
        CinemachinePositionComposer composer = cinemachineCamera.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachinePositionComposer;
        float startDistance = composer.CameraDistance;
        float targetDistance = startDistance + targetOffset;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newDistance = Mathf.Lerp(startDistance, targetDistance, elapsedTime / duration);
            composer.CameraDistance = newDistance;
            yield return null;
        }
        composer.CameraDistance = targetDistance;
    }
    #endregion
}
