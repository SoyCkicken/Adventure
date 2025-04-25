using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;

public class JsonManager : MonoBehaviour
{
    [Header("JSON File Paths (이벤트 리스트에 있는 Json파일)")]
    public string storyMasterFile = "Story_Master_Custom_Format";
    public string scriptMasterMainFile = "Script_Master_Main_Custom_Format.json";
    public string successRateMasterMainFile = "SuccessRate_Master_Main_Custom_Format.json";
    public string randomEventsFile = "RandomEvents_Master_Custom_Format.json";
    public string scriptMasterEventFile = "Script_Master_Event_Custom_Format.json";
    public string successRateMasterRandomEventsFile = "SuccessRate_Master_RandomEvents_Custom_Format.json";
    public string QuesteffectMasterFile = "Quest_Effect_Master_Custom_Format.json";
    [Header("JSON File Paths (아이템 목록에 있는 Json파일)")]
    public string ItemWeaponMasterFile = "Weapon_Master.Json";
    public string ItemOptionMasterFile = "Option_Master.json";
    public string ItemArmorMasterFile = "Armor_Master.Json";
    public string ItemMasterFile = "Item_Master.json";
    [Header("Loaded Data")]
    public List<Story_Master> storyMasters;
    public List<Script_Master_Main> scriptMasterMains;
    public List<SuccessRate_Master_Main> successRateMasterMains;
    public List<RandomEvent> randomEvents;
    public List<Script_Master_Event> scriptMasterEvents;
    public List<SuccessRate_Master_RandomEvents> successRateMasterRandomEvents;
    public List<Effect_Master> effectMasters;
    public List<Weapon_Master> Weapon_Masters;
    public List<Option_Master> Item_Options;
    public List<Armor_Master> Armor_Master;
    public List<Item_Master> Item_Master;
    //메인 스토리 딕셔너리 만듬
    private Dictionary<string, List<Story_Master>> MainStoryDictionary;
    public int num = 0;
    private void Awake()
    {
        //제임스파일 로드
        LoadAllJson();
        //제임스 파일 출력
        PrintAllJsonData();

        //Debug.Log(MainStoryDictionary);
    }

    void LoadAllJson()
    {
        storyMasters = LoadJsonFile<Story_Master>(storyMasterFile);
        scriptMasterMains = LoadJsonFile<Script_Master_Main>(scriptMasterMainFile);
        successRateMasterMains = LoadJsonFile<SuccessRate_Master_Main>(successRateMasterMainFile);
        randomEvents = LoadJsonFile<RandomEvent>(randomEventsFile);
        scriptMasterEvents = LoadJsonFile<Script_Master_Event>(scriptMasterEventFile);
        successRateMasterRandomEvents = LoadJsonFile<SuccessRate_Master_RandomEvents>(successRateMasterRandomEventsFile);
        effectMasters = LoadJsonFile<Effect_Master>(QuesteffectMasterFile);
        Weapon_Masters = LoadJsonFile<Weapon_Master>(ItemWeaponMasterFile);
        Item_Options = LoadJsonFile<Option_Master>(ItemOptionMasterFile);
        Armor_Master = LoadJsonFile<Armor_Master>(ItemArmorMasterFile);
        Item_Master = LoadJsonFile<Item_Master>(ItemMasterFile);

        Debug.Log("JSON 파일 로딩 완료");
        //이러면 딕셔너리 하나 만듬
        //키로 씬 코드를 넣고 값으로 해당 스토리를 넣는다
        MainStoryDictionary = storyMasters.GroupBy(e => e.Scene_Code).ToDictionary(g => g.Key, g => g.ToList());
    }

    List<T> LoadJsonFile<T>(string fileName)
    {
        
        Debug.Log(fileName);
        num++;
        Debug.Log(num);
        TextAsset jsonAsset = Resources.Load<TextAsset>("Events/" + fileName);
        if (jsonAsset == null)
        {
            Debug.LogError("파일을 찾을 수 없습니다: Events/" + fileName);
            return new List<T>();
        }
        string jsonContent = jsonAsset.text;
        List<T> list = JsonConvert.DeserializeObject<List<T>>(jsonContent);
        //Debug.Log($"파일 불러오기 성공{list}");
        return list;
    }
    public void PrintAllJsonData()
    {
        PrintList(storyMasters, "Story Masters");
        PrintList(scriptMasterMains, "Script Master Mains");
        PrintList(successRateMasterMains, "Success Rate Master Mains");
        PrintList(randomEvents, "Random Events");
        PrintList(scriptMasterEvents, "Script Master Events");
        PrintList(successRateMasterRandomEvents, "Success Rate Master Random Events");
        PrintList(effectMasters, "Effect_Master_Custom_Format");
        PrintList(Weapon_Masters, "Weapon_Master");
        PrintList(effectMasters, "Option_Master");
    }

    // 제네릭 메서드를 사용해 각 리스트의 데이터를 순회하며 출력
    private void PrintList<T>(List<T> list, string listName)
    {
        //Debug.Log($"---- {listName} ----");
        foreach (T item in list)
        {
            // Newtonsoft.Json을 사용해 객체를 포맷된 JSON 문자열로 변환 후 출력
            string jsonStr = JsonConvert.SerializeObject(item, Formatting.Indented);
           //Debug.Log(jsonStr);
        }
    }
}

