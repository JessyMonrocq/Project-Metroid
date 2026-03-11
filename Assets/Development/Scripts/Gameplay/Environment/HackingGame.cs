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
        InputSystemManager.Instance.SetPlayerInputState(false);
        InputSystemManager.Instance.SetHackingInputState(true);

        StartCoroutine(WaitForInput());
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


        SetupHackingGame();

        IA_HackingMove.action.started += (ctx) => RegisterInput();
        IA_HackingCancel.action.performed += (ctx) => CancelGame();
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

        InputSystemManager.Instance.SetPlayerInputState(true);
        InputSystemManager.Instance.SetHackingInputState(false);

        this.gameObject.SetActive(false);
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
        SetupHackingGame();
    }
    #endregion

}
