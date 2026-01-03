using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    public Button playButton;
    public Button optionsButton;
    public Button buildButton;
    public Button exitButton;
    public Button highScoresButton;

    private void Start()
    {
        playButton.onClick.AddListener(() => GameManager.Instance.LoadScene("GameScene"));
        optionsButton.onClick.AddListener(() => GameManager.Instance.LoadScene("OptionsScene"));
        buildButton.onClick.AddListener(() => GameManager.Instance.LoadScene("BuildingScene"));
        highScoresButton.onClick.AddListener(() => GameManager.Instance.LoadScene("ScoreboardScene"));
        exitButton.onClick.AddListener(() => Application.Quit());
        
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged += UpdateUITexts;
            UpdateUITexts();
        }
    }

    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= UpdateUITexts;
        }
    }

    private void UpdateUITexts()
    {
        if (LocalizationManager.Instance == null) return;
        
        SetButtonText(playButton, "play");
        SetButtonText(optionsButton, "options");
        SetButtonText(buildButton, "build");
        SetButtonText(highScoresButton, "Scoreboard");
        SetButtonText(exitButton, "exit");
    }

    private void SetButtonText(Button btn, string key)
    {
        if (btn != null)
        {
            TextMeshProUGUI txt = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = LocalizationManager.Instance.GetText(key);
        }
    }
}
