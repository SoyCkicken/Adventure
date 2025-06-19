using UnityEngine;
using System.Linq;
using MyGame;
using UnityEngine.UI;
using UnityEditor;
using System;  // Character, OptionContext 등이 있는 네임스페이스

public class EquipmentSystem : MonoBehaviour
{
    public PlayerState playerState;
    public JsonManager jsonManager;
    public Character player;

    private void Start()
    {
        OptionManager.Initialize(jsonManager);
        Init();
    }

    public void Init()
    {
        ClearInit();
        // 자동 참조
        if (jsonManager == null)
            jsonManager = FindObjectOfType<JsonManager>();
        var weapon = jsonManager.GetWeaponMasters("Weapon_Master")
                          .FirstOrDefault(w => w.Weapon_ID == player.weapon_Name);
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
    void ClearInit()
    {
        player.OnHitOptions.Clear();
        player.MaxHealth = 50;
        player.armor = 5;
        player.speed = 1.5f;
        player.CitChance = 10;
    }
}

