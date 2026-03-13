using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class DronePanel : MonoBehaviour
{
    #region Inspector Fields
    public UnityEvent OnActivate;

    [SerializeField] private DroneHackingGame droneHackingGame;
    [SerializeField] private GameObject panelDeactivatedIndicator;
    [SerializeField] private GameObject panelActivatedIndicator;
    [SerializeField] private GameObject panelInteractionIndicator;
    [SerializeField] private InputActionReference IA_DroneInteract;
    [SerializeField] private float failureCooldownDuration = 1;
    [SerializeField] private bool requiresHacking;

    private GameObject detectedDrone;
    private bool droneDetected;
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
            panelInteractionIndicator.SetActive(false);
            if (requiresHacking)
            {
                droneHackingGame.gameObject.SetActive(false);
                droneHackingGame.OnHackingComplete.AddListener(HackingComplete);
                droneHackingGame.OnHackingFailed.AddListener(PanelCooldown);
            }
        }
        else
        {
            panelDeactivatedIndicator.SetActive(false);
            panelActivatedIndicator.SetActive(true);
            panelInteractionIndicator.SetActive(false);
        }

        droneDetected = false;
        detectedDrone = null;
    }

    private void OnEnable()
    {
        IA_DroneInteract.action.performed += OnDroneInteract;
    }

    private void OnDisable()
    {
        IA_DroneInteract.action.performed -= OnDroneInteract;
    }

    private void Update()
    {
        if (panelActivated)
        {
            return;
        }
        if (droneDetected && detectedDrone == null)
        {
            panelInteractionIndicator.SetActive(false);
            droneDetected = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (panelActivated)
        {
            return;
        }

        if (other.gameObject.GetComponent<DroneMovement>())
        {
            droneDetected = true;
            detectedDrone = other.gameObject;

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

        if (other.gameObject.GetComponent<DroneMovement>())
        {
            panelInteractionIndicator.SetActive(false);
            droneDetected = false;
            detectedDrone = null;
        }
    }
    #endregion

    #region Input Callbacks
    private void OnDroneInteract(InputAction.CallbackContext context)
    {
        ActivatePanel();
    }
    #endregion

    #region Custom Methods
    private void ActivatePanel()
    {
        if (droneDetected)
        {
            if (requiresHacking && !hackingComplete)
            {
                panelInteractionIndicator.SetActive(false);
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
        panelActivatedIndicator.SetActive(true);
        panelInteractionIndicator.SetActive(false);
        droneHackingGame.OnHackingComplete.RemoveListener(HackingComplete);
        droneHackingGame.OnHackingFailed.RemoveListener(PanelCooldown);

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

        if (droneDetected)
        {
            panelInteractionIndicator.SetActive(true);
        }
    }
    #endregion
}
