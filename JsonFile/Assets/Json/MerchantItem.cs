[System.Serializable]
public class MerchantItem
{
    public string Item_ID;        // 무기, 방어구, 소비 아이템 등 공통 ID
    public string Item_Type;      // "Weapon", "Armor", "Consumable" 등으로 구분
    public string Item_Name;      // 이름
    public int Item_Price;        // 가격
    public string Item_Option_1;
    public int Item_Option_value1;
    public string Item_Option_2;
    public int Item_Option_value2;
}