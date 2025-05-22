using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class StoryDisplayManager : MonoBehaviour
{
    [Header("Prefabs & References")]
    public GameObject ImagePrefab;
    public GameObject TextPrefab;
    public GameObject SkipButton;
    public GameObject choiceButtonPrefab;
    public ScrollRect scrollRect;
    public Transform content;
    public Transform choiceButtonParent;

    [Header("Data")]
    public JsonManager jsonManager;
    private List<Story_Master_Main> storyList;
    private Story_Master_Main currentStory;
    private int currentIndex;
    private List<Main_Script_Master_Main> scriptEventsCache;

    [Header("State")]
    public bool isSkip;
    public bool isTyping;
    public List<GameObject> Testblocks = new List<GameObject>();

    public event Action<string> OnBattleJoin;
    private Action onCompleteCallback;

    void Awake()
    {
        if (jsonManager == null)
            jsonManager = FindObjectOfType<JsonManager>();
    }

    /// <summary>
    /// 메인 스토리 연출 시작
    /// </summary>
    public void StartMainStory(Action onComplete)
    {
        onCompleteCallback = onComplete;
        ResetSkipButton();

        storyList = jsonManager.GetStoryMainMasters("Story_Master_Main");
        if (storyList == null || storyList.Count == 0)
        {
            Debug.LogError("Story_Master 파일을 불러오는 데 실패했습니다.");
            onCompleteCallback?.Invoke();
            return;
        }
        storyList = storyList
            .OrderBy(s => s.Chapter_Index)
            .ThenBy(s => s.Event_Index)
            .ThenBy(s => s.Script_Index)
            .ToList();

        scriptEventsCache = jsonManager.GetStoryMainScriptMasters("Main_Script_Master_Main")
                             ?? new List<Main_Script_Master_Main>();

        currentIndex = 0;
        currentStory = storyList[currentIndex];

        SkipButton.SetActive(true);
        ClearContent();
        DisplayCurrentStory();
    }

    /// <summary>
    /// 메인 스토리 일시 중지
    /// </summary>
    public void StopMainStory()
    {
        StopAllCoroutines();
        ClearContent();
        SkipButton.SetActive(false);
    }

    /// <summary>
    /// 현재 스토리 노드 표시
    /// </summary>
    private void DisplayCurrentStory()
    {
        var script = scriptEventsCache
            .FirstOrDefault(sm => sm.Script_Code.Trim() == currentStory.Script_Text.Trim());
        GameObject lastBlock = Testblocks.Count > 0 ? Testblocks[^1] : null;

        if (script == null)
        {
            OnAfterScene();
            return;
        }

        switch (script.displayType)
        {
            case "IMAGE":
                CreateImageBlock(script.KOR);
                OnAfterScene();
                break;

            case "TEXT":
                HandleTextDisplay(script.KOR, lastBlock, OnAfterScene);
                break;

            case "BATTLE":
                OnBattleJoin?.Invoke(script.KOR);
                break;

            default:
                Debug.LogWarning("알 수 없는 displayType: " + script.displayType);
                OnAfterScene();
                break;
        }
    }

    /// <summary>
    /// 타입 이펙트 또는 새 텍스트 블록 처리
    /// </summary>
    private void HandleTextDisplay(string text, GameObject lastBlock, Action onDone)
    {
        if (lastBlock == null || lastBlock.TryGetComponent<Image>(out _))
        {
            CreateTextBlock(text);
            onDone?.Invoke();
        }
        else
        {
            StartCoroutine(TypeTextEffect(text, lastBlock, onDone));
        }
    }

    private IEnumerator TypeTextEffect(string fullText, GameObject go, Action onDone)
    {
        isTyping = true;
        SkipButton.SetActive(true);

        var tmp = go.GetComponent<TMP_Text>();
        string full = tmp.text + fullText;
        for (int i = 0; i < fullText.Length; i++)
        {
            if (isSkip)
            {
                tmp.text = full;
                break;
            }
            tmp.text += fullText[i];
            yield return new WaitForSeconds(0.05f);
        }

        isTyping = false;
        isSkip = false;
        SkipButton.SetActive(false);

        // 스크롤 자동 갱신
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;

        onDone?.Invoke();
    }

    /// <summary>
    /// 이미지 블록 생성
    /// </summary>
    private void CreateImageBlock(string spriteName)
    {
        var go = Instantiate(ImagePrefab, content);
        var img = go.GetComponent<Image>();
        img.sprite = Resources.Load<Sprite>($"Images/{spriteName}");
        Testblocks.Add(go);
    }
    private void CreateTextBlock(string text)
    {
        var go = Instantiate(TextPrefab, content);
        //var tmp = go.GetComponent<TMP_Text>();
        StartCoroutine(TypeTextEffect(text, go, OnAfterScene));
        Testblocks.Add(go);
    }

    /// <summary>
    /// 선택지 생성 또는 다음 씬 분기 처리
    /// </summary>
    private void OnAfterScene()
    {
        // 선택지 개수 확인
        var choices = new[]
        {
            currentStory.Choice1_Text,
            currentStory.Choice2_Text,
            currentStory.Choice3_Text
        }.Where(c => !string.IsNullOrEmpty(c)).ToList();

        if (choices.Count > 1)
        {
            SetupChoices();
        }
        else if (choices.Count == 1)
        {
            OnChoiceSelected(choices[0]);
        }
        else
        {
            // 스토리 브레이크 분기
            var script = scriptEventsCache
                .FirstOrDefault(sm => sm.Script_Code.Trim() == currentStory.Script_Text.Trim());
            if (script != null && script.StoryBreak == "Break")
            {
                ResetSkipButton();
                SkipButton.GetComponent<Button>().onClick.AddListener(OnMainStoryComplete);
            }
            else
            {
                NextScene();
            }
        }
    }

    private void SetupChoices()
    {
        ClearChoiceButtons();
        var available = new List<(string code, string text)>();
        for (int i = 1; i <= 3; i++)
        {
            var code = currentStory.GetType()
                .GetProperty($"Choice{i}_Text")
                .GetValue(currentStory) as string;
            if (!string.IsNullOrEmpty(code))
            {
                var display = GetDisplayTextFromScript(code);
                available.Add((code, display));
            }
        }

        foreach (var (code, text) in available)
        {
            var btnObj = Instantiate(choiceButtonPrefab, choiceButtonParent);
            var btn = btnObj.GetComponent<Button>();
            btn.GetComponentInChildren<TMP_Text>().text = text;
            btn.onClick.AddListener(() => OnChoiceSelected(code));
        }
    }

    private void OnChoiceSelected(string newCode)
    {
        ClearChoiceButtons();
        var target = FindStoryBySceneCode(newCode);
        if (target != null)
        {
            currentStory = target;
            currentIndex = storyList.IndexOf(target);
            DisplayCurrentStory();
        }
        else
        {
            Debug.LogWarning("Scene not found: " + newCode);
            OnMainStoryComplete();
        }
    }

    public void NextScene()
    {
        if (isTyping) return;
        var next = storyList.FirstOrDefault(s =>
            s.Chapter_Index == currentStory.Chapter_Index &&
            s.Event_Index == currentStory.Event_Index &&
            s.Script_Index == currentStory.Script_Index + 1);

        if (next != null)
        {
            currentIndex = storyList.IndexOf(next);
            currentStory = next;
            DisplayCurrentStory();
        }
        else
        {
            OnMainStoryComplete();
        }
    }

    private void ResetSkipButton()
    {
        var btn = SkipButton.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => { if (isTyping) isSkip = true; });
    }

    public void OnMainStoryComplete() => onCompleteCallback?.Invoke();

    private void ClearContent()
    {
        foreach (var go in Testblocks) Destroy(go);
        Testblocks.Clear();
        ClearChoiceButtons();
    }

    private void ClearChoiceButtons()
    {
        foreach (Transform t in choiceButtonParent) Destroy(t.gameObject);
    }

    private Story_Master_Main FindStoryBySceneCode(string sceneCode)
        => storyList.FirstOrDefault(s => s.Scene_Code.Trim() == sceneCode.Trim());

    private string GetDisplayTextFromScript(string code)
    {
        var match = scriptEventsCache
            .FirstOrDefault(sm => sm.Script_Code.Trim() == code.Trim());
        return match != null ? match.KOR : code;
    }
}
