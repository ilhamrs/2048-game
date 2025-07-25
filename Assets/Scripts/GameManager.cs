using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Manages the overall game flow, including grid generation, input handling, shifting blocks, and game states.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private int width = 4; // Grid width.
    [SerializeField] private int height = 4; // Grid height.
    [SerializeField] private Node nodePrefab; // Prefab for grid nodes.
    [SerializeField] private Block blockPrefab; // Prefab for blocks.
    [SerializeField] private SpriteRenderer boardPrefab; // Background board sprite.
    [SerializeField] private List<BlockType> types; // List of available block types (values/colors).
    [SerializeField] private float travelTime = .2f; // Time for block animations.
    [SerializeField] private int winCondition = 2048; // Target block value to win.
    [SerializeField] private GameObject winScreen; // UI shown on win.
    [SerializeField] private GameObject loseScreen; // UI shown on lose.
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private float swipeThreshold = 50f; // Minimum distance for a swipe to count
    [SerializeField] private TextMeshProUGUI highscoreText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI comboText;

    private List<Node> nodes; // List of all grid nodes.
    private List<Block> blocks; // List of all active blocks.
    private GameState state; // Current game state.
    private int round; // Current round number.
    private bool isPause = false;

    private Vector2 touchStart;
    private Vector2 touchEnd;

    private int score;
    private int highscore = 0;

    private BlockType GetBlockTypeByValue(int value) => types.First(t => t.Value == value);
    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        Application.targetFrameRate = 60;
        ChangeState(GameState.GenerateLevel);
        highscoreText.text = highscore + "";
    }
    public void SetHighScore(int value)
    {
        highscore = value;
        highscoreText.text = highscore + "";
    }

    /// <summary>
    /// Changes the current game state and triggers corresponding actions.
    /// </summary>
    private void ChangeState(GameState newState)
    {
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
                SaveHighScore();
                winScreen.SetActive(true);
                break;
            case GameState.Lose:
                SaveHighScore();
                loseScreen.SetActive(true);
                break;
        }
    }

    void Update()
    {
        if (state != GameState.WaitingInput || isPause) return;

        // Keyboard input
        if (Input.GetKeyDown(KeyCode.LeftArrow)) Shift(Vector2.left);
        if (Input.GetKeyDown(KeyCode.RightArrow)) Shift(Vector2.right);
        if (Input.GetKeyDown(KeyCode.UpArrow)) Shift(Vector2.up);
        if (Input.GetKeyDown(KeyCode.DownArrow)) Shift(Vector2.down);

        // Mobile touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStart = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                touchEnd = touch.position;
                HandleSwipe();
            }
        }
    }


    /// <summary>
    /// Generates the initial grid and board layout.
    /// </summary>
    void GenerateGrid()
    {
        score = 0;
        round = 0;
        nodes = new List<Node>();
        blocks = new List<Block>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < width; y++)
            {
                var node = Instantiate(nodePrefab, new Vector2(x, y), Quaternion.identity);
                nodes.Add(node);
            }
        }

        var center = new Vector2((float)width / 2 - .5f, (float)height / 2 - .5f);

        var board = Instantiate(boardPrefab, center, Quaternion.identity);
        board.size = new Vector2(width, height);

        Camera.main.transform.position = new Vector3(center.x, center.y, -10);

        ChangeState(GameState.SpawningBlocks);
    }

    /// <summary>
    /// Spawns a set amount of new blocks in random empty nodes.
    /// </summary>
    void SpawnBlocks(int amount)
    {
        var freeNodes = nodes.Where(n => n.OccupiedBlock == null).OrderBy(b => UnityEngine.Random.value).ToList();

        foreach (var node in freeNodes.Take(amount))
        {
            var block = Instantiate(blockPrefab, node.Pos, Quaternion.identity);
            block.Init(GetBlockTypeByValue(UnityEngine.Random.value > 0.8f ? 4 : 2));
            block.SetBlock(node);
            blocks.Add(block);
        }

        if (freeNodes.Count() == 1 && !CanMoveOrMerge())
        {
            ChangeState(GameState.Lose);
            return;
        }

        ChangeState(blocks.Any(b => b.Value == winCondition) ? GameState.Win : GameState.WaitingInput);
    }

    /// <summary>
    /// Spawns a single block of a specific value on a given node.
    /// </summary>
    void SpawnBlock(Node node, int value)
    {
        var block = Instantiate(blockPrefab, node.Pos, Quaternion.identity);
        block.Init(GetBlockTypeByValue(value));
        block.SetBlock(node);
        blocks.Add(block);
    }

    /// <summary>
    /// Shifts all blocks in the specified direction and handles merging.
    /// </summary>
    void Shift(Vector2 dir)
    {
        if (!CanMoveOrMerge(dir)) return;

        ChangeState(GameState.Moving);

        var orderedBlocks = blocks.OrderBy(b => b.Pos.x).ThenBy(b => b.Pos.y).ToList();

        if (dir == Vector2.right || dir == Vector2.up) orderedBlocks.Reverse();

        foreach (var block in orderedBlocks)
        {
            var next = block.Node;
            do
            {
                block.SetBlock(next);

                var possibleNode = GetNodeAtPosition(next.Pos + dir);

                if (possibleNode != null)
                {
                    if (possibleNode.OccupiedBlock != null && possibleNode.OccupiedBlock.CanMerge(block.Value))
                    {
                        block.MergeBlock(possibleNode.OccupiedBlock);
                    }
                    else if (possibleNode.OccupiedBlock == null) next = possibleNode;
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

        sequence.OnComplete(() =>
        {
            foreach (var block in orderedBlocks.Where(b => b.MergingBlock != null))
            {
                MergeBlock(block.MergingBlock, block);
            }
            ChangeState(GameState.SpawningBlocks);
        });
    }

    /// <summary>
    /// Merges two blocks into a new block with doubled value.
    /// </summary>
    void MergeBlock(Block baseBlock, Block mergingBlock)
    {
        AddScore(baseBlock.Value * 2);

        SpawnBlock(baseBlock.Node, baseBlock.Value * 2);

        RemoveBlock(baseBlock);
        RemoveBlock(mergingBlock);
    }
    void AddScore(int addValue)
    {
        score += addValue;
        scoreText.text = score + "";
    }

    /// <summary>
    /// Removes a block from the game.
    /// </summary>
    void RemoveBlock(Block block)
    {
        blocks.Remove(block);
        Destroy(block.gameObject);
    }

    /// <summary>
    /// Returns the node at a given position, or null if none exists.
    /// </summary>
    Node GetNodeAtPosition(Vector2 pos)
    {
        return nodes.FirstOrDefault(n => n.Pos == pos);
    }

    /// <summary>
    /// Checks if there are any possible moves or merges.
    /// </summary>
    bool CanMoveOrMerge()
    {
        foreach (var block in blocks)
        {
            // Check four directions for each block
            Vector2[] directions = { Vector2.left, Vector2.right, Vector2.up, Vector2.down };

            foreach (var dir in directions)
            {
                var neighborNode = GetNodeAtPosition(block.Pos + dir);
                if (neighborNode == null) continue;

                if (neighborNode.OccupiedBlock == null)
                {
                    // There is an empty space next to the block (can move)
                    return true;
                }
                else if (neighborNode.OccupiedBlock.CanMerge(block.Value))
                {
                    // There is a block that can merge with the current block
                    return true;
                }
            }
        }

        // No moves or merges possible
        return false;
    }

    /// <summary>
    /// Checks if there are any possible moves or merges.
    /// </summary>
    bool CanMoveOrMerge(Vector2 dir)
    {
        foreach (var block in blocks)
        {
            var neighborNode = GetNodeAtPosition(block.Pos + dir);
            if (neighborNode == null) continue;

            if (neighborNode.OccupiedBlock == null)
            {
                // There is an empty space next to the block (can move)
                return true;
            }
            else if (neighborNode.OccupiedBlock.CanMerge(block.Value))
            {
                // There is a block that can merge with the current block
                return true;
            }
        }

        // No moves or merges possible
        return false;
    }

    /// <summary>
    /// Detects swipe direction from touch input and shifts blocks accordingly.
    /// </summary>
    void HandleSwipe()
    {
        Vector2 swipe = touchEnd - touchStart;

        Debug.Log("Swipe Detected: " + swipe);


        if (swipe.magnitude < swipeThreshold) return; // Ignore short swipes

        swipe.Normalize(); // Make it direction-only

        if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
        {
            // Horizontal swipe
            if (swipe.x > 0)
            {
                Shift(Vector2.right);
            }
            else
            {
                Shift(Vector2.left);
            }
        }
        else
        {
            // Vertical swipe
            if (swipe.y > 0)
            {
                Shift(Vector2.up);
            }
            else
            {
                Shift(Vector2.down);
            }
        }

        touchStart = Vector2.zero;
        touchEnd = Vector2.zero;
    }

    public void Reset()
    {

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMainMenu()
    {
        InterstitialAd.Instance?.ShowAd();
        SceneManager.LoadScene("Home");
    }
    void SaveHighScore()
    {
        if (score > highscore) SaveGameManager.Instance.Save(score);
    }
    public void togglePause()
    {
        pauseScreen.SetActive(isPause);
        isPause = !isPause;
    }
    public void SetPause(bool v)
    {
        StartCoroutine(SetPauseDelay(v));
        // pauseScreen.SetActive(v);
        // isPause = v;
    }
    IEnumerator SetPauseDelay(bool v)
    {
        yield return new WaitForSeconds(.3f);

        pauseScreen.SetActive(v);
        isPause = v;
    }


}

/// <summary>
/// Represents a type of block with a value and a color.
/// </summary>
[Serializable]
public struct BlockType
{
    public int Value;
    public Color Color;
}

/// <summary>
/// Possible game states.
/// </summary>
public enum GameState{
    GenerateLevel,
    SpawningBlocks,
    WaitingInput,
    Moving,
    Win,
    Lose
}
