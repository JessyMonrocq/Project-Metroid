using DG.Tweening;
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
    [SerializeField] private bool DEBUG_MODE = false;

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
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image sliderImage;

    [Header("Animation Colors")]
    [SerializeField] private Color correctInputColor = Color.blue;
    [SerializeField] private Color wrongInputColor = Color.red;
    [SerializeField] private Color defaultInputColor = Color.white;
    [SerializeField] private Color backgroundColorDefault;
    [SerializeField] private Color backgroundColorComplete;
    [SerializeField] private Color backgroundColorFailure;

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
    private bool canRegisterInput;
    private bool inputRegistered;
    private bool gameStarted;
    private bool gameFailed;
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
            gameFailed = true;
            backgroundImage.DOColor(backgroundColorFailure, 0.25f);

            CancelGame();
        }
    }

    private void OnEnable()
    {
        StartCoroutine(OpeningCoroutine());
    }
    #endregion

    #region Custom Methods
    private void SetupHackingGame()
    {
        canRegisterInput = false;
        inputRegistered = false;
        gameStarted = false;
        gameFailed = false;

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
            inputImages[i].DOKill();
            inputImages[i].color = defaultInputColor;
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
        if (!canRegisterInput)
        {
            return;
        }

        input = IA_HackingMove.action.ReadValue<Vector2>();
        input.Normalize();

        float magnitude = input.magnitude;
        if (magnitude == 0)
        {
            if (inputRegistered)
            {
                inputRegistered = false;
            }
            return;
        }

        if (magnitude != 0)
        {
            if (inputRegistered)
            {
                return;
            }
            else
            {
                inputRegistered = true;
            }
        }

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
            inputImages[currentInput].transform.DOScale(1f, 0.2f).From(1.1f).SetEase(Ease.OutBack);
            inputImages[currentInput].DOColor(correctInputColor, 0.2f);
            if (currentInput < randomInputSequence.Length - 1)
            {
                inputImages[currentInput + 1].transform.DOScale(1.1f, 0.2f).SetEase(Ease.OutBack);
            }

            RumbleManager.Instance.RumblePulse(0.75f, 0.75f, 0.1f);

            currentInput++;

            if (currentInput >= randomInputSequence.Length)
            {
                sequencesSucceeded++;
                StartCoroutine(CompletionToggleAnimationCoroutine(sequencesSucceeded));
                if (sequencesSucceeded >= numberOfSequences)
                {
                    gameTimer = 0;
                    if (!DEBUG_MODE)
                    {
                        OnHackingComplete.Invoke();
                    }
                    backgroundImage.DOColor(backgroundColorComplete, 0.25f);

                    CancelGame();
                }
                else
                {
                    StartCoroutine(WaitForNewSequenceCoroutine());
                }
            }
        }
        else
        {
            
            gameTimer += 0.5f;

            StartCoroutine(WrongInputDelayCoroutine());
        }
    }

    private void CancelGame()
    {
        canRegisterInput = false;
        gameStarted = false;

        StartCoroutine(ClosingCoroutine());
    }
    #endregion

    #region Coroutine Methods
    private IEnumerator WrongInputDelayCoroutine()
    {
        canRegisterInput = false;
        RumbleManager.Instance.RumblePulse(1f, 1f, 0.3f);

        Color sliderColor = sliderImage.color;
        sliderImage.DOColor(backgroundColorFailure, 0.1f).WaitForCompletion();

        background.transform.DOShakePosition(0.3f, new Vector3(10, 0, 0), 20, 90).SetEase(Ease.OutQuad);
        inputImages[currentInput].DOColor(wrongInputColor, 0.1f).WaitForCompletion();
        yield return inputImages[currentInput].transform.DOScale(0.9f, 0.1f).SetEase(Ease.OutBack).WaitForCompletion();
        yield return new WaitForSeconds(0.1f);

        sliderImage.DOColor(sliderColor, 0.1f).WaitForCompletion();

        inputImages[currentInput].DOColor(defaultInputColor, 0.1f).WaitForCompletion();
        yield return inputImages[currentInput].transform.DOScale(1f, 0.1f).SetEase(Ease.OutBack).WaitForCompletion();
        canRegisterInput = true;
    }

    private IEnumerator WaitForNewSequenceCoroutine()
    {
        canRegisterInput = false;
        yield return inputSequenceCG.DOFade(0f, 0.1f).WaitForCompletion();
        yield return new WaitForSeconds(0.1f);
        SetupHackingSequence();
        yield return new WaitForSeconds(0.1f);
        yield return inputSequenceCG.DOFade(1f, 0.1f).WaitForCompletion();
        canRegisterInput = true;
    }

    private IEnumerator CompletionToggleAnimationCoroutine(int sequence)
    {
        Toggle toggle = sequence switch
        {
            1 => sequenceCompleteToggle1,
            2 => sequenceCompleteToggle2,
            3 => sequenceCompleteToggle3,
            _ => null
        };
        yield return toggle.transform.DORotate(new Vector3(0, 0, 360), 0.33f, RotateMode.LocalAxisAdd).WaitForCompletion();
        toggle.isOn = true;
    }

    private IEnumerator OpeningCoroutine()
    {
        InputSystemManager.Instance.SetPlayerInputState(false);

        background.transform.localScale = new Vector3(0.01f, 0.01f, 1);
        backgroundImage.color = backgroundColorDefault;
        inputSequenceCG.alpha = 0;
        sequenceCompletionCG.alpha = 0;
        sliderCG.alpha = 0;

        SetupHackingGame();

        SetupHackingSequence();

        yield return OpeningAnimationCoroutine();

        gameStarted = true;
        canRegisterInput = true;

        InputSystemManager.Instance.SetHackingInputState(true);

        IA_HackingMove.action.performed += (ctx) => RegisterInput();
        IA_HackingCancel.action.performed += (ctx) => gameFailed = true;
        IA_HackingCancel.action.performed += (ctx) => CancelGame();
    }

    private IEnumerator OpeningAnimationCoroutine()
    {
        yield return new WaitForSeconds(0.1f);

        yield return background.transform.DOScaleY(1f, 0.2f).From(0.01f).WaitForCompletion();
        yield return new WaitForSeconds(0.1f);

        yield return background.transform.DOScaleX(1f, 0.2f).From(0.01f).WaitForCompletion();
        yield return new WaitForSeconds(0.2f);

        yield return sequenceCompletionCG.DOFade(1f, 0.25f).WaitForCompletion();
        yield return sliderCG.DOFade(1f, 0.25f).WaitForCompletion();
        yield return inputSequenceCG.DOFade(1f, 0.25f).WaitForCompletion();
    }

    private IEnumerator ClosingCoroutine()
    {
        InputSystemManager.Instance.SetHackingInputState(false);

        yield return ClosingAnimationCoroutine();

        if (gameFailed)
        {
            OnHackingFailed?.Invoke();
        }

        IA_HackingMove.action.performed -= (ctx) => RegisterInput();
        IA_HackingCancel.action.performed -= (ctx) => gameFailed = true;
        IA_HackingCancel.action.performed -= (ctx) => CancelGame();

        InputSystemManager.Instance.SetPlayerInputState(true);

        this.gameObject.SetActive(false);
    }

    private IEnumerator ClosingAnimationCoroutine()
    {
        if (gameFailed)
        {
            background.transform.DOShakePosition(0.5f, new Vector3(20, 0, 0), 30, 90).SetEase(Ease.OutQuad);
        }

        yield return new WaitForSeconds(0.5f);

        sequenceCompletionCG.DOFade(0f, 0.25f);
        sliderCG.DOFade(0f, 0.25f);
        inputSequenceCG.DOFade(0f, 0.25f);

        yield return new WaitForSeconds(0.5f);

        yield return background.transform.DOScaleX(0.01f, 0.2f).WaitForCompletion();
        yield return new WaitForSeconds(0.1f);

        yield return background.transform.DOScaleY(0.01f, 0.2f).WaitForCompletion();
    }
    #endregion

}
