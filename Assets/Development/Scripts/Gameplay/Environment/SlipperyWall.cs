using UnityEngine;

public class SlipperyWall : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement playerController = other.GetComponent<PlayerMovement>();
            if (playerController != null)
            {
                playerController.IncreaseWallSlideIntensity(true);
            }
        }
    }
}
