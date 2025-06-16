using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using System.Linq;
using static Unity.VisualScripting.FlowStateWidget;

public class RemoteTester : MonoBehaviour
{
    [Header("왼쪽 카테고리 버튼")]
    public Button mainStoryButton;
    public Button randomStoryButton;
    public Button battleButton;
    public Button WeaponTestButton;

    [Header("오른쪽 버튼 프리팹 및 부모")]
    public GameObject buttonPrefab;
    public Transform rightPanelParent;

    [Header("메인스토리 , 이벤트 , 적 관련 정보")]
    public StoryDisplayManager storyDisplayManager;
    public EventDisplay eventDisplay;
    public GameFlowManager gameFlowManager;
    public InventoryManager inventoryManager;
    public JsonManager jsonManager;

    // 가상 시나리오 / 적 ID 리스트
    private List<string> mainStories = new List<string> { "MainScene_1", "MainScene_2", "MainScene_3" };
    private List<string> randomStories = new List<string> { "EventScene_1", "EventScene_2", "EventScene_3", "EventScene_4" };
    private List<string> enemyIDs = new List<string> { "monster_001", "monster_002", "monster_003" };
    private List<string> WeaponID = new List<string>();
    private List<ItemData> WeaponitemData = new List<ItemData>();

    private void Start()
    {
        var allWeapons = jsonManager.GetWeaponMasters("Weapon_Master").ToList();
        foreach (var weapon in allWeapons)
        {
            WeaponID.Add(weapon.Weapon_ID);
            WeaponitemData.Add(new ItemData { Item_ID = weapon.Weapon_ID, Item_Name = weapon.Weapon_Name, Item_Type = weapon.ItemType });
        }
        mainStoryButton.onClick.AddListener(() => ShowOptions(mainStories, OnMainStorySelected));
        randomStoryButton.onClick.AddListener(() => ShowOptions(randomStories, OnRandomStorySelected));
        battleButton.onClick.AddListener(() => ShowOptions(enemyIDs, OnBattleSelected));
        WeaponTestButton.onClick.AddListener(() => ShowOptions(WeaponID, WeaponAddInventory));
    }

    // 오른쪽 패널 버튼 생성
    void ShowOptions(List<string> options, System.Action<string> onClickAction)
    {
        // 기존 버튼 제거
        foreach (Transform child in rightPanelParent)
            Destroy(child.gameObject);

        // 새로운 버튼 생성
        foreach (var option in options)
        {
            GameObject btnObj = Instantiate(buttonPrefab, rightPanelParent);
            btnObj.GetComponentInChildren<TMP_Text>().text = option;

            btnObj.GetComponent<Button>().onClick.AddListener(() => onClickAction(option));
        }
    }

    // 각 항목 클릭 시 동작
    void OnMainStorySelected(string groupID)
    {
        if (int.TryParse(groupID.Replace("MainScene_", ""), out int id))
        {
            Debug.Log($"[리모컨] 랜덤 이벤트 수동 실행: 그룹 ID = {id}");
            //일단 정지 시키고 실행
            storyDisplayManager.StopMainStory();
            eventDisplay.StopRandomEvent();
            storyDisplayManager.storyList.Clear();
            eventDisplay.groupEvents.Clear();
            FindObjectOfType<StoryDisplayManager>().LoadMainStory(id);
        }

    }

    void OnRandomStorySelected(string groupID)
    {
        if (int.TryParse(groupID.Replace("EventScene_", ""), out int id))
        {
            Debug.Log($"[리모컨] 랜덤 이벤트 수동 실행: 그룹 ID = {id}");
            //일단 정지 시키고 실행
            storyDisplayManager.StopMainStory();
            eventDisplay.StopRandomEvent();
            storyDisplayManager.storyList.Clear();
            eventDisplay.groupEvents.Clear();
            FindObjectOfType<EventDisplay>().LoadEventStory(id);
        }
    }

    void OnBattleSelected(string enemyID)
    {
        Debug.Log($"[리모컨] 전투 시작: {enemyID}");
        storyDisplayManager.StopMainStory();
        eventDisplay.StopRandomEvent();
        storyDisplayManager.storyList.Clear();
        eventDisplay.groupEvents.Clear();
        FindObjectOfType<GameFlowManager>().ForceBattleWithMonster(enemyID);
    }
    void WeaponAddInventory(string weaponID)
    {
        Debug.Log($"[리모컨] 아이템 추가 시작: {weaponID}");
        var itemData = WeaponitemData.FirstOrDefault(i => i.Item_ID == weaponID);
        if (itemData != null)
        {
            inventoryManager.AddItemToInventory(itemData);
        }
        else
        {
            Debug.LogError($"[리모컨] 무기 {weaponID}의 ItemData를 찾을 수 없습니다.");
        }
    }
}