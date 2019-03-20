using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// For villages & guarding them
public class PatrolRoute : MonoBehaviour
{
    public List<GameObject> RoutePoints = new List<GameObject>();

    public int GetClosestPointTo(Vector3 pos)
    {
        int result = 0;
        float closestDistSqr = 999999.0f;

        for (int i = 0; i < RoutePoints.Count; ++i)
        {
            float distSqr = (RoutePoints[i].transform.position - pos).sqrMagnitude;
            if (distSqr < closestDistSqr)
            {
                result = i;
                closestDistSqr = distSqr;
            }
        }

        return result;
    }
}
