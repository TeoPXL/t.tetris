using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    /// <summary>
    /// ScriptableObject that holds all translations for a single language.
    /// Create instances via: Right-click → Create → Tetris → Language Data
    /// </summary>
    [CreateAssetMenu(fileName = "NewLanguage", menuName = "Tetris/Language Data", order = 1)]
    public class LanguageData : ScriptableObject
    {
        [Header("Language Info")] [Tooltip("Display name of this language (e.g., 'English', 'Nederlands')")]
        public string languageName = "English";

        [Tooltip("Two-letter language code (e.g., 'en', 'nl')")]
        public string languageCode = "en";

        [Header("Translations")] [Tooltip("List of all text translations for this language")]
        public List<TranslationEntry> translations = new List<TranslationEntry>();

        /// <summary>
        /// Get a translation by key. Returns the key itself if not found.
        /// </summary>
        public string GetText(string key)
        {
            foreach (var entry in translations)
            {
                if (entry.key == key)
                {
                    return entry.value;
                }
            }

            // Return the key itself if translation not found (helpful for debugging)
            Debug.LogWarning($"Translation key '{key}' not found in {languageName}");
            return key;
        }

        /// <summary>
        /// Check if a translation exists for a given key
        /// </summary>
        public bool HasKey(string key)
        {
            foreach (var entry in translations)
            {
                if (entry.key == key)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Add or update a translation entry (useful for editor tools)
        /// </summary>
        public void SetTranslation(string key, string value)
        {
            for (int i = 0; i < translations.Count; i++)
            {
                if (translations[i].key == key)
                {
                    translations[i].value = value;
                    return;
                }
            }

            // Key doesn't exist, add new entry
            translations.Add(new TranslationEntry { key = key, value = value });
        }
    }

    /// <summary>
    /// A single key-value pair for translations.
    /// Serializable so it appears in the Inspector.
    /// </summary>
    [Serializable]
    public class TranslationEntry
    {
        [Tooltip("The lookup key (e.g., 'play', 'options', 'game_over')")]
        public string key;

        [Tooltip("The translated text in this language")] [TextArea(1, 3)]
        public string value;
    }
}