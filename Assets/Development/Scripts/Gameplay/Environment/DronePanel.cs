using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class DronePanel : MonoBehaviour
{
    public UnityEvent OnActivate;

    [SerializeField] private DroneHackingGame droneHackingGame;
    [SerializeField] private bool requiresHacking;
    [SerializeField] private GameObject dronePresenceIndicator;
    [SerializeField] private GameObject panelActivatedIndicator;
    [SerializeField] private InputActionReference IA_DroneInteract;

    private bool droneDetected;
    private bool hackingComplete = false;
    private bool panelActivated = false;

    private void Start()
    {
        dronePresenceIndicator.SetActive(false);
        panelActivatedIndicator.SetActive(false);
        droneDetected = false;

        if (requiresHacking)
        {
            droneHackingGame.gameObject.SetActive(false);
            droneHackingGame.OnHackingComplete.AddListener(HackingComplete);
        }
    }

    private void OnEnable()
    {
        IA_DroneInteract.action.performed += (ctx) => ActivatePanel();
    }

    private void OnDisable()
    {
        IA_DroneInteract.action.performed -= (ctx) => ActivatePanel();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (panelActivated)
        {
            return;
        }

        if (other.gameObject.GetComponent<DroneMovement>())
        {
            dronePresenceIndicator.SetActive(true);
            droneDetected = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (panelActivated)
        {
            return;
        }

        if (other.gameObject.GetComponent<DroneMovement>())
        {
            dronePresenceIndicator.SetActive(false);
            droneDetected = false;
        }
    }

    private void ActivatePanel()
    {
        if (droneDetected)
        {
            if (requiresHacking && !hackingComplete)
            {
                droneHackingGame.gameObject.SetActive(true);
            }
            else
            {
                OnActivate?.Invoke();
                panelActivatedIndicator.SetActive(true);
            }
        }
    }

    private void HackingComplete()
    {
        hackingComplete = true;
        OnActivate?.Invoke();
        panelActivatedIndicator.SetActive(true);
    }
}
