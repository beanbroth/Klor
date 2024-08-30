using UnityEngine;

public abstract class BaseWeaponController : MonoBehaviour
{
    [SerializeField] protected Transform bulletSpawnPoint;
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected float bulletSpeed = 20f;
    [SerializeField] protected float fireRate = 0.5f;
    [SerializeField] protected int maxAmmo = 30;
    [SerializeField] protected float reloadTime = 2f;

    protected int currentAmmo;
    protected float nextFireTime;

    protected virtual void Start()
    {
        currentAmmo = maxAmmo;
    }

    protected virtual void Update()
    {
        if (CanFire())
        {
            Fire();
        }
    }

    protected abstract bool CanFire();

    protected virtual void Fire()
    {
        if (currentAmmo > 0)
        {
            SpawnBullet();
            currentAmmo--;
            nextFireTime = Time.time + fireRate;
        }
        else
        {
            StartReload();
        }
    }

    protected abstract void SpawnBullet();

    protected virtual void StartReload()
    {
        // Implement reload logic here
        Invoke(nameof(FinishReload), reloadTime);
    }

    protected virtual void FinishReload()
    {
        currentAmmo = maxAmmo;
    }
}