using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.Playables;
using static SaveManager;
using Palmmedia.ReportGenerator.Core.Reporting.Builders;

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
    private SpriteBank spriteBank;
    public ScrollRect scrollRect;
    public PlayerState playerState;
    
    private bool isSkip = false;
    private bool isTyping = false;
    private System.Random rng = new System.Random();
    public int count;
    public int currCount = 0;
    public int currentGroup;
    public int currentGroupIndex;

    public bool toBattle = false;

    private string winScriptCode;
    private string loseScriptCode;

    public List<int> eventGroups;
    public List<RandomEvents_Master_Event> groupEvents;
    public List<GameObject> activeBlocks = new List<GameObject>();
    public event Action<string> OnBattleJoin;
    // 이벤트 데이터들
    private List<RandomEvents_Master_Event> eventList;
    private RandomEvents_Master_Event currentEvent;

    // 스크립트 캐시 (Ran_Script_Master_Event)
    private List<Ran_Script_Master_Event> scriptEventsCache;
    
    // 외부 콜백
    private Action<bool> onCompleteCallback;


    private void Awake()
    {
        if (jsonManager == null)
            jsonManager = FindObjectOfType<JsonManager>();
        if(spriteBank == null)
            spriteBank = FindObjectOfType<SpriteBank>();

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
    public void StartEventSequence(Action<bool> onComplete)
    {
        onCompleteCallback = onComplete;

        // JSON 데이터 로드
        eventList = jsonManager.GetRandomMainMasters("RandomEvents_Master_Event");
        if (eventList == null || eventList.Count == 0)
        {
            Debug.LogError("RandomEvents_Master_Event 로드 실패");
            onCompleteCallback?.Invoke(false);
            return;
        }

        // 이벤트 정렬
        eventList = eventList.OrderBy(e => e.RandomEvent_Index)
                             .ThenBy(e => e.Script_Index)
                             .ToList();

        // 스크립트 캐시
        scriptEventsCache = jsonManager.GetRandomScriptMasters("Ran_Script_Master_Event");

        //전체 이벤트 리스트에 추가
        eventGroups = eventList
            .Select(e => e.RandomEvent_Index)
            .Distinct()
            .ToList();

        // 초기화
        ClearContent();
        //터치 패널 초기화
        TouchCatcher.SetActive(true);

        // 첫 이벤트 출력
        //DisplayCurrentEvent();
        PickNewGroup();
    }


    private void PickNewGroup()
    {
        Debug.Log("랜덤 값을 뽑습니다");
        ClearContent();
        SkipButton.GetComponent<Button>().onClick.RemoveAllListeners();
        TouchCatcher.GetComponent<TouchCatcher>().onTapOutsideScrollView += () =>
        {
            OnSkip();
        };
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
        StopAllCoroutines();                    // 텍스트 출력 중단
        ClearContent();                         // 텍스트, 이미지, 선택지 등 제거
        SkipButton.SetActive(false);            // 스킵 버튼 비활성화
        TouchCatcher.SetActive(false);          // 터치 패널도 비활성화
        isTyping = false;
        isSkip = false;
    }
    /// <summary>
    /// 현재 이벤트 노드 표시
    /// </summary>
    public void DisplayCurrentEvent()
    {
        SkipButton.GetComponent<Button>().onClick.RemoveAllListeners();
        Debug.Log("이벤트 출력 시작");
        SkipButton.SetActive(true);
        TouchCatcher.GetComponent<TouchCatcher>().onTapOutsideScrollView += () =>
        {
            OnSkip();
        };
        if (groupEvents == null || currentGroupIndex >= groupEvents.Count)
        {
            Debug.LogError("groupEvents가 비어있거나 인덱스 초과");
            onCompleteCallback?.Invoke(false);
            return;
        }

        currentEvent = groupEvents[currentGroupIndex];
        var script = scriptEventsCache.FirstOrDefault(s =>
            s.Script_Code.Trim() == currentEvent.Event_Text.Trim());

        if (script == null)
        {
            Debug.LogWarning($"스크립트 매칭 실패: {currentEvent.Event_Text}");
            SetupSkipToEnd();  // → 이후 스킵 처리 및 복귀
            return;
        }

        GameObject lastBlock = activeBlocks.Count > 0 ? activeBlocks.Last() : null;

        switch (script.displayType)
        {
            case "TEXT":
                Debug.Log("텍스트 출력");
                HandleTextDisplayWithChoice(script.KOR, lastBlock,false);
                break;

            case "IMAGE":
                Debug.Log("이미지 출력");
                CreateImageBlock(script.KOR);
                break;
            case "CLEAR" :
                HandleTextDisplayWithChoice(script.KOR, lastBlock,true);
                break;
            case "BATTLE":
                Debug.Log("전투 시작");
                winScriptCode = script.NEXTWIN?.Trim();
                Debug.Log($"전투 승리시 들어갈 코드{winScriptCode}");
                loseScriptCode = script.NEXTLOSE?.Trim();
                Debug.Log($"전투 패배시 들어갈 코드{loseScriptCode}");
                OnBattleJoin?.Invoke(script.KOR);
                break;
        }
    }

    private void HandleTextDisplayWithChoice(string text, GameObject lastBlock,bool isClear)
    {
        if (lastBlock == null || lastBlock.TryGetComponent<Image>(out _))
            CreateTextBlock(text, isClear);
        else
            StartCoroutine(TypeTextEffectWithChoice(text, lastBlock, isClear));
    }
    public void ClearContent()
    {
        foreach (var go in activeBlocks)
            Destroy(go);
        activeBlocks.Clear();
        TouchCatcher.GetComponent<TouchCatcher>().onTapOutsideScrollView = null;
        ClearChoiceButtons();
    }

    private void ClearChoiceButtons()
    {
        foreach (Transform t in choiceButtonParent)
            Destroy(t.gameObject);
        //choices.Clear();
    }

    /// <summary>
    /// 이벤트 흐름 진행 (전투 여부 결정)
    /// </summary>
    public void AdvanceEvent()
    {
        if (isTyping) return;

        // 현재 선택지 유무 확인
        var choices = new[]
        {
        currentEvent.Choice1_Text,
        currentEvent.Choice2_Text,
        currentEvent.Choice3_Text
    }.Where(c => !string.IsNullOrEmpty(c)).ToList();

        if (choices.Count == 1)
        {
            // 단일 선택지 자동 진행
            return;
        }
        else if (choices.Count > 1)
        {
            // 복수 선택지는 SetupChoices에서 처리
            return;
        }

        // 다음 스크립트 찾기
        var next = groupEvents.FirstOrDefault(e =>
            e.RandomEvent_Index == currentEvent.RandomEvent_Index &&
            e.Script_Index == currentEvent.Script_Index + 1);

        // 스크립트 캐시에서 현재 스크립트 구조 확인
        var script = scriptEventsCache.FirstOrDefault(s =>
            s.Script_Code.Trim() == currentEvent.Event_Text.Trim());

        if (script != null && script.EventBreak == "Break")
        {
            Debug.Log("이벤트 종료: Break 문 탐지");
            SetupSkipToEnd();
            return;
        }

        if (next != null)
        {
            currentEvent = next;
            currentGroupIndex = groupEvents.IndexOf(next);
            DisplayCurrentEvent();
        }
        else
        {
            Debug.Log("다음 노드 없음 → 이벤트 종료");
            SetupSkipToEnd();
        }
    }

    public void WinBattle(bool playerWin)
    {
        Debug.Log($"[WinBattle] 전투 결과: {playerWin}");
        Debug.Log($"[WinBattle] winScriptCode: {winScriptCode}, loseScriptCode: {loseScriptCode}");

        string nextCode = playerWin ? winScriptCode : loseScriptCode;

        if (string.IsNullOrEmpty(nextCode))
        {
            Debug.LogWarning("전투 결과에 따른 다음 코드가 비어 있습니다.");
            StopRandomEvent();
            return;
        }

        var nextNode = groupEvents.FirstOrDefault(s =>
            s.Event_Text.Trim() == nextCode.Trim());

        if (nextNode == null)
        {
            Debug.LogWarning($"다음 스크립트를 찾을 수 없습니다: {nextCode}");
            StopRandomEvent();
            return;
        }

        currentEvent = nextNode;
        currentGroupIndex = groupEvents.IndexOf(nextNode);

        Debug.Log($"[WinBattle] 다음 이벤트 이동: {currentEvent.Event_Text}");

        ClearContent();
        DisplayCurrentEvent();

    }
    private IEnumerator TypeTextEffectWithChoice(string fullText, GameObject go, bool isClear)
    {
        TMP_Text tmp = go.GetComponentInChildren<TMP_Text>();
        isTyping = true;
        string complete = tmp.text + fullText;

        for (int i = 0; i < fullText.Length; i++)
        {
            if (isSkip)
            {
                tmp.text = complete;
                Canvas.ForceUpdateCanvases();
                var contentRect = scrollRect.content as RectTransform;
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
                scrollRect.verticalNormalizedPosition = 0f;
                break;
            }
            tmp.text += fullText[i];
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content as RectTransform);
            scrollRect.verticalNormalizedPosition = 0f;
            scrollRect.verticalNormalizedPosition = 0f;
            yield return new WaitForSeconds(0.05f);
        }

        isTyping = false;
        isSkip = false;
        SkipButton.SetActive(false);
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content as RectTransform);
        scrollRect.verticalNormalizedPosition = 0f;

        // 타이핑이 끝난 후에 선택지 출력
        if (!string.IsNullOrEmpty(currentEvent.Choice1_Text) ||
            !string.IsNullOrEmpty(currentEvent.Choice2_Text) ||
            !string.IsNullOrEmpty(currentEvent.Choice3_Text))
        {
            SetupChoices();
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
        else
        {
            SkipButton.SetActive(true);
            CanvasGroup group = SkipButton.GetComponent<CanvasGroup>();
            if (group != null) group.blocksRaycasts = true;

            Button skipBtn = SkipButton.GetComponent<Button>();
            skipBtn.onClick.RemoveAllListeners();

            skipBtn.onClick.AddListener(() =>
            {
                SkipButton.SetActive(false);
                if (isClear)
                {
                    Debug.Log("[EventDisplay] ClearContent 호출됨");
                    ClearContent();
                }
                Debug.Log("AdvanceEvent 호출됨");
                AdvanceEvent();
            });
        }
    }

    private void CreateImageBlock(string name)
    {
        //Debug.Log("이미지 블록 생성");
        var go = Instantiate(ImagePrefab, content);
        var image = go.GetComponent<Image>();
        Sprite s = spriteBank.Load(name);
        image.sprite = s;
        //go.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/" + name);
        activeBlocks.Add(go);
        AdvanceEvent();
    }

    void CreateTextBlock(string text,bool isClear)
    {
        var go = Instantiate(TextPrefab, content);
        TMP_Text tmp = go.GetComponentInChildren<TMP_Text>();
        fontSizeManager.Register(tmp);
        activeBlocks.Add(go);
        //var tmp = go.GetComponent<TMP_Text>();
        StartCoroutine(TypeTextEffectWithChoice(text, go , isClear));
    }


    private void SetupChoices()
    {
        ClearChoiceButtons();
        SkipButton.GetComponent<Button>().onClick.RemoveAllListeners();

        var choices = new List<(string code, string text, int choiceNo)>();

        if (!string.IsNullOrEmpty(currentEvent.Choice1_Text))
            choices.Add((currentEvent.Choice1_Text, GetScriptText(currentEvent.Choice1_Text), 1));
        if (!string.IsNullOrEmpty(currentEvent.Choice2_Text))
            choices.Add((currentEvent.Choice2_Text, GetScriptText(currentEvent.Choice2_Text), 2));
        if (!string.IsNullOrEmpty(currentEvent.Choice3_Text))
            choices.Add((currentEvent.Choice3_Text, GetScriptText(currentEvent.Choice3_Text), 3));

        var successRates = jsonManager.GetSuccessRatesRanByScene(currentEvent.Random_Event_ID);

        foreach (var ch in choices)
        {
            var btn = Instantiate(choiceButtonPrefab, choiceButtonParent).GetComponent<Button>();
            btn.GetComponentInChildren<TMP_Text>().text = ch.text;

            var rateData = successRates.FirstOrDefault(r => r.Choice_No == ch.choiceNo);

            btn.onClick.AddListener(() =>
            {
                string nextCode = ch.code; // 기본값

                if (rateData != null)
                {
                    float chance = EvaluateFormula(rateData.Success_Formula);
                    bool isSuccess = UnityEngine.Random.value < chance;
                    nextCode = isSuccess ? rateData.Success_Next_Script : rateData.Fail_Next_Script;
                }

                OnChoice(nextCode);
            });
        }
    }

    private void OnChoice(string code)
    {
        // 선택된 코드로 이동
        var target = eventList.FirstOrDefault(e => e.Event_Text.Trim() == code.Trim());
        var targetscript = scriptEventsCache.FirstOrDefault(e => e.Script_Code.Trim() == code.Trim());
        Debug.Log($"{targetscript.KOR} , {targetscript.EventBreak} ");
        Debug.Log($"{target.Random_Event_ID}\n{target.Event_Text}");
        ClearContent();
        if (target != null)
        {
            //여기가 문제인거 확인
            //currentGroupIndex = eventList.IndexOf(target);
            currentGroupIndex = target.Script_Index;
           Debug.Log(target.Script_Index);
            if (targetscript.EventBreak == "Break")
            {
                //끝나는 선택지 누를 시 
                //이벤트 종료 시킴
                if (targetscript != null && targetscript.EventBreak == "Break")
                {
                    Debug.Log("이벤트 종료: Break 문 탐지");
                    SetupSkipToEnd();
                    return;
                }
            }
            else
            {
                //이벤트 끝나는 부분이 아님
                DisplayCurrentEvent();
            }
            
        }
        else
        {
            Debug.LogWarning("이벤트 코드 미발견: " + code);
        }
    }

    private string GetScriptText(string code)
    {
        var s = scriptEventsCache.FirstOrDefault(sv => sv.Script_Code.Trim() == code.Trim());
        Debug.Log(s);
        return s != null ? s.KOR : code;
    }

    //이 부분이 추가가 되었습니다!
    private void SetupSkipToEnd()
    {
        //스킵 버튼에 있는 모든 추가 기능 삭제 후
        SkipButton.GetComponent<Button>().onClick.RemoveAllListeners();
        //활성화 후
        SkipButton.SetActive(true);
        //터치 감지 기능 추가
        SkipButton.GetComponent<CanvasGroup>().blocksRaycasts = true;
        //SkipButton.GetComponent<Button>().onClick.AddListener(() =>
        //{
            
        //});
        PickNewGroup();  // or onCompleteCallback?.Invoke(false); depending on context
        SkipButton.SetActive(false);
    }
    private float EvaluateFormula(string formula)
    {
        if (string.IsNullOrEmpty(formula)) return 0f;
        // 간단한 STR * 10 구조만 처리
        if (formula.Contains("STR"))
        {
            int str = playerState.STR; // 임시 값 (플레이어 스탯에서 가져와야 함)
            string sanitized = formula.Replace(" ", ""); // 공백 제거
            string factor = sanitized.Replace("STR*", "");

            if (float.TryParse(factor, out float percent))
            {
                //Debug.Log($"계산된 배율: {percent}");
                return (str * percent) / 100f;
            }
            else
            {
                Debug.LogWarning($"배율 파싱 실패: {factor}");
            }


        }
        else if (formula.Contains("DEX"))
        {
            int DEX = playerState.AGI; // 임시 값 (플레이어 스탯에서 가져와야 함)
            string sanitized = formula.Replace(" ", ""); // 공백 제거
            string factor = sanitized.Replace("DEX*", "");

            if (float.TryParse(factor, out float percent))
            {
                //Debug.Log($"계산된 배율: {percent}");
                return (DEX * percent) / 100f;
            }
            else
            {
                Debug.LogWarning($"배율 파싱 실패: {factor}");
            }
        }
        else if (formula.Contains("DIV"))
        {
            int DIV = playerState.DIV; // 임시 값 (플레이어 스탯에서 가져와야 함)
            string sanitized = formula.Replace(" ", ""); // 공백 제거
            string factor = sanitized.Replace("DIV*", "");

            if (float.TryParse(factor, out float percent))
            {
                //Debug.Log($"계산된 배율: {percent}");
                return (DIV * percent) / 100f;
            }
            else
            {
                Debug.LogWarning($"배율 파싱 실패: {factor}");
            }
        }

        else if (formula.Contains("INT"))
        {
            int INT = playerState.INT; // 임시 값 (플레이어 스탯에서 가져와야 함)
            string sanitized = formula.Replace(" ", ""); // 공백 제거
            string factor = sanitized.Replace("INT*", "");

            if (float.TryParse(factor, out float percent))
            {
                //Debug.Log($"계산된 배율: {percent}");
                return (INT * percent) / 100f;
            }
            else
            {
                Debug.LogWarning($"배율 파싱 실패: {factor}");
            }
        }

        else if (formula.Contains("MAG"))
        {
            int MAG = playerState.MAG; // 임시 값 (플레이어 스탯에서 가져와야 함)
            string sanitized = formula.Replace(" ", ""); // 공백 제거
            string factor = sanitized.Replace("MAG*", "");

            if (float.TryParse(factor, out float percent))
            {
                //Debug.Log($"계산된 배율: {percent}");
                return (MAG * percent) / 100f;
            }
            else
            {
                Debug.LogWarning($"배율 파싱 실패: {factor}");
            }
        }

        else if (formula.Contains("CHA"))
        {
            int CHA = playerState.CHA; // 임시 값 (플레이어 스탯에서 가져와야 함)
            string sanitized = formula.Replace(" ", ""); // 공백 제거
            string factor = sanitized.Replace("CHA*", "");

            if (float.TryParse(factor, out float percent))
            {
                //Debug.Log($"계산된 배율: {percent}");
                return (CHA * percent) / 100f;
            }
            else
            {
                Debug.LogWarning($"배율 파싱 실패: {factor}");
            }
        }

        else if (formula.Contains("HEALTH"))
        {
            int HEALTH = playerState.Health; // 임시 값 (플레이어 스탯에서 가져와야 함)
            string sanitized = formula.Replace(" ", ""); // 공백 제거
            string factor = sanitized.Replace("HEALTH*", "");

            if (float.TryParse(factor, out float percent))
            {
                //Debug.Log($"계산된 배율: {percent}");
                return (HEALTH * percent) / 100f;
            }
            else
            {
                Debug.LogWarning($"배율 파싱 실패: {factor}");
            }
        }
        return 0f;
    }

    public void LoadEventStory(int groupID)
    {
        Debug.Log($"[리모컨] 수동 로드된 이벤트 그룹 ID: {groupID}");

        // JSON 데이터가 없다면 초기화부터
        if (eventList == null || eventList.Count == 0)
        {
            eventList = jsonManager.GetRandomMainMasters("RandomEvents_Master_Event");
            if (eventList == null || eventList.Count == 0)
            {
                Debug.LogError("RandomEvents_Master_Event 로드 실패");
                return;
            }

            eventList = eventList.OrderBy(e => e.RandomEvent_Index)
                                 .ThenBy(e => e.Script_Index)
                                 .ToList();
        }

        // 스크립트 캐시도 마찬가지로 초기화
        if (scriptEventsCache == null || scriptEventsCache.Count == 0)
        {
            scriptEventsCache = jsonManager.GetRandomScriptMasters("Ran_Script_Master_Event");
        }

        // 해당 그룹 ID가 존재하는지 확인
        var matchedEvents = eventList
            .Where(e => e.RandomEvent_Index == groupID)
            .OrderBy(e => e.Script_Index)
            .ToList();

        if (matchedEvents == null || matchedEvents.Count == 0)
        {
            Debug.LogWarning($"[리모컨] 해당 그룹 ID({groupID})의 이벤트가 없습니다.");
            return;
        }

        // UI 초기화 및 변수 설정
        ClearContent();
        TouchCatcher.SetActive(true);
        SkipButton.GetComponent<Button>().onClick.RemoveAllListeners();
        SkipButton.GetComponent<Button>().onClick.AddListener(OnSkip);

        // 실행 세팅
        currentGroup = groupID;
        groupEvents = matchedEvents;
        currentGroupIndex = 0;

        // 이벤트 출력
        DisplayCurrentEvent();

    }
    public void SaveEventData(ref SaveManager.SaveData data)
    {
        data.savedEventGroups = new List<int>(eventGroups); // 남은 이벤트 그룹 복사
        data.savedCurrentEventGroup = currentGroup;
        data.savedCurrentEvetnGroupIndex = currentGroupIndex;

        Debug.Log("[EventDisplay] 이벤트 데이터 저장 완료");
    }
    public void LoadEventData(SaveManager.SaveData data)
    {
        if (data.savedEventGroups == null || data.savedEventGroups.Count == 0) return;

        Debug.Log("이벤트 쪽 로드 시작 합니다");
        eventGroups = new List<int>(data.savedEventGroups);
        currentGroup = data.savedCurrentEventGroup;
        currentGroupIndex = data.savedCurrentEvetnGroupIndex;

        jsonManager ??= FindObjectOfType<JsonManager>();
        spriteBank ??= FindObjectOfType<SpriteBank>();

        eventList = jsonManager.GetRandomMainMasters("RandomEvents_Master_Event")
            .OrderBy(e => e.RandomEvent_Index)
            .ThenBy(e => e.Script_Index)
            .ToList();

        scriptEventsCache = jsonManager.GetRandomScriptMasters("Ran_Script_Master_Event");

        groupEvents = eventList
            .Where(e => e.RandomEvent_Index == currentGroup)
            .OrderBy(e => e.Script_Index)
            .ToList();

        currentEvent = groupEvents[currentGroupIndex];

        ClearContent();
        TouchCatcher.SetActive(true);
        SkipButton.GetComponent<Button>().onClick.RemoveAllListeners();
        SkipButton.GetComponent<Button>().onClick.AddListener(() => OnSkip());

        // ⛔ DisplayCurrentEvent() 호출 안함!
    }
}
