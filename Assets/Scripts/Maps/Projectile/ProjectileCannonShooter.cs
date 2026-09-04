using Unity.Netcode;
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

    private double lastActivationTime = -1d;
    private int lastFiredShotIndex = -1;

    private MapActivity mapActivity;

    private void Awake()
    {
        Debug.Assert(firePoint != null);
        Debug.Assert(projectilePool != null);

        mapActivity = GetComponentInParent<MapActivity>();
        Debug.Assert(mapActivity != null);
    }

    private void Update()
    {
        if (!mapActivity.IsActive)
            return;

        double activationTime = mapActivity.ActivatedServerTime;

        if (activationTime != lastActivationTime)
        {
            lastActivationTime = activationTime;
            lastFiredShotIndex = -1;
        }

        double serverTime = NetworkManager.Singleton.ServerTime.Time;

        double elapsed = serverTime - activationTime;

        if (elapsed < initialDelay)
            return;

        float interval = Mathf.Max(fireInterval, 0.01f);

        int currentShotIndex = Mathf.FloorToInt((float)((elapsed - initialDelay) / interval));

        while (lastFiredShotIndex < currentShotIndex)
        {
            lastFiredShotIndex++;

            double shotServerTime = activationTime + initialDelay+ lastFiredShotIndex * interval;
            double age = serverTime - shotServerTime;

            if (age < projectileLifetime)
            {
                Fire(shotServerTime);
            }
        }
    }


    private void Fire(double shotServerTime)
    {
        CannonProjectile projectile = projectilePool.GetProjectile();

        projectile.transform.SetPositionAndRotation(firePoint.position, firePoint.rotation);

        projectile.Launch( firePoint.position, firePoint.forward, projectileSpeed, projectileLifetime, shotServerTime);
    }
}