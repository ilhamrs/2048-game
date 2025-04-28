using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int width = 4;
    [SerializeField] private int height = 4;
    [SerializeField] private Node nodePrefab;
    [SerializeField] private Block blockPrefab;
    [SerializeField] private SpriteRenderer boardPrefab;
    [SerializeField] private List<BlockType> types;
    [SerializeField] private float travelTime = .2f;
    [SerializeField] private int winCondition = 2048; 
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;

    private List<Node> nodes;
    private List<Block> blocks;
    private GameState state;
    private int round;

    private BlockType GetBlockTypeByValue(int value) => types.First(t => t.Value == value);

    void Start()
    {
        ChangeState(GameState.GenerateLevel);
    }

    private void ChangeState(GameState newState){
        state = newState;

        switch (newState)
        {
            case GameState.GenerateLevel:
                GenerateGrid();
                break;
            case GameState.SpawningBlocks:
                SpawnBlocks(round++ == 0 ? 2 : 1);
                break;
            case GameState.WaitingInput:
                break;
            case GameState.Moving:
                break;
            case GameState.Win:
                winScreen.SetActive(true);
                break;
            case GameState.Lose:
                loseScreen.SetActive(true);
                break;
            default:
                break;
        }
    }

    void Update()
    {
        if(state != GameState.WaitingInput) return;

        if(Input.GetKeyDown(KeyCode.LeftArrow)) Shift(Vector2.left);
        if(Input.GetKeyDown(KeyCode.RightArrow)) Shift(Vector2.right);
        if(Input.GetKeyDown(KeyCode.UpArrow)) Shift(Vector2.up);
        if(Input.GetKeyDown(KeyCode.DownArrow)) Shift(Vector2.down);
    }

    void GenerateGrid()
    {
        round = 0;
        nodes = new List<Node>();
        blocks = new List<Block>();
        for(int x = 0; x < width; x++){
            for(int y = 0; y < width; y++){
                var node = Instantiate(nodePrefab, new Vector2(x, y), Quaternion.identity);
                nodes.Add(node);
            }
        }

        var center = new Vector2((float) width / 2 - .5f, (float) height / 2 - .5f);

        var board = Instantiate(boardPrefab, center, Quaternion.identity);
        board.size = new Vector2(width, height);

        Camera.main.transform.position = new Vector3(center.x, center.y, -10);

        ChangeState(GameState.SpawningBlocks);
    }

    void SpawnBlocks(int amount){
        var freeNodes = nodes.Where(n=>n.OccupiedBlock == null).OrderBy(b => UnityEngine.Random.value).ToList();

        foreach (var node in freeNodes.Take(amount))
        {
            var block = Instantiate(blockPrefab, node.Pos, Quaternion.identity);
            block.Init(GetBlockTypeByValue(UnityEngine.Random.value > 0.8f ? 4 : 2));
            block.SetBlock(node);
            blocks.Add(block);
        }

        if(freeNodes.Count() == 1){
            ChangeState(GameState.Lose);
            return;
        }

        ChangeState(blocks.Any(b => b.Value == winCondition) ? GameState.Win : GameState.WaitingInput);
    }

    void SpawnBlock(Node node, int value){
        var block = Instantiate(blockPrefab, node.Pos, Quaternion.identity);
        block.Init(GetBlockTypeByValue(value));
        block.SetBlock(node);
        blocks.Add(block);
    }

    void Shift(Vector2 dir){
        ChangeState(GameState.Moving);

        var orderedBlocks = blocks.OrderBy(b => b.Pos.x).ThenBy(b => b.Pos.y).ToList();

        if(dir == Vector2.right || dir == Vector2.up) orderedBlocks.Reverse();

        foreach (var block in orderedBlocks)
        {
            var next = block.Node;
            do
            {
                block.SetBlock(next);

                var possibleNode = GetNodeAtPosition(next.Pos + dir);

                if(possibleNode != null)
                {
                    if(possibleNode.OccupiedBlock != null && possibleNode.OccupiedBlock.CanMerge(block.Value)){
                        block.MergeBlock(possibleNode.OccupiedBlock);

                    }
                    else if(possibleNode.OccupiedBlock == null) next = possibleNode;
                }
                
            } while (next != block.Node);

            block.transform.DOMove(block.Node.Pos, travelTime);
        }

        var sequence = DOTween.Sequence();

        foreach (var block in orderedBlocks)
        {
            var movePoint = block.MergingBlock != null ? block.MergingBlock.Node.Pos : block.Node.Pos;

            sequence.Insert(0, block.transform.DOMove(movePoint, travelTime));

        }

        sequence.OnComplete(() => {
            foreach (var block in orderedBlocks.Where(b => b.MergingBlock != null))
            {
                MergeBlock(block.MergingBlock, block);
            }

            ChangeState(GameState.SpawningBlocks);
        });

    }

    void MergeBlock(Block baseBlock, Block mergingBlock){
        SpawnBlock(baseBlock.Node, baseBlock.Value * 2);

        RemoveBlock(baseBlock);
        RemoveBlock(mergingBlock);
    }

    void RemoveBlock(Block block){
        blocks.Remove(block);
        Destroy(block.gameObject);
    }

    Node GetNodeAtPosition(Vector2 pos){
        return nodes.FirstOrDefault(n => n.Pos == pos);
    }
}


[Serializable]
public struct BlockType
{
    public int Value;
    public Color Color;
}

public enum GameState{
    GenerateLevel,
    SpawningBlocks,
    WaitingInput,
    Moving,
    Win,
    Lose
}