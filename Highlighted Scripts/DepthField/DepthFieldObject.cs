using UnityEngine;

public partial class DepthField
{
    public class DepthFieldObject
    {
        public bool InInternalField { get; set; } = false;

        // Pozwala okreslic czy dany obiekt wyszedl juz ze wszystkich pol, dzieki czemu
        // bede mogl go usunac z DepthField.RegisteredDepthFieldObjects
        public int NumberOfMyFields { get; set; }

        // Obiekt ktory wchodzi w DF
        public GameObject Object { get; }

        // Pole ktore aktualnie na mnie oddzialowuje, tzn okresla moja pozycje "Z"
        DepthField myCurrentDepthField;

        // Jest to najwazniejsza zmienna. Dzieki niej okreslam ktory DF staje sie myCurrentDepthField
        float percentDistanceFromMyCurrentDF = 0f;

        public DepthFieldObject(GameObject obj)
        {
            NumberOfMyFields = 1;
            Object = obj;
        }

        public void SetZPosition(DepthField sentDF, float newZposition)
        {
            // Zmienic moze tylko moj myCurrentDepthField
            if (sentDF == myCurrentDepthField)
            {
                // Obiekt zostal zniszczony
                if (!Object)
                {
                    sentDF.RemoveObject(Object);
                    return;
                }

                Vector3 newPosition = Object.transform.position;
                newPosition.z = newZposition;

                Object.transform.position = newPosition;
            }
        }

        public void UpdateZPosition(DepthField sentDF, float newZposition, float percentDistanceFromSentDF)
        {
            // Gdy updatuje inny depthField i ma on wieksza priorytetowosc 
            // tzn obiekt jest blizej niego to one staje sie myCurrentDepthField

            if (sentDF != myCurrentDepthField && percentDistanceFromSentDF > percentDistanceFromMyCurrentDF)
                myCurrentDepthField = sentDF;

            if (sentDF == myCurrentDepthField)
            {
                percentDistanceFromMyCurrentDF = percentDistanceFromSentDF;

                Vector3 newPosition = Object.transform.position;
                newPosition.z = newZposition;

                Object.transform.position = newPosition;
            }
        }

    }
}

