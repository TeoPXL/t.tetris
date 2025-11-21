using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BlockBuilder : MonoBehaviour
{
    private HashSet<Vector2Int> activeCells = new HashSet<Vector2Int>();
    private const int MAX_BLOCKS = 10;
    private const string REGISTRY_KEY = "BLOCK_REGISTRY";

    private void Start() { ResetBuilder(); }

    public void ResetBuilder()
    {
        activeCells.Clear();
        activeCells.Add(Vector2Int.zero); // Always start center
    }

    public bool AddCell(Vector2Int pos)
    {
        if (activeCells.Count >= MAX_BLOCKS) return false;
        if (activeCells.Contains(pos)) return false;
        activeCells.Add(pos);
        return true;
    }

    public void RemoveCell(Vector2Int pos)
    {
        if (pos == Vector2Int.zero) return; // Cannot remove anchor
        if (activeCells.Contains(pos)) activeCells.Remove(pos);
    }

    public List<Vector2Int> GetActiveCells() { return activeCells.ToList(); }

    public List<Vector2Int> GetAvailableExpansions()
    {
        HashSet<Vector2Int> candidates = new HashSet<Vector2Int>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int cell in activeCells)
        {
            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighbor = cell + dir;
                if (!activeCells.Contains(neighbor)) candidates.Add(neighbor);
            }
        }
        return candidates.ToList();
    }

    // --- SAVING SYSTEM ---

    public void SaveBlock(string name)
    {
        if (string.IsNullOrEmpty(name) || activeCells.Count == 0) return;

        // Use a serializable class that mimics BlockData structure but is JSON friendly
        // Since we don't have that, we rely on JsonUtility to handle the SO fields.
        BlockData data = ScriptableObject.CreateInstance<BlockData>();
        data.cells = activeCells.ToArray();
        data.name = name;
        // The color is the biggest issue for JSON serialization in PlayerPrefs.
        // We'll store the color as a separate hex string or using the SO itself.
        // Since the requirement is to use the existing data, we rely on the SO fields being public/serialized.
        data.color = Color.HSVToRGB(Random.value, 0.8f, 0.8f); // Random nice color

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString($"CustomBlock_{name}", json);
        
        AddToRegistry(name);
        PlayerPrefs.Save();
    }

    public void LoadBlockToBuilder(string name)
    {
        BlockData data = GetBlockData(name);
        if (data == null) return;

        activeCells.Clear();
        foreach (var cell in data.cells) activeCells.Add(cell);
    }

    // Retrieves data for UI previews without altering active builder state
    public BlockData GetBlockData(string name)
    {
        string json = PlayerPrefs.GetString($"CustomBlock_{name}");
        if (string.IsNullOrEmpty(json)) return null;

        // CRITICAL FIX: Need a new instance for deserialization, otherwise it might overwrite an asset if it was loaded.
        // We also rely on JsonUtility.FromJsonOverwrite to correctly set the ScriptableObject fields.
        BlockData data = ScriptableObject.CreateInstance<BlockData>();
        JsonUtility.FromJsonOverwrite(json, data);
        return data;
    }

    public void DeleteBlock(string name)
    {
        PlayerPrefs.DeleteKey($"CustomBlock_{name}");
        RemoveFromRegistry(name);
        PlayerPrefs.Save();
    }

    // Registry Management
    private void AddToRegistry(string name)
    {
        List<string> reg = GetRegistry();
        if (!reg.Contains(name))
        {
            reg.Add(name);
            PlayerPrefs.SetString(REGISTRY_KEY, string.Join(";", reg));
        }
    }

    private void RemoveFromRegistry(string name)
    {
        List<string> reg = GetRegistry();
        if (reg.Contains(name))
        {
            reg.Remove(name);
            PlayerPrefs.SetString(REGISTRY_KEY, string.Join(";", reg));
        }
    }

    public List<string> GetRegistry()
    {
        string raw = PlayerPrefs.GetString(REGISTRY_KEY, "");
        if (string.IsNullOrEmpty(raw)) return new List<string>();
        // Only return non-empty strings, for robustness against double separators
        return raw.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries).ToList(); 
    }
}