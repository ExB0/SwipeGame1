using UnityEngine;

namespace PlatformPuzzle.Pathfinder
{
    public class Node
    {
        public Node(Vector2Int position, bool isWalkable)
        {
            GridPosition = position;
            IsWalkable = isWalkable;
        }
        
        public Vector2Int GridPosition { get; }
        public bool IsWalkable { get; private set; }
        public Node Parent { get; private set; }
        public int GCost { get; private set; }
        public int HCost { get; private set; }
        public int FCost => GCost + HCost;

        public void SetParent(Node parent)
        {
            Parent = parent;
        }

        public void SetGCost(int gCost)
        {
            GCost = gCost;
        }

        public void SetHCost(int hCost)
        {
            HCost = hCost;
        }
    }
}