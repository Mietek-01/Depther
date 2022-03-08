using UnityEngine;

public class SmallGoblin : Enemy
{
    [Space(10), Header("SmallGoblin")]
    [SerializeField] bool activeOnStart = true;
    [SerializeField] Vector2 rangeOfSpeed = new Vector2(5, 10);
    [SerializeField] Vector2 activationTimeRange = new Vector2(1, 5);

    [SerializeField] GameObject firstEye;
    [SerializeField] GameObject secondEye;
    [SerializeField] Collider2D frontCheck;

    float whenJump;
    float whenFlip;

    LayerMask whatIsGround;
    Vector2 velocity;

    bool jump = false;

    private void Start()
    {
        whatIsGround = LayerMask.GetMask("Platform");

        speed = Random.Range(rangeOfSpeed.x, rangeOfSpeed.y);
        whenJump = Random.Range(1, 4);
        whenFlip = Random.Range(3, 6);

        rb.simulated = false;
        enabled = false;

        var myTrigger = GetComponentInChildren<Trigger>(true);

        if (activeOnStart)
        {
            Destroy(myTrigger.gameObject);
            Activate();
        }
        else
            myTrigger.transform.SetParent(transform.parent);
    }

    void Update()
    {
        if (frontCheck.IsTouchingLayers(whatIsGround))
            Flip();

        JumpCheck();

        FlipCheck();
    }

    private void FixedUpdate()
    {
        if (jump)
        {
            rb.AddForce(new Vector2(0, Random.Range(12, 25) * 100));
            jump = false;
        }

        velocity.x = rightFacing ? speed : -speed;
        velocity.y = rb.velocity.y;

        rb.velocity = velocity;
    }

    // Call by my trigger
    public void Activate()
    {
        Invoke("Attack", Random.Range((int)activationTimeRange.x, (int)activationTimeRange.y));
    }

    protected override void SetSubscribers()
    {
        OnDie += (GameObject whoIsAttacking) =>
        {
            Destroy(gameObject);

            if (deathFX)
                Instantiate(deathFX, transform.position, Quaternion.identity, transform.root);

            AudioManager.PlaySFX(deathClip);
        };
    }

    void Attack()
    {
        firstEye.SetActive(true);
        secondEye.SetActive(true);

        rb.simulated = true;
        enabled = true;

        anim.SetTrigger("Attack");

        var player = GameplayManager.Player;

        if (player)
            if (transform.position.x > player.transform.position.x)
                Flip();
    }

    void JumpCheck()
    {
        if (whenJump <= 0)
        {
            jump = true;
            whenJump = Random.Range(1, 4);
        }
        else
            whenJump -= Time.deltaTime;
    }

    void FlipCheck()
    {
        if (whenFlip <= 0)
        {
            Flip();
            whenFlip = Random.Range(3, 5);
        }
        else
            whenFlip -= Time.deltaTime;
    }
}
