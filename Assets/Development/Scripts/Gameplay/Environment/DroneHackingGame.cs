using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DroneHackingGame : MonoBehaviour
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
    [SerializeField] private InputActionReference IA_HackingInputA;
    [SerializeField] private InputActionReference IA_HackingInputB;
    [SerializeField] private InputActionReference IA_HackingInputC;

    [Header("Hacking Wheel References")]
    [SerializeField] private GameObject wheelCursor;
    [SerializeField] private Image cursorImage;
    [SerializeField] private Image inputImage;
    [SerializeField] private Sprite inputSpriteA;
    [SerializeField] private Sprite inputSpriteB;
    [SerializeField] private Sprite inputSpriteC;

    [Header("Hacking Completion References")]
    [SerializeField] private Slider timerSlider;
    [SerializeField] private Toggle sequenceCompleteToggle1;
    [SerializeField] private Toggle sequenceCompleteToggle2;
    [SerializeField] private Toggle sequenceCompleteToggle3;

    [Header("Animation References")]
    [SerializeField] private GameObject gameUI;
    [SerializeField] private CanvasGroup wheelCG;
    [SerializeField] private CanvasGroup sequenceCompletionCG;
    [SerializeField] private CanvasGroup sliderCG;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image sliderImage;

    [Header("Animation Colors")]
    [SerializeField] private Color defaultCursorColor;
    [SerializeField] private Color correctCursorColor;
    [SerializeField] private Color wrongCursorColor;
    [SerializeField] private Color backgroundColorDefault;
    [SerializeField] private Color backgroundColorComplete;
    [SerializeField] private Color backgroundColorFailure;

    private enum HackingInput
    {
        A,
        B,
        C
    }

    private HackingInput randomInput;
    private Vector2 input;

    private float gameTimer;
    private float randomAngle;
    private float lastGeneratedAngle;
    private float currentCursorAngle;

    private int sequencesSucceeded;
    private bool canRegisterInput;
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
        IA_HackingMove.action.performed += OnHackingMove;
        IA_HackingInputA.action.performed += OnHackingInputA;
        IA_HackingInputB.action.performed += OnHackingInputB;
        IA_HackingInputC.action.performed += OnHackingInputC;
        IA_HackingCancel.action.performed += OnHackingCancel;

        StartCoroutine(OpeningCoroutine());
    }

    private void OnDisable()
    {

        IA_HackingMove.action.performed -= OnHackingMove;
        IA_HackingInputA.action.performed -= OnHackingInputA;
        IA_HackingInputB.action.performed -= OnHackingInputB;
        IA_HackingInputC.action.performed -= OnHackingInputC;
        IA_HackingCancel.action.performed -= OnHackingCancel;
    }
    #endregion

    #region Custom Methods
    private void SetupHackingGame()
    {
        canRegisterInput = false;
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
        do
        {
            randomAngle = Random.Range(-135, 180);
            randomAngle = Mathf.Round(randomAngle / 45) * 45;
        } while (randomAngle == lastGeneratedAngle);
        lastGeneratedAngle = randomAngle;

        randomInput = (HackingInput)Random.Range(0, 3);
        inputImage.sprite = randomInput switch
        {
            HackingInput.A => inputSpriteA,
            HackingInput.B => inputSpriteB,
            HackingInput.C => inputSpriteC,
            _ => inputSpriteA
        };
    }

    private void MoveCursor()
    {
        if (!canRegisterInput)
        {
            return;
        }

        input = IA_HackingMove.action.ReadValue<Vector2>();
        if (input.magnitude == 0)
        {
            return;
        }


        float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
        float snappedAngle = Mathf.Round(angle / 45) * 45;

        if (snappedAngle == -180)
        {
            snappedAngle = -snappedAngle;
        }

        wheelCursor.transform.rotation = Quaternion.Euler(0, 0, snappedAngle); //Rotate with DOTween ?
        currentCursorAngle = snappedAngle;

        if (currentCursorAngle == randomAngle)
        {
            cursorImage.DOKill();
            inputImage.DOKill();
            cursorImage.DOColor(correctCursorColor, 0.2f);
            inputImage.DOFade(1f, 0.2f);
        }
        else
        {
            cursorImage.DOKill();
            inputImage.DOKill();
            cursorImage.DOColor(defaultCursorColor, 0.2f);
            inputImage.DOFade(0f, 0.2f);
        }
    }

    private void RegisterInput(HackingInput input)
    {
        if (!canRegisterInput)
        {
            return;
        }

        if (input == randomInput && currentCursorAngle == randomAngle)
        {
            sequencesSucceeded++;

            RumbleManager.Instance.RumblePulse(0.75f, 0.75f, 0.1f);
            
            StartCoroutine(CompletionToggleAnimationCoroutine(sequencesSucceeded));
            if (sequencesSucceeded == numberOfSequences)
            {
                gameTimer = 0;
                if (!DEBUG_MODE)
                {
                    OnHackingComplete?.Invoke();
                }
                backgroundImage.DOColor(backgroundColorComplete, 0.25f);

                CancelGame();
            }
            else
            {
                StartCoroutine(WaitForNewSequenceCoroutine());
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

    #region Input Callbacks
    private void OnHackingMove(InputAction.CallbackContext context)
    {
        MoveCursor();
    }

    private void OnHackingInputA(InputAction.CallbackContext context)
    {
        RegisterInput(HackingInput.A);
    }

    private void OnHackingInputB(InputAction.CallbackContext context)
    {
        RegisterInput(HackingInput.B);
    }

    private void OnHackingInputC(InputAction.CallbackContext context)
    {
        RegisterInput(HackingInput.C);
    }

    private void OnHackingCancel(InputAction.CallbackContext context)
    {
        gameFailed = true;
        CancelGame();
    }
    #endregion

    #region Coroutine Methods
    private IEnumerator WrongInputDelayCoroutine()
    {
        canRegisterInput = false;
        RumbleManager.Instance.RumblePulse(1f, 1f, 0.3f);

        Color sliderColor = sliderImage.color;
        sliderImage.DOColor(backgroundColorFailure, 0.1f).WaitForCompletion();

        gameUI.transform.DOShakePosition(0.3f, new Vector3(10, 0, 0), 20, 90).SetEase(Ease.OutQuad);
        cursorImage.DOColor(wrongCursorColor, 0.1f).WaitForCompletion();
        yield return new WaitForSeconds(0.1f);

        sliderImage.DOColor(sliderColor, 0.1f).WaitForCompletion();
        cursorImage.DOColor(defaultCursorColor, 0.1f).WaitForCompletion();
        yield return new WaitForSeconds(0.1f);

        canRegisterInput = true;
    }

    private IEnumerator WaitForNewSequenceCoroutine()
    {
        canRegisterInput = false;
        yield return new WaitForSeconds(0.1f);
        cursorImage.DOKill();
        inputImage.DOKill();
        cursorImage.DOColor(defaultCursorColor, 0.1f);
        yield return inputImage.DOFade(0f, 0.1f).WaitForCompletion();
        SetupHackingSequence();
        yield return new WaitForSeconds(0.1f);
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
        InputSystemManager.Instance.SetDroneInputState(false);

        gameUI.transform.localScale = new Vector3(0.01f, 0.01f, 1);
        backgroundImage.color = backgroundColorDefault;
        wheelCG.alpha = 0;
        sequenceCompletionCG.alpha = 0;
        sliderCG.alpha = 0;
        inputImage.DOFade(0f, 0f);
        cursorImage.color = defaultCursorColor;

        SetupHackingGame();

        SetupHackingSequence();

        yield return OpeningAnimationCoroutine();

        gameStarted = true;
        canRegisterInput = true;

        InputSystemManager.Instance.SetHackingInputState(true);
    }

    private IEnumerator OpeningAnimationCoroutine()
    {
        yield return new WaitForSeconds(0.1f);

        yield return gameUI.transform.DOScaleY(1f, 0.2f).From(0.01f).WaitForCompletion();
        yield return new WaitForSeconds(0.1f);

        yield return gameUI.transform.DOScaleX(1f, 0.2f).From(0.01f).WaitForCompletion();
        yield return new WaitForSeconds(0.2f);

        yield return sequenceCompletionCG.DOFade(1f, 0.25f).WaitForCompletion();
        yield return sliderCG.DOFade(1f, 0.25f).WaitForCompletion();
        yield return wheelCG.DOFade(1f, 0.25f).WaitForCompletion();
    }

    private IEnumerator ClosingCoroutine()
    {
        InputSystemManager.Instance.SetHackingInputState(false);

        yield return ClosingAnimationCoroutine();

        if (gameFailed)
        {
            OnHackingFailed?.Invoke();
        }

        InputSystemManager.Instance.SetDroneInputState(true);

        this.gameObject.SetActive(false);
    }

    private IEnumerator ClosingAnimationCoroutine()
    {
        if (gameFailed)
        {
            gameUI.transform.DOShakePosition(0.5f, new Vector3(20, 0, 0), 30, 90).SetEase(Ease.OutQuad);
        }

        yield return new WaitForSeconds(0.5f);

        sequenceCompletionCG.DOFade(0f, 0.25f);
        sliderCG.DOFade(0f, 0.25f);
        wheelCG.DOFade(0f, 0.25f);
        inputImage.DOFade(0f, 0.25f);

        yield return new WaitForSeconds(0.5f);

        yield return gameUI.transform.DOScaleX(0.01f, 0.2f).WaitForCompletion();
        yield return new WaitForSeconds(0.1f);

        yield return gameUI.transform.DOScaleY(0.01f, 0.2f).WaitForCompletion();
    }
    #endregion
}
