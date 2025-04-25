using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameStory : MonoBehaviour
{
    [Header("출력하는 스크립트")]
    [SerializeField] private StoryDisplayManager storyDisplay;
    [SerializeField] private EventDisplay eventDisplay;

    [Header("Json파일 관리자")]
    public JsonManager jsonManager;

    //메인 스토리 처리용 쿼리
    private Queue<Script_Master_Main> mainQueue;
    //이벤트 처리용 쿼리
    private Queue<RandomEvent> eventQueue;

    private bool isEventMode = false; //랜덤 이벤트로 뽑을 것인지
    private System.Random rng = new System.Random();//랜덤인데 명시해준것 유니티랑 시스템이랑 랜덤이 2개 있음

    private void Start()
    {
       EnqueueMainStory();
        ShowNext();
    }

    private void ShowNext()
    {
        //랜덤 이벤트로 설정이 안되어 있거나 메인 스토리 쿼리가 0이면 초기화
        //랜덤 이벤트 실행
        if (!isEventMode && mainQueue.Count == 0)
        {
            isEventMode = true;
            EnqueueRandomEvent();
        }
        else if (isEventMode && eventQueue.Count == 0)
        {
            isEventMode = false;
            EnqueueMainStory();
        }
    }
    //메인 스토리 끝나면 알려주는 용도
    private void OnMainNodeComlete(Script_Master_Main node)
    {
        if (node.StoryBreak == "Break")
        {
            //초기화
            mainQueue.Clear();
            ShowNext();
        }
    }
    //이벤트 끝나면 알려주는 용도
    private void OnEventNodeComplete(RandomEvent node)
    {
        if (node.EventBreak == "Break")
        {
            eventQueue.Clear();
            ShowNext();
        }
    }
    private void EnqueueMainStory()
    {
        var list = jsonManager.scriptMasterMains.OrderBy(n => n.Chapter_Index).ThenBy(n => n.Event_Index).ThenBy(n => n.Scenc_Index);
        mainQueue = new Queue<Script_Master_Main>(list);
    }
    private void EnqueueRandomEvent()
    {
        var groups=  jsonManager.randomEvents.Select(e=>e.RandomEvent_Index).Distinct().ToList();
        int pick = groups[rng.Next(groups.Count)];
        var scripts = jsonManager.randomEvents.Where(e => e.RandomEvent_Index == pick).OrderBy(e => e.Script_Index);
        eventQueue = new Queue<RandomEvent>(scripts);
    }
}
