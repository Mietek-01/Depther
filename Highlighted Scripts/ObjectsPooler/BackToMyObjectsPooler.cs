using UnityEngine;

public class BackToMyObjectsPooler : MonoBehaviour
{
    public Transform MyObjectsPooler { get; set; }

    private void OnDisable()
    {
        // I cant change parent here
        Invoke("ReturnToPool", .05f);
    }

    void ReturnToPool()
    {
        transform.SetParent(MyObjectsPooler);
    }
}
