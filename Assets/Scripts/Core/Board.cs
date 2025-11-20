using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap tilemap;
    public Block activeBlock;
    public Vector2Int boardSize = new Vector2Int(10, 20);
    public Vector3Int spawnPosition = new Vector3Int(0, 8, 0);
    
    private void Start()
    {
        SpawnBlock();
    }

    public void SpawnBlock()
    {
        BlockData data = GameManager.Instance.GetRandomBlock();
        if (data == null) return;

        GameObject blockObj = new GameObject("Block");
        blockObj.transform.position = spawnPosition;
        activeBlock = blockObj.AddComponent<Block>();
        activeBlock.Initialize(this, data);
    }

    public bool IsValidPosition(Block block, Vector3Int position)
    {
        RectInt bounds = new RectInt(new Vector2Int(-boardSize.x / 2, -boardSize.y / 2), boardSize);

        foreach (Vector2Int cell in block.data.cells)
        {
            // Convert local cell pos to world pos based on block position/rotation
            Vector3Int tilePos = Vector3Int.RoundToInt(block.transform.TransformPoint((Vector3Int)cell));

            if (!bounds.Contains((Vector2Int)tilePos))
                return false;

            if (tilemap.HasTile(tilePos))
                return false;
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
            tile.sprite = Resources.Load<Sprite>("Square"); // Ensure Square.png exists in Resources
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
            if (IsLineFull(row))
            {
                LineClear(row);
                // Don't increment row, check same index again because lines shifted down
            }
            else
            {
                row++;
            }
        }
    }

    private bool IsLineFull(int row)
    {
        RectInt bounds = new RectInt(new Vector2Int(-boardSize.x / 2, -boardSize.y / 2), boardSize);
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            if (!tilemap.HasTile(new Vector3Int(col, row, 0)))
                return false;
        }
        return true;
    }

    private void LineClear(int row)
    {
        RectInt bounds = new RectInt(new Vector2Int(-boardSize.x / 2, -boardSize.y / 2), boardSize);
        
        // Clear
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            tilemap.SetTile(new Vector3Int(col, row, 0), null);
        }

        // Shift down
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