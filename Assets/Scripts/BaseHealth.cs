using UnityEngine;
using System;

public abstract class BaseHealth : MonoBehaviour
{
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField][ReadOnly] protected float currentHealth;

    // Events
    public event Action<float> OnDamageTaken;
    public event Action OnDeath;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;
        OnDamageTaken?.Invoke(damage);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnDeath?.Invoke();
            Die();
        }
    }

    protected abstract void Die();

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }
}