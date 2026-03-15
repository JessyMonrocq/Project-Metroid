using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public enum PlayerState
    {
        Normal,
        Dashing,
        Grappling
    }

    #region Inspector Fields
    public UnityEvent<int> OnPlayerDirectionChanged;
    [HideInInspector]
    public UnityEvent OnPlayerCrouchJump;

    public static PlayerMovement Instance;

    [SerializeField] private CharacterController characterController;

    [Header("Input Action References")]
    [SerializeField] private InputActionReference IA_PlayerMove;
    [SerializeField] private InputActionReference IA_PlayerJump;
    [SerializeField] private InputActionReference IA_PlayerDash;
    [SerializeField] private InputActionReference IA_PlayerGrapple;
    private bool enableInput = true;

    [Header("Movement Settings")]
    [SerializeField] private float playerSpeed = 10f;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float deceleration = 50f;

    [Header("Slope Settings")]
    [SerializeField] private float slopeForce = 8f;
    [SerializeField] private float slopeForceRayLength = 1.5f;
    [SerializeField] private float slideSpeed = 10f;
    [SerializeField] private float slideMomentumDuration = 0.3f;

    [Header("Edge Sliding Settings")]
    [SerializeField] private float edgeSlipStrength = 10f;
    [SerializeField] private float edgeFriction = 0.95f;

    [Header("Jump Settings")]
    [SerializeField] private float playerJumpHeight;
    [SerializeField] private float jumpCutMultiplier = 0.5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float fallGravityMultiplier = 2f;
    [SerializeField] private float maxFallSpeed = -20f;
    [SerializeField] private float coyoteDuration = 0.2f;
    [SerializeField] private float jumpBufferDuration = 0.2f;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Grapple Settings")]
    [SerializeField] private float grappleHoldDuration = 0.3f;
    [SerializeField] private float grapplePullSpeed = 25f;
    [SerializeField] private float grappleAcceleration = 40f;
    [SerializeField] private float grappleMaxSpeed = 35f;
    [SerializeField] private float grapplePassDistance = 2f;
    [SerializeField] private float grappleMomentumMultiplier = 0.6f;

    [Header("Wall Jump Settings")]
    [SerializeField] private float wallCheckDistance = 0.6f;
    [SerializeField] private float wallSlideSpeed = -2f;
    [SerializeField] private float wallJumpHeight = 10f;
    [SerializeField] private float wallJumpHorizontalForce = 15f;
    [SerializeField] private float wallJumpInputLockDuration = 0.2f;
    [SerializeField] private LayerMask wallLayer;

    [Header("Abilites Settings")]
    [SerializeField] private bool canDoubleJump;
    [SerializeField] private bool canDash;
    [SerializeField] private bool canPhazeDash;
    [SerializeField] private bool canMultiDirectionDash;
    [SerializeField] private bool canWallJump;
    [SerializeField] private bool canStickToWalls;
    [SerializeField] private bool canGrapple;

    // State Machine
    private PlayerState currentState = PlayerState.Normal;

    private Vector2 currentInput;
    private Vector3 playerVelocity;
    private Vector3 horizontalVelocity;
    private Vector3 dashDirection;
    private Vector3 slideMomentum;
    private Vector3 edgeSlideVelocity;
    private Vector3 grappleVelocity;
    private Vector3 grappleDirection;
    private Transform currentGrapplePoint;

    private int playerDirection = 1;
    private int jumpsRemaining;
    private int wallDirection;

    private bool isPlayerGrounded;
    private bool isPlayerJumping;
    private bool isOnWall;
    private bool isOnSlipperyWall;
    private bool isSliding;
    private bool isGrappleHolding;
    private bool hasPassedGrapplePoint;

    private float coyoteTimer;
    private float jumpBufferTimer;
    private float dashDurationTimer;
    private float dashCooldownTimer;
    private float wallJumpInputLockTimer;
    private float wallSlideMultiplier;
    private float groundCheckTimer;
    private float slideMomentumTimer;
    private float currentGrappleSpeed;
    private float grappleHoldTimer;

    public int PlayerDirection => playerDirection;
    public bool IsPlayerGrounded => isPlayerGrounded;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
        }
        else {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        IA_PlayerMove.action.Enable();
        IA_PlayerJump.action.Enable();
        IA_PlayerDash.action.Enable();
        IA_PlayerGrapple.action.Enable();

        IA_PlayerJump.action.performed += OnJumpPerformed;
        IA_PlayerJump.action.canceled += OnJumpCanceled;
        IA_PlayerDash.action.performed += OnDashPerformed;
        IA_PlayerGrapple.action.performed += OnGrapplePerformed;
    }

    private void OnDisable()
    {
        IA_PlayerJump.action.performed -= OnJumpPerformed;
        IA_PlayerJump.action.canceled -= OnJumpCanceled;
        IA_PlayerDash.action.performed -= OnDashPerformed;
        IA_PlayerGrapple.action.performed -= OnGrapplePerformed;

        IA_PlayerMove.action.Disable();
        IA_PlayerJump.action.Disable();
        IA_PlayerDash.action.Disable();
        IA_PlayerGrapple.action.Disable();
    }
    #endregion

    #region Input Event Handlers
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (!enableInput) {
            return;
        }

        if (isPlayerGrounded && currentInput.y < -0.5f && Mathf.Abs(currentInput.x) < 0.4f) {
            OnPlayerCrouchJump?.Invoke();
            return;
        }

        jumpBufferTimer = jumpBufferDuration;
    }

    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        if (isPlayerJumping && playerVelocity.y > 0) {
            playerVelocity.y *= jumpCutMultiplier;
            isPlayerJumping = false;
        }
    }

    private void OnDashPerformed(InputAction.CallbackContext context)
    {
        if (!enableInput || !canDash || dashCooldownTimer > 0 || isOnWall || isSliding || currentState != PlayerState.Normal) {
            return;
        }

        StartDash(currentInput);
    }

    private void OnGrapplePerformed(InputAction.CallbackContext context)
    {
        if (!enableInput || !canGrapple || currentGrapplePoint == null || currentState == PlayerState.Grappling) {
            return;
        }

        StartGrappling();
    }
    #endregion

    #region Update Method
    private void Update()
    {
        if (!enableInput) {
            return;
        }

        currentInput = IA_PlayerMove.action.ReadValue<Vector2>();

        HandleDashCooldown();
        HandleWallJumpInputLock();

        switch (currentState) {
            case PlayerState.Dashing:
                HandleDash();
                return;
            case PlayerState.Grappling:
                HandleGrappling();
                return;
            case PlayerState.Normal:
                HandleNormalMovement();
                break;
        }
    }

    private void HandleNormalMovement()
    {
        isPlayerGrounded = characterController.isGrounded;

        if (canWallJump) {
            CheckForWall();
        }

        HandleCoyoteTiming();
        HandleJumpBufferTiming();

        Vector3 targetVelocity = new Vector3(currentInput.x, 0, 0) * playerSpeed;
        targetVelocity = Vector3.ClampMagnitude(targetVelocity, playerSpeed);

        if (wallJumpInputLockTimer <= 0 && !isSliding) {
            if (slideMomentumTimer > 0) {
                horizontalVelocity = slideMomentum;
                slideMomentumTimer -= Time.deltaTime;
                if (slideMomentumTimer <= 0) {
                    slideMomentum = Vector3.zero;
                }
            }
            else {
                float speedChange = (currentInput.x != 0) ? acceleration : deceleration;
                Vector3 targetWithEdgeSlide = targetVelocity + new Vector3(edgeSlideVelocity.x, 0, 0);
                horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetWithEdgeSlide, speedChange * Time.deltaTime);
            }
        }
        else if (isSliding) {
            horizontalVelocity = Vector3.zero;
        }
        else {
            slideMomentumTimer = 0;
            slideMomentum = Vector3.zero;
        }

        UpdatePlayerDirection();
        HandleJump();
        ApplyGravity();

        Vector3 finalMove = horizontalVelocity + Vector3.up * playerVelocity.y;

        if (isPlayerGrounded && !isPlayerJumping) {
            finalMove += ApplySlopeForce();
            ApplyEdgeSliding();
        }
        else {
            edgeSlideVelocity = Vector3.zero;
        }

        finalMove.z = 0;
        characterController.Move(finalMove * Time.deltaTime);

        if ((characterController.collisionFlags & CollisionFlags.Above) != 0) {
            playerVelocity.y = 0;
        }

        if (Mathf.Abs(transform.position.z) > 0.001f) {
            transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        }
    }
    #endregion

    #region Private Methods
    private void HandleDashCooldown()
    {
        if (dashCooldownTimer > 0) {
            dashCooldownTimer -= Time.deltaTime;
        }
    }

    private void HandleWallJumpInputLock()
    {
        if (wallJumpInputLockTimer > 0) {
            wallJumpInputLockTimer -= Time.deltaTime;
        }
    }

    private void HandleCoyoteTiming()
    {
        if (isPlayerGrounded) {
            if (playerVelocity.y < -2f) {
                playerVelocity.y = -2f;
            }
            isPlayerJumping = false;
            isOnWall = false;
            coyoteTimer = coyoteDuration;
            jumpsRemaining = canDoubleJump ? 2 : 1;
        }
        else if (coyoteTimer > 0) {
            coyoteTimer -= Time.deltaTime;
            if (coyoteTimer <= 0) {
                jumpsRemaining--;
                coyoteTimer = 0;
            }
        }
    }

    private void StartGrappling()
    {
        currentState = PlayerState.Grappling;
        isGrappleHolding = true;
        hasPassedGrapplePoint = false;
        grappleHoldTimer = grappleHoldDuration;
        currentGrappleSpeed = 0;

        playerVelocity = Vector3.zero;
        horizontalVelocity = Vector3.zero;
        grappleVelocity = Vector3.zero;

        grappleDirection = (currentGrapplePoint.position - transform.position).normalized;

        int newDirection = grappleDirection.x > 0 ? 1 : -1;
        if (newDirection != playerDirection) {
            playerDirection = newDirection;
            OnPlayerDirectionChanged?.Invoke(playerDirection);
        }

        transform.right = new Vector3(playerDirection, 0, 0);
    }

    private void HandleGrappling()
    {
        if (currentGrapplePoint == null) {
            EndGrapple();
            return;
        }

        if (isGrappleHolding) {
            grappleHoldTimer -= Time.deltaTime;
            if (grappleHoldTimer <= 0) {
                isGrappleHolding = false;
                currentGrappleSpeed = grapplePullSpeed;
                grappleDirection = (currentGrapplePoint.position - transform.position).normalized;
            }
            return;
        }

        float distanceToGrapple = Vector3.Distance(transform.position, currentGrapplePoint.position);

        if (!hasPassedGrapplePoint) {
            currentGrappleSpeed += grappleAcceleration * Time.deltaTime;
            currentGrappleSpeed = Mathf.Min(currentGrappleSpeed, grappleMaxSpeed);

            grappleVelocity = grappleDirection * currentGrappleSpeed;
            grappleVelocity.z = 0;

            characterController.Move(grappleVelocity * Time.deltaTime);

            if (distanceToGrapple <= grapplePassDistance) {
                hasPassedGrapplePoint = true;
                horizontalVelocity = new Vector3(grappleVelocity.x, 0, 0) * grappleMomentumMultiplier;
                playerVelocity.y = grappleVelocity.y * grappleMomentumMultiplier;
                EndGrapple();
            }
        }
    }

    private void EndGrapple()
    {
        currentState = PlayerState.Normal;
        isGrappleHolding = false;
        hasPassedGrapplePoint = false;
        grappleVelocity = Vector3.zero;
        currentGrappleSpeed = 0;
        grappleHoldTimer = 0;
    }

    private Vector3 ApplySlopeForce()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, slopeForceRayLength)) {
            if (hit.collider.gameObject.layer == 13) {
                return Vector3.zero;
            }

            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

            if (slopeAngle >= characterController.slopeLimit) {
                isSliding = true;
                Vector3 slideDirection = new Vector3(hit.normal.x, -hit.normal.y, hit.normal.z);
                Vector3 slideForce = slideDirection.normalized * slideSpeed;

                slideMomentum = new Vector3(slideForce.x, 0, 0);
                slideMomentumTimer = slideMomentumDuration;

                return slideForce;
            }
            else if (hit.normal != Vector3.up) {
                isSliding = false;
                slideMomentumTimer = 0;
                slideMomentum = Vector3.zero;
                return Vector3.down * slopeForce;
            }
        }

        isSliding = false;
        return Vector3.zero;
    }

    private void HandleJumpBufferTiming()
    {
        if (jumpBufferTimer > 0) {
            jumpBufferTimer -= Time.deltaTime;
        }
    }

    private void UpdatePlayerDirection()
    {
        if (horizontalVelocity.x != 0) {
            int newDirection = (int)Mathf.Sign(horizontalVelocity.x);
            if (newDirection != playerDirection) {
                playerDirection = newDirection;
                OnPlayerDirectionChanged?.Invoke(playerDirection);
            }
            transform.right = horizontalVelocity.normalized;
        }
    }

    private void HandleJump()
    {
        if (isOnWall && jumpBufferTimer > 0) {
            PerformWallJump();
        }
        else {
            bool canJump = (coyoteTimer > 0 || jumpsRemaining > 0) && jumpBufferTimer > 0 && !isSliding;

            if (canJump) {
                playerVelocity.y = Mathf.Sqrt(playerJumpHeight * -2f * gravity);
                isPlayerJumping = true;
                coyoteTimer = 0;
                jumpBufferTimer = 0;
                jumpsRemaining--;
            }
        }
    }

    private void ApplyGravity()
    {
        if (isOnWall && playerVelocity.y <= 0) {
            if (canStickToWalls) {
                playerVelocity.y = 0;
            }
            else {
                playerVelocity.y = wallSlideSpeed * wallSlideMultiplier;
            }
        }
        else {
            float appliedGravity = gravity;
            if (playerVelocity.y < 0) {
                appliedGravity *= fallGravityMultiplier;
            }

            playerVelocity.y += appliedGravity * Time.deltaTime;
            playerVelocity.y = Mathf.Max(playerVelocity.y, maxFallSpeed);
        }
    }

    private void CheckForWall()
    {
        if (isPlayerGrounded) {
            isOnWall = false;
            wallSlideMultiplier = 1f;
            isOnSlipperyWall = false;
            return;
        }

        bool rightWall = Physics.Raycast(transform.position, Vector3.right, wallCheckDistance, wallLayer);
        bool leftWall = Physics.Raycast(transform.position, Vector3.left, wallCheckDistance, wallLayer);

        if (rightWall) {
            isOnWall = true;
            wallDirection = 1;
        }
        else if (leftWall) {
            isOnWall = true;
            wallDirection = -1;
        }
        else {
            isOnWall = false;
            wallDirection = 0;
        }
    }

    private void PerformWallJump()
    {
        if (isOnSlipperyWall) {
            return;
        }

        playerVelocity.y = Mathf.Sqrt(wallJumpHeight * -2f * gravity);
        horizontalVelocity = new Vector3(-wallDirection * wallJumpHorizontalForce, 0, 0);
        wallJumpInputLockTimer = wallJumpInputLockDuration;

        isPlayerJumping = true;
        isOnWall = false;
        wallSlideMultiplier = 1f;
        isOnSlipperyWall = false;
        jumpBufferTimer = 0;
        jumpsRemaining = canDoubleJump ? 1 : 0;
    }

    private void StartDash(Vector2 input)
    {
        currentState = PlayerState.Dashing;
        dashDurationTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
        playerVelocity.y = 0;
        isOnWall = false;

        if (canPhazeDash) {
            this.gameObject.layer = 14;
        }

        if (input.x != 0) {
            int newDirection = (int)Mathf.Sign(input.x);
            if (newDirection != playerDirection) {
                playerDirection = newDirection;
                OnPlayerDirectionChanged?.Invoke(playerDirection);
            }
            transform.right = new Vector3(playerDirection, 0, 0);
        }

        if (canMultiDirectionDash) {
            if (input.x == 0) {
                dashDirection = new Vector3(playerDirection, 0, 0);
            }
            else {
                dashDirection = new Vector3(input.x, input.y, 0).normalized;
            }
        }
        else {
            dashDirection = new Vector3(playerDirection, 0, 0);
        }
    }

    private void HandleDash()
    {
        dashDurationTimer -= Time.deltaTime;

        if (dashDurationTimer <= 0) {
            currentState = PlayerState.Normal;
            if (canPhazeDash) {
                this.gameObject.layer = 9;
            }
            return;
        }

        Vector3 dashMove = dashDirection * dashSpeed;
        dashMove.z = 0;
        characterController.Move(dashMove * Time.deltaTime);
    }

    private void ApplyEdgeSliding()
    {
        if (Physics.SphereCast(transform.position, characterController.radius, Vector3.down, out RaycastHit hit, slopeForceRayLength)) {
            if (hit.collider.gameObject.layer == 13) {
                return;
            }

            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

            if (slopeAngle < characterController.slopeLimit) {
                Vector3 slopeDirection = Vector3.ProjectOnPlane(Vector3.down, hit.normal);
                edgeSlideVelocity += slopeDirection * Vector3.Dot(Vector3.down, slopeDirection) * Mathf.Abs(gravity) * edgeSlipStrength * Time.deltaTime;
                edgeSlideVelocity *= edgeFriction;
            }
        }
        else {
            edgeSlideVelocity = Vector3.zero;
        }
    }
    #endregion

    #region Public Methods
    public void EnableInput(bool state)
    {
        enableInput = state;
    }

    public void ForceJump()
    {
        if (currentState == PlayerState.Dashing) {
            return;
        }

        playerVelocity.y = Mathf.Sqrt(playerJumpHeight * -2f * gravity);
        isPlayerJumping = true;
        coyoteTimer = 0;
        jumpBufferTimer = 0;
        jumpsRemaining--;
    }

    public void PushPlayer(Vector3 direction, float force)
    {
        horizontalVelocity += direction.normalized * force;
    }

    public void IncreaseWallSlideIntensity(bool state)
    {
        if (canStickToWalls) {
            return;
        }

        wallSlideMultiplier = state ? 3f : 1f;
        isOnSlipperyWall = state;
    }

    public void SetGrapplePoint(Transform grapplePoint)
    {
        currentGrapplePoint = grapplePoint;
    }
    #endregion
}
