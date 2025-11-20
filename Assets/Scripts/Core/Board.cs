using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    [Header("References")]
    public Tilemap tilemap;
    public Tilemap backgroundTilemap; // New: Reference to background
    public Block activeBlock;
    
    [Header("Settings")]
    public Vector2Int boardSize = new Vector2Int(10, 20);
    public Vector3Int spawnPosition = new Vector3Int(0, 8, 0);
    
    [Header("Visuals")]
    public Color gridColor = new Color(0.1f, 0.1f, 0.1f, 1f); // Dark grey
    
    private void Start()
    {
        // 1. Auto-assign refs if missing (Safety check)
        if (tilemap == null) tilemap = transform.Find("Grid/Tilemap")?.GetComponent<Tilemap>();
        if (backgroundTilemap == null) backgroundTilemap = transform.Find("Grid/BackgroundTilemap")?.GetComponent<Tilemap>();

        // 2. Configure Tilemaps
        ConfigureTilemap(tilemap);
        ConfigureTilemap(backgroundTilemap);

        // 3. Setup View
        FitCamera();
        DrawGrid();

        SpawnBlock();
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

        // Calculate height needed: Board Height / 2 + Padding (2 units)
        float targetSize = (boardSize.y / 2f) + 2f;
        
        cam.orthographic = true;
        cam.orthographicSize = targetSize;
        
        // Center the camera on the board (assuming board is centered at 0,0)
        // We keep Z at -10 to ensure things are rendered
        cam.transform.position = new Vector3(0, 0, -10);
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

        BlockData data = GameManager.Instance.GetRandomBlock();
        if (data == null) return;

        GameObject blockObj = new GameObject("Block");
        activeBlock = blockObj.AddComponent<Block>();
        activeBlock.Initialize(this, data);

        // Center Logic + Spawn Y
        Vector3Int centerOffset = CalculateSpawnOffset(data);
        blockObj.transform.position = new Vector3Int(centerOffset.x, spawnPosition.y, 0);

        if (!IsValidPosition(activeBlock, Vector3Int.RoundToInt(blockObj.transform.position)))
        {
            Destroy(blockObj);
            GameManager.Instance.SetState(GameManager.GameState.GameOver);
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

        while (row < bounds.yMax)
        {
            if (IsLineFull(row)) LineClear(row);
            else row++;
        }
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