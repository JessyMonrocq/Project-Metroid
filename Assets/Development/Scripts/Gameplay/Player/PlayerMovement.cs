using Unity.Cinemachine;
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
    [HideInInspector]
    public UnityEvent<bool> OnPlayerAiming;

    public static PlayerMovement Instance { get; private set; }

    [SerializeField] private CharacterController characterController;

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
    private bool isPlayerAiming;
    private bool isPlayerJumping;
    private bool isOnWall;
    private bool isOnSlipperyWall;
    private bool isSliding;
    private bool isGrappleHolding;
    private bool hisHookedToGrapplePoint;

    private float coyoteTimer;
    private float jumpBufferTimer;
    private float dashDurationTimer;
    private float dashCooldownTimer;
    private float wallJumpInputLockTimer;
    private float wallSlideMultiplier;
    private float slideMomentumTimer;
    private float currentGrappleSpeed;
    private float grappleHoldTimer;
    private float initialGrappleDistance;

    private float fallSpeedYDampChangeThreshold;
    public int PlayerDirection => playerDirection;
    public bool IsPlayerGrounded => isPlayerGrounded;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        isPlayerGrounded = false;
        isPlayerAiming = false;
        isPlayerJumping = false;
        isOnWall = false;
        isOnSlipperyWall = false;
        isSliding = false;
        isGrappleHolding = false;
        hisHookedToGrapplePoint = false;

        coyoteTimer = 0;
        jumpBufferTimer = 0;
        dashDurationTimer = 0;
        dashCooldownTimer = 0;
        wallJumpInputLockTimer = 0;

        fallSpeedYDampChangeThreshold = CameraInterpolation.Instance.fallSpeedYDampingChangeThreshold;
    }

    private void OnEnable()
    {
        InputManager.Instance.PlayerAim.performed += OnAimingPerformed;
        InputManager.Instance.PlayerAim.canceled += OnAimingCanceled;
        InputManager.Instance.PlayerJump.performed += OnJumpPerformed;
        InputManager.Instance.PlayerJump.canceled += OnJumpCanceled;
        InputManager.Instance.PlayerDash.performed += OnDashPerformed;
        InputManager.Instance.PlayerGrapple.performed += OnGrapplePerformed;
    }

    private void OnDisable()
    {
        if (InputManager.Instance == null)
        {
            return;
        }

        InputManager.Instance.PlayerAim.performed -= OnAimingPerformed;
        InputManager.Instance.PlayerAim.canceled -= OnAimingCanceled;
        InputManager.Instance.PlayerJump.performed -= OnJumpPerformed;
        InputManager.Instance.PlayerJump.canceled -= OnJumpCanceled;
        InputManager.Instance.PlayerDash.performed -= OnDashPerformed;
        InputManager.Instance.PlayerGrapple.performed -= OnGrapplePerformed;
    }
    #endregion

    #region Input Event Handlers
    private void OnAimingPerformed(InputAction.CallbackContext context)
    {
        if (!isPlayerGrounded && !isOnWall && !hisHookedToGrapplePoint)
        {
            return;
        }

        isPlayerAiming = true;
        OnPlayerAiming?.Invoke(isPlayerAiming);
    }

    private void OnAimingCanceled(InputAction.CallbackContext context)
    {
        isPlayerAiming = false;
        OnPlayerAiming?.Invoke(isPlayerAiming);
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        isPlayerAiming = false;
        OnPlayerAiming?.Invoke(isPlayerAiming);

        if (currentState == PlayerState.Grappling && hisHookedToGrapplePoint)
        {
            if (currentInput.y < -0.5f && Mathf.Abs(currentInput.x) < 0.4f)
            {
                jumpsRemaining--;
                EndGrapple();
            }
            else
            {
                jumpsRemaining--;
                playerVelocity.y = Mathf.Sqrt(playerJumpHeight * -2f * gravity);
                if (Mathf.Abs(currentInput.x) > 0.1f)
                {
                    horizontalVelocity = new Vector3(Mathf.Sign(currentInput.x) * playerSpeed, 0, 0);
                }
                isPlayerJumping = true;
                EndGrapple();
            }
            return;
        }

        if (isPlayerGrounded && currentInput.y < -0.5f && Mathf.Abs(currentInput.x) < 0.4f)
        {
            OnPlayerCrouchJump?.Invoke();
            return;
        }

        jumpBufferTimer = jumpBufferDuration;
    }

    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        if (isPlayerJumping && playerVelocity.y > 0)
        {
            playerVelocity.y *= jumpCutMultiplier;
            isPlayerJumping = false;
        }
    }

    private void OnDashPerformed(InputAction.CallbackContext context)
    {
        if (!canDash || dashCooldownTimer > 0 || isOnWall || isSliding || currentState != PlayerState.Normal)
        {
            return;
        }

        isPlayerAiming = false;
        OnPlayerAiming?.Invoke(isPlayerAiming);

        StartDash(currentInput);
    }

    private void OnGrapplePerformed(InputAction.CallbackContext context)
    {
        if (isPlayerAiming || !canGrapple || currentGrapplePoint == null || currentState == PlayerState.Grappling)
        {
            return;
        }

        StartGrappling();
    }
    #endregion

    #region Update Method
    private void Update()
    {
        currentInput = InputManager.Instance.PlayerMove.ReadValue<Vector2>();

        HandleDashCooldown();
        HandleWallJumpInputLock();

        switch (currentState)
        {
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

        if (!isPlayerGrounded)
        {
            if (playerVelocity.y < fallSpeedYDampChangeThreshold && !CameraInterpolation.Instance.IsLerpingYDamping && !CameraInterpolation.Instance.LerpedFromPlayerFalling)
            {
                CameraInterpolation.Instance.LerpYDamping(true);
            }
        }
        else
        {
            if (!CameraInterpolation.Instance.IsLerpingYDamping && CameraInterpolation.Instance.LerpedFromPlayerFalling)
            {
                CameraInterpolation.Instance.LerpedFromPlayerFalling = false;
                CameraInterpolation.Instance.LerpYDamping(false);
            }
        }

        if (canWallJump)
        {
            CheckForWall();
        }

        HandleCoyoteTiming();
        HandleJumpBufferTiming();

        float activeInputX = isPlayerAiming ? 0 : currentInput.x;

        Vector3 targetVelocity = new Vector3(activeInputX, 0, 0) * playerSpeed;
        targetVelocity = Vector3.ClampMagnitude(targetVelocity, playerSpeed);

        if (wallJumpInputLockTimer <= 0 && !isSliding)
        {
            if (slideMomentumTimer > 0)
            {
                horizontalVelocity = slideMomentum;
                slideMomentumTimer -= Time.deltaTime;
                if (slideMomentumTimer <= 0)
                {
                    slideMomentum = Vector3.zero;
                }
            }
            else
            {
                float speedChange = (activeInputX != 0) ? acceleration : deceleration;
                Vector3 targetWithEdgeSlide = targetVelocity + new Vector3(edgeSlideVelocity.x, 0, 0);
                horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetWithEdgeSlide, speedChange * Time.deltaTime);
            }
        }
        else if (isSliding)
        {
            horizontalVelocity = Vector3.zero;
        }
        else
        {
            slideMomentumTimer = 0;
            slideMomentum = Vector3.zero;
        }

        UpdatePlayerDirection();
        HandleJump();
        ApplyGravity();

        Vector3 finalMove = horizontalVelocity + Vector3.up * playerVelocity.y;

        if (isPlayerGrounded && !isPlayerJumping)
        {
            finalMove += ApplySlopeForce();
            ApplyEdgeSliding();
        }
        else
        {
            edgeSlideVelocity = Vector3.zero;
        }

        finalMove.z = 0;
        characterController.Move(finalMove * Time.deltaTime);

        if ((characterController.collisionFlags & CollisionFlags.Above) != 0)
        {
            playerVelocity.y = 0;
        }

        if (Mathf.Abs(transform.position.z) > 0.001f)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        }
    }
    #endregion

    #region Private Methods
    private void HandleDashCooldown()
    {
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }

    private void HandleWallJumpInputLock()
    {
        if (wallJumpInputLockTimer > 0)
        {
            wallJumpInputLockTimer -= Time.deltaTime;
        }
    }

    private void HandleCoyoteTiming()
    {
        if (isPlayerGrounded)
        {
            if (playerVelocity.y < -2f)
            {
                playerVelocity.y = -2f;
            }
            isPlayerJumping = false;
            isOnWall = false;
            coyoteTimer = coyoteDuration;
            jumpsRemaining = canDoubleJump ? 2 : 1;
        }
        else if (coyoteTimer > 0)
        {
            coyoteTimer -= Time.deltaTime;
            if (coyoteTimer <= 0)
            {
                jumpsRemaining--;
                coyoteTimer = 0;
            }
        }
    }

    private void StartGrappling()
    {
        currentState = PlayerState.Grappling;
        isGrappleHolding = true;
        hisHookedToGrapplePoint = false;
        grappleHoldTimer = grappleHoldDuration;
        currentGrappleSpeed = 0;

        playerVelocity = Vector3.zero;
        horizontalVelocity = Vector3.zero;
        grappleVelocity = Vector3.zero;

        grappleDirection = (currentGrapplePoint.position - transform.position).normalized;

        int newDirection = grappleDirection.x > 0 ? 1 : -1;
        if (newDirection != playerDirection)
        {
            playerDirection = newDirection;
            OnPlayerDirectionChanged?.Invoke(playerDirection);
        }

        transform.right = new Vector3(playerDirection, 0, 0);
    }

    private void HandleGrappling()
    {
        if (currentGrapplePoint == null)
        {
            EndGrapple();
            return;
        }

        if (hisHookedToGrapplePoint)
        {
            playerVelocity = Vector3.zero;
            grappleVelocity = Vector3.zero;

            UpdatePlayerDirection();
            return;
        }

        if (isGrappleHolding)
        {
            grappleHoldTimer -= Time.deltaTime;
            if (grappleHoldTimer <= 0)
            {
                isGrappleHolding = false;
                currentGrappleSpeed = 0f;
                grappleDirection = (currentGrapplePoint.position - transform.position).normalized;
                initialGrappleDistance = Vector3.Distance(transform.position, currentGrapplePoint.position);
            }
            return;
        }

        float distanceToGrapple = Vector3.Distance(transform.position, currentGrapplePoint.position);

        if (distanceToGrapple > initialGrappleDistance / 2f)
        {
            currentGrappleSpeed += grappleAcceleration * Time.deltaTime;
            currentGrappleSpeed = Mathf.Min(currentGrappleSpeed, grappleMaxSpeed);
        }
        else
        {
            currentGrappleSpeed -= grappleAcceleration * Time.deltaTime;
            currentGrappleSpeed = Mathf.Max(currentGrappleSpeed, grapplePullSpeed / 2f);
        }

        grappleVelocity = grappleDirection * currentGrappleSpeed;
        grappleVelocity.z = 0;

        characterController.Move(grappleVelocity * Time.deltaTime);

        if (distanceToGrapple <= 0.5f)
        {
            transform.position = currentGrapplePoint.position;

            hisHookedToGrapplePoint = true;
            playerVelocity = Vector3.zero;
            horizontalVelocity = Vector3.zero;

            jumpsRemaining = canDoubleJump ? 2 : 1;
        }
    }

    private void EndGrapple()
    {
        currentState = PlayerState.Normal;
        isGrappleHolding = false;
        hisHookedToGrapplePoint = false;
        grappleVelocity = Vector3.zero;
        currentGrappleSpeed = 0;
        grappleHoldTimer = 0;
    }

    private Vector3 ApplySlopeForce()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        float checkLength = slopeForceRayLength + 0.1f;

        bool sphereHits = Physics.SphereCast(rayOrigin, characterController.radius, Vector3.down, out RaycastHit sphereHit, checkLength);
        bool centerHits = Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit centerHit, checkLength);
        if (sphereHits && sphereHit.collider.gameObject.layer != 13)
        {
            float sphereAngle = Vector3.Angle(sphereHit.normal, Vector3.up);
            float centerAngle = centerHits ? Vector3.Angle(centerHit.normal, Vector3.up) : 0f;

            bool isGenuineSlope = centerHits && Mathf.Abs(sphereAngle - centerAngle) < 2f;

            if (isGenuineSlope)
            {
                if (centerAngle >= characterController.slopeLimit)
                {
                    isSliding = true;

                    Vector3 slideDirection = Vector3.ProjectOnPlane(Vector3.down, centerHit.normal).normalized;
                    Vector3 slideForce = (slideDirection + Vector3.down * 0.1f) * slideSpeed;

                    slideMomentum = new Vector3(slideForce.x, 0, 0);
                    slideMomentumTimer = slideMomentumDuration;

                    return slideForce;
                }
                else if (centerAngle > 0.1f)
                {
                    isSliding = false;
                    slideMomentumTimer = 0;
                    slideMomentum = Vector3.zero;
                    return Vector3.down * slopeForce;
                }
            }
        }

        isSliding = false;
        return Vector3.zero;
    }

    private void HandleJumpBufferTiming()
    {
        if (jumpBufferTimer > 0)
        {
            jumpBufferTimer -= Time.deltaTime;
        }
    }

    private void UpdatePlayerDirection()
    {
        float directionCheck = (isPlayerAiming || hisHookedToGrapplePoint) ? currentInput.x : horizontalVelocity.x;

        if (directionCheck != 0)
        {
            int newDirection = (int)Mathf.Sign(directionCheck);
            if (newDirection != playerDirection)
            {
                playerDirection = newDirection;
                OnPlayerDirectionChanged?.Invoke(playerDirection);
            }

            transform.right = new Vector3(playerDirection, 0, 0);
        }
    }

    private void HandleJump()
    {
        if (isOnWall && jumpBufferTimer > 0)
        {
            PerformWallJump();
        }
        else
        {
            bool canJump = (coyoteTimer > 0 || jumpsRemaining > 0) && jumpBufferTimer > 0 && !isSliding;

            if (canJump)
            {
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
        if (isOnWall && playerVelocity.y <= 0)
        {
            if (canStickToWalls)
            {
                playerVelocity.y = 0;
            }
            else
            {
                playerVelocity.y = wallSlideSpeed * wallSlideMultiplier;
            }
        }
        else
        {
            float appliedGravity = gravity;
            if (playerVelocity.y < 0)
            {
                appliedGravity *= fallGravityMultiplier;
            }

            playerVelocity.y += appliedGravity * Time.deltaTime;
            playerVelocity.y = Mathf.Max(playerVelocity.y, maxFallSpeed);
        }
    }

    private void CheckForWall()
    {
        if (isPlayerGrounded)
        {
            isOnWall = false;
            wallSlideMultiplier = 1f;
            isOnSlipperyWall = false;
            return;
        }

        bool rightWall = Physics.Raycast(transform.position, Vector3.right, wallCheckDistance, wallLayer);
        bool leftWall = Physics.Raycast(transform.position, Vector3.left, wallCheckDistance, wallLayer);

        if (rightWall)
        {
            isOnWall = true;
            wallDirection = 1;
        }
        else if (leftWall)
        {
            isOnWall = true;
            wallDirection = -1;
        }
        else
        {
            isOnWall = false;
            wallDirection = 0;
        }
    }

    private void PerformWallJump()
    {
        if (isOnSlipperyWall)
        {
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

        if (canPhazeDash)
        {
            this.gameObject.layer = 14;
        }

        if (input.x != 0)
        {
            int newDirection = (int)Mathf.Sign(input.x);
            if (newDirection != playerDirection)
            {
                playerDirection = newDirection;
                OnPlayerDirectionChanged?.Invoke(playerDirection);
            }
            transform.right = new Vector3(playerDirection, 0, 0);
        }

        if (canMultiDirectionDash)
        {
            if (input.x == 0)
            {
                dashDirection = new Vector3(playerDirection, 0, 0);
            }
            else
            {
                dashDirection = new Vector3(input.x, input.y, 0).normalized;
            }
        }
        else
        {
            dashDirection = new Vector3(playerDirection, 0, 0);
        }
    }

    private void HandleDash()
    {
        dashDurationTimer -= Time.deltaTime;

        if (dashDurationTimer <= 0)
        {
            currentState = PlayerState.Normal;
            if (canPhazeDash)
            {
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
        if (isSliding)
        {
            edgeSlideVelocity = Vector3.zero;
            return;
        }

        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        float checkLength = slopeForceRayLength + 0.1f;

        bool sphereHits = Physics.SphereCast(rayOrigin, characterController.radius, Vector3.down, out RaycastHit sphereHit, checkLength);
        bool centerHits = Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit centerHit, checkLength);

        if (sphereHits && sphereHit.collider.gameObject.layer != 13)
        {
            float sphereAngle = Vector3.Angle(sphereHit.normal, Vector3.up);
            float centerAngle = centerHits ? Vector3.Angle(centerHit.normal, Vector3.up) : 0f;

            bool isGenuineSlope = centerHits && Mathf.Abs(sphereAngle - centerAngle) < 2f;

            if (!isGenuineSlope && sphereAngle > 0.1f)
            {
                Vector3 slideDirection = Vector3.ProjectOnPlane(Vector3.down, sphereHit.normal);

                edgeSlideVelocity += slideDirection * Vector3.Dot(Vector3.down, slideDirection) * Mathf.Abs(gravity) * edgeSlipStrength * Time.deltaTime;
                edgeSlideVelocity *= edgeFriction;
                return;
            }
        }

        edgeSlideVelocity = Vector3.zero;
    }
    #endregion

    #region Public Methods
    public void ForceJump()
    {
        if (currentState == PlayerState.Dashing)
        {
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
        if (canStickToWalls)
        {
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
