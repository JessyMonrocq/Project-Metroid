using UnityEngine;

public interface IInteraction
{
    public Interactable interactable { get; set; }
    public bool interactionState { get; set; }

    public void InitializeInteraction(bool interacted);

    public void Interact();
}
