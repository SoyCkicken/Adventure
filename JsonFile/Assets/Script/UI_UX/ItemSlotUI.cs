using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    public Image icon;
    public Button button;
    private ItemData data;
    private System.Action<ItemData> onClickCallback;

    private void Awake()
    {
        button = this.GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    public void Setup(ItemData item, System.Action<ItemData> onClick)
    {
        data = item;
        //이미지 지금 없음!
        onClickCallback = onClick;
        //icon.sprite = Resources.Load<Sprite>($"Icons/{item.Icon}");
        Debug.Log("아이템 슬롯의 SetUp가 호출 되었습니다");
        Debug.Log($"아이템 슬롯의 Data 의 값입니다{data}");
    }

    public void OnClick()
    {
        onClickCallback?.Invoke(data);
    }
}