
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using Image = UnityEngine.UI.Image;

public class EventDisplay : MonoBehaviour
{
    public Transform content;
    //이미지 프리팹 생성할 예정
    public GameObject ImagePrefab;
    //텍스트 프리팹 생성 예정
    public GameObject TextPrefab;
    public JsonManager jsonManager;
    public List<RandomEvent> randomEvent;
    private RandomEvent currentEvent;
    public bool isSkip = false;
    public bool isTyping;
    public GameObject SkipButton;

    [Header("UI References")]
    //public TMP_Text sceneText;
    public Transform choiceButtonParent;
    public GameObject choiceButtonPrefab;
    StringBuilder stringBuilder = new StringBuilder();
    public List<GameObject> Testblocks = new List<GameObject>();


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

        randomEvent = jsonManager.randomEvents;
        if (randomEvent == null || randomEvent.Count == 0)
        {
            Debug.LogError("Story_Master 데이터가 없습니다.");
            return;
        }

        // 정렬 (챕터, 이벤트, 씬 순)
        //이러면 1~9챕터 1~9이벤트 1~9씬까지 알잘딱하게 정렬해줌
        randomEvent = randomEvent.OrderBy(s => s.RandomEvent_Index)
                             .ThenBy(s => s.Script_Index)
                             .ThenBy(s => s.Chapter_Index)
                             .ToList();

        currentEvent = randomEvent[0];
        DisplayCurrentEvent();
        //Debug.Log(storyList.Count);
        //총 18개가 들어가 있는지 확인
        //Story_Master_Custom_Format에도 블록으로 18개가 들어가 있는걸 확인했음
        //스킵버튼 활성화
        SkipButton.SetActive(true);
    }

    void DisplayCurrentEvent()
    {
        //https://learn.microsoft.com/ko-kr/dotnet/api/system.text.stringbuilder?view=net-8.0
        // Script_Master_Main 데이터 불러오기
        List<Script_Master_Event> scriptEvents = jsonManager.scriptMasterEvents;
        //Debug.Log(stringBuilder.ToString());
        // 현재 스토리의 Scene_Text(대상 스크립트 코드)를 찾아서 해당 KOR 값을 출력
        var matchingScript = scriptEvents.FirstOrDefault(sm => sm.Script_Code.Trim() == currentEvent.Event_Text.Trim());
        Debug.Log(matchingScript);
        if (matchingScript != null)
        {
            bool isImage = matchingScript.displayType == "Image";
            Debug.Log($"matchingScript.displayType의 대한 true false값 확인용 {matchingScript.displayType}");
            //이미지일때
            if (isImage)
            {
                var go = Instantiate(ImagePrefab, content);
                Testblocks.Add(go);
                RectTransform rt = go.GetComponent<RectTransform>();
                Sprite sprite = Resources.Load<Sprite>("Images/" + matchingScript.KOR);
                //Debug.Log(matchingScript.KOR);
                if (sprite == null)
                {
                    Debug.Log("프로그래머야 이게 뭐냐 버그났잖아!");
                }
                //Debug.Log(sprite);
                go.GetComponent<Image>().sprite = sprite;
                //stringBuilder.Append(matchingScript.KOR);
                //Debug.Log(stringBuilder.ToString());
                //sceneText.text = stringBuilder.ToString();
            }
            //첫 블록 일때
            else if (Testblocks.Count == 0)
            {
                var go = Instantiate(TextPrefab, content);
                Testblocks.Add(go);
                RectTransform rt = go.GetComponent<RectTransform>();
                //혹시 모르니 값 초기화
                go.GetComponent<TMP_Text>().text = string.Empty;
                //넣을 값이랑 text를 받아감
                StartCoroutine(TypeTextEffect(matchingScript.KOR, go));
            }
            //첫 블록이 아니고 마지막 블록이 이미지가 아닐때
            else
            {
                var lastUi = Testblocks[Testblocks.Count - 1];
                //matchingScript.kor + 마지막 블록 게임 오브젝트를 받아옴 
                StartCoroutine(TypeTextEffect(matchingScript.KOR, lastUi));
                //Debug.Log($"{ev.KOR}\n글자수 : {ev.KOR.Length}");
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
        if (currentEvent.Choice1_Text != "--")
        {
            string code = currentEvent.Choice1_Text;

            string display = GetDisplayTextFromScript(code, scriptEvents);
            Debug.Log($"선택지 1번의 값 : {code} \n display의 값 : {display}");
            //Debug.Log($"테스트용 문자열입니다 {display}");
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

    // Script_Master_Main 리스트에서 스크립트 코드(code)에 해당하는 KOR 값을 반환 (없으면 code 자체)
    private string GetDisplayTextFromScript(string code, List<Script_Master_Event> scriptEvents)
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
        if (currentEvent.Choice1_Text == "--" &&
            currentEvent.Choice2_Text == "--" &&
            currentEvent.Choice3_Text == "--")
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

    void NextScene()
    {
        List<Script_Master_Event> scriptEvents = jsonManager.scriptMasterEvents;
        //Debug.Log(stringBuilder.ToString());
        // 현재 스토리의 Scene_Text(대상 스크립트 코드)를 찾아서 해당 KOR 값을 출력
        var matchingScript = scriptEvents.FirstOrDefault(sm => sm.Script_Code.Trim() == currentEvent.Event_Text.Trim());
        Debug.Log($"matchingScript의 값을 출력을 위한 디버그 입니다  = {matchingScript.EventBreak} , {matchingScript.KOR}");
        Debug.LogError("NextScene에서 에러 발생중 고쳐야됨");
        RandomEvent nextStory = randomEvent.FirstOrDefault(s =>
            s.RandomEvent_Index == currentEvent.RandomEvent_Index &&
            s.Script_Index == currentEvent.Script_Index+1);
        Debug.LogError($"nextStory의 값 : {nextStory.Random_Event_ID}");


        if (nextStory == null || matchingScript.EventBreak == "Break")
        {
            Debug.LogError("다음 씬이 존재하지 않습니다. 이벤트가 끝났거나 다음 챕터로 전환해야 합니다.");

        }
        else
        {
            currentEvent = nextStory;
            DisplayCurrentEvent();
        }
    }

    RandomEvent FindStoryBySceneCode(string sceneCode)
    {
        Debug.Log(sceneCode);
        return randomEvent.FirstOrDefault(s => s.Random_Event_ID.Trim() == sceneCode.Trim());
    }
    public IEnumerator TestdebugLog(string temp)
    {
        List<RandomEvent> scriptEvents = jsonManager.randomEvents;
        List<Script_Master_Main> script_Master_Mains = jsonManager.scriptMasterMains;
        //Debug.Log($"{script_Master_Mains}가 있는지 확인");
        foreach (RandomEvent ev in scriptEvents)
        {

            foreach (Script_Master_Main sm in script_Master_Mains)
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
    IEnumerator TypeTextEffect(string text, GameObject go)
    {
        SkipButton.SetActive(true);
        Debug.Log("스킵버튼 활성화");
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
        Debug.Log("스킵버튼 비활성화");
    }
    //지금 같은 경우 연 달아 출력 하는것은 가능
    //한글자씩 출력 하는것도 가능
    //그렇다면 지금 for문을 돌려서 문제가 생기는게 아닐까?
    //방식을 생각을 해봤는데 


}
