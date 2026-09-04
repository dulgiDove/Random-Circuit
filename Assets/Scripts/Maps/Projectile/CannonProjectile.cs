using UnityEngine;

public class CannonProjectile : MonoBehaviour
{
    private static int playerLayer;

    private Vector3 velocity;

    private bool returnedToPool = true;

    private ProjectilePool projectilePool;

    private Vector3 spawnPosition;
    private double spawnServerTime;
    private float lifetime;

    internal int ActiveIndex { get; set; } = -1;

    private void Awake()
    {
        playerLayer = LayerMask.NameToLayer("Player");
    }

    public void SetPool(ProjectilePool pool)
    {
        projectilePool = pool;
    }

    public void Launch(Vector3 position, Vector3 direction, float speed, float lifetime, double serverTime)
    {
        returnedToPool = false;

        spawnPosition = position;
        velocity = direction.normalized * speed;

        this.lifetime = lifetime;
        spawnServerTime = serverTime;

        transform.position = position;
    }

    internal void Tick(double currentServerTime)
    {
        double elapsed = currentServerTime - spawnServerTime;

        if (elapsed >= lifetime)
        {
            ReturnToPool();
            return;
        }

        float time = Mathf.Max(0f, (float)elapsed);

        transform.position = spawnPosition + velocity * time;
    }

    internal void PrepareForUse()
    {
        returnedToPool = false;
    }

    internal void PrepareForPool()
    {
        returnedToPool = true;
        spawnPosition = Vector3.zero;
        spawnServerTime = 0d;
        lifetime = 0f;
        velocity = Vector3.zero;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (returnedToPool)
            return;

        if (other.gameObject.layer == playerLayer)
        {
            PlayerRespawn playerRespawn = other.GetComponentInParent<PlayerRespawn>();

            if (playerRespawn != null)
            {
                playerRespawn.Respawn();
                ReturnToPool();
                return;
            }
        }

        if (!other.isTrigger)
        {
            ReturnToPool();
        }
    }


    private void ReturnToPool()
    {
        if (returnedToPool)
            return;

        returnedToPool = true;
        projectilePool.ReleaseProjectile(this);
    }
}