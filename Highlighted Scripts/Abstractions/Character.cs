using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(PlayerInput))]
public abstract class Character : Damageable
{
    [Header("Character")]
    [SerializeField] protected float speed = 700f;
    [SerializeField] protected float jumpForce = 1500f;
    [SerializeField] protected float basicFriction = 0.008f;

    [SerializeField] protected LayerMask whatIsPlatform;
    [SerializeField] protected Collider2D groundCheck;
    [SerializeField] protected Collider2D myCollider;

    [SerializeField] protected AudioClip[] jumpsClip;
    [SerializeField] protected AudioClip dropClip;

    public bool RightFacing { get; protected set; } = true;

    public bool IsGrounded { get; protected set; }

    public bool Frozen { get; protected set; }

    public bool WasGrounded { get; protected set; }

    public bool IsCrouching { get; protected set; }

    public bool Jump { get; protected set; }

    public bool Immortality { get; set; }

    public bool IamDead { get; protected set; }

    public Vector2 Velocity => velocity;

    readonly protected int hashOfIsJumping = Animator.StringToHash("isJumping");
    readonly protected int hashOfIsRunning = Animator.StringToHash("isRunning");
    readonly protected int hashOfIsCrouching = Animator.StringToHash("isCrouching");

    protected Vector2 velocity = Vector2.zero;

    protected IPlayerInputData inputData;
    protected IPlayerInputManagement inputManagement;

    protected Animator anim;
    protected Rigidbody2D rb;

    protected override void Awake()
    {
        base.Awake();

        SetReferences();

        SetData();
    }

    protected virtual void Update()
    {
        if (IsCrouching)
            StandCheck();
        else
        {
            GroundedStateUpdate();

            LandCheck();

            CrouchCheck();

            if (!IsCrouching)
                JumpCheck();
        }
    }

    private void FixedUpdate()
    {
        if (IsCrouching)
            return;

        HorizontalMovement();

        AirMovement();
    }

    public void AssignPlayerInputDataInterface(IPlayerInputData playerInputData)
    {
        if (playerInputData == null)
            Debug.LogError("I have no access to the input");
        else
            inputData = playerInputData;
    }

    public void FreezeInput(bool value, bool withRigidBody)
    {
        Frozen = value;

        inputManagement.Enabled = !value;

        rb.velocity = Vector2.zero;

        // I may want to freeze the player with physics as wall ( e.g. during the ZS Field)
        if (withRigidBody)
            rb.simulated = !value;
    }

    protected virtual void StandCheck()
    {
        if (inputData.StandUp)
        {
            myCollider.sharedMaterial.friction = basicFriction;
            anim.SetBool(hashOfIsCrouching, false);

            Invoke("SetCrouchingToFalse", .3f);
        }
    }

    protected virtual void CrouchCheck()
    {
        if (IsGrounded && inputData.Crouch)
        {
            anim.SetBool(hashOfIsCrouching, true);
            IsCrouching = true;

            AudioManager.PlayPlayerVoices(jumpsClip[Random.Range(0, jumpsClip.Length)]);
        }
    }

    protected virtual void GroundedStateUpdate()
    {
        WasGrounded = IsGrounded;

        IsGrounded = groundCheck.IsTouchingLayers(whatIsPlatform);
    }

    protected virtual void LandCheck()
    {
        // I have landed
        if (!WasGrounded && IsGrounded)
        {
            anim.SetBool(hashOfIsJumping, false);

            // Enable Upper dash move
            if (!Frozen)
                inputManagement.EnableInputFor("UpperDashMove", true);

            AudioManager.PlayPlayerVoices(dropClip);
        }
    }

    protected virtual void JumpCheck()
    {
        if (IsGrounded && inputData.Jump)
        {
            Jump = true;
            anim.SetBool(hashOfIsJumping, true);
            AudioManager.PlayPlayerVoices(jumpsClip[Random.Range(0, jumpsClip.Length)]);
        }
        else
        // The fall is like a jump
        if (WasGrounded && !IsGrounded)
            anim.SetBool(hashOfIsJumping, true);
    }

    protected virtual void HorizontalMovement()
    {
        velocity.x = inputData.HorizontalMovement * speed * Time.fixedDeltaTime;
        velocity.y = rb.velocity.y;

        rb.velocity = velocity;

        anim.SetBool(hashOfIsRunning, inputData.HorizontalMovement != 0f);
    }

    protected virtual void AirMovement()
    {
        if (Jump)
        {
            rb.AddForce(new Vector2(0, jumpForce));
            Jump = false;
        }
    }

    protected virtual void SetReferences()
    {
        AssignPlayerInputDataInterface(GetComponent<IPlayerInputData>());

        inputManagement = GetComponent<IPlayerInputManagement>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    protected virtual void SetData()
    {
        myCollider.sharedMaterial.friction = basicFriction;
    }

    protected void Flip()
    {
        transform.Rotate(0, -180f, 0);
        RightFacing = !RightFacing;
    }

    void SetCrouchingToFalse()
    {
        if (!anim.GetBool(hashOfIsCrouching))
            IsCrouching = false;
    }
}