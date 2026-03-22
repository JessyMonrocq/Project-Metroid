using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

public class PlayerWeapon : MonoBehaviour
{
    #region Inspector Fields
    [Header("Player Weapon Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float projectileSpeed = 10;
    
    [Header("Visuals")]
    [SerializeField] private LineRenderer laserLineRenderer;
    [SerializeField] private Transform laserStartingPoint;
    [SerializeField] private float laserOffsetDistance = 0.25f;
    [SerializeField] private LayerMask laserCollisionLayers;

    private bool enableInput = true;

    private IObjectPool<Projectile> projectilePool;
    private Transform poolParent;
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
        GameObject poolObject = new GameObject("PlayerProjectilePool");
        DontDestroyOnLoad(poolObject);
        poolParent = poolObject.transform;

        projectilePool = new ObjectPool<Projectile>(CreateProjectile, OnGetFromPool, OnReleaseFromPool, OnDestroyPooledObject, false, poolDefaultCapacity, poolMaxCapacity);
    }
    
    private void Start()
    {
        playerWeaponTransform = GetComponent<Transform>();
        playerWeaponTransform.localPosition = Vector3.zero;
        playerWeaponTransform.localRotation = Quaternion.identity;
        isPlayerAiming = false;

        for (int i = 0; i < poolDefaultCapacity; i++)
        {
            Projectile projectile = CreateProjectile();
            projectilePool.Release(projectile);
        }

        if (laserLineRenderer != null)
        {
            laserLineRenderer.enabled = false;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        InputManager.Instance.PlayerAttack.performed -= OnPlayerShoot;
        PlayerMovement.Instance.OnPlayerAiming.RemoveListener(OnPlayerAimingChanged);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InputManager.Instance.PlayerAttack.performed += OnPlayerShoot;
        PlayerMovement.Instance.OnPlayerAiming.AddListener(OnPlayerAimingChanged);

        foreach (Projectile projectile in poolParent.GetComponentsInChildren<Projectile>())
        {
            projectilePool.Release(projectile);
        }
    }

    private void OnPlayerAimingChanged(bool aimingState)
    {
        isPlayerAiming = aimingState;
    }

    private void Update()
    {
        Vector2 input = InputManager.Instance.PlayerMove.ReadValue<Vector2>();

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
            Vector3 rayOrigin = new Vector3(laserStartingPoint.transform.position.x, laserStartingPoint.transform.position.y, 0);
            Vector3 rayDirection = -laserStartingPoint.up;
            float maxDistance = 100f;

            if (laserLineRenderer != null)
            {
                laserLineRenderer.enabled = true;
                laserLineRenderer.SetPosition(0, rayOrigin);

                if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hitInfo, maxDistance, laserCollisionLayers))
                {
                    Vector3 offset = hitInfo.point + hitInfo.normal * laserOffsetDistance;
                    laserLineRenderer.SetPosition(1, offset);
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
        Projectile projectileInstance = Instantiate(projectilePrefab.GetComponent<Projectile>(), poolParent);
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
