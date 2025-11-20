using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameHUD : MonoBehaviour
{
    [Header("HUD Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI nextBlockText; // Placeholder for now
    public Button pauseButton;
    public GameObject pauseMenuPanel;
    public Button resumeButton;
    public Button optionsButton;
    public Button quitButton;

    [Header("Game Over")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public Button restartButton;
    public Button menuButton;
    public TMP_InputField nameInputField;
    public Button submitScoreButton;

    [Header("Hold & Next")]
    public Image holdImage;
    public List<Image> nextImages; // Assign 3 images in Inspector

    private int currentScore = 0;

    private void Start()
    {
        if (pauseButton != null) pauseButton.onClick.AddListener(PauseGame);
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (optionsButton != null) optionsButton.onClick.AddListener(() => GameManager.Instance.LoadScene("OptionsScene"));
        if (quitButton != null) quitButton.onClick.AddListener(() => GameManager.Instance.LoadScene("MainMenu"));
        
        if (restartButton != null) restartButton.onClick.AddListener(() => GameManager.Instance.LoadScene("GameScene"));
        if (menuButton != null) menuButton.onClick.AddListener(() => GameManager.Instance.LoadScene("MainMenu"));
        if (submitScoreButton != null) submitScoreButton.onClick.AddListener(SubmitScore);
        
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

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

        UpdateScore(currentScore); // Refresh score text
        
        if (nextBlockText != null) nextBlockText.text = LocalizationManager.Instance.GetText("next");
        
        SetButtonText(resumeButton, "resume");
        SetButtonText(optionsButton, "options");
        SetButtonText(quitButton, "main_menu");
        
        SetButtonText(restartButton, "restart");
        SetButtonText(menuButton, "main_menu");
        SetButtonText(submitScoreButton, "submit");
        
        // Update Game Over Title if accessible, or other static texts
        // Since we don't have direct references to titles (Paused, Game Over), we might miss them.
        // But buttons and score are covered.
    }

    private void SetButtonText(Button btn, string key)
    {
        if (btn != null)
        {
            TextMeshProUGUI txt = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = LocalizationManager.Instance.GetText(key);
        }
    }

    public void UpdateScore(int score)
    {
        currentScore = score;
        if (scoreText != null && LocalizationManager.Instance != null)
        {
            scoreText.text = $"{LocalizationManager.Instance.GetText("score")}: {score}";
        }
    }

    public void ShowGameOver(int score)
    {
        GameManager.Instance.SetState(GameManager.GameState.GameOver);
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (finalScoreText != null && LocalizationManager.Instance != null)
            {
                finalScoreText.text = $"{LocalizationManager.Instance.GetText("final_score")}{score}";
            }
        }
    }

    private void SubmitScore()
    {
        string playerName = nameInputField.text;
        if (string.IsNullOrEmpty(playerName)) playerName = "Player";
        
        // We need to get the score from somewhere. 
        // Ideally, GameHUD should track it or get it from Board.
        // For now, let's assume we can access it via the text or pass it in.
        // A better way is to have a reference to the Board or store score locally.
        // Let's parse it from the text for this quick implementation or fix Board to pass it.
        // Actually, let's just grab it from the Board instance if possible.
        Board board = FindObjectOfType<Board>();
        if (board != null)
        {
            ScoreBoard.Instance.AddScore(playerName, board.score);
        }
        
        GameManager.Instance.LoadScene("MainMenu");
    }

    public void UpdateHold(BlockData data)
    {
        if (holdImage == null) return;
        if (data == null)
        {
            holdImage.sprite = null;
            holdImage.color = Color.clear;
            return;
        }
        
        // Just show a simple square with color for now, or generate a preview sprite
        // Generating a preview sprite at runtime is complex. 
        // We will just set the color of the image to the block's color.
        holdImage.sprite = Resources.Load<Sprite>("Square");
        holdImage.color = data.color;
    }

    public void UpdateNext(System.Collections.Generic.List<BlockData> nextBlocks)
    {
        if (nextImages == null) return;

        for (int i = 0; i < nextImages.Count; i++)
        {
            if (i < nextBlocks.Count)
            {
                nextImages[i].sprite = Resources.Load<Sprite>("Square");
                nextImages[i].color = nextBlocks[i].color;
                nextImages[i].gameObject.SetActive(true);
            }
            else
            {
                nextImages[i].gameObject.SetActive(false);
            }
        }
    }

    private void PauseGame()
    {
        GameManager.Instance.SetState(GameManager.GameState.Paused);
        pauseMenuPanel.SetActive(true);
    }

    private void ResumeGame()
    {
        GameManager.Instance.SetState(GameManager.GameState.Playing);
        pauseMenuPanel.SetActive(false);
    }
}
