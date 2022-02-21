using UnityEngine;

public static class UsefulFunctions
{
    // Allows to see how the algorithms works. It will be set to false at the end of the algorithm
    public static bool TestMode { get; set; } = false;

    #region Tangent Finding Algorithm

    public static bool FindTangentFor(Collision2D collision, Vector3 objectPosition
        , out Vector3 tangentPoint, out Quaternion tangentDirection, int layer)
    {
        if (TestMode) Debug.Log("<color=blue>----Start counting the tanget-----</color>");

        tangentDirection = Quaternion.identity;

        // --- I need to find a tangent point. This is the core of the whole algorithm --- //    
        tangentPoint = CalculateTangentPoint(collision, ref objectPosition, ref layer);

        // Draw the collision direction
        if (TestMode) Debug.DrawRay(objectPosition, tangentPoint - objectPosition, Color.yellow);

        // To set translatedTangentPoint I need to move the collision point by a small amount
        Vector2 translation = (objectPosition - tangentPoint).normalized * 0.035f;

        // It is very important to count the tangent because I will need to use the RayCast method
        Vector2 translatedTangentPoint = tangentPoint + (Vector3)translation;

        // Draw the translation
        if (TestMode) Debug.DrawRay(tangentPoint, translation, Color.blue);

        // --- Calculate the angle of tangent --- //
        float angle;

        if (!CalculateTangentAngle(out angle, ref translatedTangentPoint, ref layer))
        {
            if (TestMode) Debug.LogWarning("I cant count the tangent for " + collision.gameObject.name
                  + " with the angle: " + collision.transform.eulerAngles.z);
            return false;
        }

        // --- Check if should I reflect the tangent --- //
        bool reflect = ShouldIReflectInAxes(ref angle, ref translatedTangentPoint, ref translation);

        if (reflect)
            if (TestMode) Debug.Log("I have made the reflection in axes");

        // Assign the tangent data
        tangentDirection = Quaternion.Euler(reflect ? new Vector3(180, 180, angle) : new Vector3(0, 0, angle));

        // I cant use objectPosition.z because the bullet in the depth field
        // have not yet reached the z position with the colliding object
        tangentPoint = new Vector3(translatedTangentPoint.x, translatedTangentPoint.y
            , collision.gameObject.transform.position.z);

        if (TestMode) Debug.Log("<color=green>----Counting successful-----</color>");

        if (TestMode) Debug.Break();

        TestMode = false;

        return true;
    }

    static bool ShouldIReflectInAxes(ref float angle, ref Vector2 translatedTangentPoint
        , ref Vector2 translation)
    {
        // The determination that I should reflect is based on the direction from which
        // the collision occurred. This direction is define by the translation 

        Vector2 tangentPoint = translatedTangentPoint - translation;

        // In the case of these angles I can define it using a simple comparsion
        if (angle == 0f)
            return translatedTangentPoint.y > tangentPoint.y;

        if (angle == 90f)
            return translatedTangentPoint.x < tangentPoint.x;

        // To define the collision direction for the rest of angles I need to create a linear function
        // y = ax + b for translated tangent. I create it using the translation which is my point
        // for the function and A coefficient using a = tan(angle) pattern. 

        // I create this funcion because of B coefficient which define my direction. I mean 
        // whether my created linear function is above or below the tangent

        float A_Coefficient = Mathf.Tan(angle * Mathf.Deg2Rad);

        float B_Coefficient = translation.y - A_Coefficient * translation.x;

        // Reflect only if B coefficient is greather than 0. It means that the linear function of
        // the translated tangent is over the linear function of the tangent
        return B_Coefficient > 0f;
    }

    static bool CalculateTangentAngle(out float angle, ref Vector2 translatedTangentPoint, ref int layer)
    {
        // I start the algorithm with an angle is equal 90. I will decrease this angle by 1
        // until I find my tangent or the angle will reach -90. Decreasing by one can be problematic for 
        // counting of the tangent for colliders that have rotation e.g 33,3 but I cant use the value of .1 
        // because of the optimalization issues
        angle = 90f;

        // This is half the lenght of the tangent
        const float radius = 0.3f;

        // Smaller allows you to count with the greater precision
        const float angleReducingValue = .2f;

        // These two points will define my potential tangent
        Vector2 firstPointOfTangent, secondPointOfTangent;

        // --- The algorithm of finding the angle of tangent in regard to the collider --- //
        do
        {
            // Define first point of the potential tangent
            firstPointOfTangent = translatedTangentPoint;

            firstPointOfTangent.x += Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
            firstPointOfTangent.y += Mathf.Sin(angle * Mathf.Deg2Rad) * radius;

            Vector2 halfLenghtOfTangent = firstPointOfTangent - translatedTangentPoint;

            // Define the second point od the potential tangent using the direction
            secondPointOfTangent = translatedTangentPoint - halfLenghtOfTangent;

            // Check that it is a tangent
            RaycastHit2D tangentHit = Physics2D.Raycast(firstPointOfTangent
                , secondPointOfTangent - firstPointOfTangent
                , Vector2.Distance(secondPointOfTangent, firstPointOfTangent)
                , 1 << layer);

            // If this is null it means that this potential tangent doesnt collide with anything and
            // this is my tangent that I am looking for
            if (!tangentHit)
            {
                if (TestMode) Debug.Log("I have found for the angle: " + angle);

                // Draw my tangent
                if (TestMode) Debug.DrawRay(firstPointOfTangent, secondPointOfTangent - firstPointOfTangent, Color.red);

                return true;

            }
            else
                // Still look for the tangent for antoher angle when the angle is less than -90
                angle -= angleReducingValue;

        } while (angle >= -89f);

        // This means that the angle is equal -90 and the algorithm didnt find the tangent
        return false;
    }

    static Vector2 CalculateTangentPoint(Collision2D collision, ref Vector3 objectPosition, ref int layer)
    {
        // It is inaccurate because it may be be inside or outsie of the collider
        Vector2 collsionPoint = collision.GetContact(0).point;

        Vector2 collisionDirection = (collsionPoint - (Vector2)objectPosition).normalized;

        // Draw the inaccurate collison point
        if (TestMode) Debug.DrawRay(collsionPoint, (Vector2)objectPosition - collsionPoint, Color.green);

        RaycastHit2D tangentPointHit = Physics2D.Raycast(objectPosition, collisionDirection
                 , Vector2.Distance(collsionPoint, objectPosition), 1 << layer);

        // If the Raycast didnt find the tangent point, it means that collision point
        // is OUTSIDE od the collider but very close to it
        // This is a feature of the continous mode in a rigid body.
        // Then I need to use the ClosestPoint method to return the closest point to the
        // collider in regard to the collision point.

        // Assignt the tangent point to the collision point
        collsionPoint = tangentPointHit ? tangentPointHit.point
            : collision.collider.ClosestPoint(collsionPoint);

        return collsionPoint;
    }

    #endregion

    public static float GetLengthOfClip(Animator anim, string clipName)
    {
        //anim.GetCurrentAnimatorStateInfo(0).length

        AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;

        var clip = System.Array.Find(clips, obj => obj.name == clipName);

        if (clip)
        {
            //Debug.Log("Lenght of clip: " + clip.length);
            return clip.length;
        }
        else
        {
            Debug.LogError("I dont have a clip named: " + clipName);
            return 0f;
        }
    }

    /// <summary>
    /// Included children
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="value"></param>
    public static void EnableCollidersInObject(GameObject obj, bool value)
    {
        foreach (var collider in obj.GetComponentsInChildren<Collider2D>(true))
            collider.enabled = value;
    }
}
