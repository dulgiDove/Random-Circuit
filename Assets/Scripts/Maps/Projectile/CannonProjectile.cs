using UnityEngine;

public class CannonProjectile : MonoBehaviour
{
    private static int playerLayer;

    private Vector3 velocity;

    private float lifeTimer;

    private bool returnedToPool = true;

    private ProjectilePool projectilePool;

    internal int ActiveIndex { get; set; } = -1;

    private void Awake()
    {
        playerLayer = LayerMask.NameToLayer("Player");
    }

    public void SetPool(ProjectilePool pool)
    {
        projectilePool = pool;
    }

    public void Launch(Vector3 direction, float speed, float lifetime
    )
    {
        returnedToPool = false;
        velocity = direction.normalized * speed;
        lifeTimer = lifetime;
    }

    internal void Tick(float deltaTime)
    {
        transform.position += velocity * deltaTime;
        lifeTimer -= deltaTime;

        if (lifeTimer <= 0f)
        {
            ReturnToPool();
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (returnedToPool)
        {
            return;
        }

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
        {
            return;
        }

        returnedToPool = true;
        projectilePool.ReleaseProjectile(this);
    }


    internal void PrepareForUse()
    {
        returnedToPool = false;
    }


    internal void PrepareForPool()
    {
        returnedToPool = true;
        velocity = Vector3.zero;
        lifeTimer = 0f;
    }
}