using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerBullet : MonoBehaviour
{
    [Header("PlayerBullet")]
    [SerializeField] protected float speed = 20f;
    [SerializeField] protected float lifeTime = 2f;
    [SerializeField] protected int damage = 1;
    [SerializeField] protected string spatterFXName = "SpatterFX";

    [Space(10)]
    [SerializeField] protected GameObject enemyHitFX;
    [SerializeField] protected AudioClip enemyHitClip;

    int whatIsPlatform;
    int whatIsEnemy;

    bool hitEnemy;

    protected virtual void Awake()
    {
        whatIsPlatform = LayerMask.NameToLayer("Platform");
        whatIsEnemy = LayerMask.NameToLayer("Enemy");
    }

    protected virtual void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    private void OnEnable()
    {
        hitEnemy = false;
        Invoke(nameof(Disable), lifeTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == whatIsPlatform)
            PlatformCollision(collision);
        else
        if (collision.gameObject.layer == whatIsEnemy)
        {
            var enemy = collision.gameObject.GetComponent<Enemy>();

            if (enemy)
                EnemyCollision(enemy, collision.GetContact(0).point);
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
                EnemyCollision(enemy, collision.transform.position);
            else
                Debug.LogWarning("The object with the Enemy layer doesnt have the Enemy script");
        }
    }

    protected virtual void EnemyCollision(Enemy enemy, Vector2 collisionPoint)
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
    }

    protected virtual void PlatformCollision(Collision2D collision)
    {
        // Protects againts the excess FX
        if (hitEnemy)
            return;

        Vector3 FXPosition;
        Quaternion FXDirection;

        if (UsefulFunctions.FindTangentFor(collision, transform.position, out FXPosition
            , out FXDirection, whatIsPlatform))
        {
            ObjectsPooler.PlayParticleSystem(spatterFXName, FXPosition, FXDirection
                , GameplayManager.DynamicContainerOfCurrentZone);
        }

        Disable();
    }

    void Disable()
    {
        gameObject.SetActive(false);
    }
}
