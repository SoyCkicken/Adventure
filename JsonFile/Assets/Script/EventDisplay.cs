using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventDisplay : MonoBehaviour
{
    [Header("Prefabs & References")]
    public Transform content;
    public GameObject ImagePrefab;
    public GameObject TextPrefab;
    public GameObject SkipButton;
    public Transform choiceButtonParent;
    public GameObject choiceButtonPrefab;
    public FontSizeManager fontSizeManager;
    public GameObject TouchCatcher;
    private JsonManager jsonManager;
    
    private RandomEvents_Master_Event currentEvent;
    private int currentIndex = 0;
    
    
    private bool isSkip = false;
    private bool isTyping = false;
    private Action<bool> onCompleteCallback;
    private System.Random rng = new System.Random();
    public int count;
    public int currCount = 0;
    public int currentGroup;
    public int currentGroupIndex;

    public bool toBattle = false;

    private List<int> eventGroups;
    public List<RandomEvents_Master_Event> eventList;
    public List<RandomEvents_Master_Event> groupEvents;
    private List<Ran_Script_Master_Event> scriptEventsCache;
    public List<GameObject> activeBlocks = new List<GameObject>();
    public event Action<string> OnBattleJoin;
    private void Awake()
    {
        if (jsonManager == null)
            jsonManager = FindObjectOfType<JsonManager>();


        TouchCatcher.GetComponent<TouchCatcher>().onTapOutsideScrollView += () =>
        {
            OnSkip();
        };
        count = rng.Next(1,2);
    }

    private void OnSkip()
    {
        if (isTyping)
            isSkip = true;
    }
    public void Start()
    {
    }
    /// <summary>
    /// 랜덤 이벤트 연출 시작
    /// </summary>
    /// <param name="onComplete">전투 여부를 매개변수로 받는 콜백</param>
    public void StartRandomEvent(Action<bool> onComplete = null)
    {
        onCompleteCallback = onComplete;

        // 이벤트 데이터 로드 및 정렬
        eventList = jsonManager.GetRandomMainMasters("RandomEvents_Master_Event");
        if (eventList == null || eventList.Count == 0)
        {
            Debug.LogError("RandomEvent 파일 로드 실패");
            onCompleteCallback?.Invoke(toBattle);
            return;
        }
        eventList = eventList.OrderBy(e => e.RandomEvent_Index)
                             .ThenBy(e => e.Script_Index)
                             .ToList();


        // 스크립트 캐시
        scriptEventsCache = jsonManager.GetRandomScriptMasters("Ran_Script_Master_Event");

        eventGroups = eventList
            .Select(e => e.RandomEvent_Index)
            .Distinct()
            .ToList();

        // 초기화
        currentIndex = 0;
        currentEvent = eventList[currentIndex];
        ClearContent();
        TouchCatcher.SetActive(true);
        PickNewGroup();
        //랜덤으로 바꿨음
        //DisplayCurrentEvent();
    }

    private void PickNewGroup()
    {
        Debug.Log("랜덤 값을 뽑습니다");
        ClearContent();
        SkipButton.GetComponent<Button>().onClick.RemoveAllListeners();
        SkipButton.GetComponent<Button>().onClick.AddListener(OnSkip);
        if (eventGroups == null || eventGroups.Count == 0)
        {
            // 남은 그룹 없음 -> 메인 스토리 복귀ㄴ
            onCompleteCallback?.Invoke(false);
            Debug.Log("랜덤값이 없어서 취소 됩니다");
            return;
        }
        // 랜덤으로 그룹 번호 선택 후 제거
        int gi = rng.Next(eventGroups.Count);
        currentGroup = eventGroups[gi];
        eventGroups.RemoveAt(gi);
        Debug.Log($"랜덤 값을 뽑았습니다.= {currentGroup}");
        // 선택된 그룹의 이벤트 시퀀스 구성
        groupEvents = eventList
            .Where(e => e.RandomEvent_Index == currentGroup)
            .OrderBy(e => e.Script_Index)
            .ToList();
        currentGroupIndex = 0;
        
        // 첫 이벤트 실행
        DisplayCurrentEvent();
    }


    /// <summary>
    /// 랜덤 이벤트 일시 중지 및 UI 클린업
    /// </summary>
    public void StopRandomEvent()
    {
        StopAllCoroutines();
        ClearContent();
        SkipButton.SetActive(false);
    }
    /// <summary>
    /// 현재 이벤트 노드 표시
    /// </summary>
    private void DisplayCurrentEvent()
    {

        Debug.Log(eventList[currentGroupIndex].Event_Text);
        if (currentIndex < 0 || currentIndex >= eventList.Count || groupEvents == null)
        {
            Debug.LogError($"Invalid currentIndex: {currentIndex}");
            onCompleteCallback?.Invoke(false);
            return;
        }
        //Debug.Log(groupEvents[currentGroupIndex]);
        currentEvent = groupEvents[currentGroupIndex];
        var script = scriptEventsCache.FirstOrDefault(s =>
            s.Script_Code.Trim() == currentEvent.Event_Text.Trim());

        if (script == null)
        {
            Debug.LogWarning($"스크립트 매칭 실패: {currentEvent.Event_Text}");
            AdvanceEvent();
            return;
        }

        GameObject last = activeBlocks.Count > 0 ? activeBlocks[activeBlocks.Count - 1] : null;
        switch (script.displayType)
        {
            case "IMAGE":
                CreateImageBlock(script.KOR);
                break;
            case "TEXT":
                HandleTextDisplay(script.KOR, last);
                if (currentEvent.Choice1_Text != null)
                {
                    SetupChoices();
                }
                break;
            case "BATTLE":
                OnBattleJoin?.Invoke(script.KOR);
                break;  
        }
        if (!string.IsNullOrEmpty(currentEvent.Choice1_Text))
            SetupChoices();

    }

    private void HandleTextDisplay(string text, GameObject last)
    {
        //Debug.Log("텍스트인데 마지막 블록이 있는지 없는지 확인");
        if (last == null || last.TryGetComponent<Image>(out _))
            CreateTextBlock(text);
        else
            StartCoroutine(TypeTextEffect(text, last));
    }
    private void ClearContent()
    {
        foreach (var go in activeBlocks)
            Destroy(go);
        activeBlocks.Clear();
        ClearChoiceButtons();
    }

    private void ClearChoiceButtons()
    {
        foreach (Transform t in choiceButtonParent)
            Destroy(t.gameObject);
    }

    /// <summary>
    /// 이벤트 흐름 진행 (전투 여부 결정)
    /// </summary>
    public void AdvanceEvent()
    {
        var choices = new[]
   {
        currentEvent.Choice1_Text,
        currentEvent.Choice2_Text,
        currentEvent.Choice3_Text
    }.Where(c => !string.IsNullOrEmpty(c)).ToList();
        if (choices.Count == 1)
        {
            return;
        }
        else if (choices.Count > 1)
        {
            // 다중 분기는 SetupChoices() 에서 버튼 클릭으로 처리됐을 것
            return;
        }






        var script = scriptEventsCache.FirstOrDefault(s =>
            s.Script_Code.Trim() == currentEvent.Event_Text.Trim());

        //Debug.Log(script.EventBreak);
        if (currentEvent != null && script.EventBreak == "Break")
        {
            //currCount++;
            //if (currCount == count)
            //{
            //    onCompleteCallback?.Invoke(false);
            //    StopRandomEvent();
            //}
            Debug.Log("지금 이벤트가 종료되어 여기break문으로 들어왔습니다");
            SkipButton.SetActive(true);
            TouchCatcher.SetActive(false);
            SkipButton.GetComponent<Button>().onClick.RemoveAllListeners();
            SkipButton.GetComponent<CanvasGroup>().blocksRaycasts = true;  //이 부분이 추가가 되었음
            SkipButton.GetComponent<Button>().onClick.AddListener(() => {
                PickNewGroup();
                SkipButton.SetActive(false);
            });
            
            return;
        }
        // 다음 이벤트
        if (currentGroupIndex + 1 < groupEvents.Count)
        {
            currentGroupIndex++;
            DisplayCurrentEvent();
        }
        else
        {
            // 이벤트 종료 후 메인 스토리 복귀
            onCompleteCallback?.Invoke(toBattle);
            StopRandomEvent();
        }
    }

    private IEnumerator TypeTextEffect(string full, GameObject go)
    {
        //Debug.Log("타이핑중");
        var tmp = go.GetComponent<TMP_Text>();
        isTyping = true;
        string complete = tmp.text + full;

        for (int i = 0; i < full.Length; i++)
        {
            if (isSkip)
            {
                tmp.text = complete;
                break;
            }
            tmp.text += full[i];
            yield return new WaitForSeconds(0.05f);
        }

        isTyping = false;
        isSkip = false;
        SkipButton.SetActive(false);
        AdvanceEvent();
        StopCoroutine(TypeTextEffect(full,go));
    }

    private void CreateImageBlock(string name)
    {
        //Debug.Log("이미지 블록 생성");
        var go = Instantiate(ImagePrefab, content);
        go.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/" + name);
        activeBlocks.Add(go);
    }

    void CreateTextBlock(string text)
    {
        var go = Instantiate(TextPrefab, content);
        TMP_Text tmp = go.GetComponentInChildren<TMP_Text>();
        fontSizeManager.Register(tmp);
        activeBlocks.Add(go);
        //var tmp = go.GetComponent<TMP_Text>();
        StartCoroutine(TypeTextEffect(text, go));
    }


    private void SetupChoices()
    {
        ClearChoiceButtons();
        SkipButton.GetComponent<Button>().onClick.RemoveAllListeners();
        var choices = new List<(string code, string text)>();
        Debug.Log(GetScriptText(currentEvent.Choice1_Text));
        if (!string.IsNullOrEmpty(currentEvent.Choice1_Text))
            choices.Add((currentEvent.Choice1_Text, GetScriptText(currentEvent.Choice1_Text)));
        if (!string.IsNullOrEmpty(currentEvent.Choice2_Text))
            choices.Add((currentEvent.Choice2_Text, GetScriptText(currentEvent.Choice2_Text)));
        if (!string.IsNullOrEmpty(currentEvent.Choice3_Text))
            choices.Add((currentEvent.Choice3_Text, GetScriptText(currentEvent.Choice3_Text)));
        foreach (var ch in choices)
        {
            var btn = Instantiate(choiceButtonPrefab, choiceButtonParent).GetComponent<Button>();
            btn.GetComponentInChildren<TMP_Text>().text = ch.text;
            Debug.Log(ch.code);
            btn.onClick.AddListener(() => OnChoice(ch.code));
        }
    }

    private void OnChoice(string code)
    {
        // 선택된 코드로 이동
        var target = eventList.FirstOrDefault(e => e.Event_Text.Trim() == code.Trim());
        Debug.Log(target);
        ClearContent();
        if (target != null)
        {
            //여기가 문제인거 확인
            //currentGroupIndex = eventList.IndexOf(target);
            currentGroupIndex = target.Script_Index;
            DisplayCurrentEvent();
        }
        else
        {
            Debug.LogWarning("이벤트 코드 미발견: " + code);
            AdvanceEvent();
        }
    }

    private string GetScriptText(string code)
    {
        var s = scriptEventsCache.FirstOrDefault(sv => sv.Script_Code.Trim() == code.Trim());
        Debug.Log(s);
        return s != null ? s.KOR : code;
    }
}
