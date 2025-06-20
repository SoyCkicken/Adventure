using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using MyGame;
using TMPro;
using static UnityEditor.Progress;
using Unity.VisualScripting;

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
    public GameObject merchantDetailPanel;
    [Header("패널에 들어가 있는 부속품들")]
    public TMP_Text MerchantItem_Name;
    public TMP_Text MerchantItem_Decription;
    public TMP_Text MerchantItem_Type;
    public TMP_Text MerchantItem_State;
    public TMP_Text MerchantItem_Option;
    public Button MerchantItem_ClearButton;
    public Button MerchantItem_BuyButton;


    public TMP_Text goldText;

    private List<BlackSmith> allItems;
    private List<BlackSmith> shopItems;

    private Dictionary<BlackSmith, GameObject> itemButtons = new();

    void Start()
    {
        MerchantItem_ClearButton.onClick.AddListener(() => { merchantDetailPanel.gameObject.SetActive(false); });
        merchantDetailPanel.SetActive(false);
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
            itemButtons[bs] = go;
        }
    }

    void OnClickMerchantItem(BlackSmith bs)
    {
        Debug.Log("정보창 출력 부분");
        var weaponMasters = jsonManager.GetWeaponMasters("Weapon_Master");
        var armorMasters = jsonManager.GetArmorMasters("Armor_Master");
        merchantDetailPanel.SetActive(true);

        if (bs.Item_Type == "Weapon")
        {
            Debug.Log("무기입니다.");
            var weapon = weaponMasters.FirstOrDefault(w => w.Weapon_ID == bs.Item_ID);
            MerchantItem_Name.text = weapon?.Weapon_Name;
            MerchantItem_Decription.text = weapon?.Description;
            MerchantItem_Type.text = "무기";
        }
        else if (bs.Item_Type == "Armor")
        {
            Debug.Log("방어구입니다.");
            var armor = armorMasters.FirstOrDefault(a => a.Armor_ID == bs.Item_ID);
            MerchantItem_Name.text = armor?.Armor_NAME;
            MerchantItem_Decription.text = armor?.Description;
            MerchantItem_Type.text = "방어구";
        }
        //else
        //{
        //    MerchantItem_Name.text = bs.Item_ID;
        //    MerchantItem_Type.text = "소비아이템";
        //}
        //Debug.Log("무기입니다.");
        MerchantItem_State.text = GetStatText(ConvertToItemData(bs));
        MerchantItem_Option.text = GetOptionText(ConvertToItemData(bs));

        MerchantItem_BuyButton.gameObject.SetActive(true);
        MerchantItem_BuyButton.onClick.AddListener(() =>
        {
            ConfirmPopup.Show(
                $"[{bs.Weapon_Name}] 을(를) {bs.Item_Price:0.##} 골드에 구매하시겠습니까?", () =>
                {
                    TryBuy(bs);

                }
            );
        });
       
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
        //shopItems.Remove(bs);

        //if (itemButtons.TryGetValue(bs, out var go))
        //{
        //    Destroy(go);
        //    itemButtons.Remove(bs);
        //}
        var slotUI = itemButtons[bs].GetComponent<MerchantSlotUI>();
        slotUI.MarkSold();
        merchantDetailPanel.SetActive(false);
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
            Item_Type = bs.Item_Type,
            Item_Name = bs.Weapon_Name,
            Item_Price = bs.Item_Price,
            //Heal_Value = bs.Heal_Value,
            //Mental_Heal_Value = bs.Mental_Heal_Value
            // 필요한 경우 추가 필드(Heal_Value 등)도 채워 주세요.
        };
    }

    string GetStatText(ItemData item)
    {
        var weaponMasters = jsonManager.GetWeaponMasters("Weapon_Master");
        var armorMasters = jsonManager.GetArmorMasters("Armor_Master");
        if (item.Item_Type == "Weapon")
        {
            var weapon = weaponMasters.FirstOrDefault(w => w.Weapon_ID == item.Item_ID);
            return $"공격력: {weapon?.Weapon_DMG}";
        }
        else if (item.Item_Type == "Armor")
        {
            var armor = armorMasters.FirstOrDefault(a => a.Armor_ID == item.Item_ID);
            return $"방어력: {armor?.Armor_DEF}, 체력: {armor?.Armor_HP}";
        }
        else if (item.Item_Type == "Consumable")
        {
            List<string> effects = new();
            if (item.Heal_Value > 0) effects.Add($"체력 회복: {item.Heal_Value}");
            if (item.Mental_Heal_Value > 0) effects.Add($"정신력 회복: {item.Mental_Heal_Value}");
            return string.Join(", ", effects);
        }
        return "";
    }

    string GetOptionText(ItemData item)
    {
        List<string> options = new();
        var weaponMasters = jsonManager.GetWeaponMasters("Weapon_Master");
        var armorMasters = jsonManager.GetArmorMasters("Armor_Master");

        string id1 = "", id2 = "";
        int val1 = 0, val2 = 0;

        if (item.Item_Type == "Weapon")
        {
            var weapon = weaponMasters.FirstOrDefault(w => w.Weapon_ID == item.Item_ID);
            id1 = weapon?.Option_1_ID;
            val1 = weapon?.Option_Value1 ?? 0;
            id2 = weapon?.Option_2_ID;
            val2 = weapon?.Option_Value2 ?? 0;
        }
        else if (item.Item_Type == "Armor")
        {
            var armor = armorMasters.FirstOrDefault(a => a.Armor_ID == item.Item_ID);
            id1 = armor?.Armor_Option1;
            val1 = armor?.Option1_Value ?? 0;
            id2 = armor?.Armor_Option2;
            val2 = armor?.Option2_Value ?? 0;
        }
        //else
        //{
        //    id1 = item.Option_1_ID;
        //    val1 = item.Option_Value1;
        //    id2 = item.Option_2_ID;
        //    val2 = item.Option_Value2;
        //}

        if (!string.IsNullOrEmpty(id1) && id1 != "null")
        {
            string desc = OptionManager.GetOptionDescription(id1);
            if (!string.IsNullOrEmpty(desc))
                options.Add($"{desc} +{val1}");
        }

        if (!string.IsNullOrEmpty(id2) && id2 != "null")
        {
            string desc = OptionManager.GetOptionDescription(id2);
            if (!string.IsNullOrEmpty(desc))
                options.Add($"{desc} +{val2}");
        }

        return string.Join("\n", options);
    }
}
