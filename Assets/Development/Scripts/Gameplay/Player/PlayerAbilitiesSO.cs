using UnityEngine;

[CreateAssetMenu(menuName = "PlayerAbilitiesSO")]
public class PlayerAbilitiesSO : ScriptableObject
{
    public bool canDoubleJump;
    public bool canDash;
    public bool canPhazeDash;
    public bool canMultiDirectionDash;
    public bool canWallJump;
    public bool canStickToWalls;
    public bool canGrapple;
    public bool canSpawnDrone;

    public void SetMovementAbility(PlayerMovement.PlayerAbility ability, bool isEnabled)
    {
        switch (ability)
        {
            case PlayerMovement.PlayerAbility.DoubleJump:
                canDoubleJump = isEnabled;
                break;
            case PlayerMovement.PlayerAbility.Dash:
                canDash = isEnabled;
                break;
            case PlayerMovement.PlayerAbility.PhazeDash:
                canPhazeDash = isEnabled;
                break;
            case PlayerMovement.PlayerAbility.MultiDirectionDash:
                canMultiDirectionDash = isEnabled;
                break;
            case PlayerMovement.PlayerAbility.WallJump:
                canWallJump = isEnabled;
                break;
            case PlayerMovement.PlayerAbility.StickToWalls:
                canStickToWalls = isEnabled;
                break;
            case PlayerMovement.PlayerAbility.Grapple:
                canGrapple = isEnabled;
                break;
        }
    }

    public void SetDroneAbility(DroneManager.DroneAbility ability, bool isEnabled)
    {
        switch (ability)
        {
            case DroneManager.DroneAbility.Spawn:
                canSpawnDrone = isEnabled;
                break;
            //...
        }
    }
}
