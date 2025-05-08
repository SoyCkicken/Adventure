//using UnityEngine;
//using System.Linq;
//using MyGame;
//using UnityEngine.UI;
//using UnityEditor;  // Character, OptionContext 등이 있는 네임스페이스

//public class EquipmentSystem : MonoBehaviour
//{
//    public JsonManager jsonManager;
//    public Character player;
//    public Character enemy;

//    void Start()
//    {
//        // 자동 참조
//        if (jsonManager == null)
//            jsonManager = FindObjectOfType<JsonManager>();
//        if (player == null)
//            player = FindObjectOfType<Character>();
//        var weapon = jsonManager.Weapon_Masters
//                          .FirstOrDefault(w => w.Weapon_ID == player.weapon_Name);
//        // 무기 장착 처리
//        if (weapon != null)
//        {
//            player.damage += weapon.Weapon_DMG;
//            // 옵션 리스트에 추가
//            if (!string.IsNullOrEmpty(weapon.Option_1_ID))
//                player.OnHitOptions.Add(new Character.EquippedOption
//                {
//                    OptionID = weapon.Option_1_ID,
//                    Value = weapon.Option_Value1,
                    
//                    item_ID = weapon.Weapon_Name

//                });
//            if (!string.IsNullOrEmpty(weapon.Option_2_ID))
//                player.OnHitOptions.Add(new Character.EquippedOption
//                {
//                    OptionID = weapon.Option_2_ID,
//                    Value = weapon.Option_Value2,
//                    item_ID= weapon.Weapon_Name
//                });
//        }
//        var armor = jsonManager.Armor_Master
//                         .FirstOrDefault(w => w.Armor_ID == player.armor_Name);
//        // 방어구 장착 처리
//        if (armor != null)
//        {
//            player.armor += armor.Armor_DEF;
//            player.Health += armor.Armor_HP;
//            // 옵션 리스트에 추가
//            if (!string.IsNullOrEmpty(armor.Armor_Option1))
//                player.OnHitOptions.Add(new Character.EquippedOption
//                {
//                    OptionID = armor.Armor_Option1,
//                    Value = armor.Option1_Value,
//                    item_ID = armor.Armor_Name
//                });
//            if (!string.IsNullOrEmpty(armor.Armor_Option2))
//                player.OnHitOptions.Add(new Character.EquippedOption
//                {
//                    OptionID = armor.Armor_Option2,
//                    Value = armor.Option2_Value,
//                    item_ID = armor.Armor_ID
//                });
//        }

//        //void ApplyWeaponOptions(Weapon_Master w)
//        //{
//        //    // 옵션 1
//        //    if (!string.IsNullOrEmpty(w.Option_1_ID))
//        //    {
//        //        //var ctx = new OptionContext
//        //        //{
//        //        //    User = player,
//        //        //    Target = player,           // 패시브 옵션이라면 자기 자신을 타겟으로
//        //        //    Value = w.Option_Value1,
//        //        //    // DamageDealt, TurnNumber 등은 옵션에 따라 채워주면 됨
//        //        //};
//        //        //optionManager.ApplyOption(w.Option_1_ID, ctx);
//        //        player.Option1_ID = w.Option_1_ID;
//        //        player.Option1_Value = w.Option_Value1;
//        //    }

//        //    // 옵션 2
//        //    if (!string.IsNullOrEmpty(w.Option_2_ID))
//        //    {
//        //        //var ctx = new OptionContext
//        //        //{
//        //        //    User = player,
//        //        //    Target = player,
//        //        //    Value = w.Option_Value2,
//        //        //};
//        //        //optionManager.ApplyOption(w.Option_2_ID, ctx);
//        //        player.Option2_ID = w.Option_2_ID;
//        //        player.Option2_Value = w.Option_Value2;
//        //    }
//    }
//}

