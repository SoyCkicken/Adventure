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

    [SerializeField] public PlayerStatsUI statsUI;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private EquipmentSystem equipmentSystem;
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
        statsUI.UpdateUI();
    }

    public void AddSTR() => AddStat(ref STR);
    public void AddAGI() => AddStat(ref AGI);
    public void AddCHA() => AddStat(ref CHA);

    public void AddINT()
    {
        AddStat(ref INT);
        MP = CalculateMental(INT);
        statsUI.UpdateUI();
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
        //Debug.Log($"HP = {HP}");
        //Debug.Log($"Health = {Health}");
        statsUI.UpdateUI();
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
        inventoryManager.updateDPS_MaxHealth();
        inventoryManager.UpdateInventoryByStrength();
        equipmentSystem.Init();

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

    private int CalculateHealth(int value)
    {
        return value >= 15 ? 5 : Mathf.Max(value / 3, 3);
    }
    private int CalculateMental(int value)
    {
        return value >= 15 ? 5 : Mathf.Max(value / 3, 3);
    }

    public void SavePlayer()
    {
        SaveManager.SaveData data = new SaveManager.SaveData
        {
            //playerName = PlayerName,      //플레이어 이름을 쓸지 안쓸지 몰라서 일단 주석처리
            STR = STR,
            INT = INT,
            AGI = AGI,
            MAG = MAG,
            CHA = CHA,
            Health = Health,
            HP = HP,
            MP = MP,
            Experience = Experience,
            ExperienceRequired = ExperienceRequired,
            Level = Level,
        };
        inventoryManager.SaveInventory(data); // ✅ 인벤토리 저장 포함
        SaveManager.SaveGame(data);
    }

    public void LoadPlayer()
    {
        if (!SaveManager.HasSave()) return;

        SaveManager.SaveData data = SaveManager.LoadGame();
        //PlayerName = data.playerName;
        STR = data.STR;
        INT = data.INT;
        AGI = data.AGI;
        MAG = data.MAG;
        CHA = data.CHA;
        Health = data.Health;
        HP = data.HP;
        MP = data.MP;
        Level = data.Level;
        Experience = data.Experience;
        ExperienceRequired = data.ExperienceRequired;
        inventoryManager.LoadInventory(data); // ✅ 인벤토리 불러오기 포함
        statsUI.UpdateUI();
        equipmentSystem.Init();
    }
}
