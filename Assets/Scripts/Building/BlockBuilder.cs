using UnityEngine;
using System.Collections.Generic;

public class BlockBuilder : MonoBehaviour
{
    public int gridSize = 5;
    private bool[,] grid;

    private void Start()
    {
        grid = new bool[gridSize, gridSize];
    }

    public void ToggleCell(int x, int y)
    {
        if (x >= 0 && x < gridSize && y >= 0 && y < gridSize)
        {
            grid[x, y] = !grid[x, y];
        }
    }

    public BlockData CreateBlockData(string name)
    {
        List<Vector2Int> cells = new List<Vector2Int>();
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (grid[x, y])
                {
                    cells.Add(new Vector2Int(x, y));
                }
            }
        }

        if (cells.Count == 0 || cells.Count > 6)
        {
            Debug.LogError("Invalid block size");
            return null;
        }

        // Check contiguity here (BFS/DFS)

        BlockData newData = ScriptableObject.CreateInstance<BlockData>();
        newData.cells = cells.ToArray();
        newData.name = name;
        
        // Save asset (Editor only usually, but for runtime we might need JSON or just keep in memory)
        // For this assignment, we might simulate saving or use JSON serialization for custom blocks
        
        return newData;
    }

    public void SaveBlock(BlockData data)
    {
        string json = JsonUtility.ToJson(data);
        // Save to file or PlayerPrefs
        PlayerPrefs.SetString($"CustomBlock_{data.name}", json);
        PlayerPrefs.Save();
    }

    public BlockData LoadBlock(string name)
    {
        if (PlayerPrefs.HasKey($"CustomBlock_{name}"))
        {
            string json = PlayerPrefs.GetString($"CustomBlock_{name}");
            BlockData data = ScriptableObject.CreateInstance<BlockData>();
            JsonUtility.FromJsonOverwrite(json, data);
            return data;
        }
        return null;
    }

    public void DeleteBlock(string name)
    {
        if (PlayerPrefs.HasKey($"CustomBlock_{name}"))
        {
            PlayerPrefs.DeleteKey($"CustomBlock_{name}");
            PlayerPrefs.Save();
        }
    }

    public void RestoreDefaults()
    {
        // Clear all custom blocks or reset specific keys
        // For now, let's just log it as we don't have a list of all custom blocks tracked yet
        Debug.Log("Restoring defaults...");
    }
}


