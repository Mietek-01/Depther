using UnityEngine;

[DefaultExecutionOrder(0)]// After the PlayerInput script and the PlayerDashMoveCreator script
public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float speed = 15f;
    [SerializeField] float jumpForce = 1500f;
    [SerializeField] float basicFrictionValue = 0.008f;
    [SerializeField] LayerMask whatIsPlatform;

    [SerializeField] CircleCollider2D playerCollider;
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

    public IPlayerInputData InputData { get; private set; }

    public IPlayerInputManagement InputManagement { get; private set; }

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

    Animator anim;
    Rigidbody2D rb;

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

        InputManagement = GetComponent<IPlayerInputManagement>();
        dashMoveCreator = GetComponent<PlayerDashMoveCreator>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        aimingControler = GetComponentInChildren<ReversalSetter>();
        weapon = FindObjectOfType<Weapon>();
    }

    void SetData()
    {
        Immortality = CheatsSetter.CheckCheatActivity("Immortality");

        GetComponent<CircleCollider2D>().sharedMaterial.friction = basicFrictionValue;

        try
        {
            GameplayManager.CinemachineCamera.Follow = transform;

            GlobalCursor.SetCursorSprite(GlobalCursor.KindOfCursor.GAMEPLAY);

        }
        catch (System.NullReferenceException e)
        {
            Debug.LogWarning("Probably the player is created during the Unit Test so " +
                " the GameplayManager and GlobalCursor dont exist");
        }
    }

    #endregion

    #region Checks

    void StandCheck()
    {
        if (InputData.StandUp)
        {
            playerCollider.sharedMaterial.friction = basicFrictionValue;
            anim.SetBool(hashOfIsCrouching, false);

            // Time for animation
            Invoke("SetCrouchingToFalse", .3f);
        }
    }

    void CrouchCheck()
    {
        if (IsGrounded && InputData.Crouch)
        {
            // The friction adequate if after dash move
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
            AudioManager.PlayPlayerVoices(dropClip);
            anim.SetBool(hashOfIsJumping, false);

            // Enable UP dash move
            if (!Frozen)
                InputManagement.EnableInputFor("UpperDashMove", true);
        }
    }

    void JumpCheck()
    {
        if (IsGrounded && InputData.Jump)
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
        wallSliding = !IsGrounded && InputData.HorizontalMovement != 0f
            && frontCheck.IsTouchingLayers(whatIsPlatform) && !wallJumping;
    }

    void WallJumpingCheck()
    {
        if (wallSliding && InputData.Jump)
        {
            AudioManager.PlayPlayerVoices(jumpsClip[Random.Range(0, jumpsClip.Length)]);

            wallJumping = true;

            Invoke("SetWallJumpingToFalse", wallJumpingTime);
        }
    }

    void DashMoveCheck()
    {
        if (InputData.LeftDoubleTap)
            dashMoveCreator.Activate(PlayerDashMoveCreator.Type.LEFT);
        else if (InputData.RightDoubleTap)
            dashMoveCreator.Activate(PlayerDashMoveCreator.Type.RIGHT);
        else if (InputData.UpperDoubleTap)
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
        velocity.x = InputData.HorizontalMovement * speed * Time.fixedDeltaTime;
        velocity.y = rb.velocity.y;

        rb.velocity = velocity;

        anim.SetBool(hashOfIsRunning, InputData.HorizontalMovement != 0f);
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
            velocity.x = xWallForce * -InputData.HorizontalMovement * Time.fixedDeltaTime;
            velocity.y = yWallForce;
            rb.velocity = velocity;
        }
    }

    #endregion

    #region Public Methods

    public bool Kill(KindOfDeath kind)
    {
        if ((Immortality && kind != KindOfDeath.FALL) || IamDead || dashMoveCreator.enabled
            || WinLoseController.IsWin || WinLoseController.IsLose)
            return false;

        IamDead = true;
        enabled = false;

        // Disable a gameplay timer
        GameplayManager.EnableGameplayTimer(false);

        // Disable the cursor
        GlobalCursor.Visible = false;

        // Disable eyes
        dashMoveCreator.DisableEyes();

        // Disable input
        InputManagement.Enabled = false;

        // Deactivate potentially active the strong bullet
        weapon.DestroyStrongBullet();

        // Execute sent kind of death
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
            rb.AddForceAtPosition(new Vector2(-force.x, force.y)
                , aimingControler.transform.position);
        else
            rb.AddForceAtPosition(force, aimingControler.transform.position);
    }

    public void FreezeInput(bool value, bool withRigidBody)
    {
        Frozen = value;

        InputManagement.EnableInputFor("Crouch", !value);
        InputManagement.EnableInputFor("Jump", !value);
        InputManagement.EnableInputFor("Shot", !value);
        InputManagement.EnableInputFor("HorizontalMovement", !value);

        InputManagement.EnableInputFor("LeftDashMove", !value);
        InputManagement.EnableInputFor("RightDashMove", !value);
        InputManagement.EnableInputFor("UpperDashMove", !value);

        rb.velocity = Vector2.zero;

        // I may want to freeze the player with physics as wall ( e.g. during the ZS Field)
        if (withRigidBody)
            rb.simulated = !value;
    }

    public void AssignPlayerInputDataInterface(IPlayerInputData playerInputData)
    {
        if (playerInputData == null)
            Debug.LogError("I have no access to the input");

        InputData = playerInputData;
    }

    #endregion

    #region Death Methods

    private void BoomDeath()
    {
        Instantiate(boomFX, transform.position, Quaternion.identity
            , GameplayManager._DynamicOfCurrentZone);

        AudioManager.PlayPlayerVoices(boomDeathClip);

        Destroy(gameObject);

        GameplayManager.StartGameplayReset(4f);
    }

    private void DivisionDeath()
    {
        var dismemberedPlayer = Instantiate(dismemberedPlayerPrefab, transform.position
            , transform.rotation, GameplayManager._DynamicOfCurrentZone)
            .GetComponent<DismemberedPlayer>();

        Vector2 myVelocity = rb.velocity;

        Rigidbody2D[] partsOfBody = dismemberedPlayer.GetComponentsInChildren<Rigidbody2D>();

        // Assign dismemberd body the player velocity
        for (int i = 1; i < partsOfBody.Length; i++)
            partsOfBody[i].velocity = myVelocity * 1.05f;

        // It makes setting so much easier
        System.Func<Transform, Transform, bool> AplayFromTo = (from, to) =>
        {
            to.localPosition = from.localPosition;
            to.localEulerAngles = from.localEulerAngles;
            to.localScale = from.localScale;

            return true;
        };

        // Setting dismembered elements
        AplayFromTo(transform.Find("Body"), partsOfBody[0].transform);

        AplayFromTo(transform.Find("Body").transform.Find("Shield")
            , partsOfBody[1].transform);

        AplayFromTo(transform.Find("Body").transform.Find("Head")
            , partsOfBody[2].transform);

        AplayFromTo(transform.Find("Leg Left"), partsOfBody[3].transform);

        AplayFromTo(transform.Find("Leg Right"), partsOfBody[4].transform);

        Destroy(gameObject);

        GameplayManager.StartGameplayReset(4f + dismemberedPlayer.WhenStartDissolve);

        AudioManager.PlayPlayerVoices(deathClip);
    }

    private void DissolveDeath()
    {
        anim.SetTrigger(hashOfDisolve);

        rb.velocity = new Vector2(0, rb.velocity.y);

        // Change materials in all my renderers for dissolve material
        transform.Find("Body").GetComponent<SpriteRenderer>()
            .material = dissolveMaterial;
        transform.Find("Body").transform.Find("Shield")
            .GetComponent<SpriteRenderer>().material = dissolveMaterial;
        transform.Find("Body").transform.Find("Head")
            .GetComponent<SpriteRenderer>().material = dissolveMaterial;
        transform.Find("Leg Left").GetComponent<SpriteRenderer>()
            .material = dissolveMaterial;
        transform.Find("Leg Right").GetComponent<SpriteRenderer>()
            .material = dissolveMaterial;

        Destroy(weapon.gameObject);
        Destroy(gameObject, 3f);

        GameplayManager.StartGameplayReset(5f);

        AudioManager.PlayPlayerVoices(deathClip);
    }

    private void FallDeath()
    {
        enabled = false;

        Invoke("DisableCameraFollow", .5f);

        Destroy(gameObject, 2f);

        GameplayManager.StartGameplayReset(4f);

        AudioManager.PlayPlayerVoices(deathClip);
    }

    private void ExplosionDeath()
    {
        Destroy(Instantiate(explosionFX, aimingControler.transform.position
            , Quaternion.identity, transform.root), 5f);

        AudioManager.PlayPlayerVoices(deathClip);

        Destroy(gameObject);

        GameplayManager.StartGameplayReset(4f);
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