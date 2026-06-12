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
        spriteBank = SpriteBank.Instance;
    }
public void Setup(ItemData item, System.Action<ItemData> onClick)
    {
        data = item;
        CurrentItem = item;
        onClickCallback = onClick;
        if (spriteBank == null)
            spriteBank = SpriteBank.Instance ?? FindObjectOfType<SpriteBank>(true);
        if (data != null)
        {
            if (icon == null)
            {
                Debug.LogError("[ItemSlotUI] icon(Image)가 에디터에 연결되지 않았습니다.");
                return;
            }
            foreach (string spriteName in ItemDataFactory.GetIconKeys(data))
            {
                if (spriteBank != null && spriteBank.TryLoad(spriteName, out Sprite s))
                {
                    icon.sprite = s;
                    return;
                }
            }
        }
        else
        {
            Debug.Log("이미지가 없어서 여기 들어와졌습니다");
            if (spriteBank != null && spriteBank.TryLoad("UI_InventorySlot 1", out Sprite t))
                icon.sprite = t;
        }
    }
public void Clear()
    {
        data = null;
        CurrentItem = null;
        icon.sprite = null;
        onClickCallback = null;
        if (spriteBank == null)
            spriteBank = SpriteBank.Instance ?? FindObjectOfType<SpriteBank>(true);
        switch (slotType)
        {
            case SlotType.RWeapon:
                if (spriteBank != null && spriteBank.TryLoad("UI_EquipmentSlot_RightHand", out var rightHandSprite))
                    icon.sprite = rightHandSprite;
                break;
            case SlotType.LWeapon:
                if (spriteBank != null && spriteBank.TryLoad("UI_EquipmentSlot_LeftHand", out var leftHandSprite))
                    icon.sprite = leftHandSprite;
                break;
            case SlotType.Armor:
                if (spriteBank != null && spriteBank.TryLoad("UI_EquipmentSlot_Armor 1", out var armorSprite))
                    icon.sprite = armorSprite;
                break;
        }
    }

    public void OnClick()
    {
        onClickCallback?.Invoke(data);
    }
}