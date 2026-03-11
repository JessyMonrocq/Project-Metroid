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

    [Header("Hacking Game Settings")]
    [SerializeField][Range(1, 3)] private int numberOfSequences;
    [SerializeField] private float gameDuration = 5;

    [Header("Input Action References")]
    [SerializeField] private InputActionReference IA_HackingMove;
    [SerializeField] private InputActionReference IA_HackingCancel;
    [SerializeField] private InputActionReference IA_HackingInputA;
    [SerializeField] private InputActionReference IA_HackingInputB;
    [SerializeField] private InputActionReference IA_HackingInputC;

    [Header("Hacking Game References")]
    [SerializeField] private GameObject selectorObject;
    [SerializeField] private Image selectorImage;
    [SerializeField] private Color defaultSelectorColor;
    [SerializeField] private Color correctSelectorColor;
    [SerializeField] private Image inputImage;
    [SerializeField] private Sprite inputSpriteA;
    [SerializeField] private Sprite inputSpriteB;
    [SerializeField] private Sprite inputSpriteC;
    [SerializeField] private Slider timerSlider;
    [SerializeField] private Toggle sequenceCompleteToggle1;
    [SerializeField] private Toggle sequenceCompleteToggle2;
    [SerializeField] private Toggle sequenceCompleteToggle3;

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
    private float currentSelectorAngle;
    private int sequencesSucceeded;
    private bool registerInput;
    #endregion

    #region Unity Methods
    private void Update()
    {
        gameTimer += Time.deltaTime;
        timerSlider.value = (gameDuration - gameTimer) / gameDuration;
        if (gameTimer > gameDuration)
        {
            CancelGame();
        }
    }

    private void OnEnable()
    {
        InputSystemManager.Instance.SetDroneInputState(false);
        InputSystemManager.Instance.SetHackingInputState(true);

        registerInput = false;
        gameTimer = 0;
        sequencesSucceeded = 0;
        lastGeneratedAngle = 0;
        selectorObject.transform.rotation = Quaternion.identity;
        selectorImage.color = defaultSelectorColor;
        timerSlider.value = 1;

        switch(numberOfSequences)
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


        SetupHackingGame();

        IA_HackingMove.action.performed += (ctx) => MoveSelector();
        IA_HackingInputA.action.performed += (ctx) => RegisterInput(HackingInput.A);
        IA_HackingInputB.action.performed += (ctx) => RegisterInput(HackingInput.B);
        IA_HackingInputC.action.performed += (ctx) => RegisterInput(HackingInput.C);
        IA_HackingCancel.action.performed += (ctx) => CancelGame();
    }

    private void OnDisable()
    {
        IA_HackingMove.action.performed -= (ctx) => MoveSelector();
        IA_HackingInputA.action.performed -= (ctx) => RegisterInput(HackingInput.A);
        IA_HackingInputB.action.performed -= (ctx) => RegisterInput(HackingInput.B);
        IA_HackingInputC.action.performed -= (ctx) => RegisterInput(HackingInput.C);
        IA_HackingCancel.action.performed -= (ctx) => CancelGame();
    }
    #endregion

    #region Custom Methods
    private void SetupHackingGame()
    {
        selectorImage.color = defaultSelectorColor;
        inputImage.gameObject.SetActive(false);

        randomInput = (HackingInput)Random.Range(0, 2);
        do
        {
            randomAngle = Random.Range(-135, 180);
            randomAngle = Mathf.Round(randomAngle / 45) * 45;
        } while (randomAngle == lastGeneratedAngle);
        lastGeneratedAngle = randomAngle;

        inputImage.gameObject.SetActive(false);
        inputImage.sprite = randomInput switch
        {
            HackingInput.A => inputSpriteA,
            HackingInput.B => inputSpriteB,
            HackingInput.C => inputSpriteC,
            _ => inputSpriteA
        };

        StartCoroutine(WaitForInput());
    }

    private void MoveSelector()
    {
        if (!registerInput)
        {
            return;
        }

        input = IA_HackingMove.action.ReadValue<Vector2>();
        float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
        float snappedAngle = Mathf.Round(angle / 45) * 45;

        if (snappedAngle == -180)
        {
            snappedAngle = -snappedAngle;
        }

        selectorObject.transform.rotation = Quaternion.Euler(0, 0, snappedAngle);
        currentSelectorAngle = snappedAngle;

        if (currentSelectorAngle == randomAngle)
        {
            selectorImage.color = correctSelectorColor;
            inputImage.gameObject.SetActive(true);
        }
        else
        {
            selectorImage.color = defaultSelectorColor;
            inputImage.gameObject.SetActive(false);
        }
    }

    private void RegisterInput(HackingInput input)
    {
        if (!registerInput)
        {
            return;
        }

        if (input == randomInput && currentSelectorAngle == randomAngle)
        {
            sequencesSucceeded++;
            sequenceCompleteToggle1.isOn = sequencesSucceeded >= 1;
            sequenceCompleteToggle2.isOn = sequencesSucceeded >= 2;
            sequenceCompleteToggle3.isOn = sequencesSucceeded >= 3;

            if (sequencesSucceeded == numberOfSequences)
            {
                gameTimer = 0;
                OnHackingComplete?.Invoke();

                CancelGame();
            }
            else
            {
                SetupHackingGame();
            }
        }
    }

    private void CancelGame()
    {
        registerInput = false;

        InputSystemManager.Instance.SetDroneInputState(true);
        InputSystemManager.Instance.SetHackingInputState(false);

        this.gameObject.SetActive(false);
    }
    #endregion

    #region Coroutine Methods
    private IEnumerator WaitForInput()
    {
        registerInput = false;
        yield return new WaitForSeconds(0.5f);
        registerInput = true;
    }
    #endregion
}
