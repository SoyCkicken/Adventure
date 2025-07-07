// PlayerState.cs (Refactored for clarity, structure, and readability)

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class PlayerState : MonoBehaviour
{
    [Header("플레이어 능력치")]
    public int STR = 5, AGI = 5, DIV = 5, MAG = 5, CHA = 5;
    public int Health = 5, INT = 5;
    public int HP { get; private set; }
    public int MP { get; private set; }
    public int CurrentHealth = 0, CurrentMental = 0;

    public int Level = 1;
    public int Experience = 100000;
    private int ExperienceRequired = 100;
    public int Point { get; private set; }

    private int tempPoint;
    private int tempSTR, tempAGI, tempDIV, tempINT, tempMAG, tempCHA, tempHealth;

    private void Awake()
    {
        RecalculateHPMP();
        SaveTempStats();
    }

    public bool TryLevelUp()
    {
        if (Experience < ExperienceRequired) return false;

        Experience -= ExperienceRequired;
        ExperienceRequired = Mathf.CeilToInt(ExperienceRequired * 1.2f);
        Point += 3;
        tempPoint = Point;
        SaveTempStats();
        return true;
    }

    public void AddStat(ref int stat)
    {
        if (Point <= 0) return;
        stat++;
        Point--;
        SaveTempStats();
    }

    public void AddSTR() => AddStat(ref STR);
    public void AddAGI() => AddStat(ref AGI);
    public void AddCHA() => AddStat(ref CHA);

    public void AddINT()
    {
        AddStat(ref INT);
        MP = CalculateMental(INT);
    }

    public void AddMAG()
    {
        AddStat(ref MAG);
        DIV = MAG; // 마법력이 신성력도 결정
    }

    public void AddHealth()
    {
        AddStat(ref Health);
        HP = CalculateHealth(Health);
    }

    public void ResetStats()
    {
        Point = tempPoint;
        STR = tempSTR; AGI = tempAGI; DIV = tempDIV;
        INT = tempINT; MAG = tempMAG; CHA = tempCHA; Health = tempHealth;
        RecalculateHPMP();
    }
    public void ApplyStats()
    {
        tempPoint = 0;
        SaveTempStats();
    }


    private void SaveTempStats()
    {
        tempSTR = STR; tempAGI = AGI; tempDIV = DIV;
        tempINT = INT; tempMAG = MAG; tempCHA = CHA; tempHealth = Health;
    }

    public void RecalculateHPMP()
    {
        HP = CalculateHealth(Health);
        MP = CalculateMental(INT);
        CurrentHealth = HP;
        CurrentMental = MP;
    }

    private int CalculateHealth(int value) => value >= 15 ? 5 : Mathf.Max(value / 3, 3);
    private int CalculateMental(int value) => value >= 15 ? 5 : Mathf.Max(value / 3, 3);
}
