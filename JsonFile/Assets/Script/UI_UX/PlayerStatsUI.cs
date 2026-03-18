using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEngine;


public class PlayerStatsUI : MonoBehaviour
{
    [SerializeField]
    private PlayerState playerState;
    [Header("위쪽 플레이어 스탯 값")]
    public TMP_Text STRTEXT;
    public TMP_Text AGITEXT, DIVTEXT, INTTEXT, MAGTEXT, CHATEXT, HEALTHTEXT;
    [Header("임시값")]
    public int tempPoint;
    public int tempSTR, tempAGI, tempDIV, tempINT, tempMAG, tempCHA, tempHealth;
    public int Point { get; private set; }

    [Header("UI 요소 및 연결 객체")]
    public GameObject PlayerStatePanel;
    public TMP_Text UISTRText, UIAGIText, UIDIVText, UIINTText, UIMAGText, UICHAText, UIHealthText, POINTTEXT;
    public GameObject CloseButton;
    public InventoryManager InventoryManager;
    public IntegerHPBarScaler integerHPBarScaler;
    public IntegerHPBarScaler integerMPBarScaler;


    private void Start()
    {
        playerState = PlayerState.Instance;
        PlayerStatePanel.SetActive(false);
        playerState.RecalculateHPMP();
        UpdateUI();
        SaveTempStats();
    }

    private void Update()
    {
        integerHPBarScaler.SetCurrent(playerState.CurrentHealth);
        integerMPBarScaler.SetCurrent(playerState.CurrentMental);
        CloseButton.SetActive(Point == 0);
    }

    public void ShowPanel()
    {
        PlayerStatePanel.SetActive(true);
        UpdateUI();
    }

    public void HidePanel()
    {
        PlayerStatePanel.SetActive(false);
    }

    public void UpdateUI()
    {
        STRTEXT.text = playerState.STR.ToString(); UISTRText.text = playerState.STR.ToString();
        AGITEXT.text = playerState.AGI.ToString(); UIAGIText.text = playerState.AGI.ToString();
        INTTEXT.text = playerState.INT.ToString(); UIINTText.text = playerState.INT.ToString();
        MAGTEXT.text = playerState.MAG.ToString(); UIMAGText.text = playerState.MAG.ToString();
        CHATEXT.text = playerState.CHA.ToString(); UICHAText.text = playerState.CHA.ToString();
        HEALTHTEXT.text = playerState.Health.ToString(); UIHealthText.text = playerState.Health.ToString();
        //DIVText.text = playerState.DIV.ToString(); DIVTextUI.text = playerState.DIV.ToString();

        POINTTEXT.text = Point.ToString();
        integerHPBarScaler.SetHPMax(playerState.HP);
        integerMPBarScaler.SetMPMax(playerState.MP);
        //갱신 한번 더 해주고
        InventoryManager.UpdateDPS_MaxHealth();
        InventoryManager.UpdateInventoryByStrength();
        InventoryManager.UpdateGoldText();
    }

    public bool TryLevelUp()
    {
        if (playerState.Experience < playerState.ExperienceRequired) return false;

        playerState.Experience -= playerState.ExperienceRequired;
        playerState.ExperienceRequired = Mathf.CeilToInt(playerState.ExperienceRequired * 1.2f);
        Point += 3;
        tempPoint = Point;
        SaveTempStats();
        return true;
    }


    private void SaveTempStats()
    {
        tempSTR = playerState.STR; tempAGI = playerState.AGI; tempDIV = playerState.DIV;
        tempINT = playerState.INT; tempMAG = playerState.MAG; tempCHA = playerState.CHA; tempHealth = playerState.Health;
    }
    public void AddStat(ref int stat)
    {
        if (Point <= 0) return;
        stat++;
        Point--;
        UpdateUI();
    }

    public void AddSTR() => AddStat(ref playerState.STR);
    public void AddAGI() => AddStat(ref playerState.AGI);
    public void AddCHA() => AddStat(ref playerState.CHA);

    public void AddINT()
    {
        AddStat(ref playerState.INT);
        playerState.MP = playerState.CalculateMental(playerState.INT);
        UpdateUI();
    }

    public void AddMAG()
    {
        AddStat(ref playerState.MAG);
        playerState.DIV = playerState.MAG; // 마법력이 신성력도 결정
    }

    public void AddHealth()
    {
        AddStat(ref playerState.Health);
        playerState.HP = playerState.CalculateMental(playerState.Health);
        //Debug.Log($"HP = {HP}");
        //Debug.Log($"Health = {Health}");
        UpdateUI();
    }

    public void ResetStats()
    {
        Point = tempPoint;
        playerState.STR = tempSTR; playerState.AGI = tempAGI; playerState.DIV = tempDIV;
        playerState.INT = tempINT; playerState.MAG = tempMAG; playerState.CHA = tempCHA; playerState.Health = tempHealth;
        playerState.RecalculateHPMP();
    }
    public void ApplyStats()
    {
        tempPoint = 0;
        SaveTempStats();
        playerState.inventoryManager.UpdateDPS_MaxHealth();
        playerState.inventoryManager.UpdateInventoryByStrength();
        playerState.equipmentSystem.Init();

    }



    public void ResetUI()
    {
        ResetStats();
        UpdateUI();
    }

    public void ApplyUI()
    {
        ApplyStats();
        HidePanel();
    }

    public void OnClickLevelUp()
    {
        ShowPanel();
        if (TryLevelUp())
        {
            UpdateUI();
            InventoryManager.UpdateDPS_MaxHealth();
            InventoryManager.UpdateInventoryByStrength();
            InventoryManager.UpdateGoldText();
        }
    }
}
