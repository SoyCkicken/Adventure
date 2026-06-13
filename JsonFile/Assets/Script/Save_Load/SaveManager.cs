using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveManager : MonoBehaviour
{
    // ====== Singleton ======
    public static SaveManager Instance { get; private set; }

    // ====== Scene References ======
    public PlayerState playerState;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private StoryDisplayManager displayManager;
    [SerializeField] private EventDisplay eventDisplay;
    [SerializeField] private GameFlowManager gameFlowManager;
    [SerializeField] private FontSizeManager fontSizeManager;
    // UI
    [SerializeField] public Toggle showPatchNoteToggle;
    public Button _startButton;
    public Button SaveButton;
    public Button LoadButton;

    private string currentGameVersion;

    // ====== Save Path / Pending Data ======
    public static string SavePath => Application.persistentDataPath + "/save.json";
    public static SaveData pendingLoadData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        currentGameVersion = Application.version;
        if (!HasSave())
        {
            if (LoadButton != null) LoadButton.gameObject.SetActive(false);
        }
        if (autoCreateOnBoot && !HasSave())
            WriteSaveFile(CreateDefaultSave());
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        InitializeForScene(SceneManager.GetActiveScene());
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeForScene(scene);
    }

    private void InitializeForScene(Scene scene)
    {



        // 레퍼런스 재바인딩
        RefreshReferences();
        //버튼 초기화

        // 토글 설정
        SetupPatchNoteToggle();
        // 버튼 리스너 재등록
        SetupButtons();
        // 새 게임 버튼 설정
        SetupNewGameButton();

        // 씬별 초기화
        if (scene.name == "LobbyScenes")
        {
            CheckPatchNoteDisplay();
        }

        if (scene.name == "GameScene")
        {
            // ✅ 수정: 중복 호출 방지
            ApplyPendingLoadDataOnce();
            //OnClickLoadGame();

        }
    }

    private void RefreshReferences()
    {
        if (playerState == null) playerState = PlayerState.Instance;
        if (inventoryManager == null) inventoryManager = FindObjectOfType<InventoryManager>(true);
        if (displayManager == null) displayManager = FindObjectOfType<StoryDisplayManager>(true);
        if (eventDisplay == null) eventDisplay = FindObjectOfType<EventDisplay>(true);
        if (gameFlowManager == null) gameFlowManager = FindObjectOfType<GameFlowManager>(true);
        if (fontSizeManager == null) fontSizeManager = FindObjectOfType<FontSizeManager>(true);
    }

    private void SetupNewGameButton()
    {
        if (_startButton != null)
        {
            _startButton.onClick.RemoveAllListeners();
            _startButton.onClick.AddListener(() =>
            {
                Debug.Log("시작 버튼 눌림");
                if (SceneFader.Instance != null)
                {
                    SceneFader.Instance.LoadSceneWithFade(
                        sceneName: "GameScene",
                        fadeOut: 0.35f,
                        fadeIn: 0.25f,
                        onBeforeUnload: () =>
                        {
                            playerState?.GenerateRandomStats();
                        },
                        onAfterLoad: () =>
                        {
                            // 1) 레퍼런스 재바인딩(씬 갓 로드 직후)
                            RefreshReferences();
                            // 2) 게임 흐름을 "메인 스토리"로 명시 전환
                            gameFlowManager?.SetState(GameFlowManager.FlowState.MainStory);
                            // 3) 첫 스토리 바로 띄우기
                            StartCoroutine(DelayedDisplayMainStory());
                            // 4) 시작 상태 저장
                            SaveGame();
                        }
                    );
                }
                else
                {
                    Debug.LogWarning("[SaveManager] SceneFader가 없어 즉시 로드로 대체합니다.");
                    SceneManager.LoadScene("GameScene");
                }
            });
        }
    }

    private void SetupPatchNoteToggle()
    {
        if (showPatchNoteToggle == null)
        {
            showPatchNoteToggle = FindObjectOfType<Toggle>(true);
        }

        if (showPatchNoteToggle != null)
        {
            var data = WriteLoadFile();
            if (data != null) showPatchNoteToggle.isOn = data.showPatchNoteToggle;

            showPatchNoteToggle.onValueChanged.RemoveAllListeners();
            showPatchNoteToggle.onValueChanged.AddListener(OnPatchNoteToggleChanged);
        }
    }

    private void SetupButtons()
    {
        if (SaveButton == null) SaveButton = FindButtonByNameContains("Save");
        if (_startButton == null) _startButton = FindButtonByNameContains("Start");
        if (LoadButton == null) LoadButton = FindButtonByNameContains("Load");


        if (SaveButton != null)
        {
            SaveButton.onClick.RemoveAllListeners();
            SaveButton.onClick.AddListener(SaveGame);
        }

        if (LoadButton != null)
        {
            LoadButton.onClick.RemoveAllListeners();
            LoadButton.onClick.AddListener(OnClickLoadGame);
        }
    }

    // ✅ 수정: 중복 호출 방지를 위한 새로운 메서드
    private void ApplyPendingLoadDataOnce()
    {
        if (pendingLoadData == null) return;

        Debug.Log("[SaveManager] pendingLoadData 적용 시작");

        // 레퍼런스 재확인
        RefreshReferences();

        var data = pendingLoadData;
        
        // 실제 데이터 적용
        playerState?.LoadPlayer(data);
        inventoryManager?.LoadInventoryData(data);

       

        // GameFlowManager 먼저 로드해서 상태 복원
        gameFlowManager?.LoadFlow(data);

        // 스토리/이벤트 로드 후 실제 표시까지 처리
        if (displayManager != null)
        {
            displayManager.LoadMainStory(data);
            // 메인 스토리가 진행 중이었다면 표시
            if (gameFlowManager?.GetCurrentState() == GameFlowManager.FlowState.MainStory)
            {
                StartCoroutine(DelayedDisplayMainStory());
            }
        }

        if (eventDisplay != null)
        {
            eventDisplay.LoadEventData(data);
            // 랜덤 이벤트가 진행 중이었다면 표시
            if (gameFlowManager?.GetCurrentState() == GameFlowManager.FlowState.RandomEvent)
            {
                StartCoroutine(DelayedDisplayEvent());
            }
        }

        // 일회성 사용
        pendingLoadData = null;
        Debug.Log("[SaveManager] pendingLoadData 적용 완료");
    }

    // ✅ 추가: 딜레이를 둔 스토리 표시 (UI 초기화 대기)
    private IEnumerator DelayedDisplayMainStory()
    {
        yield return new WaitForEndOfFrame(); // UI 초기화 대기
        if (displayManager != null)
        {
            displayManager.SetOnCompleteCallback(() => {
                //gameFlowManager?.SetState(GameFlowManager.FlowState.None);
            });
            //displayManager.DisplayCurrentStory();
        }
    }

    // ✅ 추가: 딜레이를 둔 이벤트 표시
    private IEnumerator DelayedDisplayEvent()
    {
        yield return new WaitForEndOfFrame();
        if (eventDisplay != null)
        {
            eventDisplay.SetOnCompleteCallback((battleResult) => {
                //gameFlowManager?.SetState(GameFlowManager.FlowState.None);
            });
            //eventDisplay.DisplayCurrentEvent();
        }
    }

    public void OnPatchNoteToggleChanged(bool value)
    {
        SaveData data = WriteLoadFile();
        if (data == null) data = new SaveData();
        data.showPatchNoteToggle = value;
        WriteSaveFile(data);
    }

    private void CheckPatchNoteDisplay()
    {
        SaveData data = WriteLoadFile();
        if (data == null)
        {
            ShowPatchNote();
            return;
        }
        if (!HasSave())
        {
            ShowPatchNote();
            return;
        }

        if (data.lastSeenVersion != currentGameVersion)
        {
            ShowPatchNote(forceShow: true);
        }
        else if (data.showPatchNoteToggle)
        {
            ShowPatchNote();
        }
    }

    private void ShowPatchNote(bool forceShow = false)
    {
        Debug.Log("[SaveManager] 패치노트 표시");
        var patchNoteUI = FindObjectOfType<PatchNoteViewer>(true);
        if (patchNoteUI != null)
        {
            patchNoteUI.Open(forceShow);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F7))
        {

            PlayerPrefs.DeleteAll(); // 모든 플레이어 프리퍼스 삭제
            Debug.Log("▶ 저장된 플레이어 능력치 삭제 완료");
        }
    }

    public static bool HasSave() => File.Exists(SavePath);

    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("[SaveManager] 저장 파일 삭제됨");
        }
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();

        // 기존: 각 시스템에 저장 위임 (변경 없음)
        playerState?.SavePlayer(ref data);
        gameFlowManager?.SaveFlow(ref data);
        displayManager?.SaveMainStory(ref data);
        eventDisplay?.SaveEventData(ref data);
        inventoryManager?.SaveInventoryData(ref data);

        data.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        data.lastSeenVersion = currentGameVersion;

        if (SceneManager.GetActiveScene().name != "GameScene")
        {
            data.showPatchNoteToggle = showPatchNoteToggle.isOn;
        }
        // 파일로 기록
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);

        Debug.Log("[SaveManager] 저장 완료 → " + SavePath);

        if (fontSizeManager != null)
        {
            Debug.Log("세이브 버튼 눌림");
            fontSizeManager.LoadSaveTimeOnly();
        }
    }

    public SaveData ReadSaveFile()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("[SaveManager] 세이브 파일이 없습니다.");
            return null;
        }
        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<SaveData>(json);
    }

    // ✅ 수정: 중복 호출 제거
    public void OnClickLoadGame()
    {
        if (!HasSave()) { Debug.LogWarning("[SaveManager] 세이브 없음"); return; }
        pendingLoadData = ReadSaveFile();
        if (pendingLoadData == null) { Debug.LogWarning("[SaveManager] 로컬 읽기 실패"); return; }
        EnterGameScene();
    }
    private void EnterGameScene()
    {
        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.LoadSceneWithFade(
                sceneName: "GameScene",
                fadeOut: 0.35f,
                fadeIn: 0.25f,
                onBeforeUnload: null,
                onAfterLoad: null
            );
        }
        else
        {
            //SceneManager.LoadScene("GameScene");
        }
    }

    public SaveData WriteLoadFile()
    {
        if (!File.Exists(SavePath))
            return null;
        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<SaveData>(json);
    }


    public void WriteSaveFile(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
    }

    private Button FindButtonByNameContains(string keyword)
    {
        var buttons = FindObjectsOfType<Button>(true);
        foreach (var b in buttons)
        {
            if (b.name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                return b;
        }
        return null;
    }
    public void ToggleScene()
    {
        var currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "GameScene")
        {
            SceneManager.LoadScene("LobbyScenes");
        }
    }

    [Header("Save – options")]
    public bool autoCreateOnBoot = true;   // 처음 실행 시 자동 생성 여부

    public SaveData CreateDefaultSave()
    {
        return new SaveData
        {
            // 처음값 원하는 대로 채워
            lastSeenVersion = "",   // 아직 패치노트를 본 버전 없음
            showPatchNoteToggle = true // 기본 표시할지 여부(기본값 결정)
                                       // ... 나머지 세이브 기본값
        };
    }

    // 세이브가 없으면 생성하고 반환
    public SaveData GetOrCreateSave()
    {
        var data = ReadSaveFile();           // 네 프로젝트의 읽기 함수
        if (data == null)
        {
            data = CreateDefaultSave();
            WriteSaveFile(data);             // 즉시 생성
        }
        return data;
    }

    [System.Serializable]
    public class SaveData
    {
        public string playerName;
        public int STR, INT, AGI, MAG, CHA, Health;
        public int HP, MP;
        public int Level, Experience, ExperienceRequired;

        public ItemData equippedWeaponData;
        public ItemData equippedArmorData;

        public int PlayerCurrentChapterIndex;
        public int MainstoryEventIndex;
        public int MainstoryCurrentIndex;
        public string MainstorySceneCode;

        public List<int> savedEventGroups = new();
        public int savedCurrentEventGroup;
        public int savedCurrentEvetnGroupIndex;

        public string flowState;
        public string saveTime;

        // 패치노트 관련
        public string lastSeenVersion;
        public bool showPatchNoteToggle;

        public List<ItemData> inventoryItems = new();
    }
}
