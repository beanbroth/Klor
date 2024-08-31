using UnityEngine;
using System;

public struct DamageInfo
{
    public float amount;
    public bool isPostDeath;
    public bool isKillingBlow;
    public Vector3 hitLocation; // New field for hit location in local space

    public DamageInfo(float amount, bool isPostDeath, bool isKillingBlow, Vector3 hitLocation)
    {
        this.amount = amount;
        this.isPostDeath = isPostDeath;
        this.isKillingBlow = isKillingBlow;
        this.hitLocation = hitLocation;
    }
}

public abstract class BaseHealth : MonoBehaviour
{
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField][ReadOnly] protected float currentHealth;
    protected bool isDead = false;
    public event Action<DamageInfo> OnDamageTaken;
    public event Action OnDeath;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(float damage, Vector3 hitLocation)
    {
        bool wasAliveBeforeDamage = !isDead;
        bool isKillingBlow = false;

        if (!isDead)
        {
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                isDead = true;
                isKillingBlow = true;
            }
        }


        // Always invoke OnDamageTaken, whether alive or dead
        OnDamageTaken?.Invoke(new DamageInfo(damage, isDead && !wasAliveBeforeDamage, isKillingBlow, hitLocation));

        if (wasAliveBeforeDamage && isDead)
        {
            OnDeath?.Invoke();
            Die();
        }
    }

    protected abstract void Die();

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public bool IsDead() => isDead;
}