using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New WayPoints", menuName = "Planet Force/Way Points Data")]
public class WayPointsData : ScriptableObject
{
    public Vector2[] wayPoints; // son puntos en coordenadas de mundo. Esto podría ser perfectamente un Scriptable Object
    public float distanceToTarget = 1f;
    public float minWaitInTarget = 0f;
    public float maxWaitInTarget = 2f;

    public Vector2 GetRandomPosition()
    {
        int idxWayPoint = Random.Range(0, wayPoints.Length);
        return wayPoints[idxWayPoint];
    }
}
