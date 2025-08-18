//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using TMPro;
//using UnityEditor;
//using UnityEngine;
//using UnityEngine.Playables;
//using UnityEngine.SceneManagement;
//using UnityEngine.UI;
//using static SaveManager;

//public class SaveManager : MonoBehaviour
//{
//    public PlayerState playerState;
//    [SerializeField] private InventoryManager inventoryManager;
//    [SerializeField] private StoryDisplayManager displayManager;
//    [SerializeField] private EventDisplay eventDisplay;
//    [SerializeField] private GameFlowManager gameFlowManager;
//    [SerializeField] private FontSizeManager fontSizeManager;
//    [SerializeField] public Toggle showPatchNoteToggle; // 패치 노트 표시 여부 토글
//    public Button SaveButton;
//    public Button LoadButton;
//    public static SaveManager Instance { get; private set; }
//    private string currentGameVersion;


//    private void Awake()
//    {
//        if (Instance != null && Instance != this)
//        {
//            Destroy(gameObject);
//            return;
//        }
//        Instance = this;
//        DontDestroyOnLoad(gameObject);
//        // 여기서 초기화
//        currentGameVersion = Application.version;
//        //토글 초기화
//        //SaveManager는 게임 시작 시 한 번만 생성지만 씬이 변경이 되면서 또 호출이 될수도 있으니 예외처리
//        if (showPatchNoteToggle != null)
//        {
//            var data = WriteLoadFile();
//            if (data != null)
//                showPatchNoteToggle.isOn = data.showPatchNoteToggle;
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
//        if (playerState == null) playerState = PlayerState.Instance;
//        if (inventoryManager == null) inventoryManager = FindObjectOfType<InventoryManager>();
//        if (displayManager == null) displayManager = FindObjectOfType<StoryDisplayManager>();
//        if (eventDisplay == null) eventDisplay = FindObjectOfType<EventDisplay>();
//        if (gameFlowManager == null) gameFlowManager = FindObjectOfType<GameFlowManager>();
//        if (fontSizeManager == null) fontSizeManager = FindObjectOfType<FontSizeManager>();
//        // 버튼 이벤트 등록
//        if (SaveButton != null) SaveButton.onClick.AddListener(SaveGame);
//        if (LoadButton != null) LoadButton.onClick.AddListener(LoadGame);

//        if (scene.name == "LobbyScenes") // 메인 화면에서만 체크
//        {
//            CheckPatchNoteDisplay();
//        }

//        if (scene.name == "GameScene" && SaveManager.pendingLoadData != null)
//        {
//            var data = SaveManager.pendingLoadData;
//            playerState.LoadPlayer(data);
//            inventoryManager.LoadInventoryData(data);
//            gameFlowManager.LoadFlow(data);
//            SaveManager.pendingLoadData = null; // 한 번 쓰고 초기화
//        }


//    }

//    private void Start()
//    {
//    }

//    public void OnPatchNoteToggleChanged(bool value)
//    {
//        SaveData data = WriteLoadFile();
//        if (data == null) data = new SaveData();
//        data.showPatchNoteToggle = value;
//        WriteSaveFile(data);
//    }


//    // 패치노트 표시 여부 체크
//    private void CheckPatchNoteDisplay()
//    {
//        SaveData data = WriteLoadFile();
//        if (data == null)
//        {
//            ShowPatchNote();
//            return;
//        }

//        if (data.lastSeenVersion != currentGameVersion)
//        {
//            // 버전이 다르면 무조건 표시
//            ShowPatchNote(forceShow: true);
//        }
//        else if (data.showPatchNoteToggle)
//        {
//            // 버전이 같고 토글이 켜져있으면 표시
//            ShowPatchNote();
//        }
//    }
//    private void ShowPatchNote(bool forceShow = false)
//    {
//        Debug.Log("[SaveManager] 패치노트 표시");

//        var patchNoteUI = FindObjectOfType<PatchNoteViewer>(true); // 비활성화 상태까지 검색
//        if (patchNoteUI != null)
//        {
//            patchNoteUI.Open(forceShow);
//        }
//    }

//    private void Update()
//    {

//        if (Input.GetKeyDown(KeyCode.F7))
//        {
//            DeleteSave();

//            Debug.Log("▶ 저장된 플레이어 능력치 삭제 완료");
//        }
//    }

//    public static string SavePath => Application.persistentDataPath + "/save.json";
//    public static SaveData pendingLoadData; // 임시 저장
//    /// <summary>
//    /// 저장 존재 여부 확인
//    /// </summary>
//    public static bool HasSave() => File.Exists(SavePath);

//    /// <summary>
//    /// 저장 삭제 (테스트용)
//    /// </summary>
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
//        playerState.SavePlayer(ref data);
//        gameFlowManager.SaveFlow(ref data);
//        displayManager.SaveMainStory(ref data);
//        eventDisplay.SaveEventData(ref data);
//        inventoryManager.SaveInventoryData(ref data); // 인벤토리 저장
//        data.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
//        // 현재 게임 버전 기록 (이 부분 추가)
//        data.lastSeenVersion = currentGameVersion;
//        string json = JsonUtility.ToJson(data, true);


//        File.WriteAllText(Application.persistentDataPath + "/save.json", json);
//        Debug.Log("저장 완료");
//        if (fontSizeManager != null)
//        {
//            Debug.Log("세이브 버튼 눌림");
//            fontSizeManager.LoadSaveTimeOnly();
//        }
//    }
//    public void LoadGame()
//    {
//        string path = Application.persistentDataPath + "/save.json";
//        if (!File.Exists(path))
//        {
//            Debug.LogWarning("세이브 파일이 없습니다.");
//            return;
//        }

//        string json = File.ReadAllText(path);
//        pendingLoadData = JsonUtility.FromJson<SaveData>(json);
//        ToggleScene(); // 씬 전환
//    }
//    /// <summary>
//    /// 저장용 데이터 구조
//    /// </summary>
//   [System.Serializable]
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
//        public string lastSeenVersion;      // 마지막으로 본 게임 버전
//        public bool showPatchNoteToggle = true; // 같은 버전에서도 표시할지 여부

//        public List<ItemData> inventoryItems = new();
//    }

//    // 저장 파일 로드 (유틸)
//    public SaveData WriteLoadFile()
//    {
//        if (!File.Exists(SavePath))
//            return null;
//        string json = File.ReadAllText(SavePath);
//        return JsonUtility.FromJson<SaveData>(json);
//    }

//    // 저장 파일 저장 (유틸)
//    public void WriteSaveFile(SaveData data)
//    {
//        string json = JsonUtility.ToJson(data, true);
//        File.WriteAllText(SavePath, json);
//    }
//    public void ToggleScene()
//    {
//        var currentScene = SceneManager.GetActiveScene().name;
//        if (currentScene == "GameScene")
//        {
//            SceneManager.LoadScene("LobbyScenes");
//        }
//    }
//}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveManager : MonoBehaviour
{
    // ====== Singleton ======
    public static SaveManager Instance { get; private set; }

    // ====== Scene References (씬이 바뀌면 깨지므로 OnSceneLoaded에서 항상 재바인딩) ======
    public PlayerState playerState;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private StoryDisplayManager displayManager;
    [SerializeField] private EventDisplay eventDisplay;
    [SerializeField] private GameFlowManager gameFlowManager;
    [SerializeField] private FontSizeManager fontSizeManager;

    // UI (씬마다 프리팹/오브젝트가 다를 수 있으니 OnSceneLoaded에서 재연결)
    [SerializeField] public Toggle showPatchNoteToggle; // 패치 노트 표시 여부 토글
    public Button NewGameStartButton;
    public Button SaveButton;
    public Button LoadButton;

    // 버전 기록
    private string currentGameVersion;

    // ====== Save Path / Pending Data ======
    public static string SavePath => Application.persistentDataPath + "/save.json";

    /// <summary>
    /// 씬 전환 중 전달할 로드 데이터(일회성).
    /// LobbyScene → GameScene으로 넘어간 뒤, GameScene에서 이 값을 실제로 적용하고 null로 되돌린다.
    /// </summary>
    public static SaveData pendingLoadData;

    // ====== Unity Lifecycle ======
    private void Awake()
    {
        // 싱글톤 보장
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 현재 실행 중 게임 버전
        currentGameVersion = Application.version;
        if (!HasSave())
        {
            LoadButton.gameObject.SetActive(false); // 저장된 데이터가 없으면 로드 버튼 숨김
        }
    }

    private void OnEnable()
    {
        // 씬 로드 콜백 등록
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // 씬 로드 콜백 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// 씬이 로드될 때마다 필요한 레퍼런스를 안전하게 갱신하고,
    /// 버튼 리스너를 재등록한다. (DontDestroyOnLoad 오브젝트는 씬 참조가 자주 끊긴다)
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // --- 레퍼런스 재바인딩 ---
        if (playerState == null) playerState = PlayerState.Instance;
        if (inventoryManager == null) inventoryManager = FindObjectOfType<InventoryManager>(true);
        if (displayManager == null) displayManager = FindObjectOfType<StoryDisplayManager>(true);
        if (eventDisplay == null) eventDisplay = FindObjectOfType<EventDisplay>(true);
        if (gameFlowManager == null) gameFlowManager = FindObjectOfType<GameFlowManager>(true);
        if (fontSizeManager == null) fontSizeManager = FindObjectOfType<FontSizeManager>(true);
        if(NewGameStartButton != null) NewGameStartButton.onClick.AddListener(() =>
        {
            if (SceneFader.Instance != null)
            {
                SceneFader.Instance.LoadSceneWithFade(
                    sceneName: "GameScene",
                    fadeOut: 0.35f,
                    fadeIn: 0.25f,
                    onBeforeUnload: () =>
                    {
                        ApplyPendingLoadData();
                        playerState.GenerateRandomStats();

                    },
                    onAfterLoad: () =>
                    {
                        // 3) GameScene 로드 직후, 같은 프레임에서 데이터 적용
                        SaveGame(); // 새 게임 시작 시 자동 저장

                    }
                );
            }
            else
            {
                // 백업: SceneFader가 없다면 즉시 로드
                Debug.LogWarning("[SaveManager] SceneFader가 없어 즉시 로드로 대체합니다.");
                SceneManager.LoadScene("GameScene");
                // 로드 직후 적용
                ApplyPendingLoadData();
            }

        });

        // 토글/버튼 레퍼런스가 프리팹 교체로 null일 수 있으니, 씬에서 재탐색 시도
        if (showPatchNoteToggle == null)
        {
            // 이름으로 특정할 수 있으면 GameObject.Find("ShowPatchNoteToggle") 후 GetComponent<Toggle>()로 더 정확히 잡아도 됨.
            showPatchNoteToggle = FindObjectOfType<Toggle>(true);
            // 찾은 토글에 저장된 상태를 반영
            if (showPatchNoteToggle != null)
            {
                var data = WriteLoadFile();
                if (data != null) showPatchNoteToggle.isOn = data.showPatchNoteToggle;
                // 토글 변경 시 저장
                showPatchNoteToggle.onValueChanged.RemoveAllListeners();
                showPatchNoteToggle.onValueChanged.AddListener(OnPatchNoteToggleChanged);
            }
        }
        else
        {
            // 이미 직렬화 연결되어 있던 경우에도 리스너 재등록(중복 방지)
            showPatchNoteToggle.onValueChanged.RemoveAllListeners();
            showPatchNoteToggle.onValueChanged.AddListener(OnPatchNoteToggleChanged);

            // 저장값 반영
            var data = WriteLoadFile();
            if (data != null) showPatchNoteToggle.isOn = data.showPatchNoteToggle;
        }

        // --- 버튼 리스너 재등록(즉시 호출 방지: 괄호 X) ---
        if (SaveButton == null) SaveButton = FindButtonByNameContains("Save");
        if (LoadButton == null) LoadButton = FindButtonByNameContains("Load");

        if (SaveButton != null)
        {
            SaveButton.onClick.RemoveAllListeners();
            SaveButton.onClick.AddListener(SaveGame);
        }
        if (LoadButton != null)
        {
            LoadButton.onClick.RemoveAllListeners();
            LoadButton.onClick.AddListener(OnClickLoadGame); // ※ 괄호 없음!
        }

        // --- 씬별 초기화 ---
        if (scene.name == "LobbyScenes")
        {
            // 메인 화면에서만 패치노트 표시 여부 체크
            CheckPatchNoteDisplay();
        }

        if (scene.name == "GameScene")
        {
            // pending 데이터가 준비되어 있다면 실제 적용
            ApplyPendingLoadData();
        }
    }

    // ====== Patch Note ======

    public void OnPatchNoteToggleChanged(bool value)
    {
        SaveData data = WriteLoadFile();
        if (data == null) data = new SaveData();
        data.showPatchNoteToggle = value;
        WriteSaveFile(data);
    }

    /// <summary>
    /// 패치노트 표시 여부 체크
    /// </summary>
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
            ShowPatchNote(); //세이브 데이터가 없으면 무조건 표시
            return;
        }

        if (data.lastSeenVersion != currentGameVersion)
        {
            // 버전이 다르면 무조건 표시
            ShowPatchNote(forceShow: true);
        }
        else if (data.showPatchNoteToggle)
        {
            // 버전이 같고 토글이 켜져있으면 표시
            ShowPatchNote();
        }
    }

    private void ShowPatchNote(bool forceShow = false)
    {
        Debug.Log("[SaveManager] 패치노트 표시");
        var patchNoteUI = FindObjectOfType<PatchNoteViewer>(true); // 비활성화 상태까지 검색
        if (patchNoteUI != null)
        {
            patchNoteUI.Open(forceShow);
        }
    }

    // ====== Debug ======

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F7))
        {
            DeleteSave();
            Debug.Log("▶ 저장된 플레이어 능력치 삭제 완료");
        }
    }

    // ====== Save/Load Core ======

    /// <summary>
    /// 저장 존재 여부
    /// </summary>
    public static bool HasSave() => File.Exists(SavePath);

    /// <summary>
    /// 저장 삭제 (테스트용)
    /// </summary>
    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("[SaveManager] 저장 파일 삭제됨");
        }
    }

    /// <summary>
    /// 저장
    /// </summary>
    public void SaveGame()
    {
        SaveData data = new SaveData();

        // 각 시스템에 저장 위임
        playerState?.SavePlayer(ref data);
        gameFlowManager?.SaveFlow(ref data);
        displayManager?.SaveMainStory(ref data);
        eventDisplay?.SaveEventData(ref data);
        inventoryManager?.SaveInventoryData(ref data);

        data.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // 현재 게임 버전 기록
        data.lastSeenVersion = currentGameVersion;

        // 파일로 기록
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);

        Debug.Log("[SaveManager] 저장 완료 → " + SavePath);

        // 저장시간 UI 갱신 등
        if (fontSizeManager != null)
        {
            Debug.Log("세이브 버튼 눌림");
            fontSizeManager.LoadSaveTimeOnly();
        }
    }

    /// <summary>
    /// (유틸) 저장 파일 로드 → SaveData 반환. 씬 전환은 하지 않는다.
    /// </summary>
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

    /// <summary>
    /// 로드 버튼 핸들러.
    /// 1) 저장 파일 검사 및 읽기
    /// 2) pendingLoadData에 적재
    /// 3) 페이드 아웃 → GameScene 로드 → 씬 로드 직후 ApplyPendingLoadData() 호출 → 페이드 인
    /// </summary>
    public void OnClickLoadGame()
    {
        if (!HasSave())
        {
            Debug.LogWarning("[SaveManager] 세이브 파일이 없습니다.");
            return;
        }

        // 1) 저장 파일을 읽어 pending에 적재
        pendingLoadData = ReadSaveFile();
        if (pendingLoadData == null)
        {
            Debug.LogWarning("[SaveManager] 저장 파일 읽기에 실패했습니다.");
            return;
        }

        // 2) 페이드 전환으로 GameScene 로드
        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.LoadSceneWithFade(
                sceneName: "GameScene",
                fadeOut: 0.35f,
                fadeIn: 0.25f,
                onBeforeUnload: () =>
                {
                    // 로비에서 떠나기 직전 정리할 것(사운드 Stop 등) 필요 시 여기에
                    ApplyPendingLoadData();
                },
                onAfterLoad: () =>
                {
                    // 3) GameScene 로드 직후, 같은 프레임에서 데이터 적용
                    
                }
            );
        }
        else
        {
            // 백업: SceneFader가 없다면 즉시 로드
            Debug.LogWarning("[SaveManager] SceneFader가 없어 즉시 로드로 대체합니다.");
            SceneManager.LoadScene("GameScene");
            // 로드 직후 적용
            ApplyPendingLoadData();
        }
    }

    /// <summary>
    /// pendingLoadData를 실제 씬 오브젝트들에 적용하고, 일회성으로 null 초기화.
    /// OnSceneLoaded("GameScene")과 onAfterLoad 둘 다 이 함수를 호출하게 설계하여
    /// 흐름이 끊기지 않도록 했다(중복 적용 방지를 위해 null 체크).
    /// </summary>
    private void ApplyPendingLoadData()
    {
        if (pendingLoadData == null) return;

        // 씬 내 레퍼런스가 혹시 비어있다면 재바인딩
        if (playerState == null) playerState = PlayerState.Instance;
        if (inventoryManager == null) inventoryManager = FindObjectOfType<InventoryManager>(true);
        if (displayManager == null) displayManager = FindObjectOfType<StoryDisplayManager>(true);
        if (eventDisplay == null) eventDisplay = FindObjectOfType<EventDisplay>(true);
        if (gameFlowManager == null) gameFlowManager = FindObjectOfType<GameFlowManager>(true);

        var data = pendingLoadData;

        // 실제 데이터 적용 (존재하는 것만)
        playerState?.LoadPlayer(data);
        inventoryManager?.LoadInventoryData(data);
        gameFlowManager?.LoadFlow(data);
        displayManager?.LoadMainStory(data);
        eventDisplay?.LoadEventData(data);

        // 일회성 사용
        pendingLoadData = null;

        Debug.Log("[SaveManager] pendingLoadData 적용 완료");
    }

    // ====== JSON 유틸 ======

    /// <summary> 저장 파일 로드 (외부에서도 호출하는 간단 유틸) </summary>
    public SaveData WriteLoadFile()
    {
        if (!File.Exists(SavePath))
            return null;
        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<SaveData>(json);
    }

    /// <summary> 저장 파일 저장 (외부에서도 호출하는 간단 유틸) </summary>
    public void WriteSaveFile(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
    }

    // ====== Helper: 이름에 키워드가 들어간 버튼을 느슨하게 찾는다(씬마다 프리팹 이름이 달라도 어느 정도 대응) ======
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

    // ====== 저장용 데이터 구조 ======
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
        public string lastSeenVersion;             // 마지막으로 본 게임 버전
        public bool showPatchNoteToggle = true;    // 같은 버전에서도 표시할지 여부

        public List<ItemData> inventoryItems = new();
    }
}

