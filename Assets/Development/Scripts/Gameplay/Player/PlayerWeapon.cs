using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

public class PlayerWeapon : MonoBehaviour
{
    #region Inspector Fields
    [Header("Input Actions References")]
    [SerializeField] private InputActionReference IA_PlayerLook;
    [SerializeField] private InputActionReference IA_PlayerShoot;

    [Header("Player Weapon Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float projectileSpeed = 10;
    
    [Header("Visuals")]
    [SerializeField] private LineRenderer laserLineRenderer;

    private bool enableInput = true;

    private IObjectPool<Projectile> projectilePool;
    private int poolDefaultCapacity = 10;
    private int poolMaxCapacity = 20;
    
    private Transform playerWeaponTransform;
    private Vector2 playerWeaponDefaultRotation = new Vector2(1, 0);
    private int playerDirection = 1;
    private bool isPlayerAiming;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        projectilePool = new ObjectPool<Projectile>(CreateProjectile, OnGetFromPool, OnReleaseFromPool, OnDestroyPooledObject, false, poolDefaultCapacity, poolMaxCapacity);
    }
    
    private void Start()
    {
        playerWeaponTransform = GetComponent<Transform>();
        playerWeaponTransform.localPosition = Vector3.zero;
        playerWeaponTransform.localRotation = Quaternion.identity;
        isPlayerAiming = false;

        PlayerMovement.Instance.OnPlayerAiming.AddListener((aimingState) => {
            isPlayerAiming = aimingState;
        });

        if (laserLineRenderer != null)
        {
            laserLineRenderer.enabled = false;
        }
    }

    private void OnEnable()
    {
        IA_PlayerLook.action.Enable();
        IA_PlayerShoot.action.Enable();

        IA_PlayerShoot.action.performed += OnPlayerShoot;
    }

    private void OnDisable()
    {
        IA_PlayerLook.action.Disable();
        IA_PlayerShoot.action.Disable();

        IA_PlayerShoot.action.performed -= OnPlayerShoot;
        PlayerMovement.Instance.OnPlayerAiming.RemoveAllListeners();
    }

    private void Update()
    {
        Vector2 input = IA_PlayerLook.action.ReadValue<Vector2>();

        float angle = Vector2.Angle(playerWeaponDefaultRotation, input);

        if (!enableInput)
        {
            return;
        }

        if (input.x == 0 && input.y == 0)
        {
            playerWeaponTransform.localRotation = Quaternion.identity;
        }
        else
        {
            bool isAnglePositive = input.y > 0;
            playerWeaponTransform.localRotation = Quaternion.Euler(0, 0, isAnglePositive ? angle : -angle);
        }

        if (isPlayerAiming)
        {
            Vector3 rayOrigin = new Vector3(projectileSpawnPoint.transform.position.x, projectileSpawnPoint.transform.position.y, 0);
            Vector3 rayDirection = -projectileSpawnPoint.up;
            float maxDistance = 100f;

            if (laserLineRenderer != null)
            {
                laserLineRenderer.enabled = true;
                laserLineRenderer.SetPosition(0, rayOrigin);

                if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hitInfo, maxDistance))
                {
                    laserLineRenderer.SetPosition(1, hitInfo.point);
                }
                else
                {
                    laserLineRenderer.SetPosition(1, rayOrigin + rayDirection * maxDistance);
                }
            }
        }
        else
        {
            if (laserLineRenderer != null)
            {
                laserLineRenderer.enabled = false;
            }
        }
    }
    #endregion

    #region Input Callbacks
    private void OnPlayerShoot(InputAction.CallbackContext context)
    {
        Shoot();
    }
    #endregion

    #region Custom Methods
    public void EnableInput(bool state)
    {
        enableInput = state;
    }

    private void Shoot()
    {
        if (!enableInput)
        {
            return;
        }

        Projectile projectile = projectilePool.Get();

        Vector3 spawnPosition = new Vector3(projectileSpawnPoint.transform.position.x, projectileSpawnPoint.transform.position.y, 0);
        projectile.transform.position = spawnPosition;
        projectile.transform.rotation = projectileSpawnPoint.transform.rotation;
        projectile.GetComponent<Rigidbody>().AddForce(transform.right * projectileSpeed, ForceMode.Impulse);
    }

    public void SetPlayerDirection(int direction)
    {
        playerDirection = direction;
        playerWeaponDefaultRotation = playerDirection == 1 ? new Vector2(1, 0) : new Vector2(-1, 0);
    }
    #endregion

    #region Pooling Methods
    private Projectile CreateProjectile()
    {
        Projectile projectileInstance = Instantiate(projectilePrefab.GetComponent<Projectile>());
        projectileInstance.ProjectilePool = projectilePool;
        return projectileInstance;
    }

    private void OnGetFromPool(Projectile pooledProjectile)
    {
        pooledProjectile.gameObject.SetActive(true);
    }

    private void OnReleaseFromPool(Projectile pooledProjectile)
    {
        pooledProjectile.gameObject.SetActive(false);
    }

    private void OnDestroyPooledObject(Projectile pooledProjectile)
    {
        Destroy(pooledProjectile.gameObject);
    }
    #endregion
}
