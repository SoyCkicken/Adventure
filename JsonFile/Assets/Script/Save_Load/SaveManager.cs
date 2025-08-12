using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static SaveManager;

public class SaveManager : MonoBehaviour
{
    public PlayerState playerState;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private StoryDisplayManager displayManager;
    [SerializeField] private EventDisplay eventDisplay;
    [SerializeField] private GameFlowManager gameFlowManager;
    [SerializeField] private FontSizeManager fontSizeManager;
    [SerializeField] public Toggle showPatchNoteToggle; // 패치 노트 표시 여부 토글
    public Button SaveButton;
    public Button LoadButton;
    public static SaveManager Instance { get; private set; }
    private string currentGameVersion;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        // 여기서 초기화
        currentGameVersion = Application.version;
        //토글 초기화
        //SaveManager는 게임 시작 시 한 번만 생성지만 씬이 변경이 되면서 또 호출이 될수도 있으니 예외처리
        if (showPatchNoteToggle != null)
        {
            var data = WriteLoadFile();
            if (data != null)
                showPatchNoteToggle.isOn = data.showPatchNoteToggle;
        }
        
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
        if (playerState == null) playerState = PlayerState.Instance;
        if (inventoryManager == null) inventoryManager = FindObjectOfType<InventoryManager>();
        if (displayManager == null) displayManager = FindObjectOfType<StoryDisplayManager>();
        if (eventDisplay == null) eventDisplay = FindObjectOfType<EventDisplay>();
        if (gameFlowManager == null) gameFlowManager = FindObjectOfType<GameFlowManager>();
        if (fontSizeManager == null) fontSizeManager = FindObjectOfType<FontSizeManager>();
        // 버튼 이벤트 등록
        if (SaveButton != null) SaveButton.onClick.AddListener(SaveGame);
        if (LoadButton != null) LoadButton.onClick.AddListener(LoadGame);

        if (scene.name == "MainScene") // 메인 화면에서만 체크
        {
            CheckPatchNoteDisplay();
        }

        if (scene.name == "GameScene" && SaveManager.pendingLoadData != null)
        {
            var data = SaveManager.pendingLoadData;
            playerState.LoadPlayer(data);
            inventoryManager.LoadInventoryData(data);
            gameFlowManager.LoadFlow(data);
            SaveManager.pendingLoadData = null; // 한 번 쓰고 초기화
        }

        
    }

    private void Start()
    {
    }

    public void OnPatchNoteToggleChanged(bool value)
    {
        SaveData data = WriteLoadFile();
        if (data == null) data = new SaveData();
        data.showPatchNoteToggle = value;
        WriteSaveFile(data);
    }


    // 패치노트 표시 여부 체크
    private void CheckPatchNoteDisplay()
    {
        SaveData data = WriteLoadFile();
        if (data == null)
        {
            ShowPatchNote();
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

    private void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.F7))
        {
            DeleteSave();

            Debug.Log("▶ 저장된 플레이어 능력치 삭제 완료");
        }
    }

    public static string SavePath => Application.persistentDataPath + "/save.json";
    public static SaveData pendingLoadData; // 임시 저장
    /// <summary>
    /// 저장 존재 여부 확인
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


    public void SaveGame()
    {
        SaveData data = new SaveData();
        playerState.SavePlayer(ref data);
        gameFlowManager.SaveFlow(ref data);
        displayManager.SaveMainStory(ref data);
        eventDisplay.SaveEventData(ref data);
        inventoryManager.SaveInventoryData(ref data); // 인벤토리 저장
        data.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        // 현재 게임 버전 기록 (이 부분 추가)
        data.lastSeenVersion = currentGameVersion;
        string json = JsonUtility.ToJson(data, true);
       

        File.WriteAllText(Application.persistentDataPath + "/save.json", json);
        Debug.Log("저장 완료");
        if (fontSizeManager != null)
        {
            Debug.Log("세이브 버튼 눌림");
            fontSizeManager.LoadSaveTimeOnly();
        }
    }
    public void LoadGame()
    {
        string path = Application.persistentDataPath + "/save.json";
        if (!File.Exists(path))
        {
            Debug.LogWarning("세이브 파일이 없습니다.");
            return;
        }

        string json = File.ReadAllText(path);
        pendingLoadData = JsonUtility.FromJson<SaveData>(json);
        ToggleScene(); // 씬 전환
    }
    /// <summary>
    /// 저장용 데이터 구조
    /// </summary>
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
        public string lastSeenVersion;      // 마지막으로 본 게임 버전
        public bool showPatchNoteToggle = true; // 같은 버전에서도 표시할지 여부

        public List<ItemData> inventoryItems = new();
    }

    // 저장 파일 로드 (유틸)
    public SaveData WriteLoadFile()
    {
        if (!File.Exists(SavePath))
            return null;
        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<SaveData>(json);
    }

    // 저장 파일 저장 (유틸)
    public void WriteSaveFile(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
    }
    public void ToggleScene()
    {
        var currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "GameScene")
        {
            SceneManager.LoadScene("LobbyScenes");
        }
    }
}
