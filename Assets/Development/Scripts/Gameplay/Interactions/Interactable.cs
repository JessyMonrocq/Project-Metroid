using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    private int interactableID;

    public int InteractableID
    {
        get { return interactableID; }
        set { interactableID = value; }
    }

    public void InitializeObject(bool interacted)
    {
        IInteraction interactionComponent = GetComponent<IInteraction>();
        if (interactionComponent != null)
        {
            interactionComponent.InitializeInteraction(interacted);
        }
    }

    public void OnObjectInteracted()
    {
        SceneManagement.Instance.UpdateInteractableState(interactableID, true);
    }
}
