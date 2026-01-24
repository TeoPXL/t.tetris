using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Menu, Playing, Paused, GameOver, Building }
    public GameState CurrentState { get; private set; }

    // Internal list to hold both Standard (Resource) and Custom (SaveSystem) blocks
    private List<BlockData> allBlocks = new List<BlockData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllBlocks(); // Load blocks immediately on startup
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- MISSING METHOD FIXED HERE ---
    public void LoadAllBlocks()
    {
        allBlocks.Clear();
        
        // 1. Load Default Resource Blocks (Ensure you ran Setup Project tools!)
        var defaults = Resources.LoadAll<BlockData>("Blocks");
        if(defaults != null)
        {
            allBlocks.AddRange(defaults);
        }

        // 2. Load Custom Blocks from PlayerPrefs
        string registryRaw = PlayerPrefs.GetString("BLOCK_REGISTRY", "");
        if (!string.IsNullOrEmpty(registryRaw))
        {
            string[] names = registryRaw.Split(';');
            foreach (string name in names)
            {
                string json = PlayerPrefs.GetString($"CustomBlock_{name}");
                if (!string.IsNullOrEmpty(json))
                {
                    BlockData custom = ScriptableObject.CreateInstance<BlockData>();
                    JsonUtility.FromJsonOverwrite(json, custom);
                    allBlocks.Add(custom);
                }
            }
        }
        
        Debug.Log($"GameManager loaded {allBlocks.Count} blocks.");
    }

    // --- MISSING METHOD FIXED HERE ---
    public BlockData GetRandomBlock()
    {
        // If we are just starting the game, refresh list to ensure we have the latest custom blocks
        if (allBlocks.Count == 0) LoadAllBlocks();
        
        if (allBlocks.Count == 0)
        {
            Debug.LogError("No blocks found! Did you run 'Tetris/Setup Project'?");
            return null;
        }
        
        return allBlocks[Random.Range(0, allBlocks.Count)];
    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;
        Time.timeScale = (newState == GameState.Paused) ? 0f : 1f;
    }

    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f; // Ensure time is running when changing scenes
        SceneManager.LoadScene(sceneName);
        
        if (sceneName == "GameScene") 
        {
            SetState(GameState.Playing);
            LoadAllBlocks(); // Refresh blocks when entering game
        }
        else 
        {
            SetState(GameState.Menu);
        }
    }
}