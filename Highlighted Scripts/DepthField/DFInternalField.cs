using UnityEngine;

public class DFInternalField : MonoBehaviour
{
    DepthField depthField;

    private void Start()
    {
        depthField = GetComponentInParent<DepthField>();
    }

    private void Update()
    {
        // Dzieki temu gdy DF zmieni poz zmieni tez poz obiektu
        foreach (var obj in depthField.ObjectsInMyField)
            if (obj.InInternalField)
                obj.SetZPosition(depthField, transform.position.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (depthField.CheckIfCompatible(collision.gameObject.layer))
            ObjectInInternalField(true, collision.gameObject);
    }

    // !! Gdy obiekt bedacy w colliderze zginie wywola sie OnTriggerExit2D !!
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (depthField.CheckIfCompatible(collision.gameObject.layer))
            ObjectInInternalField(false, collision.gameObject);
    }

    // Zanim wejde do Internal Field obiekt powinien przed tym wejsc w External Field
    void ObjectInInternalField(bool value, GameObject obj)
    {
        var result = depthField.ObjectsInMyField
            .Find(depthFieldObject => depthFieldObject.Object == obj);

        // Czyli albo obiekt zostal juz usuniety w OnTriggerExit2D w externalField
        // albo obiekt mial tak duza predkosc ze odrazu zlapal kolizje z InternalField
        if (result != null)
            // Bardzo wazne bo okresla czy zewnetrzen pole moze oddzialywac na obiekt
            result.InInternalField = value;
    }
}
