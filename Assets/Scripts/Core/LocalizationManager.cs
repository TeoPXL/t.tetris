using System.Collections.Generic;
using Data;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Manages localization using ScriptableObject-based language data files.
    /// Add language files to the 'availableLanguages' list in the Inspector.
    /// </summary>
    public class LocalizationManager : MonoBehaviour
    {
        public static LocalizationManager Instance { get; private set; }

        [Header("Language Configuration")]
        [Tooltip("List of all available language data files. Add your LanguageData assets here.")]
        public List<LanguageData> availableLanguages = new List<LanguageData>();

        [Tooltip("Index of the default language to use on startup (0 = first in list)")]
        public int defaultLanguageIndex = 0;

        private LanguageData currentLanguageData;
        private int currentLanguageIndex = 0;

        public event System.Action OnLanguageChanged;

        /// <summary>
        /// Get the name of the current language
        /// </summary>
        public string CurrentLanguageName => currentLanguageData != null ? currentLanguageData.languageName : "Unknown";

        /// <summary>
        /// Get the current language index (for dropdown UI)
        /// </summary>
        public int CurrentLanguageIndex => currentLanguageIndex;

        /// <summary>
        /// Get count of available languages
        /// </summary>
        public int LanguageCount => availableLanguages.Count;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeLanguage();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeLanguage()
        {
            // Validate setup
            if (availableLanguages == null || availableLanguages.Count == 0)
            {
                Debug.LogError("[LocalizationManager] No language data files assigned! Please add LanguageData assets to the LocalizationManager.");
                return;
            }

            // Clamp default index
            if (defaultLanguageIndex < 0 || defaultLanguageIndex >= availableLanguages.Count)
            {
                Debug.LogWarning($"[LocalizationManager] Invalid default language index {defaultLanguageIndex}. Using 0.");
                defaultLanguageIndex = 0;
            }

            // Load default language
            SetLanguageByIndex(defaultLanguageIndex);
        }

        /// <summary>
        /// Change language by index (useful for dropdowns)
        /// </summary>
        public void SetLanguageByIndex(int index)
        {
            if (index < 0 || index >= availableLanguages.Count)
            {
                Debug.LogError($"[LocalizationManager] Invalid language index: {index}");
                return;
            }

            if (availableLanguages[index] == null)
            {
                Debug.LogError($"[LocalizationManager] Language data at index {index} is null!");
                return;
            }

            currentLanguageIndex = index;
            currentLanguageData = availableLanguages[index];

            Debug.Log($"[LocalizationManager] Language changed to: {currentLanguageData.languageName}");

            OnLanguageChanged?.Invoke();
        }

        /// <summary>
        /// Change language by language code (e.g., "en", "nl")
        /// </summary>
        public void SetLanguageByCode(string languageCode)
        {
            for (int i = 0; i < availableLanguages.Count; i++)
            {
                if (availableLanguages[i] != null && availableLanguages[i].languageCode == languageCode)
                {
                    SetLanguageByIndex(i);
                    return;
                }
            }

            Debug.LogWarning($"[LocalizationManager] Language code '{languageCode}' not found.");
        }

        /// <summary>
        /// Get translated text by key
        /// </summary>
        public string GetText(string key)
        {
            if (currentLanguageData == null)
            {
                Debug.LogError("[LocalizationManager] No language data loaded!");
                return key;
            }

            return currentLanguageData.GetText(key);
        }

        /// <summary>
        /// Get the name of a language by index (useful for populating dropdowns)
        /// </summary>
        public string GetLanguageName(int index)
        {
            if (index >= 0 && index < availableLanguages.Count && availableLanguages[index] != null)
            {
                return availableLanguages[index].languageName;
            }

            return "Unknown";
        }

        /// <summary>
        /// Get all language names (useful for populating dropdowns)
        /// </summary>
        public List<string> GetAllLanguageNames()
        {
            List<string> names = new List<string>();
            foreach (var lang in availableLanguages)
            {
                if (lang != null)
                {
                    names.Add(lang.languageName);
                }
                else
                {
                    names.Add("Missing");
                }
            }
            return names;
        }
    }
}