using System.Collections.Generic;
using Building;
using UnityEngine;

namespace Core
{
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

            // --- 2. Test Localization (UPDATED FOR SCRIPTABLEOBJECT SYSTEM) ---
            // Note: Ensure LocalizationManager exists and has language data assigned
            if (LocalizationManager.Instance != null)
            {
                // Test switching to second language (if available)
                if (LocalizationManager.Instance.LanguageCount > 1)
                {
                    LocalizationManager.Instance.SetLanguageByIndex(1); // Switch to second language
                    Debug.Log($"Switched to language: {LocalizationManager.Instance.CurrentLanguageName}");
                }
                
                // Test getting a translation
                string playText = LocalizationManager.Instance.GetText("play");
                if (!string.IsNullOrEmpty(playText))
                {
                    Debug.Log($"Localization Test Passed - 'play' = '{playText}'");
                }
                else
                {
                    Debug.LogWarning("Localization Test Warning - 'play' key returned empty");
                }
                
                // Switch back to first language
                if (LocalizationManager.Instance.LanguageCount > 0)
                {
                    LocalizationManager.Instance.SetLanguageByIndex(0);
                }
            }
            else
            {
                Debug.LogWarning("LocalizationManager instance not found - skipping localization test.");
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
}