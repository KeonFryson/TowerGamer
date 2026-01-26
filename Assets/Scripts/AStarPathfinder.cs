using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinder : MonoBehaviour
{
    public static AStarPathfinder Instance { get; private set; }

    [Header("Grid Settings")]
    [SerializeField] private Vector2 gridWorldSize = new Vector2(50f, 50f);
    [SerializeField] private float nodeRadius = 0.2f;
    [SerializeField] private LayerMask unwalkableMask;

    [Header("Debug")]
    [SerializeField] private bool displayGridGizmos = false;
    [SerializeField][Range(0f, 1f)] private float gridAlpha = 0.3f;

    private PathNode[,] grid;
    private float nodeDiameter;
    private int gridSizeX, gridSizeY;
    private float gridUpdateTimer = 0f;
    private const float GRID_UPDATE_INTERVAL = 5f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }


        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }

    private void Update()
    {
        gridUpdateTimer += Time.deltaTime;

        if (gridUpdateTimer >= GRID_UPDATE_INTERVAL)
        {
            CreateGrid();
            gridUpdateTimer = 0f;
        }
    }

    private void CreateGrid()
    {
        grid = new PathNode[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.up * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);
                bool walkable = !Physics2D.OverlapCircle(worldPoint, nodeRadius * 0.9f, unwalkableMask);
                grid[x, y] = new PathNode(walkable, worldPoint, x, y);
            }
        }
    }

    public List<Vector2> FindPath(Vector2 startPos, Vector2 targetPos)
    {
        PathNode startNode = NodeFromWorldPoint(startPos);
        PathNode targetNode = NodeFromWorldPoint(targetPos);

        if (startNode == null || targetNode == null || !targetNode.walkable)
        {
            return null;
        }

        List<PathNode> openSet = new List<PathNode>();
        HashSet<PathNode> closedSet = new HashSet<PathNode>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            PathNode currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            foreach (PathNode neighbor in GetNeighbors(currentNode))
            {
                if (!neighbor.walkable || closedSet.Contains(neighbor))
                {
                    continue;
                }

                int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        return null;
    }

    private List<Vector2> RetracePath(PathNode startNode, PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>();
        PathNode currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        List<Vector2> waypoints = new List<Vector2>();
        for (int i = path.Count - 1; i >= 0; i--)
        {
            waypoints.Add(path[i].worldPosition);
        }

        return SimplifyPath(waypoints);
    }

    private List<Vector2> SimplifyPath(List<Vector2> path)
    {
        if (path.Count <= 2)
            return path;

        List<Vector2> simplifiedPath = new List<Vector2>();
        simplifiedPath.Add(path[0]);

        Vector2 oldDirection = Vector2.zero;

        for (int i = 1; i < path.Count; i++)
        {
            Vector2 newDirection = (path[i] - path[i - 1]).normalized;
            if (newDirection != oldDirection)
            {
                simplifiedPath.Add(path[i - 1]);
            }
            oldDirection = newDirection;
        }

        simplifiedPath.Add(path[path.Count - 1]);
        return simplifiedPath;
    }

    private int GetDistance(PathNode nodeA, PathNode nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    private List<PathNode> GetNeighbors(PathNode node)
    {
        List<PathNode> neighbors = new List<PathNode>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbors.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbors;
    }

    private PathNode NodeFromWorldPoint(Vector2 worldPosition)
    {
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.up * gridWorldSize.y / 2;
        Vector2 relativePos = worldPosition - (Vector2)worldBottomLeft;

        float percentX = relativePos.x / gridWorldSize.x;
        float percentY = relativePos.y / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        if (x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY)
        {
            return grid[x, y];
        }

        return null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, 1));

        if (grid != null && displayGridGizmos)
        {
            foreach (PathNode node in grid)
            {
                Gizmos.color = (node.walkable) ? new Color(1f, 1f, 1f, gridAlpha) : Color.red;
                Gizmos.DrawCube(node.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
            }
        }
    }
}

public class PathNode
{
    public bool walkable;
    public Vector2 worldPosition;
    public int gridX;
    public int gridY;

    public int gCost;
    public int hCost;
    public PathNode parent;

    public PathNode(bool walkable, Vector2 worldPosition, int gridX, int gridY)
    {
        this.walkable = walkable;
        this.worldPosition = worldPosition;
        this.gridX = gridX;
        this.gridY = gridY;
    }

    public int fCost
    {
        get { return gCost + hCost; }
    }
}