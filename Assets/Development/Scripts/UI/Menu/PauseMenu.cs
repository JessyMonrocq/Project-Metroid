using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("Pause Menu References")]
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private CanvasGroup pauseMenuCG;
    [SerializeField] private GameObject pauseMenuMain;
    [SerializeField] private GameObject pauseMenuConfirm;
    [SerializeField] private Button continueButton;

    private void Start()
    {
        pauseMenuUI.transform.localScale = Vector3.zero;
        pauseMenuCG.alpha = 0f;

        InputManager.Instance.PauseGame.performed += OnPlayerPauseGame;
        InputManager.Instance.UIPause.performed += OnUICancelPause;
    }

    private void OnPlayerPauseGame(InputAction.CallbackContext context)
    {
        TogglePauseMenu(true);
    }

    private void OnUICancelPause(InputAction.CallbackContext context)
    {
        TogglePauseMenu(false);
    }

    public void TogglePauseMenu(bool isPaused)
    {
        StartCoroutine(TogglePauseMenuCoroutine(isPaused));
    }

    private IEnumerator TogglePauseMenuCoroutine(bool isPaused)
    {
        if (isPaused)
        {
            GameManager.Instance.PauseGame(true);

            pauseMenuMain.SetActive(true);
            pauseMenuConfirm.SetActive(false);

            pauseMenuUI.transform.localScale = new Vector3(0.01f, 0.01f, 1f);
            yield return new WaitForSecondsRealtime(0.1f);

            yield return pauseMenuUI.transform.DOScaleY(1f, 0.2f).SetUpdate(true).WaitForCompletion();
            yield return new WaitForSecondsRealtime(0.1f);

            yield return pauseMenuUI.transform.DOScaleX(1f, 0.2f).SetUpdate(true).WaitForCompletion();
            yield return new WaitForSecondsRealtime(0.2f);

            yield return pauseMenuCG.DOFade(1f, 0.25f).SetUpdate(true).WaitForCompletion();

            InputManager.Instance.SetUIInputState(true);
            continueButton.Select();
        }
        else
        {
            InputManager.Instance.SetUIInputState(false);

            yield return pauseMenuCG.DOFade(0f, 0.25f).SetUpdate(true).WaitForCompletion();

            yield return new WaitForSecondsRealtime(0.2f);
            yield return pauseMenuUI.transform.DOScaleX(0.01f, 0.2f).SetUpdate(true).WaitForCompletion();

            yield return new WaitForSecondsRealtime(0.1f);
            yield return pauseMenuUI.transform.DOScaleY(0.01f, 0.2f).SetUpdate(true).WaitForCompletion();

            pauseMenuUI.transform.localScale = new Vector3(0.01f, 0.01f, 1f);
            yield return new WaitForSecondsRealtime(0.1f);

            pauseMenuUI.transform.localScale = Vector3.zero;

            GameManager.Instance.PauseGame(false);
        }
    }
}
