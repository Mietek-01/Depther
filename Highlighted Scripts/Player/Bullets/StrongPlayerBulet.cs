using UnityEngine;
using Cinemachine;

public class StrongPlayerBulet : PlayerBullet
{
    [SerializeField] GameObject platformCollisionFX;
    [SerializeField] CinemachineImpulseSource loadingShaking;

    Collider2D mycollider;
    Animator anim;

    protected override void Awake()
    {
        base.Awake();

        anim = GetComponentInChildren<Animator>();
        mycollider = GetComponent<Collider2D>();

        mycollider.enabled = false;
        enabled = false;

        loadingShaking.GenerateImpulse();
    }

    public void FireOff(float strenght)
    {
        float value = 1f + (strenght - .1f);

        if (value > 1.65f)
            value += 1.3f;

        speed *= value;

        enabled = true;
        mycollider.enabled = true;

        CinemachineImpulseManager.Instance.Clear();

        anim.SetTrigger("Shoot");

        Destroy(gameObject, lifeTime);
    }

    protected override void SetSubscribers()
    {
        OnPlatformCollision += collision =>
        {
            enabled = false;

            // Disable must be execute in the next frame
            Invoke("DisableCollider", 0.1f);

            Vector3 pos = collision.GetContact(0).point;

            pos += new Vector3(0, 0, transform.position.z);

            Destroy(Instantiate(platformCollisionFX, pos, Quaternion.identity
                , GameplayManager.CurrentZone.transform), 5f);

            // To execute shaking I cant destroy the bullet instantly
            Destroy(gameObject, 2f);

            GetComponentInChildren<Renderer>().enabled = false;
            GetComponent<Rigidbody2D>().simulated = false;

        };

        OnEnemyCollision += (enemy, collisionPoint) =>
        {
            enemy.TakeDamage(this.gameObject, damage);

            Vector3 pos = collisionPoint;
            pos += new Vector3(0, 0, transform.position.z);

            Destroy(Instantiate(enemyHitFX, pos, Quaternion.identity
                 , GameplayManager.CurrentZone.transform), 5f);

            Destroy(gameObject);
        };
    }
    void DisableCollider()
    {
        mycollider.enabled = false;
    }
}
