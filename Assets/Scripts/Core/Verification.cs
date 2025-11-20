using UnityEngine;

public class Verification : MonoBehaviour
{
    private void Start()
    {
        RunTests();
    }

    private void RunTests()
    {
        Debug.Log("Starting Verification...");

        // Test ScoreBoard
        ScoreBoard.Instance.ClearScores();
        ScoreBoard.Instance.AddScore("TestPlayer", 1000);
        if (ScoreBoard.Instance.HighScores.Count == 1 && ScoreBoard.Instance.HighScores[0].score == 1000)
        {
            Debug.Log("ScoreBoard Test Passed");
        }
        else
        {
            Debug.LogError("ScoreBoard Test Failed");
        }

        // Test Localization
        LocalizationManager.Instance.SetLanguage(LocalizationManager.Language.Dutch);
        if (LocalizationManager.Instance.GetText("play") == "Spelen")
        {
            Debug.Log("Localization Test Passed");
        }
        else
        {
            Debug.LogError("Localization Test Failed");
        }

        // Test BlockBuilder
        BlockBuilder builder = new GameObject("Builder").AddComponent<BlockBuilder>();
        BlockData data = ScriptableObject.CreateInstance<BlockData>();
        data.name = "TestBlock";
        data.cells = new Vector2Int[] { Vector2Int.zero, Vector2Int.up };
        builder.SaveBlock(data);
        BlockData loaded = builder.LoadBlock("TestBlock");
        if (loaded != null && loaded.cells.Length == 2)
        {
            Debug.Log("BlockBuilder Save/Load Test Passed");
        }
        else
        {
            Debug.LogError("BlockBuilder Save/Load Test Failed");
        }
        builder.DeleteBlock("TestBlock");
        if (builder.LoadBlock("TestBlock") == null)
        {
            Debug.Log("BlockBuilder Delete Test Passed");
        }
        else
        {
            Debug.LogError("BlockBuilder Delete Test Failed");
        }
        Destroy(builder.gameObject);

        Debug.Log("Verification Complete.");
    }
}
