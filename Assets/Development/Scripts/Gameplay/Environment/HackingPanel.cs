using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class HackingPanel : MonoBehaviour
{
    public UnityEvent OnActivate;

    [SerializeField] private HackingGame hackingGame;
    [SerializeField] private GameObject panelDeactivatedIndicator;
    [SerializeField] private GameObject panelActivatedIndicator;
    [SerializeField] private InputActionReference IA_PlayerInteract;

    private bool playerDetected;
    private bool hackingComplete = false;
    private bool panelActivated = false;

    private void Start()
    {
        panelDeactivatedIndicator.SetActive(true);
        panelActivatedIndicator.SetActive(false);
        playerDetected = false;

        hackingGame.gameObject.SetActive(false);
        hackingGame.OnHackingComplete.AddListener(HackingComplete);
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
        }
    }

    private void ActivatePanel()
    {
        if (playerDetected && PlayerMovement.Instance.IsPlayerGrounded)
        {
            if (!hackingComplete)
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
}
