using UnityEngine;
using System.Collections.Generic;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    public enum Language { English, Dutch }
    public Language CurrentLanguage { get; private set; } = Language.English;

    private Dictionary<string, string> englishTexts = new Dictionary<string, string>();
    private Dictionary<string, string> dutchTexts = new Dictionary<string, string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadTexts();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public event System.Action OnLanguageChanged;

    public void SetLanguage(Language lang)
    {
        CurrentLanguage = lang;
        OnLanguageChanged?.Invoke();
    }

    public string GetText(string key)
    {
        if (CurrentLanguage == Language.English)
        {
            return englishTexts.ContainsKey(key) ? englishTexts[key] : key;
        }
        else
        {
            return dutchTexts.ContainsKey(key) ? dutchTexts[key] : key;
        }
    }

    private void LoadTexts()
    {
        // Mock data for now
        englishTexts["play"] = "Play";
        englishTexts["options"] = "Options";
        englishTexts["build"] = "Build";
        englishTexts["exit"] = "Exit";
        englishTexts["resolution"] = "Resolution";
        englishTexts["language"] = "Language";
        englishTexts["clear_scores"] = "Clear Scores";
        englishTexts["back"] = "Back";
        englishTexts["confirm_clear"] = "Are you sure you want to clear high scores?";
        englishTexts["yes"] = "Yes";
        englishTexts["no"] = "No";
        englishTexts["resume"] = "Resume";
        englishTexts["main_menu"] = "Main Menu";
        englishTexts["score"] = "Score";
        englishTexts["next"] = "Next";
        englishTexts["game_over"] = "GAME OVER";
        englishTexts["final_score"] = "Final Score: ";
        englishTexts["restart"] = "Restart";
        englishTexts["submit"] = "Submit";
        englishTexts["paused"] = "PAUSED";
        englishTexts["save"] = "Save";
        englishTexts["reset"] = "Reset";
        englishTexts["saved_blocks"] = "Saved Blocks";
        englishTexts["block_builder"] = "BLOCK BUILDER";
        englishTexts["leaderboard_title"] = "LEADERBOARD";
        
        dutchTexts["play"] = "Spelen";
        dutchTexts["options"] = "Opties";
        dutchTexts["build"] = "Bouwen";
        dutchTexts["exit"] = "Afsluiten";
        dutchTexts["resolution"] = "Resolutie";
        dutchTexts["language"] = "Taal";
        dutchTexts["clear_scores"] = "Scores Wissen";
        dutchTexts["back"] = "Terug";
        dutchTexts["confirm_clear"] = "Weet je zeker dat je de scores wilt wissen?";
        dutchTexts["yes"] = "Ja";
        dutchTexts["no"] = "Nee";
        dutchTexts["resume"] = "Hervatten";
        dutchTexts["main_menu"] = "Hoofdmenu";
        dutchTexts["score"] = "Score";
        dutchTexts["next"] = "Volgende";
        dutchTexts["game_over"] = "SPEL VOORBIJ";
        dutchTexts["final_score"] = "Eindscore: ";
        dutchTexts["restart"] = "Opnieuw";
        dutchTexts["submit"] = "Indienen";
        dutchTexts["paused"] = "GEPAUZEERD";
        dutchTexts["save"] = "Opslaan";
        dutchTexts["reset"] = "Resetten";
        dutchTexts["saved_blocks"] = "Opgeslagen Blokken";
        dutchTexts["block_builder"] = "BLOKKENBOUWER";
        dutchTexts["leaderboard_title"] = "KLASSEMENT";
    }
}
