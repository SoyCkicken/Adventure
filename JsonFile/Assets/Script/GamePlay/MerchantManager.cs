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
        jsonManager = JsonManager.Instance ?? FindObjectOfType<JsonManager>(true); // 수정
        inventoryManager = inventoryManager ?? FindObjectOfType<InventoryManager>(true);
        //패널 닫기
        MerchantItem_ClearButton?.onClick.AddListener(() => { MerchantDetailPanel?.gameObject.SetActive(false); });
        MerchantItem_CloseButton?.onClick.AddListener(() => {
            Debug.Log("상점 닫기를 시도 했습니다");
            Merchant_Invantory?.gameObject.SetActive(false);
            inventoryManager?.inventoryPanel?.SetActive(false);
            onCloseCallback?.Invoke();  // ⬅ 닫을 때 콜백 실행
        });


            // 1) JsonManager 에서 상인용 리스트 가져오기
        if (jsonManager == null)
        {
            Debug.LogWarning("[MerchantManager] JsonManager를 찾지 못해 상점 초기화를 건너뜁니다.");
            return;
        }
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
        ItemData itemData = ConvertToItemData(bs);
        if (itemData == null)
        {
            Debug.LogWarning($"[MerchantManager] 상품 데이터를 찾을 수 없습니다: {bs?.Item_ID}");
            return;
        }
        MerchantDetailPanel.SetActive(true);

        if (itemData.Item_Type == "Weapon")
        {
            Debug.Log("무기입니다.");
            MerchantItem_Name.text = itemData.Item_Name;
            MerchantItem_Decription.text = itemData.Description;
            MerchantItem_Type.text = "무기";
        }
        else if (itemData.Item_Type == "Armor")
        {
            Debug.Log("방어구입니다.");
            MerchantItem_Name.text = itemData.Item_Name;
            MerchantItem_Decription.text = itemData.Description;
            MerchantItem_Type.text = "방어구";
        }
        else if (itemData.Item_Type == "Consumable")
        {
            MerchantItem_Name.text = itemData.Item_Name;
            MerchantItem_Decription.text = itemData.Description;
            MerchantItem_Type.text = "소비 아이템";
        }
        else
        {
            MerchantItem_Name.text = itemData.Item_Name;
            MerchantItem_Decription.text = itemData.Description;
            MerchantItem_Type.text = "일반 아이템";
        }
            //Debug.Log("무기입니다.");
            MerchantItem_State.text = GetStatText(itemData);
        MerchantItem_Option.text = GetOptionText(itemData);

        MerchantItem_BuyButton.gameObject.SetActive(true);
        MerchantItem_BuyButton.onClick.RemoveAllListeners();
        MerchantItem_BuyButton.onClick.AddListener(() =>
        {
            ConfirmPopup.Show(
                $"[{bs.Item_Name}] 을(를) {(bs.Item_Price*5):0.##} 골드에 구매하시겠습니까?", () =>
                {
                    TryBuy(bs);
                }
            );
        });
       
    }

    void TryBuy(MerchantItem bs)
    {
        if (playerState.Experience < (bs.Item_Price*5))
        {
            Debug.Log("골드가 부족합니다.");
            return;
        }

        ItemData itemData = ConvertToItemData(bs);
        if (itemData == null)
        {
            Debug.LogWarning($"[MerchantManager] 구매할 상품 데이터를 찾을 수 없습니다: {bs.Item_ID}");
            return;
        }

        playerState.Experience -= (bs.Item_Price*5);
        inventoryManager.AddItemToInventory(itemData);
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
        return ItemDataFactory.FromMerchantItem(bs, jsonManager);
    }

    string GetStatText(ItemData item)
    {
        return ItemDataFactory.GetStatText(item);
    }

    string GetOptionText(ItemData item)
    {
        List<string> options = new();
        ItemDataFactory.TryGetOptionValues(item, out string id1, out int val1, out string id2, out int val2);

        if (ItemDataFactory.HasOption(id1))
        {
            string desc = OptionManager.GetOptionDescription(id1);
            if (!string.IsNullOrEmpty(desc))
                options.Add($"{desc} +{val1}");
        }

        if (ItemDataFactory.HasOption(id2))
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
