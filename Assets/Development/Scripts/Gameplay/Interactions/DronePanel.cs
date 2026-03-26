using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DronePanel : Interactable
{
    #region Inspector Fields
    [Header("Activation Object")]
    [SerializeField] private ActivationCamera activationObject;

    [Header("References")]
    [SerializeField] private DroneHackingGame droneHackingGame;
    [SerializeField] private GameObject panelDeactivatedIndicator;
    [SerializeField] private GameObject panelActivatedIndicator;
    [SerializeField] private Image panelInteractionIndicator;
    [SerializeField] private float failureCooldownDuration = 1;
    [SerializeField] private bool requiresHacking;

    private GameObject detectedDrone;
    private bool droneDetected;
    private bool hackingComplete = false;
    private bool panelActivated = false;
    private bool panelCooldown = false;

    public bool IsPanelActivated => panelActivated;
    #endregion

    #region Interactable overrides
    public override void Interact(bool state)
    {
        interactionState = true;
        SceneManagement.Instance.UpdateInteractableState(InteractableID, interactionState);
    }

    public override void InitializeInteraction(bool interacted)
    {
        panelActivated = interacted;
        if (!panelActivated)
        {
            panelDeactivatedIndicator.SetActive(true);
            panelActivatedIndicator.SetActive(false);
            panelInteractionIndicator.DOFade(0f, 0f);

            droneHackingGame.OnHackingComplete.AddListener(HackingComplete);
            droneHackingGame.OnHackingFailed.AddListener(PanelCooldown);
        }
        else
        {
            panelDeactivatedIndicator.SetActive(false);
            panelActivatedIndicator.SetActive(true);
            panelInteractionIndicator.DOFade(0f, 0f);
        }

        droneDetected = false;
        droneHackingGame.gameObject.SetActive(false);

        InputManager.Instance.DroneInteract.performed += OnDroneInteract;
    }
    #endregion

    #region Unity Methods
    private void OnDisable()
    {
        InputManager.Instance.DroneInteract.performed -= OnDroneInteract;
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
        if (requiresHacking && !PlayerController.Instance.CanDroneHack)
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
        if (requiresHacking && !PlayerController.Instance.CanDroneHack)
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

                InputManager.Instance.SetDroneInputState(false);

                droneHackingGame.gameObject.SetActive(true);
            }
            else
            {
                activationObject.Activate(ActivationCamera.Hacker.Drone);
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

        Interact(true);

        InputManager.Instance.SetDroneInputState(true);

        activationObject.Activate(ActivationCamera.Hacker.Drone);
    }

    private void PanelCooldown()
    {
        StartCoroutine(WaitForCooldown());
    }
    #endregion

    #region Coroutine Methods
    private IEnumerator WaitForCooldown()
    {
        InputManager.Instance.SetDroneInputState(false);

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
