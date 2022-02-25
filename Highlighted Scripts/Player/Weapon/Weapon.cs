using UnityEngine;
using Cinemachine;

public class Weapon : MonoBehaviour
{
    [SerializeField] bool strongShotEnlargement = false;
    [SerializeField] int weaponDamage = 2;

    [SerializeField] Transform attackPoint;
    [SerializeField] GameObject hitFX;

    [Header("Bullets")]
    [SerializeField] GameObject strongBulletPrefab;
    [SerializeField] string basicBulletName = "PlayerBullet";

    [Header("Audio")]
    [SerializeField] AudioClip basicShotClip;
    [SerializeField] AudioClip enemyHitClip;

    readonly int hashOfBasicShot = Animator.StringToHash("BasicShot");
    readonly int hashOfStrongBulletLoading = Animator.StringToHash("StrongBulletLoading");
    readonly int hashOfHit = Animator.StringToHash("Hit");

    IPlayerInputData playerInputData;

    WeaponCollider weaponCollider;
    ReversalSetter aimingControler;
    Animator anim;

    GameObject myStrongBullet;

    bool canHit = true;
    bool canShoot = true;

    bool shotInitiate = false;
    bool IAmShooting = false;

    readonly Vector2 recoilForce = new Vector2(3000, 500);

    const float whenCreateStrongBullet = .5f;

    const float shootingFreaquency = .01f;
    const float hitFreaquency = .4f;

    float clikingTime = 0f;

    // This is a really important variable because it determines wrong shooting area,meaning 
    // the field in which chroshair is in front of the attack point instead of being behind it
    float radiusOfIncorrectShootingArea;

    // Start is called before the first frame update
    void Awake()
    {
        playerInputData = GetComponentInParent<IPlayerInputData>();
        weaponCollider = GetComponentInChildren<WeaponCollider>();
        anim = GetComponent<Animator>();
        aimingControler = GetComponentInParent<ReversalSetter>();

        radiusOfIncorrectShootingArea = (attackPoint.position - transform.position).magnitude + 1f;
    }

    public void ShotCheck()
    {
        if (!canShoot || IAmShooting)
            return;

        if (strongShotEnlargement)
        {
            // Check shot initiate
            if (!shotInitiate && !weaponCollider.BlockedShooting && playerInputData.Shot)
                shotInitiate = true;

            // In this mode the shot is released when the mouse button is released insted of cliking
            if (shotInitiate && ReleaseCheck())
                // The length of clicking determine kind of bullet 
                Shot(clikingTime < whenCreateStrongBullet);
        }
        else
            if (playerInputData.Shot)
            Shot(true);
    }

    #region Shooting Methods

    void Shot(bool basicShot)
    {
        // I cant shoot when the weapon is colliding with something
        if (weaponCollider.BlockedShooting)
        {
            if (basicShot)
                MyReset();
            else
                DestroyStrongBullet();
        }
        else
        {
            IAmShooting = true;

            if (basicShot)
                anim.SetTrigger(hashOfBasicShot);
            else
                StrongShot();
        }
    }

    // Call by animator    
    public void BasicShot()
    {
        MyReset();

        if (weaponCollider.BlockedShooting)
            return;

        var bullet = ObjectsPooler.SpawnObjectfFromThePool(basicBulletName, attackPoint.transform.position
            , Quaternion.identity, GameplayManager._DynamicOfCurrentZone);

        // The direction of bullet is depends on the position of the cursor. If it is in the right area
        // and aimingController is not blocked then bullet will be shoot towards the cursor.
        // Otherwise, the bullet reversal will match with attackPoint
        bullet.transform.right = CheckIfGlobalCursorIsInProperArea(GlobalCursor.Position)
            && !aimingControler.Blocked ? GlobalCursor.Position - attackPoint.position : attackPoint.right;

        // Play a sound
        AudioManager.PlayPlayerSFX(basicShotClip);
    }

    void StrongShot()
    {
        // Loading lenght
        float ratio = anim.GetCurrentAnimatorStateInfo(0).normalizedTime;

        // I was loading too short
        if (ratio < .5f)
        {
            DestroyStrongBullet();
            return;
        }

        GetComponentInParent<Player>().MakeRecoil(recoilForce * (ratio + .1f));

        anim.SetBool(hashOfStrongBulletLoading, false);

        myStrongBullet.GetComponent<StrongPlayerBulet>().FireOff(ratio);

        myStrongBullet.transform.parent = GameplayManager._DynamicOfCurrentZone;

        myStrongBullet.transform.right = attackPoint.transform.right;

        myStrongBullet = null;

        MyReset();
    }

    #endregion

    public void HitEnemy(Enemy enemy)
    {
        if (!canHit || IAmShooting || shotInitiate)
            return;

        anim.SetTrigger(hashOfHit);

        enemy.TakeDamage(weaponDamage);

        // Still alive
        if (enemy.Health > 0)
        {
            Instantiate(hitFX, enemy.transform.position, Quaternion.identity
                , GameplayManager._DynamicOfCurrentZone);

            AudioManager.PlayPlayerSFX(enemyHitClip);
        }

        canHit = false;
        canShoot = false;

        Invoke("SetCanHitToTrue", hitFreaquency);
        Invoke("SetCanShootToTrue", .5f);
    }

    bool ReleaseCheck()
    {
        if (playerInputData.ReleasedShot)
            return true;
        else
        {
            clikingTime += Time.deltaTime;

            if (clikingTime > whenCreateStrongBullet)
            {
                if (weaponCollider.BlockedShooting)
                    DestroyStrongBullet();
                else if (!anim.GetBool(hashOfStrongBulletLoading))
                {
                    // Create strong bullet at attack point
                    myStrongBullet = Instantiate(strongBulletPrefab, attackPoint.position
                        , Quaternion.identity, attackPoint);

                    anim.SetBool(hashOfStrongBulletLoading, true);
                }
                else
                    // It must be here because I dont know why the but parent doesnt impact of it
                    myStrongBullet.transform.position = attackPoint.position;
            }

            return false;
        }
    }

    public void DestroyStrongBullet()
    {
        if (!myStrongBullet)
            return;

        CinemachineImpulseManager.Instance.Clear();

        Destroy(myStrongBullet);

        MyReset();

        anim.SetBool(hashOfStrongBulletLoading, false);
    }

    void MyReset()
    {
        Invoke("SetCanShootToTrue", shootingFreaquency);

        clikingTime = 0f;

        IAmShooting = false;
        canShoot = false;
        shotInitiate = false;
    }

    bool CheckIfGlobalCursorIsInProperArea(Vector3 globalCursorPosition)
    {
        Vector3 myPosition = transform.position;

        // The proper area means that the global cursor position is outside of the circle.
        // The circle has a center at my position and a radius that is qual radiusOfIncorrectShootingArea
        return !((globalCursorPosition.x - myPosition.x) * (globalCursorPosition.x - myPosition.x)
            + (globalCursorPosition.y - myPosition.y) * (globalCursorPosition.y - myPosition.y)
            <= radiusOfIncorrectShootingArea * radiusOfIncorrectShootingArea);
    }

    // Call by the end of StrongBulletLoading animation
    public void Boom()
    {
        if (!anim.GetBool(hashOfStrongBulletLoading))
            return;

        Destroy(myStrongBullet);

        CinemachineImpulseManager.Instance.Clear();

        GetComponentInParent<Player>().Kill(Player.KindOfDeath.EXPLOSION);
    }

    void SetCanHitToTrue()
    {
        canHit = true;
    }

    void SetCanShootToTrue()
    {
        canShoot = true;
    }
}
