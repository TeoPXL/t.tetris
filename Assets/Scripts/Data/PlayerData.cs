using System;

[Serializable]
public class PlayerData
{
    public string playerName;
    public int score;
    public string dateCompleted;

    public PlayerData(string name, int score)
    {
        this.playerName = name;
        this.score = score;
        this.dateCompleted = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
    }
}
