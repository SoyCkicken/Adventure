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
        LoadAllJson();
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
}
