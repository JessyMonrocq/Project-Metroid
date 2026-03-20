using UnityEngine;

public class DebugMessage : MonoBehaviour
{
    public void DebugLog(string message)
    {
        Debug.Log(message);
    }

    public void DebugError(string message)
    {
        Debug.LogError(message);
    }
}
