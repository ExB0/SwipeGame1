using System.Collections.Generic;

using Grid;
using PlatformPuzzle.Managers;
using UnityEngine;

namespace PlatformPuzzle.Pathfinder
{
    public class PathFinder
    {
        private const int StraightCost = 10;
        private const int DiagonalCost = 14;

        private readonly Dictionary<Vector2Int, Node> _nodeCache = new();
        private Vector2Int _startPosition;

        public List<Vector2Int> FindPath(
            Vector2Int start,
            Vector2Int target,
            GridManager gridManager)
        {
            if (gridManager == null)
            {
                return null;
            }

            _startPosition = start;
            _nodeCache.Clear();

            if (!IsWalkable(target, gridManager))
            {
                return null;
            }

            Node startNode = GetOrCreateNode(start, gridManager);
            Node targetNode = GetOrCreateNode(target, gridManager);

            List<Node> openSet = new() { startNode };
            HashSet<Vector2Int> closedSet = new();

            while (openSet.Count > 0)
            {
                Node currentNode = GetLowestFCostNode(openSet);

                if (currentNode.GridPosition == targetNode.GridPosition)
                {
                    return RetracePath(startNode, currentNode);
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode.GridPosition);

                foreach (Node neighbor in GetNeighbors(currentNode, gridManager))
                {
                    if (closedSet.Contains(neighbor.GridPosition))
                    {
                        continue;
                    }

                    int newCost =
                        currentNode.GCost + GetDistance(currentNode, neighbor);

                    if (newCost < neighbor.GCost ||
                        !openSet.Contains(neighbor))
                    {
                        neighbor.SetGCost(newCost);
                        neighbor.SetHCost(GetDistance(neighbor, targetNode));
                        neighbor.SetParent(currentNode);

                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
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
            List<Vector2Int> path = new();
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
                if (ShouldReplaceNode(nodes[i], lowestNode))
                {
                    lowestNode = nodes[i];
                }
            }

            return lowestNode;
        }

        private bool ShouldReplaceNode(Node candidate, Node currentBest)
        {
            if (candidate.FCost < currentBest.FCost)
            {
                return true;
            }

            if (candidate.FCost == currentBest.FCost &&
                candidate.HCost < currentBest.HCost)
            {
                return true;
            }

            return false;
        }

        private int GetDistance(Node a, Node b)
        {
            int deltaX = Mathf.Abs(a.GridPosition.x - b.GridPosition.x);
            int deltaY = Mathf.Abs(a.GridPosition.y - b.GridPosition.y);

            int diagonalSteps = Mathf.Min(deltaX, deltaY);
            int straightSteps = Mathf.Abs(deltaX - deltaY);

            return DiagonalCost * diagonalSteps +
                   StraightCost * straightSteps;
        }

        private List<Node> GetNeighbors(Node node, GridManager gridManager)
        {
            List<Node> neighbors = new();
            Vector2Int position = node.GridPosition;

            Vector2Int[] directions =
            {
                Vector2Int.up,
                Vector2Int.right,
                Vector2Int.down,
                Vector2Int.left,
                new Vector2Int(1, 1),
                new Vector2Int(-1, 1),
                new Vector2Int(1, -1),
                new Vector2Int(-1, -1)
            };

            foreach (Vector2Int direction in directions)
            {
                Vector2Int neighborPos = position + direction;

                if (!gridManager.IsCellExists(neighborPos))
                {
                    continue;
                }

                Node neighbor = GetOrCreateNode(neighborPos, gridManager);

                if (!neighbor.IsWalkable)
                {
                    continue;
                }

                if (IsDiagonal(direction) &&
                    IsDiagonalBlocked(position, direction, gridManager))
                {
                    continue;
                }

                neighbors.Add(neighbor);
            }

            return neighbors;
        }

        private bool IsDiagonal(Vector2Int direction)
        {
            return Mathf.Abs(direction.x) == 1 &&
                   Mathf.Abs(direction.y) == 1;
        }

        private bool IsDiagonalBlocked(
            Vector2Int position,
            Vector2Int direction,
            GridManager gridManager)
        {
            Vector2Int horizontalNeighbor =
                position + new Vector2Int(direction.x, 0);

            Vector2Int verticalNeighbor =
                position + new Vector2Int(0, direction.y);

            Cell horizontalCell = gridManager.GetCell(horizontalNeighbor);
            Cell verticalCell = gridManager.GetCell(verticalNeighbor);

            if (horizontalCell == null || horizontalCell.IsBlocked)
            {
                return true;
            }

            if (verticalCell == null || verticalCell.IsBlocked)
            {
                return true;
            }

            return false;
        }

        private bool IsWalkable(Vector2Int gridPosition, GridManager gridManager)
        {
            Cell cell = gridManager.GetCell(gridPosition);

            if (cell == null)
            {
                return false;
            }

            if (gridPosition == _startPosition)
            {
                return true;
            }

            return !cell.IsBlocked;
        }
    }
}