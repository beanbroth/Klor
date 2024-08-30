using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_PlayerShootingController : BaseWeaponController
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float maxShootDistance = 100f;

    protected override bool CanFire()
    {
        return Input.GetButton("Fire1") && Time.time >= nextFireTime;
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
}