using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

public class PlayerWeapon : MonoBehaviour
{
    #region Inspector Fields
    [SerializeField] private InputActionReference IA_PlayerLook;
    [SerializeField] private InputActionReference IA_PlayerShoot;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float projectileSpeed = 10;
    private bool enableInput = true;

    private IObjectPool<Projectile> projectilePool;
    private int poolDefaultCapacity = 10;
    private int poolMaxCapacity = 20;
    
    private int playerDirection = 1;
    private Transform playerWeaponTransform;
    private Vector2 playerWeaponDefaultRotation = new Vector2(1, 0);
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
    }

    private void OnEnable()
    {
        IA_PlayerLook.action.Enable();
        IA_PlayerShoot.action.performed += OnPlayerShoot;
    }

    private void OnDisable()
    {
        IA_PlayerLook.action.Disable();
        IA_PlayerShoot.action.performed -= OnPlayerShoot;
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
