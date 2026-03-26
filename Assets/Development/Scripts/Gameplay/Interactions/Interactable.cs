using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    private int interactableID;

    public int InteractableID
    {
        get { return interactableID; }
        set { interactableID = value; }
    }

    protected bool interactionState;

    public void InitializeObject(bool interacted)
    {
        interactionState = interacted;
        InitializeInteraction(interacted);
    }

    public abstract void InitializeInteraction(bool interacted);
    public abstract void Interact(bool state);
}
