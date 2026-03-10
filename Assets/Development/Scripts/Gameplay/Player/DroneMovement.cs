using UnityEngine;
using UnityEngine.InputSystem;

public class DroneMovement : MonoBehaviour
{
    #region Inspector Fields
    [SerializeField] private Rigidbody droneRigidbody;

    [Header("Input Action References")]
    [SerializeField] private InputActionReference IA_DroneMove;

    [Header("Movement Settings")]
    [SerializeField] private float droneSpeed = 5f;
    [SerializeField] private float droneMaxSpeed = 8f;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float deceleration = 50f;
    [SerializeField] private float movementOffset = 0.25f;

    private Vector2 currentInput;
    #endregion

    #region Unity Methods
    private void OnEnable()
    {
        IA_DroneMove.action.Enable();
    }

    private void OnDisable()
    {
        IA_DroneMove.action.Disable();
    }
    #endregion

    #region Update Method
    private void Update()
    {
        Vector2 input = IA_DroneMove.action.ReadValue<Vector2>();
        if (Mathf.Abs(input.x) < movementOffset)
        {
            input = new Vector2(0, input.y);
        }
        if (Mathf.Abs(input.y) < movementOffset)
        {
            input = new Vector2(input.x, 0);
        }

        currentInput = input;
    }

    private void FixedUpdate()
    {
        Vector3 targetVelocity = new Vector3(currentInput.x, currentInput.y, 0) * droneSpeed;
        Vector3 currentVelocity = droneRigidbody.linearVelocity;

        Vector3 newVelocity;
        if (currentInput.magnitude > 0)
        {
            newVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            newVelocity = Vector3.MoveTowards(currentVelocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
        }

        if (newVelocity.magnitude > droneMaxSpeed)
        {
            newVelocity = newVelocity.normalized * droneMaxSpeed;
        }

        droneRigidbody.linearVelocity = newVelocity;
    }
    #endregion
}
