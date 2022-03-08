using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class Enemy : Damageable, IPlayerKiller
{
    [Space(10), Header("Enemy")]
    [SerializeField] protected float speed = 5f;
    [SerializeField] protected int damage = 1;
    [SerializeField] protected Player.KindOfDeath kindOfPlayerDeath = Player.KindOfDeath.BOOM;

    [SerializeField] protected GameObject deathFX;
    [SerializeField] protected AudioClip deathClip;

    public Player.KindOfDeath PlayerDeath => kindOfPlayerDeath;

    protected bool rightFacing = true;

    protected Animator anim;
    protected Rigidbody2D rb;

    protected override void Awake()
    {
        base.Awake();

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            collision.transform.GetComponent<Player>().TakeDamage(this.gameObject, damage);
    }

    protected virtual void Flip()
    {
        rightFacing = !rightFacing;
        transform.Rotate(0, 180, 0);
    }
}
