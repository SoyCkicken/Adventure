using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class JsonManager : MonoBehaviour
{
    [Header("JSON File Paths (리소스폴더/이벤트폴더안에 있는 Json파일 불러올 예정)")]
    public string storyMasterFile = "Story_Master_Custom_Format";
    public string scriptMasterMainFile = "Script_Master_Main_Custom_Format.json";
    public string successRateMasterMainFile = "SuccessRate_Master_Main_Custom_Format.json";
    public string randomEventsFile = "RandomEvents_Master_Custom_Format.json";
    public string scriptMasterEventFile = "Script_Master_Event_Custom_Format.json";
    public string successRateMasterRandomEventsFile = "SuccessRate_Master_RandomEvents_Custom_Format.json";
    public string effectMasterFile = "Effect_Master_Custom_Format.json";

    [Header("Loaded Data")]
    public List<Story_Master> storyMasters;
    public List<Script_Master_Main> scriptMasterMains;
    public List<SuccessRate_Master_Main> successRateMasterMains;
    public List<RandomEvent> randomEvents;
    public List<Script_Master_Event> scriptMasterEvents;
    public List<SuccessRate_Master_RandomEvents> successRateMasterRandomEvents;
    public List<Effect_Master> effectMasters;

    public int num = 0;
    private void Awake()
    {
        //제임스파일 로드
        LoadAllJson();
        //제임스 파일 출력
        PrintAllJsonData();
    }

    void LoadAllJson()
    {
        storyMasters = LoadJsonFile<Story_Master>(storyMasterFile);
        scriptMasterMains = LoadJsonFile<Script_Master_Main>(scriptMasterMainFile);
        successRateMasterMains = LoadJsonFile<SuccessRate_Master_Main>(successRateMasterMainFile);
        randomEvents = LoadJsonFile<RandomEvent>(randomEventsFile);
        scriptMasterEvents = LoadJsonFile<Script_Master_Event>(scriptMasterEventFile);
        successRateMasterRandomEvents = LoadJsonFile<SuccessRate_Master_RandomEvents>(successRateMasterRandomEventsFile);
        effectMasters = LoadJsonFile<Effect_Master>(effectMasterFile);

        Debug.Log("JSON 파일 로딩 완료");
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
        Debug.Log($"파일 불러오기 성공{list}");
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
        PrintList(effectMasters, "Effect Masters");
    }

    // 제네릭 메서드를 사용해 각 리스트의 데이터를 순회하며 출력
    private void PrintList<T>(List<T> list, string listName)
    {
        Debug.Log($"---- {listName} ----");
        foreach (T item in list)
        {
            // Newtonsoft.Json을 사용해 객체를 포맷된 JSON 문자열로 변환 후 출력
            string jsonStr = JsonConvert.SerializeObject(item, Formatting.Indented);
            Debug.Log(jsonStr);
        }
    }
}

