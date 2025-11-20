using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap tilemap;
    public Block activeBlock;
    public Vector2Int boardSize = new Vector2Int(10, 20);
    
    private int spawnY = 8; 

    private void Start()
    {
        if (tilemap == null) tilemap = GetComponentInChildren<Tilemap>();
        
        // --- VISUAL FIX: ALIGNMENT ---
        // Force the Tilemap to draw tiles at strict integer coordinates (0,0) 
        // instead of the cell center (0.5, 0.5). This prevents the "Snap/Jump".
        tilemap.tileAnchor = Vector3.zero;
        
        // Force Grid to be 1x1
        if (tilemap.layoutGrid != null)
        {
            tilemap.layoutGrid.cellSize = Vector3.one;
            tilemap.layoutGrid.cellGap = Vector3.zero;
        }
        // -----------------------------

        SpawnBlock();
    }

    public void SpawnBlock()
    {
        if (GameManager.Instance == null) return;

        BlockData data = GameManager.Instance.GetRandomBlock();
        if (data == null) return;

        GameObject blockObj = new GameObject("Block");
        activeBlock = blockObj.AddComponent<Block>();
        activeBlock.Initialize(this, data);

        // Center Logic
        Vector3Int centerOffset = CalculateSpawnOffset(data);
        blockObj.transform.position = new Vector3Int(centerOffset.x, spawnY, 0);

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

        // Adjust centering logic for 0-anchor
        int midPoint = (minX + maxX) / 2;
        int xOffset = (midPoint >= 0) ? -1 : 0;
        return new Vector3Int(xOffset, 0, 0);
    }

    // Check if Whole Block fits
    public bool IsValidPosition(Block block, Vector3Int position)
    {
        RectInt bounds = new RectInt(new Vector2Int(-boardSize.x / 2, -boardSize.y / 2), boardSize);

        foreach (Vector2Int cell in block.data.cells)
        {
            Vector3Int tilePos = Vector3Int.RoundToInt(block.transform.TransformPoint((Vector3Int)cell));

            if (!bounds.Contains((Vector2Int)tilePos)) return false;
            if (tilemap.HasTile(tilePos)) return false;
        }
        return true;
    }

    // Check if Single Point fits
    public bool IsValidPosition(Vector3Int pos)
    {
        RectInt bounds = new RectInt(new Vector2Int(-boardSize.x / 2, -boardSize.y / 2), boardSize);
        if (!bounds.Contains((Vector2Int)pos)) return false;
        if (tilemap.HasTile(pos)) return false;
        return true;
    }

    public void LockBlock(Block block)
    {
        foreach (Vector2Int cell in block.data.cells)
        {
            Vector3Int tilePos = Vector3Int.RoundToInt(block.transform.TransformPoint((Vector3Int)cell));
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.color = block.data.color;
            Sprite s = Resources.Load<Sprite>("Square");
            if(s != null) tile.sprite = s;
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