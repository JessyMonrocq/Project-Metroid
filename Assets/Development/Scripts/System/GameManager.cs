using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

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
    }
}
