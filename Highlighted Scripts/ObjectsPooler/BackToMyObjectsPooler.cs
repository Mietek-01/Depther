using UnityEngine;

public class BackToMyObjectsPooler : MonoBehaviour
{
    public Transform MyObjectsPooler { get; set; }

    const string RETURN_TO_POOL = "ReturnToPool";

    private void OnDisable()
    {
        // I cant change parent here
        Invoke(RETURN_TO_POOL, .05f);
    }

    void ReturnToPool()
    {
        transform.SetParent(MyObjectsPooler);
    }
}
