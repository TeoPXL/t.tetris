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

    public void SetLanguage(Language lang)
    {
        CurrentLanguage = lang;
        // Trigger UI update event
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
        
        dutchTexts["play"] = "Spelen";
        dutchTexts["options"] = "Opties";
        dutchTexts["build"] = "Bouwen";
        dutchTexts["exit"] = "Afsluiten";
    }
}
