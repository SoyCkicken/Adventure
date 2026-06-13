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
    private Image equippedItemIcon;

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

            if (IsEquipmentSlot())
            {
                SetSlotBackground();
                Image itemIcon = EnsureEquippedItemIcon();
                foreach (string spriteName in ItemDataFactory.GetIconKeys(data))
                {
                    if (spriteBank != null && spriteBank.TryLoad(spriteName, out Sprite equipmentSprite))
                    {
                        itemIcon.sprite = equipmentSprite;
                        itemIcon.enabled = true;
                        return;
                    }
                }

                itemIcon.enabled = false;
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
        if (icon != null)
            icon.sprite = null;
        onClickCallback = null;
        if (spriteBank == null)
            spriteBank = SpriteBank.Instance ?? FindObjectOfType<SpriteBank>(true);
        if (equippedItemIcon != null)
        {
            equippedItemIcon.sprite = null;
            equippedItemIcon.enabled = false;
        }
        SetSlotBackground();
    }

    private bool IsEquipmentSlot()
    {
        return slotType == SlotType.RWeapon || slotType == SlotType.LWeapon || slotType == SlotType.Armor;
    }

    private Image EnsureEquippedItemIcon()
    {
        if (equippedItemIcon != null)
            return equippedItemIcon;

        var go = new GameObject("EquippedItemIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(icon.transform, false);

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = GetEquipmentIconSize();

        equippedItemIcon = go.GetComponent<Image>();
        equippedItemIcon.raycastTarget = false;
        equippedItemIcon.preserveAspect = true;
        equippedItemIcon.enabled = false;
        return equippedItemIcon;
    }

    private Vector2 GetEquipmentIconSize()
    {
        RectTransform rect = icon != null ? icon.rectTransform : null;
        if (rect == null) return new Vector2(72f, 72f);

        Vector2 size = rect.rect.size;
        if (size.x <= 0f || size.y <= 0f)
            size = rect.sizeDelta;
        if (size.x <= 0f || size.y <= 0f)
            size = new Vector2(96f, 96f);

        return size * 0.72f;
    }

    private void SetSlotBackground()
    {
        if (icon == null) return;
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
            default:
                if (spriteBank != null && spriteBank.TryLoad("UI_InventorySlot 1", out var inventorySlotSprite))
                    icon.sprite = inventorySlotSprite;
                break;
        }
    }

    public void OnClick()
    {
        onClickCallback?.Invoke(data);
    }
}
