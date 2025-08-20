using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static SaveManager;
using UnityEngine.SceneManagement;
using MyGame.TextEffects;
public class StoryDisplayManager : MonoBehaviour
{
    public GameObject ImagePrefab;
    public GameObject TextPrefab;
    public GameObject SkipButton;
    public GameObject touchCatcher;
    public GameObject choiceButtonPrefab;
    public ScrollRect scrollRect;         // 에디터에서 연결
    public Transform content;
    public Transform choiceButtonParent;
    public FontSizeManager fontSizeManager;
    public JsonManager jsonManager;
    [SerializeField] private EquipmentSystem equipmentSystem;
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
    public event Action<string> OnFocusBattleJoin; // 집중 전투 이벤트
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
        jsonManager = JsonManager.Instance; // 수정
        spriteBank = SpriteBank.Instance;
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
    private List<ChoiceRequirement> GetRequirementsFor(string sceneCode, int choiceNo,
    Main_SuccessRate_Master_Main rateRow)
    {
        // 1) 성공률 시트에 ChoiceRequirement 컬럼이 있으면 그걸 우선 사용 (네 스샷 1번처럼)
        if (rateRow != null && rateRow.ChoiceRequirement != null && rateRow.ChoiceRequirement.Count > 0)
            return rateRow.ChoiceRequirement;

        // 2) 없으면 JsonManager에서 (Scene, ChoiceNo)로 별도 색인된 조건을 꺼냄 (있다면)
        if (jsonManager != null)
            return jsonManager.GetChoiceRequirementsByScene(sceneCode, choiceNo);

        return null;
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
            return; // 🔴 이게 없으면 다음 코드 실행될 수도 있음
        }
        currentStory = storyList[currentIndex];
        touchCatcher.SetActive(true);
        ClearContent();
        Debug.Log("시작부분");
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
        Debug.Log($"[MainStory] 스토리 중지됨. 현재 인덱스: {currentIndex}");
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
        int rewardBlocks = 0;
        if (currentStory.Main_Effect != null && currentStory.Main_Effect.Count > 0)
        {
            //rewardBlocks = ApplyEffects(currentStory.Main_Effect, currentStory.Script_Text);
            rewardBlocks = EffectProcessor.ApplyEffects(currentStory.Main_Effect, playerState, inventoryManager, jsonManager, fontSizeManager, content, TextPrefab, "MainScene", Testblocks);

        }
        if (rewardBlocks > 0)
            lastBlock = null; // ✅ 새 텍스트 블록으로 시작시키기

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
                btn.onClick.AddListener(() => { OnFocusBattleJoin?.Invoke(enemyID); });
                txt.text = "집중 전투 입니다";
            }
            
        }
      
    }

    private void HandleTextDisplayWithChoice(string text, GameObject lastBlock, bool isClear)
    {
        if (lastBlock == null || lastBlock.TryGetComponent<UnityEngine.UI.Image>(out _))
        {
            // 새 텍스트 블록 생성 → 이펙트 초기화 O
            CreateTextBlock(text, isClear); // 내부에서 resetFx=true로 코루틴 시작
            return;
        }

        // 같은 블록에 이어붙임 → 이펙트 초기화 X
        StartCoroutine(TypeTextEffectWithChoice(text, lastBlock, isClear, resetFx: false));
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
        Debug.Log("호출이 되었습니다");
        ClearDisplayBlocks();       // 텍스트 / 이미지 블록 제거
        ClearChoiceButtons();       // 선택지 버튼 제거
        ClearTouchCatcher();        // 터치 패널 이벤트 초기화
        Debug.Log($"남은 블록의 갯수{Testblocks.Count()}");
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
        if (touchCatcher != null)
        {
            var catcher = touchCatcher.GetComponent<TouchCatcher>();
            if (catcher != null)
                catcher.onTapOutsideScrollView = null;
        }
    }

    private void RegisterTouchCatcher()
    {
        if (touchCatcher != null)
        {
            var catcher = touchCatcher.GetComponent<TouchCatcher>();
            Debug.Log("재 지정 되었습니다");
            catcher.onTapOutsideScrollView = () =>
            {
                Debug.Log("터치 캐처가 작동했습니다");
                OnSkip();
            };
        }
        else
        {
            Debug.LogWarning("catchat가 없습니다!!");
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
    private void CreateTextBlock(string text, bool isClear)
    {
        var go = Instantiate(TextPrefab, content);
        TMP_Text tmp = go.GetComponentInChildren<TMP_Text>();
        var fx = go.GetComponent<TMPTextRangeEffects>() ?? go.AddComponent<TMPTextRangeEffects>();
        fx.ClearEffects(); // 💥 여기도 안전하게 초기화
        fontSizeManager.Register(tmp);
        //var tmp = go.GetComponent<TMP_Text>();
        StartCoroutine(TypeTextEffectWithChoice(text, go, isClear,true));
        Testblocks.Add(go);
    }
    //진짜 버그 심하면 쓸 원본 텍스트 타이핑
    // 타입라이터 이펙트
    //private IEnumerator TypeTextEffectWithChoice(string fullText, GameObject go, bool isClear)
    //{
    //    TMP_Text tmp = go.GetComponentInChildren<TMP_Text>();
    //    isTyping = true;
    //    string complete = tmp.text + fullText;

    //    for (int i = 0; i < fullText.Length; i++)
    //    {
    //        if (isSkip)
    //        {
    //            tmp.text = complete;
    //            Canvas.ForceUpdateCanvases();
    //            var contentRect = scrollRect.content as RectTransform;
    //            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
    //            scrollRect.verticalNormalizedPosition = 0f;
    //            break;
    //        }
    //        Canvas.ForceUpdateCanvases();
    //        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content as RectTransform);
    //        scrollRect.verticalNormalizedPosition = 0f;
    //        tmp.text += fullText[i];

    //        yield return new WaitForSeconds(0.05f);
    //    }

    //    isTyping = false;
    //    isSkip = false;
    //    SkipButton.SetActive(false);
    //    Canvas.ForceUpdateCanvases();
    //    LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content as RectTransform);
    //    scrollRect.verticalNormalizedPosition = 0f;

    //    // 타이핑이 끝난 후에 선택지 출력
    //    if (!string.IsNullOrEmpty(currentStory.Choice1_Text) ||
    //        !string.IsNullOrEmpty(currentStory.Choice2_Text) ||
    //        !string.IsNullOrEmpty(currentStory.Choice3_Text))
    //    {
    //        SetupChoices();
    //    }
    //    else
    //    {
    //        Debug.Log("");
    //        SkipButton.SetActive(true);
    //        SkipButton.GetComponent<Button>().onClick.RemoveAllListeners();
    //        SkipButton.GetComponent<CanvasGroup>().blocksRaycasts = true;
    //        SkipButton.GetComponent<Button>().onClick.AddListener(() =>
    //        {
    //            SkipButton.SetActive(false);
    //            if (isClear)

    //                ClearContent();
    //            NextScene();
    //        });
    //    }
    //    ;
    //}
    IEnumerator TypeTextEffectWithChoice(string fullText, GameObject go, bool isClear, bool resetFx)
    {
        var tmp = go.GetComponentInChildren<TMP_Text>();
        var fx = go.GetComponent<TMPTextRangeEffects>() ?? go.AddComponent<TMPTextRangeEffects>();
        List<TextFragment> fragments = TextEffectParser.ParseFragments(fullText);

        if (resetFx) fx.ClearEffects();
        isTyping = true;
        int cursor = tmp.text.Length;

        foreach (var fragment in fragments)
        {
            int start = cursor;
            int written = 0;

            for (int i = 0; i < fragment.text.Length; i++)
            {
                if (isSkip)
                {
                    // 스킵: 남은 텍스트 강제 출력
                    string remaining = fragment.text.Substring(i);
                    tmp.text += remaining;
                    cursor += remaining.Length;
                    written += remaining.Length;

                    // 효과 강제 적용
                    foreach (var fxData in fragment.effects)
                    {
                        switch (fxData.type)
                        {
                            case EffectType.Wave:
                                if (written > 0) fx.AddWaveRange(start, written);
                                break;
                            case EffectType.Shake:
                                if (written > 0) fx.AddShakeRange(start, written);
                                break;
                            case EffectType.Color:
                                if (fxData.color.HasValue)
                                    fx.AddColorRange(start, written, fxData.color.Value);
                                break;
                        }
                    }

                    break; // 해당 fragment 종료
                }

                // 1글자씩 출력
                tmp.text += fragment.text[i];
                written++;
                cursor++;

                // 효과 적용
                foreach (var fxData in fragment.effects)
                {
                    switch (fxData.type)
                    {
                        case EffectType.Wave:
                            if (written == 1) fx.AddWaveRange(start, 1);
                            else fx.UpdateLastWaveLength(written);
                            break;
                        case EffectType.Shake:
                            if (written == 1) fx.AddShakeRange(start, 1);
                            else fx.UpdateLastShakeLength(written);
                            break;
                        case EffectType.Color:
                            if (fxData.color.HasValue)
                            {
                                if (written == 1) fx.AddColorRange(start, 1, fxData.color.Value);
                                else fx.UpdateLastColorLength(written);
                            }
                            break;
                    }
                }

                // UI 업데이트
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)scrollRect.content);
                scrollRect.verticalNormalizedPosition = 0f;

                yield return new WaitForSeconds(0.05f);
            }
        }

        // 종료 처리
        isTyping = false;
        isSkip = false;
        SkipButton.SetActive(false);

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)scrollRect.content);
        scrollRect.verticalNormalizedPosition = 0f;

        // 선택지 or 다음 버튼
        if (!string.IsNullOrEmpty(currentStory?.Choice1_Text) ||
            !string.IsNullOrEmpty(currentStory?.Choice2_Text) ||
            !string.IsNullOrEmpty(currentStory?.Choice3_Text))
        {
            SetupChoices();
        }
        else
        {
            SkipButton.SetActive(true);
            var btn = SkipButton.GetComponent<Button>();
            var cg = SkipButton.GetComponent<CanvasGroup>();

            btn.onClick.RemoveAllListeners();
            if (cg != null) cg.blocksRaycasts = true;

            btn.onClick.AddListener(() =>
            {
                SkipButton.SetActive(false);
                if (isClear) ClearContent();
                NextScene();
            });
        }
    }
    private void SetupChoices()
    {
        // 0) 한 번만 참조 보장
        equipmentSystem = equipmentSystem ?? FindObjectOfType<EquipmentSystem>();
        playerState = playerState ?? PlayerState.Instance;
        jsonManager = jsonManager ?? JsonManager.Instance;

        // 1) 기존 버튼 정리
        foreach (Transform t in choiceButtonParent)
            Destroy(t.gameObject);

        // 2) 성공률 데이터 (기존 흐름 그대로)
        var successRateList = jsonManager.GetSuccessRatesMainByScene(currentStory.Scene_Code);

        // 3) 1~3번 선택지 처리
        for (int i = 1; i <= 3; i++)
        {
            // 3-1) 선택지 스크립트 코드/표시 텍스트/오버라이드 다음 씬
            string choiceText = i == 1 ? currentStory.Choice1_Text :
                                i == 2 ? currentStory.Choice2_Text :
                                         currentStory.Choice3_Text;
            if (string.IsNullOrEmpty(choiceText)) continue;

            string display = GetDisplayTextFromScript(choiceText, scriptEventsCache);
            string overrideScene = (i == 1 ? currentStory.Choice1_Next_Scene :
                                    i == 2 ? currentStory.Choice2_Next_Scene :
                                             currentStory.Choice3_Next_Scene)?.Trim();

            // 3-2) 성공률/분기 정보 (있으면 확률 버튼, 없으면 일반 버튼)
            var rateRow = successRateList?.FirstOrDefault(r => r.Choice_No == i);
            bool hasRate = rateRow != null;

            // 3-3) 🔴 조건 로드 + 평가(먼저!)
            var reqs = GetRequirementsFor(currentStory.Scene_Code, i, rateRow);
            bool ok = ConditionEvaluator.Evaluate(reqs, playerState, inventoryManager, inventoryManager.equipmentSystem, out var reasons);

            // 3-4) 버튼 생성
            var go = Instantiate(choiceButtonPrefab, choiceButtonParent);
            var btn = go.GetComponent<Button>();
            var txt = go.GetComponentInChildren<TMP_Text>();
            txt.text = display;

            // 3-5) 조건 미충족 → 비활성 + 사유 표기, 클릭 핸들러 없음
            if (!ok)
            {
                btn.interactable = false;
                txt.text = $"{display}\n<color=#FF6666>조건: {string.Join(", ", reasons)}</color>";
                continue;
            }

            // 3-6) 조건 통과 → 확률 있는지에 따라 분기
            if (hasRate)
            {
                // 성공률 계산(기존에 쓰던 ChoiceEvaluator 그대로 활용)
                var choiceResult = ChoiceEvaluator.Resolve(
                    formula: rateRow.Success_Formula,
                    nextOnSuccess: rateRow.Success_Next_Script,
                    nextOnFail: rateRow.Fail_Next_Script,
                    state: playerState
                );

                // 배지 UI 붙이는 도우미 그대로 사용
                if (choiceResult != null)
                {
                    ChoiceUIHelper.CreateChanceBadge(
                        buttonGO: go,
                        mainText: txt,
                        rate01: choiceResult.SuccessRate,
                        bgSprite: null,
                        yOffset: -10f,
                        labelSize: Mathf.RoundToInt(txt.fontSize * 0.7f),
                        percentScale: 1.6f
                    );
                }

                // 클릭 → 성공/실패에 따라 분기 스크립트
                btn.onClick.AddListener(() =>
                {
                    //string nextCode = null;
                    //bool success = ChoiceEvaluator.EvaluateSuccess(choiceResult.SuccessRate);
                    //nextCode = success ? rateRow.Success_Next_Script?.Trim()
                    //                   : rateRow.Fail_Next_Script?.Trim();

                    //if (string.IsNullOrEmpty(nextCode))
                    //    nextCode = !string.IsNullOrEmpty(overrideScene) ? overrideScene : choiceText.Trim();

                    //OnChoiceSelected(nextCode);
                    string nextCode = null;
                    bool success = ChoiceEvaluator.EvaluateSuccess(choiceResult.SuccessRate);
                    nextCode = success ? rateRow.Success_Next_Script?.Trim()
                                       : rateRow.Fail_Next_Script?.Trim();

                    if (string.IsNullOrEmpty(nextCode))
                        nextCode = !string.IsNullOrEmpty(overrideScene) ? overrideScene : choiceText.Trim();

                    OnChoiceSelected(nextCode, choiceText); // 🔴 라벨 코드 같이 전달
                });
            }
            else
            {
                // 일반 버튼
                //btn.onClick.AddListener(() =>
                //{
                //    string nextCode = !string.IsNullOrEmpty(overrideScene) ? overrideScene : choiceText.Trim();
                //    OnChoiceSelected(nextCode);
                //});
                btn.onClick.AddListener(() =>
                {
                    string nextCode = !string.IsNullOrEmpty(overrideScene) ? overrideScene : choiceText.Trim();
                    OnChoiceSelected(nextCode, choiceText); // 🔴 라벨 코드 같이 전달
                });
            }
        }
    }

    private string GetFallbackNextSceneCodeOrNull()
    {
        // 1) 우선 currentStory.Next_Scene이 있으면 그걸로
        if (!string.IsNullOrWhiteSpace(currentStory.Next_Scene))
            return currentStory.Next_Scene.Trim();

        // 2) 없으면 같은 Event 내 Script_Index + 1 씬
        var next = storyList.FirstOrDefault(s =>
            s.Chapter_Index == currentStory.Chapter_Index &&
            s.Event_Index == currentStory.Event_Index &&
            s.Script_Index == currentStory.Script_Index + 1);

        return next != null ? next.Scene_Code?.Trim() : null;
    }
    private static string NormalizeToSceneCode(string code)
    {
        if (string.IsNullOrEmpty(code)) return null;
        return code.StartsWith("MainScript") ? code.Replace("MainScript", "MainScene").Trim()
                                             : code.Trim();
    }
    //private void OnChoiceSelected(string newSceneCode)
    //{
    //    // MainScript → MainScene 교정
    //    if (newSceneCode.StartsWith("MainScript"))
    //        newSceneCode = newSceneCode.Replace("MainScript", "MainScene");

    //    // 버튼 제거
    //    foreach (Transform t in choiceButtonParent)
    //        Destroy(t.gameObject);

    //    // 실제 다음 스토리 찾기
    //    var next = storyList.FirstOrDefault(s => s.Scene_Code.Trim() == newSceneCode.Trim());
    //    if (next != null)
    //    {
    //        currentStory = next;
    //        DisplayCurrentStory();
    //    }
    //    else
    //    {
    //        Debug.LogWarning($"선택된 씬을 찾을 수 없습니다: {newSceneCode}");
    //        onCompleteCallback?.Invoke();
    //    }
    //}
    private void OnChoiceSelected(string newSceneCode, string labelScriptCode = null)
    {
        if (newSceneCode.StartsWith("MainScript"))
            newSceneCode = newSceneCode.Replace("MainScript", "MainScene");

        // 버튼 제거
        foreach (Transform t in choiceButtonParent)
            Destroy(t.gameObject);

        // 실제 다음 스토리 찾기
        var next = storyList.FirstOrDefault(s => s.Scene_Code.Trim() == newSceneCode.Trim());
        if (next == null)
        {
            Debug.LogWarning($"선택된 씬을 찾을 수 없습니다: {newSceneCode}");
            onCompleteCallback?.Invoke();
            return;
        }

        // 🔴 라벨 억제: 도착한 씬의 Script_Text가 라벨 스크립트 코드와 같으면 1회 패스
        if (!string.IsNullOrEmpty(labelScriptCode) &&
            !string.IsNullOrEmpty(next.Script_Text) &&
            next.Script_Text.Trim() == labelScriptCode.Trim())
        {
            // “다음 씬”으로 진행
            var next2 = storyList.FirstOrDefault(s =>
                s.Chapter_Index == next.Chapter_Index &&
                s.Event_Index == next.Event_Index &&
                s.Script_Index == next.Script_Index + 1);

            if (next2 != null)
            {
                currentStory = next2;
                DisplayCurrentStory();
                return;
            }
            else if (!string.IsNullOrEmpty(next.Next_Scene))
            {
                var jump = storyList.FirstOrDefault(s => s.Scene_Code.Trim() == next.Next_Scene.Trim());
                if (jump != null)
                {
                    currentStory = jump;
                    DisplayCurrentStory();
                    return;
                }
            }
            // 패스할 다음이 없으면 종료 처리
            NextScene();
            return;
        }

        // 평소 흐름
        currentStory = next;
        DisplayCurrentStory();
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
        string Clean(string input) =>
    input?.Trim().Replace("\n", "").Replace("\r", "").Replace("\t", "");

        var script = scriptEventsCache
     .FirstOrDefault(sm => Clean(sm.Script_Code) == Clean(currentStory.Script_Text));
        Debug.Log($"현재 스크립트 텍스트: [{currentStory.Script_Text}]");
        Debug.Log($"현재 Script의 값 : {script.Script_Code} , 내용 : {script.KOR}");
        if (script.StoryBreak?.Trim() == "Break")
        {
            Debug.Log($"isStoryTransitioning의 값 : {isStoryTransitioning}");
            Debug.Log("브레이크문 들어왔습니다");
            if (isStoryTransitioning) return;
            isStoryTransitioning = true;
            SkipButton.GetComponent<Button>().onClick.RemoveAllListeners();
            SkipButton.SetActive(true);
            SkipButton.GetComponent<CanvasGroup>().blocksRaycasts = true;
            if (script.ChapterBreack?.Trim() == "Break")
            {
                Debug.Log("일단 다음 스토리가 없을 경우 이쪽으로 넘어갔음(모든 스토리 진행했다 판단하는거임)");
                ClearContent(); //이유는 모르겠지만 씬 넘어갈때 초기화를 해주고 있을텐데 안되고 넘어감..
                currentStoryIndex = 0;              // Event_Index 초기화
                isStoryTransitioning = false; //<--이거 있어야 할듯..
                onCompleteCallback?.Invoke();
                
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
        Debug.Log($"전투 결과에 따라 스토리 정리: {currentStory.Script_Text}");
        ClearContent();
        DisplayCurrentStory();
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
        playerState.CurrentChapterIndex = chapter;

        touchCatcher.SetActive(true);
        isTyping = false;
        Debug.Log("리모컨 기능 사용으로 스토리 넘어왔을때 정리");
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

    // ✅ 스토리 로드 이게 세이브 로드에서 사용중인거
    //public void LoadMainStory(SaveData data)
    //{
    //    if (jsonManager == null)
    //        jsonManager = FindObjectOfType<JsonManager>();
    //    playerState.CurrentChapterIndex = data.PlayerCurrentChapterIndex;
    //    currentStoryIndex = data.MainstoryEventIndex;

    //    storyList = jsonManager.GetStoryMainMasters("Story_Master_Main")
    //        .Where(s => s.Chapter_Index == data.PlayerCurrentChapterIndex)
    //        .OrderBy(s => s.Script_Index)
    //        .ToList();

    //    scriptEventsCache = jsonManager.GetStoryMainScriptMasters("Main_Script_Master_Main");

    //    // 🔥 Scene_Code로 정확한 위치 찾기
    //    int index = storyList.FindIndex(s => s.Scene_Code.Trim() == data.MainstorySceneCode.Trim());
    //    currentIndex = index >= 0 ? index : 0;

    //    currentStory = storyList[currentIndex];
    //    touchCatcher.SetActive(true);
    //    SkipButton.SetActive(true);
    //    Debug.Log("세이브파일 로드 시 사용하고 있는 클리어 부분");
    //    ClearContent();
    //    //DisplayCurrentStory();
    //}
    //public void SetOnCompleteCallback(Action cb)
    //{
    //    onCompleteCallback = cb;
    //}
    

    public void LoadMainStory(SaveData data)
    {
        if (jsonManager == null)
            jsonManager = FindObjectOfType<JsonManager>();

        playerState.CurrentChapterIndex = data.PlayerCurrentChapterIndex;
        currentStoryIndex = data.MainstoryEventIndex;

        storyList = jsonManager.GetStoryMainMasters("Story_Master_Main")
            .Where(s => s.Chapter_Index == data.PlayerCurrentChapterIndex)
            .OrderBy(s => s.Script_Index)
            .ToList();

        scriptEventsCache = jsonManager.GetStoryMainScriptMasters("Main_Script_Master_Main");

        // Scene_Code로 정확한 위치 찾기
        int index = storyList.FindIndex(s => s.Scene_Code.Trim() == data.MainstorySceneCode.Trim());
        currentIndex = index >= 0 ? index : 0;

        if (currentIndex < storyList.Count)
        {
            currentStory = storyList[currentIndex];
        }
        else
        {
            Debug.LogWarning($"[StoryDisplayManager] 인덱스 {currentIndex}가 범위를 벗어남. storyList.Count: {storyList.Count}");
            currentIndex = 0;
            if (storyList.Count > 0)
                currentStory = storyList[0];
        }

        touchCatcher.SetActive(true);
        SkipButton.SetActive(true);
        Debug.Log("로드 시 한번 더 확인용");
        RegisterTouchCatcher();
        Debug.Log($"[StoryDisplayManager] 로드 완료 - Chapter: {data.PlayerCurrentChapterIndex}, Event: {currentStoryIndex}, Scene: {data.MainstorySceneCode}");
    }
    public void SetOnCompleteCallback(Action cb)
    {
        onCompleteCallback = cb;
    }
    void gameOver()
    {
        Debug.LogError("게임 끝났습니다 선생님들 일어나세요");
        // 게임 오버 처리 로직
        SceneManager.LoadSceneAsync("GameEndingScene");
    }
}