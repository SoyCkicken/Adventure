using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using Image = UnityEngine.UI.Image;
using System;


public class StoryDisplayManager : MonoBehaviour
{
    public Transform content;
    //이미지 프리팹 생성할 예정
    public GameObject ImagePrefab;
    //텍스트 프리팹 생성 예정
    public GameObject TextPrefab;
    public JsonManager jsonManager;
    public List<Story_Master_Main> storyList;
    private Story_Master_Main currentStory;
    public bool isSkip = false;
    public bool isTyping;
    public GameObject SkipButton;

    [Header("UI References")]
    //public TMP_Text sceneText;
    public Transform choiceButtonParent;
    public GameObject choiceButtonPrefab;
    StringBuilder stringBuilder = new StringBuilder();
    public List<GameObject> Testblocks = new List<GameObject>();
    //마지막 블록이 무엇인지 확인용
    GameObject lastBlock;
    private Action<Main_Script_Master_Main> onComplete;


    //public void DisplayNode(Script_Master_Main node, Action<Script_Master_Main> onComplete)
    //{
    //    this.onComplete = onComplete;
    //    // 노드 데이터 세팅
    //    currentStory = node;
    //    // 기존 렌더링 로직 호출
    //    DisplayCurrentStory();
    //}

    private void Awake()
    {
        SkipButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (isTyping)
                isSkip = true;
        });
    }
    void Start()
    {
        if (jsonManager == null)
        {
            jsonManager = FindObjectOfType<JsonManager>();
        }
        //OnStoryComplete.Invoke();

         storyList = jsonManager.GetStoryMainMasters("Story_Master_Main");
        Debug.Log($"StoryList Count: {(storyList != null ? storyList.Count : -1)}");
        if (storyList == null || storyList.Count == 0)
        {
            Debug.LogError("Story_Master 데이터가 없습니다.");
            return;
        }
        //Debug.Log($"불러온 스토리 데이터 개수: {storyList.Count}");
        //foreach (var item in storyList)
        //{
        //    Debug.Log($"Event_Index: {item.Event_Index}, Scene_Text: {item.Script_Text}");
        //}
        // 정렬 (챕터, 이벤트, 씬 순)
        //이러면 1~9챕터 1~9이벤트 1~9씬까지 알잘딱하게 정렬해줌
        storyList = storyList.OrderBy(s => s.Chapter_Index)
                             .ThenBy(s => s.Event_Index)
                             .ThenBy(s => s.Script_Index)
                             .ToList();

        currentStory = storyList[0];
        DisplayCurrentStory();
        //Debug.Log(storyList.Count);
        //총 18개가 들어가 있는지 확인
        //Story_Master_Custom_Format에도 블록으로 18개가 들어가 있는걸 확인했음
        //스킵버튼 활성화
        SkipButton.SetActive(true);
    }

    void DisplayCurrentStory()
    {
        //https://learn.microsoft.com/ko-kr/dotnet/api/system.text.stringbuilder?view=net-8.0
        // Script_Master_Main 데이터 불러오기
        List<Main_Script_Master_Main> scriptEvents = jsonManager.GetStoryMainScriptMasters("Main_Script_Master_Main");
        //Debug.Log(stringBuilder.ToString());
        // 현재 스토리의 Scene_Text(대상 스크립트 코드)를 찾아서 해당 KOR 값을 출력
        var matchingScript = scriptEvents.FirstOrDefault(sm => sm.Script_Code.Trim() == currentStory.Script_Text.Trim());
        //Debug.Log(Testblocks.Count);
        GameObject lastBlock = Testblocks.Count > 0
        ? Testblocks[Testblocks.Count - 1]
        : null;
        Debug.Log(matchingScript.displayType);
        bool isImage = matchingScript.displayType == "Image";
        Debug.Log(lastBlock);
        if (matchingScript != null)
        {
            //Debug.Log($"matchingScript.displayType의 대한 true false값 확인용 {matchingScript.displayType}");
            //이미지일때
            if (isImage)
            {
                //생성함수로 정리했음
                CreateImageBlock(matchingScript.KOR);
            }
            else
            {
                //첫 블록 일때 이거나 마지막 블록이 이미지일때
                if (lastBlock == null)
                {
                    CreateTextBlock(matchingScript.KOR);
                }
                //첫 블록이 아니고.마지막 블록이 이미지였다면
                else if (lastBlock.TryGetComponent<Image>(out _))
                {
                    CreateTextBlock(matchingScript.KOR);
                }
                else
                {
                    StartCoroutine(TypeTextEffect(matchingScript.KOR, lastBlock));
                }
            }
        }
        else
        {
            Debug.LogWarning("현재 스토리의 스크립트 텍스트를 찾을 수 없습니다.");
            //sceneText.text = currentStory.Scene_Text;
        }

        // 기존 선택지 버튼 제거
        foreach (Transform child in choiceButtonParent)
        {
            Destroy(child.gameObject);
        }

        // availableChoices: (destCode, displayText)
        List<(string destCode, string displayText)> availableChoices = new List<(string, string)>();

        // 여기서는 Choice1_Text, Choice2_Text, Choice3_Text가
        // 스크립트 코드(예: "MainScript_1_1_4" 또는 "MainScene_1_1_8")같이 선택지가 있을때만 작동
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

    // Script_Master_Main 리스트에서 스크립트 코드(code)에 해당하는 KOR 값을 반환 (없으면 code 자체)
    private string GetDisplayTextFromScript(string code, List<Main_Script_Master_Main> scriptEvents)
    {
        var match = scriptEvents.FirstOrDefault(sm => sm.Script_Code.Trim() == code.Trim());
        Debug.Log(match);
        if (match != null)
            return match.KOR;
        else
            return code;
    }

    void Update()
    {
        // 선택지가 없는 경우엔 (모두 "--" 이면) 화면 클릭시 자동 진행
        if (currentStory.Choice1_Text == "" &&
            currentStory.Choice2_Text == "" &&
            currentStory.Choice3_Text == "")
        {
            if (isTyping == false)
            {
                NextScene();
            }
        }
    }

    // 선택 버튼 클릭시 호출: newSceneCode는 Choice 텍스트(실제 값이 스크립트 코드임)
    void OnChoiceSelected(string newSceneCode)
    {
        // 만약 newSceneCode가 "MainScript"로 시작하면 "MainScene"으로 변환
        if (newSceneCode.StartsWith("MainScript"))
        {
            newSceneCode = newSceneCode.Replace("MainScript", "MainScene");
        }

        Story_Master_Main nextStory = FindStoryBySceneCode(newSceneCode);
        if (nextStory != null)
        {
            currentStory = nextStory;
            DisplayCurrentStory();
        }
        else
        {
            Debug.LogWarning("해당 Scene_Code를 가진 스토리를 찾을 수 없습니다: " + newSceneCode);
        }
    }

    void NextScene()
    {
        List<Main_Script_Master_Main> scriptEvents = jsonManager.GetStoryMainScriptMasters("Main_Script_Master_Main");
        //Debug.Log(stringBuilder.ToString());
        // 현재 스토리의 Scene_Text(대상 스크립트 코드)를 찾아서 해당 KOR 값을 출력
        var matchingScript = scriptEvents.FirstOrDefault(sm => sm.Script_Code.Trim() == currentStory.Script_Text.Trim());
        //Debug.Log($"matchingScript의 값을 출력을 위한 디버그 입니다  = {matchingScript.StoryBreak}");

        Story_Master_Main nextStory = storyList.FirstOrDefault(s =>
            s.Chapter_Index == currentStory.Chapter_Index &&
            s.Event_Index == currentStory.Event_Index &&
            s.Script_Index == currentStory.Script_Index + 1);


        if (nextStory == null || matchingScript.StoryBreak == "Break")
        {
            //Debug.LogError("다음 씬이 존재하지 않습니다. 이벤트가 끝났거나 다음 챕터로 전환해야 합니다.");

        }
        else
        {
            currentStory = nextStory;
            DisplayCurrentStory();
        }
    }

    Story_Master_Main FindStoryBySceneCode(string sceneCode)
    {
        return storyList.FirstOrDefault(s => s.Scene_Code.Trim() == sceneCode.Trim());
    }
    public IEnumerator TestdebugLog(string temp)
    {
        List<Story_Master_Main> scriptEvents = jsonManager.GetStoryMainMasters("Story_Master_Main");
        List<Main_Script_Master_Main> script_Master_Mains = jsonManager.GetStoryMainScriptMasters("Main_Script_Master_Main");
        //Debug.Log($"{script_Master_Mains}가 있는지 확인");
        foreach (Story_Master_Main ev in scriptEvents)
        {

            foreach (Main_Script_Master_Main sm in script_Master_Mains)
            {
                string temp2 = sm.Script_Code;//
                //Debug.Log(temp2);
                if (temp == temp2)
                {
                    yield return temp2;
                }
                else
                {
                    //Debug.Log($"temp의 값 : {temp}\ntemp2의 값 {temp2}");
                }
            }
        }
    }
    IEnumerator TypeTextEffect(string text , GameObject go)
    {
        SkipButton.SetActive(true);
        //Debug.Log("스킵버튼 활성화");
        //textComp.text = string.Empty; //문자열을 비우고
        //스트링빌더(한글자씩 추가해주는 함수)
        StringBuilder stringBuilder = new StringBuilder();
        //스킵 버튼 누르면 저장해놓은 값 그대로 넣어버림
        string temp = go.GetComponent<TMP_Text>().text + text;
        if (text != null)
        {
            //타입핑 중 인지 확인
            isTyping = true;
            for (int i = 0; i < text.Length; i++)
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
                go.GetComponent<TMP_Text>().text += text[i].ToString();
                yield return new WaitForSeconds(0.05f);
                //0.01초마다 한번씩 출력시킴
            }
            //char tempchar = stringBuilder[stringBuilder.Length -1];
            //Debug.Log(tempchar);
            
        }
        else
        {
            //RamEvent같은 경우 설명 같은게 하나도 없기 때문에 에러가 발생을 하는데 그걸 막고자 if문 사용했음
            yield break;
        }
        isTyping = false;
        SkipButton.SetActive(false);
        isSkip = false;
        //Debug.Log("스킵버튼 비활성화");
    }
    //지금 같은 경우 연 달아 출력 하는것은 가능
    //한글자씩 출력 하는것도 가능
    //그렇다면 지금 for문을 돌려서 문제가 생기는게 아닐까?
    //방식을 생각을 해봤는데 

    //생성 함수
    void CreateImageBlock(string matchingScript)
    {
        var go = Instantiate(ImagePrefab, content);
        Testblocks.Add(go);
        RectTransform rt = go.GetComponent<RectTransform>();
        Sprite sprite = Resources.Load<Sprite>("Images/" + matchingScript);
        //Debug.Log(matchingScript.KOR);
        if (sprite == null)
        {
            Debug.Log("프로그래머야 이게 뭐냐 버그났잖아!");
        }
        //Debug.Log(sprite);
        go.GetComponent<Image>().sprite = sprite;
    }
    void CreateTextBlock(string matchingScript)
    {
        var go = Instantiate(TextPrefab, content);
        Testblocks.Add(go);
        RectTransform rt = go.GetComponent<RectTransform>();
        //혹시 모르니 값 초기화
        go.GetComponent<TMP_Text>().text = string.Empty;
        //넣을 값이랑 text를 받아감
        StartCoroutine(TypeTextEffect(matchingScript, go));
    }

}

