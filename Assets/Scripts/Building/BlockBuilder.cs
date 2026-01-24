using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BlockBuilder : MonoBehaviour
{
    private HashSet<Vector2Int> activeCells = new HashSet<Vector2Int>();
    private const int MAX_BLOCKS = 6; // Kept the limit from previous request
    private const string REGISTRY_KEY = "BLOCK_REGISTRY";

    // Shared directions array for expansion and connectivity checks
    private readonly Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

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
        // We now check CanRemove before actually doing it
        if (CanRemove(pos))
        {
            activeCells.Remove(pos);
        }
    }

    // --- NEW: CONNECTIVITY LOGIC ---

    /// <summary>
    /// Checks if a cell can be removed without breaking the block or removing the anchor.
    /// </summary>
    public bool CanRemove(Vector2Int pos)
    {
        // 1. Cannot remove the anchor (0,0)
        if (pos == Vector2Int.zero) return false;

        // 2. Cannot remove if it's not there
        if (!activeCells.Contains(pos)) return false;

        // 3. Run Connectivity Check
        return IsConnectivityPreserved(pos);
    }

    private bool IsConnectivityPreserved(Vector2Int cellToRemove)
    {
        // Create a temporary set of what the block WOULD look like
        HashSet<Vector2Int> remaining = new HashSet<Vector2Int>(activeCells);
        remaining.Remove(cellToRemove);

        if (remaining.Count == 0) return true; // Should not happen due to anchor check, but safe fallback

        // --- FLOOD FILL ALGORITHM ---
        
        // 1. Pick an arbitrary start point (The anchor is always a safe bet)
        Vector2Int startNode = Vector2Int.zero; 
        
        // 2. Setup traversal
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        
        queue.Enqueue(startNode);
        visited.Add(startNode);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighbor = current + dir;
                
                // If the neighbor exists in our remaining structure and we haven't visited it yet
                if (remaining.Contains(neighbor) && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        // 4. If we visited every remaining cell, the structure is still whole
        return visited.Count == remaining.Count;
    }

    // -------------------------------

    public List<Vector2Int> GetActiveCells() { return activeCells.ToList(); }

    public List<Vector2Int> GetAvailableExpansions()
    {
        if (activeCells.Count >= MAX_BLOCKS) return new List<Vector2Int>();

        HashSet<Vector2Int> candidates = new HashSet<Vector2Int>();

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

    // --- SAVING SYSTEM (Unchanged) ---
    public void SaveBlock(string name)
    {
        if (string.IsNullOrEmpty(name) || activeCells.Count == 0) return;
        BlockData data = ScriptableObject.CreateInstance<BlockData>();
        data.cells = activeCells.ToArray();
        data.name = name;
        data.color = Color.HSVToRGB(Random.value, 0.8f, 0.8f); 
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

    public BlockData GetBlockData(string name)
    {
        string json = PlayerPrefs.GetString($"CustomBlock_{name}");
        if (string.IsNullOrEmpty(json)) return null;
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
        return raw.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries).ToList(); 
    }
}