public interface IDamageable
{
    int CurrentHealth { get; set; }
    int MaxHealth { get; }

    bool IsAlive { get; }

    void TakeDamage(int amount);

    void Heal(int amount);
}
