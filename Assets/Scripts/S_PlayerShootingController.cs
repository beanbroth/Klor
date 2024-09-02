using UnityEngine;

public class PlayerShootingController : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float maxShootDistance = 100f;
    [SerializeField] private Transform weaponModelTransform;
    private GunInstance primary, secondary, currentWeapon;
    private bool isFiring;
    private float nextFireTime;

    private void Start()
    {
        var inventory = InventoryManager.Instance;
        inventory.primaryWeaponSlot.OnItemChanged += OnWeaponChanged;
        inventory.secondaryWeaponSlot.OnItemChanged += OnWeaponChanged;
    }

    private void OnDestroy()
    {
        var inventory = InventoryManager.Instance;
        inventory.primaryWeaponSlot.OnItemChanged -= OnWeaponChanged;
        inventory.secondaryWeaponSlot.OnItemChanged -= OnWeaponChanged;
    }

    private void OnWeaponChanged(BaseItemInstance newWeapon)
    {
        var gunInstance = (GunInstance)newWeapon;
        if (newWeapon == InventoryManager.Instance.primaryWeaponSlot.CurrentItem)
            primary = gunInstance;
        else
            secondary = gunInstance;
        if (currentWeapon == null)
            SwitchToWeapon(gunInstance);
    }

    private void Update()
    {
        HandleWeaponSwitching();
        if (currentWeapon == null)
            return;
        HandleFiring();
        HandleReloading();
    }

    private void HandleWeaponSwitching()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && primary != null)
            SwitchToWeapon(primary);
        else if (Input.GetKeyDown(KeyCode.Alpha2) && secondary != null)
            SwitchToWeapon(secondary);
    }

    private void SwitchToWeapon(GunInstance newWeapon)
    {
        if (newWeapon == currentWeapon)
            return;
        currentWeapon = newWeapon;
        UpdateWeaponModel();
    }

    private void UpdateWeaponModel()
    {
        foreach (Transform child in weaponModelTransform)
            Destroy(child.gameObject);
        var gunData = (GunItemData)currentWeapon.ItemData;
        var newWeaponModel = Instantiate(gunData.prefab, weaponModelTransform);
        newWeaponModel.transform.localPosition = Vector3.zero;
        newWeaponModel.transform.localRotation = Quaternion.identity;
        if (!newWeaponModel.transform.Find("BulletSpawnPoint"))
            Debug.LogWarning("BulletSpawnPoint not found in the weapon model. Using default position.");
    }

    private void HandleFiring()
    {
        var gunData = (GunItemData)currentWeapon.ItemData;
        var fireButton = gunData.isAutomatic ? Input.GetButton("Fire1") : Input.GetButtonDown("Fire1");
        if (fireButton && currentWeapon.currentAmmo > 0)
            StartFiring();
        else if (Input.GetButtonUp("Fire1"))
            StopFiring();
        if (CanFire())
            Fire();
    }

    private bool CanFire()
    {
        if (currentWeapon == null || currentWeapon.currentAmmo <= 0)
            return false;
        var gunData = (GunItemData)currentWeapon.ItemData;
        return isFiring && Time.time >= nextFireTime && (gunData.isAutomatic || !gunData.isAutomatic);
    }

    private void Fire()
    {
        var gunData = (GunItemData)currentWeapon.ItemData;
        nextFireTime = Time.time + 1f / gunData.fireRate;
        for (int i = 0; i < gunData.bulletsPerShot; i++)
        {
            SpawnBullet();
        }

        //TODO: ApplyRecoil(gunData.recoilPerShot);
        if (!gunData.isAutomatic)
            StopFiring();
        currentWeapon.UpdateDurability(-1f);
        currentWeapon.AddXP(1f);
        currentWeapon.currentAmmo--;
    }

    private void StartFiring() => isFiring = true;
    private void StopFiring() => isFiring = false;

    private void SpawnBullet()
    {
        var ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        var targetPoint = Physics.Raycast(ray, out var hit, maxShootDistance)
            ? hit.point
            : ray.GetPoint(maxShootDistance);
        var bulletSpawnPoint = weaponModelTransform.Find("BulletSpawnPoint") ?? weaponModelTransform;
        var gunData = (GunItemData)currentWeapon.ItemData;
        var bullet = Instantiate(gunData.bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        Vector3 spreadDirection = ApplyInaccuracy(targetPoint - bulletSpawnPoint.position);
        bullet.transform.forward = spreadDirection.normalized;
        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * gunData.bulletSpeed;

        // Set bullet damage
        var bulletComponent = bullet.GetComponent<S_BulletController>();
        if (bulletComponent != null)
        {
            bulletComponent.damage = gunData.damage;
        }
    }

    private Vector3 ApplyInaccuracy(Vector3 direction)
    {
        var gunData = (GunItemData)currentWeapon.ItemData;
        float inaccuracyAngle = gunData.inaccuracy;
        Quaternion spreadRotation = Quaternion.AngleAxis(Random.Range(-inaccuracyAngle, inaccuracyAngle), Vector3.up) *
                                    Quaternion.AngleAxis(Random.Range(-inaccuracyAngle, inaccuracyAngle),
                                        Vector3.right);
        return spreadRotation * direction;
    }

    private void HandleReloading()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }
    }

    private void Reload()
    {
        if (currentWeapon == null)
            return;
        var gunData = (GunItemData)currentWeapon.ItemData;
        currentWeapon.currentAmmo = gunData.ammoCapacity;
        Debug.Log($"Reloaded {currentWeapon.ItemData.ItemName}. Current ammo: {currentWeapon.currentAmmo}");
    }

    public void OnPauseStateChanged(bool isPaused) => enabled = !isPaused;
}