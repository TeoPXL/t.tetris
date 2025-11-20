using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

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
        Debug.Log("OptionsUI Start called.");
        
        // Initialize dropdowns
        if (clearScoresButton != null) clearScoresButton.onClick.AddListener(ShowWarning);
        else Debug.LogError("ClearScoresButton is not assigned in OptionsUI!");

        if (confirmClearButton != null) confirmClearButton.onClick.AddListener(ClearScores);
        if (cancelClearButton != null) cancelClearButton.onClick.AddListener(HideWarning);
        
        if (backButton != null) 
        {
            backButton.onClick.AddListener(() => 
            {
                Debug.Log("Back Button Clicked");
                if (GameManager.Instance != null) GameManager.Instance.LoadScene("MainMenu");
                else Debug.LogError("GameManager Instance is null!");
            });
        }
        else Debug.LogError("BackButton is not assigned in OptionsUI!");
        
        if (languageDropdown != null) languageDropdown.onValueChanged.AddListener(SetLanguage);
        else Debug.LogError("LanguageDropdown is not assigned in OptionsUI!");

        if (resolutionDropdown != null) resolutionDropdown.onValueChanged.AddListener(SetResolution);
        else Debug.LogError("ResolutionDropdown is not assigned in OptionsUI!");

        InitializeResolutionDropdown();
        InitializeLanguageDropdown();
        UpdateUITexts();
    }

    private void InitializeResolutionDropdown()
    {
        if (resolutionDropdown == null) return;

        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        Resolution[] resolutions = Screen.resolutions;
        Debug.Log($"Found {resolutions.Length} resolutions.");
        
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    private void InitializeLanguageDropdown()
    {
        if (languageDropdown == null) return;

        languageDropdown.ClearOptions();
        List<string> options = new List<string> { "English", "Dutch" };
        languageDropdown.AddOptions(options);
        
        if (LocalizationManager.Instance != null)
        {
            languageDropdown.value = (int)LocalizationManager.Instance.CurrentLanguage;
        }
        else
        {
            Debug.LogError("LocalizationManager Instance is null!");
        }
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
        LocalizationManager.Instance.SetLanguage((LocalizationManager.Language)index);
        UpdateUITexts();
    }

    private void SetResolution(int index)
    {
        Resolution[] resolutions = Screen.resolutions;
        Resolution resolution = resolutions[index];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public TextMeshProUGUI resolutionLabel;
    public TextMeshProUGUI languageLabel;
    public TextMeshProUGUI warningText;

    private void UpdateUITexts()
    {
        if (LocalizationManager.Instance == null) return;

        if (resolutionLabel != null) resolutionLabel.text = LocalizationManager.Instance.GetText("resolution");
        if (languageLabel != null) languageLabel.text = LocalizationManager.Instance.GetText("language");
        if (warningText != null) warningText.text = LocalizationManager.Instance.GetText("confirm_clear");

        SetButtonText(clearScoresButton, "clear_scores");
        SetButtonText(backButton, "back");
        SetButtonText(confirmClearButton, "yes");
        SetButtonText(cancelClearButton, "no");
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
