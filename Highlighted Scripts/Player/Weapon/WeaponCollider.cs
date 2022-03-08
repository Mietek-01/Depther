using UnityEngine;

public class WeaponCollider : MonoBehaviour
{
    public bool BlockedShooting { get; private set; }

    LayerMask whatIsPlatform;

    Collider2D myCollider;

    private void Awake()
    {
        myCollider = GetComponent<Collider2D>();

        BlockedShooting = false;
        whatIsPlatform = LayerMask.GetMask("Platform");

    }

    // Using OnEnter OnExit doesnt work
    private void Update()
    {
        BlockedShooting = myCollider.IsTouchingLayers(whatIsPlatform);
    }
}
