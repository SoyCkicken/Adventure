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
    [Header("Data References")]
    public EquipmentSystem equipmentSystem;
    public JsonManager jsonManager;
    public Character player; // 전투 시 체력 등
    public PlayerState playerState; // 스토리용 체력, 정신력

    private List<ItemData> inventoryItems = new List<ItemData>();
    private ItemData selectedItem;

    private void Start()
    {
        inventoryItems.Add(new ItemData
        {
            Item_ID = "Potion_001",
            Item_Type = "Consumable",
            Item_Name = "빨간 포션",
            Heal_Value = 30,
            Mental_Heal_Value = 0,
            Description = "체력을 30 회복하는 포션입니다.",
            Icon = "potion_red"
        });

        inventoryItems.Add(new ItemData
        {
            Item_ID = "Weapon_002",
            Item_Type = "Weapon",
            One_Handed = "TRUE",
            Icon = "sword_iron"
        });
        inventoryItems.Add(new ItemData
        {
            Item_ID = "Armor_001",
            Item_Type = "Armor",
            Icon = "sword_iron"
        });
        equipButton.onClick.AddListener(() => { OnClickEquip(); });
        unequipButton.onClick.AddListener(() => { OnClickUnequip(); });
        useButton.onClick.AddListener(() => { OnClickUse(); });
        OnInventoryButton.onClick.AddListener(() =>
        {
            OffInventoryButton.gameObject.SetActive(true);
            OnInventoryButton.gameObject.SetActive(false);
            onInventory();
        });
        OffInventoryButton.onClick.AddListener(() => { offInventroy();
            OffInventoryButton.gameObject.SetActive(false);
            OnInventoryButton.gameObject.SetActive(true);
            offInventroy();
        });
        OffItemDetailButton.onClick.AddListener(() => { itemDetailPanel.SetActive(false); });
        LoadInventory();

    }
    void onInventory()
    {
        inventoryPanel.SetActive(true);
    }
    void offInventroy()
    {
        inventoryPanel.SetActive(false);
    }
    public void LoadInventory()
    {
        ClearInventoryUI();

        foreach (var item in inventoryItems)
        {
            CreateItemSlot(item);
        }
    }

    void ClearInventoryUI()
    {
        foreach (Transform child in itemGridParent)
        {
            Destroy(child.gameObject);
        }
    }

    void ShowItemDetail(ItemData item)
    {
        selectedItem = item;
        itemDetailPanel.SetActive(true);

        itemNameText.text = item.Item_Name;
        itemStatText.text = GetStatText(item);
        itemOptionText.text = GetOptionText(item);
        itemDescText.text = item.Description;

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
        if (item.Item_Type == "Weapon")
            return $"공격력: {item.Weapon_DMG}";
        else if (item.Item_Type == "Armor")
            return $"방어력: {item.Armor_DEF}, 체력: {item.Armor_HP}";
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
        if (!string.IsNullOrEmpty(item.Option_1_ID))
            options.Add($"{item.Option_1_ID} +{item.Option_Value1}");
        if (!string.IsNullOrEmpty(item.Option_2_ID))
            options.Add($"{item.Option_2_ID} +{item.Option_Value2}");
        return string.Join("\n", options);
    }

    public void OnClickEquip()
    {
        if (selectedItem == null)
        {
            Debug.Log("selectedItem이 없습니다");
            return;
        }

        if (selectedItem.Item_Type == "Weapon")
        {
            player.weapon_Name = selectedItem.Item_ID;
            Debug.Log("장착(무기) 버튼을 눌렀습니다!");
        }
        else if (selectedItem.Item_Type == "Armor")
        {
            player.armor_Name = selectedItem.Item_ID;
            Debug.Log("장착(방어구) 버튼을 눌렀습니다!");
        }
            

        equipmentSystem.Init();
        ShowItemDetail(selectedItem);
    }

    public void OnClickUnequip()
    {
        if (selectedItem == null) return;

        if (selectedItem.Item_Type == "Weapon")
        {
            if (jsonManager != null)
            {
                var weapon = jsonManager.GetWeaponMasters("Weapon_Master")
                    .FirstOrDefault(w => w.Weapon_ID == selectedItem.Item_ID);
                if (weapon != null)
                {
                    int weaponDamage = (int)(weapon.Weapon_DMG +
                        playerState.Strength * weapon.STR_Scaling +
                        playerState.DEX * weapon.DEX_Scaling +
                        playerState.Int * weapon.INT_Scaling +
                        playerState.MAG * weapon.MAG_Scaling +
                        playerState.Charisma * weapon.CHR_Scaling +
                        playerState.Divinity * weapon.DIV_Scaling);
                    player.damage -= weaponDamage;
                }
                Debug.Log("무기를 장착 해제 했습니다");
            }
            player.weapon_Name = null;
        }
        else if (selectedItem.Item_Type == "Armor")
        {
            if (jsonManager != null)
            {
                var armor = jsonManager.GetArmorMasters("Armor_Master")
                    .FirstOrDefault(a => a.Armor_ID == selectedItem.Item_ID);
                if (armor != null)
                {
                    player.MaxHealth -= armor.Armor_HP;
                }
                Debug.Log("방어구를 장착 해제 했습니다");
            }
            player.armor_Name = null;
        }

        player.OnHitOptions.RemoveAll(opt => opt.item_ID == selectedItem.Item_ID);
        equipmentSystem.Init();
        ShowItemDetail(selectedItem);
    }

    public void OnClickUse()
    {
        if (selectedItem == null || selectedItem.Item_Type != "Consumable") return;

        // 스토리용 체력/정신력 회복
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

    public void AddItemToInventory(ItemData newItem)
    {
        inventoryItems.Add(newItem);
        CreateItemSlot(newItem);
    }
    private void CreateItemSlot(ItemData item)
    {
        var slotGO = Instantiate(itemSlotPrefab, itemGridParent);
        var slotUI = slotGO.GetComponent<ItemSlotUI>();
        if (slotUI != null)
        {
            slotUI.Setup(item, ShowItemDetail);
        }
    }
}



// 참고용 아이템 데이터 클래스
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
