using System;
using System.Collections.Generic;
using UnityEngine;

public static class ItemDataFactory
{
    public static ItemData FromWeapon(Weapon_Master weapon)
    {
        if (weapon == null) return null;

        return new ItemData
        {
            Item_ID = weapon.Weapon_ID,
            Item_Name = weapon.Weapon_Name,
            Item_Type = string.IsNullOrEmpty(weapon.ItemType) ? "Weapon" : weapon.ItemType,
            Weapon_DMG = weapon.Weapon_DMG,
            One_Handed = weapon.One_Handed.ToString(),
            Option_1_ID = NormalizeOptionId(weapon.Option_1_ID),
            Option_Value1 = weapon.Option_Value1,
            Option_2_ID = NormalizeOptionId(weapon.Option_2_ID),
            Option_Value2 = weapon.Option_Value2,
            Description = weapon.Description,
            Item_Price = weapon.Item_Price
        };
    }

    public static ItemData FromArmor(Armor_Master armor)
    {
        if (armor == null) return null;

        return new ItemData
        {
            Item_ID = armor.Armor_ID,
            Item_Name = armor.Armor_NAME,
            Item_Type = string.IsNullOrEmpty(armor.ItemType) ? "Armor" : armor.ItemType,
            Armor_DEF = armor.Armor_DEF,
            Armor_HP = armor.Armor_HP,
            Option_1_ID = NormalizeOptionId(armor.Armor_Option1),
            Option_Value1 = armor.Option1_Value,
            Option_2_ID = NormalizeOptionId(armor.Armor_Option2),
            Option_Value2 = armor.Option2_Value,
            Description = armor.Description,
            Item_Price = armor.Item_Price
        };
    }

    public static ItemData FromItem(Item_Master item)
    {
        if (item == null) return null;

        return new ItemData
        {
            Item_ID = item.Item_ID,
            Item_Name = item.Item_NAME,
            Item_Type = string.IsNullOrEmpty(item.ItemType) ? "Item" : item.ItemType,
            Option_1_ID = NormalizeOptionId(item.Item_Option1),
            Option_Value1 = item.Option1_Value,
            Option_2_ID = NormalizeOptionId(item.Item_Option2),
            Option_Value2 = item.Option2_Value,
            Description = item.Item_Description,
            Item_Price = item.Item_Price,
            Heal_Value = 0,
            Mental_Heal_Value = 0
        };
    }

    public static ItemData FromMerchantItem(MerchantItem merchantItem, JsonManager jsonManager)
    {
        if (merchantItem == null) return null;

        ItemData item = FromCode(jsonManager, merchantItem.Item_ID) ?? new ItemData
        {
            Item_ID = merchantItem.Item_ID,
            Item_Type = merchantItem.Item_Type,
            Item_Name = merchantItem.Item_Name
        };

        if (string.IsNullOrEmpty(item.Item_Type) && !string.IsNullOrEmpty(merchantItem.Item_Type))
            item.Item_Type = merchantItem.Item_Type;
        if (string.IsNullOrEmpty(item.Item_Name) && !string.IsNullOrEmpty(merchantItem.Item_Name))
            item.Item_Name = merchantItem.Item_Name;
        item.Item_Price = merchantItem.Item_Price;

        if (HasOption(merchantItem.Item_Option_1))
        {
            item.Option_1_ID = NormalizeOptionId(merchantItem.Item_Option_1);
            item.Option_Value1 = merchantItem.Item_Option_value1;
        }

        if (HasOption(merchantItem.Item_Option_2))
        {
            item.Option_2_ID = NormalizeOptionId(merchantItem.Item_Option_2);
            item.Option_Value2 = merchantItem.Item_Option_value2;
        }

        return item;
    }

    public static ItemData FromCode(JsonManager jsonManager, string itemCode)
    {
        if (jsonManager == null || string.IsNullOrEmpty(itemCode)) return null;

        if (itemCode.StartsWith("Weapon_", StringComparison.OrdinalIgnoreCase))
            return FromWeapon(jsonManager.GetWeaponById(itemCode));

        if (itemCode.StartsWith("Armor_", StringComparison.OrdinalIgnoreCase))
            return FromArmor(jsonManager.GetArmorById(itemCode));

        if (itemCode.StartsWith("Item_", StringComparison.OrdinalIgnoreCase))
            return FromItem(jsonManager.GetItemMasterById(itemCode));

        return null;
    }

    public static bool ApplyMasterData(ItemData item, JsonManager jsonManager)
    {
        if (item == null || jsonManager == null || string.IsNullOrEmpty(item.Item_ID))
            return false;

        ItemData master = FromCode(jsonManager, item.Item_ID);
        if (master == null) return false;

        string savedType = item.Item_Type;
        Copy(master, item);
        if (!string.IsNullOrEmpty(savedType) && string.IsNullOrEmpty(item.Item_Type))
            item.Item_Type = savedType;
        return true;
    }

    public static bool TryGetOptionValues(ItemData item, out string option1, out int value1, out string option2, out int value2)
    {
        option1 = NormalizeOptionId(item?.Option_1_ID);
        value1 = item?.Option_Value1 ?? 0;
        option2 = NormalizeOptionId(item?.Option_2_ID);
        value2 = item?.Option_Value2 ?? 0;
        return HasOption(option1) || HasOption(option2);
    }

    public static string GetStatText(ItemData item)
    {
        if (item == null) return string.Empty;

        if (IsType(item, "Weapon"))
            return $"공격력: {item.Weapon_DMG}";

        if (IsType(item, "Armor"))
            return $"방어력: {item.Armor_DEF}, 체력: {item.Armor_HP}";

        if (IsType(item, "Consumable"))
        {
            var effects = new System.Collections.Generic.List<string>();
            if (item.Heal_Value > 0) effects.Add($"체력 회복: {item.Heal_Value}");
            if (item.Mental_Heal_Value > 0) effects.Add($"정신력 회복: {item.Mental_Heal_Value}");
            return string.Join(", ", effects);
        }

        return string.Empty;
    }

    public static bool HasOption(string optionId)
    {
        return !string.IsNullOrWhiteSpace(optionId) &&
               !string.Equals(optionId, "null", StringComparison.OrdinalIgnoreCase);
    }

    public static string NormalizeOptionId(string optionId)
    {
        return HasOption(optionId) ? optionId.Trim() : null;
    }

    public static IEnumerable<string> GetIconKeys(ItemData item, string fallback = "UI_InventorySlot 1")
    {
        if (!string.IsNullOrWhiteSpace(item?.Icon))
            yield return item.Icon.Trim();
        if (!string.IsNullOrWhiteSpace(item?.Item_ID))
            yield return item.Item_ID.Trim();
        if (!string.IsNullOrWhiteSpace(item?.Item_Name))
            yield return item.Item_Name.Trim();
        if (!string.IsNullOrWhiteSpace(fallback))
            yield return fallback;
    }

    private static bool IsType(ItemData item, string itemType)
    {
        return string.Equals(item?.Item_Type, itemType, StringComparison.OrdinalIgnoreCase);
    }

    private static void Copy(ItemData source, ItemData target)
    {
        target.Item_ID = source.Item_ID;
        target.Item_Type = source.Item_Type;
        target.Item_Name = source.Item_Name;
        target.Weapon_DMG = source.Weapon_DMG;
        target.Armor_DEF = source.Armor_DEF;
        target.Armor_HP = source.Armor_HP;
        target.One_Handed = source.One_Handed;
        target.Heal_Value = source.Heal_Value;
        target.Mental_Heal_Value = source.Mental_Heal_Value;
        target.Option_1_ID = source.Option_1_ID;
        target.Option_Value1 = source.Option_Value1;
        target.Option_2_ID = source.Option_2_ID;
        target.Option_Value2 = source.Option_Value2;
        target.Description = source.Description;
        target.Icon = source.Icon;
        target.Item_Price = source.Item_Price;
    }
}
