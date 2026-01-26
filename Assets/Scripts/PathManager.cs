using System.Collections.Generic;
using UnityEngine;

public class PathManager : MonoBehaviour
{
    public static PathManager Instance { get; private set; }

    [Header("Path Waypoints")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private Color pathColor = Color.cyan;
    [SerializeField] private bool showPath = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Vector3 GetWaypoint(int index)
    {
        if (index >= 0 && index < waypoints.Length)
        {
            return waypoints[index].position;
        }
        return Vector3.zero;
    }

    public int GetWaypointCount()
    {
        return waypoints.Length;
    }

    private void OnDrawGizmos()
    {
        if (!showPath || waypoints == null || waypoints.Length < 2)
            return;

        Gizmos.color = pathColor;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                Gizmos.DrawWireSphere(waypoints[i].position, 0.3f);
            }
        }

        if (waypoints[waypoints.Length - 1] != null)
        {
            Gizmos.DrawWireSphere(waypoints[waypoints.Length - 1].position, 0.3f);
        }
    }
}