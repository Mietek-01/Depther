using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerDashMoveCreator))]
[DefaultExecutionOrder(0)]// After the PlayerInput.cs and the PlayerDashMoveCreator.cs
public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float speed = 15f;
    [SerializeField] float jumpForce = 1500f;
    [SerializeField] float basicFrictionValue = 0.008f;
    [SerializeField] LayerMask whatIsPlatform;
    [SerializeField] Collider2D groundCheck;

    public bool RightFacing { get; private set; } = true;

    public bool IsGrounded { get; private set; }

    public bool Frozen { get; private set; }

    public bool WasGrounded { get; private set; }

    public bool IsCrouching { get; private set; }

    public bool Jump { get; private set; }

    public Vector2 Velocity => velocity;

    Vector2 velocity = Vector2.zero;
    Vector3 mousePosition;

    [Header("Wall Sliding")]
    [SerializeField] float wallSlidingSpeed = 2f;
    [SerializeField] float xWallForce = 15f;
    [SerializeField] float yWallForce = 30f;
    [SerializeField] float wallJumpingTime = 0.05f;
    [SerializeField] Collider2D frontCheck;

    bool wallSliding = false;
    bool wallJumping = false;

    [Header("FX")]
    [SerializeField] GameObject dismemberedPlayerPrefab;
    [SerializeField] GameObject boomFX;
    [SerializeField] GameObject explosionFX;

    [SerializeField] Material dissolveMaterial;

    [Header("Audio")]
    [SerializeField] AudioClip[] jumpsClip;
    [SerializeField] AudioClip deathClip;
    [SerializeField] AudioClip boomDeathClip;
    [SerializeField] AudioClip dropClip;

    public bool Immortality { get; set; }

    public bool IamDead { get; private set; }

    public enum KindOfDeath
    {
        DISSOLVE,
        BOOM,
        DIVISION,
        EXPLOSION,
        FALL
    }

    readonly int hashOfIsJumping = Animator.StringToHash("isJumping");
    readonly int hashOfIsRunning = Animator.StringToHash("isRunning");
    readonly int hashOfIsCrouching = Animator.StringToHash("isCrouching");
    readonly int hashOfDisolve = Animator.StringToHash("disolve");

    IPlayerInputData inputData;
    IPlayerInputManagement inputManagement;

    Animator anim;
    Rigidbody2D rb;
    CircleCollider2D playerCollider;

    Weapon weapon;
    ReversalSetter aimingControler;
    PlayerDashMoveCreator dashMoveCreator;

    void Awake()
    {
        SetReferences();

        SetData();
    }

    void Update()
    {
        if (IsCrouching)
            StandCheck();
        else
        {
            GroundedStateUpdate();

            LandCheck();

            CrouchCheck();

            if (!IsCrouching)
            {
                DashMoveCheck();

                if (!dashMoveCreator.enabled)
                {
                    JumpCheck();

                    WallSlidingCheck();

                    WallJumpingCheck();

                    UpdateAiming();

                    ShotCheck();
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (IsCrouching)
            return;

        HorizontalMovement();

        AirMovement();
    }

    #region Awake Methods

    void SetReferences()
    {
        AssignPlayerInputDataInterface(GetComponent<IPlayerInputData>());

        inputManagement = GetComponent<IPlayerInputManagement>();
        dashMoveCreator = GetComponent<PlayerDashMoveCreator>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        playerCollider = GetComponent<CircleCollider2D>();
        aimingControler = GetComponentInChildren<ReversalSetter>();
        weapon = GetComponentInChildren<Weapon>();
    }

    void SetData()
    {
        Immortality = CheatsSetter.CheckCheatActivity("Immortality");

        playerCollider.sharedMaterial.friction = basicFrictionValue;

        try
        {
            GameplayManager.CinemachineCamera.Follow = transform;

            GlobalCursor.SetCursorSprite(GlobalCursor.KindOfCursor.GAMEPLAY);

        }
        catch (System.NullReferenceException e)
        {
            Debug.LogWarning("Probably the player is created during the Unit Test, " +
                "so GameplayManager and GlobalCursor dont exist");
        }
    }

    #endregion

    #region Checks

    void StandCheck()
    {
        if (inputData.StandUp)
        {
            playerCollider.sharedMaterial.friction = basicFrictionValue;
            anim.SetBool(hashOfIsCrouching, false);

            // Time for animation
            Invoke("SetCrouchingToFalse", .3f);
        }
    }

    void CrouchCheck()
    {
        if (IsGrounded && inputData.Crouch)
        {
            // The friction is less if the player is after dash move
            playerCollider.sharedMaterial.friction = dashMoveCreator.AfterDashing ? .02f : .1f;

            anim.SetBool(hashOfIsCrouching, true);
            weapon.DestroyStrongBullet();
            IsCrouching = true;

            AudioManager.PlayPlayerVoices(jumpsClip[Random.Range(0, jumpsClip.Length)]);
        }
    }

    void GroundedStateUpdate()
    {
        WasGrounded = IsGrounded;

        IsGrounded = groundCheck.IsTouchingLayers(whatIsPlatform);
    }

    void LandCheck()
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

    void JumpCheck()
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

    void WallSlidingCheck()
    {
        wallSliding = !IsGrounded && inputData.HorizontalMovement != 0f
            && frontCheck.IsTouchingLayers(whatIsPlatform) && !wallJumping;
    }

    void WallJumpingCheck()
    {
        if (wallSliding && inputData.Jump)
        {
            wallJumping = true;

            Invoke("SetWallJumpingToFalse", wallJumpingTime);

            AudioManager.PlayPlayerVoices(jumpsClip[Random.Range(0, jumpsClip.Length)]);
        }
    }

    void DashMoveCheck()
    {
        if (inputData.LeftDoubleTap)
            dashMoveCreator.Activate(PlayerDashMoveCreator.Type.LEFT);
        else if (inputData.RightDoubleTap)
            dashMoveCreator.Activate(PlayerDashMoveCreator.Type.RIGHT);
        else if (inputData.UpperDoubleTap)
            dashMoveCreator.Activate(PlayerDashMoveCreator.Type.UPPER);
    }

    void ShotCheck()
    {
        weapon.ShotCheck();
    }

    void UpdateAiming()
    {
        mousePosition = GlobalCursor.Position;

        if (mousePosition.x > transform.position.x && !RightFacing)
            Flip();
        else if (mousePosition.x < transform.position.x && RightFacing)
            Flip();

        aimingControler.SetReversal(mousePosition);
    }

    #endregion

    #region Physical Methods

    void HorizontalMovement()
    {
        velocity.x = inputData.HorizontalMovement * speed * Time.fixedDeltaTime;
        velocity.y = rb.velocity.y;

        rb.velocity = velocity;

        anim.SetBool(hashOfIsRunning, inputData.HorizontalMovement != 0f);
    }

    void AirMovement()
    {
        if (Jump)
        {
            rb.AddForce(new Vector2(0, jumpForce));
            Jump = false;
        }

        if (wallSliding)
        {
            velocity.x = rb.velocity.x;
            velocity.y = Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, 100f);

            rb.velocity = velocity;
        }

        if (wallJumping)
        {
            velocity.x = xWallForce * -inputData.HorizontalMovement * Time.fixedDeltaTime;
            velocity.y = yWallForce;
            rb.velocity = velocity;
        }
    }

    #endregion

    #region Public Methods

    public void AssignPlayerInputDataInterface(IPlayerInputData playerInputData)
    {
        if (playerInputData == null)
            Debug.LogError("I have no access to the input");
        else
            inputData = playerInputData;
    }

    public bool Kill(KindOfDeath kind)
    {
        if ((Immortality && kind != KindOfDeath.FALL) || IamDead || dashMoveCreator.enabled
            || WinLoseController.IsWin || WinLoseController.IsLose)
            return false;

        IamDead = true;
        enabled = false;

        GameplayManager.EnableGameplayTimer(false);

        GlobalCursor.Visible = false;

        inputManagement.Enabled = false;

        dashMoveCreator.DisableEyes();

        // Destroy potentially strong bullet
        weapon.DestroyStrongBullet();

        // Perform the sent kind of death
        switch (kind)
        {
            case KindOfDeath.FALL:
                {
                    FallDeath();
                    break;
                }

            case KindOfDeath.EXPLOSION:
                {
                    ExplosionDeath();
                    break;
                }

            case KindOfDeath.DISSOLVE:
                {
                    DissolveDeath();
                    break;
                }

            case KindOfDeath.DIVISION:
                {
                    DivisionDeath();
                    break;
                }

            case KindOfDeath.BOOM:
                {
                    BoomDeath();
                    break;

                }
        }

        return true;
    }

    public void MakeRecoil(Vector2 force)
    {
        if (RightFacing)
            rb.AddForceAtPosition(new Vector2(-force.x, force.y), aimingControler.transform.position);
        else
            rb.AddForceAtPosition(force, aimingControler.transform.position);
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

    #endregion

    #region Death Methods

    private void BoomDeath()
    {
        Instantiate(boomFX, transform.position, Quaternion.identity, GameplayManager._DynamicOfCurrentZone);

        GameplayManager.StartGameplayReset(4f);

        AudioManager.PlayPlayerVoices(boomDeathClip);

        Destroy(gameObject);
    }

    private void DivisionDeath()
    {
        var dismemberedPlayer = Instantiate(dismemberedPlayerPrefab, transform.position
            , transform.rotation, GameplayManager._DynamicOfCurrentZone)
            .GetComponent<DismemberedPlayer>();

        var bodyParts = dismemberedPlayer.GetComponentsInChildren<Rigidbody2D>();
        Vector2 myVelocity = rb.velocity;

        // Assign dismemberd body the player velocity
        for (int i = 1; i < bodyParts.Length; i++)
            bodyParts[i].velocity = myVelocity * 1.05f;

        // Makes setting easier
        System.Func<Transform, Transform, bool> AplayFromTo = (from, to) =>
        {
            to.localPosition = from.localPosition;
            to.localEulerAngles = from.localEulerAngles;
            to.localScale = from.localScale;

            return true;
        };

        // Set dismembered elements
        AplayFromTo(transform.Find("Body"), bodyParts[0].transform);

        AplayFromTo(transform.Find("Body").transform.Find("Shield"), bodyParts[1].transform);

        AplayFromTo(transform.Find("Body").transform.Find("Head"), bodyParts[2].transform);

        AplayFromTo(transform.Find("Leg Left"), bodyParts[3].transform);

        AplayFromTo(transform.Find("Leg Right"), bodyParts[4].transform);

        GameplayManager.StartGameplayReset(4f + dismemberedPlayer.WhenStartDissolve);

        AudioManager.PlayPlayerVoices(deathClip);

        Destroy(gameObject);
    }

    private void DissolveDeath()
    {
        anim.SetTrigger(hashOfDisolve);

        rb.velocity = new Vector2(0, rb.velocity.y);

        // Change materials on dissolve material
        transform.Find("Body").GetComponent<SpriteRenderer>().material = dissolveMaterial;
        transform.Find("Body").transform.Find("Shield").GetComponent<SpriteRenderer>().material = dissolveMaterial;
        transform.Find("Body").transform.Find("Head").GetComponent<SpriteRenderer>().material = dissolveMaterial;
        transform.Find("Leg Left").GetComponent<SpriteRenderer>().material = dissolveMaterial;
        transform.Find("Leg Right").GetComponent<SpriteRenderer>().material = dissolveMaterial;

        GameplayManager.StartGameplayReset(5f);

        AudioManager.PlayPlayerVoices(deathClip);

        Destroy(weapon.gameObject);
        Destroy(gameObject, 3f);
    }

    private void FallDeath()
    {
        enabled = false;

        Invoke("DisableCameraFollow", .5f);

        GameplayManager.StartGameplayReset(4f);

        AudioManager.PlayPlayerVoices(deathClip);

        Destroy(gameObject, 2f);
    }

    private void ExplosionDeath()
    {
        Destroy(Instantiate(explosionFX, transform.position, Quaternion.identity
        , GameplayManager._DynamicOfCurrentZone), 5f);

        GameplayManager.StartGameplayReset(4f);

        AudioManager.PlayPlayerVoices(deathClip);

        Destroy(gameObject);
    }

    #endregion

    #region Others

    void Flip()
    {
        transform.Rotate(0, -180f, 0);
        RightFacing = !RightFacing;
    }

    void SetWallJumpingToFalse()
    {
        wallJumping = false;
    }

    void SetCrouchingToFalse()
    {
        if (!anim.GetBool(hashOfIsCrouching))
            IsCrouching = false;
    }

    void DisableCameraFollow()
    {
        GameplayManager.CinemachineCamera.Follow = null;
    }

    #endregion
}
