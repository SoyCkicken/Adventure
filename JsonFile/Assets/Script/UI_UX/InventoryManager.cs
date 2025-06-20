using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using MyGame;

public class InventoryManager : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject inventoryPanel;
    public Transform itemGridParent;
    public GameObject itemSlotPrefab;
    public GameObject itemDetailPanel;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemStatText;
    public TextMeshProUGUI itemOptionText;
    public TextMeshProUGUI itemDescText;
    public TextMeshProUGUI itemTypeText;
    public Button equipButton;
    public Button unequipButton;
    public Button useButton;
    public Button OnInventoryButton;
    public Button OffInventoryButton;
    public Button OffItemDetailButton;
    public Button removeButton;
    public TextMeshProUGUI DPSText;
    public TextMeshProUGUI HPText;
    public GameObject pendingItemUIPrefab;
    public Transform pendingItemUIParent;
    
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
    private const int minSlotCount = 10;
    private int currnetSlotCount;
    private const int maxSlotCount = 21;

    private ItemData selectedItem;

    private void Start()
    {
        // 테스트용 아이템 추가
        // 소모 아이템 같은 경우 아직 구조가 정해지지 않아서 이렇게 되어 있음
        inventoryItems.Add(new ItemData { Item_ID = "Potion_001", Item_Type = "Consumable", Item_Name = "빨간 포션", Heal_Value = 30, Description = "체력을 30 회복하는 포션입니다.", Icon = "potion_red" });
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
        });
        OffInventoryButton.onClick.AddListener(() =>
        {
            inventoryPanel.SetActive(false);
            OffInventoryButton.gameObject.SetActive(false);
            OnInventoryButton.gameObject.SetActive(true);
        });
        OffItemDetailButton.onClick.AddListener(() => itemDetailPanel.SetActive(false));

        // 슬롯 생성
        //for (int i = 0; i < currnetSlotCount; i++)
        //{
        //    var slotGO = Instantiate(itemSlotPrefab, itemGridParent);
        //    var slotUI = slotGO.GetComponent<ItemSlotUI>();
        //    slotUI.Clear();
        //    slotUIs.Add(slotUI);
        //}
        UpdateInventoryByStrength();
        LoadInventory();
        updateDPS_MaxHealth();
    }

    public void LoadInventory()
    {
        foreach (var slot in slotUIs)
            slot.Clear();

        for (int i = 0; i < inventoryItems.Count && i < slotUIs.Count; i++)
            slotUIs[i].Setup(inventoryItems[i], ShowItemDetail);
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
        selectedItem = item;
        itemDetailPanel.SetActive(true);

        if (item.Item_Type == "Weapon")
        {
            var weapon = weaponMasters.FirstOrDefault(w => w.Weapon_ID == selectedItem.Item_ID);
            itemNameText.text = weapon?.Weapon_Name;
            itemDescText.text = weapon?.Description;
            itemTypeText.text = "무기";
        }
        else if (item.Item_Type == "Armor")
        {
            var armor = armorMasters.FirstOrDefault(a => a.Armor_ID == selectedItem.Item_ID);
            itemNameText.text = armor?.Armor_NAME;
            itemDescText.text = armor?.Description;
            itemTypeText.text = "방어구";
        }
        else
        {
            itemNameText.text = item.Item_Name;
            itemDescText.text = item.Description;
            itemTypeText.text = "소비아이템";
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
                break;

            case "Armor":
                bool isArmorEquipped = (item.Item_ID == player.armor_Name);
                equipButton.gameObject.SetActive(!isArmorEquipped);
                unequipButton.gameObject.SetActive(isArmorEquipped);
                removeButton.gameObject.SetActive(!isArmorEquipped);
                break;

            case "Consumable":
                useButton.gameObject.SetActive(true);
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
        else
        {
            id1 = item.Option_1_ID;
            val1 = item.Option_Value1;
            id2 = item.Option_2_ID;
            val2 = item.Option_Value2;
        }

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

    public void OnClickEquip()
    {
        if (selectedItem == null) return;

        // 장착 시 기존 장비는 인벤토리로
       
        //if (selectedItem.Item_Type == "Weapon")
        //{
        //    //Debug.Log(selectedItem.Item_Type);
            
        //    if (weaponEquipSlot.CurrentItem != null &&
        //        weaponEquipSlot.CurrentItem != selectedItem)
        //    {
        //        //Debug.Log(weaponEquipSlot.CurrentItem.Item_Type);
        //        AddItemToInventory(weaponEquipSlot.CurrentItem);
        //        player.RemoveBuffByItem(weaponEquipSlot.CurrentItem.Item_ID);
        //        weaponEquipSlot.Clear();
        //        //Debug.Log("무기가 장착 중이라서 장착 해제 처리됩니다");
        //        //Debug.Log(player.CitChance);
        //    }
        //    weaponEquipSlot.Setup(selectedItem, ShowItemDetail);
        //    inventoryItems.Remove(selectedItem);
        //    player.weapon_Name = selectedItem.Item_ID;
            
        //}
        //else if (selectedItem.Item_Type == "Armor")
        //{
        //    //Debug.Log(selectedItem.Item_Type);
        //    if (armorEquipSlot.CurrentItem != null &&
        //        armorEquipSlot.CurrentItem != selectedItem)
        //    {
        //        //Debug.Log(armorEquipSlot.CurrentItem.Item_Type);
        //        AddItemToInventory(armorEquipSlot.CurrentItem);
        //        player.RemoveBuffByItem(armorEquipSlot.CurrentItem.Item_ID);
        //        armorEquipSlot.Clear();
        //        //Debug.Log("방어구가 장착 중이라서 장착 해제 처리됩니다");
        //        //Debug.Log(player.CitChance);
        //    }
        //    armorEquipSlot.Setup(selectedItem, ShowItemDetail);
        //    inventoryItems.Remove(selectedItem);
        //    player.armor_Name = selectedItem.Item_ID;
        //}

        //equipmentSystem.Init();
        //LoadInventory();
        //updateDPS_MaxHealth();
        //ShowItemDetail(selectedItem);

        if (selectedItem.Item_Type == "Weapon")
    {
        weaponEquipSlot.Setup(selectedItem, ShowItemDetail);
        inventoryItems.Remove(selectedItem);
        player.weapon_Name = selectedItem.Item_ID;
    }
    else if (selectedItem.Item_Type == "Armor")
    {
        armorEquipSlot.Setup(selectedItem, ShowItemDetail);
        inventoryItems.Remove(selectedItem);
        player.armor_Name = selectedItem.Item_ID;
    }

    // 3) 스탯 등 초기화 & 재적용
    equipmentSystem.Init();
        Debug.Log("인벤토리에서 초기화 됨");
    LoadInventory();
    updateDPS_MaxHealth();
    ShowItemDetail(selectedItem);
    }

    public void OnClickUnequip()
    {
        if (selectedItem == null) return;
        if (inventoryItems.Count >= maxSlotCount)
        {
            Debug.Log("인벤토리가 가득 찼습니다. 장착 해제 실패");
            return;
        }

        //if (selectedItem.Item_Type == "Weapon")
        //{
        //    var unequippedItem = weaponEquipSlot.CurrentItem; // 🔒 캐시
        //    weaponEquipSlot.Clear();
        //    AddItemToInventory(unequippedItem);               // ⬅ 이걸로 넣어야 안전
        //    player.RemoveBuffByItem(unequippedItem.Item_ID);

        //    player.RemoveBuffByItem(selectedItem.Item_ID);

        //}
        //else if (selectedItem.Item_Type == "Armor")
        //{
        //    var unequippedItem = armorEquipSlot.CurrentItem; // 🔒 캐시
        //    armorEquipSlot.Clear();
        //    AddItemToInventory(unequippedItem);               // ⬅ 이걸로 넣어야 안전
        //    player.RemoveBuffByItem(unequippedItem.Item_ID);

        //    player.RemoveBuffByItem(selectedItem.Item_ID);
        //}

        if (selectedItem.Item_Type == "Weapon")
        {
            var old = weaponEquipSlot.CurrentItem;
            if (old != null)
            {
                AddItemToInventory(old);
                player.RemoveBuffByItem(old.Item_ID);
                weaponEquipSlot.Clear();
            }
            player.weapon_Name = null;
        }
        else if (selectedItem.Item_Type == "Armor")
        {
            var old = armorEquipSlot.CurrentItem;
            if (old != null)
            {
                AddItemToInventory(old);
                player.RemoveBuffByItem(old.Item_ID);
                armorEquipSlot.Clear();
            }
            player.armor_Name = null;
        }


        equipmentSystem.Init();
        LoadInventory();
        updateDPS_MaxHealth();
        itemDetailPanel.SetActive(false);
        selectedItem = null;
    }

    public void OnClickUse()
    {
        if (selectedItem == null || selectedItem.Item_Type != "Consumable") return;

        if (selectedItem.Heal_Value > 0)
        {
            playerState.CurrentHealth = Mathf.Min(playerState.HP, playerState.CurrentHealth + selectedItem.Heal_Value);
            Debug.Log("체력 회복 포션 사용 했습니다");
        }

        if (selectedItem.Mental_Heal_Value > 0)
        {
            playerState.CurrentMental = Mathf.Min(playerState.MP, playerState.CurrentMental + selectedItem.Mental_Heal_Value);
            Debug.Log("정신력 회복 포션 사용 했습니다");
        }

        inventoryItems.Remove(selectedItem);
        itemDetailPanel.SetActive(false);
        LoadInventory();
    }
    public void updateDPS_MaxHealth()
    {
        DPSText.text = (player.damage * player.speed).ToString("0.0");
        HPText.text = player.MaxHealth.ToString();
        //Debug.Log($"플레이어의 공격력 : {player.damage}\n플레이어의 속도 : {player.speed}\n플레이어의 체력 : {player.MaxHealth}");
    }
    int GetInventorySizeFromStrength(int strength)
    {
        return Mathf.Clamp(10 + (strength / 3), minSlotCount, maxSlotCount);
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
        //inventoryItems.Remove(selectedItem);
        //selectedItem = null;
        //itemDetailPanel.SetActive(false);
        //LoadInventory();
        //Debug.Log("아이템이 삭제되었습니다.");
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
}