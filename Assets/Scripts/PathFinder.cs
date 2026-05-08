using System.Collections.Generic;
using UnityEngine;

public class PathFinder
{
    private Dictionary<Vector2Int, Node> _nodeCache = new Dictionary<Vector2Int, Node>();
    private Vector2Int _startPosition;

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int target)
    {
        _startPosition = start;
        _nodeCache.Clear();
        GridManager gridManager = GridManager.Instance;

        if (!IsWalkable(target, gridManager))
            return null;

        Node startNode = GetOrCreateNode(start, gridManager);
        Node targetNode = GetOrCreateNode(target, gridManager);

        List<Node> openSet = new List<Node> { startNode };
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();

        while (openSet.Count > 0)
        {
            Node currentNode = GetLowestFCostNode(openSet);

            if (currentNode.GridPosition == targetNode.GridPosition)
                return RetracePath(startNode, currentNode);

            openSet.Remove(currentNode);
            closedSet.Add(currentNode.GridPosition);

            foreach (Node neighbor in GetNeighbors(currentNode, gridManager))
            {
                if (closedSet.Contains(neighbor.GridPosition))
                    continue;

                int newCost = currentNode.GCost + GetDistance(currentNode, neighbor);

                if (newCost < neighbor.GCost || !openSet.Contains(neighbor))
                {
                    neighbor.GCost = newCost;
                    neighbor.HCost = GetDistance(neighbor, targetNode);
                    neighbor.Parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return null;
    }

    private Node GetOrCreateNode(Vector2Int position, GridManager gridManager)
    {
        if (!_nodeCache.TryGetValue(position, out Node node))
        {
            bool walkable = IsWalkable(position, gridManager);
            node = new Node(position, walkable);
            _nodeCache[position] = node;
        }
        return node;
    }

    private List<Vector2Int> RetracePath(Node startNode, Node endNode)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode.GridPosition);
            currentNode = currentNode.Parent ?? startNode;
        }

        path.Reverse();
        return path;
    }

    private Node GetLowestFCostNode(List<Node> nodes)
    {
        Node lowestNode = nodes[0];
        for (int i = 1; i < nodes.Count; i++)
        {
            if (nodes[i].FCost < lowestNode.FCost || 
               (nodes[i].FCost == lowestNode.FCost && nodes[i].HCost < lowestNode.HCost))
            {
                lowestNode = nodes[i];
            }
        }
        return lowestNode;
    }

    private int GetDistance(Node a, Node b)
    {
        int dstX = Mathf.Abs(a.GridPosition.x - b.GridPosition.x);
        int dstY = Mathf.Abs(a.GridPosition.y - b.GridPosition.y);

        return 14 * Mathf.Min(dstX, dstY) + 10 * Mathf.Abs(dstX - dstY);
    }

    private List<Node> GetNeighbors(Node node, GridManager gridManager)
    {
        List<Node> neighbors = new List<Node>();
        Vector2Int pos = node.GridPosition;


        Vector2Int[] directions = {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left,
        new Vector2Int(1, 1),
        new Vector2Int(-1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, -1),
    };

        foreach (var direction in directions)
        {
            Vector2Int neighborPos = pos + direction;

            if (!gridManager.IsCellExists(neighborPos)) continue;

            Node neighbor = GetOrCreateNode(neighborPos, gridManager);
            if (!neighbor.IsWalkable) continue;

            bool isDiagonal = Mathf.Abs(direction.x) == 1 && Mathf.Abs(direction.y) == 1;

            if (isDiagonal)
            {

                Vector2Int cell1 = pos + new Vector2Int(direction.x, 0);
                Vector2Int cell2 = pos + new Vector2Int(0, direction.y);

                Cell c1 = gridManager.GetCell(cell1);
                Cell c2 = gridManager.GetCell(cell2);

                if ((c1 == null || c1.IsBlocked) || (c2 == null || c2.IsBlocked))
                {
                    continue;
                }
            }

            neighbors.Add(neighbor);
        }

        return neighbors;
    }

private bool IsWalkable(Vector2Int gridPosition, GridManager gridManager)
{
    Cell cell = gridManager.GetCell(gridPosition);
    if (cell == null)
    {
        Debug.Log($"Cell at {gridPosition} is null");
        return false;
    }

    if (gridPosition == _startPosition)
    {
        return true;
    }

    bool walkable = !cell.IsBlocked;
    return walkable;
}
}