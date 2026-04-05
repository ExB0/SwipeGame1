using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Vector2Int GridPosition { get; }
    public bool IsWalkable { get; set; }
    public Node Parent { get; set; }

    public int GCost { get; set; }
    public int HCost { get; set; }
    public int FCost => GCost + HCost;

    public Node(Vector2Int position, bool isWalkable)
    {
        GridPosition = position;
        IsWalkable = isWalkable;
    }
}

