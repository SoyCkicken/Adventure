using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameEndingManager : MonoBehaviour
{
    public TMP_Text scoreText;

    void Start()
    {
        string path = Application.persistentDataPath + "/save.json";
        if (!System.IO.File.Exists(path)) return;

        string json = System.IO.File.ReadAllText(path);
        SaveManager.SaveData data = JsonUtility.FromJson<SaveManager.SaveData>(json);

        int statScore, levelScore, expScore;
        int totalScore = CalculateScore(data, out statScore, out levelScore, out expScore);
        scoreText.text =
       $"스탯 점수 : {statScore}\n" +
       $"레벨 점수 : {levelScore}\n" +
       $"보유 재화 점수 : {expScore}\n\n" +
       $"최종 점수 : {totalScore}";
    }

    int CalculateScore(SaveManager.SaveData data, out int statScore, out int levelScore, out int expScore)
    {
        statScore = data.STR + data.AGI + data.INT + data.MAG + data.CHA + data.Health;
        levelScore = data.Level * 10;
        expScore = data.Experience / 10;

        return statScore + levelScore + expScore;
    }
}
