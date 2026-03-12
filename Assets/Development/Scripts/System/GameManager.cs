using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Start()
    {
        InputSystemManager.Instance.SetPlayerInputState(true);
        InputSystemManager.Instance.SetDroneInputState(false);
        InputSystemManager.Instance.SetHackingInputState(false);
        InputSystemManager.Instance.SetUIInputState(true);
    }
}
