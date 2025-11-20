using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI nextBlockText; // Placeholder for now
    public Button pauseButton;
    public GameObject pauseMenuPanel;
    public Button resumeButton;
    public Button optionsButton;
    public Button quitButton;

    private void Start()
    {
        pauseButton.onClick.AddListener(PauseGame);
        resumeButton.onClick.AddListener(ResumeGame);
        optionsButton.onClick.AddListener(() => GameManager.Instance.LoadScene("OptionsScene"));
        quitButton.onClick.AddListener(() => GameManager.Instance.LoadScene("MainMenu"));
    }

    public void UpdateScore(int score)
    {
        scoreText.text = $"Score: {score}";
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
