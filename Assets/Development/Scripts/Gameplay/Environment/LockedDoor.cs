using DG.Tweening;
using System.Collections;
using UnityEngine;

public class LockedDoor : MonoBehaviour
{
    [SerializeField] private GameObject upperDoor;
    [SerializeField] private GameObject lowerDoor;
    [SerializeField] private bool isLocked = true;

    private void Start()
    {
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

    public void UnlockDoor()
    {
        isLocked = false;
        StartCoroutine(OpenDoorCoroutine());
    }

    private IEnumerator OpenDoorCoroutine()
    {
        yield return new WaitForSeconds(1f);
        upperDoor.transform.DOScaleY(0f, 0.75f).SetEase(Ease.InOutSine);
        lowerDoor.transform.DOScaleY(0f, 0.75f).SetEase(Ease.InOutSine);

    }
}
