using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using MyGame;
using TMPro;

public class MerchantManager : MonoBehaviour
{
    [Header("데이터")]
    [Tooltip("JsonManager.GetBlackSmiths 에 넘길 키(파일명)")]
    public string merchantKey = "BlackSmith";
    public int displayCount = 10;
    public JsonManager jsonManager;
    public PlayerState playerState;
    public InventoryManager inventoryManager;

    [Header("UI")]
    public Transform itemGridParent;
    public GameObject merchantSlotPrefab;
    public TMP_Text goldText;

    private List<BlackSmith> allItems;
    private List<BlackSmith> shopItems;

    void Start()
    {
        // 1) JsonManager 에서 상인용 리스트 가져오기
        allItems = jsonManager.GetBlackSmiths(merchantKey);
        if (allItems == null || allItems.Count == 0)
        {
            Debug.LogError($"[{merchantKey}] 상인 아이템 로드 실패");
            return;
        }

        // 2) 무작위로 섞어서 displayCount 개만 추출
        shopItems = allItems
            .OrderBy(_ => Guid.NewGuid())
            .Take(displayCount)
            .ToList();

        PopulateShop();
        RefreshGoldUI();
    }

    void PopulateShop()
    {
        foreach (var bs in shopItems)
        {
            var go = Instantiate(merchantSlotPrefab, itemGridParent);
            var slot = go.GetComponent<MerchantSlotUI>();
            slot.Setup(bs, OnClickMerchantItem);
        }
    }

    void OnClickMerchantItem(BlackSmith bs)
    {
        ConfirmPopup.Show(
            $"[{bs.Weapon_Name}] 을(를) {bs.Item_Price:0.##} 골드에 구매하시겠습니까?", () =>
            {
                TryBuy(bs);
            }
        );
    }

    void TryBuy(BlackSmith bs)
    {
        if (playerState.Experience < bs.Item_Price)
        {
            Debug.Log("골드가 부족합니다.");
            return;
        }

        playerState.Experience -= bs.Item_Price;
        inventoryManager.AddItemToInventory(ConvertToItemData(bs));
        RefreshGoldUI();
        Debug.Log($"[{bs.Weapon_Name}] 구매 완료! 남은 골드: {playerState.Experience}");
    }

    void RefreshGoldUI()
    {
        if (goldText != null)
            goldText.text = $"Gold: {playerState.Experience:0}";
    }

    // BlackSmith → ItemData 로 변환
    ItemData ConvertToItemData(BlackSmith bs)
    {
        return new ItemData
        {
            Item_ID = bs.Item_ID,
            Item_Type = bs.ItemType,
            Item_Name = bs.Weapon_Name,
            // 필요한 경우 추가 필드(Heal_Value 등)도 채워 주세요.
        };
    }
}
