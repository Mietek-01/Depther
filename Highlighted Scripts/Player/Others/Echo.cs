using UnityEngine;

public class Echo : MonoBehaviour
{
    Animator anim;
    SpriteRenderer myRenderer;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        myRenderer = GetComponent<SpriteRenderer>();
        anim.enabled = false;
    }

    public void Disable()
    {
        gameObject.SetActive(false);
        
        Color color = myRenderer.color;
        color.a = 1f;

        myRenderer.color = color;

        anim.enabled = false; 
    }

    public void SetWhenDisappear(float when,int whichEcho)
    {
        myRenderer.sortingOrder += whichEcho;

        Invoke("EnableAnimator", when);
    }

    void EnableAnimator()
    {
        anim.enabled = true;
    }
}
