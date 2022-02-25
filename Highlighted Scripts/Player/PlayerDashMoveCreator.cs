using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Player))]
[DefaultExecutionOrder(-5)]// Before the Player.cs
public class PlayerDashMoveCreator : MonoBehaviour
{
    [SerializeField] float duration = .12f;
    [SerializeField] float speed = 90f;
    [SerializeField] int echosNumber = 5;
    [SerializeField] float echoDisappearingFrequancy = .2f;
    [SerializeField] string dashMoveFXName = "DashMoveFX";
    [SerializeField] string echoFXName = "PlayerEcho";

    [Header("Ref")]
    [SerializeField] ParticleSystem lCanDashFX;
    [SerializeField] ParticleSystem rCanDashFX;
    [SerializeField] CinemachineImpulseSource myShaking;

    [SerializeField] AudioClip dashMoveClip;

    public bool AfterDashing { get; private set; } = false;

    public enum Type
    {
        LEFT,
        RIGHT,
        UPPER
    }

    float timeOfDashMove;

    Type dashMoveType;
    Vector3 echoPosition;
    Vector2 velocity = Vector2.zero;

    IPlayerInputManagement inputManagement;
    Player player;
    Rigidbody2D rb;

    // Start is called before the first frame update
    void Awake()
    {
        inputManagement = GetComponent<IPlayerInputManagement>();
        player = GetComponent<Player>();
        rb = GetComponent<Rigidbody2D>();

        enabled = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        DashMoveUpdate();
    }

    public void Activate(Type dashMoveType)
    {
        if (enabled)
        {
            Debug.LogWarning("You are trying to activate dash move twice");
            return;
        }

        this.dashMoveType = dashMoveType;

        // Enable fixedUpdate that make dash move
        enabled = true;

        // !!! Very important because during the dash move updates in player must be stoped !!!
        player.enabled = false;

        // Position of first echo FX
        echoPosition = transform.position;

        timeOfDashMove = duration;

        DisableEyes();

        EnableDashMoveInput(false);

        myShaking.GenerateImpulse();

        CreateDashMoveFX();

        AudioManager.PlayPlayerSFX(dashMoveClip);
    }

    public void DisableEyes()
    {
        lCanDashFX.gameObject.SetActive(false);
        rCanDashFX.gameObject.SetActive(false);
    }

    void DashMoveUpdate()
    {
        if (timeOfDashMove <= 0f)
            End();
        else
        {
            switch (dashMoveType)
            {
                case Type.RIGHT:
                    {
                        velocity.x = speed;
                        velocity.y = rb.velocity.y;

                        break;
                    }

                case Type.LEFT:
                    {
                        velocity.x = -speed;
                        velocity.y = rb.velocity.y;

                        break;
                    }

                case Type.UPPER:
                    {
                        velocity.x = rb.velocity.x;
                        velocity.y = speed;

                        break;
                    }
            }

            rb.velocity = velocity;
            timeOfDashMove -= Time.fixedDeltaTime;
        }
    }

    void End()
    {
        float whenAvailable;

        if (dashMoveType != Type.UPPER)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);

            // After that time dash move will be available again
            whenAvailable = (float)Random.Range(1, 10) * 0.2f + 1.5f;
        }
        else
        {
            rb.velocity = new Vector2(rb.velocity.x, 10);

            // After upper dash move next one will be available faster
            whenAvailable = (float)Random.Range(1, 10) * 0.1f;
        }

        // After that time dash move will be available again
        Invoke("Available", whenAvailable);

        // I have done my work so updates are not required
        enabled = false;

        // Enable player updates
        player.enabled = true;

        // This information can be useful
        AfterDashing = true;

        Invoke("SetAfterDashingToFalse", 0.7f);

        // Echo FX
        CreateEchoFX();
    }

    void Available()
    {
        if (player.IamDead)
            return;

        // Enable eyes
        lCanDashFX.gameObject.SetActive(true);
        rCanDashFX.gameObject.SetActive(true);

        lCanDashFX.Play();
        rCanDashFX.Play();

        EnableDashMoveInput(true, dashMoveType == Type.UPPER);
    }

    void CreateEchoFX()
    {
        Vector3 road = transform.position - echoPosition;
        Vector3 distanceBetweenEchoes = road / echosNumber;

        float whenStartDisappearing = 0f;

        for (int i = 1; i <= echosNumber; i++)
        {
            var echo = ObjectsPooler.SpawnObjectfFromThePool(echoFXName, echoPosition
                , transform.rotation, GameplayManager._DynamicOfCurrentZone).GetComponent<Echo>();

            echo.SetWhenDisappear(whenStartDisappearing, i);

            whenStartDisappearing += echoDisappearingFrequancy;

            echoPosition += distanceBetweenEchoes;
        }
    }

    void CreateDashMoveFX()
    {
        Vector3 rotation;

        if (dashMoveType != Type.UPPER)
            rotation = dashMoveType == Type.RIGHT ? new Vector3(180, 180, 0) : Vector3.zero;
        else
        {
            if (rb.velocity.x == 0f)
                rotation = player.RightFacing ? new Vector3(180, 180, 0) : Vector3.zero;
            else
                rotation = rb.velocity.x > 0f ? new Vector3(180, 180, 0) : Vector3.zero;
        }

        ObjectsPooler.PlayParticleSystem(dashMoveFXName, transform.position
            , Quaternion.Euler(rotation), GameplayManager._DynamicOfCurrentZone);
    }

    void EnableDashMoveInput(bool value, bool withoutUpperDashMove = false)
    {
        if (player.Frozen)
            return;

        inputManagement.EnableInputFor("LeftDashMove", value);
        inputManagement.EnableInputFor("RightDashMove", value);

        if (!withoutUpperDashMove)
            inputManagement.EnableInputFor("UpperDashMove", value);
    }

    void SetAfterDashingToFalse()
    {
        AfterDashing = false;
    }
}
