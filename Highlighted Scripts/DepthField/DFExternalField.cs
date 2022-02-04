using UnityEngine;

public class DFExternalField : MonoBehaviour
{
    DepthField depthField;
    CircleCollider2D internalField;

    float internalFieldRadius;

    // Miedzy polem zewnetrznym a wewnetrznym
    float distanceToInternalField;

    private void Start()
    {
        depthField = GetComponentInParent<DepthField>();
        internalField = depthField.gameObject.GetComponentInChildren<DFInternalField>()
        .GetComponent<CircleCollider2D>();

        internalFieldRadius = internalField.radius;

        float myRadius = GetComponent<CircleCollider2D>().radius;
        distanceToInternalField = myRadius - internalFieldRadius;
    }

    private void Update()
    {
        // Wywolujac UpdateZPosition moge usunac obiekt a w forechu jest to niedozwolone
        for (int i = 0; i < depthField.ObjectsInMyField.Count; i++)
        {
            var obj = depthField.ObjectsInMyField[i];

            if (!obj.InInternalField)
            {
                // Obiekt w extermal fieldzie moze zostac zniszczony
                if (obj.Object == null)
                {
                    depthField.RemoveObject(obj.Object);
                    return;
                }

                // Na jego podstawie okreslam priorytetowosc danego DepthFiedla
                float percentObjectDistance;

                float posZ = CountZPosition(obj.Object, out percentObjectDistance);

                obj.UpdateZPosition(depthField, posZ, percentObjectDistance);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (depthField.CheckIfCompatible(collision.gameObject.layer))
            depthField.AddObject(collision.gameObject);
    }

    // !!! Gdy obiekt w polu ginie ten exit musi wykonywac sie przed exitem w internal field
    // Zapewnia no kolejnosc w inspektorze !!!
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (depthField.CheckIfCompatible(collision.gameObject.layer))
            depthField.RemoveObject(collision.gameObject);
    }

    float CountZPosition(GameObject obj, out float percentObjectDistance)
    {
        float objectDistanceFromCenter = Vector2.Distance(transform.position, obj.transform.position);

        // Zalezy mi na odleglosci objektu od zewnetrznej czesci wewnetrznego pola
        float objectDistanceToInternalField = objectDistanceFromCenter - internalFieldRadius;

        // Obliczam procent tej odleglosci
        percentObjectDistance = objectDistanceToInternalField / distanceToInternalField;

        if (percentObjectDistance < 1f)
        {
            // Gdy 90% to odwracam. Wtedy mowi mi ze jestem w 10-atym% drogi 
            // do punktu stycznosci z internalFiled
            percentObjectDistance = (1f - percentObjectDistance);

            // Jest to wartosc mojego wychylenia w pozycji "Z" od punktu odniesienia
            float distanceToWaypoint = Mathf.Abs(depthField.WaypointOfPositionZ - transform.position.z);

            // Teraz przemnazam procent mojej drogi przez wartosc wychylenia i wynik dodaje do waypointa
            // Gdy juz jestem powyzej 40% drogi zwiekszam ta wartosc dla szybszej zmiany pozycji Z 
            // Czyli jestem w 40 % drogi ale pozycje "Z" okresl mi jak dla 55% drogi
            float ZValueToAdd = distanceToWaypoint * (percentObjectDistance > .4f ?
                 (percentObjectDistance + .15f) : percentObjectDistance);

            if (depthField.WaypointOfPositionZ < transform.position.z)
                return depthField.WaypointOfPositionZ + ZValueToAdd;
            else
                return depthField.WaypointOfPositionZ - ZValueToAdd;
        }
        // Czyli odlegolosc playera jest dalsza od odleglosci miedzy polami internal i external
        else
        {
            //Nie przejmie priorytetowosci( kontroli nad obiektem) bo jestem za daleko
            percentObjectDistance = 0f;
            return depthField.WaypointOfPositionZ;
        }
    }
}
