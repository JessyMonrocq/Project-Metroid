using UnityEngine;

public class AbilityUpgradeItem : MonoBehaviour, IInteraction
{
    public PlayerMovement.PlayerAbility playerAbility;

    #region IInteraction interface
    public Interactable interactable { get; set; }
    public bool interactionState { get; set; }

    private void Start()
    {
        interactable = GetComponent<Interactable>();
    }

    public void Interact()
    {
        interactionState = true;
        interactable.OnObjectInteracted();
    }

    public void InitializeInteraction(bool interacted)
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

            gameObject.SetActive(false);
        }
    }
}
