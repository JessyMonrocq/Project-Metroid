using UnityEngine;
using UnityEngine.Events;

public class AbilityUpgradeItem : Interactable
{
    public UnityEvent OnAbilityAcquired;

    public enum AbilityType
    {
        Player,
        Drone
    }

    public AbilityType type;

    [SerializeField] private PlayerMovement.PlayerAbility playerAbility;
    [SerializeField] private DroneManager.DroneAbility droneAbility;

    #region Interactable overrides
    public override void Interact(bool state)
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
            string message = "";
            if (type == AbilityType.Player)
            {
                PlayerController.Instance.SetPlayerAbility(playerAbility, true);
                message = playerAbility.ToString() + " upgrade acquired!";
            }
            else if (type == AbilityType.Drone)
            {
                PlayerController.Instance.SetDroneAbility(droneAbility, true);
                message = "Drone " + droneAbility.ToString() + " upgrade acquired!";
            }
            
            GameManager.Instance.UINotification(message);

            Interact(true);
            OnAbilityAcquired?.Invoke();
            gameObject.SetActive(false);
        }
    }
}
