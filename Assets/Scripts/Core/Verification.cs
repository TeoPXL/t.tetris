using UnityEngine;
using System.Collections.Generic;

public class Verification : MonoBehaviour
{
    private void Start()
    {
        RunTests();
    }

    private void RunTests()
    {
        Debug.Log("Starting Verification...");

        // --- 1. Test ScoreBoard (Assumed unchanged) ---
        // Note: Ensure ScoreBoard exists in your project, or comment this out.
        if (ScoreBoard.Instance != null)
        {
            ScoreBoard.Instance.ClearScores();
            ScoreBoard.Instance.AddScore("TestPlayer", 1000);
            if (ScoreBoard.Instance.HighScores.Count > 0 && ScoreBoard.Instance.HighScores[0].score == 1000)
            {
                Debug.Log("ScoreBoard Test Passed");
            }
            else
            {
                Debug.LogError("ScoreBoard Test Failed");
            }
        }
        else
        {
            Debug.LogWarning("ScoreBoard instance not found - skipping test.");
        }

        // --- 2. Test Localization (Assumed unchanged) ---
        // Note: Ensure LocalizationManager exists, or comment this out.
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.SetLanguage(LocalizationManager.Language.Dutch);
            // Assuming 'play' key exists
            if (!string.IsNullOrEmpty(LocalizationManager.Instance.GetText("play"))) 
            {
                Debug.Log("Localization Test Passed");
            }
        }

        // --- 3. Test BlockBuilder (UPDATED FOR NEW LOGIC) ---
        GameObject builderObj = new GameObject("BuilderTester");
        BlockBuilder builder = builderObj.AddComponent<BlockBuilder>();
        
        // A. Setup the builder state (Simulate UI Clicks)
        builder.ResetBuilder(); // Adds (0,0) automatically
        builder.AddCell(Vector2Int.up); // Adds (0,1)
        
        string testBlockName = "VerifyTestBlock";

        // B. Test Save
        // The new SaveBlock takes a name and saves the CURRENT builder state
        builder.SaveBlock(testBlockName);
        
        // Check Registry
        List<string> registry = builder.GetRegistry();
        if (registry.Contains(testBlockName))
        {
            Debug.Log("BlockBuilder Registry Update Passed");
        }
        else
        {
            Debug.LogError("BlockBuilder Registry Update Failed");
        }

        // C. Test Load
        // Reset builder to default (just 0,0)
        builder.ResetBuilder();
        
        // Load the saved block back into the builder state
        builder.LoadBlockToBuilder(testBlockName);
        
        List<Vector2Int> loadedCells = builder.GetActiveCells();

        // We expect 2 cells: (0,0) and (0,1)
        if (loadedCells.Count == 2 && loadedCells.Contains(Vector2Int.up))
        {
            Debug.Log("BlockBuilder Save/Load Logic Passed");
        }
        else
        {
            Debug.LogError($"BlockBuilder Save/Load Failed. Count: {loadedCells.Count}");
        }

        // D. Test Delete
        builder.DeleteBlock(testBlockName);
        
        if (!builder.GetRegistry().Contains(testBlockName) && !PlayerPrefs.HasKey($"CustomBlock_{testBlockName}"))
        {
            Debug.Log("BlockBuilder Delete Test Passed");
        }
        else
        {
            Debug.LogError("BlockBuilder Delete Test Failed");
        }

        // Cleanup
        Destroy(builderObj);

        Debug.Log("Verification Complete.");
    }
}