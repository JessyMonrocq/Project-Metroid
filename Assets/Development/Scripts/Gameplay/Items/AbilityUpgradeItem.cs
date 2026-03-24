using UnityEngine;

public class AbilityUpgradeItem : Interactable
{
    public PlayerMovement.PlayerAbility playerAbility;

    #region Interactable overrides
    public override void Interact()
    {
        interactionState = true;
        SceneManagement.Instance.UpdateInteractableState(InteractableID, interactionState);
    }

    public override void InitializeInteraction(bool interacted)
    {
        if (interacted)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerController>())
        {
            PlayerController.Instance.SetPlayerAbility(playerAbility, true);

            string message = playerAbility.ToString() + " upgrade acquired!";
            GameManager.Instance.UINotification(message);

            Interact();
            gameObject.SetActive(false);
        }
    }
}
