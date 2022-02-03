using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-200)]
public class ObjectsPooler : MonoBehaviour
{
    // It is intended to generate objects for later use.
    // Thanks to the tag I can easily navigate throught the pools
    [System.Serializable]
    public class Pool
    {
        public string Tag => prefab.name;

        public GameObject prefab;
        public int size;
    }

    // It is intended to generate only one instance of object ( particle system ) for later use.
    // Thanks to the tag I can easily navigate throught the pools
    [System.Serializable]
    public class OneInstancePool
    {
        public string Tag => prefab.name;

        [Tooltip("It is intended for particle system !")]
        public GameObject prefab;
    }

    static ObjectsPooler instance;

    [SerializeField] Pool[] pools;
    [SerializeField] OneInstancePool[] oneInstancePools;

    Dictionary<string, Queue<GameObject>> poolsDictionary = new Dictionary<string, Queue<GameObject>>();

    Dictionary<string, ParticleSystem> oneInstancePoolsDictionary =
        new Dictionary<string, ParticleSystem>();

    // Start is called before the first frame update
    void Awake()
    {
        if (instance)
        {
            Debug.LogError("You are trying to create another: " + name);
            Destroy(gameObject);
            return;
        }
        else
            instance = this;

        SetPools();

        SetParticlesPools();
    }

    void SetPools()
    {
        foreach (var pool in pools)
        {
            var queue = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                var obj = Instantiate(pool.prefab, transform);

                obj.SetActive(false);

                // !!! Vert important because provided proper return to me. It protect from
                // unexpected destroy my object which is in another parent. Return will be execute when
                // object change to disabled
                obj.AddComponent<BackToMyObjectsPooler>().MyObjectsPooler = transform;

                // Add at the end of queue
                queue.Enqueue(obj);
            }

            // Adding a prepared pool
            poolsDictionary.Add(pool.Tag, queue);
        }
    }

    void SetParticlesPools()
    {
        foreach (var particle in oneInstancePools)
        {
            var particleSystem = Instantiate(particle.prefab, transform).GetComponent<ParticleSystem>();

            if (particleSystem == null)
            {
                Debug.LogError("OneInstancePool must contain the only particle system object !!" +
                    " Change prefab");
                return;
            }

            // !!! Vert important because provided proper return to me. It protect from
            // unexpected destroy my object which is in another parent. Return will be execute when
            // object change to disabled
            particleSystem.gameObject.AddComponent<BackToMyObjectsPooler>().MyObjectsPooler = transform;

            particleSystem.gameObject.SetActive(false);

            oneInstancePoolsDictionary.Add(particle.Tag, particleSystem);
        }
    }

    public static void BringBackAllMyObjects()
    {
        // I can destroy my objects unexpectedly ( e.g. during zone reset)
        // , so I have to prevent it by bringing all my objects to me

        foreach (var pool in instance.pools)
        {
            var poolObjects = instance.poolsDictionary[pool.Tag].ToArray();

            foreach (var obj in poolObjects)
                obj.transform.SetParent(instance.transform);
        }

        foreach (var pool in instance.oneInstancePools)
        {
            var obj = instance.oneInstancePoolsDictionary[pool.Tag];

            obj.transform.SetParent(instance.transform);
        }
    }

    public static GameObject SpawnObjectfFromThePool(string tag, Vector3 position, Quaternion rotation
        , Transform parent)
    {
        if (!instance.poolsDictionary.ContainsKey(tag))
        {
            Debug.LogError("Pool with tag " + tag + " doesnt excist");
            return null;
        }

        // Get the firt object from selected pool
        var obj = instance.poolsDictionary[tag].Dequeue();

        if (obj.activeSelf)
            Debug.LogWarning("All my objects are active");

        // Setting an object
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.transform.SetParent(parent);

        // I cant use SetActive and SetParent at the same frame
        // so I call it in the next frame using coroutine
        instance.StartCoroutine(ActivateObjectAtNextFrameCoroutine(obj));

        // Move the object to the end of queue
        instance.poolsDictionary[tag].Enqueue(obj);

        return obj;
    }

    public static ParticleSystem PlayParticleSystem(string tag, Vector3 position, Quaternion rotation
        , Transform parent)
    {
        if (!instance.oneInstancePoolsDictionary.ContainsKey(tag))
        {
            Debug.LogError("Pool with tag " + tag + " doesnt excist");
            return null;
        }

        // Get my particle system in selected pool
        var particleSystem = instance.oneInstancePoolsDictionary[tag];

        // Setting particleSystem
        particleSystem.transform.SetParent(parent);
        particleSystem.transform.position = position;
        particleSystem.transform.rotation = rotation;

        // I cant use SetActive and SetParent at the same frame
        // so I call it in the next frame using coroutine
        instance.StartCoroutine(ActivateObjectAtNextFrameCoroutine(particleSystem.gameObject));

        particleSystem.Play();

        return particleSystem;
    }

    static IEnumerator ActivateObjectAtNextFrameCoroutine(GameObject obj)
    {
        yield return null;

        obj.SetActive(true);
    }
}
