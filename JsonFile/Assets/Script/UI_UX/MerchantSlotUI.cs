using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class MerchantSlotUI : MonoBehaviour
{
    //[SerializeField] Image icon;
    //[SerializeField] TMP_Text nameText;
    //[SerializeField] TMP_Text priceText;
    [SerializeField] Button buyButton;
    public SpriteBank spriteBank;
    MerchantItem _data;
    Image _icon;
    TMP_Text _priceText;


    public void Awake()
    {
        if (spriteBank == null)
            spriteBank = FindObjectOfType<SpriteBank>();

    }
    public void Setup(MerchantItem bs, Action<MerchantItem> onClick)
    {
        _data = bs;
        //nameText.text = bs.Weapon_Name;
        //priceText.text = $"{bs.Item_Price:0.##}G";
        // 아이콘은 SpriteBank 로 로드하거나 미리 연결해두세요.
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => onClick(_data));
    }

    public void MarkSold()
    {
        _data = null;
        buyButton.interactable = false;
    }
}