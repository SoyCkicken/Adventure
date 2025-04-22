public class Weapon_Master 
{
    //Weapon_Master.Json
    //아이템 아이디
    public string Weapon_ID;
    //아이템 이름
    public string Weapon_Name;
    //아이템 태그
    public string Weapon_Type;
    //아이템 데미지
    public int Weapon_DMG;
    //능력치 배율 -일 경우 곱하지 않고 1.2배 이렇게 수정할 예정임
    public float STR_Scaling;
    public float DEX_Scaling;
    public float INT_Scaling;
    public float MAG_Scaling;
    public float DIV_Scaling;
    public float CHR_Scaling;
    //첫번째 옵션의 아이디
    public string Option_1_ID;
    //첫번째 옵션의 값
    public int Option_Value1;
    //두번째 옵션의 아이디
    public string Option_2_ID;
    //두번째 옵션의 값
    public int Option_Value2;
    //아이템 설명
    public string Description;
}
