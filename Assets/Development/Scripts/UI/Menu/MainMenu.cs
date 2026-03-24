using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float rotationSpeed = 1f;

    [Header("Scene Settings")]
    [SerializeField] private string sceneToLoad = "MainScene";
    [SerializeField] private SceneDataManagementSO sceneDataManagement;

    [Header("Main Menu")]
    [SerializeField] private CanvasGroup mainMenuCG;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject playMenuPanel;

    [SerializeField] private Button playButton;
    [SerializeField] private Button startButton;
    [SerializeField] private Button returnButton;
    [SerializeField] private Button quitButton;

    private void Start()
    {
        mainMenuCG.alpha = 0f;
        mainMenuPanel.SetActive(true);
        playMenuPanel.SetActive(false);
        playButton.interactable = false;
        quitButton.interactable = false;
        StartCoroutine(FadeInMenu());

        playButton.onClick.AddListener(() =>
        {
            mainMenuPanel.SetActive(false);
            playMenuPanel.SetActive(true);

            startButton.Select();
        });
        startButton.onClick.AddListener(() => {
            startButton.interactable = false;
            returnButton.interactable = false;
            EraseAllData();
            StartCoroutine(FadeOutMenu());
        });
        returnButton.onClick.AddListener(() =>
        {
            playMenuPanel.SetActive(false);
            mainMenuPanel.SetActive(true);

            playButton.Select();
        });
        quitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }

    private void OnDestroy()
    {
        playButton.onClick.RemoveAllListeners();
        startButton.onClick.RemoveAllListeners();
        returnButton.onClick.RemoveAllListeners();
        quitButton.onClick.RemoveAllListeners();
    }

    private void Update()
    {
        cameraTransform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    private void EraseAllData()
    {
        sceneDataManagement.EraseAllData();
    }

    private IEnumerator FadeInMenu()
    {
        yield return new WaitForSeconds(1f);

        float duration = 1.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            mainMenuCG.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        mainMenuCG.alpha = 1f;
        playButton.interactable = true;
        quitButton.interactable = true;

        playButton.Select();
    }

    private IEnumerator FadeOutMenu()
    {
        float duration = 1.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            mainMenuCG.alpha = Mathf.Clamp01(1f - (elapsed / duration));
            yield return null;
        }
        mainMenuCG.alpha = 0f;
        
        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene(sceneToLoad);
    }
}
