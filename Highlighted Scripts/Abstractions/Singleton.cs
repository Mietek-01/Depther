using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T instance;

    protected virtual void Awake()
    {
        if (instance == null)
            instance = (T)(object)this;
        else
        {
            Debug.LogWarning("You are trying to create another instance of: " + instance.GetType());
            Destroy(gameObject);
        }
    }
}