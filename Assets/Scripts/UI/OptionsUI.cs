using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsUI : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown languageDropdown;
    public Button clearScoresButton;
    public Button backButton;
    public GameObject warningPanel;
    public Button confirmClearButton;
    public Button cancelClearButton;

    private void Start()
    {
        // Initialize dropdowns
        clearScoresButton.onClick.AddListener(ShowWarning);
        confirmClearButton.onClick.AddListener(ClearScores);
        cancelClearButton.onClick.AddListener(HideWarning);
        backButton.onClick.AddListener(() => GameManager.Instance.LoadScene("MainMenu"));
        
        languageDropdown.onValueChanged.AddListener(SetLanguage);
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    private void ShowWarning()
    {
        warningPanel.SetActive(true);
    }

    private void HideWarning()
    {
        warningPanel.SetActive(false);
    }

    private void ClearScores()
    {
        ScoreBoard.Instance.ClearScores();
        HideWarning();
    }

    private void SetLanguage(int index)
    {
        // Implement localization logic
        Debug.Log($"Language set to {index}");
    }

    private void SetResolution(int index)
    {
        // Implement resolution logic
        Debug.Log($"Resolution set to {index}");
    }
}
