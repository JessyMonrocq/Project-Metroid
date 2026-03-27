using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WorldUI : MonoBehaviour
{
    #region Inspector Fields
    [SerializeField] private LayerMask layer;

    [Header("UI References")]
    [SerializeField] private GameObject UIPanelGameObject;
    [SerializeField] private CanvasGroup UIPanelContentCG;
    #endregion

    #region Unity Methods
    private void Start()
    {
        UIPanelGameObject.transform.localScale = Vector3.zero;
        UIPanelContentCG.alpha = 0f;
    }

    public void Display()
    {
        StartCoroutine(DisplayWorldUICoroutine(true));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & layer) == 0)
        {
            return;
        }

        StopCoroutine(DisplayWorldUICoroutine(false));
        StartCoroutine(DisplayWorldUICoroutine(true));
    }

    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & layer) == 0)
        {
            return;
        }

        StopCoroutine(DisplayWorldUICoroutine(true));
        StartCoroutine(DisplayWorldUICoroutine(false));
    }
    #endregion

    #region Coroutine Methods
    private IEnumerator DisplayWorldUICoroutine(bool display)
    {
        if (display)
        {
            UIPanelGameObject.transform.localScale = new Vector3(0.01f, 0.01f, 1f);
            yield return new WaitForSecondsRealtime(0.05f);

            yield return UIPanelGameObject.transform.DOScaleY(1f, 0.1f).WaitForCompletion();
            yield return new WaitForSecondsRealtime(0.05f);

            yield return UIPanelGameObject.transform.DOScaleX(1f, 0.1f).WaitForCompletion();
            yield return new WaitForSecondsRealtime(0.1f);

            yield return UIPanelContentCG.DOFade(1f, 0.1f).WaitForCompletion();
        }
        else
        {
            yield return UIPanelContentCG.DOFade(0f, 0.1f).WaitForCompletion();
            yield return new WaitForSecondsRealtime(0.1f);

            yield return UIPanelGameObject.transform.DOScaleX(0.01f, 0.1f).From(1f).WaitForCompletion();
            yield return new WaitForSecondsRealtime(0.05f);

            yield return UIPanelGameObject.transform.DOScaleY(0.01f, 0.1f).From(1f).WaitForCompletion();
            yield return new WaitForSecondsRealtime(0.05f);

            UIPanelGameObject.transform.localScale = Vector3.zero;
        }
    }
    #endregion
}
