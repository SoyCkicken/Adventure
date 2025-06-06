using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Runtime.CompilerServices;
using UnityEngine.U2D;


public class StoryDisplayManager : MonoBehaviour
{
    public GameObject ImagePrefab;
    public GameObject TextPrefab;
    public GameObject SkipButton;
    public GameObject TouchCatcher;
    public GameObject choiceButtonPrefab;
    public ScrollRect scrollRect;         // 에디터에서 연결
    public Transform content;
    public Transform choiceButtonParent;
    public FontSizeManager fontSizeManager;
    public JsonManager jsonManager;
    public List<Story_Master_Main> storyList;
    public Story_Master_Main currentStory;
    private int currentIndex = 0;
    public int currentStoryIndex = 0;
    public bool isSkip = false;
    public bool isTyping;
    private string winScriptCode;
    private string loseScriptCode;

    public SpriteBank spriteBank;

    private List<Main_Script_Master_Main> scriptEventsCache;
    public event Action<string> OnBattleJoin;
    [Header("UI References")]
    public List<GameObject> Testblocks = new List<GameObject>();

    // 콜백 저장용
    private Action onCompleteCallback;

    /// <summary>
    /// 메인 스토리 연출 시작 (GameFlowManager에서 호출)
    /// </summary>
    /// 

    private void OnSkip()
    {
        if (isTyping)
            isSkip = true;
    }
    public void StartMainStory(Action onComplete)
    {
        currentStoryIndex++;
        SkipButton.GetComponent<Button>().onClick.RemoveAllListeners();
        TouchCatcher.GetComponent<TouchCatcher>().onTapOutsideScrollView+= () =>
        {
            OnSkip();
        };

        if (jsonManager == null)
            jsonManager = FindObjectOfType<JsonManager>();
        onCompleteCallback = onComplete;
        storyList = jsonManager.GetStoryMainMasters("Story_Master_Main");
        Debug.Log($"StoryList Count: {(storyList != null ? storyList.Count : -1)}");
        if (storyList == null || storyList.Count == 0)
        {
            Debug.LogError("Story_Master 파일을 불러오는 데 실패했습니다.");
            onCompleteCallback?.Invoke();
            return;
        }

        storyList = storyList
          .Where(s =>s.Event_Index == currentStoryIndex)
          .OrderBy(e => e.Script_Index)
          .ToList();
        currentIndex = 0;
        scriptEventsCache = jsonManager.GetStoryMainScriptMasters("Main_Script_Master_Main");
        currentStory = storyList[currentIndex];
        TouchCatcher.SetActive(true);
        ClearContent();
        // 첫 시퀀스 표시
        DisplayCurrentStory();
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

    void DisplayCurrentStory()
    {
        // 기존 DisplayCurrentStory 내부 로직 그대로 유지
        var matchingScript = scriptEventsCache.FirstOrDefault(sm => sm.Script_Code.Trim() == currentStory.Script_Text.Trim());
        //Debug.Log(matchingScript.KOR);
        GameObject lastBlock = Testblocks.Count > 0 ? Testblocks[Testblocks.Count - 1] : null;
        if (matchingScript == null)
        {
            SkipButton.GetComponent<Button>().onClick.RemoveAllListeners();
            SkipButton.SetActive(true);
            SkipButton.GetComponent<Button>().onClick.AddListener(() => OnMainStoryComplete());
        }
        else if (matchingScript != null)
        {
            switch (matchingScript.displayType)
            {
                case "IMAGE":
                    Debug.Log("이미지 생성에 들어왔습니다");
                    CreateImageBlock(matchingScript.KOR);
                    break;
                case "TEXT":
                    Debug.Log("텍스트 생성에 들어왔습니다");
                    HandleTextDisplay(matchingScript.KOR, lastBlock);
                    if (currentStory.Choice1_Text != "")
                    {
                        SetupChoices();
                    }
                    break;
                case "BATTLE":
                    Debug.Log("배틀에 들어왔습니다");
                    Debug.Log(matchingScript.KOR);
                    winScriptCode = matchingScript.NEXTWIN?.Trim();
                    loseScriptCode = matchingScript.NEXTLOSE?.Trim();
                    OnBattleJoin?.Invoke(matchingScript.KOR);
                    break;
            }
        }
        else
        {
            Debug.LogWarning("해당 스크립트를 찾지 못했습니다.");
        }

        // Choice 버튼 세팅, OnChoiceSelected에서 currentIndex++, 필요 시 onCompleteCallback 호출


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

        Sprite s = spriteBank.Load(spriteName);
        //img.sprite = Resources.Load<Sprite>($"Images/{spriteName}");
        if (s != null)
        {
            img.sprite = s;
        }
        
        Testblocks.Add(go);
        NextScene();
    }

    // 텍스트 블록 생성 (초기화만)
    private void CreateTextBlock(string text)
    {
        var go = Instantiate(TextPrefab, content);
        TMP_Text tmp = go.GetComponentInChildren<TMP_Text>();
        fontSizeManager.Register(tmp);
        //var tmp = go.GetComponent<TMP_Text>();
        StartCoroutine(TypeTextEffect(text, go));
        Testblocks.Add(go);
    }

    // 타입라이터 이펙트
    private IEnumerator TypeTextEffect(string fullText, GameObject go)
    {
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
                scrollRect.verticalNormalizedPosition = 0f;
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
        Canvas.ForceUpdateCanvases();

        // 2) 스크롤을 맨 아래(또는 맨 위)로 이동
        //    verticalNormalizedPosition == 1 → 맨 위, 0 → 맨 아래
        

        isTyping = false;
        SkipButton.SetActive(false);
        isSkip = false;
        NextScene();
    }

    // 선택지 버튼 세팅
    private void SetupChoices()
    {
        List<(string destCode, string displayText)> availableChoices = new List<(string, string)>();

        // 기존 버튼 삭제
        foreach (Transform t in choiceButtonParent) Destroy(t.gameObject);

        if (currentStory.Choice1_Text != "")
        {
            string code = currentStory.Choice1_Text;
            Debug.Log(code);
            string display = GetDisplayTextFromScript(code, scriptEventsCache);
            //Debug.Log($"테스트용 문자열입니다 {display}");
            availableChoices.Add((code, display));
        }
        if (currentStory.Choice2_Text != "")
        {
            string code = currentStory.Choice2_Text;
            string display = GetDisplayTextFromScript(code, scriptEventsCache);
            availableChoices.Add((code, display));
        }
        if (currentStory.Choice3_Text != "")
        {
            string code = currentStory.Choice3_Text;
            string display = GetDisplayTextFromScript(code, scriptEventsCache);
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
            DisplayCurrentStory();
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
    public void NextScene()
    {
        if (isTyping) return;
        var next = storyList
        .FirstOrDefault(s => s.Chapter_Index == currentStory.Chapter_Index &&
                             s.Event_Index == currentStory.Event_Index &&
                             s.Script_Index == currentStory.Script_Index + 1);
        //Debug.Log(next.Script_Text);
        var choices = new[]
   {
        currentStory.Choice1_Text,
        currentStory.Choice2_Text,
        currentStory.Choice3_Text
    }.Where(c => !string.IsNullOrEmpty(c)).ToList();
        Debug.Log(choices.Count);
        if (choices.Count == 1)
        {
            OnChoiceSelected(choices[0]);
            return;
        }
        else if (choices.Count > 1)
        {
            // 다중 분기는 SetupChoices() 에서 버튼 클릭으로 처리됐을 것
            return;
        }

        var script = scriptEventsCache
        .FirstOrDefault(sm => sm.Script_Code.Trim() == currentStory.Script_Text.Trim());
        if (script != null && script.StoryBreak == "Break")
        {
            Debug.Log("브레이크문 들어왔습니다");
            SkipButton.GetComponent<Button>().onClick.RemoveAllListeners();
            SkipButton.SetActive(true);
            SkipButton.GetComponent<CanvasGroup>().blocksRaycasts = true;
            SkipButton.GetComponent<Button>().onClick.AddListener(() => {
                OnMainStoryComplete();
                SkipButton.SetActive(false); });

            
            return;
        }
        if (next != null)
        {
            currentIndex = storyList.IndexOf(next);
            currentStory = next;
            DisplayCurrentStory();
        }
        else
        {
            Debug.Log("아무조건에도 맞지 않습니다");
            SkipButton.GetComponent<Button>().onClick.RemoveAllListeners();
            SkipButton.SetActive(true);
            SkipButton.GetComponent<Button>().onClick.AddListener(() => {
                OnMainStoryComplete();
                SkipButton.SetActive(false);
            });
        }
    }

    public void WinBattle(bool battle)
    {
        string nextCode = (battle == true) ? winScriptCode : loseScriptCode;
        Debug.Log("전투 종료가 되어서 해당 부분으로 넘어갔습니다");
        var nextNode = storyList.FirstOrDefault(s => s.Script_Text.Trim() == nextCode.Trim());
        if (nextNode == null)
        {
            Debug.LogWarning($"다음 스크립트를 찾을 수 없습니다: {nextCode}");
            OnMainStoryComplete();
            return;
        }
        currentStory = nextNode;
        currentIndex = storyList.IndexOf(nextNode);
        ClearContent();
        DisplayCurrentStory();
    }
}