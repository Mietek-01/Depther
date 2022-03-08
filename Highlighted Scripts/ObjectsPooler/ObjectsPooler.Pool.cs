using UnityEngine;

public partial class ObjectsPooler
{
    // It is intended to generate objects for later use.
    // Thanks to the tag I can easily navigate throught the pools
    [System.Serializable]
    public class Pool
    {
        public GameObject prefab;
        public int size;

        public string Tag => prefab.name;
    }

}
