using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;


/// <summary>
/// 모든 JSON 파일 자동 로드 및 파싱 (Resources/ExcelJsons 폴더 기준)
/// </summary>
public class JsonManager : MonoBehaviour
{
    // 파일명 → 파싱된 List<Story_Master> 저장
    //메인 스토리 관련해서 추가 된 딕셔너리
    private Dictionary<string, List<Story_Master_Main>> storyMasterDict = new Dictionary<string, List<Story_Master_Main>>();
    private Dictionary<string, List<Main_Script_Master_Main>> storyMasterScriptDict = new Dictionary<string, List<Main_Script_Master_Main>>();
    private Dictionary<string, List<Main_SuccessRate_Master_Main>> storyMastersuccessRateDict = new Dictionary<string, List<Main_SuccessRate_Master_Main>>();
    private Dictionary<string, List<Story_Effect_Master>> storyMasterEffectDict = new Dictionary<string, List<Story_Effect_Master>>();
    // 파일명 → 파싱된 List<RandomEvent> 저장 (필요 시 추가)
    private Dictionary<string, List<RandomEvents_Master_Event>> RandomMasterDict = new Dictionary<string, List<RandomEvents_Master_Event>>();
    private Dictionary<string, List<Ran_Script_Master_Event>> RandomMasterScriptDict = new Dictionary<string, List<Ran_Script_Master_Event>>();
    private Dictionary<string, List<Ran_SuccessRate_Master_Events>> RandomMasterSuccessRateDict = new Dictionary<string, List<Ran_SuccessRate_Master_Events>>();
    private Dictionary<string, List<Event_Effect_Master>> RandomMasterEffectDict = new Dictionary<string, List<Event_Effect_Master>>();
    //아이템 파싱
    private Dictionary<string, List<Weapon_Master>> WeaponMasterDict = new Dictionary<string, List<Weapon_Master>>();
    private Dictionary<string, List<Armor_Master>> ArmorMasterDict = new Dictionary<string, List<Armor_Master>>();
    private Dictionary<string, List<Item_Master>> ItemMasterDict = new Dictionary<string, List<Item_Master>>();
    private Dictionary<string, List<Option_Master>> Option_MasterDict = new Dictionary<string, List<Option_Master>>();

    //적
    private Dictionary<string, List<Mon_Master>> Mon_MasterDict = new Dictionary<string, List<Mon_Master>>();
    private Dictionary<string, List<Mon_Effect_Master>> Mon_EffectMasterDict = new Dictionary<string, List<Mon_Effect_Master>>();
    //선택지 관련
    private Dictionary<string, List<Main_SuccessRate_Master_Main>> _mainSuccessRateByScene = new();
    private Dictionary<string, List<Ran_SuccessRate_Master_Events>> _RanSuccessRateByScene = new();
    void Awake()
    {
        LoadAllJsonFiles();
    }

    private void LoadAllJsonFiles()
    {
        TextAsset[] jsonFiles = Resources.LoadAll<TextAsset>("Events");

        foreach (TextAsset jsonFile in jsonFiles)
        {
            if (jsonFile != null)
            {
                string fileName = jsonFile.name;
                Debug.Log(fileName);
                string jsonContent = jsonFile.text;

                // 파일명 기준으로 어떤 데이터인지 구분
                //메인 스토리
                if (fileName.Contains("Story_Master_Main"))
                {
                    // ✅ jsonContent는 전체 JSON 문자열
                    var jObj = JObject.Parse(jsonContent);

                    // ✅ 배열 부분만 추출
                    string arrayStr = jObj["Story_Master_Main"].ToString();

                    // ✅ 배열을 items로 감싸기
                    string wrappedJson = WrapJsonArray(arrayStr);

                    // ✅ 파싱
                    Wrapper<Story_Master_Main> wrapper = JsonUtility.FromJson<Wrapper<Story_Master_Main>>(wrappedJson);

                    if (wrapper != null && wrapper.items != null)
                    {
                        string cleanFileName = Path.GetFileNameWithoutExtension(fileName);
                        storyMasterDict[cleanFileName] = wrapper.items;
                        Debug.Log($"[JsonManager] {fileName}.json 로드 완료 (데이터 {wrapper.items.Count}개)");
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
                        string cleanFileName = Path.GetFileNameWithoutExtension(fileName);
                        storyMasterScriptDict[cleanFileName] = wrapper.items;
                        Debug.Log($"[JsonManager] {fileName}.json 로드 완료 (데이터 {wrapper.items.Count}개)");
                    }
                }
                else if (fileName.Contains("Main_SuccessRate_Master_Main"))
                {
                    var jObj = JObject.Parse(jsonContent);
                    string arrayStr = jObj["Main_SuccessRate_Master_Main"].ToString();
                    string wrappedJson = WrapJsonArray(arrayStr);

                    Wrapper<Main_SuccessRate_Master_Main> wrapper = JsonUtility.FromJson<Wrapper<Main_SuccessRate_Master_Main>>(wrappedJson);

                    if (wrapper != null && wrapper.items != null)
                    {
                        string cleanFileName = Path.GetFileNameWithoutExtension(fileName);
                        storyMastersuccessRateDict[cleanFileName] = wrapper.items;

                        // ✅ Scene_Code 기준으로 정리
                        foreach (var entry in wrapper.items)
                        {
                            if (!_mainSuccessRateByScene.TryGetValue(entry.Scene_Code, out var list))
                            {
                                list = new List<Main_SuccessRate_Master_Main>();
                                _mainSuccessRateByScene[entry.Scene_Code] = list;
                            }
                            list.Add(entry);
                        }

                        Debug.Log($"[JsonManager] {fileName}.json 로드 완료 (데이터 {wrapper.items.Count}개)");
                    }
                }
                else if (fileName.Contains("Story_Effect_Master"))
                {
                    var jObj = JObject.Parse(jsonContent);

                    // ✅ 배열 부분만 추출
                    string arrayStr = jObj["Story_Effect_Master"].ToString();

                    // ✅ 배열을 items로 감싸기
                    string wrappedJson = WrapJsonArray(arrayStr);

                    // ✅ 파싱
                    Wrapper<Story_Effect_Master> wrapper = JsonUtility.FromJson<Wrapper<Story_Effect_Master>>(wrappedJson);

                    if (wrapper != null && wrapper.items != null)
                    {
                        string cleanFileName = Path.GetFileNameWithoutExtension(fileName);
                        storyMasterEffectDict[cleanFileName] = wrapper.items;
                        Debug.Log($"[JsonManager] {fileName}.json 로드 완료 (데이터 {wrapper.items.Count}개)");
                    }

                }
                //이벤트
                else if (fileName.Contains("RandomEvents_Master_Event"))
                {
                    // ✅ jsonContent는 전체 JSON 문자열
                    var jObj = JObject.Parse(jsonContent);

                    // ✅ 배열 부분만 추출
                    string arrayStr = jObj["RandomEvents_Master_Event"].ToString();

                    // ✅ 배열을 items로 감싸기
                    string wrappedJson = WrapJsonArray(arrayStr);

                    // ✅ 파싱
                    Wrapper<RandomEvents_Master_Event> wrapper = JsonUtility.FromJson<Wrapper<RandomEvents_Master_Event>>(wrappedJson);

                    if (wrapper != null && wrapper.items != null)
                    {
                        string cleanFileName = Path.GetFileNameWithoutExtension(fileName);
                        RandomMasterDict[cleanFileName] = wrapper.items;
                        Debug.Log($"[JsonManager] {fileName}.json 로드 완료 (데이터 {wrapper.items.Count}개)");
                    }
                }
                else if (fileName.Contains("Ran_Script_Master_Event"))
                {
                    // ✅ jsonContent는 전체 JSON 문자열
                    var jObj = JObject.Parse(jsonContent);

                    // ✅ 배열 부분만 추출
                    string arrayStr = jObj["Ran_Script_Master_Event"].ToString();

                    // ✅ 배열을 items로 감싸기
                    string wrappedJson = WrapJsonArray(arrayStr);

                    // ✅ 파싱
                    Wrapper<Ran_Script_Master_Event> wrapper = JsonUtility.FromJson<Wrapper<Ran_Script_Master_Event>>(wrappedJson);

                    if (wrapper != null && wrapper.items != null)
                    {
                        string cleanFileName = Path.GetFileNameWithoutExtension(fileName);
                        RandomMasterScriptDict[cleanFileName] = wrapper.items;
                        Debug.Log($"[JsonManager] {fileName}.json 로드 완료 (데이터 {wrapper.items.Count}개)");
                    }
                }
                else if (fileName.Contains("Ran_SuccessRate_Master_Events"))
                {
                    // ✅ jsonContent는 전체 JSON 문자열
                    var jObj = JObject.Parse(jsonContent);
                    string arrayStr = jObj["Ran_SuccessRate_Master_Events"].ToString();
                    string wrappedJson = WrapJsonArray(arrayStr);

                    Wrapper<Ran_SuccessRate_Master_Events> wrapper = JsonUtility.FromJson<Wrapper<Ran_SuccessRate_Master_Events>>(wrappedJson);

                    if (wrapper != null && wrapper.items != null)
                    {
                        string cleanFileName = Path.GetFileNameWithoutExtension(fileName);
                        RandomMasterSuccessRateDict[cleanFileName] = wrapper.items;

                        // ✅ Scene_Code 기준으로 정리
                        foreach (var entry in wrapper.items)
                        {
                            if (!_RanSuccessRateByScene.TryGetValue(entry.Scene_Code, out var list))
                            {
                                list = new List<Ran_SuccessRate_Master_Events>();
                                _RanSuccessRateByScene[entry.Scene_Code] = list;
                            }
                            list.Add(entry);
                        }

                        Debug.Log($"[JsonManager] {fileName}.json 로드 완료 (데이터 {wrapper.items.Count}개)");
                    }
                }
                else if (fileName.Contains("Event_Effect_Master"))
                {
                    // ✅ jsonContent는 전체 JSON 문자열
                    var jObj = JObject.Parse(jsonContent);

                    // ✅ 배열 부분만 추출
                    string arrayStr = jObj["Event_Effect_Master"].ToString();

                    // ✅ 배열을 items로 감싸기
                    string wrappedJson = WrapJsonArray(arrayStr);

                    // ✅ 파싱
                    Wrapper<Event_Effect_Master> wrapper = JsonUtility.FromJson<Wrapper<Event_Effect_Master>>(wrappedJson);

                    if (wrapper != null && wrapper.items != null)
                    {
                        string cleanFileName = Path.GetFileNameWithoutExtension(fileName);
                        RandomMasterEffectDict[cleanFileName] = wrapper.items;
                        Debug.Log($"[JsonManager] {fileName}.json 로드 완료 (데이터 {wrapper.items.Count}개)");
                    }
                }
                //아이템
                else if (fileName.Contains("Weapon_Master"))
                {
                    // ✅ jsonContent는 전체 JSON 문자열
                    var jObj = JObject.Parse(jsonContent);

                    // ✅ 배열 부분만 추출
                    string arrayStr = jObj["Weapon_Master"].ToString();

                    // ✅ 배열을 items로 감싸기
                    string wrappedJson = WrapJsonArray(arrayStr);

                    // ✅ 파싱
                    Wrapper<Weapon_Master> wrapper = JsonUtility.FromJson<Wrapper<Weapon_Master>>(wrappedJson);

                    if (wrapper != null && wrapper.items != null)
                    {
                        string cleanFileName = Path.GetFileNameWithoutExtension(fileName);
                        WeaponMasterDict[cleanFileName] = wrapper.items;
                        Debug.Log($"[JsonManager] {fileName}.json 로드 완료 (데이터 {wrapper.items.Count}개)");
                    }
                }
                else if (fileName.Contains("Armor_Master"))
                {
                    // ✅ jsonContent는 전체 JSON 문자열
                    var jObj = JObject.Parse(jsonContent);

                    // ✅ 배열 부분만 추출
                    string arrayStr = jObj["Armor_Master"].ToString();

                    // ✅ 배열을 items로 감싸기
                    string wrappedJson = WrapJsonArray(arrayStr);

                    // ✅ 파싱
                    Wrapper<Armor_Master> wrapper = JsonUtility.FromJson<Wrapper<Armor_Master>>(wrappedJson);

                    if (wrapper != null && wrapper.items != null)
                    {
                        string cleanFileName = Path.GetFileNameWithoutExtension(fileName);
                        ArmorMasterDict[cleanFileName] = wrapper.items;
                        Debug.Log($"[JsonManager] {fileName}.json 로드 완료 (데이터 {wrapper.items.Count}개)");
                    }
                }
                else if (fileName.Contains("Item_Master"))
                {
                    // ✅ jsonContent는 전체 JSON 문자열
                    var jObj = JObject.Parse(jsonContent);

                    // ✅ 배열 부분만 추출
                    string arrayStr = jObj["Item_Master"].ToString();

                    // ✅ 배열을 items로 감싸기
                    string wrappedJson = WrapJsonArray(arrayStr);

                    // ✅ 파싱
                    Wrapper<Item_Master> wrapper = JsonUtility.FromJson<Wrapper<Item_Master>>(wrappedJson);

                    if (wrapper != null && wrapper.items != null)
                    {
                        string cleanFileName = Path.GetFileNameWithoutExtension(fileName);
                        ItemMasterDict[cleanFileName] = wrapper.items;
                        Debug.Log($"[JsonManager] {fileName}.json 로드 완료 (데이터 {wrapper.items.Count}개)");
                    }
                }
                else if (fileName.Contains("Option_Master"))
                {
                    // ✅ jsonContent는 전체 JSON 문자열
                    var jObj = JObject.Parse(jsonContent);

                    // ✅ 배열 부분만 추출
                    string arrayStr = jObj["Option_Master"].ToString();

                    // ✅ 배열을 items로 감싸기
                    string wrappedJson = WrapJsonArray(arrayStr);

                    // ✅ 파싱
                    Wrapper<Option_Master> wrapper = JsonUtility.FromJson<Wrapper<Option_Master>>(wrappedJson);

                    if (wrapper != null && wrapper.items != null)
                    {
                        string cleanFileName = Path.GetFileNameWithoutExtension(fileName);
                        Option_MasterDict[cleanFileName] = wrapper.items;
                        Debug.Log($"[JsonManager] {fileName}.json 로드 완료 (데이터 {wrapper.items.Count}개)");
                    }
                }

                else if (fileName.Contains("Mon_Master"))
                {
                    // ✅ jsonContent는 전체 JSON 문자열
                    var jObj = JObject.Parse(jsonContent);

                    // ✅ 배열 부분만 추출
                    string arrayStr = jObj["Mon_Master"].ToString();

                    // ✅ 배열을 items로 감싸기
                    string wrappedJson = WrapJsonArray(arrayStr);

                    // ✅ 파싱
                    Wrapper<Mon_Master> wrapper = JsonUtility.FromJson<Wrapper<Mon_Master>>(wrappedJson);

                    if (wrapper != null && wrapper.items != null)
                    {
                        string cleanFileName = Path.GetFileNameWithoutExtension(fileName);
                        Mon_MasterDict[cleanFileName] = wrapper.items;
                        Debug.Log($"[JsonManager] {fileName}.json 로드 완료 (데이터 {wrapper.items.Count}개)");
                    }
                }
                else if (fileName.Contains("Mon_Effect_Master"))
                {
                    // ✅ jsonContent는 전체 JSON 문자열
                    var jObj = JObject.Parse(jsonContent);

                    // ✅ 배열 부분만 추출
                    string arrayStr = jObj["Mon_Effect_Master"].ToString();

                    // ✅ 배열을 items로 감싸기
                    string wrappedJson = WrapJsonArray(arrayStr);

                    // ✅ 파싱
                    Wrapper<Mon_Effect_Master> wrapper = JsonUtility.FromJson<Wrapper<Mon_Effect_Master>>(wrappedJson);

                    if (wrapper != null && wrapper.items != null)
                    {
                        string cleanFileName = Path.GetFileNameWithoutExtension(fileName);
                        Mon_EffectMasterDict[cleanFileName] = wrapper.items;
                        Debug.Log($"[JsonManager] {fileName}.json 로드 완료 (데이터 {wrapper.items.Count}개)");
                    }
                }
                //플레이어 + 적 (데이터가 적만 있어서 적만 추가를 해놨음)

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
    //메인 스토리
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
    //확률 조회
    public List<Main_SuccessRate_Master_Main> GetSuccessRatesMainByScene(string sceneCode)
    {
        if (_mainSuccessRateByScene.TryGetValue(sceneCode, out var list))
            return list;

        return new List<Main_SuccessRate_Master_Main>();
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
    //이벤트 스토리
    public List<RandomEvents_Master_Event> GetRandomMainMasters(string fileName)
    {
        if (RandomMasterDict.TryGetValue(fileName, out List<RandomEvents_Master_Event> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} Story_Master 데이터가 없습니다.");
        return null;
    }
    public List<Ran_Script_Master_Event> GetRandomScriptMasters(string fileName)
    {
        if (RandomMasterScriptDict.TryGetValue(fileName, out List<Ran_Script_Master_Event> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} Story_Master 데이터가 없습니다.");
        return null;
    }
    //확률 조회
    public List<Ran_SuccessRate_Master_Events> GetSuccessRatesRanByScene(string sceneCode)
    {
        if (_RanSuccessRateByScene.TryGetValue(sceneCode, out var list))
            return list;

        return new List<Ran_SuccessRate_Master_Events>();
    }
    public List<Ran_SuccessRate_Master_Events> GetRandomSuccessRateMasters(string fileName)
    {
        if (RandomMasterSuccessRateDict.TryGetValue(fileName, out List<Ran_SuccessRate_Master_Events> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} Story_Master 데이터가 없습니다.");
        return null;
    }
    public List<Event_Effect_Master> GetRanomEffectMasters(string fileName)
    {
        if (RandomMasterEffectDict.TryGetValue(fileName, out List<Event_Effect_Master> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} Story_Master 데이터가 없습니다.");
        return null;
    }
    //아이템 목록
    public List<Weapon_Master> GetWeaponMasters(string fileName)
    {
        if (WeaponMasterDict.TryGetValue(fileName, out List<Weapon_Master> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} Story_Master 데이터가 없습니다.");
        return null;
    }
    public List<Armor_Master> GetArmorMasters(string fileName)
    {
        if (ArmorMasterDict.TryGetValue(fileName, out List<Armor_Master> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} Story_Master 데이터가 없습니다.");
        return null;
    }
    public List<Item_Master> GetItemMasters(string fileName)
    {
        if (ItemMasterDict.TryGetValue(fileName, out List<Item_Master> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} Story_Master 데이터가 없습니다.");
        return null;
    }
    public List<Option_Master> GetOptionMasters(string fileName)
    {
        if (Option_MasterDict.TryGetValue(fileName, out List<Option_Master> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} Story_Master 데이터가 없습니다.");
        return null;
    }
    //몬스터
    public List<Mon_Master> GetMonMasters(string fileName)
    {
        if (Mon_MasterDict.TryGetValue(fileName, out List<Mon_Master> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} Story_Master 데이터가 없습니다.");
        return null;
    }
    public List<Mon_Effect_Master> GetMonEffectMasters(string fileName)
    {
        if (Mon_EffectMasterDict.TryGetValue(fileName, out List<Mon_Effect_Master> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} Story_Master 데이터가 없습니다.");
        return null;
    }

    //몬스터

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



