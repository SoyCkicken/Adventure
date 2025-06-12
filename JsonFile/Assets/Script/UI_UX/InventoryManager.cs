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
    public Button equipButton;
    public Button unequipButton;
    public Button useButton;
    public Button OnInventoryButton;
    public Button OffInventoryButton;
    public Button OffItemDetailButton;
    public TextMeshProUGUI DPSText;
    public TextMeshProUGUI HPText;

    [Header("Data References")]
    public EquipmentSystem equipmentSystem;
    public JsonManager jsonManager;
    public Character player; // 전투 시 체력 등
    public PlayerState playerState; // 스토리용 체력, 정신력

    private List<ItemData> inventoryItems = new List<ItemData>();
    public ItemSlotUI weaponEquipSlot;
    public ItemSlotUI armorEquipSlot;
    private List<ItemSlotUI> slotUIs = new();
    private const int maxSlotCount = 15;

    private ItemData selectedItem;

    private void Start()
    {
        // 테스트용 아이템 추가
        inventoryItems.Add(new ItemData { Item_ID = "Potion_001", Item_Type = "Consumable", Item_Name = "빨간 포션", Heal_Value = 30, Description = "체력을 30 회복하는 포션입니다.", Icon = "potion_red" });
        inventoryItems.Add(new ItemData { Item_ID = "Weapon_002", Item_Type = "Weapon", One_Handed = "TRUE", Icon = "sword_iron" });
        inventoryItems.Add(new ItemData { Item_ID = "Armor_001", Item_Type = "Armor", Icon = "sword_iron" });

        // UI 버튼 연결
        equipButton.onClick.AddListener(OnClickEquip);
        unequipButton.onClick.AddListener(OnClickUnequip);
        useButton.onClick.AddListener(OnClickUse);

        OnInventoryButton.onClick.AddListener(() => {
            OffInventoryButton.gameObject.SetActive(true);
            OnInventoryButton.gameObject.SetActive(false);
            inventoryPanel.SetActive(true);
        });
        OffInventoryButton.onClick.AddListener(() => {
            inventoryPanel.SetActive(false);
            OffInventoryButton.gameObject.SetActive(false);
            OnInventoryButton.gameObject.SetActive(true);
        });
        OffItemDetailButton.onClick.AddListener(() => itemDetailPanel.SetActive(false));

        // 슬롯 생성
        for (int i = 0; i < maxSlotCount; i++)
        {
            var slotGO = Instantiate(itemSlotPrefab, itemGridParent);
            var slotUI = slotGO.GetComponent<ItemSlotUI>();
            slotUI.Clear();
            slotUIs.Add(slotUI);
        }

        LoadInventory();
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

        inventoryItems.Add(newItem);
        LoadInventory();
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
        }
        else if (item.Item_Type == "Armor")
        {
            var armor = armorMasters.FirstOrDefault(a => a.Armor_ID == selectedItem.Item_ID);
            itemNameText.text = armor?.Armor_NAME;
            itemDescText.text = armor?.Description;
        }
        else
        {
            itemNameText.text = item.Item_Name;
            itemDescText.text = item.Description;
        }

        itemStatText.text = GetStatText(item);
        itemOptionText.text = GetOptionText(item);

        equipButton.gameObject.SetActive(false);
        unequipButton.gameObject.SetActive(false);
        useButton.gameObject.SetActive(false);

        switch (item.Item_Type)
        {
            case "Weapon":
                bool isWeaponEquipped = (item.Item_ID == player.weapon_Name);
                equipButton.gameObject.SetActive(!isWeaponEquipped);
                unequipButton.gameObject.SetActive(isWeaponEquipped);
                break;

            case "Armor":
                bool isArmorEquipped = (item.Item_ID == player.armor_Name);
                equipButton.gameObject.SetActive(!isArmorEquipped);
                unequipButton.gameObject.SetActive(isArmorEquipped);
                break;

            case "Consumable":
                useButton.gameObject.SetActive(true);
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

        if (item.Item_Type == "Weapon")
        {
            var weapon = weaponMasters.FirstOrDefault(w => w.Weapon_ID == selectedItem.Item_ID);
            if (!string.IsNullOrEmpty(weapon?.Option_1_ID)) options.Add($"{weapon.Option_1_ID} +{weapon.Option_Value1}");
            if (!string.IsNullOrEmpty(weapon?.Option_2_ID)) options.Add($"{weapon.Option_2_ID} +{weapon.Option_Value2}");
        }
        else if (item.Item_Type == "Armor")
        {
            var armor = armorMasters.FirstOrDefault(a => a.Armor_ID == selectedItem.Item_ID);
            if (!string.IsNullOrEmpty(armor?.Armor_Option1)) options.Add($"{armor.Armor_Option1} +{armor.Option1_Value}");
            if (!string.IsNullOrEmpty(armor?.Armor_Option2)) options.Add($"{armor.Armor_Option2} +{armor.Option2_Value}");
        }
        else
        {
            if (!string.IsNullOrEmpty(item.Option_1_ID)) options.Add($"{item.Option_1_ID} +{item.Option_Value1}");
            if (!string.IsNullOrEmpty(item.Option_2_ID)) options.Add($"{item.Option_2_ID} +{item.Option_Value2}");
        }
        return string.Join("\n", options);
    }

    public void OnClickEquip()
    {
        if (selectedItem == null) return;

        // 장착 시 기존 장비는 인벤토리로
        if (selectedItem.Item_Type == "Weapon")
        {
            if (weaponEquipSlot.CurrentItem != null)
                AddItemToInventory(weaponEquipSlot.CurrentItem);

            weaponEquipSlot.Setup(selectedItem, ShowItemDetail);
            inventoryItems.Remove(selectedItem);
            player.weapon_Name = selectedItem.Item_ID;
        }
        else if (selectedItem.Item_Type == "Armor")
        {
            if (armorEquipSlot.CurrentItem != null)
                AddItemToInventory(armorEquipSlot.CurrentItem);

            armorEquipSlot.Setup(selectedItem, ShowItemDetail);
            inventoryItems.Remove(selectedItem);
            player.armor_Name = selectedItem.Item_ID;
        }

        equipmentSystem.Init();
        LoadInventory();
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

        if (selectedItem.Item_Type == "Weapon")
        {
            player.weapon_Name = null;
            weaponEquipSlot.Clear();
            AddItemToInventory(selectedItem);
        }
        else if (selectedItem.Item_Type == "Armor")
        {
            player.armor_Name = null;
            armorEquipSlot.Clear();
            AddItemToInventory(selectedItem);
        }

        equipmentSystem.Init();
        ShowItemDetail(selectedItem);
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
}