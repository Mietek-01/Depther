using UnityEngine;

public class DismemberedPlayer : MonoBehaviour
{
    [SerializeField] Material disolveMaterial;
    [SerializeField] float whenStartDissolve = 3f;

    public float WhenStartDissolve => whenStartDissolve;

    SpriteRenderer [] bodyElements;
    Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        bodyElements = GetComponentsInChildren<SpriteRenderer>();
        anim = GetComponent<Animator>();

        anim.enabled = false;
        Invoke("Disolve", whenStartDissolve);
    }

    void Destroy()
    {
        Destroy(gameObject);
    }

    void Disolve()
    {
        // Change material to dissolve which will be use by animator
        for (int i = 0; i < bodyElements.Length; i++)
            bodyElements[i].material = disolveMaterial;

        anim.enabled = true;
    }
}
