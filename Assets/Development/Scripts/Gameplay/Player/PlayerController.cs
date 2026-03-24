using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [Header("Player References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerWeapon playerWeapon;
    [SerializeField] private DroneManager droneManager;
    [SerializeField] private PlayerAbilitiesSO playerAbilities;

    public Transform playerTransform => transform;
    public bool IsPlayerGrounded => playerMovement.IsPlayerGrounded;
    public UnityEvent OnPlayerCrouchJump => playerMovement.OnPlayerCrouchJump;
    public UnityEvent<bool> OnPlayerAiming => playerMovement.OnPlayerAiming;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        UpdatePlayerAbilities();
    }

    public void SetPlayerAbility(PlayerMovement.PlayerAbility ability, bool isEnabled)
    {
        playerAbilities.SetMovementAbility(ability, isEnabled);
        UpdatePlayerAbilities();
    }

    public void SetDroneAbility(DroneManager.DroneAbility ability, bool isEnabled)
    {
        playerAbilities.SetDroneAbility(ability, isEnabled);
        UpdateDroneAbilities();
    }

    private void UpdatePlayerAbilities()
    {
        playerMovement.SetPlayerAbility(PlayerMovement.PlayerAbility.DoubleJump, playerAbilities.canDoubleJump);
        playerMovement.SetPlayerAbility(PlayerMovement.PlayerAbility.Dash, playerAbilities.canDash);
        playerMovement.SetPlayerAbility(PlayerMovement.PlayerAbility.PhazeDash, playerAbilities.canPhazeDash);
        playerMovement.SetPlayerAbility(PlayerMovement.PlayerAbility.MultiDirectionDash, playerAbilities.canMultiDirectionDash);
        playerMovement.SetPlayerAbility(PlayerMovement.PlayerAbility.WallJump, playerAbilities.canWallJump);
        playerMovement.SetPlayerAbility(PlayerMovement.PlayerAbility.StickToWalls, playerAbilities.canStickToWalls);
        playerMovement.SetPlayerAbility(PlayerMovement.PlayerAbility.Grapple, playerAbilities.canGrapple);
    }

    private void UpdateDroneAbilities()
    {
        droneManager.SetDroneAbility(DroneManager.DroneAbility.Spawn, playerAbilities.canSpawnDrone);
        //...
        //...
    }
}
