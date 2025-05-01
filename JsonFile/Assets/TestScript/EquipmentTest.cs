//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System.Linq;
//using MyGame;

//public class EquipmentTest : MonoBehaviour
//{
//    public JsonManager jsonManager;
//    public Character character;

//    private Weapon_Master equippedWeapon;
//    private Armor_Master equippedArmor;

//    private void Start()
//    {
//        if(jsonManager==null) jsonManager = FindObjectOfType<JsonManager>();
//        if (character == null) character = FindObjectOfType<Character>();

//        EquippedWeapon(character.weapon_Name);
//        EquipedArmor(jsonManager.Armor_Master.FirstOrDefault());
//    }

//    void EquippedWeapon(Weapon_Master weapon)
//    {
//        if (weapon == null)
//        {
//            Debug.Log("무기 없음!");
//            return;
//        }
//        equippedWeapon = weapon;
//        Debug.Log($"장착 중 인 무기 {equippedWeapon.Weapon_Name} ");
//        Debug.Log($"공격력 : {equippedWeapon.Weapon_DMG}");
//        if (!string.IsNullOrEmpty(weapon.Option_1_ID))
//        LogOption(weapon.Option_1_ID, weapon.Option_Value1);
//        if (!string.IsNullOrEmpty(weapon.Option_2_ID))
//            LogOption(weapon.Option_2_ID, weapon.Option_Value2);
//    }
//    public void EquipedArmor(Armor_Master armor)
//    {
//        if (armor == null)
//        {
//            Debug.LogWarning("장착할 방어구가 없습니다.");
//            return;
//        }

//        equippedArmor = armor;
//        Debug.Log($"▶ 방어구 장착: {armor.Armor_Name}");
//        Debug.Log($"   • 방어력: {armor.Armor_DEF}");
//        Debug.Log($"   • 체력 보너스: {armor.Armor_HP}");

//        // 옵션 1
//        if (!string.IsNullOrEmpty(armor.Armor_Option1))
//            LogOption(armor.Armor_Option1, armor.Option1_Value);

//        // 옵션 2
//        if (!string.IsNullOrEmpty(armor.Armor_Option2))
//            LogOption(armor.Armor_Option2, armor.Option2_Value);
//    }


//    void LogOption(string option_id, int option_value)
//    {
//        var opt = jsonManager.Item_Options.FirstOrDefault(o => o.Option_ID == option_id);
//        if (opt != null)
//        {
//            Debug.Log($"$ 옵션 : {opt.Option_Description}  : {option_value}");
//        }
//        else 
//        {
//            Debug.Log("해당 옵션을 찾을수가 없었습니다");
//        }
//    }
//}
