using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using static UnityEditor.Timeline.TimelinePlaybackControls;

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

    [Header("UI 요소 및 연결 객체")]
    public GameObject PlayerStatePanel;
    public TMP_Text UISTRText, UIAGIText, UIDIVText, UIINTText, UIMAGText, UICHAText, UIHealthText, POINTTEXT;
    public GameObject CloseButton;
    public InventoryManager InventoryManager;
    public IntegerHPBarScaler integerHPBarScaler;
    public IntegerHPBarScaler integerMPBarScaler;
    private void Start()
    {
        PlayerStatePanel.SetActive(false);
        playerState.RecalculateHPMP();
        UpdateUI();
    }

    private void Update()
    {
        integerHPBarScaler.SetCurrent(playerState.CurrentHealth);
        integerMPBarScaler.SetCurrent(playerState.CurrentMental);
        CloseButton.SetActive(playerState.Point == 0);
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

        POINTTEXT.text = playerState.Point.ToString();
        integerHPBarScaler.SetHPMax(playerState.HP);
        integerMPBarScaler.SetMPMax(playerState.MP);
        //갱신 한번 더 해주고
        InventoryManager.updateDPS_MaxHealth();
        InventoryManager.UpdateInventoryByStrength();
        InventoryManager.updateSoulText();
    }

    public void ResetUI()
    {
        playerState.ResetStats();
        UpdateUI();
    }

    public void ApplyUI()
    {
        playerState.ApplyStats();
        HidePanel();
    }

    public void OnClickLevelUp()
    {
        ShowPanel();
        if (playerState.TryLevelUp())
        {
            UpdateUI();
            InventoryManager.updateDPS_MaxHealth();
            InventoryManager.UpdateInventoryByStrength();
            InventoryManager.updateSoulText();
        }
    }
}
