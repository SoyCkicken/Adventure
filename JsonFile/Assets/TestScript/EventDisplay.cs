using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Text;

public class EventDisplay : MonoBehaviour
{
    [Header("Prefabs & UI")]
    public Transform content;
    public GameObject ImagePrefab;
    public GameObject TextPrefab;
    public Transform choiceButtonParent;
    public GameObject choiceButtonPrefab;
    public GameObject SkipButton;

    [Header("Data Manager")]
    public JsonManager jsonManager;
    private List<RandomEvent> randomEvents;
    private List<int> eventGroups;      // 유니크한 RandomEvent_Index 리스트
    private int currentGroup;           // 현재 실행 중인 그룹 인덱스

    private RandomEvent currentEvent;
    private bool isTyping, isSkip;
    private List<GameObject> Testblocks = new List<GameObject>();
    private System.Random rng = new System.Random();
    private List<(string destCode, string displayText)> availableChoices = new List<(string, string)>();
    private void Awake()
    {
        SkipButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (isTyping) isSkip = true;
        });
    }

    void Start()
    {
        if (jsonManager == null) jsonManager = FindObjectOfType<JsonManager>();

        randomEvents = jsonManager.randomEvents;
        if (randomEvents == null || randomEvents.Count == 0)
        {
            Debug.LogError("RandomEvent 데이터가 없습니다.");
            return;
        }

        // 그룹 인덱스만 추려서 리스트 생성
        eventGroups = randomEvents
            .Select(e => e.RandomEvent_Index)
            .Distinct()
            .ToList();

        // 첫 그룹 무작위 픽
        PickNewGroup();
        SkipButton.SetActive(true);
    }

    /// <summary>
    /// eventGroups 에서 무작위로 하나 골라 currentEvent 초기화 후 출력
    /// </summary>
    void PickNewGroup()
    {
        // 클리어 UI, 버퍼
        foreach (var go in Testblocks) Destroy(go);
        Testblocks.Clear();

        currentGroup = eventGroups[rng.Next(eventGroups.Count)];
        // 해당 그룹 내 첫 스크립트 인덱스 가져오기
        currentEvent = randomEvents
            .Where(e => e.RandomEvent_Index == currentGroup)
            .OrderBy(e => e.Script_Index)
            .First();

        DisplayCurrentEvent();
    }

    /// <summary>
    /// currentEvent 를 화면에 표시
    /// </summary>
    void DisplayCurrentEvent()
    {
        availableChoices.Clear();
        // 스크립트 텍스트/이미지를 처리
        var scriptEvents = jsonManager.scriptMasterEvents;
        var matching = scriptEvents
            .FirstOrDefault(sm => sm.Script_Code.Trim() == currentEvent.Event_Text.Trim());

        if (matching != null)
        {
            bool isImage = matching.displayType == "Image";

            if (isImage)
            {
                CreateImageBlock(matching.KOR);
            }
            else
            {
                // 마지막 블록이 이미지면 새로, 텍스트면 이어쓰기
                var last = Testblocks.Count > 0 ? Testblocks.Last() : null;
                Debug.Log(last);
                if (last == null || last.TryGetComponent<Image>(out _))
                {
                    CreateTextBlock(matching.KOR);
                }
                else
                {
                    StartCoroutine(TypeTextEffect(matching.KOR, last));
                }
            }
        }
        else
        {
            Debug.LogWarning("스크립트 미발견: " + currentEvent.Event_Text);
        }

        //선택지 버튼(랜덤 이벤트는 보통 없으니 이부분은 필요에 따라 삭제)
        //기존 선택지 버튼 제거
                foreach (Transform child in choiceButtonParent)
        {
            Destroy(child.gameObject);
        }

        // availableChoices: (destCode, displayText)
        

        // 여기서는 Choice1_Text, Choice2_Text, Choice3_Text가
        // 스크립트 코드(예: "MainScript_1_1_4" 또는 "MainScene_1_1_8")같이 선택지가 있을때만 작동
        if (currentEvent.Choice1_Text != "--")
        {
            string code = currentEvent.Choice1_Text;

            string display = GetDisplayTextFromScript(code, scriptEvents);
            Debug.Log($"선택지 1번의 값 : {code} \n display의 값 : {display}");
            //Debug.Log($"테스트용 문자열입니다 {display}");`
            availableChoices.Add((code, display));
        }
        if (currentEvent.Choice2_Text != "--")
        {
            string code = currentEvent.Choice2_Text;
            string display = GetDisplayTextFromScript(code, scriptEvents);
            Debug.Log($"선택지 2번의 값 : {code} \n display의 값 : {display}");
            availableChoices.Add((code, display));
        }
        if (currentEvent.Choice3_Text != "--")
        {
            string code = currentEvent.Choice3_Text;

            string display = GetDisplayTextFromScript(code, scriptEvents);
            availableChoices.Add((code, display));
        }

        if (availableChoices.Count > 0)
        {
            // 선택지가 있으면 버튼 생성
            foreach (var choice in availableChoices)
            {
                GameObject buttonObj = Instantiate(choiceButtonPrefab, choiceButtonParent);
                Button btn = buttonObj.GetComponent<Button>();
                TMP_Text btnText = buttonObj.GetComponentInChildren<TMP_Text>();
                if (btnText != null)
                    btnText.text = choice.displayText;

                btn.onClick.AddListener(() => { OnChoiceSelected(choice.destCode); });
            }
        }
        // 선택지가 없으면 Update()에서 클릭시 자동 진행함.
    }

    RandomEvent FindStoryBySceneCode(string sceneCode)
    {
        Debug.Log(sceneCode);
        return randomEvents.FirstOrDefault(s => s.Random_Event_ID.Trim() == sceneCode.Trim());
    }

    private string GetDisplayTextFromScript(string code, List<Script_Master_Event> scriptEvents)
    {
        var match = scriptEvents.FirstOrDefault(sm => sm.Script_Code.Trim() == code.Trim());
        Debug.Log(match);
        if (match != null)
            return match.KOR;
        else
            return code;
    }

    void OnChoiceSelected(string newSceneCode)
    {
        Debug.Log(newSceneCode);
        // 만약 newSceneCode가 "MainScript"로 시작하면 "MainScene"으로 변환
        if (newSceneCode.StartsWith("EventScript"))
        {
            newSceneCode = newSceneCode.Replace("EventScript", "EventScene");
        }

        RandomEvent nextStory = FindStoryBySceneCode(newSceneCode);
        Debug.Log(nextStory);
        if (nextStory != null)
        {
            currentEvent = nextStory;
            DisplayCurrentEvent();
        }
        else
        {
            Debug.LogWarning($"해당 Scene_Code를 가진 스토리를 찾을 수 없습니다: \n newSceneCode{newSceneCode}  \n currentEvent = {currentEvent}");
        }
    }

    /// <summary>
    /// 화면 클릭 또는 ▶ 버튼 누르면 다음으로
    /// </summary>
    void Update()
    {
        if (!isTyping && Input.GetMouseButtonDown(0) && availableChoices.Count == 0)
            NextScene();
    }

    void NextScene()
    {
        // 현재 스크립트가 Break 포인트인지 확인
        var script = jsonManager.scriptMasterEvents
            .FirstOrDefault(sm => sm.Script_Code.Trim() == currentEvent.Event_Text.Trim());
        if (script != null && script.EventBreak == "Break")
        {
            PickNewGroup();
            return;
        }

        // 같은 그룹 내에서 다음 Script_Index 찾기
        var groupList = randomEvents
            .Where(e => e.RandomEvent_Index == currentGroup)
            .OrderBy(e => e.Script_Index)
            .ToList();

        int idx = groupList.FindIndex(e => e.Script_Index == currentEvent.Script_Index);
        int nextIdx = idx + 1;

        if (nextIdx < groupList.Count)
        {
            currentEvent = groupList[nextIdx];
            DisplayCurrentEvent();
        }
        else
        {
            // 그룹 끝 → 새 그룹으로
            PickNewGroup();
        }
    }

    //——— UI 생성 헬퍼 ———
    void CreateImageBlock(string spriteName)
    {
        var go = Instantiate(ImagePrefab, content);
        Testblocks.Add(go);
        var img = go.GetComponent<Image>();
        var spr = Resources.Load<Sprite>($"Images/{spriteName}");
        if (spr != null) img.sprite = spr;
        else Debug.LogWarning("이미지 로드 실패: " + spriteName);
    }

    void CreateTextBlock(string text)
    {
        var go = Instantiate(TextPrefab, content);
        Testblocks.Add(go);
        var tmp = go.GetComponent<TMP_Text>();
        tmp.text = "";
        StartCoroutine(TypeTextEffect(text, go));
    }

    IEnumerator TypeTextEffect(string text, GameObject go)
    {
        isTyping = true;
        isSkip = false;
        var tmp = go.GetComponent<TMP_Text>();
        SkipButton.SetActive(true);
        string temp = go.GetComponent<TMP_Text>().text + text;
        Debug.Log($"tmp의 텍스트 값입니다{tmp.text}");
        Debug.Log($"입력 받은 TEXT 값입니다{text}");
        foreach (char c in text)
        {
            //여기부분 지금 오류 있음 버튼 누르면 스킵되면서 강제로 TEXT값이 들어가고 있는데 이부분 수정 필요
            if (isSkip) { tmp.text = temp; break; }
            tmp.text += c;
            yield return new WaitForSeconds(0.05f);
        }
        Debug.Log($"tmp의 텍스트 값입니다{tmp.text}");
        SkipButton.SetActive(false);
        isTyping = false;
    }
}

