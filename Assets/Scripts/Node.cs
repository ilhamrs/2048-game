using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a single cell (node) in the grid, potentially occupied by a block.
/// </summary>
public class Node : MonoBehaviour
{
    public Vector2 Pos => transform.position; // Position of the node on the grid.
    public Block OccupiedBlock; // The block currently occupying this node (if any).
}
