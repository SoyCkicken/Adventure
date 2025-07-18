using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Playables;

public class SaveManager : MonoBehaviour
{
    public PlayerState playerState;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private StoryDisplayManager displayManager;
    [SerializeField] private EventDisplay eventDisplay;
    [SerializeField] private GameFlowManager gameFlowManager;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {

            SaveAll(playerState, inventoryManager, eventDisplay, displayManager, gameFlowManager);
            Debug.Log("▶ 플레이어 스탯 저장됨");
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            LoadAll(playerState, inventoryManager, eventDisplay, displayManager, gameFlowManager);

            Debug.Log("▶ 플레이어 스탯 불러오기 완료");
        }
        if (Input.GetKeyDown(KeyCode.F7))
        {
            DeleteSave();

            Debug.Log("▶ 저장된 플레이어 능력치 삭제 완료");
        }
    }

    private static string SavePath => Application.persistentDataPath + "/save.json";

    /// <summary>
    /// SaveData 파일로 저장
    /// </summary>
    //public static void SaveGame(SaveData data)
    //{
    //    string json = JsonUtility.ToJson(data, true);
    //    File.WriteAllText(SavePath, json);
    //    Debug.Log($"[SaveManager] 저장 완료: {SavePath}");
    //}

    ///// <summary>
    ///// SaveData 불러오기
    ///// </summary>
    //public static SaveData LoadGame()
    //{
    //    if (!File.Exists(SavePath))
    //    {
    //        Debug.LogWarning("[SaveManager] 저장 파일 없음");
    //        return null;
    //    }

    //    string json = File.ReadAllText(SavePath);
    //    SaveData data = JsonUtility.FromJson<SaveData>(json);
    //    Debug.Log("[SaveManager] 불러오기 완료");
    //    return data;
    //}

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

    public static void SaveAll(PlayerState player, InventoryManager inventory, EventDisplay eventDisplay, StoryDisplayManager mainStory , GameFlowManager gameFlowManager)
    {
        SaveData data = new SaveData();

        // 각 시스템에 SaveData 전달해서 값 채우기
        player.SavePlayer(ref data);
        inventory.SaveInventoryData(ref data);
        eventDisplay.SaveEventData(ref data);
        mainStory.SaveMainStory(ref data);
        gameFlowManager.SaveFlow(ref data);

        // 최종 저장
        string json = JsonUtility.ToJson(data, true);
        PlayerPrefs.SetString("SaveFile", json);
        Debug.Log("전체 저장 완료");
    }

    public static void LoadAll(PlayerState player, InventoryManager inventory, EventDisplay eventDisplay, StoryDisplayManager mainStory, GameFlowManager gameFlowManager)
    {
        if (!PlayerPrefs.HasKey("SaveFile"))
        {
            Debug.LogWarning("저장 파일 없음");
            return;
        }

        string json = PlayerPrefs.GetString("SaveFile");
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // 각 시스템에 데이터 전달해서 복구
        player.LoadPlayer(data);
        inventory.LoadInventoryData(data);
        //eventDisplay.LoadEventData(data);
        //mainStory.LoadMainStory(data);
        gameFlowManager.LoadFlow(data);

        Debug.Log("전체 불러오기 완료");
    }

    /// <summary>
    /// 저장용 데이터 구조
    /// </summary>
    [System.Serializable]
    public class SaveData
    {
        public string playerName;
        public int STR, INT, AGI, MAG, CHA,Health; //힘 지능 민첩 마력 카리스마
        public int HP, MP; //스토리 진행 체력 , 정신력
        public int Level, Experience , ExperienceRequired; //레벨 , 돈 , 레벨업에 필요한 돈

        public ItemData equippedWeaponData;
        public ItemData equippedArmorData;

        public int MainstoryEventIndex;           // 필터 기준용: Event_Index
        public int MainstoryCurrentIndex;         // 현재 인덱스
        public string MainstorySceneCode;         // 혹시 중복 방지를 위해 Scene_Code도 함께

        public List<int> savedEventGroups = new(); // 남은 랜덤 이벤트 그룹
        public int savedCurrentEventGroup;
        public int savedCurrentEvetnGroupIndex;

        public string flowState; // 예: "Main", "Event", "Battle" 등

        public List<ItemData> inventoryItems = new(); // 인벤토리 전체 직렬화 저장
    }
}
