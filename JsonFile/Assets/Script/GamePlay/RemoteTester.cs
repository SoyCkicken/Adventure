using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
#if UNITY_EDITOR
public class RemoteTester : MonoBehaviour
{
    [Header("���� ī�װ�� ��ư")]
    public Button mainStoryButton;
    public Button randomStoryButton;
    public Button battleButton;
    public Button weaponTestButton;
    public Button reMoveButton;
    public Button armorTestButton;

    [Header("������ ��ư ������ �� �θ�")]
    public GameObject buttonPrefab;
    public Transform rightPanelParent;

    [Header("���ν��丮 , �̺�Ʈ , �� ���� ����")]
    public StoryDisplayManager storyDisplayManager;
    public EventDisplay eventDisplay;
    public GameFlowManager gameFlowManager;
    public InventoryManager inventoryManager;
    public JsonManager jsonManager;

    // ���� �ó����� / �� ID ����Ʈ
    private List<string> mainStories = new List<string> { "MainScene_1_1", "MainScene_1_2", "MainScene_1_3", "MainScene_1_4" ,"MainScene_1_5","MainScene_2_1"};
    private List<string> randomStories = new List<string> { "EventScene_1", "EventScene_2", "EventScene_3", "EventScene_4" };
    private List<string> enemyIDs = new List<string>();
    private List<string> WeaponID = new List<string>();
    private List<string> ArmorID = new List<string>();

private void Start()
    {
        jsonManager = JsonManager.Instance ?? FindObjectOfType<JsonManager>(true); // 수정
        if (jsonManager == null)
        {
            Debug.LogWarning("[RemoteTester] JsonManager를 찾지 못해 디버그 버튼 초기화를 건너뜁니다.");
            return;
        }

        foreach (var weapon in jsonManager.GetWeaponMasters("Weapon_Master"))
        {
            WeaponID.Add(weapon.Weapon_ID);
        }
        foreach (var armor in jsonManager.GetArmorMasters("Armor_Master"))
        {
            ArmorID.Add(armor.Armor_ID);
        }
        foreach (var monster in jsonManager.GetMonMasters("Mon_Master"))
        {
            enemyIDs.Add(monster.Mon_ID);
        }

        mainStoryButton?.onClick.AddListener(() => ShowOptions(mainStories, OnMainStorySelected));
        randomStoryButton?.onClick.AddListener(() => ShowOptions(randomStories, OnRandomStorySelected));
        battleButton?.onClick.AddListener(() => ShowOptions(enemyIDs, OnBattleSelected));
        weaponTestButton?.onClick.AddListener(() => ShowOptions(WeaponID, WeaponAddInventory));
        armorTestButton?.onClick.AddListener(() => ShowOptions(ArmorID, ArmorAddInventory));
        reMoveButton?.onClick.AddListener(() => RemoveAllInventory());
    }

    // ������ �г� ��ư ����
    void ShowOptions(List<string> options, System.Action<string> onClickAction)
    {
        // ���� ��ư ����
        foreach (Transform child in rightPanelParent)
            Destroy(child.gameObject);

        // ���ο� ��ư ����
        foreach (var option in options)
        {
            GameObject btnObj = Instantiate(buttonPrefab, rightPanelParent);
            btnObj.GetComponentInChildren<TMP_Text>().text = option;

            btnObj.GetComponent<Button>().onClick.AddListener(() => onClickAction(option));
        }
    }

    // �� �׸� Ŭ�� �� ����
    void OnMainStorySelected(string groupID)
    {
        string[] parts = groupID.Replace("MainScene_", "").Split('_');

        if (parts.Length == 2 &&
            int.TryParse(parts[0], out int chapter) &&
            int.TryParse(parts[1], out int eventIndex))
        {
            {
                Debug.Log($"[������] ���� �̺�Ʈ ���� ����: �׷� ID = {chapter} , {eventIndex}");
                //�ϴ� ���� ��Ű�� ����
                storyDisplayManager.StopMainStory();
                eventDisplay.StopRandomEvent();
                storyDisplayManager.storyList.Clear();
                eventDisplay.groupEvents.Clear();
                FindObjectOfType<StoryDisplayManager>().LoadMainStory(chapter, eventIndex);
            }
        }
    }

    void OnRandomStorySelected(string groupID)
    {
        if (int.TryParse(groupID.Replace("EventScene_", ""), out int id))
        {
            Debug.Log($"[������] ���� �̺�Ʈ ���� ����: �׷� ID = {id}");
            //�ϴ� ���� ��Ű�� ����
            storyDisplayManager.StopMainStory();
            eventDisplay.StopRandomEvent();
            storyDisplayManager.storyList.Clear();
            eventDisplay.groupEvents.Clear();
            FindObjectOfType<EventDisplay>().LoadEventStory(id);
        }
    }

    void OnBattleSelected(string enemyID)
    {
        Debug.Log($"[������] ���� ����: {enemyID}");
        storyDisplayManager.StopMainStory();
        eventDisplay.StopRandomEvent();
        storyDisplayManager.storyList.Clear();
        eventDisplay.groupEvents.Clear();
        FindObjectOfType<GameFlowManager>().ForceBattleWithMonster(enemyID);
    }
    void WeaponAddInventory(string weaponID)
    {
        Debug.Log($"[������] ������ �߰� ����: {weaponID}");
        var itemData = ItemDataFactory.FromCode(jsonManager, weaponID);
        if (itemData != null)
        {
            inventoryManager.AddItemToInventory(itemData);
        }
        else
        {
            Debug.LogError($"[������] ���� {weaponID}�� ItemData�� ã�� �� �����ϴ�.");
        }
    }
    void ArmorAddInventory(string armorID)
    {
        Debug.Log($"[������] ������ �߰� ���� : {armorID}");
        var itemData = ItemDataFactory.FromCode(jsonManager, armorID);
        if (itemData != null)
        {
            inventoryManager.AddItemToInventory(itemData);
        }
    }

    void RemoveAllInventory()
    {
        Debug.Log("[������] �κ��丮�� �ִ� ��� ������ ���� �����δ� �۵� ����");
        //inventoryManager.ClearAllItems();
    }
}
#endif

