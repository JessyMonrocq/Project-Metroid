using UnityEngine;

public class AbilityUpgradeItem : MonoBehaviour
{
    public PlayerMovement.PlayerAbility playerAbility;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerController>())
        {
            PlayerController.Instance.SetPlayerAbility(playerAbility, true);
            Destroy(gameObject);

            string message = playerAbility.ToString() + " upgrade acquired!";
            GameManager.Instance.UINotification(message);
        }
    }
}
