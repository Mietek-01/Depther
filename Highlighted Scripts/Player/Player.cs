using UnityEngine;

[RequireComponent(typeof(PlayerDashMoveCreator))]
[DefaultExecutionOrder(0)]// After the PlayerInput.cs
public partial class Player : PlayerCharacter
{
    [Header("Player")]
    [SerializeField] WallMovementSO wallMovementSO;
    [SerializeField] Collider2D frontCheck;

    [Header("FX")]
    [SerializeField] GameObject dismemberedPlayerPrefab;
    [SerializeField] GameObject boomFX;
    [SerializeField] GameObject explosionFX;

    [SerializeField] Material dissolutionMaterial;

    [Header("Audio")]
    [SerializeField] AudioClip deathClip;
    [SerializeField] AudioClip boomDeathClip;

    readonly int hashOfDissolution = Animator.StringToHash("disolve");

    Weapon weapon;
    ReversalSetter aimingControler;
    PlayerDashMoveCreator dashMoveCreator;

    protected override void Update()
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

    protected override void SetSubscribers()
    {
        OnProtect += (whoIsAttacking) =>
        {
            var myKiller = whoIsAttacking.GetComponent<IPlayerKiller>();

            if (myKiller == null)
                return true;

            if ((Immortality && myKiller.PlayerDeath != KindOfDeath.FALL) || IamDead || dashMoveCreator.enabled
                || WinLoseController.IsWin || WinLoseController.IsLose)
                return true;

            return false;
        };

        OnDie += (whoIsAttacking) => Die(whoIsAttacking.GetComponent<IPlayerKiller>().PlayerDeath);
    }

    protected override void SetReferences()
    {
        base.SetReferences();

        dashMoveCreator = GetComponent<PlayerDashMoveCreator>();
        aimingControler = GetComponentInChildren<ReversalSetter>();
        weapon = GetComponentInChildren<Weapon>();
    }

    protected override void SetData()
    {
        base.SetData();

        Immortality = CheatsSetter.CheckCheatActivity("Immortality");

        try
        {
            GameplayManager.Player = this;

            GameplayManager.CinemachineCamera.Follow = transform;

            GlobalCursor.SetCursorType(GlobalCursor.CursorType.GAMEPLAY);

        }
        catch
        {
            Debug.LogWarning("Probably the player is created during the Unit Test, " +
                "so GameplayManager and GlobalCursor dont exist");
        }
    }

    protected override void AirMovement()
    {
        base.AirMovement();

        if (wallMovementSO.WallSliding)
        {
            velocity.x = rb.velocity.x;
            velocity.y = Mathf.Clamp(rb.velocity.y, -wallMovementSO.wallSlidingSpeed, 100f);

            rb.velocity = velocity;
        }

        if (wallMovementSO.WallJumping)
        {
            velocity.x = wallMovementSO.xWallForce * -inputData.HorizontalMovement * Time.fixedDeltaTime;
            velocity.y = wallMovementSO.yWallForce;
            rb.velocity = velocity;
        }
    }

    #region Checks

    protected override void CrouchCheck()
    {
        if (IsGrounded && inputData.Crouch)
        {
            myCollider.sharedMaterial.friction = dashMoveCreator.AfterDashing ? .02f : .1f;

            anim.SetBool(hashOfIsCrouching, true);
            weapon.DestroyStrongBullet();
            IsCrouching = true;

            AudioManager.PlayPlayerVoices(jumpsClip[Random.Range(0, jumpsClip.Length)]);
        }
    }

    void WallSlidingCheck()
    {
        wallMovementSO.WallSliding = !IsGrounded && inputData.HorizontalMovement != 0f
            && frontCheck.IsTouchingLayers(whatIsPlatform) && !wallMovementSO.WallJumping;
    }

    void WallJumpingCheck()
    {
        if (wallMovementSO.WallSliding && inputData.Jump)
        {
            wallMovementSO.WallJumping = true;

            Invoke("SetWallJumpingToFalse", wallMovementSO.wallJumpingTime);

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
        if (GlobalCursor.Position.x > transform.position.x && !RightFacing)
            Flip();
        else if (GlobalCursor.Position.x < transform.position.x && RightFacing)
            Flip();

        aimingControler.SetReversal(GlobalCursor.Position);
    }

    #endregion

    #region Death Methods

    private void BoomDeath()
    {
        Instantiate(boomFX, transform.position, Quaternion.identity, GameplayManager.DynamicContainerOfCurrentZone);

        MyDeath(boomDeathClip, 4f, 0f);
    }

    private void DivisionDeath()
    {
        var dismemberedPlayer = Instantiate(dismemberedPlayerPrefab, transform.position
            , transform.rotation, GameplayManager.DynamicContainerOfCurrentZone)
            .GetComponent<DismemberedPlayer>();

        var bodyParts = dismemberedPlayer.GetComponentsInChildren<Rigidbody2D>();

        Vector2 myVelocity = rb.velocity;

        for (int i = 1; i < bodyParts.Length; i++)
            bodyParts[i].velocity = myVelocity * 1.05f;

        // Set dismembered elements
        UsefulFunctions.CopyTransform(transform.Find("Body"), bodyParts[0].transform);

        UsefulFunctions.CopyTransform(transform.Find("Body").transform.Find("Shield"), bodyParts[1].transform);

        UsefulFunctions.CopyTransform(transform.Find("Body").transform.Find("Head"), bodyParts[2].transform);

        UsefulFunctions.CopyTransform(transform.Find("Leg Left"), bodyParts[3].transform);

        UsefulFunctions.CopyTransform(transform.Find("Leg Right"), bodyParts[4].transform);

        MyDeath(deathClip, 4f + dismemberedPlayer.WhenStartDissolve, 0f);
    }

    private void DissolutionDeath()
    {
        anim.SetTrigger(hashOfDissolution);

        rb.velocity = new Vector2(0, rb.velocity.y);

        // Change materials on dissolution material
        transform.Find("Body").GetComponent<SpriteRenderer>().material = dissolutionMaterial;
        transform.Find("Body").transform.Find("Shield").GetComponent<SpriteRenderer>().material = dissolutionMaterial;
        transform.Find("Body").transform.Find("Head").GetComponent<SpriteRenderer>().material = dissolutionMaterial;
        transform.Find("Leg Left").GetComponent<SpriteRenderer>().material = dissolutionMaterial;
        transform.Find("Leg Right").GetComponent<SpriteRenderer>().material = dissolutionMaterial;

        Destroy(weapon.gameObject);

        MyDeath(deathClip, 5f, 3f);
    }

    private void FallDeath()
    {
        enabled = false;

        Invoke(nameof(DisableCameraFollow), .5f);

        MyDeath(deathClip, 4f, 2f);
    }

    private void ExplosionDeath()
    {
        Destroy(Instantiate(explosionFX, transform.position, Quaternion.identity
        , GameplayManager.DynamicContainerOfCurrentZone), 5f);

        MyDeath(deathClip, 4f, 0f);
    }

    private void MyDeath(AudioClip clip, float whenGameplayReset, float whenDestroy)
    {
        GameplayManager.StartGameplayReset(whenGameplayReset);

        AudioManager.PlayPlayerVoices(clip);

        Destroy(gameObject, whenDestroy);
    }

    #endregion

    #region Others

    private void Die(KindOfDeath kindOfDeath)
    {
        IamDead = true;
        enabled = false;

        GameplayManager.Player = null;
        GameplayManager.EnableGameplayTimer(false);

        GlobalCursor.Visible = false;

        inputManagement.Enabled = false;

        dashMoveCreator.DisableEyes();

        // Destroy potentially strong bullet
        weapon.DestroyStrongBullet();

        switch (kindOfDeath)
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

            case KindOfDeath.DISSOLUTION:
                {
                    DissolutionDeath();
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
    }

    void SetWallJumpingToFalse()
    {
        wallMovementSO.WallJumping = false;
    }

    void DisableCameraFollow()
    {
        GameplayManager.CinemachineCamera.Follow = null;
    }

    /// <summary>
    /// I need this method for Editor scripts and Unit Tests
    /// </summary>
    /// <param name="kindOfDeath"></param>
    public void SuddenDeath(KindOfDeath kindOfDeath)
    {
        Die(kindOfDeath);
    }

    #endregion
}
