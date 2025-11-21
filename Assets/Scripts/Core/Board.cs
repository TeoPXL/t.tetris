using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

public class Board : MonoBehaviour
{
    [Header("References")]
    public Tilemap tilemap;
    public Tilemap backgroundTilemap; // New: Reference to background
    public Block activeBlock;
    
    [Header("Audio")]
    public AudioClip moveClip;
    public AudioClip clearClip;
    private AudioSource audioSource;
    
    [Header("Settings")]
    public Vector2Int boardSize = new Vector2Int(10, 20);
    public Vector3Int spawnPosition = new Vector3Int(0, 8, 0);
    
    [Header("Visuals")]
    public Color gridColor = new Color(0.1f, 0.1f, 0.1f, 1f); // Dark grey

    [Header("Game State")]
    public int score = 0;
    public BlockData heldBlock;
    public bool canHold = true;
    private Queue<BlockData> nextBlocks = new Queue<BlockData>();
    private const int NextBlockCount = 3;
    
    private void Start()
    {
        // --- AUDIO SETUP ---
        audioSource = gameObject.AddComponent<AudioSource>();
        // Load clips automatically from Assets/Resources/Sounds/
        if (moveClip == null) moveClip = Resources.Load<AudioClip>("Sounds/beat");
        if (clearClip == null) clearClip = Resources.Load<AudioClip>("Sounds/clear");
        // -------------------

        if (tilemap == null) tilemap = transform.Find("Grid/Tilemap")?.GetComponent<Tilemap>();
        if (backgroundTilemap == null) backgroundTilemap = transform.Find("Grid/BackgroundTilemap")?.GetComponent<Tilemap>();

        ConfigureTilemap(tilemap);
        ConfigureTilemap(backgroundTilemap);

        FitCamera();
        DrawGrid();

        InitializeQueue();
        SpawnBlock();
    }

    private void InitializeQueue()
    {
        for (int i = 0; i < NextBlockCount; i++)
        {
            nextBlocks.Enqueue(GameManager.Instance.GetRandomBlock());
        }
        UpdateHUD();
    }

    private BlockData GetNextBlock()
    {
        BlockData next = nextBlocks.Dequeue();
        nextBlocks.Enqueue(GameManager.Instance.GetRandomBlock());
        UpdateHUD();
        return next;
    }

    private void UpdateHUD()
    {
        GameHUD hud = FindObjectOfType<GameHUD>();
        if (hud != null)
        {
            hud.UpdateScore(score);
            hud.UpdateNext(nextBlocks.ToList());
            hud.UpdateHold(heldBlock);
        }
    }
    
    public void PlayMoveSound()
    {
        if (moveClip != null) audioSource.PlayOneShot(moveClip);
    }

    public void PlayClearSound()
    {
        if (clearClip != null) audioSource.PlayOneShot(clearClip);
    }

    private void ConfigureTilemap(Tilemap tm)
    {
        if (tm == null) return;
        tm.tileAnchor = Vector3.zero;
        if (tm.layoutGrid != null)
        {
            tm.layoutGrid.cellSize = Vector3.one;
            tm.layoutGrid.cellGap = Vector3.zero;
        }
    }

    private void FitCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // Height calculation
        float targetHeight = boardSize.y + 4f; // Board height + padding
        
        // Set Size
        cam.orthographic = true;
        cam.orthographicSize = targetHeight / 2f;
        
        // Position
        // Center X: 0 (Board center)
        // Center Y: -1 (Visual offset to account for ground)
        cam.transform.position = new Vector3(0, -1, -10);
        
        // Viewport Rect
        // Squeeze the camera rendering into the middle 50% of the screen
        // to prevent it from being covered by the UI sidebars
        cam.rect = new Rect(0.25f, 0f, 0.5f, 1f);
    }

    private void DrawGrid()
    {
        if (backgroundTilemap == null) return;

        backgroundTilemap.ClearAllTiles();
        
        // Create a visual tile for the background
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = Resources.Load<Sprite>("Square"); // Uses your existing Square asset
        tile.color = gridColor;

        // Loop from negative half to positive half to cover the board centered at 0,0
        RectInt bounds = new RectInt(new Vector2Int(-boardSize.x / 2, -boardSize.y / 2), boardSize);

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                backgroundTilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
    }

    public void SpawnBlock()
    {
        if (GameManager.Instance == null) return;

        BlockData data = GetNextBlock();
        SpawnBlock(data);
    }

    public void SpawnBlock(BlockData data)
    {
        if (data == null) return;
        
        canHold = true; // Reset hold capability on new spawn

        GameObject blockObj = new GameObject("Block");
        activeBlock = blockObj.AddComponent<Block>();
        activeBlock.Initialize(this, data);

        // Center Logic + Spawn Y
        Vector3Int centerOffset = CalculateSpawnOffset(data);
        blockObj.transform.position = new Vector3Int(centerOffset.x, spawnPosition.y, 0);

        if (!IsValidPosition(activeBlock, Vector3Int.RoundToInt(blockObj.transform.position)))
        {
            Destroy(blockObj);
            GameOver();
        }
    }

    private void GameOver()
    {
        GameManager.Instance.SetState(GameManager.GameState.GameOver);
        // Show Game Over UI
        GameHUD hud = FindObjectOfType<GameHUD>();
        if (hud != null)
        {
            hud.ShowGameOver(score);
        }
    }

    private Vector3Int CalculateSpawnOffset(BlockData data)
    {
        if (data.cells == null || data.cells.Length == 0) return Vector3Int.zero;
        int minX = int.MaxValue;
        int maxX = int.MinValue;
        foreach (var cell in data.cells)
        {
            if (cell.x < minX) minX = cell.x;
            if (cell.x > maxX) maxX = cell.x;
        }
        int midPoint = (minX + maxX) / 2;
        int xOffset = (midPoint >= 0) ? -1 : 0;
        return new Vector3Int(xOffset, 0, 0);
    }

    public bool IsValidPosition(Block block, Vector3Int position)
    {
        RectInt bounds = new RectInt(new Vector2Int(-boardSize.x / 2, -boardSize.y / 2), boardSize);

        foreach (Vector2Int cell in block.data.cells)
        {
            // FIX: Manually create a Vector3 from the Vector2Int cell data
            Vector3 cellLocal = new Vector3(cell.x, cell.y, 0);

            // 1. Get the rotated offset
            Vector3 cellOffset = block.transform.rotation * cellLocal;
            
            // 2. Add that offset to the PROPOSED position
            Vector3Int tilePos = Vector3Int.RoundToInt((Vector3)position + cellOffset);

            // 3. Check bounds and tiles
            if (!bounds.Contains((Vector2Int)tilePos)) return false;
            if (tilemap.HasTile(tilePos)) return false;
        }
        return true;
    }

    public void LockBlock(Block block)
    {
        foreach (Vector2Int cell in block.data.cells)
        {
            Vector3Int tilePos = Vector3Int.RoundToInt(block.transform.TransformPoint((Vector3Int)cell));
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.color = block.data.color;
            tile.sprite = Resources.Load<Sprite>("Square");
            tilemap.SetTile(tilePos, tile);
        }

        Destroy(block.gameObject);
        ClearLines();
        SpawnBlock();
    }

    public void ClearLines()
    {
        RectInt bounds = new RectInt(new Vector2Int(-boardSize.x / 2, -boardSize.y / 2), boardSize);
        int row = bounds.yMin;
        int linesCleared = 0;

        while (row < bounds.yMax)
        {
            if (IsLineFull(row))
            {
                LineClear(row);
                linesCleared++;
            }
            else
            {
                row++;
            }
        }
        
        if (linesCleared > 0)
        {
            PlayClearSound();
            CalculateScore(linesCleared);
        }
    }

    private void CalculateScore(int lines)
    {
        int points = 0;
        switch (lines)
        {
            case 1: points = 100; break;
            case 2: points = 300; break;
            case 3: points = 500; break;
            case 4: points = 800; break;
        }
        score += points;
        UpdateHUD();
    }

    public void HoldBlock()
    {
        if (!canHold || activeBlock == null) return;

        BlockData currentData = activeBlock.data;
        Destroy(activeBlock.gameObject);

        if (heldBlock == null)
        {
            heldBlock = currentData;
            SpawnBlock(); // Spawns next from queue
        }
        else
        {
            BlockData temp = heldBlock;
            heldBlock = currentData;
            SpawnBlock(temp); // Spawns the previously held block
        }

        canHold = false;
        UpdateHUD();
    }

    private bool IsLineFull(int row)
    {
        RectInt bounds = new RectInt(new Vector2Int(-boardSize.x / 2, -boardSize.y / 2), boardSize);
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            if (!tilemap.HasTile(new Vector3Int(col, row, 0))) return false;
        }
        return true;
    }

    private void LineClear(int row)
    {
        RectInt bounds = new RectInt(new Vector2Int(-boardSize.x / 2, -boardSize.y / 2), boardSize);
        for (int col = bounds.xMin; col < bounds.xMax; col++) tilemap.SetTile(new Vector3Int(col, row, 0), null);
        for (int r = row + 1; r < bounds.yMax; r++)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                TileBase tile = tilemap.GetTile(new Vector3Int(col, r, 0));
                tilemap.SetTile(new Vector3Int(col, r - 1, 0), tile);
            }
        }
    }
}