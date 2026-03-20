using UnityEngine;
using UnityEngine.Pool;

public class Projectile : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    private float projectileLifeTime = 5f;
    private float projectileLifeTimer;

    private IObjectPool<Projectile> projectilePool;

    public IObjectPool<Projectile> ProjectilePool
    {
        set => projectilePool = value;
    }

    private void Start()
    {
        projectileLifeTimer = 0f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Deactivate();
    }

    private void Update()
    {
        projectileLifeTimer += Time.deltaTime;
        if (projectileLifeTimer > projectileLifeTime)
        {
            Deactivate();
        }
    }

    public int GetDamage()
    {
        return damage;
    }

    public void Deactivate()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        projectileLifeTimer = 0f;

        projectilePool.Release(this);
    }
}
