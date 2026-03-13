using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DronePanel : MonoBehaviour
{
    #region Inspector Fields
    public UnityEvent OnActivate;

    [SerializeField] private DroneHackingGame droneHackingGame;
    [SerializeField] private GameObject panelDeactivatedIndicator;
    [SerializeField] private GameObject panelActivatedIndicator;
    [SerializeField] private Image panelInteractionIndicator;
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
            panelInteractionIndicator.DOFade(0f, 0f);
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
            panelInteractionIndicator.DOFade(0f, 0f);
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
            panelInteractionIndicator.DOKill();
            panelInteractionIndicator.DOFade(0f, 0.2f);
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

        if (other.gameObject.GetComponent<DroneMovement>())
        {
            panelInteractionIndicator.DOKill();
            panelInteractionIndicator.DOFade(0f, 0.2f);
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
                panelInteractionIndicator.DOKill();
                panelInteractionIndicator.DOFade(0f, 0.2f);
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
        panelInteractionIndicator.DOKill();
        panelInteractionIndicator.DOFade(0f, 0.2f);
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
            panelInteractionIndicator.DOKill();
            panelInteractionIndicator.DOFade(1f, 0.2f);
        }
    }
    #endregion
}
