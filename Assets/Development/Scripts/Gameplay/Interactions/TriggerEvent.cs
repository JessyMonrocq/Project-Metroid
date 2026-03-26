using UnityEngine;
using UnityEngine.Events;
using static UnityEditor.Experimental.GraphView.GraphView;

public class TriggerEvent : Interactable
{
    public UnityEvent OnTriggerEnterEvent;
    public UnityEvent OnTriggerExitEvent;

    [SerializeField] private LayerMask layer;
    [SerializeField] private bool oneTimeEvent;

    public override void Interact(bool state)
    {
        interactionState = true;
        SceneManagement.Instance.UpdateInteractableState(InteractableID, interactionState);
    }

    public override void InitializeInteraction(bool interacted)
    {
        if (interacted)
        {
            this.gameObject.SetActive(false);
        }
        else
        {
            this.gameObject.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & layer) == 0)
        {
            return;
        }

        OnTriggerEnterEvent?.Invoke();

        if (oneTimeEvent)
        {
            Interact(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & layer) == 0)
        {
            return;
        }

        OnTriggerExitEvent?.Invoke();
    }
}
