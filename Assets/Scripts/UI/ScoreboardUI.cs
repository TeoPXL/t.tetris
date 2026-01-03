using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreboardUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The container where score rows will be instantiated (e.g., Content of a ScrollView).")]
    [SerializeField] private Transform scoreContainer;
    [Tooltip("The prefab for a single score row.")]
    [SerializeField] private GameObject scoreRowPrefab;
    [Tooltip("The button to return to the main menu.")]
    [SerializeField] private Button backButton;

    private void Start()
    {
        UpdateScoreboard();

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }
    }

    private void UpdateScoreboard()
    {
        if (scoreContainer == null || scoreRowPrefab == null)
        {
            Debug.LogError("ScoreboardUI: Missing references to ScoreContainer or ScoreRowPrefab.");
            return;
        }

        foreach (Transform child in scoreContainer)
        {
            Destroy(child.gameObject);
        }

        if (ScoreBoard.Instance == null)
        {
            Debug.LogWarning("ScoreboardUI: ScoreBoard instance not found. High scores cannot be loaded. Start the game from the MainMenu to initialize the ScoreBoard.");
            return;
        }

        var scores = ScoreBoard.Instance.HighScores ?? new System.Collections.Generic.List<PlayerData>();

        foreach (var scoreData in scores)
        {
            CreateScoreRow(scoreData);
        }

        // Force layout update to fix overlapping issues
        if (scoreContainer.GetComponent<RectTransform>() != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(scoreContainer.GetComponent<RectTransform>());
        }
    }

    private void CreateScoreRow(PlayerData data)
    {
        GameObject row = Instantiate(scoreRowPrefab, scoreContainer);
        
        // Ensure the row has a LayoutElement so the VerticalLayoutGroup knows how to size it
        LayoutElement layoutElement = row.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = row.AddComponent<LayoutElement>();
            layoutElement.minHeight = 50f; // Default height if not set
            layoutElement.preferredHeight = 50f;
        }

        TextMeshProUGUI[] texts = row.GetComponentsInChildren<TextMeshProUGUI>();

        if (texts.Length >= 2)
        {
            texts[0].text = data.playerName;
            texts[0].color = Color.white; // Ensure text is visible
            
            texts[1].text = data.score.ToString();
            texts[1].color = Color.white; // Ensure text is visible
            
            if (texts.Length > 2)
            {
                texts[2].text = data.dateCompleted;
                texts[2].color = Color.white; // Ensure text is visible
            }
        }
        else
        {
            Debug.LogWarning("ScoreboardUI: row prefab does not have enough TextMeshProUGUI components.");
        }
    }

    private void OnBackButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadScene("MainMenu");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
}
