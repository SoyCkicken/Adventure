//using System.Collections.Generic;
//using UnityEngine;
//using Newtonsoft.Json;
//using System.Linq;

//public class JsonManager : MonoBehaviour
//{
//    [Header("JSON File Paths (이벤트 리스트에 있는 Json파일)")]
//    public string storyMasterFile = "TRPG_ScriptData_StoryMasterMain";
//    public string scriptMasterMainFile = "TRPG_ScriptData_ScriptMasterMain";
//    public string successRateMasterMainFile = "TRPG_ScriptData_SuccessRateMasterMain";
//    public string randomEventsFile = "RandomEvents_Master_Custom_Format";
//    public string scriptMasterEventFile = "Script_Master_Event_Custom_Format";
//    public string successRateMasterRandomEventsFile = "SuccessRate_Master_RandomEvents_Custom_Format";
//    public string MainQuesteffectMasterFile = "Quest_Effect_Master_Custom_Format";
//    public string EventQuesteffectMasterFile = "Quest_Effect_Master_Custom_Format";
//    [Header("JSON File Paths (아이템 목록 Json파일)")]
//    public string ItemWeaponMasterFile = "Weapon_Master";
//    public string ItemOptionMasterFile = "Option_Master";
//    public string ItemArmorMasterFile = "Armor_Master";
//    public string ItemMasterFile = "Item_Master";
//    [Header("JSON File Paths (몬스터 목록 Json파일)")]
//    public string Monster_DataFile = "MonsterData";
//    public string Monster_EffectFile = "MonsterEffect";
//    public string allItemMasterArmorMasterFile;
//    public string allItemMasterItemMasterFile;
//    public string allItemMasterOptionMasterFile;
//    public string allItemMasterWeaponMasterFile;
//    [Header("JSON File Paths (몬스터 목록 Json파일)")]
//    public string Test5;
//    public string Test6;
//    public string Test7;
//    public string Test8;
//    [Header("스토리")]
//    public List<Story_Master_Main> storyMasters;
//    public List<Main_Script_Master_Main> scriptMasterMains;
//    public List<Main_SuccessRate_Master_Main> successRateMasterMains;
//    public List<Story_Effect_Master> Story_effectMasters;
//    [Header("이벤트")]
//    public List<RandomEvents_Master_Event> randomEvents;
//    public List<Ran_Script_Master_Event> scriptMasterEvents;
//    public List<Ran_SuccessRate_Master_Events> successRateMasterRandomEvents;
//    public List<Event_Effect_Master> Event_effectMasters;
//    [Header("아이템")]
//    public List<Weapon_Master> Weapon_Masters;
//    public List<Option_Master> Item_Options;
//    public List<Armor_Master> Armor_Master;
//    public List<Item_Master> Item_Master;
//    [Header("몬스터")]
//    public List<Mon_Master> Monster_Data;
//    public List<Mon_Effect_Master> Monster_Effect;
//    //메인 스토리 딕셔너리 만듬
//    //private Dictionary<string, List<Story_Master>> MainStoryDictionary;
//    public int num = 0;
//    private void Awake()
//    {
//        //제임스파일 로드
//        LoadAllJson();
//        //제임스 파일 출력
//        PrintAllJsonData();

//        //Debug.Log(MainStoryDictionary);
//    }

//    void LoadAllJson()
//    {
//        storyMasters = LoadJsonFile<Story_Master_Main>(storyMasterFile);
//        scriptMasterMains = LoadJsonFile<Main_Script_Master_Main>(scriptMasterMainFile);
//        successRateMasterMains = LoadJsonFile<Main_SuccessRate_Master_Main>(successRateMasterMainFile);
//        randomEvents = LoadJsonFile<RandomEvents_Master_Event>(randomEventsFile);
//        scriptMasterEvents = LoadJsonFile<Ran_Script_Master_Event>(scriptMasterEventFile);
//        successRateMasterRandomEvents = LoadJsonFile<Ran_SuccessRate_Master_Events>(successRateMasterRandomEventsFile);
//        Story_effectMasters = LoadJsonFile<Story_Effect_Master>(MainQuesteffectMasterFile);
//        Event_effectMasters = LoadJsonFile<Event_Effect_Master>(EventQuesteffectMasterFile);
//        Weapon_Masters = LoadJsonFile<Weapon_Master>(ItemWeaponMasterFile);
//        Item_Options = LoadJsonFile<Option_Master>(ItemOptionMasterFile);
//        Armor_Master = LoadJsonFile<Armor_Master>(ItemArmorMasterFile);
//        Item_Master = LoadJsonFile<Item_Master>(ItemMasterFile);
//        Monster_Data = LoadJsonFile<Mon_Master>(Monster_DataFile);
//        Monster_Effect = LoadJsonFile<Mon_Effect_Master>(Monster_EffectFile);


//        Debug.Log("JSON 파일 로딩 완료");
//        //이러면 딕셔너리 하나 만듬
//        //키로 씬 코드를 넣고 값으로 해당 스토리를 넣는다
//        //MainStoryDictionary = storyMasters.GroupBy(e => e.Scene_Code).ToDictionary(g => g.Key, g => g.ToList());
//    }

//    List<T> LoadJsonFile<T>(string fileName)
//    {

//        Debug.Log(fileName);
//        num++;
//        Debug.Log(num);
//        TextAsset jsonAsset = Resources.Load<TextAsset>("Events/" + fileName);
//        if (jsonAsset == null)
//        {
//            Debug.LogError("파일을 찾을 수 없습니다: Events/" + fileName);
//            return new List<T>();
//        }
//        string jsonContent = jsonAsset.text;
//        List<T> list = JsonConvert.DeserializeObject<List<T>>(jsonContent);
//        //Debug.Log($"파일 불러오기 성공{list}");
//        return list;
//    }
//    public void PrintAllJsonData()
//    {
//        PrintList(storyMasters, "Story Masters");
//        PrintList(scriptMasterMains, "Script Master Mains");
//        PrintList(successRateMasterMains, "Success Rate Master Mains");
//        PrintList(randomEvents, "Random Events");
//        PrintList(scriptMasterEvents, "Script Master Events");
//        PrintList(successRateMasterRandomEvents, "Success Rate Master Random Events");
//        PrintList(Story_effectMasters, "Effect_Master_Custom_Format");
//        PrintList(Event_effectMasters, "Effect_Master_Custom_Format");
//        PrintList(Weapon_Masters, "Weapon_Master");
//        PrintList(Item_Options, "Option_Master");
//    }

//    // 제네릭 메서드를 사용해 각 리스트의 데이터를 순회하며 출력
//    private void PrintList<T>(List<T> list, string listName)
//    {
//        //Debug.Log($"---- {listName} ----");
//        foreach (T item in list)
//        {
//            // Newtonsoft.Json을 사용해 객체를 포맷된 JSON 문자열로 변환 후 출력
//            string jsonStr = JsonConvert.SerializeObject(item, Formatting.Indented);
//            //Debug.Log(jsonStr);
//        }
//    }
//}

using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;


/// <summary>
/// 모든 JSON 파일 자동 로드 및 파싱 (Resources/ExcelJsons 폴더 기준)
/// </summary>
public class JsonManager : MonoBehaviour
{
    // 파일명 → 파싱된 List<Story_Master> 저장
    private Dictionary<string, List<Story_Master_Main>> storyMasterDict = new Dictionary<string, List<Story_Master_Main>>();
    private Dictionary<string, List<Main_Script_Master_Main>> storyMasterScriptDict = new Dictionary<string, List<Main_Script_Master_Main>>();
    private Dictionary<string, List<Main_SuccessRate_Master_Main>> storyMastersuccessRateDict = new Dictionary<string, List<Main_SuccessRate_Master_Main>>();
    private Dictionary<string, List<Story_Effect_Master>> storyMasterEffectDict = new Dictionary<string, List<Story_Effect_Master>>();
    // 파일명 → 파싱된 List<RandomEvent> 저장 (필요 시 추가)

    void Awake()
    {
        LoadAllJsonFiles();
        foreach (var key in storyMasterScriptDict.Keys)
        {
            Debug.Log($"등록된 키: {key}");
        }
    }

    private void LoadAllJsonFiles()
    {
        TextAsset[] jsonFiles = Resources.LoadAll<TextAsset>("Events2");

        foreach (TextAsset jsonFile in jsonFiles)
        {
            if (jsonFile != null)
            {
                string fileName = jsonFile.name;
                string jsonContent = jsonFile.text;

                // 파일명 기준으로 어떤 데이터인지 구분
                if (fileName.Contains("Story_Master_Main"))
                {
                    // ✅ Story_Master로 파싱
                    Wrapper<Story_Master_Main> wrapper = JsonUtility.FromJson<Wrapper<Story_Master_Main>>(WrapJsonArray(jsonContent));
                    if (wrapper != null && wrapper.items != null)
                    {
                        storyMasterDict[fileName] = wrapper.items;
                        Debug.Log($"[JsonManager] {fileName}.json 로드 완료 (Story_Master {wrapper.items.Count}개)");
                    }
                }
                else if (fileName.Contains("Main_Script_Master_Main"))
                {
                    // ✅ jsonContent는 전체 JSON 문자열
                    var jObj = JObject.Parse(jsonContent);

                    // ✅ 배열 부분만 추출
                    string arrayStr = jObj["Main_Script_Master_Main"].ToString();

                    // ✅ 배열을 items로 감싸기
                    string wrappedJson = WrapJsonArray(arrayStr);

                    // ✅ 파싱
                    Wrapper<Main_Script_Master_Main> wrapper = JsonUtility.FromJson<Wrapper<Main_Script_Master_Main>>(wrappedJson);

                    if (wrapper != null && wrapper.items != null)
                    {
                        storyMasterScriptDict[fileName] = wrapper.items;
                        Debug.Log($"[JsonManager] {fileName}.json 로드 완료 (데이터 {wrapper.items.Count}개)");
                    }
                }
                else if (fileName.Contains("Main_SuccessRate_Master_Main"))
                {
                    // ✅ Story_Master로 파싱
                    Wrapper<Main_SuccessRate_Master_Main> wrapper = JsonUtility.FromJson<Wrapper<Main_SuccessRate_Master_Main>>(WrapJsonArray(jsonContent));
                    if (wrapper != null && wrapper.items != null)
                    {
                        storyMastersuccessRateDict[fileName] = wrapper.items;
                        Debug.Log($"[JsonManager] {fileName}.json 로드 완료 (Story_Master {wrapper.items.Count}개)");
                    }
                }
                else if (fileName.Contains("Story_Effect_Master"))
                {
                    // ✅ Story_Master로 파싱
                    Wrapper<Story_Effect_Master> wrapper = JsonUtility.FromJson<Wrapper<Story_Effect_Master>>(WrapJsonArray(jsonContent));
                    if (wrapper != null && wrapper.items != null)
                    {
                        storyMasterEffectDict[fileName] = wrapper.items;
                        Debug.Log($"[JsonManager] {fileName}.json 로드 완료 (Story_Master {wrapper.items.Count}개)");
                    }
                }
                else
                {
                    Debug.LogWarning($"[JsonManager] {fileName}.json 은 인식되지 않는 형식입니다.");
                }
            }

        }

    }

    // JSON 배열을 JsonUtility 파싱용 객체로 감싸주는 함수
    private string WrapJsonArray(string jsonArray)
    {
        return "{\"items\":" + jsonArray + "}";
    }

    // 특정 파일명으로 Story_Master 리스트 가져오기
    public List<Story_Master_Main> GetStoryMainMasters(string fileName)
    {
        if (storyMasterDict.TryGetValue(fileName, out List<Story_Master_Main> list))
        {
            Debug.Log($"호출이 되었습니다!!");
            Debug.Log(list);
            return list;
        }
            
        Debug.LogWarning($"[JsonManager] {fileName} Story_Master 데이터가 없습니다.");
        return null;
    }
    public List<Main_Script_Master_Main> GetStoryMainScriptMasters(string fileName)
    {
        if (storyMasterScriptDict.TryGetValue(fileName, out List<Main_Script_Master_Main> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} Story_Master 데이터가 없습니다.");
        return null;
    }
    public List<Main_SuccessRate_Master_Main> GetStoryMainSuccessRateMasters(string fileName)
    {
        if (storyMastersuccessRateDict.TryGetValue(fileName, out List<Main_SuccessRate_Master_Main> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} Story_Master 데이터가 없습니다.");
        return null;
    }
    public List<Story_Effect_Master> GetStoryMainEffectMasters(string fileName)
    {
        if (storyMasterEffectDict.TryGetValue(fileName, out List<Story_Effect_Master> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} Story_Master 데이터가 없습니다.");
        return null;
    }

    // 전체 로드된 Story_Master 파일명 리스트
    public List<string> GetLoadedStoryFiles() => new List<string>(storyMasterDict.Keys);
}

/// <summary>
/// JsonUtility로 List<T> 파싱 시 필요한 Wrapper 클래스
/// </summary>
[System.Serializable]
public class Wrapper<T>
{
    public List<T> items;
}



