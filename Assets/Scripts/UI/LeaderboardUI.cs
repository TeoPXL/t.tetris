using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LeaderboardUI : MonoBehaviour
{
    public TextMeshProUGUI[] scoreEntries; // Array of 10 text fields
    public Button backButton;
    public TextMeshProUGUI titleText;

    private void Start()
    {
        Debug.Log("LeaderboardUI Start called.");
        
        if (backButton != null) 
        {
            backButton.onClick.AddListener(() => 
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.LoadScene("MainMenu");
                else
                    Debug.LogError("GameManager.Instance is null!");
            });
        }
        else Debug.LogError("BackButton is not assigned in LeaderboardUI!");

        DisplayScores();
        UpdateUITexts();
    }

    private void DisplayScores()
    {
        if (ScoreBoard.Instance == null)
        {
            Debug.LogError("ScoreBoard.Instance is null!");
            return;
        }

        List<PlayerData> scores = ScoreBoard.Instance.HighScores;

        for (int i = 0; i < scoreEntries.Length; i++)
        {
            if (scoreEntries[i] != null)
            {
                if (i < scores.Count)
                {
                    scoreEntries[i].text = $"{i + 1}. {scores[i].playerName}: {scores[i].score}";
                }
                else
                {
                    scoreEntries[i].text = $"{i + 1}. ---";
                }
            }
            else
            {
                Debug.LogError($"Score entry {i} is not assigned!");
            }
        }
    }

    private void UpdateUITexts()
    {
        if (LocalizationManager.Instance == null)
        {
            Debug.LogError("LocalizationManager.Instance is null!");
            return;
        }

        if (titleText != null)
            titleText.text = LocalizationManager.Instance.GetText("leaderboard_title");

        SetButtonText(backButton, "back");
    }

    private void SetButtonText(Button btn, string key)
    {
        if (btn != null)
        {
            TextMeshProUGUI txt = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
                txt.text = LocalizationManager.Instance.GetText(key);
        }
    }
}