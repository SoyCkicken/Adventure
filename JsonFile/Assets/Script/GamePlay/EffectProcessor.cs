using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 이벤트 및 스토리에서 공용으로 사용하는 효과 처리 유틸리티 클래스.
/// </summary>
public static class EffectProcessor
{
    /// <summary>
    /// 효과를 처리하고 텍스트 블록을 생성함.
    /// </summary>
    /// <param name="effects">EffectTrigger 리스트</param>
    /// <param name="playerState">플레이어 상태 참조</param>
    /// <param name="inventoryManager">인벤토리 매니저</param>
    /// <param name="jsonManager">JsonManager 참조</param>
    /// <param name="fontSizeManager">FontSizeManager 참조</param>
    /// <param name="content">텍스트 블록 생성 위치</param>
    /// <param name="textPrefab">텍스트 프리팹</param>
    /// <param name="sceneCode">현재 시나리오 코드</param>
    /// <param name="textBlockList">생성된 텍스트 블록 리스트(optional)</param>
    /// <returns>생성된 블록 수</returns>
    public static int ApplyEffects(
        List<Main_Effect> effects,
        PlayerState playerState,
        InventoryManager inventoryManager,
        JsonManager jsonManager,
        FontSizeManager fontSizeManager,
        Transform content,
        GameObject textPrefab,
        string sceneCode,
        List<GameObject> textBlockList = null)
    {
        int createdBlocks = 0;

        foreach (var effect in effects)
        {
            switch (effect.ID)
            {
                case "Effect_001": // 골드/소울 증감
                    {
                        int delta = effect.Value;
                        if (delta < 0)
                        {
                            int available = playerState.Experience;
                            if (available + delta < 0)
                                delta = -available;
                        }

                        playerState.Experience += delta;
                        inventoryManager.UpdateGoldText();

                        if (textBlockList != null)
                        {
                            var go = Object.Instantiate(textPrefab, content);
                            TMP_Text tmp = go.GetComponentInChildren<TMP_Text>();
                            fontSizeManager.Register(tmp);
                            textBlockList.Add(go);
                            tmp.text = delta >= 0
                                ? $"<color=#00ff00>+{delta}</color>\n"
                                : $"<color=#ff0000>{delta}</color>\n";
                            createdBlocks++;
                        }
                    }
                    break;

                case "Effect_002": // 체력 감소
                    {
                        int loss = Mathf.Abs(effect.Value);
                        playerState.CurrentHealth = Mathf.Max(0, playerState.CurrentHealth - loss);
                    }
                    break;

                case "Effect_003": // 아이템 지급
                    {
                        var item = jsonManager.GetItemDataFromCode(effect.Code);
                        if (item != null)
                        {
                            if (sceneCode == "MainScript_1_3_5" || sceneCode == "MainScript_1_3_6" || sceneCode == "MainScript_1_3_7")
                            {
                                inventoryManager.selectedItem = item;
                                inventoryManager.OnClickEquip();
                            }
                            else
                            {
                                inventoryManager.AddItemToInventory(item);
                            }

                            if (textBlockList != null)
                            {
                                var go = Object.Instantiate(textPrefab, content);
                                TMP_Text tmp = go.GetComponentInChildren<TMP_Text>();
                                fontSizeManager.Register(tmp);
                                textBlockList.Add(go);
                                tmp.text = $"<color=#00ff00>+ {item.Item_Name}을 획득했습니다</color>\n";
                                createdBlocks++;
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"[이펙트 실패] 잘못된 아이템 코드: {effect.Code}");
                        }
                    }
                    break;

                default:
                    Debug.LogWarning($"[이펙트 실패] 알 수 없는 이펙트 ID: {effect.ID}");
                    break;
            }
        }

        return createdBlocks;
    }
}