using UnityEngine;
using System.Linq;
using MyGame;
using UnityEngine.UI;
using UnityEditor;
using System;
using System.Collections.Generic;  // Character, OptionContext 등이 있는 네임스페이스

public class EquipmentSystem : MonoBehaviour
{
    public PlayerState playerState;
    public JsonManager jsonManager;
    public Character player;

    private void Start()
    {
        OptionManager.Initialize(jsonManager);
        playerState = PlayerState.Instance;
        //player.weapon_Name = null;
        //player.armor_Name = null;
        Init();
    }

    public void Init()
    {
        if (jsonManager == null)
            jsonManager = FindObjectOfType<JsonManager>();
        if (playerState == null)
            playerState = FindObjectOfType<PlayerState>();
        ClearInit();
        // 자동 참조
        
        var weapon = jsonManager.GetWeaponMasters("Weapon_Master")
                          .FirstOrDefault(w => w.Weapon_ID == player.weapon_Name);
        Debug.Log($"무기 = {weapon != null}");
        // 무기 장착 처리
        if (weapon != null)
        {
            int tempDamage = Convert.ToInt32(weapon.Weapon_DMG + (playerState.STR * weapon.STR_Scaling)
                + (playerState.AGI * weapon.DEX_Scaling)
                + (playerState.INT * weapon.INT_Scaling)
                + (playerState.MAG * weapon.MAG_Scaling)
                + (playerState.CHA * weapon.CHR_Scaling)
                + (playerState.DIV * weapon.DIV_Scaling));
            player.damage = tempDamage;
            // 옵션 리스트에 추가
            if (!string.IsNullOrEmpty(weapon.Option_1_ID))
                OptionManager.ApplyOption(weapon.Option_1_ID, new OptionContext
                {
                    User = player,
                    Value = weapon.Option_Value1,
                    item_ID = weapon.Weapon_ID,
                    option_ID = weapon.Option_1_ID
                });
            if (!string.IsNullOrEmpty(weapon.Option_2_ID))
                OptionManager.ApplyOption(weapon.Option_2_ID, new OptionContext
                {
                    User = player,
                    Value = weapon.Option_Value2,
                    item_ID = weapon.Weapon_ID,
                    option_ID = weapon.Option_2_ID
                });
        }
            var armor = jsonManager.GetArmorMasters("Armor_Master")
                             .FirstOrDefault(w => w.Armor_ID == player.armor_Name);
        Debug.Log($"방어구 = {armor != null}");
        // 방어구 장착 처리
        if (armor != null)
        {
            player.armor = armor.Armor_DEF;
            player.MaxHealth = armor.Armor_HP;
            // 옵션 리스트에 추가
            if (!string.IsNullOrEmpty(armor.Armor_Option1))
                OptionManager.ApplyOption(armor.Armor_Option1, new OptionContext
                {
                    User = player,
                    Value = armor.Option1_Value,
                    item_ID = armor.Armor_ID,
                    option_ID = armor.Armor_Option1
                });
            if (!string.IsNullOrEmpty(armor.Armor_Option2))
                OptionManager.ApplyOption(armor.Armor_Option2, new OptionContext
                {
                    User = player,
                    Value = armor.Option2_Value,
                    item_ID = armor.Armor_ID,
                    option_ID = armor.Armor_Option2
                });
        }
    }
    public void EquipItem(
    ItemData item,
    List<ItemData> inventoryItems,
    ItemSlotUI weaponSlot,
    ItemSlotUI armorSlot,
    Action<ItemData> onClick)
    {
        if (item.Item_Type == "Weapon")
        {
            if (weaponSlot.CurrentItem != null)
                inventoryItems.Add(weaponSlot.CurrentItem.Clone());

            weaponSlot.Setup(item, onClick);  // ✅ 콜백 전달
            inventoryItems.Remove(item);
            player.weapon_Name = item.Item_ID;
        }
        else if (item.Item_Type == "Armor")
        {
            if (armorSlot.CurrentItem != null)
                inventoryItems.Add(armorSlot.CurrentItem.Clone());

            armorSlot.Setup(item, onClick);  // ✅ 콜백 전달
            inventoryItems.Remove(item);
            player.armor_Name = item.Item_ID;
        }

        Init();
    }

    public void UnequipItem(ItemSlotUI slot, List<ItemData> inventoryItems)
    {
        if (slot.CurrentItem == null) return;

        inventoryItems.Add(slot.CurrentItem.Clone());

        if (slot.CurrentItem.Item_Type == "Weapon")
            player.weapon_Name = null;
        else if (slot.CurrentItem.Item_Type == "Armor")
            player.armor_Name = null;
        if (!string.IsNullOrEmpty(slot.CurrentItem.Option_1_ID))
            player.RemoveBuffByItem(slot.CurrentItem.Item_ID);
        slot.Clear();
        Init();
    }


    void ClearInit()
    {
        Debug.LogError("플레이어 능력치 초기화");
        player.OnHitOptions.Clear();
        //player.weapon_Name = null;
        //player.armor_Name = null;
        //
        if (playerState.Health >= 10)
        {
            player.MaxHealth = playerState.Health* 5;
        }
        else
        {
            player.MaxHealth = 50;
        }
            
        if (playerState.STR >= 10)
        {
            player.damage = playerState.STR;
        }
        else
        {
            player.damage = 10;
        }
        player.armor = 3;

        if (playerState.AGI >= 10)
        {
            player.speed = 0.15f * playerState.AGI;
        }
        else
        {
            player.speed = 1.5f;
        }
        if (playerState.INT >= 10)
        {
            int tempcri = playerState.INT - 10;
            player.CitChance = Convert.ToInt32(10  + (2.5f * tempcri)); // <-- 11이상 부터 크리티컬 확률 증가
        }
        else
        {
            player.CitChance = 10;
        }
            
    }
}

