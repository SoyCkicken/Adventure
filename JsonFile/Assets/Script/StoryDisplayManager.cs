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
    public PlayerState playerState;
    public SpriteBank spriteBank;
    private Dictionary<string, List<Main_SuccessRate_Master_Main>> _mainSuccessRateByScene = new();
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
        if (storyList.Count == 0)
        {
            Debug.LogWarning("스토리 다 썼음!!!!!!!!!!!!!!!!");
            gameOver();
        }
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
                    HandleTextDisplayWithChoice(matchingScript.KOR, lastBlock);
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
    private void HandleTextDisplayWithChoice(string text, GameObject lastBlock)
    {
        if (lastBlock == null || lastBlock.TryGetComponent<Image>(out _))
            CreateTextBlock(text);
        else
            StartCoroutine(TypeTextEffectWithChoice(text, lastBlock));
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
        StartCoroutine(TypeTextEffectWithChoice(text, go));
        Testblocks.Add(go);
    }

    // 타입라이터 이펙트
    private IEnumerator TypeTextEffectWithChoice(string fullText, GameObject go)
    {
        TMP_Text tmp = go.GetComponentInChildren<TMP_Text>();
        isTyping = true;
        string complete = tmp.text + fullText;

        for (int i = 0; i < fullText.Length; i++)
        {
            if (isSkip)
            {
                tmp.text = complete;
                scrollRect.verticalNormalizedPosition = 0f;
                Canvas.ForceUpdateCanvases();
                break;
            }
            scrollRect.verticalNormalizedPosition = 0f;
            Canvas.ForceUpdateCanvases();
            tmp.text += fullText[i];
            
            yield return new WaitForSeconds(0.05f);
        }

        isTyping = false;
        isSkip = false;
        SkipButton.SetActive(false);
        scrollRect.verticalNormalizedPosition = 0f;
        Canvas.ForceUpdateCanvases();

        // 타이핑이 끝난 후에 선택지 출력
        if (!string.IsNullOrEmpty(currentStory.Choice1_Text) ||
            !string.IsNullOrEmpty(currentStory.Choice2_Text) ||
            !string.IsNullOrEmpty(currentStory.Choice3_Text))
        {
            SetupChoices();
        }
        else
        {
            NextScene();
        }
    }

    // 선택지 버튼 세팅
    private void SetupChoices()
    {
        List<(string destCode, string displayText, int choiceNo)> availableChoices = new();
        foreach (Transform t in choiceButtonParent) Destroy(t.gameObject);

        if (!string.IsNullOrEmpty(currentStory.Choice1_Text))
            availableChoices.Add((currentStory.Choice1_Text, GetDisplayTextFromScript(currentStory.Choice1_Text, scriptEventsCache), 1));
        if (!string.IsNullOrEmpty(currentStory.Choice2_Text))
            availableChoices.Add((currentStory.Choice2_Text, GetDisplayTextFromScript(currentStory.Choice2_Text, scriptEventsCache), 2));
        if (!string.IsNullOrEmpty(currentStory.Choice3_Text))
            availableChoices.Add((currentStory.Choice3_Text, GetDisplayTextFromScript(currentStory.Choice3_Text, scriptEventsCache), 3));

        var successRateList = jsonManager.GetSuccessRatesMainByScene(currentStory.Scene_Code);

        foreach (var (destCode, displayText, choiceNo) in availableChoices)
        {
            GameObject buttonObj = Instantiate(choiceButtonPrefab, choiceButtonParent);
            TMP_Text btnText = buttonObj.GetComponentInChildren<TMP_Text>();
            if (btnText != null) btnText.text = displayText;

            var rateData = successRateList.FirstOrDefault(r => r.Choice_No == choiceNo);
            Button btn = buttonObj.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                if (rateData != null)
                {
                    float rate = EvaluateFormula(rateData.Success_Formula);
                    bool isSuccess = UnityEngine.Random.value < rate;
                    Debug.Log($"성공 했는지 실패 했는지 여부 :{isSuccess}");
                    string nextCode = isSuccess ? rateData.Success_Next_Script : rateData.Fail_Next_Script;
                    OnChoiceSelected(nextCode);
                }
                else
                {
                    OnChoiceSelected(destCode);
                }
            });
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
           //OnChoiceSelected(choices[0]);
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

    //전투 결과에 따른 스토리 넘기기
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
    //선택지에 확률 적용
    //지금 같은 경우 확률이 버튼에 출력이 되고 있지는 않음
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

    public void LoadMainStory(int id)
    { 
        TouchCatcher.GetComponent<TouchCatcher>().onTapOutsideScrollView += () =>
        {
            OnSkip();
        };

        if (jsonManager == null)
            jsonManager = FindObjectOfType<JsonManager>();
        storyList = jsonManager.GetStoryMainMasters("Story_Master_Main");
        Debug.Log($"StoryList Count: {(storyList != null ? storyList.Count : -1)}");
        if (storyList == null || storyList.Count == 0)
        {
            Debug.LogError("Story_Master 파일을 불러오는 데 실패했습니다.");
            onCompleteCallback?.Invoke();
            return;
        }



        storyList = storyList
          .Where(s => s.Event_Index == id)
          .OrderBy(e => e.Script_Index)
          .ToList();
        currentIndex = 0;

        currentStory = storyList[currentIndex];
        TouchCatcher.SetActive(true);
        ClearContent();
        // 첫 시퀀스 표시
        DisplayCurrentStory();
    }
    void gameOver()
    {
        Debug.Log("게임 끝났습니다 선생님들 일어나세요");
    }
}