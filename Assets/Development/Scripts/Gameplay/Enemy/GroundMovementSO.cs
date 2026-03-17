using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Movement/Ground")]
public class GroundMovementSO : MovementBehaviour

{
    public override void Move(Transform enemyTransform, Transform playerTransform, float moveSpeed)
    {
        Vector3 direction = (playerTransform.position - enemyTransform.position).normalized;
        enemyTransform.position += direction * moveSpeed * Time.deltaTime;
    }
}
