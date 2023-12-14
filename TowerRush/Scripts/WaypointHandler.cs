using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointHandler : MonoBehaviour
{
    public static WaypointHandler Instance;
    public List<Transform> wayPoints = new List<Transform>();

    void Awake() 
    {
        Instance = this;
    }
    public Transform returnPosition()
    {
        int randPos = Random.Range(0, wayPoints.Count-1);

        return wayPoints[randPos];
    }
}
