using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
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
    public GameObject Merchant_Invantory;
    public GameObject MerchantSlotPrefab;
    public GameObject MerchantDetailPanel;
    [Header("패널에 들어가 있는 부속품들")]
    public TMP_Text MerchantItem_Name;
    public TMP_Text MerchantItem_Decription;
    public TMP_Text MerchantItem_Type;
    public TMP_Text MerchantItem_State;
    public TMP_Text MerchantItem_Option;
    public Button MerchantItem_ClearButton;
    public Button MerchantItem_BuyButton;
    public Button MerchantItem_CloseButton;

    //상점 닫았다는것을 넘길려고 만든 액션함수
    public Action onCloseCallback;
    public TMP_Text goldText;

    private List<MerchantItem> allItems;
    private List<MerchantItem> shopItems;

    private Dictionary<MerchantItem, GameObject> itemButtons = new();

    void Start()
    {
        playerState = PlayerState.Instance;
        //패널 닫기
        MerchantItem_ClearButton.onClick.AddListener(() => { MerchantDetailPanel.gameObject.SetActive(false); });
        MerchantItem_CloseButton.onClick.AddListener(() => {
            Debug.Log("상점 닫기를 시도 했습니다");
            Merchant_Invantory.gameObject.SetActive(false);
            inventoryManager.inventoryPanel.SetActive(false);
            onCloseCallback?.Invoke();  // ⬅ 닫을 때 콜백 실행
        });


            // 1) JsonManager 에서 상인용 리스트 가져오기
            allItems = jsonManager.GetMerchantItems(merchantKey);
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
        MerchantDetailPanel.SetActive(false);
        Merchant_Invantory.SetActive(false);
    }

    void PopulateShop()
    {
        foreach (var bs in shopItems)
        {
            var go = Instantiate(MerchantSlotPrefab, itemGridParent);
            var slot = go.GetComponent<MerchantSlotUI>();
            slot.Setup(bs, OnClickMerchantItem);
            itemButtons[bs] = go;
        }
    }

    void OnClickMerchantItem(MerchantItem bs)
    {
        Debug.Log("정보창 출력 부분");
        var weaponMasters = jsonManager.GetWeaponMasters("Weapon_Master");
        var armorMasters = jsonManager.GetArmorMasters("Armor_Master");
        var itemMasters = jsonManager.GetItemMasters("Item_Master");
        MerchantDetailPanel.SetActive(true);

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
        else if (bs.Item_Type == "Consumable")
        {
            var Consumable = itemMasters.FirstOrDefault(i => i.Item_ID == bs.Item_ID);
            MerchantItem_Name.text = Consumable?.Item_NAME;
            MerchantItem_Decription.text = Consumable?.Item_Description;
            MerchantItem_Type.text = "소비 아이템";
        }
        else
        {
            var item = itemMasters.FirstOrDefault(i => i.Item_ID == bs.Item_ID);
            MerchantItem_Name.text = item?.Item_NAME;
            MerchantItem_Decription.text = item?.Item_Description;
            MerchantItem_Type.text = "일반 아이템";
        }
            //Debug.Log("무기입니다.");
            MerchantItem_State.text = GetStatText(ConvertToItemData(bs));
        MerchantItem_Option.text = GetOptionText(ConvertToItemData(bs));

        MerchantItem_BuyButton.gameObject.SetActive(true);
        MerchantItem_BuyButton.onClick.AddListener(() =>
        {
            ConfirmPopup.Show(
                $"[{bs.Item_Name}] 을(를) {bs.Item_Price:0.##} 골드에 구매하시겠습니까?", () =>
                {
                    TryBuy(bs);
                }
            );
        });
       
    }

    void TryBuy(MerchantItem bs)
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
        MerchantDetailPanel.SetActive(false);
        Debug.Log($"[{bs.Item_Name}] 구매 완료! 남은 골드: {playerState.Experience}");
    }

    void RefreshGoldUI()
    {
        if (goldText != null)
            goldText.text = $"Gold: {playerState.Experience:0}";
    }

    // BlackSmith → ItemData 로 변환
    ItemData ConvertToItemData(MerchantItem bs)
    {
        return new ItemData
        {
            Item_ID = bs.Item_ID,
            Item_Type = bs.Item_Type,
            Item_Name = bs.Item_Name,
            Item_Price = bs.Item_Price,
            //Heal_Value = bs.Heal_Value,
            //Mental_Heal_Value = bs.Mental_Heal_Value
            // 필요한 경우 추가 필드(Heal_Value 등)도 채워 주세요.
            Option_1_ID = bs.Item_Option_1,
            Option_Value1 = (int)bs.Item_Option_value1,
            Option_2_ID = bs.Item_Option_2,
            Option_Value2 = (int)bs.Item_Option_value2
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

    public void OpenShop(string merchantKey, System.Action onClose)
    {
        
        this.merchantKey = merchantKey;
        Debug.Log(merchantKey);
        Debug.Log(this.merchantKey);
        onCloseCallback = onClose;
        ClearShopUI(); // 기존 슬롯 제거
        LoadAndDisplayItems(merchantKey); // JsonManager에서 merchantKey 기준으로 아이템 로드
        gameObject.SetActive(true);
        Merchant_Invantory.SetActive(true);
        inventoryManager.inventoryPanel.SetActive(true);

    }

    void ClearShopUI()
    {
        foreach (Transform child in itemGridParent)
        {
            Destroy(child.gameObject);
        }

        // 상점 아이템 리스트 초기화
        shopItems?.Clear();

        // 슬롯 버튼 참조 초기화 (버튼 클릭 막기 등 관련)
        if (itemButtons != null)
            itemButtons.Clear();
    }

    void LoadAndDisplayItems(string merchantKey)
    {
        Debug.Log("여기까지 들어왔음!");
        allItems = jsonManager.GetMerchantItems(merchantKey);

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
        MerchantDetailPanel.SetActive(false);
        Merchant_Invantory.SetActive(false);
    }
}
