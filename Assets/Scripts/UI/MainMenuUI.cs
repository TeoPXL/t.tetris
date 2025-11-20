using UnityEngine;
using UnityEngine.UI;

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
        optionsButton.onClick.AddListener(() => GameManager.Instance.LoadScene("OptionsScene")); // Or open panel
        buildButton.onClick.AddListener(() => GameManager.Instance.LoadScene("BuildingScene"));
        exitButton.onClick.AddListener(() => Application.Quit());
        // highScoresButton.onClick.AddListener(() => ...);
    }
}
