using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Represents a single block in the game, holding a value and handling merging logic.
/// </summary>
public class Block : MonoBehaviour
{
    public int Value; // The numerical value of the block.
    public Node Node; // The current node this block occupies.
    public Block MergingBlock; // The block this block is set to merge with.
    public bool Merging; // Whether the block is in the process of merging.

    public Vector2 Pos => transform.position; // The current position of the block.

    [SerializeField] private SpriteRenderer render; // Renderer for the block's visual appearance.
    [SerializeField] private TextMeshPro text; // Text to display the block's value.

    /// <summary>
    /// Initializes the block with a specific type (value and color).
    /// </summary>
    public void Init(BlockType type){
        Value = type.Value;
        render.color = type.Color;
        text.text = type.Value.ToString();
    }

    /// <summary>
    /// Assigns the block to a node.
    /// </summary>
    public void SetBlock(Node node){
        if(Node != null) Node.OccupiedBlock = null;
        Node = node;
        Node.OccupiedBlock = this;
    }

    /// <summary>
    /// Prepares this block to merge with another block.
    /// </summary>
    public void MergeBlock(Block blockToMergeWith){
        MergingBlock = blockToMergeWith;
        Node.OccupiedBlock = null;
        MergingBlock.Merging = true;
    }

    /// <summary>
    /// Checks if the block can merge with another block of a given value.
    /// </summary>
    public bool CanMerge(int value) => value == Value && !Merging && MergingBlock == null;
}
