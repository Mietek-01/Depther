using UnityEngine;

public class WeaponDamageField : MonoBehaviour
{
    int whatIsEnemy;
    int whatIsPlatform;
    bool collidedWithPlatform = false;

    Weapon weapon;

    // Start is called before the first frame update
    void Awake()
    {
        weapon = GetComponentInParent<Weapon>();

        whatIsPlatform = LayerMask.NameToLayer("Platform");
        whatIsEnemy = LayerMask.NameToLayer("Enemy");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == whatIsPlatform)
            collidedWithPlatform = true;
        else if (!collidedWithPlatform && collision.gameObject.layer == whatIsEnemy)
            weapon.HitEnemy(collision.GetComponent<Enemy>());
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == whatIsPlatform)
            collidedWithPlatform = false;
    }
}
