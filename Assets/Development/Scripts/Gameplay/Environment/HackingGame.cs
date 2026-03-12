using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HackingGame : MonoBehaviour
{
    #region Inspector Fields
    [HideInInspector]
    public UnityEvent OnHackingComplete;
    [HideInInspector]
    public UnityEvent OnHackingFailed;

    [Header("Hacking Game Settings")]
    [SerializeField][Range(1, 3)] private int numberOfSequences;
    [SerializeField] private float gameDuration = 5;

    [Header("Input Action References")]
    [SerializeField] private InputActionReference IA_HackingMove;
    [SerializeField] private InputActionReference IA_HackingCancel;

    [Header("Input Images References")]
    [SerializeField] private Image[] inputImages;

    [Header("Hacking Completion References")]
    [SerializeField] private Slider timerSlider;
    [SerializeField] private Toggle sequenceCompleteToggle1;
    [SerializeField] private Toggle sequenceCompleteToggle2;
    [SerializeField] private Toggle sequenceCompleteToggle3;

    [Header("Animation References")]
    [SerializeField] private GameObject background;
    [SerializeField] private CanvasGroup inputSequenceCG;
    [SerializeField] private CanvasGroup sequenceCompletionCG;
    [SerializeField] private CanvasGroup sliderCG;

    public enum DirectionalInput
    {
        Up,
        Down,
        Left,
        Right
    }

    private DirectionalInput[] randomInputSequence;

    private Vector2 input;
    private float gameTimer;

    private int currentInput;

    private int sequencesSucceeded;
    private bool registerInput;
    private bool gameStarted;
    #endregion

    #region Unity Methods
    private void Update()
    {
        if (!gameStarted)
        {
            return;
        }

        gameTimer += Time.deltaTime;
        timerSlider.value = (gameDuration - gameTimer) / gameDuration;
        if (gameTimer > gameDuration)
        {
            CancelGame();
        }
    }

    private void OnEnable()
    {
        StartCoroutine(OpeningCoroutine());
    }

    private void OnDisable()
    {
        IA_HackingMove.action.started -= (ctx) => RegisterInput();
        IA_HackingCancel.action.performed -= (ctx) => CancelGame();
    }
    #endregion

    #region Custom Methods
    private void SetupHackingGame()
    {
        InputSystemManager.Instance.SetPlayerInputState(false);
        InputSystemManager.Instance.SetHackingInputState(true);

        registerInput = false;
        gameStarted = false;

        gameTimer = 0;
        sequencesSucceeded = 0;

        timerSlider.value = 1;

        switch (numberOfSequences)
        {
            case 1:
                sequenceCompleteToggle1.gameObject.SetActive(true);
                sequenceCompleteToggle2.gameObject.SetActive(false);
                sequenceCompleteToggle3.gameObject.SetActive(false);
                break;
            case 2:
                sequenceCompleteToggle1.gameObject.SetActive(true);
                sequenceCompleteToggle2.gameObject.SetActive(true);
                sequenceCompleteToggle3.gameObject.SetActive(false);
                break;
            case 3:
                sequenceCompleteToggle1.gameObject.SetActive(true);
                sequenceCompleteToggle2.gameObject.SetActive(true);
                sequenceCompleteToggle3.gameObject.SetActive(true);
                break;
        }
        sequenceCompleteToggle1.isOn = false;
        sequenceCompleteToggle2.isOn = false;
        sequenceCompleteToggle3.isOn = false;
    }

    private void SetupHackingSequence()
    {
        currentInput = 0;
        randomInputSequence = new DirectionalInput[4];
        for (int i = 0; i < 4; i++)
        {
            randomInputSequence[i] = (DirectionalInput)Random.Range(0, 3);
        }

        for (int i = 0; i < 4; i++)
        {
            inputImages[i].color = Color.white;
            inputImages[i].transform.localScale = Vector3.one;

            inputImages[i].gameObject.transform.rotation = randomInputSequence[i] switch
            {
                DirectionalInput.Up => Quaternion.Euler(0, 0, 0),
                DirectionalInput.Down => Quaternion.Euler(0, 0, 180),
                DirectionalInput.Left => Quaternion.Euler(0, 0, 90),
                DirectionalInput.Right => Quaternion.Euler(0, 0, -90),
                _ => Quaternion.Euler(0, 0, 0)
            };
        }

        inputImages[0].transform.localScale *= 1.1f;
    }

    private void RegisterInput()
    {
        if (!registerInput)
        {
            return;
        }

        input = IA_HackingMove.action.ReadValue<Vector2>();
        input.Normalize();

        DirectionalInput directionalInput;

        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            directionalInput = input.x > 0 ? DirectionalInput.Right : DirectionalInput.Left;
        }
        else
        {
            directionalInput = input.y > 0 ? DirectionalInput.Up : DirectionalInput.Down;
        }

        if (directionalInput == randomInputSequence[currentInput])
        {
            inputImages[currentInput].transform.localScale = Vector3.one;
            inputImages[currentInput].color = Color.green;
            if (currentInput < randomInputSequence.Length - 1)
            {
                inputImages[currentInput + 1].transform.localScale *= 1.1f;
            }

            currentInput++;

            if (currentInput >= randomInputSequence.Length)
            {
                sequencesSucceeded++;
                sequenceCompleteToggle1.isOn = sequencesSucceeded >= 1;
                sequenceCompleteToggle2.isOn = sequencesSucceeded >= 2;
                sequenceCompleteToggle3.isOn = sequencesSucceeded >= 3;
                if (sequencesSucceeded >= numberOfSequences)
                {
                    gameTimer = 0;
                    OnHackingComplete.Invoke();

                    CancelGame();
                }
                else
                {
                    StartCoroutine(WaitForNewSequence());
                    StartCoroutine(WaitForInput());
                }
            }
        }
        else
        {
            gameTimer += 0.5f;
            StartCoroutine(WaitForInput());
        }
    }

    private void CancelGame()
    {
        registerInput = false;
        gameStarted = false;

        StartCoroutine(ClosingCoroutine());
    }
    #endregion

    #region Coroutine Methods
    private IEnumerator WaitForInput()
    {
        registerInput = false;
        yield return new WaitForSeconds(0.25f);
        registerInput = true;
    }

    private IEnumerator WaitForNewSequence()
    {
        yield return new WaitForSeconds(0.2f);
        SetupHackingSequence();
    }

    private IEnumerator OpeningCoroutine()
    {
        background.transform.localScale = new Vector3(0, 1, 1);
        inputSequenceCG.alpha = 0;
        sequenceCompletionCG.alpha = 0;
        sliderCG.alpha = 0;

        SetupHackingGame();

        SetupHackingSequence();

        yield return OpeningAnimationCoroutine();

        gameStarted = true;
        registerInput = true;

        IA_HackingMove.action.started += (ctx) => RegisterInput();
        IA_HackingCancel.action.performed += (ctx) => CancelGame();
    }

    private IEnumerator OpeningAnimationCoroutine()
    {
        yield return new WaitForSeconds(0.2f);
        float elapsedTime = 0;
        float scaleDuration = 0.33f;
        while (elapsedTime < scaleDuration)
        {
            float scaleX = Mathf.Lerp(0, 1, elapsedTime / scaleDuration);
            background.transform.localScale = new Vector3(scaleX, 1, 1);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        background.transform.localScale = Vector3.one;
        yield return new WaitForSeconds(0.2f);

        float fadeDuration = 0.25f;
        elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            sequenceCompletionCG.alpha = alpha;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        sequenceCompletionCG.alpha = 1;
        elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            sliderCG.alpha = alpha;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        sliderCG.alpha = 1;
        elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            inputSequenceCG.alpha = alpha;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        inputSequenceCG.alpha = 1;
        yield return new WaitForSeconds(0.2f);
    }

    private IEnumerator ClosingCoroutine()
    {
        yield return ClosingAnimationCoroutine();

        InputSystemManager.Instance.SetPlayerInputState(true);
        InputSystemManager.Instance.SetHackingInputState(false);

        this.gameObject.SetActive(false);
    }

    private IEnumerator ClosingAnimationCoroutine()
    {
        float fadeDuration = 0.25f;
        float elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            inputSequenceCG.alpha = alpha;
            sliderCG.alpha = alpha;
            sequenceCompletionCG.alpha = alpha;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        inputSequenceCG.alpha = 0;
        sliderCG.alpha = 0;
        sequenceCompletionCG.alpha = 0;
        yield return new WaitForSeconds(0.2f);

        elapsedTime = 0;
        float scaleDuration = 0.33f;
        while (elapsedTime < scaleDuration)
        {
            float scaleX = Mathf.Lerp(1, 0, elapsedTime / scaleDuration);
            background.transform.localScale = new Vector3(scaleX, 1, 1);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        background.transform.localScale = new Vector3(0, 1, 1);
    }
    #endregion

}
