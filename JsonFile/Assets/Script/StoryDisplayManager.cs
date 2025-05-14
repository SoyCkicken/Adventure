using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoryDisplayManager : MonoBehaviour
{
    public GameObject ImagePrefab;
    public GameObject TextPrefab;
    public GameObject SkipButton;
    public GameObject choiceButtonPrefab;

    public Transform content;
    public Transform choiceButtonParent;

    public JsonManager jsonManager;
    public List<Story_Master_Main> storyList;
    private Story_Master_Main currentStory;
    private int currentIndex = 0;
    public bool isSkip = false;
    public bool isTyping;

    private List<Main_Script_Master_Main> scriptEventsCache;
    [Header("UI References")]


    private StringBuilder stringBuilder = new StringBuilder();
    public List<GameObject> Testblocks = new List<GameObject>();

    // 콜백 저장용
    private Action onCompleteCallback;

    private void Awake()
    {
        SkipButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (isTyping)
                isSkip = true;
        });

        if (jsonManager == null)
            jsonManager = FindObjectOfType<JsonManager>();


    }
    void Start()
    {
        //this.StartMainStory(() =>
        //{
        //    Debug.Log("스토리 연출 완료!");
        //});
    }

    void Update()
    {
        // 선택지가 없는 경우엔 (모두 "--" 이면) 화면 클릭시 자동 진행
        //if (currentStory.Choice1_Text == "" &&
        //    currentStory.Choice2_Text == "" &&
        //    currentStory.Choice3_Text == "" 
        //    )
        //{
        //    if (isTyping == false)
        //    {
        //        NextScene(scriptEventsCache);
        //    }
        //}
    }

    /// <summary>
    /// 메인 스토리 연출 시작 (GameFlowManager에서 호출)
    /// </summary>
    public void StartMainStory(Action onComplete)
    {
        onCompleteCallback = onComplete;
        storyList = jsonManager.GetStoryMainMasters("Story_Master_Main");
        Debug.Log($"StoryList Count: {(storyList != null ? storyList.Count : -1)}");
        if (storyList == null || storyList.Count == 0)
        {
            Debug.LogError("Story_Master 파일을 불러오는 데 실패했습니다.");
            onCompleteCallback?.Invoke();
            return;
        }

        storyList = storyList.OrderBy(s => s.Chapter_Index)
                             .ThenBy(s => s.Event_Index)
                             .ThenBy(s => s.Script_Index)
                             .ToList();

        currentIndex = 0;
        scriptEventsCache = jsonManager.GetStoryMainScriptMasters("Main_Script_Master_Main");
        currentStory = storyList[currentIndex];
        SkipButton.SetActive(true);
        ClearContent();
        // 첫 시퀀스 표시
        DisplayCurrentStory(scriptEventsCache);
    }

    /// <summary>
    /// 메인 스토리 연출 일시 정지/중지
    /// </summary>
    public void StopMainStory()
    {
        StopAllCoroutines();
        // currentIndex는 마지막 진행 지점을 자동 보존합니다.
        ClearContent();
        SkipButton.SetActive(false);
    }

    void DisplayCurrentStory(List<Main_Script_Master_Main> scriptEvents)
    {
        // 기존 DisplayCurrentStory 내부 로직 그대로 유지
        var matchingScript = scriptEvents.FirstOrDefault(sm => sm.Script_Code.Trim() == currentStory.Script_Text.Trim());
        Debug.Log(matchingScript.KOR);
        GameObject lastBlock = Testblocks.Count > 0 ? Testblocks[Testblocks.Count - 1] : null;
        bool isImage = matchingScript.displayType == "Image";

        if (matchingScript != null)
        {
            if (isImage)
                CreateImageBlock(matchingScript.KOR);
            else
                HandleTextDisplay(matchingScript.KOR, lastBlock);
        }
        else
        {
            Debug.LogWarning("해당 스크립트를 찾지 못했습니다.");
        }

        // Choice 버튼 세팅, OnChoiceSelected에서 currentIndex++, 필요 시 onCompleteCallback 호출
        if (currentStory.Choice1_Text != "")
        {
            Debug.Log(scriptEvents);
            SetupChoices(scriptEvents);
        }

    }
    private void HandleTextDisplay(string text, GameObject lastBlock)
    {
        if (lastBlock == null || lastBlock.TryGetComponent<Image>(out _))
            CreateTextBlock(text);
        else
            StartCoroutine(TypeTextEffect(text, lastBlock));
    }


    // 필요 시 마지막 노드까지 모두 진행 후 onCompleteCallback 호출
    public void OnMainStoryComplete()
    {
        onCompleteCallback?.Invoke();
    }

    private void ClearContent()
    {
        foreach (var go in Testblocks)
            Destroy(go);
        Testblocks.Clear();
        foreach (Transform t in choiceButtonParent)
            Destroy(t.gameObject);
    }

    // 이미지 블록 생성
    private void CreateImageBlock(string spriteName)
    {
        var go = Instantiate(ImagePrefab, content);
        var img = go.GetComponent<Image>();
        img.sprite = Resources.Load<Sprite>($"Images/{spriteName}");
        Testblocks.Add(go);
    }

    // 텍스트 블록 생성 (초기화만)
    private void CreateTextBlock(string text)
    {
        var go = Instantiate(TextPrefab, content);
        var tmp = go.GetComponent<TMP_Text>();
        StartCoroutine(TypeTextEffect(text, go));
        Testblocks.Add(go);
    }

    // 타입라이터 이펙트
    private IEnumerator TypeTextEffect(string fullText, GameObject go)
    {
        SkipButton.SetActive(true);
        //Debug.Log("스킵버튼 활성화");
        //textComp.text = string.Empty; //문자열을 비우고
        //스트링빌더(한글자씩 추가해주는 함수)
        StringBuilder stringBuilder = new StringBuilder();
        //스킵 버튼 누르면 저장해놓은 값 그대로 넣어버림
        string temp = go.GetComponent<TMP_Text>().text + fullText;
        if (fullText != null)
        {
            //타입핑 중 인지 확인
            isTyping = true;
            for (int i = 0; i < fullText.Length; i++)
            {
                //버튼 누를때 활성화 되게 하면 될듯
                if (isSkip == true)
                {
                    go.GetComponent<TMP_Text>().text = temp.ToString();
                    break;
                }
                //한글자씩 추가
                //stringBuilder.Append(text[i]);
                //Debug.Log(stringBuilder);
                //받은 문자들을 text에 담아서 
                go.GetComponent<TMP_Text>().text += fullText[i].ToString();
                yield return new WaitForSeconds(0.05f);
                //0.01초마다 한번씩 출력시킴
            }
        }
        else
        {
            //RamEvent같은 경우 설명 같은게 하나도 없기 때문에 에러가 발생을 하는데 그걸 막고자 if문 사용했음
            yield break;
        }
        isTyping = false;
        SkipButton.SetActive(false);
        isSkip = false;
    }

    // 선택지 버튼 세팅
    private void SetupChoices(List<Main_Script_Master_Main> scriptEvents)
    {
        List<(string destCode, string displayText)> availableChoices = new List<(string, string)>();

        // 기존 버튼 삭제
        foreach (Transform t in choiceButtonParent) Destroy(t.gameObject);

        if (currentStory.Choice1_Text != "")
        {
            string code = currentStory.Choice1_Text;
            Debug.Log(code);
            string display = GetDisplayTextFromScript(code, scriptEvents);
            //Debug.Log($"테스트용 문자열입니다 {display}");
            availableChoices.Add((code, display));
        }
        if (currentStory.Choice2_Text != "")
        {
            string code = currentStory.Choice2_Text;
            string display = GetDisplayTextFromScript(code, scriptEvents);
            availableChoices.Add((code, display));
        }
        if (currentStory.Choice3_Text != "")
        {
            string code = currentStory.Choice3_Text;
            string display = GetDisplayTextFromScript(code, scriptEvents);
            availableChoices.Add((code, display));
        }
        CreateChoicebutton(availableChoices);
    }

    void CreateChoicebutton(List<(string destCode, string displayText)> availableChoices)
    {
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
    }
    void OnChoiceSelected(string newSceneCode)
    {
        // 만약 newSceneCode가 "MainScript"로 시작하면 "MainScene"으로 변환
        if (newSceneCode.StartsWith("MainScript"))
        {
            newSceneCode = newSceneCode.Replace("MainScript", "MainScene");
            //버튼 눌렸으면 삭제 시킴
            foreach (Transform child in choiceButtonParent)
            {
                Destroy(child.gameObject);
            }
        }

        Story_Master_Main nextStory = FindStoryBySceneCode(newSceneCode);
        if (nextStory != null)
        {
            currentStory = nextStory;
            DisplayCurrentStory(scriptEventsCache);
        }
        else
        {
            Debug.LogWarning("해당 Scene_Code를 가진 스토리를 찾을 수 없습니다: " + newSceneCode);
        }

    }
    Story_Master_Main FindStoryBySceneCode(string sceneCode)
    {
        return storyList.FirstOrDefault(s => s.Scene_Code.Trim() == sceneCode.Trim());
    }
    private string GetDisplayTextFromScript(string code, List<Main_Script_Master_Main> scriptEvents)
    {
        var match = scriptEvents.FirstOrDefault(sm => sm.Script_Code.Trim() == code.Trim());
        Debug.Log(match);
        if (match != null)
            return match.KOR;
        else
            return code;
    }
    void NextScene(List<Main_Script_Master_Main> scriptEvents)
    {

        var matchingScript = scriptEvents.FirstOrDefault(sm => sm.Script_Code.Trim() == currentStory.Script_Text.Trim());
        //Debug.Log($"matchingScript의 값을 출력을 위한 디버그 입니다  = {matchingScript.StoryBreak}");

        Story_Master_Main nextStory = storyList.FirstOrDefault(s =>
            s.Chapter_Index == currentStory.Chapter_Index &&
            s.Event_Index == currentStory.Event_Index &&
            s.Script_Index == currentStory.Script_Index + 1);


        if (nextStory == null || matchingScript.StoryBreak == "Break")
        {
            //Debug.LogError("다음 씬이 존재하지 않습니다. 이벤트가 끝났거나 다음 챕터로 전환해야 합니다.");
            OnMainStoryComplete();

        }
        else
        {
            currentStory = nextStory;
            DisplayCurrentStory(scriptEventsCache);
        }
    }

}
