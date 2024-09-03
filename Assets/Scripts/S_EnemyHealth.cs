using System;

public class S_EnemyHealth : BaseHealth
{
    public event Action OnStartFalling;
    public event Action OnResetFalling;

    protected override void Start()
    {
        base.Start();
        OnDamageTaken += HandleDamage;
        OnDeath += TriggerFalling;
    }

    private void HandleDamage(DamageInfo damageInfo)
    {
        // Handle all damage effects here (flashing, particles, etc.)
        if (isDead && damageInfo.isPostDeath)
        {
            // If hit while dead, trigger reset falling
            OnResetFalling?.Invoke();
        }
    }

    protected override void Die()
    {
        isDead = true;
        TriggerFalling();
    }

    private void TriggerFalling()
    {
        OnStartFalling?.Invoke();
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        OnDamageTaken -= HandleDamage;
        OnDeath -= TriggerFalling;
    }
}
