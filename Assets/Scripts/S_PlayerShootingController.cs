using UnityEngine;

public class S_PlayerShootingController : BaseWeaponController
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float maxShootDistance = 100f;

    private bool hasFired = false;

    protected override void Update()
    {
        if (isAutomatic)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                StartFiring();
            }
            else if (Input.GetButtonUp("Fire1"))
            {
                StopFiring();
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1"))
            {
                StartFiring();
            }
            else if (Input.GetButtonUp("Fire1"))
            {
                StopFiring();
            }
        }

        base.Update();
    }

    protected override bool CanFire()
    {
        if (isAutomatic)
        {
            return isFiring && Time.time >= nextFireTime;
        }
        else
        {
            if (isFiring && !hasFired && Time.time >= nextFireTime)
            {
                hasFired = true;
                return true;
            }
            return false;
        }
    }

    protected override void Fire()
    {
        base.Fire();
        if (!isAutomatic)
        {
            StopFiring();
        }
    }

    public override void StartFiring()
    {
        base.StartFiring();
        hasFired = false;
    }

    public override void StopFiring()
    {
        base.StopFiring();
        hasFired = false;
    }

    protected override void SpawnBullet()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Vector3 targetPoint = Physics.Raycast(ray, out hit, maxShootDistance) ? hit.point : ray.GetPoint(maxShootDistance);

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        bullet.transform.forward = (targetPoint - bulletSpawnPoint.position).normalized;

        if (bullet.TryGetComponent(out Rigidbody rb))
        {
            rb.velocity = bullet.transform.forward * bulletSpeed;
        }
    }

    protected override void OnPauseStateChanged(bool isPaused)
    {
        //disable this script update while game is paused
        enabled = !isPaused;
    }
}