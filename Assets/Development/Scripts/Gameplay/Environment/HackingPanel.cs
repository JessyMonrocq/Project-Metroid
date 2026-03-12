using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class HackingPanel : MonoBehaviour
{
    public UnityEvent OnActivate;

    [SerializeField] private HackingGame hackingGame;
    [SerializeField] private GameObject panelDeactivatedIndicator;
    [SerializeField] private GameObject panelActivatedIndicator;
    [SerializeField] private GameObject panelInteractionIndicator;
    [SerializeField] private InputActionReference IA_PlayerInteract;
    [SerializeField] private float failureCooldownDuration = 3;

    private bool playerDetected;
    private bool hackingComplete = false;
    private bool panelActivated = false;
    private bool panelCooldown = false;

    private void Start()
    {
        panelDeactivatedIndicator.SetActive(true);
        panelActivatedIndicator.SetActive(false);
        panelInteractionIndicator.SetActive(false);
        playerDetected = false;

        hackingGame.gameObject.SetActive(false);
        hackingGame.OnHackingComplete.AddListener(HackingComplete);
        hackingGame.OnHackingFailed.AddListener(PanelCooldown);
    }

    private void OnEnable()
    {
        IA_PlayerInteract.action.performed += (ctx) => ActivatePanel();
    }

    private void OnDisable()
    {
        IA_PlayerInteract.action.performed -= (ctx) => ActivatePanel();
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
                panelInteractionIndicator.SetActive(true);
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
            panelInteractionIndicator.SetActive(false);
        }
    }

    private void ActivatePanel()
    {
        if (playerDetected && PlayerMovement.Instance.IsPlayerGrounded)
        {
            if (!hackingComplete && !panelCooldown)
            {
                hackingGame.gameObject.SetActive(true);
            }
        }
    }

    private void HackingComplete()
    {
        hackingComplete = true;
        OnActivate?.Invoke();
        panelDeactivatedIndicator.SetActive(false);
        panelActivatedIndicator.SetActive(true);
    }

    private void PanelCooldown()
    {
        StartCoroutine(WaitForCooldown());
    }

    private IEnumerator WaitForCooldown()
    {
        panelCooldown = true;
        yield return new WaitForSeconds(failureCooldownDuration);
        panelCooldown = false;

        if (playerDetected)
        {
            panelInteractionIndicator.SetActive(true);
        }
    }
}
