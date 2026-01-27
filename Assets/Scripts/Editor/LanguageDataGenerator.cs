using Data;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Editor tool to generate language data files with pre-filled translations.
    /// Access via: Tetris → Generate Language Files
    /// </summary>
    public class LanguageDataGenerator : EditorWindow
    {
        [MenuItem("Tetris/Generate Language Files")]
        public static void GenerateLanguageFiles()
        {
            if (!EditorUtility.DisplayDialog("Generate Language Files",
                    "This will create English and Dutch language data files in Assets/Resources/Languages/.\n\nContinue?",
                    "Yes", "No"))
            {
                return;
            }

            // Ensure directory exists
            string languagesPath = "Assets/Resources/Languages";
            if (!System.IO.Directory.Exists(languagesPath))
            {
                System.IO.Directory.CreateDirectory(languagesPath);
            }

            // Create English language data
            CreateEnglishLanguage(languagesPath);

            // Create Dutch language data
            CreateDutchLanguage(languagesPath);

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success",
                "Language files created successfully!\n\n" +
                "Location: Assets/Resources/Languages/\n\n" +
                "Next steps:\n" +
                "1. Find the LocalizationManager in your scenes\n" +
                "2. Assign the language files to 'Available Languages'\n" +
                "3. You can edit translations directly in the Inspector!",
                "OK");
        }

        private static void CreateEnglishLanguage(string path)
        {
            string assetPath = $"{path}/English.asset";

            // Check if already exists
            LanguageData existing = AssetDatabase.LoadAssetAtPath<LanguageData>(assetPath);
            if (existing != null)
            {
                Debug.Log($"English language file already exists at {assetPath}. Skipping.");
                return;
            }

            LanguageData english = CreateInstance<LanguageData>();
            english.languageName = "English";
            english.languageCode = "en";

            // Add all translations
            english.translations.Add(new TranslationEntry { key = "play", value = "Play" });
            english.translations.Add(new TranslationEntry { key = "options", value = "Options" });
            english.translations.Add(new TranslationEntry { key = "build", value = "Build" });
            english.translations.Add(new TranslationEntry { key = "exit", value = "Exit" });
            english.translations.Add(new TranslationEntry { key = "resolution", value = "Resolution" });
            english.translations.Add(new TranslationEntry { key = "language", value = "Language" });
            english.translations.Add(new TranslationEntry { key = "clear_scores", value = "Clear Scores" });
            english.translations.Add(new TranslationEntry { key = "back", value = "Back" });
            english.translations.Add(new TranslationEntry
                { key = "confirm_clear", value = "Are you sure you want to clear high scores?" });
            english.translations.Add(new TranslationEntry { key = "yes", value = "Yes" });
            english.translations.Add(new TranslationEntry { key = "no", value = "No" });
            english.translations.Add(new TranslationEntry { key = "resume", value = "Resume" });
            english.translations.Add(new TranslationEntry { key = "main_menu", value = "Main Menu" });
            english.translations.Add(new TranslationEntry { key = "score", value = "Score" });
            english.translations.Add(new TranslationEntry { key = "next", value = "Next" });
            english.translations.Add(new TranslationEntry { key = "game_over", value = "GAME OVER" });
            english.translations.Add(new TranslationEntry { key = "final_score", value = "Final Score: " });
            english.translations.Add(new TranslationEntry { key = "restart", value = "Restart" });
            english.translations.Add(new TranslationEntry { key = "submit", value = "Submit" });
            english.translations.Add(new TranslationEntry { key = "paused", value = "PAUSED" });
            english.translations.Add(new TranslationEntry { key = "save", value = "Save" });
            english.translations.Add(new TranslationEntry { key = "reset", value = "Reset" });
            english.translations.Add(new TranslationEntry { key = "saved_blocks", value = "Saved Blocks" });
            english.translations.Add(new TranslationEntry { key = "block_builder", value = "BLOCK BUILDER" });
            english.translations.Add(new TranslationEntry { key = "scoreboard", value = "High Scores" });
            english.translations.Add(new TranslationEntry { key = "try_again", value = "Try Again" });
            english.translations.Add(new TranslationEntry { key = "hold", value = "HOLD" });
            english.translations.Add(new TranslationEntry { key = "controls", value = "CONTROLS" });
            english.translations.Add(new TranslationEntry { key = "volume", value = "Volume" });
            english.translations.Add(new TranslationEntry { key = "saved_feedback", value = "Saved!" });

            AssetDatabase.CreateAsset(english, assetPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"Created English language file at {assetPath}");
        }

        private static void CreateDutchLanguage(string path)
        {
            string assetPath = $"{path}/Dutch.asset";

            // Check if already exists
            LanguageData existing = AssetDatabase.LoadAssetAtPath<LanguageData>(assetPath);
            if (existing != null)
            {
                Debug.Log($"Dutch language file already exists at {assetPath}. Skipping.");
                return;
            }

            LanguageData dutch = CreateInstance<LanguageData>();
            dutch.languageName = "Nederlands";
            dutch.languageCode = "nl";

            // Add all translations
            dutch.translations.Add(new TranslationEntry { key = "play", value = "Spelen" });
            dutch.translations.Add(new TranslationEntry { key = "options", value = "Opties" });
            dutch.translations.Add(new TranslationEntry { key = "build", value = "Bouwen" });
            dutch.translations.Add(new TranslationEntry { key = "exit", value = "Afsluiten" });
            dutch.translations.Add(new TranslationEntry { key = "resolution", value = "Resolutie" });
            dutch.translations.Add(new TranslationEntry { key = "language", value = "Taal" });
            dutch.translations.Add(new TranslationEntry { key = "clear_scores", value = "Scores Wissen" });
            dutch.translations.Add(new TranslationEntry { key = "back", value = "Terug" });
            dutch.translations.Add(new TranslationEntry
                { key = "confirm_clear", value = "Weet je zeker dat je de scores wilt wissen?" });
            dutch.translations.Add(new TranslationEntry { key = "yes", value = "Ja" });
            dutch.translations.Add(new TranslationEntry { key = "no", value = "Nee" });
            dutch.translations.Add(new TranslationEntry { key = "resume", value = "Hervatten" });
            dutch.translations.Add(new TranslationEntry { key = "main_menu", value = "Hoofdmenu" });
            dutch.translations.Add(new TranslationEntry { key = "score", value = "Score" });
            dutch.translations.Add(new TranslationEntry { key = "next", value = "Volgende" });
            dutch.translations.Add(new TranslationEntry { key = "game_over", value = "SPEL VOORBIJ" });
            dutch.translations.Add(new TranslationEntry { key = "final_score", value = "Eindscore: " });
            dutch.translations.Add(new TranslationEntry { key = "restart", value = "Opnieuw" });
            dutch.translations.Add(new TranslationEntry { key = "submit", value = "Indienen" });
            dutch.translations.Add(new TranslationEntry { key = "paused", value = "GEPAUZEERD" });
            dutch.translations.Add(new TranslationEntry { key = "save", value = "Opslaan" });
            dutch.translations.Add(new TranslationEntry { key = "reset", value = "Resetten" });
            dutch.translations.Add(new TranslationEntry { key = "saved_blocks", value = "Opgeslagen Blokken" });
            dutch.translations.Add(new TranslationEntry { key = "block_builder", value = "BLOKKENBOUWER" });
            dutch.translations.Add(new TranslationEntry { key = "scoreboard", value = "Topscores" });
            dutch.translations.Add(new TranslationEntry { key = "try_again", value = "Opnieuw Proberen" });
            dutch.translations.Add(new TranslationEntry { key = "hold", value = "VASTHOUDEN" });
            dutch.translations.Add(new TranslationEntry { key = "controls", value = "BESTURING" });
            dutch.translations.Add(new TranslationEntry { key = "volume", value = "Volume" });
            dutch.translations.Add(new TranslationEntry { key = "saved_feedback", value = "Opgeslagen!" });

            AssetDatabase.CreateAsset(dutch, assetPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"Created Dutch language file at {assetPath}");
        }
    }
}