using UnityEngine;

public class PathManager : MonoBehaviour
{
    public static PathManager Instance { get; private set; }

    [Header("Path Data")]
    [SerializeField] private PathMapData pathMapData;
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
        if (pathMapData != null && index >= 0 && index < pathMapData.nodes.Length)
        {
            return pathMapData.nodes[index].position;
        }
        return Vector3.zero;
    }

    public int GetWaypointCount()
    {
        return pathMapData != null ? pathMapData.nodes.Length : 0;
    }

    public PathNodeData GetNode(int index)
    {
        if (pathMapData != null && index >= 0 && index < pathMapData.nodes.Length)
        {
            return pathMapData.nodes[index];
        }
        return null;
    }

    private void OnDrawGizmos()
    {
        if (!showPath || pathMapData == null || pathMapData.nodes == null)
            return;

        Gizmos.color = pathColor;
        for (int i = 0; i < pathMapData.nodes.Length; i++)
        {
            var node = pathMapData.nodes[i];
            Gizmos.DrawWireSphere(node.position, 0.3f);

            if (node.nextNodeIndices != null)
            {
                foreach (var nextIndex in node.nextNodeIndices)
                {
                    if (nextIndex >= 0 && nextIndex < pathMapData.nodes.Length)
                    {
                        Vector3 from = node.position;
                        Vector3 to = pathMapData.nodes[nextIndex].position;
                        Gizmos.DrawLine(from, to);

                        // Draw arrowhead
                        DrawArrowHead(from, to, 0.3f, 20f);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Draws an arrowhead at the end of a line from 'from' to 'to'.
    /// </summary>
    private void DrawArrowHead(Vector3 from, Vector3 to, float arrowHeadLength, float arrowHeadAngle)
    {
        Vector3 direction = (to - from).normalized;
        Vector3 right = Quaternion.LookRotation(Vector3.forward) * Quaternion.Euler(0, 0, arrowHeadAngle) * -direction;
        Vector3 left = Quaternion.LookRotation(Vector3.forward) * Quaternion.Euler(0, 0, -arrowHeadAngle) * -direction;
        Gizmos.DrawLine(to, to + right * arrowHeadLength);
        Gizmos.DrawLine(to, to + left * arrowHeadLength);
    }
}