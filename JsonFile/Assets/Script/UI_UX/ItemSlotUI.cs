using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class ItemSlotUI : MonoBehaviour
{
    public enum SlotType { Normal, RWeapon, LWeapon, Armor }
    public SlotType slotType;
    public Image icon;
    public Button button;
    private ItemData data;
    private System.Action<ItemData> onClickCallback;
    public SpriteBank spriteBank;
    public ItemData CurrentItem { get; set; }
    private void Awake()
    {
        button = this.GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }
    private void Start()
    {
        if (spriteBank == null)
            spriteBank = FindObjectOfType<SpriteBank>();
    }

    public void Setup(ItemData item, System.Action<ItemData> onClick)
    {
        data = item;
        CurrentItem = item;
        onClickCallback = onClick;
        if (spriteBank == null)
            spriteBank = FindObjectOfType<SpriteBank>();
        if (!string.IsNullOrEmpty(data.Item_Name))
        {
            Debug.Log(data.Item_Name);
            if (icon == null)
            {
                Debug.LogError("[ItemSlotUI] icon(Image)가 에디터에 연결되지 않았습니다.");
                return;
            }
            Sprite s = spriteBank.Load(data.Item_Name);
            if (s != null)
            {
                icon.sprite = s;
            }
        }
        else
        {
            Debug.Log("이미지가 없어서 여기 들어와졌습니다");
            Sprite t = spriteBank.Load("UI_InventorySlot 1");
            icon.sprite = t;
        }
    }
    public void Clear()
    {
        data = null;
        CurrentItem = null;
        //icon.sprite = spriteBank.Load("UI_InventorySlot 1");
        icon.sprite = null;
        onClickCallback = null;
        switch (slotType)
        {
            case SlotType.RWeapon:
                icon.sprite = spriteBank.Load("UI_EquipmentSlot_RightHand");
                break;
            case SlotType.LWeapon:
                icon.sprite = spriteBank.Load("UI_EquipmentSlot_LeftHand");
                break;
            case SlotType.Armor:
                icon.sprite = spriteBank.Load("UI_EquipmentSlot_Armor 1");
                break;
            //default:
            //    icon.sprite = spriteBank.Load("UI_InventorySlot 1");
            //    break;
        }
    }

    public void OnClick()
    {
        onClickCallback?.Invoke(data);
    }
}