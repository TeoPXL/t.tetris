using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameHUD : MonoBehaviour
{
    [Header("References")]
    public GameObject cellPrefab; // Needs to be assigned by SetupTools

    [Header("HUD Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI levelText; // Optional polish
    
    [Header("Panels")]
    public GameObject pauseMenuPanel;
    public GameObject gameOverPanel;

    [Header("Buttons")]
    public Button pauseButton;
    public Button resumeButton;
    public Button optionsButton;
    public Button quitButton;
    
    [Header("Game Over Elements")]
    public TextMeshProUGUI finalScoreText;
    public Button restartButton;
    public Button menuButton;
    public TMP_InputField nameInputField;
    public Button submitScoreButton;

    [Header("Hold & Next Containers")]
    public RectTransform holdContainer;
    public List<RectTransform> nextContainers; 

    private int currentScore = 0;

    [Header("Audio")]
    public AudioClip gameMusic;
    private AudioSource audioSource;

    private void Start()
    {
        // Setup Music
        if (gameMusic != null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.clip = gameMusic;
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            
            if (!audioSource.isPlaying) 
            {
                audioSource.Play();
            }
        }

        // Button Listeners
        if (pauseButton) pauseButton.onClick.AddListener(PauseGame);
        if (resumeButton) resumeButton.onClick.AddListener(ResumeGame);
        if (optionsButton) optionsButton.onClick.AddListener(() => GameManager.Instance.LoadScene("OptionsScene"));
        if (quitButton) quitButton.onClick.AddListener(() => GameManager.Instance.LoadScene("MainMenu"));
        if (restartButton) restartButton.onClick.AddListener(() => GameManager.Instance.LoadScene("GameScene"));
        if (menuButton) menuButton.onClick.AddListener(() => GameManager.Instance.LoadScene("MainMenu"));
        if (submitScoreButton) submitScoreButton.onClick.AddListener(SubmitScore);
        
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (pauseMenuPanel) pauseMenuPanel.SetActive(false);

        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged += UpdateUITexts;
            UpdateUITexts();
        }
    }

    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= UpdateUITexts;
    }

    private void UpdateUITexts()
    {
        if (LocalizationManager.Instance == null) return;
        UpdateScore(currentScore);
        // Update static button texts here if localization keys exist
    }

    public void UpdateScore(int score)
    {
        currentScore = score;
        if (scoreText != null) scoreText.text = $"{score}";
    }

    public void ShowGameOver(int score)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (finalScoreText != null) finalScoreText.text = $"Final Score: {score}";
        }
    }

    private void SubmitScore()
    {
        // 1. Safety Check: Is the UI Input Field linked?
        if (nameInputField == null)
        {
            Debug.LogError("GameHUD: 'nameInputField' is not assigned in the Inspector!");
            return;
        }

        string playerName = nameInputField.text;
        if (string.IsNullOrEmpty(playerName)) playerName = "Player";

        // 2. Safety Check: Does the ScoreBoard exist?
        if (ScoreBoard.Instance == null)
        {
            Debug.LogError("GameHUD: ScoreBoard.Instance is NULL. The ScoreBoard GameObject is missing from the scene.");
            // Force return to menu so the player isn't stuck
            if (GameManager.Instance != null) GameManager.Instance.LoadScene("MainMenu");
            return;
        }

        // Safe to execute
        ScoreBoard.Instance.AddScore(playerName, currentScore);
        
        if (GameManager.Instance != null)
            GameManager.Instance.LoadScene("MainMenu");
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

    // --- PREVIEW LOGIC ---

    public void UpdateHold(BlockData data)
    {
        RenderBlockInContainer(holdContainer, data);
    }

    public void UpdateNext(List<BlockData> nextBlocks)
    {
        for (int i = 0; i < nextContainers.Count; i++)
        {
            if (i < nextBlocks.Count)
                RenderBlockInContainer(nextContainers[i], nextBlocks[i]);
            else
                RenderBlockInContainer(nextContainers[i], null);
        }
    }

    private void RenderBlockInContainer(RectTransform container, BlockData data)
    {
        if (container == null) return;

        // 1. Clear existing cells
        foreach (Transform child in container) Destroy(child.gameObject);

        if (data == null || data.cells == null) return;

        // 2. Calculate centering logic
        // We want to fit the block into the container (approx 80x80 or 100x100)
        float containerSize = container.rect.width;
        float cellSize = 20f; // Smaller cells for UI
        
        // Find bounds of the block
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        foreach (var cell in data.cells)
        {
            if (cell.x < minX) minX = cell.x;
            if (cell.x > maxX) maxX = cell.x;
            if (cell.y < minY) minY = cell.y;
            if (cell.y > maxY) maxY = cell.y;
        }

        float blockWidth = (maxX - minX + 1) * cellSize;
        float blockHeight = (maxY - minY + 1) * cellSize;

        // Calculate offset to center the block
        Vector2 centerOffset = new Vector2(
            (containerSize - blockWidth) / 2f - (minX * cellSize),
            (containerSize - blockHeight) / 2f - (minY * cellSize)
        );

        // 3. Instantiate Cells
        foreach (var cell in data.cells)
        {
            GameObject uiCell = Instantiate(cellPrefab, container);
            RectTransform rt = uiCell.GetComponent<RectTransform>();
            
            // Set size
            rt.sizeDelta = new Vector2(cellSize, cellSize);
            
            // Position (Anchor bottom-left of container)
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.zero;
            rt.pivot = Vector2.zero;
            
            float posX = centerOffset.x + (cell.x * cellSize);
            float posY = centerOffset.y + (cell.y * cellSize);
            
            rt.anchoredPosition = new Vector2(posX, posY);

            // Set Color
            Image img = uiCell.GetComponent<Image>();
            img.color = data.color;
        }
    }
}