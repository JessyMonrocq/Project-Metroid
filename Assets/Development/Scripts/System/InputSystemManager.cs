using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystemManager : MonoBehaviour
{
    public static InputSystemManager Instance;

    [SerializeField] private InputActionAsset inputActionMap;

    private const string PLAYER = "Player";
    private const string DRONE = "Drone";
    private const string UI = "UI";
    private const string HACKING = "Hacking";

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

    public void SetPlayerInputState(bool state)
    {
        SetInputState(PLAYER, state);
    }

    public void SetDroneInputState(bool state)
    {
        SetInputState(DRONE, state);
    }

    public void SetUIInputState(bool state)
    {
        SetInputState(UI, state);
    }

    public void SetHackingInputState(bool state)
    {
        SetInputState(HACKING, state);
    }

    private void SetInputState(string actionMap, bool state)
    {
        if (state)
        {
            inputActionMap.FindActionMap(actionMap).Enable();
        }
        else
        {
            inputActionMap.FindActionMap(actionMap).Disable();
        }
    }
}
