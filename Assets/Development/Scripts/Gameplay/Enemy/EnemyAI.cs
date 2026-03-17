using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    #region State Machines
    public enum EnemyState
    {
        Idle,
        Patrolling,
        Alerted,
        Combat
    }

    public enum CombatState
    {
        Idle,
        Chasing,
        Attacking,
        Cooldown
    }

    public enum AttackType
    {
        Melee,
        Ranged
    }

    private EnemyState currentState;
    private CombatState currentCombatState;
    #endregion

    #region Inspector Fields
    [Header("Movement Settings")]
    [SerializeField] private float speed;
    [SerializeField] private MovementBehaviour movementBehaviour;

    [Header("Perception Settings")]
    [SerializeField] private float detectionRange;

    [Header("Attack Settings")]
    [SerializeField] private AttackType attackType;
    #endregion

    private void Start()
    {
        currentState = EnemyState.Idle;
        currentCombatState = CombatState.Idle;
    }

    private void Update()
    {
        if (PlayerMovement.Instance != null)
        {
            movementBehaviour.Move(transform, PlayerMovement.Instance.transform, speed);
        }
    }
}
