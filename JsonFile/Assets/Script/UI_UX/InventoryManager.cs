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

    private ItemData selectedItem;

    private void Start()
    {
        playerState = PlayerState.Instance;

        // 테스트용 아이템 추가
        // 소모 아이템 같은 경우 아직 구조가 정해지지 않아서 이렇게 되어 있음
        inventoryItems.Add(new ItemData { Item_ID = "Item_001", Item_Type = "Consumable", Item_Name = "빨간 포션", Heal_Value = 30, Description = "체력을 30 회복하는 포션입니다.", Icon = "potion_red" });
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

    public void AddItemToInventory(ItemData newItem)
    {
        if (inventoryItems.Count >= maxSlotCount)
        {
            Debug.LogWarning("인벤토리가 가득 찼습니다.");
            return;
        }
        Debug.Log($"아이템 추가 완료 : {newItem}");

        inventoryItems.Add(newItem);
        LoadInventory();
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
        var weaponMasters = jsonManager.GetWeaponMasters("Weapon_Master");
        var armorMasters = jsonManager.GetArmorMasters("Armor_Master");
        var itemMasters = jsonManager.GetItemMasters("Item_Master");
        
        selectedItem = item;
        itemDetailPanel.SetActive(true);

        if (item.Item_Type == "Weapon")
        {
            var weapon = weaponMasters.FirstOrDefault(w => w.Weapon_ID == selectedItem.Item_ID);
            itemNameText.text = weapon?.Weapon_Name;
            itemDescText.text = weapon?.Description;
            itemTypeText.text = "무기";
            if (!string.IsNullOrEmpty(item.Item_Name))
            {
                Debug.Log(item.Item_Name);
                if (item_Icon == null)
                {
                    Debug.LogError("[ItemSlotUI] icon(Image)가 에디터에 연결되지 않았습니다.");
                    return;
                }
                Sprite s = spriteBank.Load(item.Item_Name);
                if (s != null)
                {
                    item_Icon.sprite = s;
                }
            }
            else
            {
                Sprite s = spriteBank.Load("UI_InventorySlot 1");
                item_Icon.sprite = s;
            }
        }
        else if (item.Item_Type == "Armor")
        {
            var armor = armorMasters.FirstOrDefault(a => a.Armor_ID == selectedItem.Item_ID);
            itemNameText.text = armor?.Armor_NAME;
            itemDescText.text = armor?.Description;
            itemTypeText.text = "방어구";
            if (!string.IsNullOrEmpty(item.Item_Name))
            {
                Debug.Log(item.Item_Name);
                if (item_Icon == null)
                {
                    Debug.LogError("[ItemSlotUI] icon(Image)가 에디터에 연결되지 않았습니다.");
                    return;
                }
                Sprite s = spriteBank.Load(item.Item_Name);
                if (s != null)
                {
                    item_Icon.sprite = s;
                }
            }
            else
            {
                Sprite s = spriteBank.Load("UI_InventorySlot 1");
                item_Icon.sprite = s;
            }
        }
        else if (item.Item_Type == "Consumable")
        {
            var Consumptionitem = itemMasters.FirstOrDefault(i => i.Item_ID == selectedItem.Item_ID);
            itemNameText.text = Consumptionitem.Item_NAME;
            itemDescText.text = Consumptionitem.Item_Description;
            itemTypeText.text = "소비";
            if (!string.IsNullOrEmpty(item.Item_Name))
            {
                Debug.Log(item.Item_Name);
                if (item_Icon == null)
                {
                    Debug.LogError("[ItemSlotUI] icon(Image)가 에디터에 연결되지 않았습니다.");
                    return;
                }
                Sprite s = spriteBank.Load(item.Item_Name);
                if (s != null)
                {
                    item_Icon.sprite = s;
                }
            }
            else
            {
                Sprite s = spriteBank.Load("UI_InventorySlot 1");
                item_Icon.sprite = s;
            }
        }
        else
        {
            var Normalitem = itemMasters.FirstOrDefault(i => i.Item_ID == selectedItem.Item_ID);
            itemNameText.text = Normalitem?.Item_NAME;
            itemDescText.text = Normalitem?.Item_Description;
            itemTypeText.text = "일반";
            if (!string.IsNullOrEmpty(item.Item_Name))
            {
                Debug.Log(item.Item_Name);
                if (item_Icon == null)
                {
                    Debug.LogError("[ItemSlotUI] icon(Image)가 에디터에 연결되지 않았습니다.");
                    return;
                }
                Sprite s = spriteBank.Load(item.Item_Name);
                if (s != null)
                {
                    item_Icon.sprite = s;
                }
            }
            else
            {
                Sprite s = spriteBank.Load("UI_InventorySlot 1");
                item_Icon.sprite = s;
            }
        }

        itemStatText.text = GetStatText(item);
        itemOptionText.text = GetOptionText(item);

        equipButton.gameObject.SetActive(false);
        unequipButton.gameObject.SetActive(false);
        useButton.gameObject.SetActive(false);
        removeButton.gameObject.SetActive(false);
        
        switch (item.Item_Type)
        {
            case "Weapon":
                bool isWeaponEquipped = (item.Item_ID == player.weapon_Name);
                equipButton.gameObject.SetActive(!isWeaponEquipped);
                unequipButton.gameObject.SetActive(isWeaponEquipped);
                removeButton.gameObject.SetActive (!isWeaponEquipped);
                var weapon_Master = weaponMasters.FirstOrDefault(i => i.Weapon_ID == selectedItem.Item_ID);
                selectedItem.Option_1_ID = weapon_Master.Option_1_ID;
                selectedItem.Option_Value1 = weapon_Master.Option_Value1;
                selectedItem.Option_2_ID = weapon_Master.Option_2_ID;
                selectedItem.Option_Value2 = weapon_Master.Option_Value1;
                if(!string.IsNullOrEmpty(selectedItem.Option_1_ID))
                Debug.Log($"{selectedItem.Option_1_ID} : {selectedItem.Option_Value1}");
                if(!string.IsNullOrEmpty(selectedItem.Option_2_ID))
                Debug.Log($"{selectedItem.Option_1_ID} : {selectedItem.Option_Value1}");
                break;

            case "Armor":
                bool isArmorEquipped = (item.Item_ID == player.armor_Name);
                equipButton.gameObject.SetActive(!isArmorEquipped);
                unequipButton.gameObject.SetActive(isArmorEquipped);
                removeButton.gameObject.SetActive(!isArmorEquipped);
                var armor_Master = armorMasters.FirstOrDefault(i => i.Armor_ID == selectedItem.Item_ID);
                selectedItem.Option_1_ID = armor_Master.Armor_Option1;
                selectedItem.Option_Value1 = armor_Master.Option1_Value;
                selectedItem.Option_2_ID = armor_Master.Armor_Option2;
                selectedItem.Option_Value2 = armor_Master.Option2_Value;
                if (!string.IsNullOrEmpty(selectedItem.Option_1_ID))
                    Debug.Log($"{selectedItem.Option_1_ID} : {selectedItem.Option_Value1}");
                if (!string.IsNullOrEmpty(selectedItem.Option_2_ID))
                    Debug.Log($"{selectedItem.Option_1_ID} : {selectedItem.Option_Value1}");
                break;

            case "Consumable":
                useButton.gameObject.SetActive(true);
                removeButton.gameObject.SetActive(true);
                var item_Master = itemMasters.FirstOrDefault(i => i.Item_ID == selectedItem.Item_ID);
                //Debug.Log($"옵션 아이디 값 : {master.Item_Option1}");
                selectedItem.Option_1_ID = item_Master.Item_Option1;
                selectedItem.Option_Value1 = item_Master.Option1_Value;
                selectedItem.Option_2_ID = item_Master.Item_Option2;
                selectedItem.Option_Value2 = item_Master.Option2_Value;
                if (!string.IsNullOrEmpty(selectedItem.Option_1_ID))
                    Debug.Log($"{selectedItem.Option_1_ID} : {selectedItem.Option_Value1}");
                if (!string.IsNullOrEmpty(selectedItem.Option_2_ID))
                    Debug.Log($"{selectedItem.Option_1_ID} : {selectedItem.Option_Value1}");
                break;
            case "Item":
                removeButton.gameObject.SetActive(true);
                break;
        }
    }

    string GetStatText(ItemData item)
    {
        var weaponMasters = jsonManager.GetWeaponMasters("Weapon_Master");
        var armorMasters = jsonManager.GetArmorMasters("Armor_Master");
        if (item.Item_Type == "Weapon")
        {
            var weapon = weaponMasters.FirstOrDefault(w => w.Weapon_ID == selectedItem.Item_ID);
            return $"공격력: {weapon?.Weapon_DMG}";
        }
        else if (item.Item_Type == "Armor")
        {
            var armor = armorMasters.FirstOrDefault(a => a.Armor_ID == selectedItem.Item_ID);
            return $"방어력: {armor?.Armor_DEF}, 체력: {armor?.Armor_HP}";
        }
        else if (item.Item_Type == "Consumable")
        {
            List<string> effects = new();
            //if (item.Heal_Value > 0) effects.Add($"체력 회복: {item.Heal_Value}");
            //if (item.Mental_Heal_Value > 0) effects.Add($"정신력 회복: {item.Mental_Heal_Value}");
            return string.Join(", ", effects);
        }
        return "";
    }

    string GetOptionText(ItemData item)
    {
        List<string> options = new();
        var weaponMasters = jsonManager.GetWeaponMasters("Weapon_Master");
        var armorMasters = jsonManager.GetArmorMasters("Armor_Master");
        var itemMasters = jsonManager.GetItemMasters("Item_Master");
        var optionMasters = jsonManager.GetOptionMasters("Option_Master");

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
        else if (item.Item_Type == "Consumable")
        {
            var Consumable = itemMasters.FirstOrDefault(a => a.Item_ID == item.Item_ID);
            id1 = Consumable?.Item_Option1;
            val1 = Consumable?.Option1_Value ?? 0;
            id2 = Consumable?.Item_Option2;
            val2 = Consumable?.Option2_Value ?? 0;
        }
        else
        {
            id1 = item.Option_1_ID;
            val1 = item.Option_Value1;
            id2 = item.Option_2_ID;
            val2 = item.Option_Value2;
        }

        if (!string.IsNullOrEmpty(id1) && id1 != "null")
        {
           var option_1 = optionMasters.FirstOrDefault(a => a.Option_ID == id1);
            string desc = option_1.Option_Description;
            if (!string.IsNullOrEmpty(desc))
                options.Add($"{desc} +{val1}");
        }

        if (!string.IsNullOrEmpty(id2) && id2 != "null")
        {
            var option_2 = optionMasters.FirstOrDefault(a => a.Option_ID == id2);
            string desc = option_2.Option_Description;
            if (!string.IsNullOrEmpty(desc))
                options.Add($"{desc} +{val2}");
        }

        return string.Join("\n", options);
    }
    public void OnClickEquip()
    {
        if (selectedItem == null) return;

        // 이미 장착된 같은 아이템이면 중복 방지
        if ((selectedItem.Item_Type == "Weapon" && weaponEquipSlot.CurrentItem != null && weaponEquipSlot.CurrentItem.Item_ID == selectedItem.Item_ID) ||
            (selectedItem.Item_Type == "Armor" && armorEquipSlot.CurrentItem != null && armorEquipSlot.CurrentItem.Item_ID == selectedItem.Item_ID))
        {
            Debug.LogWarning("이미 장착 중인 아이템입니다. 중복 장착 방지됨.");
            return;
        }

        // 기존 장착 아이템 복사 후 인벤토리에 추가
        if (selectedItem.Item_Type == "Weapon")
        {
            if (weaponEquipSlot.CurrentItem != null)
            {
                var existing = weaponEquipSlot.CurrentItem;
                if (!inventoryItems.Any(i => i == existing))
                {
                    AddItemToInventory(existing.Clone());
                }
            }
            weaponEquipSlot.Setup(selectedItem, ShowItemDetail);
            inventoryItems.Remove(selectedItem);
            player.weapon_Name = selectedItem.Item_ID;
        }
        else if (selectedItem.Item_Type == "Armor")
        {
            if (armorEquipSlot.CurrentItem != null)
            {
                var existing = armorEquipSlot.CurrentItem;
                if (!inventoryItems.Any(i => i == existing))
                {
                    AddItemToInventory(existing.Clone());
                }
            }
            armorEquipSlot.Setup(selectedItem, ShowItemDetail);
            inventoryItems.Remove(selectedItem);
            player.armor_Name = selectedItem.Item_ID;
        }

        selectedItem = null; // ⭐ 꼭 초기화!
        equipmentSystem.Init();
        LoadInventory();
        UpdateDPS_MaxHealth(); 
        itemDetailPanel.SetActive(false); // 패널 닫기
        selectedItem = null;              // 선택 정보 제거
    }

    public void OnClickUnequip()
    {
        if (selectedItem == null) return;

        // 먼저 null로 설정 (중복 방지 핵심)
        var unequipItem = selectedItem;
        selectedItem = null;

        if (inventoryItems.Count >= maxSlotCount)
        {
            Debug.Log("인벤토리가 가득 찼습니다. 장착 해제 실패");
            return;
        }

        if (unequipItem.Item_Type == "Weapon")
        {
            var clone = weaponEquipSlot.CurrentItem?.Clone();
            if (clone != null)
            {
                AddItemToInventory(clone);
                player.RemoveBuffByItem(clone.Item_ID);
                weaponEquipSlot.Clear();
            }
            player.weapon_Name = null;
        }
        else if (unequipItem.Item_Type == "Armor")
        {
            var clone = armorEquipSlot.CurrentItem?.Clone();
            if (clone != null)
            {
                AddItemToInventory(clone);
                player.RemoveBuffByItem(clone.Item_ID);
                armorEquipSlot.Clear();
                //armorEquipSlot.CurrentItem = null;
                Debug.Log($"현재 장착 중인 아이템 {armorEquipSlot.CurrentItem}");
                //Debug.Log($"현재 장착 중인 아이템의 코드 {armorEquipSlot.CurrentItem.Item_ID}");
            }
            player.armor_Name = null;
        }

        equipmentSystem.Init();
        LoadInventory();
        UpdateDPS_MaxHealth();
        itemDetailPanel.SetActive(false);
    }

    public void OnClickUse()
    {
        if (selectedItem == null || selectedItem.Item_Type != "Consumable") return;
        var itemMasters = jsonManager.GetItemMasters("Item_Master");
        var master = itemMasters.FirstOrDefault(i => i.Item_ID == selectedItem.Item_ID);
        Debug.Log("아이템 사용을 시도 했습니다");
        OptionManager.UseItem(selectedItem, new OptionContext
        {
            User = player,
            playerState = playerState,
            option_ID = master.Item_Option1,
            Value = master.Option1_Value,  
        });
        if (selectedItem.Option_2_ID != null)
        {
            OptionManager.UseItem(selectedItem, new OptionContext
            {
                User = player,
                playerState = playerState,
                option_ID = master.Item_Option2,
                Value = master.Option2_Value,
            });
        }

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
    public void updateSoulText()
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

        if (weaponEquipSlot.CurrentItem != null && !string.IsNullOrEmpty(weaponEquipSlot.CurrentItem.Item_ID))
            data.equippedWeaponData = weaponEquipSlot.CurrentItem.Clone();
        else
            data.equippedWeaponData = null;

        if (armorEquipSlot.CurrentItem != null && !string.IsNullOrEmpty(armorEquipSlot.CurrentItem.Item_ID))
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