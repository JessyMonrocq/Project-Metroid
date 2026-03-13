using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HackingPanel : MonoBehaviour
{
    #region Inspector Fields
    public UnityEvent OnActivate;

    [SerializeField] private HackingGame hackingGame;
    [SerializeField] private GameObject panelDeactivatedIndicator;
    [SerializeField] private GameObject panelActivatedIndicator;
    [SerializeField] private Image panelInteractionIndicator;
    [SerializeField] private InputActionReference IA_PlayerInteract;
    [SerializeField] private float failureCooldownDuration = 1;

    private bool playerDetected;
    private bool hackingComplete = false;
    private bool panelActivated = false;
    private bool panelCooldown = false;

    public bool IsPanelActivated => panelActivated;
    #endregion

    #region Unity Methods
    private void Start()
    {
        if (!panelActivated)
        {
            panelDeactivatedIndicator.SetActive(true);
            panelActivatedIndicator.SetActive(false);
            panelInteractionIndicator.DOFade(0f, 0f);

            hackingGame.OnHackingComplete.AddListener(HackingComplete);
            hackingGame.OnHackingFailed.AddListener(PanelCooldown);
        } else
        {
            panelDeactivatedIndicator.SetActive(false);
            panelActivatedIndicator.SetActive(true);
            panelInteractionIndicator.DOFade(0f, 0f);
        }

        playerDetected = false;
        hackingGame.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        IA_PlayerInteract.action.performed += OnPlayerInteract;
    }

    private void OnDisable()
    {
        IA_PlayerInteract.action.performed -= OnPlayerInteract;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (panelActivated)
        {
            return;
        }

        if (other.gameObject.GetComponent<PlayerMovement>())
        {
            playerDetected = true;

            if (!hackingComplete && !panelActivated && !panelCooldown)
            {
                panelInteractionIndicator.DOKill();
                panelInteractionIndicator.DOFade(1f, 0.2f);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (panelActivated)
        {
            return;
        }

        if (other.gameObject.GetComponent<PlayerMovement>())
        {
            playerDetected = false;
            panelInteractionIndicator.DOKill();
            panelInteractionIndicator.DOFade(0f, 0.2f);
        }
    }
    #endregion

    #region Input Callbacks
    private void OnPlayerInteract(InputAction.CallbackContext context)
    {
        ActivatePanel();
    }
    #endregion

    #region Custom Methods
    private void ActivatePanel()
    {
        if (playerDetected && PlayerMovement.Instance.IsPlayerGrounded)
        {
            if (!hackingComplete && !panelCooldown)
            {
                panelInteractionIndicator.DOKill();
                panelInteractionIndicator.DOFade(0f, 0.2f);
                hackingGame.gameObject.SetActive(true);
            }
        }
    }

    private void HackingComplete()
    {
        hackingComplete = true;
        panelDeactivatedIndicator.SetActive(false);
        panelActivatedIndicator.SetActive(true);
        hackingGame.OnHackingComplete.RemoveListener(HackingComplete);
        hackingGame.OnHackingFailed.RemoveListener(PanelCooldown);

        OnActivate?.Invoke();
    }

    private void PanelCooldown()
    {
        StartCoroutine(WaitForCooldown());
    }
    #endregion

    #region Coroutine Methods
    private IEnumerator WaitForCooldown()
    {
        panelCooldown = true;
        yield return new WaitForSeconds(failureCooldownDuration);
        panelCooldown = false;

        if (playerDetected)
        {
            panelInteractionIndicator.DOKill();
            panelInteractionIndicator.DOFade(1f, 0.2f);
        }
    }
    #endregion
}
