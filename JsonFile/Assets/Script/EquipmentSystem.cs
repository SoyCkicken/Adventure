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
        //player.weapon_Name = null;
        //player.armor_Name = null;
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
        Debug.Log($"무기 = {weapon != null}");
        // 무기 장착 처리
        if (weapon != null)
        {
            int tempDamage = Convert.ToInt32(weapon.Weapon_DMG + (playerState.GetStat("Strength") * weapon.STR_Scaling)
                + (playerState.GetStat("Agility") * weapon.DEX_Scaling)
                + (playerState.GetStat("Intelligence") * weapon.INT_Scaling)
                + (playerState.GetStat("Magic") * weapon.MAG_Scaling)
                + (playerState.GetStat("Charisma") * weapon.CHR_Scaling)
                + (playerState.GetStat("Divine") * weapon.DIV_Scaling));
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
    void ClearInit()
    {
        Debug.LogError("플레이어 능력치 초기화");
        player.OnHitOptions.Clear();
        //player.weapon_Name = null;
        //player.armor_Name = null;
        player.MaxHealth = 50;
        player.damage = 10;
        player.armor = 10;
        player.speed = 1.5f;
        player.CitChance = 10;
    }
}

