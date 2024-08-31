using UnityEngine;

public abstract class BaseHealth : MonoBehaviour
{
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] [ReadOnly] protected float currentHealth;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(float damage)
    {
        Debug.Log("Taking damage ");
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
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