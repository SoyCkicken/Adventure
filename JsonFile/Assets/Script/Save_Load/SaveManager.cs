//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using UnityEngine.UI;

//public class SaveManager : MonoBehaviour
//{
//    // ====== Singleton ======
//    public static SaveManager Instance { get; private set; }

//    // ====== Scene References ======
//    public PlayerState playerState;
//    [SerializeField] private InventoryManager inventoryManager;
//    [SerializeField] private StoryDisplayManager displayManager;
//    [SerializeField] private EventDisplay eventDisplay;
//    [SerializeField] private GameFlowManager gameFlowManager;
//    [SerializeField] private FontSizeManager fontSizeManager;
//    // UI
//    [SerializeField] public Toggle showPatchNoteToggle;
//    public Button _startButton;
//    public Button SaveButton;
//    public Button LoadButton;

//    private string currentGameVersion;

//    // ====== Save Path / Pending Data ======
//    public static string SavePath => Application.persistentDataPath + "/save.json";
//    public static SaveData pendingLoadData;

//    private void Awake()
//    {
//        if (Instance != null && Instance != this)
//        {
//            Destroy(gameObject);
//            return;
//        }
//        Instance = this;
//        DontDestroyOnLoad(gameObject);

//        currentGameVersion = Application.version;
//        if (!HasSave())
//        {
//            if (LoadButton != null) LoadButton.gameObject.SetActive(false);
//        }
//    }

//    private void OnEnable()
//    {
//        SceneManager.sceneLoaded += OnSceneLoaded;
//    }

//    private void OnDisable()
//    {
//        SceneManager.sceneLoaded -= OnSceneLoaded;
//    }

//    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//    {



//        // 레퍼런스 재바인딩
//        RefreshReferences();
//        //버튼 초기화

//        // 토글 설정
//        SetupPatchNoteToggle();
//        // 새 게임 버튼 설정
//        SetupNewGameButton();

//        // 버튼 리스너 재등록
//        SetupButtons();

//        // 씬별 초기화
//        if (scene.name == "LobbyScenes")
//        {
//            CheckPatchNoteDisplay();
//        }

//        if (scene.name == "GameScene")
//        {
//            // ✅ 수정: 중복 호출 방지
//            ApplyPendingLoadDataOnce();
//        }
//    }

//    private void RefreshReferences()
//    {
//        if (playerState == null) playerState = PlayerState.Instance;
//        if (inventoryManager == null) inventoryManager = FindObjectOfType<InventoryManager>(true);
//        if (displayManager == null) displayManager = FindObjectOfType<StoryDisplayManager>(true);
//        if (eventDisplay == null) eventDisplay = FindObjectOfType<EventDisplay>(true);
//        if (gameFlowManager == null) gameFlowManager = FindObjectOfType<GameFlowManager>(true);
//        if (fontSizeManager == null) fontSizeManager = FindObjectOfType<FontSizeManager>(true);
//    }

//    private void SetupNewGameButton()
//    {
//        if (_startButton != null)
//        {
//            _startButton.onClick.RemoveAllListeners();
//            _startButton.onClick.AddListener(() =>
//            {
//                if (SceneFader.Instance != null)
//                {
//                    SceneFader.Instance.LoadSceneWithFade(
//                        sceneName: "GameScene",
//                        fadeOut: 0.35f,
//                        fadeIn: 0.25f,
//                        onBeforeUnload: () =>
//                        {
//                            playerState?.GenerateRandomStats();
//                        },
//                        onAfterLoad: () =>
//                        {
//                            // 1) 레퍼런스 재바인딩(씬 갓 로드 직후)
//                            RefreshReferences();
//                                 // 2) 게임 흐름을 "메인 스토리"로 명시 전환
//                            gameFlowManager?.SetState(GameFlowManager.FlowState.MainStory);
//                                 // 3) 첫 스토리 바로 띄우기
//                            StartCoroutine(DelayedDisplayMainStory());
//                                 // 4) 시작 상태 저장
//                            SaveGame();
//                        }
//                    );
//                }
//                else
//                {
//                    Debug.LogWarning("[SaveManager] SceneFader가 없어 즉시 로드로 대체합니다.");
//                    SceneManager.LoadScene("GameScene");
//                }
//            });
//        }
//    }

//    private void SetupPatchNoteToggle()
//    {
//        if (showPatchNoteToggle == null)
//        {
//            showPatchNoteToggle = FindObjectOfType<Toggle>(true);
//        }

//        if (showPatchNoteToggle != null)
//        {
//            var data = WriteLoadFile();
//            if (data != null) showPatchNoteToggle.isOn = data.showPatchNoteToggle;

//            showPatchNoteToggle.onValueChanged.RemoveAllListeners();
//            showPatchNoteToggle.onValueChanged.AddListener(OnPatchNoteToggleChanged);
//        }
//    }

//    private void SetupButtons()
//    {
//        if (SaveButton == null) SaveButton = FindButtonByNameContains("Save");
//        if (_startButton == null) _startButton = FindButtonByNameContains("Start");
//        if (LoadButton == null) LoadButton = FindButtonByNameContains("Load");


//        if (SaveButton != null)
//        {
//            SaveButton.onClick.RemoveAllListeners();
//            SaveButton.onClick.AddListener(SaveGame);
//        }

//        if (LoadButton != null)
//        {
//            LoadButton.onClick.RemoveAllListeners();
//            LoadButton.onClick.AddListener(OnClickLoadGame);
//        }
//        if (_startButton != null)
//        {
//            _startButton.onClick.RemoveAllListeners();
//            _startButton.onClick.AddListener(() =>
//            {
//                if (SceneFader.Instance != null)
//                {
//                    SceneFader.Instance.LoadSceneWithFade(
//                        sceneName: "GameScene",
//                        fadeOut: 0.35f,
//                        fadeIn: 0.25f,
//                        onBeforeUnload: () =>
//                        {
//                            //DeleteSave(); // 새 게임 시작 시 기존 저장 삭제
//                            playerState?.GenerateRandomStats();
//                        },
//                        onAfterLoad: () =>
//                        {
//                            SaveGame(); // 새 게임 시작 시 자동 저장
//                        }
//                    );
//                }
//            });
//        }
//    }

//    // ✅ 수정: 중복 호출 방지를 위한 새로운 메서드
//    private void ApplyPendingLoadDataOnce()
//    {
//        if (pendingLoadData == null) return;

//        Debug.Log("[SaveManager] pendingLoadData 적용 시작");

//        // 레퍼런스 재확인
//        RefreshReferences();

//        var data = pendingLoadData;

//        // 실제 데이터 적용
//        playerState?.LoadPlayer(data);
//        inventoryManager?.LoadInventoryData(data);

//        // GameFlowManager 먼저 로드해서 상태 복원
//        gameFlowManager?.LoadFlow(data);

//        // 스토리/이벤트 로드 후 실제 표시까지 처리
//        if (displayManager != null)
//        {
//            displayManager.LoadMainStory(data);
//            // 메인 스토리가 진행 중이었다면 표시
//            if (gameFlowManager?.GetCurrentState() == GameFlowManager.FlowState.MainStory)
//            {
//                StartCoroutine(DelayedDisplayMainStory());
//            }
//        }

//        if (eventDisplay != null)
//        {
//            eventDisplay.LoadEventData(data);
//            // 랜덤 이벤트가 진행 중이었다면 표시
//            if (gameFlowManager?.GetCurrentState() == GameFlowManager.FlowState.RandomEvent)
//            {
//                StartCoroutine(DelayedDisplayEvent());
//            }
//        }

//        // 일회성 사용
//        pendingLoadData = null;
//        Debug.Log("[SaveManager] pendingLoadData 적용 완료");
//    }

//    // ✅ 추가: 딜레이를 둔 스토리 표시 (UI 초기화 대기)
//    private IEnumerator DelayedDisplayMainStory()
//    {
//        yield return new WaitForEndOfFrame(); // UI 초기화 대기
//        if (displayManager != null)
//        {
//            displayManager.SetOnCompleteCallback(() => {
//                //gameFlowManager?.SetState(GameFlowManager.FlowState.None);
//            });
//            //displayManager.DisplayCurrentStory();
//        }
//    }

//    // ✅ 추가: 딜레이를 둔 이벤트 표시
//    private IEnumerator DelayedDisplayEvent()
//    {
//        yield return new WaitForEndOfFrame();
//        if (eventDisplay != null)
//        {
//            eventDisplay.SetOnCompleteCallback((battleResult) => {
//                //gameFlowManager?.SetState(GameFlowManager.FlowState.None);
//            });
//            //eventDisplay.DisplayCurrentEvent();
//        }
//    }

//    public void OnPatchNoteToggleChanged(bool value)
//    {
//        SaveData data = WriteLoadFile();
//        if (data == null) data = new SaveData();
//        data.showPatchNoteToggle = value;
//        WriteSaveFile(data);
//    }

//    private void CheckPatchNoteDisplay()
//    {
//        SaveData data = WriteLoadFile();
//        if (data == null)
//        {
//            ShowPatchNote();
//            return;
//        }
//        if (!HasSave())
//        {
//            ShowPatchNote();
//            return;
//        }

//        if (data.lastSeenVersion != currentGameVersion)
//        {
//            ShowPatchNote(forceShow: true);
//        }
//        else if (data.showPatchNoteToggle)
//        {
//            ShowPatchNote();
//        }
//    }

//    private void ShowPatchNote(bool forceShow = false)
//    {
//        Debug.Log("[SaveManager] 패치노트 표시");
//        var patchNoteUI = FindObjectOfType<PatchNoteViewer>(true);
//        if (patchNoteUI != null)
//        {
//            patchNoteUI.Open(forceShow);
//        }
//    }

//    private void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.F7))
//        {

//            PlayerPrefs.DeleteAll(); // 모든 플레이어 프리퍼스 삭제
//            Debug.Log("▶ 저장된 플레이어 능력치 삭제 완료");
//        }
//    }

//    public static bool HasSave() => File.Exists(SavePath);

//    public static void DeleteSave()
//    {
//        if (File.Exists(SavePath))
//        {
//            File.Delete(SavePath);
//            Debug.Log("[SaveManager] 저장 파일 삭제됨");
//        }
//    }

//    public void SaveGame()
//    {
//        SaveData data = new SaveData();

//        // 각 시스템에 저장 위임
//        playerState?.SavePlayer(ref data);
//        gameFlowManager?.SaveFlow(ref data);
//        displayManager?.SaveMainStory(ref data);
//        eventDisplay?.SaveEventData(ref data);
//        inventoryManager?.SaveInventoryData(ref data);

//        data.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
//        data.lastSeenVersion = currentGameVersion;

//        if (SceneManager.GetActiveScene().name != "GameScene")
//        {
//            data.showPatchNoteToggle = showPatchNoteToggle.isOn; // 꼭 로비 씬에서 토글 상태 저장
//            Debug.Log("로비씬에서 저장을 시도 합니다");
//        }
//        else
//        {
//            Debug.Log("다른 씬에서 저장을 시도 했습니다");
//            Debug.Log($" 토글값 : {data.showPatchNoteToggle}");
//        }


//            // 파일로 기록
//            string json = JsonUtility.ToJson(data, true);
//        File.WriteAllText(SavePath, json);

//        Debug.Log("[SaveManager] 저장 완료 → " + SavePath);

//        if (fontSizeManager != null)
//        {
//            Debug.Log("세이브 버튼 눌림");
//            fontSizeManager.LoadSaveTimeOnly();
//        }
//    }

//    public SaveData ReadSaveFile()
//    {
//        if (!File.Exists(SavePath))
//        {
//            Debug.LogWarning("[SaveManager] 세이브 파일이 없습니다.");
//            return null;
//        }
//        string json = File.ReadAllText(SavePath);
//        return JsonUtility.FromJson<SaveData>(json);
//    }

//    // ✅ 수정: 중복 호출 제거
//    public void OnClickLoadGame()
//    {
//        if (!HasSave())
//        {
//            Debug.LogWarning("[SaveManager] 세이브 파일이 없습니다.");
//            return;
//        }

//        pendingLoadData = ReadSaveFile();
//        if (pendingLoadData == null)
//        {
//            Debug.LogWarning("[SaveManager] 저장 파일 읽기에 실패했습니다.");
//            return;
//        }

//        if (SceneFader.Instance != null)
//        {
//            SceneFader.Instance.LoadSceneWithFade(
//                sceneName: "GameScene",
//                fadeOut: 0.35f,
//                fadeIn: 0.25f,
//                onBeforeUnload: null, // ✅ 수정: 여기서는 호출하지 않음
//                onAfterLoad: null     // ✅ 수정: 여기서도 호출하지 않음 (OnSceneLoaded에서 처리)
//            );
//        }
//        else
//        {
//            Debug.LogWarning("[SaveManager] SceneFader가 없어 즉시 로드로 대체합니다.");
//            SceneManager.LoadScene("GameScene");
//        }
//    }

//    public SaveData WriteLoadFile()
//    {
//        if (!File.Exists(SavePath))
//            return null;
//        string json = File.ReadAllText(SavePath);
//        return JsonUtility.FromJson<SaveData>(json);
//    }

//    public void WriteSaveFile(SaveData data)
//    {
//        string json = JsonUtility.ToJson(data, true);
//        File.WriteAllText(SavePath, json);
//    }

//    private Button FindButtonByNameContains(string keyword)
//    {
//        var buttons = FindObjectsOfType<Button>(true);
//        foreach (var b in buttons)
//        {
//            if (b.name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
//                return b;
//        }
//        return null;
//    }
//    public void ToggleScene()
//    {
//        var currentScene = SceneManager.GetActiveScene().name;
//        if (currentScene == "GameScene")
//        {
//            SceneManager.LoadScene("LobbyScenes");
//        }
//    }

//    [System.Serializable]
//    public class SaveData
//    {
//        public string playerName;
//        public int STR, INT, AGI, MAG, CHA, Health;
//        public int HP, MP;
//        public int Level, Experience, ExperienceRequired;

//        public ItemData equippedWeaponData;
//        public ItemData equippedArmorData;

//        public int PlayerCurrentChapterIndex;
//        public int MainstoryEventIndex;
//        public int MainstoryCurrentIndex;
//        public string MainstorySceneCode;

//        public List<int> savedEventGroups = new();
//        public int savedCurrentEventGroup;
//        public int savedCurrentEvetnGroupIndex;

//        public string flowState;
//        public string saveTime;

//        // 패치노트 관련
//        public string lastSeenVersion;
//        public bool showPatchNoteToggle;

//        public List<ItemData> inventoryItems = new();
//    }
//}


using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text;                  // ← 추가
using UnityEngine.Networking;       // ← 추가

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

    // === [추가] 서버 동기화 옵션 ===
    [Header("Server Sync")]
    [Tooltip("배포 시 https://pofol2025.cafe24app.com, 로컬 테스트는 http://localhost:8001")]
    public string serverBaseUrl = "http://localhost:8001";
    [Tooltip("플레이어 식별자(계정/슬롯). 서버 저장 키로 사용됩니다.")]
    public string playerId = "USER_001";
    [Tooltip("로드 시 서버 데이터를 우선 시도할지 여부(오프라인/개발중이면 false 권장).")]
    public bool useServerOnLoad = false;

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

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {



        // 레퍼런스 재바인딩
        RefreshReferences();
        //버튼 초기화

        // 토글 설정
        SetupPatchNoteToggle();
        // 새 게임 버튼 설정
        SetupNewGameButton();

        // 버튼 리스너 재등록
        SetupButtons();

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
            //LoadFromServerThenEnterScene();

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
        if (_startButton != null)
        {
            _startButton.onClick.RemoveAllListeners();
            _startButton.onClick.AddListener(() =>
            {
                if (SceneFader.Instance != null)
                {
                    SceneFader.Instance.LoadSceneWithFade(
                        sceneName: "GameScene",
                        fadeOut: 0.35f,
                        fadeIn: 0.25f,
                        onBeforeUnload: () =>
                        {
                            //DeleteSave(); // 새 게임 시작 시 기존 저장 삭제
                            playerState?.GenerateRandomStats();
                        },
                        onAfterLoad: () =>
                        {
                            SaveGame(); // 새 게임 시작 시 자동 저장
                        }
                    );
                }
            });
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
        // === [추가] 서버에 비동기 업로드 (오류는 게임 진행을 막지 않음) ===
        if (!string.IsNullOrEmpty(serverBaseUrl))
        {
            StartCoroutine(UploadSaveToServer(playerId, data));
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
        if (useServerOnLoad && !string.IsNullOrEmpty(serverBaseUrl))
        {
            StartCoroutine(LoadFromServerThenEnterScene());
            return;
        }
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

    // ================== [추가] 서버 통신 루틴 ==================

    // 서버 저장 업로드: POST /api/v1/save  (body: { playerId, data })
    private IEnumerator UploadSaveToServer(string pid, SaveData data)
    {
        // serverBaseUrl 뒤에 슬래시 유무와 /api/v1/save 중복을 막는다
        var url = CombineUrl(serverBaseUrl, "api/v1", "save");

        var payload = new SaveUpload { playerId = pid, data = data };
        string json = JsonUtility.ToJson(payload);

        using (var req = new UnityWebRequest(url, "POST"))
        {
            req.timeout = 10;
            byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

#if UNITY_2020_3_OR_NEWER
            bool ok = req.result == UnityWebRequest.Result.Success;
#else
        bool ok = !(req.isNetworkError || req.isHttpError);
#endif
            if (!ok)
            {
                Debug.LogWarning($"[SaveManager] 서버 업로드 실패: HTTP {req.responseCode} {req.error} {req.downloadHandler.text}");
            }
            else
            {
                Debug.Log($"[SaveManager] 서버 업로드 성공: {req.downloadHandler.text}");
            }
        }
    }


    // 서버에서 로드 → 로컬 파일로 저장 → 씬 진입
    private IEnumerator LoadFromServerThenEnterScene()
    {
        var url = CombineUrl(serverBaseUrl, "api/v1", "save", UnityWebRequest.EscapeURL(playerId));
        SaveData serverData = null;

        using (var req = UnityWebRequest.Get(url))
        {
            req.timeout = 10;
            yield return req.SendWebRequest();

#if UNITY_2020_3_OR_NEWER
            var ok = req.result == UnityWebRequest.Result.Success;
#else
        var ok = !(req.isNetworkError || req.isHttpError);
#endif
            if (!ok)
            {
                Debug.LogWarning($"[SaveManager] 서버 로드 실패: HTTP {req.responseCode} {req.error}");
            }
            else
            {
                var raw = req.downloadHandler.text;
                //Debug.Log($"[SaveManager] RAW: {raw}");

                try
                {
                    // 1) payload_json이 객체로 오는 일반 케이스
                    var respObj = JsonUtility.FromJson<ServerLoadResponse>(raw);
                    if (respObj != null && respObj.ok && respObj.data != null && respObj.data.payload_json != null)
                    {
                        serverData = respObj.data.payload_json;
                        Debug.Log("[SaveManager] 서버 데이터 수신(Object) ✓");
                    }
                    else
                    {
                        // 2) payload_json이 문자열로 오는 케이스 방어
                        var respStr = JsonUtility.FromJson<ServerLoadResponseStr>(raw);
                        if (respStr != null && respStr.ok && respStr.data != null && !string.IsNullOrEmpty(respStr.data.payload_json))
                        {
                            serverData = JsonUtility.FromJson<SaveData>(respStr.data.payload_json);
                            Debug.Log("[SaveManager] 서버 데이터 수신(String→SaveData) ✓");
                        }
                        else
                        {
                            Debug.LogWarning("[SaveManager] 서버 응답은 성공이지만 data가 비었거나 예상과 다릅니다.");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[SaveManager] 서버 응답 파싱 실패: {e.Message}");
                }
            }
        }

        if (serverData != null)
        {
            WriteSaveFile(serverData);
            pendingLoadData = serverData;
            EnterGameScene();
            yield break;
        }

        // 폴백: 로컬 세이브
        if (HasSave())
        {
            pendingLoadData = ReadSaveFile();
            if (pendingLoadData != null) { EnterGameScene(); yield break; }
        }

        Debug.LogWarning("[SaveManager] 서버/로컬 모두 저장 없음");
    }

    // 클래스 내부 어딘가(예: 필드들 아래)에 추가
    private static string CombineUrl(params string[] parts)
    {
        if (parts == null || parts.Length == 0) return string.Empty;
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < parts.Length; i++)
        {
            if (string.IsNullOrEmpty(parts[i])) continue;
            var p = parts[i].Trim();
            if (i == 0) sb.Append(p.TrimEnd('/'));
            else sb.Append('/').Append(p.Trim('/'));
        }
        return sb.ToString();
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

    [Serializable]
    private class ServerLoadResponseStr
    {
        public bool ok;
        public ServerLoadRowStr data;
    }

    [Serializable]
    private class ServerLoadRowStr
    {
        public string player_id;
        public string payload_json; // payload_json이 "문자열"일 때 받기
        public string updated_at;
    }
    // 서버 업로드용 래퍼
    [Serializable]
    private class ServerLoadResponse
    {
        public bool ok;
        public ServerLoadRow data;     // 서버의 "data" 객체 (없으면 null)
    }

    [Serializable]
    private class ServerLoadRow
    {
        public string player_id;
        public SaveData payload_json;  // ★ JSON 컬럼을 SaveData로 직접 매핑
        public string updated_at;      // 참고용
    }
    [Serializable]
    private class SaveUpload
    {
        public string playerId;  // 서버에서 요구하는 필드명과 동일
        public SaveData data;    // 네 게임의 저장 데이터 구조
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
