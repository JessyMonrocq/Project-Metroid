using DG.Tweening;
using System.Collections;
using UnityEngine;

public class LockedDoor : Interactable
{
    #region Inspector Fields
    [SerializeField] private GameObject upperDoor;
    [SerializeField] private GameObject lowerDoor;
    [SerializeField] private bool isLocked = true;
    #endregion

    #region Interactable overrides
    public override void Interact(bool state)
    {
        interactionState = state;
        SceneManagement.Instance.UpdateInteractableState(InteractableID, interactionState);
    }

    public override void InitializeInteraction(bool interacted)
    {
        isLocked = !interacted;
        if (isLocked)
        {
            upperDoor.transform.localScale = new Vector3(1, 1, 1);
            lowerDoor.transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            upperDoor.transform.localScale = new Vector3(1, 0, 1);
            lowerDoor.transform.localScale = new Vector3(1, 0, 1);
        }
    }
    #endregion

    #region Custom Methods
    public void UnlockDoor()
    {
        isLocked = false;
        Interact(true);
        StartCoroutine(OpenDoorCoroutine());
    }

    public void LockDoor()
    {
        isLocked = true;
        Interact(false);
        StartCoroutine(CloseDoorCoroutine());
    }
    #endregion

    #region Coroutine Methods
    private IEnumerator OpenDoorCoroutine()
    {
        yield return new WaitForSeconds(1f);
        upperDoor.transform.DOScaleY(0f, 0.75f).SetEase(Ease.InOutSine);
        lowerDoor.transform.DOScaleY(0f, 0.75f).SetEase(Ease.InOutSine);
    }

    private IEnumerator CloseDoorCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        upperDoor.transform.DOScaleY(1f, 0.66f).SetEase(Ease.InOutSine);
        lowerDoor.transform.DOScaleY(1f, 0.66f).SetEase(Ease.InOutSine);
    }
    #endregion
}
