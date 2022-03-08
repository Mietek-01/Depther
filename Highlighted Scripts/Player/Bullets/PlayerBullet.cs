using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerBullet : MonoBehaviour
{
    [SerializeField] protected float speed = 20f;
    [SerializeField] protected float lifeTime = 2f;
    [SerializeField] protected int damage = 1;
    [SerializeField] string spatterFXName = "SpatterFX";

    [Header("Ref")]
    [SerializeField] protected GameObject enemyHitFX;
    [SerializeField] protected AudioClip enemyHitClip;

    protected event Action<Enemy, Vector2> OnEnemyCollision;
    protected event Action<Collision2D> OnPlatformCollision;

    int whatIsPlatform;
    int whatIsEnemy;

    bool hitEnemy = false;

    protected virtual void Awake()
    {
        whatIsPlatform = LayerMask.NameToLayer("Platform");
        whatIsEnemy = LayerMask.NameToLayer("Enemy");

        SetSubscribers();
    }

    protected virtual void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    private void OnEnable()
    {
        hitEnemy = false;
        Invoke("Disable", lifeTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == whatIsPlatform)
            OnPlatformCollision.Invoke(collision);
        else
        if (collision.gameObject.layer == whatIsEnemy)
        {
            var enemy = collision.gameObject.GetComponent<Enemy>();

            if (enemy)
                OnEnemyCollision.Invoke(enemy, collision.GetContact(0).point);
            else
                Debug.LogWarning("The object with the Enemy layer doesnt have the Enemy script");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == whatIsEnemy)
        {
            var enemy = collision.gameObject.GetComponent<Enemy>();

            if (enemy)
                OnEnemyCollision.Invoke(enemy, collision.transform.position);
            else
                Debug.LogWarning("The object with the Enemy layer doesnt have the Enemy script");
        }
    }

    protected virtual void SetSubscribers()
    {
        OnEnemyCollision += (enemy, collisionPoint) =>
        {
            enemy.TakeDamage(this.gameObject, damage);

            hitEnemy = true;

            if (enemy.Health > 0)
            {
                var hitFX = Instantiate(enemyHitFX, enemy.transform.position
                    , Quaternion.identity, enemy.transform);

                Destroy(hitFX, 1f);

                AudioManager.PlaySFX(enemyHitClip);
            }

            Disable();
        };

        OnPlatformCollision += collision =>
        {
            // It protects againts the excess FX
            if (hitEnemy)
                return;

            Vector3 FXPosition;
            Quaternion FXDirection;

            if (UsefulFunctions.FindTangentFor(collision, transform.position, out FXPosition
                , out FXDirection, whatIsPlatform))
            {
                ObjectsPooler.PlayParticleSystem(spatterFXName, FXPosition, FXDirection
                    , GameplayManager._DynamicOfCurrentZone);
            }

            Disable();
        };
    }

    void Disable()
    {
        gameObject.SetActive(false);
    }
}
