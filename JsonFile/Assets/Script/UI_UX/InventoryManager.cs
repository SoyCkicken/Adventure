using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using MyGame;
using UnityEngine.Timeline;
using System;


public class InventoryManager : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject inventoryPanel;
    public Transform itemGridParent;
    public GameObject itemSlotPrefab;
    [Header("아이템 정보창")]
    public GameObject itemDetailPanel;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemStatText;
    public TextMeshProUGUI itemOptionText;
    public TextMeshProUGUI itemDescText;
    public TextMeshProUGUI itemTypeText;
    public Image item_Icon;
    public Button equipButton;
    public Button unequipButton;
    public Button useButton;
    public Button OnInventoryButton;
    public Button OffInventoryButton;
    public Button OffItemDetailButton;
    public Button removeButton;
    public Button SaleButton; // 판매 버튼 추가
    public TextMeshProUGUI DPSText;
    public TextMeshProUGUI HPText;
    public TMP_Text SoulTEXT;
    public GameObject pendingItemUIPrefab;
    public Transform pendingItemUIParent;
    public SpriteBank spriteBank;

    [Header("Data References")]
    public EquipmentSystem equipmentSystem;
    public JsonManager jsonManager;
    public Character player; // 전투 시 체력 등
    public PlayerState playerState; // 스토리용 체력, 정신력
    public ConfirmPopup confirmPopup;
    public OptionManager optionManager;

    private List<ItemData> inventoryItems = new List<ItemData>();
    public ItemSlotUI weaponEquipSlot;
    public ItemSlotUI armorEquipSlot;
    private List<ItemSlotUI> slotUIs = new();
    private List<ItemData> pendingItems = new();
    private const int minSlotCount = 7;
    private int currnetSlotCount;
    private const int maxSlotCount = 14;

    public ItemData selectedItem;
    public void Awake()
    {
        jsonManager = JsonManager.Instance; // 수정
        playerState = PlayerState.Instance;
        spriteBank = SpriteBank.Instance;
        optionManager = OptionManager.Instance;
    }
    private void Start()
    {
        // 테스트용 아이템 추가
        // 소모 아이템 같은 경우 아직 구조가 정해지지 않아서 이렇게 되어 있음
        //inventoryItems.Add(new ItemData { Item_ID = "Item_001", Item_Type = "Consumable", Item_Name = "빨간 포션", Heal_Value = 30, Description = "체력을 30 회복하는 포션입니다.", Icon = "potion_red" });
        // 여기 부터는 실질 적으로 아이템의 정보가 DATA로 들어가 있음
        //inventoryItems.Add(new ItemData { Item_ID = "Weapon_002", Item_Type = "Weapon", One_Handed = "TRUE", Icon = "sword_iron" });
        //inventoryItems.Add(new ItemData { Item_ID = "Armor_001", Item_Type = "Armor", Icon = "sword_iron" });
        int currnetSlotCount = GetInventorySizeFromStrength(playerState.STR);
        // UI 버튼 연결
        equipButton.onClick.AddListener(OnClickEquip);
        unequipButton.onClick.AddListener(OnClickUnequip);
        useButton.onClick.AddListener(OnClickUse);
        removeButton.onClick.AddListener(OnClickRemove);
        //기본적으로 비활성화 시켜둠
        itemDetailPanel.SetActive(false);
        OnInventoryButton.onClick.AddListener(() =>
        {
            OffInventoryButton.gameObject.SetActive(true);
            OnInventoryButton.gameObject.SetActive(false);
            inventoryPanel.SetActive(true);
            UpdateDPS_MaxHealth();
        });
        OffInventoryButton.onClick.AddListener(() =>
        {
            inventoryPanel.SetActive(false);
            OffInventoryButton.gameObject.SetActive(false);
            OnInventoryButton.gameObject.SetActive(true);
        });
        OffItemDetailButton.onClick.AddListener(() => itemDetailPanel.SetActive(false));
        UpdateInventoryByStrength();
        LoadInventory();
        UpdateDPS_MaxHealth();
    }

    public void LoadInventory()
    {
        foreach (var slot in slotUIs)
        {
            slot.Clear();
            slot.icon.sprite = spriteBank.Load("UI_InventorySlot 1");
        }
        for (int i = 0; i < inventoryItems.Count && i < slotUIs.Count; i++)
        {
            slotUIs[i].Setup(inventoryItems[i], ShowItemDetail);
        }
    }
    // 힘에 따라 칸수 조절인데
    public void UpdateInventoryByStrength()
    {
        int newCount = GetInventorySizeFromStrength(playerState.STR);

        if (newCount < currnetSlotCount)
        {
            HandleInventoryShrink(newCount);
        }
        else if (newCount > currnetSlotCount)
        {
            for (int i = currnetSlotCount; i < newCount; i++)
            {
                var slotGO = Instantiate(itemSlotPrefab, itemGridParent);
                var slotUI = slotGO.GetComponent<ItemSlotUI>();
                slotUI.Clear();
                slotUIs.Add(slotUI);
            }
        }
        currnetSlotCount = newCount;
        LoadInventory();
        TryRecoverPendingItems();
    }

    public void AddItemToInventory(ItemData newItem)
    {
        if (inventoryItems.Count >= maxSlotCount)
        {
            Debug.LogWarning("인벤토리가 가득 찼습니다.");
            return;
        }
        Debug.Log($"아이템 추가 완료 : {newItem}");

        inventoryItems.Add(newItem.Clone());
        LoadInventory();
    }


    public void HandleInventoryShrink(int newCount)
    {
        //아이템의 칸수가 넘어가면 마지막 칸수 - 1번째 부터 임시 칸수로 이동 시킴

        while (inventoryItems.Count > newCount)
        {
            var item = inventoryItems[^1];
            inventoryItems.RemoveAt(inventoryItems.Count - 1);
            pendingItems.Add(item);

            var ui = Instantiate(pendingItemUIPrefab, pendingItemUIParent);
            var uiScript = ui.GetComponent<PendingItemUI>();
            uiScript.Setup(item, jsonManager);
        }
    }
    //혹시 아이템 칸수가 부족해질 경우 임시 아이템으로 빼버림
    private void TryRecoverPendingItems()
    {
        while (pendingItems.Count > 0 && inventoryItems.Count < currnetSlotCount)
        {

            var item = pendingItems[0];
            pendingItems.RemoveAt(0);
            inventoryItems.Add(item);

            if (pendingItemUIParent.childCount > 0)
                Destroy(pendingItemUIParent.GetChild(0).gameObject);
        }
    }
    void ShowItemDetail(ItemData item)
    {
        selectedItem = item;
        ItemDataFactory.ApplyMasterData(selectedItem, jsonManager);
        itemDetailPanel.SetActive(true);

        itemNameText.text = string.IsNullOrEmpty(selectedItem.Item_Name) ? selectedItem.Item_ID : selectedItem.Item_Name;
        itemDescText.text = selectedItem.Description;
        itemTypeText.text = GetItemTypeLabel(selectedItem);
        ApplyItemIcon(selectedItem);

        itemStatText.text = GetStatText(selectedItem);
        itemOptionText.text = GetOptionText(selectedItem);

        equipButton.gameObject.SetActive(false);
        unequipButton.gameObject.SetActive(false);
        useButton.gameObject.SetActive(false);
        removeButton.gameObject.SetActive(false);

        switch (selectedItem.Item_Type)
        {
            case "Weapon":
                bool isWeaponEquipped = IsSlotItem(weaponEquipSlot, selectedItem);
                equipButton.gameObject.SetActive(!isWeaponEquipped);
                unequipButton.gameObject.SetActive(isWeaponEquipped);
                removeButton.gameObject.SetActive (!isWeaponEquipped);
                LogItemOptions(selectedItem);
                break;

            case "Armor":
                bool isArmorEquipped = IsSlotItem(armorEquipSlot, selectedItem);
                equipButton.gameObject.SetActive(!isArmorEquipped);
                unequipButton.gameObject.SetActive(isArmorEquipped);
                removeButton.gameObject.SetActive(!isArmorEquipped);
                LogItemOptions(selectedItem);
                break;

            case "Consumable":
                useButton.gameObject.SetActive(true);
                removeButton.gameObject.SetActive(true);
                LogItemOptions(selectedItem);
                break;
            case "Item":
                removeButton.gameObject.SetActive(true);
                break;
        }
    }

    private bool IsSlotItem(ItemSlotUI slot, ItemData item)
    {
        return slot?.CurrentItem != null && item != null && slot.CurrentItem.Item_ID == item.Item_ID;
    }

    private string GetItemTypeLabel(ItemData item)
    {
        if (item == null) return "";
        return item.Item_Type switch
        {
            "Weapon" => "무기",
            "Armor" => "방어구",
            "Consumable" => "소비",
            "Item" => "일반",
            _ => item.Item_Type
        };
    }

    private void ApplyItemIcon(ItemData item)
    {
        if (item_Icon == null)
        {
            Debug.LogError("[ItemSlotUI] icon(Image)가 에디터에 연결되지 않았습니다.");
            return;
        }

        string spriteName = string.IsNullOrEmpty(item?.Item_Name) ? "UI_InventorySlot 1" : item.Item_Name;
        Sprite sprite = spriteBank.Load(spriteName);
        if (sprite != null)
            item_Icon.sprite = sprite;
    }

    private void LogItemOptions(ItemData item)
    {
        if (!ItemDataFactory.TryGetOptionValues(item, out string option1, out int value1, out string option2, out int value2))
            return;

        if (ItemDataFactory.HasOption(option1))
            Debug.Log($"{option1} : {value1}");
        if (ItemDataFactory.HasOption(option2))
            Debug.Log($"{option2} : {value2}");
    }
    // 추가 된 부분 확인용 주석

    public int CountItem(string itemCode) => CountItemInstances(itemCode);

    // 비스택: 같은 코드의 '객체 수'를 센다
    public int CountItemInstances(string itemCode)
    {
        if (string.IsNullOrEmpty(itemCode) || inventoryItems == null) return 0;

        int total = 0;
        foreach (var it in inventoryItems)
        {
            if (string.Equals(it.Item_ID, itemCode, StringComparison.OrdinalIgnoreCase))
            {
                // 스택이 아니므로 엔트리 하나가 곧 1개
                total += 1;
            }
        }
        return total;
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
            string desc = jsonManager.GetOptionById(id1)?.Option_Description;
            if (!string.IsNullOrEmpty(desc))
                options.Add($"{desc} +{val1}");
        }

        if (ItemDataFactory.HasOption(id2))
        {
            string desc = jsonManager.GetOptionById(id2)?.Option_Description;
            if (!string.IsNullOrEmpty(desc))
                options.Add($"{desc} +{val2}");
        }

        return string.Join("\n", options);
    }
    //public void OnClickEquip()
    //{
    //    if (selectedItem == null) return;

    //    // 이미 장착된 같은 아이템이면 중복 방지
    //    if ((selectedItem.Item_Type == "Weapon" && weaponEquipSlot.CurrentItem != null && weaponEquipSlot.CurrentItem.Item_ID == selectedItem.Item_ID) ||
    //        (selectedItem.Item_Type == "Armor" && armorEquipSlot.CurrentItem != null && armorEquipSlot.CurrentItem.Item_ID == selectedItem.Item_ID))
    //    {
    //        Debug.LogWarning("이미 장착 중인 아이템입니다. 중복 장착 방지됨.");
    //        return;
    //    }

    //    // 기존 장착 아이템 복사 후 인벤토리에 추가
    //    if (selectedItem.Item_Type == "Weapon")
    //    {
    //        if (weaponEquipSlot.CurrentItem != null)
    //        {
    //            var existing = weaponEquipSlot.CurrentItem;
    //            if (!inventoryItems.Any(i => i == existing))
    //            {
    //                AddItemToInventory(existing.Clone());
    //            }
    //        }
    //        weaponEquipSlot.Setup(selectedItem, ShowItemDetail);
    //        inventoryItems.Remove(selectedItem);
    //        player.weapon_Name = selectedItem.Item_ID;
    //    }
    //    else if (selectedItem.Item_Type == "Armor")
    //    {
    //        if (armorEquipSlot.CurrentItem != null)
    //        {
    //            var existing = armorEquipSlot.CurrentItem;
    //            if (!inventoryItems.Any(i => i == existing))
    //            {
    //                AddItemToInventory(existing.Clone());
    //            }
    //        }
    //        armorEquipSlot.Setup(selectedItem, ShowItemDetail);
    //        inventoryItems.Remove(selectedItem);
    //        player.armor_Name = selectedItem.Item_ID;
    //    }

    //    selectedItem = null; // ⭐ 꼭 초기화!
    //    equipmentSystem.Init();
    //    LoadInventory();
    //    UpdateDPS_MaxHealth(); 
    //    itemDetailPanel.SetActive(false); // 패널 닫기
    //    selectedItem = null;              // 선택 정보 제거
    //}

    public void OnClickEquip()
    {
        if (selectedItem == null) return;

        // 중복 체크
        if ((selectedItem.Item_Type == "Weapon" && weaponEquipSlot.CurrentItem != null && weaponEquipSlot.CurrentItem.Item_ID == selectedItem.Item_ID) ||
          (selectedItem.Item_Type == "Armor" && armorEquipSlot.CurrentItem != null && armorEquipSlot.CurrentItem.Item_ID == selectedItem.Item_ID))
        {
            Debug.LogWarning("이미 장착 중인 아이템입니다.");
            return;
        }

        equipmentSystem.EquipItem(selectedItem,inventoryItems,weaponEquipSlot,armorEquipSlot,ShowItemDetail); // 콜백도 전달

        selectedItem = null;
        LoadInventory();
        UpdateDPS_MaxHealth();
        itemDetailPanel.SetActive(false);
    }

    //public void OnClickUnequip()
    //{
    //    if (selectedItem == null) return;

    //    // 먼저 null로 설정 (중복 방지 핵심)
    //    var unequipItem = selectedItem;
    //    selectedItem = null;

    //    if (inventoryItems.Count >= maxSlotCount)
    //    {
    //        Debug.Log("인벤토리가 가득 찼습니다. 장착 해제 실패");
    //        return;
    //    }

    //    if (unequipItem.Item_Type == "Weapon")
    //    {
    //        var clone = weaponEquipSlot.CurrentItem?.Clone();
    //        if (clone != null)
    //        {
    //            AddItemToInventory(clone);
    //            player.RemoveBuffByItem(clone.Item_ID);
    //            weaponEquipSlot.Clear();
    //        }
    //        player.weapon_Name = null;
    //    }
    //    else if (unequipItem.Item_Type == "Armor")
    //    {
    //        var clone = armorEquipSlot.CurrentItem?.Clone();
    //        if (clone != null)
    //        {
    //            AddItemToInventory(clone);
    //            player.RemoveBuffByItem(clone.Item_ID);
    //            armorEquipSlot.Clear();
    //            Debug.Log($"현재 장착 중인 아이템 {armorEquipSlot.CurrentItem}");
    //        }
    //        player.armor_Name = null;
    //    }

    //    equipmentSystem.Init();
    //    LoadInventory();
    //    UpdateDPS_MaxHealth();
    //    itemDetailPanel.SetActive(false);
    //}
    public void OnClickUnequip()
    {
        if (selectedItem == null) return;

        ItemSlotUI targetSlot = selectedItem.Item_Type == "Weapon" ? weaponEquipSlot : armorEquipSlot;

        if (inventoryItems.Count >= maxSlotCount)
        {
            Debug.Log("인벤토리가 가득 찼습니다. 장착 해제 실패");
            return;
        }

        equipmentSystem.UnequipItem(targetSlot, inventoryItems);

        LoadInventory();
        UpdateDPS_MaxHealth();
        itemDetailPanel.SetActive(false);
    }
    public void OnClickUse()
    {
        if (selectedItem == null || selectedItem.Item_Type != "Consumable") return;
        ItemDataFactory.ApplyMasterData(selectedItem, jsonManager);
        Debug.Log("아이템 사용을 시도 했습니다");
        OptionManager.UseItem(selectedItem, new OptionContext
        {
            User = player,
            playerState = playerState,
            option_ID = selectedItem.Option_1_ID,
            Value = selectedItem.Option_Value1
        });

            inventoryItems.Remove(selectedItem);
        itemDetailPanel.SetActive(false);
        LoadInventory();
    }
    public void UpdateDPS_MaxHealth()
    {
        Debug.Log($"player.damage = {player.damage}");
        DPSText.text = (player.damage * player.speed).ToString("0.0");
        HPText.text = player.MaxHealth.ToString();
        //Debug.Log($"플레이어의 공격력 : {player.damage}\n플레이어의 속도 : {player.speed}\n플레이어의 체력 : {player.MaxHealth}");
    }
    int GetInventorySizeFromStrength(int strength)
    {
        return Mathf.Clamp(minSlotCount + (strength / 3), minSlotCount, maxSlotCount);
    }
    public void UpdateGoldText()
    {
        SoulTEXT.text = $"Gold: {playerState.Experience:0}";
    }
    public void OnClickRemove()
    {
        if (selectedItem == null) return;

        if (!inventoryItems.Contains(selectedItem))
        {
            Debug.LogWarning("선택된 아이템이 인벤토리에 없습니다.");
            return;
        }
        ConfirmPopup.Show($"{selectedItem.Item_Name} 을(를) 정말 삭제할까요?", () =>
        {
            inventoryItems.Remove(selectedItem);
            selectedItem = null;
            itemDetailPanel.SetActive(false);
            LoadInventory();
        });
    }
    public void SaveInventoryData(ref SaveManager.SaveData data)
    {
        data.inventoryItems = inventoryItems.Select(item => item.Clone()).ToList();
  
            if (!string.IsNullOrEmpty(player.weapon_Name))
                data.equippedWeaponData = weaponEquipSlot.CurrentItem.Clone();
            else
                data.equippedWeaponData = null;
            if (!string.IsNullOrEmpty(player.armor_Name))
                data.equippedArmorData = armorEquipSlot.CurrentItem.Clone();
            else
                data.equippedArmorData = null;
      
            
    }

    public void LoadInventoryData(SaveManager.SaveData data)
    {
        inventoryItems.Clear();
        inventoryItems.AddRange(data.inventoryItems.Select(item => item.Clone()));

        if (data.equippedWeaponData != null && !string.IsNullOrEmpty(data.equippedWeaponData.Item_ID))
        {
            weaponEquipSlot.Setup(data.equippedWeaponData.Clone(), ShowItemDetail);
            player.weapon_Name = data.equippedWeaponData.Item_ID;
        }

        if (data.equippedArmorData != null && !string.IsNullOrEmpty(data.equippedArmorData.Item_ID))
        {
            armorEquipSlot.Setup(data.equippedArmorData.Clone(), ShowItemDetail);
            player.armor_Name = data.equippedArmorData.Item_ID;
        }

        equipmentSystem.Init();      // 능력치 반영
        UpdateDPS_MaxHealth();       // DPS, 체력 갱신
        LoadInventory();        // 인벤토리 UI 갱신 
    }

    
}

[System.Serializable]
public class ItemData
{
    public string Item_ID;
    public string Item_Type;
    public string Item_Name;
    public int Weapon_DMG;
    public int Armor_DEF;
    public int Armor_HP;
    public string One_Handed;
    public int Heal_Value;
    public int Mental_Heal_Value;
    public string Option_1_ID;
    public int Option_Value1;
    public string Option_2_ID;
    public int Option_Value2;
    public string Description;
    public string Icon;
    public int Item_Price;

    public ItemData Clone()
    {
        return new ItemData
        {
            Item_ID = this.Item_ID,
            Item_Type = this.Item_Type,
            Item_Name = this.Item_Name,
            Weapon_DMG = this.Weapon_DMG,
            Armor_DEF = this.Armor_DEF,
            Armor_HP = this.Armor_HP,
            One_Handed = this.One_Handed,
            Heal_Value = this.Heal_Value,
            Mental_Heal_Value = this.Mental_Heal_Value,
            Option_1_ID = this.Option_1_ID,
            Option_Value1 = this.Option_Value1,
            Option_2_ID = this.Option_2_ID,
            Option_Value2 = this.Option_Value2,
            Description = this.Description,
            Icon = this.Icon,
            Item_Price = this.Item_Price
        };
    }
}
