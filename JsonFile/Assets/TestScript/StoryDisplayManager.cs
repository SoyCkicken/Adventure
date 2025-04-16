using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class StoryDisplayManager : MonoBehaviour
{
    public JsonManager jsonManager;
    public List<Story_Master> storyList;
    private Story_Master currentStory;

    [Header("UI References")]
    public TMP_Text sceneText;
    public Transform choiceButtonParent;
    public GameObject choiceButtonPrefab;

    void Start()
    {
        if (jsonManager == null)
        {
            jsonManager = FindObjectOfType<JsonManager>();
        }

        storyList = jsonManager.storyMasters;
        if (storyList == null || storyList.Count == 0)
        {
            Debug.LogError("Story_Master 데이터가 없습니다.");
            return;
        }

        // 정렬 (챕터, 이벤트, 씬 순)
        storyList = storyList.OrderBy(s => s.Chapter_Index)
                             .ThenBy(s => s.Event_Index)
                             .ThenBy(s => s.Scenc_Index)
                             .ToList();

        currentStory = storyList[0];
        DisplayCurrentStory();
    }

    void DisplayCurrentStory()
    {
        // Script_Master_Main 데이터 불러오기
        List<Script_Master_Main> scriptEvents = jsonManager.scriptMasterMains;

        // 현재 스토리의 Scene_Text(대상 스크립트 코드)를 찾아서 해당 KOR 값을 출력
        var matchingScript = scriptEvents.FirstOrDefault(sm => sm.Script_Code.Trim() == currentStory.Scene_Text.Trim());
        if (matchingScript != null)
        {
            sceneText.text = matchingScript.KOR;
        }
        else
        {
            Debug.LogWarning("현재 스토리의 스크립트 텍스트를 찾을 수 없습니다.");
            sceneText.text = currentStory.Scene_Text;
        }

        // 기존 선택지 버튼 제거
        foreach (Transform child in choiceButtonParent)
        {
            Destroy(child.gameObject);
        }

        // availableChoices: (destCode, displayText)
        List<(string destCode, string displayText)> availableChoices = new List<(string, string)>();

        // 여기서는 Choice1_Text, Choice2_Text, Choice3_Text가 스크립트 코드(예: "MainScript_1_1_4" 또는 "MainScene_1_1_8")를 담고 있다고 가정
        if (currentStory.Choice1_Text != "--")
        {
            string code = currentStory.Choice1_Text;
            Debug.Log(code);
            string display = GetDisplayTextFromScript(code, scriptEvents);
            Debug.Log($"테스트용 문자열입니다 {display}");
            availableChoices.Add((code, display));
        }
        if (currentStory.Choice2_Text != "--")
        {
            string code = currentStory.Choice2_Text;
            string display = GetDisplayTextFromScript(code, scriptEvents);
            availableChoices.Add((code, display));
        }
        if (currentStory.Choice3_Text != "--")
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
    private string GetDisplayTextFromScript(string code, List<Script_Master_Main> scriptEvents)
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
        if (currentStory.Choice1_Text == "--" &&
            currentStory.Choice2_Text == "--" &&
            currentStory.Choice3_Text == "--")
        {
            if (Input.GetMouseButtonDown(0))
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

        Story_Master nextStory = FindStoryBySceneCode(newSceneCode);
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
        Story_Master nextStory = storyList.FirstOrDefault(s =>
            s.Chapter_Index == currentStory.Chapter_Index &&
            s.Event_Index == currentStory.Event_Index &&
            s.Scenc_Index == currentStory.Scenc_Index + 1);

        if (nextStory != null)
        {
            currentStory = nextStory;
            DisplayCurrentStory();
        }
        else
        {
            Debug.Log("다음 씬이 존재하지 않습니다. 이벤트가 끝났거나 다음 챕터로 전환해야 합니다.");
        }
    }

    Story_Master FindStoryBySceneCode(string sceneCode)
    {
        return storyList.FirstOrDefault(s => s.Scene_Code.Trim() == sceneCode.Trim());
    }
    public IEnumerator TestdebugLog(string temp)
    {
        List<Story_Master> scriptEvents = jsonManager.storyMasters;
        List<Script_Master_Main> script_Master_Mains = jsonManager.scriptMasterMains;
        //Debug.Log($"{script_Master_Mains}가 있는지 확인");
        foreach (Story_Master ev in scriptEvents)
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
}
