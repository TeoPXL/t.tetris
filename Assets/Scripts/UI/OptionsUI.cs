using System.Collections.Generic;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class OptionsUI : MonoBehaviour
    {
        public TMP_Dropdown resolutionDropdown;
        public TMP_Dropdown languageDropdown;
        public Button clearScoresButton;
        public Button backButton;
        public GameObject warningPanel;
        public Button confirmClearButton;
        public Button cancelClearButton;

        public Slider volumeSlider;
        public TextMeshProUGUI volumeLabel;

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
                });
            }

            if (languageDropdown != null) languageDropdown.onValueChanged.AddListener(SetLanguage);
            if (resolutionDropdown != null) resolutionDropdown.onValueChanged.AddListener(SetResolution);

            if (volumeSlider != null)
            {
                volumeSlider.onValueChanged.AddListener(SetVolume);
                volumeSlider.value = AudioListener.volume; // Initialize with current volume
            }

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

            if (LocalizationManager.Instance != null)
            {
                // Get all available language names from the LocalizationManager
                List<string> languageNames = LocalizationManager.Instance.GetAllLanguageNames();
                languageDropdown.AddOptions(languageNames);

                // Set current language
                languageDropdown.value = LocalizationManager.Instance.CurrentLanguageIndex;
                languageDropdown.RefreshShownValue();
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
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.SetLanguageByIndex(index);
                UpdateUITexts();
            }
        }

        private void SetResolution(int index)
        {
            Resolution[] resolutions = Screen.resolutions;
            Resolution resolution = resolutions[index];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }

        private void SetVolume(float value)
        {
            AudioListener.volume = value;
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
            if (volumeLabel != null) volumeLabel.text = LocalizationManager.Instance.GetText("volume");

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
}