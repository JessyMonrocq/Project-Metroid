using UnityEngine;

public class HealthComponent : MonoBehaviour, IDamageable
{
    [Header("Healt Component Settings")]
    [SerializeField] private int m_maxHealth = 10;

    public int MaxHealth => m_maxHealth;
    public int CurrentHealth { get; set; }
    public bool IsAlive => CurrentHealth > 0;

    protected virtual void Start()
    {
        CurrentHealth = m_maxHealth;
    }

    public void TakeDamage(int ammount)
    {
        if (!IsAlive)
        {
            return;
        }

        CurrentHealth -= ammount;
        CurrentHealth = Mathf.Max(CurrentHealth, 0);

        if (CurrentHealth <= 0)
        {
            OnDeath();
        }
    }

    public void Heal(int ammount)
    {
        if (!IsAlive)
        {
            return;
        }

        CurrentHealth += ammount;
        CurrentHealth = Mathf.Min(CurrentHealth, m_maxHealth);
    }

    protected virtual void OnDeath()
    {

    }
}
