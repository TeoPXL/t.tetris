using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class ScoreBoard : MonoBehaviour
{
    public static ScoreBoard Instance { get; private set; }
    public List<PlayerData> HighScores { get; private set; } = new List<PlayerData>();
    private string savePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            savePath = Path.Combine(Application.persistentDataPath, "highscores.json");
            LoadScores();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddScore(string name, int score)
    {
        HighScores.Add(new PlayerData(name, score));
        HighScores = HighScores.OrderByDescending(p => p.score).Take(10).ToList();
        SaveScores();
    }

    public void ClearScores()
    {
        HighScores.Clear();
        SaveScores();
    }

    private void SaveScores()
    {
        string json = JsonUtility.ToJson(new ScoreListWrapper { scores = HighScores });
        File.WriteAllText(savePath, json);
    }

    private void LoadScores()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            ScoreListWrapper wrapper = JsonUtility.FromJson<ScoreListWrapper>(json);
            if (wrapper != null)
            {
                HighScores = wrapper.scores;
            }
        }
    }

    [System.Serializable]
    private class ScoreListWrapper
    {
        public List<PlayerData> scores;
    }
}
