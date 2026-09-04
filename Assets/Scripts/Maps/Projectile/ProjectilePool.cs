using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ProjectilePool : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField]
    private CannonProjectile projectilePrefab;

    [Header("Pool")]
    [SerializeField]
    private int prewarmCount = 80;
    [SerializeField]
    private int defaultCapacity = 64;
    [SerializeField]
    private int maxSize = 128;

    private ObjectPool<CannonProjectile> pool;
    private List<CannonProjectile> activeProjectiles;

    private void Awake()
    {
        Debug.Assert(projectilePrefab != null);

        activeProjectiles = new List<CannonProjectile>(prewarmCount);

        pool = new ObjectPool<CannonProjectile>(
            CreateProjectile,
            OnGetProjectile,
            OnReleaseProjectile,
            OnDestroyProjectile,
            true,
            defaultCapacity,
            maxSize
        );

        Prewarm();
    }


    private void Update()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
            return;

        double serverTime = NetworkManager.Singleton.ServerTime.Time;

        int index = 0;

        while (index < activeProjectiles.Count)
        {
            CannonProjectile projectile = activeProjectiles[index];

            projectile.Tick(serverTime);

            if (index < activeProjectiles.Count && activeProjectiles[index] == projectile)
            {
                index++;
            }
        }
    }

    private CannonProjectile CreateProjectile()
    {
        CannonProjectile projectile = Instantiate(projectilePrefab, transform);
        projectile.SetPool(this);
        projectile.gameObject.SetActive(false);
        return projectile;
    }

    internal void ReleaseProjectile(CannonProjectile projectile)
    {
        pool.Release(projectile);
    }

    public void ReleaseAllProjectiles()
    {
        while (activeProjectiles.Count > 0)
        {
            pool.Release(activeProjectiles[activeProjectiles.Count - 1]);
        }
    }

    private void Prewarm()
    {
        int count = Mathf.Min(prewarmCount, maxSize);
        CannonProjectile[] projectiles = new CannonProjectile[count];

        for (int i = 0; i < count; i++)
        {
            projectiles[i] = pool.Get();
        }

        for (int i = 0; i < count; i++)
        {
            pool.Release(projectiles[i]);
        }
    }

    public CannonProjectile GetProjectile()
    {
        return pool.Get();
    }

    private void OnGetProjectile(CannonProjectile projectile)
    {
        projectile.PrepareForUse();
        AddActiveProjectile(projectile);
        projectile.gameObject.SetActive(true);
    }


    private void OnReleaseProjectile(CannonProjectile projectile)
    {
        RemoveActiveProjectile(projectile);
        projectile.PrepareForPool();
        projectile.gameObject.SetActive(false);
    }


    private void OnDestroyProjectile(CannonProjectile projectile)
    {
        RemoveActiveProjectile(projectile);
        projectile.PrepareForPool();
        Destroy(projectile.gameObject);
    }

    private void AddActiveProjectile(CannonProjectile projectile)
    {
        projectile.ActiveIndex = activeProjectiles.Count;
        activeProjectiles.Add(projectile);
    }


    private void RemoveActiveProjectile(CannonProjectile projectile)
    {
        int index = projectile.ActiveIndex;

        if (index < 0 || index >= activeProjectiles.Count)
            return;

        int lastIndex = activeProjectiles.Count - 1;

        if (index != lastIndex)
        {
            CannonProjectile lastProjectile = activeProjectiles[lastIndex];
            activeProjectiles[index] = lastProjectile;
            lastProjectile.ActiveIndex = index;
        }

        activeProjectiles.RemoveAt(lastIndex);
        projectile.ActiveIndex = -1;
    }
}