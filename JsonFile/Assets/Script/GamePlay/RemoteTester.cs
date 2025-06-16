using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class RemoteTester : MonoBehaviour
{
    [Header("왼쪽 카테고리 버튼")]
    public Button mainStoryButton;
    public Button randomStoryButton;
    public Button battleButton;

    [Header("오른쪽 버튼 프리팹 및 부모")]
    public GameObject buttonPrefab;
    public Transform rightPanelParent;

    [Header("메인스토리 , 이벤트 , 적 관련 정보")]
    public StoryDisplayManager storyDisplayManager;
    public EventDisplay eventDisplay;
    public GameFlowManager gameFlowManager;
    //public 

    // 가상 시나리오 / 적 ID 리스트
    private List<string> mainStories = new List<string> { "MainScene_1", "MainScene_2", "MainScene_3" };
    private List<string> randomStories = new List<string> { "EventScene_1", "EventScene_2", "EventScene_3", "EventScene_4" };
    private List<string> enemyIDs = new List<string> { "monster_001", "monster_002", "monster_003" };

    private void Start()
    {
        mainStoryButton.onClick.AddListener(() => ShowOptions(mainStories, OnMainStorySelected));
        randomStoryButton.onClick.AddListener(() => ShowOptions(randomStories, OnRandomStorySelected));
        battleButton.onClick.AddListener(() => ShowOptions(enemyIDs, OnBattleSelected));
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
            FindObjectOfType<EventDisplay>().LoadEventStory(id);
        }
    }

    void OnBattleSelected(string enemyID)
    {
        Debug.Log($"[리모컨] 전투 시작: {enemyID}");
        FindObjectOfType<GameFlowManager>().ForceBattleWithMonster(enemyID);
    }
}