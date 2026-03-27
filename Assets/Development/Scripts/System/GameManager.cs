using DG.Tweening;
using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private CanvasGroup fadeCG;
    [SerializeField] private CanvasGroup UINotificationCG;
    [SerializeField] private GameObject UINotificationParent;
    [SerializeField] private TextMeshProUGUI UINotificationText;
    [SerializeField] private PauseMenu pauseMenu;

    private const string MAINMENUSCENE = "MainMenuScene";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void Start()
    {
        InputManager.Instance.SetPlayerInputState(true);
        InputManager.Instance.SetDroneInputState(false);
        InputManager.Instance.SetHackingInputState(false);
        InputManager.Instance.SetUIInputState(false);

        fadeCG.alpha = 0f;
        UINotificationCG.alpha = 0f;
        UINotificationText.DOFade(0f, 0f).SetUpdate(true);
    }

    public void PauseGame(bool isPaused)
    {
        InputManager.Instance.SetPlayerInputState(!isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void UINotification(string message)
    {
        StartCoroutine(UINotificationCoroutine(message));
    }

    public void BackToMenu()
    {
        StartCoroutine(BackToMenuCoroutine());
    }

    public Coroutine FadeScreen(bool fadeIn, float duration)
    {
        return StartCoroutine(FadeScreenCoroutine(fadeIn, duration));
    }

    private IEnumerator FadeScreenCoroutine(bool fadeIn, float duration)
    {
        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            fadeCG.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }
        fadeCG.alpha = endAlpha;
    }

    private IEnumerator UINotificationCoroutine(string message)
    {
        PauseGame(true);

        UINotificationText.text = message;
        UINotificationParent.transform.localScale = new Vector3(0.01f, 0.01f, 1f);
        yield return UINotificationCG.DOFade(1f, 0.1f).SetUpdate(true).WaitForCompletion();

        yield return new WaitForSecondsRealtime(0.1f);
        
        yield return UINotificationParent.transform.DOScaleY(1f, 0.2f).SetUpdate(true).WaitForCompletion();
        yield return new WaitForSecondsRealtime(0.1f);

        yield return UINotificationParent.transform.DOScaleX(1f, 0.2f).SetUpdate(true).WaitForCompletion();
        yield return new WaitForSecondsRealtime(0.2f);

        yield return UINotificationText.DOFade(1f, 0.25f).SetUpdate(true).WaitForCompletion();

        yield return new WaitForSecondsRealtime(3f);

        yield return UINotificationText.DOFade(0f, 0.25f).SetUpdate(true).WaitForCompletion();
        yield return new WaitForSecondsRealtime(0.2f);
        
        yield return UINotificationParent.transform.DOScaleX(0.01f, 0.2f).From(1f).SetUpdate(true).WaitForCompletion();
        yield return new WaitForSecondsRealtime(0.1f);

        yield return UINotificationParent.transform.DOScaleY(0.01f, 0.2f).From(1f).SetUpdate(true).WaitForCompletion();
        yield return new WaitForSecondsRealtime(0.1f);

        UINotificationCG.alpha = 0f;

        PauseGame(false);
    }

    private IEnumerator BackToMenuCoroutine()
    {
        Time.timeScale = 1f;

        InputManager.Instance.SetUIInputState(false);
        yield return FadeScreen(true, 1f);

        ClearDontDestroyOnLoad();

        SceneManager.LoadScene(MAINMENUSCENE);
        InputManager.Instance.SetUIInputState(true);
    }

    private void ClearDontDestroyOnLoad()
    {
        var allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var go in allGameObjects)
        {
            if (go.hideFlags == HideFlags.None && go.scene.name == "DontDestroyOnLoad")
            {
                Destroy(go);
            }
        }
    }
}
