using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class JumpThroughPlatform : MonoBehaviour
{
    [SerializeField] private BoxCollider platformCollider;
    [SerializeField] private Transform platformTopPosition;

    private Transform playerPosition;
    private float playerFeetPosition;
    private float colliderTopPosition;
    private bool playerOnPlatform;
    private bool platformDisabled;

    private void Start()
    {
        playerOnPlatform = false;
        platformDisabled = false;
        playerPosition = PlayerController.Instance.playerTransform;
        colliderTopPosition = platformTopPosition.position.y;
        PlayerController.Instance.OnPlayerCrouchJump.AddListener(HandleCrouchJump);
    }

    private void OnDisable()
    {
        PlayerController.Instance.OnPlayerCrouchJump.RemoveListener(HandleCrouchJump);
    }

    private void Update()
    {
        playerFeetPosition = playerPosition.position.y - 1;

        if (platformDisabled)
        {
            return;
        }

        if (playerFeetPosition > colliderTopPosition)
        {
            platformCollider.gameObject.layer = 0; // Default layer, allowing collision
        }
        else
        {
            platformCollider.gameObject.layer = 12; // Phaze layer, ignoring collision with player
        }

        if (playerFeetPosition >= colliderTopPosition && (playerFeetPosition - colliderTopPosition) <= 0.1f && PlayerController.Instance.IsPlayerGrounded)
        {
            playerOnPlatform = true;
        }
        else
        {
            playerOnPlatform = false;
        }
    }

    private void HandleCrouchJump()
    {
        if (playerOnPlatform)
        {
            StartCoroutine(DisableCollisionCoroutine());
        }
    }

    private IEnumerator DisableCollisionCoroutine()
    {
        platformDisabled = true;
        platformCollider.gameObject.layer = 12;
        yield return new WaitForSeconds(0.33f);
        platformCollider.gameObject.layer = 0;
        platformDisabled = false;
    }
}
