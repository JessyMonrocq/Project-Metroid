using UnityEngine;

public abstract class MovementBehaviour : ScriptableObject
{
    public abstract void Move(Transform enemyTransform, Transform playerTransform, float moveSpeed);
}
