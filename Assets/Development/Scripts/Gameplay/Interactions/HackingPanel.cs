using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HackingPanel : MonoBehaviour
{
    #region Inspector Fields
    [Header("Activation Object")]
    [SerializeField] private ActivationCamera activationObject;

    [Header("References")]
    [SerializeField] private HackingGame hackingGame;
    [SerializeField] private GameObject panelDeactivatedIndicator;
    [SerializeField] private GameObject panelActivatedIndicator;
    [SerializeField] private Image panelInteractionIndicator;
    [SerializeField] private float failureCooldownDuration = 1;
    [SerializeField] private bool panelActivated = false;

    private bool playerDetected;
    private bool hackingComplete = false;
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

        InputManager.Instance.PlayerInteract.performed += OnPlayerInteract;

    }

    private void OnDisable()
    {
        InputManager.Instance.PlayerInteract.performed -= OnPlayerInteract;
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
        if (playerDetected && PlayerController.Instance.IsPlayerGrounded)
        {
            if (!hackingComplete && !panelCooldown)
            {
                panelInteractionIndicator.DOKill();
                panelInteractionIndicator.DOFade(0f, 0.2f);

                InputManager.Instance.SetPlayerInputState(false);

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

        InputManager.Instance.SetPlayerInputState(false);

        activationObject.Activate(ActivationCamera.Hacker.Player);
    }

    private void PanelCooldown()
    {
        StartCoroutine(WaitForCooldownCoroutine());
    }
    #endregion

    #region Coroutine Methods
    private IEnumerator WaitForCooldownCoroutine()
    {
        InputManager.Instance.SetPlayerInputState(true);

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
