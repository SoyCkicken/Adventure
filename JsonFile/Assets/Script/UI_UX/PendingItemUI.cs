using UnityEngine;
using TMPro;

public class PendingItemUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public TextMeshProUGUI stateText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI optinonText;

    private ItemData data;

    public void Setup(ItemData item, JsonManager jsonManager)
    {
        data = item;

        // 이름 출력 (소비형은 이름 포함, 장비는 마스터에서 가져올 수 있음)
        nameText.text = string.IsNullOrEmpty(item.Item_Name) ? item.Item_ID : item.Item_Name;

        // 설명 출력 처리
        switch (item.Item_Type)
        {
            case "Weapon":
                var weapon = jsonManager.GetWeaponById(item.Item_ID);
                nameText.text = weapon?.Weapon_Name ?? "알수없는 무기";
                stateText.text = weapon?.Weapon_DMG.ToString() ?? "알수없는 데미지";
                typeText.text = weapon?.ItemType ?? "알수없는 무기 타입";
                descText.text = weapon?.Description ?? "무기 정보 없음";
                if (weapon != null && ItemDataFactory.HasOption(weapon.Option_1_ID))
                {
                    optinonText.text += $"{weapon.Option_1_ID} : {weapon.Option_Value1}\n";
                }
                if (weapon != null && ItemDataFactory.HasOption(weapon.Option_2_ID))
                {
                    optinonText.text += $"{weapon.Option_2_ID} : {weapon.Option_Value2}";
                }
                break;

            case "Armor":
                var armor = jsonManager.GetArmorById(item.Item_ID);
                nameText.text = armor?.Armor_ID ?? "알수없는 방어구";
                stateText.text = armor?.Armor_HP.ToString() ?? "알수없는 체력값";
                typeText.text = armor?.ItemType ?? "알수없는 방어구 타입";
                descText.text = armor?.Description ?? "방어구 정보 없음";
                if (armor != null && ItemDataFactory.HasOption(armor.Armor_Option1))
                {
                    optinonText.text += $"{armor.Armor_Option1} : {armor.Option1_Value}\n";
                }
                if (armor != null && ItemDataFactory.HasOption(armor.Armor_Option2))
                {
                    optinonText.text += $"{armor.Armor_Option2} : {armor.Option2_Value}";
                }
                break;

            case "Consumable":
                descText.text = item.Description ?? "설명이 없는 소비 아이템";
                break;

            default:
                descText.text = "알 수 없는 아이템 유형";
                break;
        }
    }
}

