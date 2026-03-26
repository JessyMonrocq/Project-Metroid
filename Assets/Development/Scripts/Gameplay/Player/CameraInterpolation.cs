using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraInterpolation : MonoBehaviour
{
    public static CameraInterpolation Instance;

    [Header("Camera Lerping Settings")]
    [SerializeField] private float fallPanAmount = 0.25f;
    [SerializeField] private float fallYPanTime = 0.35f;
    public float fallDistanceThreshold = 15f;

    public bool IsLerpingYDamping { get; private set; }

    public bool LerpedFromPlayerFalling { get; set; }

    private Coroutine lerpYPanCoroutine;
    private Coroutine lerpYTargetOffsetCoroutine;
    private CinemachinePositionComposer composer;
    private CinemachineCamera virtualCamera;
    private CinemachineBrain brain;

    private float normYPanAmount;
    private float normYTargetOffset;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(RefreshCameraReferences());
    }

    private void Start()
    {
        StartCoroutine(RefreshCameraReferences());
    }

    public void LerpYDamping(bool isPlayerFalling)
    {
        if (composer == null)
        {
            return;
        }

        lerpYPanCoroutine = StartCoroutine(LerpYPan(isPlayerFalling));
    }

    private IEnumerator LerpYPan(bool isPlayerFalling)
    {
        IsLerpingYDamping = true;

        float startDampAmount = composer.Damping.y;
        float startOffsetAmount = composer.TargetOffset.y;

        float endDampAmount = 0f;
        float endOffsetAmount = 0f;

        if (isPlayerFalling)
        {
            endDampAmount = fallPanAmount;
            endOffsetAmount = -normYTargetOffset * 2;

            LerpedFromPlayerFalling = true;
        }
        else
        {
            endDampAmount = normYPanAmount;
            endOffsetAmount = normYTargetOffset;
        }

        float elapsedTime = 0f;
        while (elapsedTime < fallYPanTime)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / fallYPanTime;

            composer.Damping.y = Mathf.Lerp(startDampAmount, endDampAmount, t);
            composer.TargetOffset.y = Mathf.Lerp(startOffsetAmount, endOffsetAmount, t);

            yield return null;
        }

        composer.Damping.y = endDampAmount;
        composer.TargetOffset.y = endOffsetAmount;

        IsLerpingYDamping = false;
    }

    private IEnumerator RefreshCameraReferences()
    {
        yield return new WaitForEndOfFrame();

        if (Camera.main != null)
        {
            brain = Camera.main.GetComponent<CinemachineBrain>();
            if (brain != null)
            {
                virtualCamera = brain.ActiveVirtualCamera as CinemachineCamera;
                if (virtualCamera != null)
                {
                    composer = virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachinePositionComposer;

                    if (composer != null)
                    {
                        normYPanAmount = composer.Damping.y;
                        normYTargetOffset = composer.TargetOffset.y;
                    }
                }
            }
        }
    }
}
