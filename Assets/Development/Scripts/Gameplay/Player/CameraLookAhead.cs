using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraLookAhead : MonoBehaviour
{
    [SerializeField] private float lookAheadAmount;
    [SerializeField] private float adjustingSpeed;

    private bool isPlayerDashing;
    private CinemachineBrain brain;
    private CinemachineCamera currentCamera;
    private CinemachinePositionComposer currentComposer;

    private void Start()
    {
        brain = Camera.main.GetComponent<CinemachineBrain>();
    }

    private void Update()
    {
        CheckForDash();
    }

    private void CheckForDash()
    {
        if (!isPlayerDashing && PlayerController.Instance.IsPlayerDashing)
        {
            isPlayerDashing = true;
            StopCoroutine(SetCameraLookAhead(false));
            StartCoroutine(SetCameraLookAhead(true));
        } else if (isPlayerDashing && !PlayerController.Instance.IsPlayerDashing)
        {
            isPlayerDashing = false;
            StopCoroutine(SetCameraLookAhead(true));
            StartCoroutine(SetCameraLookAhead(false));
        }
    }

    private IEnumerator SetCameraLookAhead(bool reduce)
    {
        currentCamera = brain.ActiveVirtualCamera as CinemachineCamera;
        currentComposer = currentCamera.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachinePositionComposer;
        float elapsedTime = 0f;
        float startingValue = currentComposer.Lookahead.Time;
        float targetValue = reduce ? 0f : lookAheadAmount;
        while (elapsedTime < adjustingSpeed)
        {
            elapsedTime += Time.deltaTime;
            startingValue = Mathf.Lerp(startingValue, targetValue, elapsedTime);
            currentComposer.Lookahead.Time = startingValue;
            yield return null;
        }
        currentComposer.Lookahead.Time = reduce ? 0f : lookAheadAmount;
    }
}
