// PlayerState.cs (Refactored for clarity, structure, and readability)

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerState : MonoBehaviour
{
    public static PlayerState Instance { get; private set; }

    [Header("스탯 관련 변수")]
    public int STR = 5, AGI = 5, DIV = 5, MAG = 5, CHA = 5;
    public int Health = 5, CurrentHealth = 0, HP = 0;
    public int INT = 5, CurrentMental = 0, MP = 0;
    public int Level = 1;
    public int Experience = 100000;

    [Header("레벨업 및 임시 저장용")]
    private int ExperienceRequired = 100;
    private int point;
    private int tempPoint, tempSTR, tempAGI, tempDIV, tempINT, tempMAG, tempCHA, tempHealth;

    [Header("UI 요소 및 연결 객체")]
    public GameObject PlayerStateObject;
    public TMP_Text StateSTRText, StateAGIText, StateDIVText, StateINTText, StateMAGText, StateCHAText, StateHealthText , StatePointText;
    public TMP_Text UISSTRText, UIDEXText, UIDIVText, UIINTText, UIMAGText, UICHAText, UIHealthText;
    public GameObject CloseButton;
    public InventoryManager InventoryManager;
    public JsonManager jsonManager;
    public List<GameObject> buttons;
    public IntegerHPBarScaler integerHPBarScaler;
    public IntegerHPBarScaler integerMPBarScaler;

    private void Awake()
    {
        if (transform.parent != null)
            transform.SetParent(null);

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        PlayerStateObject.SetActive(false);
        HP = CalculateStatHealth(Health);
        MP = CalculateStatMental(INT);
        CurrentHealth = HP;
        CurrentMental = MP;
        integerHPBarScaler.SetMax(HP);
        integerMPBarScaler.SetMax(MP);
        UpdateStateUI();
    }

    private void Update()
    {
        CurrentHealth = Mathf.Min(CurrentHealth, HP);
        CurrentMental = Mathf.Min(CurrentMental, MP);
        integerHPBarScaler.SetCurrent(CurrentHealth);
        integerMPBarScaler.SetCurrent(CurrentMental);
        if (point == 0)
        {
            CloseButton.SetActive(true);
        }
        else
        {
            CloseButton.SetActive(false);
        }
    }

    public void LevelUp()
    {
        if (Experience < ExperienceRequired) return;

        Experience -= ExperienceRequired;
        ExperienceRequired = Mathf.CeilToInt(ExperienceRequired * 1.2f);
        //레벨업 하면 UI 갱신
        InventoryManager.updateSoulText();
        point += 3;
        tempPoint = point;
        StatePointText.text = point.ToString();
        SaveTempStats();
        PlayerStateObject.SetActive(true);
        UpdateStateUI();
    }

    public void ResetPlayerState()
    {
        point = tempPoint;
        StatePointText.text = point.ToString();
        STR = tempSTR; AGI = tempAGI; DIV = tempDIV; INT = tempINT; MAG = tempMAG; CHA = tempCHA; Health = tempHealth;
        UpdateStateUI();
    }

    public void ClosePlayerState()
    {
        PlayerStateObject.SetActive(false);
        InventoryManager.updateDPS_MaxHealth();
        InventoryManager.UpdateInventoryByStrength();
        Debug.Log(MP);
        tempPoint = 0;
    }

    public void AddStrength() => AddStat(ref STR);
    public void AddDEX() => AddStat(ref AGI);
    public void AddCHR() => AddStat(ref CHA);

    public void AddINT()
    {
        AddStat(ref INT);
        MP = CalculateStatMental(INT);
    }

    public void AddMAG()
    {
        AddStat(ref MAG);
        DIV = MAG; // 신성은 마법력 기반
    }

    public void AddHealth()
    {
        AddStat(ref Health);
        HP = CalculateStatHealth(Health);
    }

    public void AddDivinity() => AddStat(ref DIV);

    private void AddStat(ref int stat)
    {
        if (point <= 0) return;
        stat++;
        point--;
        StatePointText.text = point.ToString();
        UpdateStateUI();
    }

    private int CalculateStatHealth(int value)
    {
        if (value >= 15) return 5;
        return Mathf.Max(value / 3, 3);
    }

    private int CalculateStatMental(int value)
    {
        if (value >= 15) return 5;
        return Mathf.Max(value / 3, 3);
    }

    private void SaveTempStats()
    {
        tempSTR = STR; tempAGI = AGI; tempDIV = DIV;
        tempINT = INT; tempMAG = MAG; tempCHA = CHA; tempHealth = Health;
    }

    private void UpdateStateUI()
    {
        StateSTRText.text = STR.ToString(); UISSTRText.text = STR.ToString();
        StateAGIText.text = AGI.ToString(); UIDEXText.text = AGI.ToString();
        //StateDIVText.text = DIV.ToString(); UIDIVText.text = DIV.ToString();
        StateINTText.text = INT.ToString(); UIINTText.text = INT.ToString();
        StateMAGText.text = MAG.ToString(); UIMAGText.text = MAG.ToString();
        StateCHAText.text = CHA.ToString(); UICHAText.text = CHA.ToString();
        StateHealthText.text = Health.ToString(); UIHealthText.text = Health.ToString();
        integerHPBarScaler.SetMax(HP);
        integerMPBarScaler.SetMax(MP);
    }
}
