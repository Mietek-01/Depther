using System.Collections.Generic;
using UnityEngine;

public partial class DepthField : MonoBehaviour
{
    [Tooltip("On its basis, the entire Z position changing algorithm will work")]
    [SerializeField] Transform waypoint;

    [Tooltip("Defines which objects can be affected by the depth field")]
    [SerializeField] LayerMask[] forWhichObjects;

    // Dzieki temu kontenerowi obietk ktory wejdzie w depth field bedzie tylko raz zarejestrowany
    public static List<DepthFieldObject> RegisteredDepthFieldObjects { get; set; } = new List<DepthFieldObject>();

    // Kazde pole ma swoj kontener na obietky ktore w tym polu sie znajduja
    public List<DepthFieldObject> ObjectsInMyField { get; private set; } = new List<DepthFieldObject>();

    // Na jego podstawie jestem w stanie okreslic odpowiedni ruch w external field
    public float WaypointOfPositionZ { get; private set; }

    private void Awake()
    {
        if (waypoint == null)
        {
            WaypointOfPositionZ = 0f;
            Debug.LogWarning("---My waypoint is null");
        }
        else
            WaypointOfPositionZ = waypoint.position.z;

        if (WaypointOfPositionZ == transform.position.z)
            Debug.LogError("The depth field must have a different position Z than the waypoint");
    }

    /// <summary>
    /// Zapisuje obiekt ktory wszedl w moje pole
    /// </summary>
    /// <param name="obj"></param>
    public void AddObject(GameObject obj)
    {
        // Caly sens algorytmu polega na tym by nie tworzyc dwoch tych samych depthFieldObject. 
        // By temu zapobiec sprawdzam czy obiekt jest juz zarejestrowny.
        // Gdy jest zarejestrownu to zwiekszam informacje o ilosci pol w jakich ten obiekt sie znajduje

        var result = RegisteredDepthFieldObjects.Find(depthFieldObject => depthFieldObject.Object == obj);

        if (result == null)
        {
            //Debug.Log($"The {obj} enters to the first depth field");
            var depthFieldObject = new DepthFieldObject(obj);

            RegisteredDepthFieldObjects.Add(depthFieldObject);
            ObjectsInMyField.Add(depthFieldObject);
        }
        else
        {
            //Debug.Log($"The {obj} enters to the next depth field");
            result.NumberOfMyFields++;
            ObjectsInMyField.Add(result);
        }
    }

    /// <summary>
    /// Usuwam obiekt z mojego pola
    /// </summary>
    /// <param name="obj"></param>
    public void RemoveObject(GameObject obj)
    {
        var objectInDepthField = ObjectsInMyField.Find(objInDepthField => objInDepthField.Object == obj);

        if (objectInDepthField == null)
            return;

        objectInDepthField.NumberOfMyFields--;
        ObjectsInMyField.Remove(objectInDepthField);

        if (objectInDepthField.NumberOfMyFields == 0)
        {
            //Debug.Log($"The {obj} exits from the last depth field");

            if (obj != null)
                objectInDepthField.SetZPosition(this, WaypointOfPositionZ);

            RegisteredDepthFieldObjects.Remove(objectInDepthField);
        }
        //else Debug.Log($"The {obj} exits from the the depth field");

    }

    public bool CheckIfCompatible(int sentLayer)
    {
        bool compatible = false;

        foreach (var mask in forWhichObjects)
            if (1 << sentLayer == mask)
            {
                compatible = true;
                break;
            }
        return compatible;
    }

    // Musze o tym pamietac przy resecie rozgrywki
    public static void ClearRegisteredDepthFieldObjects()
    {
        RegisteredDepthFieldObjects.Clear();
    }

}