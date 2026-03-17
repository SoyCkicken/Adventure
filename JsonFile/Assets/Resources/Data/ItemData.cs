using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class ItemOption
{
    public string OptionID; // 예: "BURN_01", "CRIT_UP"
    public int Value;       // 수치
}
[System.Serializable]
public class ItemData // : MonoBehaviour를 삭제했습니다.
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
    public string Description;
    public string Icon;
    public int Item_Price;
    public List<ItemOption> Options = new List<ItemOption>();

    public ItemData Clone()
    {
        // 1. 새 객체 생성
        ItemData newItem = new ItemData();

        // 2. 일반 값 타입 변수 복사
        newItem.Item_ID = this.Item_ID;
        newItem.Item_Type = this.Item_Type;
        newItem.Item_Name = this.Item_Name;
        newItem.Weapon_DMG = this.Weapon_DMG;
        newItem.Armor_DEF = this.Armor_DEF;
        newItem.Armor_HP = this.Armor_HP;
        newItem.One_Handed = this.One_Handed;
        newItem.Heal_Value = this.Heal_Value;
        newItem.Mental_Heal_Value = this.Mental_Heal_Value;
        newItem.Description = this.Description;
        newItem.Icon = this.Icon;
        newItem.Item_Price = this.Item_Price;

        // 3. 리스트 깊은 복사 (알맹이까지 새로 생성)
        if (this.Options != null)
        {
            newItem.Options = new List<ItemOption>();
            foreach (var opt in this.Options)
            {
                newItem.Options.Add(new ItemOption { OptionID = opt.OptionID, Value = opt.Value });
            }
        }

        return newItem;
    }
}
