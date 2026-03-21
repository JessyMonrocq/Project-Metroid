using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    #region InspectorFields
    public static InputManager Instance { get; private set; }

    #region Player Input
    [Header("Player Input")]
    [SerializeField] private InputActionReference movePlayerAction;
    [SerializeField] private InputActionReference jumpPlayerAction;
    [SerializeField] private InputActionReference interactPlayerAction;
    [SerializeField] private InputActionReference attackPlayerAction;
    [SerializeField] private InputActionReference grapplePlayerAction;
    [SerializeField] private InputActionReference aimPlayerAction;
    [SerializeField] private InputActionReference dashPlayerAction;
    [SerializeField] private InputActionReference spawnDronePlayerAction;

    public InputAction PlayerMove => movePlayerAction.action;
    public InputAction PlayerJump => jumpPlayerAction.action;
    public InputAction PlayerInteract => interactPlayerAction.action;
    public InputAction PlayerAttack => attackPlayerAction.action;
    public InputAction PlayerGrapple => grapplePlayerAction.action;
    public InputAction PlayerAim => aimPlayerAction.action;
    public InputAction PlayerDash => dashPlayerAction.action;
    public InputAction PlayerSpawnDrone => spawnDronePlayerAction.action;
    #endregion

    #region Drone Input
    [Header("Drone Input")]
    [SerializeField] private InputActionReference moveDroneAction;
    [SerializeField] private InputActionReference interactDroneAction;
    [SerializeField] private InputActionReference destroyDroneAction;

    public InputAction DroneMove => moveDroneAction.action;
    public InputAction DroneInteract => interactDroneAction.action;
    public InputAction DroneDestroy => destroyDroneAction.action;
    #endregion

    #region Hacking Input
    [Header("Hacking Input")]
    [SerializeField] private InputActionReference moveHackingAction;
    [SerializeField] private InputActionReference cancelHackingAction;
    [SerializeField] private InputActionReference inputAHackingAction;
    [SerializeField] private InputActionReference inputBHackingAction;
    [SerializeField] private InputActionReference inputCHackingAction;

    public InputAction HackingMove => moveHackingAction.action;
    public InputAction HackingCancel => cancelHackingAction.action;
    public InputAction HackingInputA => inputAHackingAction.action;
    public InputAction HackingInputB => inputBHackingAction.action;
    public InputAction HackingInputC => inputCHackingAction.action;
    #endregion

    #region UI Input
    [Header("UI Input")]
    [SerializeField] private InputActionReference UINavigateAction;
    [SerializeField] private InputActionReference UISubmitAction;
    [SerializeField] private InputActionReference UICancelAction;
    // ...
    public InputAction UINavigate => UINavigateAction.action;
    public InputAction UISubmit => UISubmitAction.action;
    public InputAction UICancel => UICancelAction.action;
    #endregion

    #endregion

    #region Awake Method
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
    #endregion

    #region Public Methods
    public void SetPlayerInputState(bool state)
    {
        SetInputState(PlayerMove, state);
        SetInputState(PlayerJump, state);
        SetInputState(PlayerInteract, state);
        SetInputState(PlayerAttack, state);
        SetInputState(PlayerGrapple, state);
        SetInputState(PlayerAim, state);
        SetInputState(PlayerDash, state);
        SetInputState(PlayerSpawnDrone, state);
    }

    public void SetDroneInputState(bool state)
    {
        SetInputState(DroneMove, state);
        SetInputState(DroneInteract, state);
        SetInputState(DroneDestroy, state);
    }

    public void SetHackingInputState(bool state)
    {
        SetInputState(HackingMove, state);
        SetInputState(HackingCancel, state);
        SetInputState(HackingInputA, state);
        SetInputState(HackingInputB, state);
        SetInputState(HackingInputC, state);
    }

    public void SetUIInputState(bool state)
    {
        SetInputState(UINavigate, state);
        SetInputState(UISubmit, state);
        SetInputState(UICancel, state);
    }
    #endregion

    private void SetInputState(InputAction action, bool state)
    {
        if (state)
        {
            action.Enable();
        }
        else
        {
            action.Disable();
        }
    }
}
