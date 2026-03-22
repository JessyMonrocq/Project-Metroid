using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private CanvasGroup fadeCanvasGroup;

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
        InputManager.Instance.SetUIInputState(true);

        fadeCanvasGroup.alpha = 0f;
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
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }
        fadeCanvasGroup.alpha = endAlpha;
    }
}
