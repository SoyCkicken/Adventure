using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static SaveManager;



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
    public InventoryManager inventoryManager; // 아이템 지급을 위해 필요
    public event Action<string> OnBattleJoin;
    private bool isStoryTransitioning = false;
    [Header("UI References")]
    public List<GameObject> Testblocks = new List<GameObject>();

    // 콜백 저장용
    private Action onCompleteCallback;

    /// <summary>
    /// 메인 스토리 연출 시작 (GameFlowManager에서 호출)
    /// </summary>
    /// 

    private void Start()
    {
        playerState = PlayerState.Instance;

        var handler = SkipButton.GetComponent<SkipOrScrollHandler>();
        if (handler != null)
        {
            handler.targetScrollRect = scrollRect; // Scroll View 연결
            handler.OnTapSkip = () =>
            {
                Debug.Log("스킵 버튼 눌림");
                OnSkip();
                // 여기에 기존 스킵 처리 로직 넣어도 됨
            };
        }
    }

    private void OnSkip()
    {
        if (isTyping)
            isSkip = true;
    }
    public void StartMainStory(Action onComplete)
    {
        currentStoryIndex++;
        SkipButton.GetComponent<Button>().onClick.RemoveAllListeners();
        RegisterTouchCatcher();

        if (jsonManager == null)
            jsonManager = FindObjectOfType<JsonManager>();
        onCompleteCallback = onComplete;
        storyList = jsonManager.GetStoryMainMasters("Story_Master_Main");
        Debug.Log($"StoryList Count: {(storyList != null ? storyList.Count : -1)}");
        Debug.Log(storyList);
        if (storyList == null || storyList.Count == 0)
        {
            Debug.LogError("Story_Master 파일을 불러오는 데 실패했습니다.");
            onCompleteCallback?.Invoke();
            return;
        }

        storyList = jsonManager.GetStoryMainMasters("Story_Master_Main")
                .Where(s => s.Chapter_Index == playerState.CurrentChapterIndex && s.Event_Index == currentStoryIndex)
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
        SkipButton.GetComponent<Button>().onClick.RemoveAllListeners();
        ClearTouchCatcher();
        // currentIndex는 마지막 진행 지점을 자동 보존합니다.
        ClearContent();
        SkipButton.SetActive(false);
    }

    public void DisplayCurrentStory()
    {
        SkipButton.GetComponent<Button>().onClick.RemoveAllListeners();
        RegisterTouchCatcher();
        // 기존 DisplayCurrentStory 내부 로직 그대로 유지
        var matchingScript = scriptEventsCache.FirstOrDefault(sm => sm.Script_Code.Trim() == currentStory.Script_Text.Trim());
        //Debug.Log(matchingScript.KOR);
        GameObject lastBlock = Testblocks.Count > 0 ? Testblocks[Testblocks.Count - 1] : null;
        Debug.Log($"[MainStory] currentIndex: {currentIndex}, listCount: {storyList.Count}");
        Debug.Log($"[MainStory] 지금 Effect = {currentStory.Main_Effect.ToString()}");
        if (currentStory.Main_Effect != null && currentStory.Main_Effect.Count > 0)
        {
            Debug.Log("이펙트 테스트에 들어오긴 했음");
            ApplyEffects(currentStory.Main_Effect);

        }

        if (matchingScript == null)
        {
            SkipButton.GetComponent<Button>().onClick.RemoveAllListeners();
            SkipButton.SetActive(true);
            Debug.Log("버튼 초기화 되는 중 이였음!");
            SkipButton.GetComponent<Button>().onClick.AddListener(() => OnMainStoryComplete());
        }
        else if (matchingScript != null)
        {
            Debug.Log(matchingScript);
            switch (matchingScript.displayType)
            {
                case "IMAGE":
                    Debug.Log("이미지 생성에 들어왔습니다");
                    CreateImageBlock(matchingScript.KOR);
                    break;
                case "TEXT":
                    Debug.Log("텍스트 생성에 들어왔습니다");
                    if (matchingScript.StoryBreak == "Break")
                    {
                        NextScene();
                    }
                    else
                    {
                        HandleTextDisplayWithChoice(matchingScript.KOR, lastBlock, false);
                    }
                    break;
                case "CLAER":
                    Debug.Log("클리어 후 텍스트 생성에 들어왔습니다");
                    HandleTextDisplayWithChoice(matchingScript.KOR, null, true);
                    //ClearContent();
                    break;
                case "BATTLE":
                    Debug.Log("배틀에 들어왔습니다");
                    Debug.Log(matchingScript.KOR);
                    winScriptCode = matchingScript.NEXTWIN?.Trim();
                    loseScriptCode = matchingScript.NEXTLOSE?.Trim();
                    //OnBattleJoin?.Invoke(matchingScript.KOR);
                    BattleState(matchingScript.KOR);
                    break;
                case "MERCHANT":
                    string merchantKey = matchingScript.KOR.Trim();  // KOR이 실제 키값임
                    OpenMerchant(merchantKey);
                    break;
            }
        }
        else
        {
            Debug.LogWarning("해당 스크립트를 찾지 못했습니다.");
        }

        // Choice 버튼 세팅, OnChoiceSelected에서 currentIndex++, 필요 시 onCompleteCallback 호출


    }

    private void BattleState(string enemyID)
    {
        //일단 여기서는 버튼 생성해서 집중 전투 , 자동 전투 선택 가능하게 할꺼임
        for (int i = 0; i < 2; i++)
        {
            var testbutton = Instantiate(choiceButtonPrefab, choiceButtonParent);
            var btn = testbutton.GetComponent<Button>();
            var txt = testbutton.GetComponentInChildren<TMP_Text>();
            if (i == 0)
            {
                txt.text = "자동 전투 입니다";
                btn.onClick.AddListener(() => { OnBattleJoin?.Invoke(enemyID); });
            }
            else
            {
                txt.text = "집중 전투 입니다";
            }
            
        }
      
    }

    private void HandleTextDisplayWithChoice(string text, GameObject lastBlock, bool isClear)
    {
        if (lastBlock == null || lastBlock.TryGetComponent<Image>(out _))
            CreateTextBlock(text, isClear);
        else
            StartCoroutine(TypeTextEffectWithChoice(text, lastBlock, isClear));
    }


    // 필요 시 마지막 노드까지 모두 진행 후 onCompleteCallback 호출
    public void OnMainStoryComplete()
    {
        
        isStoryTransitioning = false;
        StartMainStory(onCompleteCallback); // 다음 챕터 로드
        Debug.Log($"isStoryTransitioning의 값 : {isStoryTransitioning} 이번에는 넘어가면 안된다!");
    }

    public void ClearContent()
    {
        ClearDisplayBlocks();       // 텍스트 / 이미지 블록 제거
        ClearChoiceButtons();       // 선택지 버튼 제거
        ClearTouchCatcher();        // 터치 패널 이벤트 초기화
    }
    private void ClearDisplayBlocks()
    {
        foreach (var go in Testblocks)
            Destroy(go);
        Testblocks.Clear();
    }

    private void ClearChoiceButtons()
    {
        foreach (Transform t in choiceButtonParent)
            Destroy(t.gameObject);
    }

    private void ClearTouchCatcher()
    {
        if (TouchCatcher != null)
        {
            var catcher = TouchCatcher.GetComponent<TouchCatcher>();
            if (catcher != null)
                catcher.onTapOutsideScrollView = null;
        }
    }

    private void RegisterTouchCatcher()
    {
        if (TouchCatcher != null)
        {
            var catcher = TouchCatcher.GetComponent<TouchCatcher>();
            catcher.onTapOutsideScrollView = () => { OnSkip(); };
        }
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
    private void CreateTextBlock(string text,bool isClear)
    {
        var go = Instantiate(TextPrefab, content);
        TMP_Text tmp = go.GetComponentInChildren<TMP_Text>();
        fontSizeManager.Register(tmp);
        //var tmp = go.GetComponent<TMP_Text>();
        StartCoroutine(TypeTextEffectWithChoice(text, go, isClear));
        Testblocks.Add(go);
    }

    // 타입라이터 이펙트
    private IEnumerator TypeTextEffectWithChoice(string fullText, GameObject go,bool isClear)
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
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content as RectTransform);
            scrollRect.verticalNormalizedPosition = 0f;
            tmp.text += fullText[i];

            yield return new WaitForSeconds(0.05f);
        }

        isTyping = false;
        isSkip = false;
        SkipButton.SetActive(false);
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content as RectTransform);
        scrollRect.verticalNormalizedPosition = 0f;

        // 타이핑이 끝난 후에 선택지 출력
        if (!string.IsNullOrEmpty(currentStory.Choice1_Text) ||
            !string.IsNullOrEmpty(currentStory.Choice2_Text) ||
            !string.IsNullOrEmpty(currentStory.Choice3_Text))
        {
            SetupChoices();
        }
        else
        {
            Debug.Log("");
            SkipButton.SetActive(true);
            SkipButton.GetComponent<Button>().onClick.RemoveAllListeners();
            SkipButton.GetComponent<CanvasGroup>().blocksRaycasts = true;
            SkipButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                SkipButton.SetActive(false);
                if (isClear)
                    
                    ClearContent();
                NextScene();
            });
        };

    }
    private void SetupChoices()
    {
        // 기존 버튼 클리어
        foreach (Transform t in choiceButtonParent)
            Destroy(t.gameObject);

        // 성공 확률 데이터
        var successRateList = jsonManager.GetSuccessRatesMainByScene(currentStory.Scene_Code);

        // 1~3번 선택지 반복 처리
        for (int i = 1; i <= 3; i++)
        {
            // Choice#_Text 필드 읽기
            var choiceText = i == 1 ? currentStory.Choice1_Text
                            : i == 2 ? currentStory.Choice2_Text
                            : currentStory.Choice3_Text;
            if (string.IsNullOrEmpty(choiceText))
                continue;

            // 화면에 보여줄 문자열
            var display = GetDisplayTextFromScript(choiceText, scriptEventsCache);

            // 우선 사용해야 할 Next_Scene (override)
            var overrideScene = i == 1 ? currentStory.Choice1_Next_Scene
                              : i == 2 ? currentStory.Choice2_Next_Scene
                              : currentStory.Choice3_Next_Scene;
            overrideScene = overrideScene?.Trim();

            // 성공률이 있을 경우 가져오기
            var rateData = successRateList.FirstOrDefault(r => r.Choice_No == i);

            // 버튼 생성
            var go = Instantiate(choiceButtonPrefab, choiceButtonParent);
            var btn = go.GetComponent<Button>();
            var txt = go.GetComponentInChildren<TMP_Text>();
            txt.text = display;

            btn.onClick.AddListener(() =>
            {
                string nextCode = null;

                if (rateData != null)
                {
                    // 성공/실패 분기
                    float rate = EvaluateFormula(rateData.Success_Formula);
                    bool ok = UnityEngine.Random.value < rate;
                    nextCode = ok
                        ? rateData.Success_Next_Script?.Trim()
                        : rateData.Fail_Next_Script?.Trim();
                }

                // 성공률 분기가 없거나 분기 결과가 비어있으면 overrideScene 사용
                if (string.IsNullOrEmpty(nextCode))
                    nextCode = !string.IsNullOrEmpty(overrideScene)
                        ? overrideScene
                        : choiceText.Trim();

                OnChoiceSelected(nextCode);
            });
        }
    }

    private void OnChoiceSelected(string newSceneCode)
    {
        // MainScript → MainScene 교정
        if (newSceneCode.StartsWith("MainScript"))
            newSceneCode = newSceneCode.Replace("MainScript", "MainScene");

        // 버튼 제거
        foreach (Transform t in choiceButtonParent)
            Destroy(t.gameObject);

        // 실제 다음 스토리 찾기
        var next = storyList.FirstOrDefault(s => s.Scene_Code.Trim() == newSceneCode.Trim());
        if (next != null)
        {
            currentStory = next;
            DisplayCurrentStory();
        }
        else
        {
            Debug.LogWarning($"선택된 씬을 찾을 수 없습니다: {newSceneCode}");
            onCompleteCallback?.Invoke();
        }
    }

    //Story_Master_Main FindStoryBySceneCode(string sceneCode)
    //{
    //    return storyList.FirstOrDefault(s => s.Scene_Code.Trim() == sceneCode.Trim());
    //}
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
        if (!string.IsNullOrEmpty(currentStory.Next_Scene))
        {
            Debug.Log($"[NEXTSCENE] 현재 씬 {currentStory.Scene_Code} → {currentStory.Next_Scene}");

            var nextStory = storyList.FirstOrDefault(s => s.Scene_Code == currentStory.Next_Scene);
            if (nextStory != null)
            {
                currentStory = nextStory;
                currentIndex = storyList.IndexOf(nextStory);
                DisplayCurrentStory();
                return;
            }
            else
            {
                Debug.LogWarning($"[NEXTSCENE] {currentStory.Next_Scene} 씬을 찾지 못했습니다.");
            }
        }

        var script = scriptEventsCache
        .FirstOrDefault(sm => sm.Script_Code.Trim() == currentStory.Script_Text.Trim());
        if (script != null && script.StoryBreak == "Break")
        {
            if (isStoryTransitioning) return;
            isStoryTransitioning = true;
            Debug.Log($"isStoryTransitioning의 값 : {isStoryTransitioning}");
            Debug.Log("브레이크문 들어왔습니다");
            SkipButton.GetComponent<Button>().onClick.RemoveAllListeners();
            SkipButton.SetActive(true);
            SkipButton.GetComponent<CanvasGroup>().blocksRaycasts = true;
            if (script.ChapterBreack == "Break")
            {
                currentStoryIndex = 0;              // Event_Index 초기화
                onCompleteCallback?.Invoke();
                Debug.Log("일단 다음 스토리가 없을 경우 이쪽으로 넘어갔음(모든 스토리 진행했다 판단하는거임)");
            }
            else
            {
                Debug.Log("여기 넘어 갔으면 아직 스토리 남았다는 거임");
                OnMainStoryComplete();
            }
            //SkipButton.SetActive(false);

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

    private void ApplyEffects(List<EffectTrigger> effects)
    {
        foreach (var effect in effects)
        {
            switch (effect.ID)
            {
                case "Effect_001":
                    playerState.Experience += effect.Value;
                    Debug.Log($"소울 {effect.Value} 증가 → 현재: {playerState.Experience}");
                    inventoryManager.updateSoulText();
                    break;

                case "Effect_002":
                    playerState.CurrentHealth = Mathf.Max(0, playerState.CurrentHealth - Mathf.Abs(effect.Value));
                    Debug.Log($"체력 {effect.Value} 감소 → 현재: {playerState.CurrentHealth}");
                    break;

                case "Effect_003": // 아이템 추가
                    ItemData item = jsonManager.GetItemDataFromCode(effect.Code);
                    Debug.Log($"{item.Item_Name} , {item.Item_ID} , {item.Item_Type}");
                    if (item != null)
                    {
                        inventoryManager.AddItemToInventory(item); // 또는 AddItem(item)
                    }
                    else
                    {
                        Debug.LogWarning($"[이펙트 실패] 잘못된 아이템 코드: {effect.Code}");
                    }
                    break;

                default:
                    Debug.LogWarning($"알 수 없는 이펙트 ID: {effect.ID}");
                    break;
            }
        }
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

    private void OpenMerchant(string key)
    {
        Debug.Log("상점 로드 시도");
        var merchantManager = FindObjectOfType<MerchantManager>();
        if (merchantManager != null)
        {
            Debug.Log("상점 로드 진행중");
            ClearContent();
            merchantManager.OpenShop(key, OnMerchantClosed);
        }
    }
    private void OnMerchantClosed()
    {
        Debug.Log("상점 닫힘. 다음 스토리로 진행.");
        NextScene(); // 또는 DisplayNextEvent 등
    }
    //리모컨에서 사용중인거
    public void LoadMainStory(int chapter, int eventIndex)
    {
        var flowManager = FindObjectOfType<GameFlowManager>();
        if (flowManager != null && !flowManager.CanEnterFlow())
        {
            Debug.LogWarning("다른 상태가 진행 중입니다. 메인 스토리를 시작할 수 없습니다.");
            //return;
        }

        flowManager?.SetState(GameFlowManager.FlowState.MainStory);

        if (jsonManager == null)
            jsonManager = FindObjectOfType<JsonManager>();

        RegisterTouchCatcher();
        storyList.Clear();
        storyList = jsonManager.GetStoryMainMasters("Story_Master_Main");

        Debug.Log($"StoryList Count: {(storyList != null ? storyList.Count : -1)}");

        if (storyList == null || storyList.Count == 0)
        {
            Debug.LogError("Story_Master 파일을 불러오는 데 실패했습니다.");
            onCompleteCallback?.Invoke();
            flowManager?.SetState(GameFlowManager.FlowState.None);
            return;
        }

        Debug.Log($"[MainStory] currentIndex: {currentIndex}, listCount: {storyList.Count}");
        storyList = storyList
          .Where(s => s.Chapter_Index == chapter && s.Event_Index == eventIndex)
          .OrderBy(s => s.Script_Index)
          .ToList();

        if (storyList.Count == 0)
        {
            Debug.LogError($"Event_Index {chapter} , {eventIndex}에 해당하는 스토리가 없습니다.");
            onCompleteCallback?.Invoke();
            flowManager?.SetState(GameFlowManager.FlowState.None); // 실패 시 상태 복구
            return;
        }

        currentIndex = 0;
        currentStory = storyList[currentIndex];

        TouchCatcher.SetActive(true);
        isTyping = false;
        ClearContent();

        // 첫 시퀀스 표시
        DisplayCurrentStory();
    }

    public void SaveMainStory(ref SaveData data)
    {
        data.PlayerCurrentChapterIndex = playerState.CurrentChapterIndex;
        data.MainstoryEventIndex = currentStoryIndex;
        data.MainstoryCurrentIndex = currentIndex;
        data.MainstorySceneCode = currentStory.Scene_Code;
    }

    public void LoadMainStory(SaveData data)
    {
        currentStoryIndex = data.MainstoryEventIndex;

        storyList = jsonManager.GetStoryMainMasters("Story_Master_Main")
       .Where(s => s.Chapter_Index == playerState.CurrentChapterIndex && // 🔥 챕터 조건 추가
                   s.Event_Index == currentStoryIndex)
       .OrderBy(e => e.Script_Index)
       .ToList();

        scriptEventsCache = jsonManager.GetStoryMainScriptMasters("Main_Script_Master_Main");

        currentIndex = Mathf.Clamp(data.MainstoryCurrentIndex, 0, storyList.Count);
        currentStory = storyList[currentIndex];

        Debug.Log($"[로드 완료] 현재 스토리: {currentStory.Scene_Code}");

        ClearContent();
        TouchCatcher.SetActive(true);
        SkipButton.GetComponent<Button>().onClick.RemoveAllListeners();
        SkipButton.GetComponent<Button>().onClick.AddListener(() => OnSkip());
    }
    void gameOver()
    {
        Debug.Log("게임 끝났습니다 선생님들 일어나세요");
    }
}