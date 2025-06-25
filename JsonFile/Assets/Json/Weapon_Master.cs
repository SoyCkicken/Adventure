using System;
using System.Collections.Generic;
[Serializable]
public class Weapon_Master
{
    public string Weapon_ID;
    public string Weapon_Name;
    public int Weapon_DMG;
    public float STR_Scaling;
    public float DEX_Scaling;
    public float INT_Scaling;
    public float MAG_Scaling;
    public float DIV_Scaling;
    public float CHR_Scaling;
    public string Option_1_ID;
    public int Option_Value1;
    public string Option_2_ID;
    public int Option_Value2;
    public string Description;
    public string ItemType;
    public bool One_Handed;
    public int Item_Price;
}
