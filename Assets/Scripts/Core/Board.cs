using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap tilemap;
    public Block activeBlock;
    public Vector2Int boardSize = new Vector2Int(10, 20);
    public RectInt Bounds => new RectInt(new Vector2Int(-boardSize.x / 2, -boardSize.y / 2), boardSize);

    private void Awake()
    {
        if (tilemap == null)
            tilemap = GetComponentInChildren<Tilemap>();
    }

    public void SpawnBlock(BlockData data)
    {
        // Instantiate block prefab and set data
        // Set activeBlock
    }

    public bool IsValidPosition(Block block)
    {
        foreach (Transform child in block.transform)
        {
            Vector3Int pos = Vector3Int.RoundToInt(child.position);

            if (!Bounds.Contains((Vector2Int)pos))
                return false;

            if (tilemap.HasTile(pos))
                return false;
        }
        return true;
    }

    public void LockBlock(Block block)
    {
        // Transfer block tiles to tilemap
        // Clear lines
        // Spawn new block or Game Over
    }

    public void ClearLines()
    {
        // Check for full rows and clear them
    }
}
