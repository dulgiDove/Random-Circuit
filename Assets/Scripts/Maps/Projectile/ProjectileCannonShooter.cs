using UnityEngine;

public class ProjectileCannonShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform firePoint;
    [SerializeField]
    private ProjectilePool projectilePool;
    [SerializeField]
    private float projectileLifetime = 5f;


    [Header("Fire")]
    [SerializeField]
    private float initialDelay = 0f;
    [SerializeField]
    private float fireInterval = 1.5f;
    [SerializeField]
    private float projectileSpeed = 12f;

    private float fireTimer;

    private MapActivity mapActivity;

    private void Awake()
    {
        mapActivity = GetComponentInParent<MapActivity>();
    }

    private void Start()
    {
        fireTimer = initialDelay;
    }

    private void Update()
    {
        if (mapActivity != null && !mapActivity.IsActive)
        {
            return;
        }

        fireTimer -= Time.deltaTime;

        if (fireTimer > 0f)
        {
            return;
        }

        Fire();
        fireTimer = fireInterval;
    }


    private void Fire()
    {
        CannonProjectile projectile = projectilePool.GetProjectile();
        projectile.transform.SetPositionAndRotation(firePoint.position, firePoint.rotation);
        projectile.Launch(firePoint.forward, projectileSpeed, projectileLifetime);
    }
}