using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayData
{
    // Todos estos campos deben ser SerializeField (o public) para que puedan ser serializados por JSON
    [SerializeField] private string gamePlayDataID; // Este ID es el que va a parar como llave del PlayerPrefs
    [SerializeField] private int score;
    [SerializeField] private int hiScore;    
    [SerializeField] private int lives;
    [SerializeField] private int nextScoreToExtendLife;

    public GameplayData(string gamePlayDataID, int initialHiScore)
    {
        this.gamePlayDataID = gamePlayDataID;        
        hiScore = initialHiScore;
    }

    public string GamePlayDataID => gamePlayDataID;

    public int Score { 
        get { return score; } 
        set 
        { 
            score = value;
            if (score > hiScore)
                hiScore = score;
        } 
    }

    public int Lives { get { return lives; } set { lives = value; } }
    public int NextScoreToExtendLife { get { return nextScoreToExtendLife; } set { nextScoreToExtendLife = value; } }
    public int HiScore => hiScore;        
}

public static class GameDataRepository
{    
    private const int INITIAL_HISCORE = 150;

    public static GameplayData GetById(string GamePlayDataID)
    {
        if (PlayerPrefs.HasKey(GamePlayDataID))
        {
            var jsonData = PlayerPrefs.GetString(GamePlayDataID);
            GameplayData gameData = JsonUtility.FromJson<GameplayData>(jsonData);
            return gameData;
        }
        else
        {
            return new GameplayData(GamePlayDataID, INITIAL_HISCORE);
        }
    }


    public static void Update(GameplayData gameData)
    {
        var jsonData = JsonUtility.ToJson(gameData);
        PlayerPrefs.SetString(gameData.GamePlayDataID, jsonData);
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }

}
