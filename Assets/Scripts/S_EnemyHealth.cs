using UnityEngine;
using UnityEngine.SceneManagement;

public class S_EnemyHealth : BaseHealth
{
    protected override void Die()
    {
        Destroy(gameObject);
    }
}