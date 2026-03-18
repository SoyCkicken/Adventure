using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;



/// <summary>
/// 모든 JSON 파일 자동 로드 및 파싱 (Resources/ExcelJsons 폴더 기준)
/// </summary>
public class JsonManager : MonoBehaviour
{
    public bool IsReady { get; private set; }
    public event Action OnReady;
    public static JsonManager Instance { get; private set; }
    private void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 씬 변경 시 유지

        LoadAllJsonFiles();
        IsReady = true;
        OnReady?.Invoke();
    }

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
    //상인 관련
    private Dictionary<string, List<BlackSmith>> BlackSmith_Item_Dict = new Dictionary<string, List<BlackSmith>>();
    private Dictionary<string, List<Gradient>> Gradient_Item_Dict = new Dictionary<string, List<Gradient>>();

    private Dictionary<string, List<MerchantItem>> merchantItemCache = new();
    private Dictionary<string, List<Patch_Notes>> patchNotesDict = new();
    //선택지 선택 시 필요 조건 관련
    private readonly Dictionary<(string scene, int choiceNo), List<ChoiceRequirement>> _choiceReqBySceneChoice
    = new Dictionary<(string, int), List<ChoiceRequirement>>();
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
                else if (fileName.Contains("BlackSmith"))
                {
                    // ✅ jsonContent는 전체 JSON 문자열
                    var jObj = JObject.Parse(jsonContent);

                    // ✅ 배열 부분만 추출
                    string arrayStr = jObj["BlackSmith"].ToString();

                    // ✅ 배열을 items로 감싸기
                    string wrappedJson = WrapJsonArray(arrayStr);

                    // ✅ 파싱
                    Wrapper<BlackSmith> wrapper = JsonUtility.FromJson<Wrapper<BlackSmith>>(wrappedJson);

                    if (wrapper != null && wrapper.items != null)
                    {
                        string cleanFileName = Path.GetFileNameWithoutExtension(fileName);
                        BlackSmith_Item_Dict[cleanFileName] = wrapper.items;
                        Debug.Log($"[JsonManager] {fileName}.json 로드 완료 (데이터 {wrapper.items.Count}개)");
                    }
                }
                else if (fileName.Contains("Gradient"))
                {
                    // ✅ jsonContent는 전체 JSON 문자열
                    var jObj = JObject.Parse(jsonContent);

                    // ✅ 배열 부분만 추출
                    string arrayStr = jObj["Gradient"].ToString();

                    // ✅ 배열을 items로 감싸기
                    string wrappedJson = WrapJsonArray(arrayStr);

                    // ✅ 파싱
                    Wrapper<Gradient> wrapper = JsonUtility.FromJson<Wrapper<Gradient>>(wrappedJson);

                    if (wrapper != null && wrapper.items != null)
                    {
                        string cleanFileName = Path.GetFileNameWithoutExtension(fileName);
                        Gradient_Item_Dict[cleanFileName] = wrapper.items;
                        Debug.Log($"[JsonManager] {fileName}.json 로드 완료 (데이터 {wrapper.items.Count}개)");
                    }
                }
                //패치 노트
                else if (fileName.Contains("Patch_Notes"))
                {
                    var jObj = Newtonsoft.Json.Linq.JObject.Parse(jsonContent);
                    string arrayStr = jObj["Patch_Notes"].ToString();
                    string wrappedJson = WrapJsonArray(arrayStr);

                    Wrapper<Patch_Notes> wrapper = JsonUtility.FromJson<Wrapper<Patch_Notes>>(wrappedJson);
                    if (wrapper != null && wrapper.items != null)
                    {
                        string cleanFileName = Path.GetFileNameWithoutExtension(fileName);
                        patchNotesDict[cleanFileName] = wrapper.items;
                        Debug.Log($"[JsonManager] {fileName}.json 로드 완료 (패치노트 {wrapper.items.Count}개)");
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
    //메인 스토리
    public List<Story_Master_Main> GetStoryMainMasters(string fileName)
    {
        if (storyMasterDict.TryGetValue(fileName, out List<Story_Master_Main> list))
        {
            Debug.Log($"호출이 되었습니다!!");
            Debug.Log(list);
            return list;
        }
            
        Debug.LogWarning($"[JsonManager] {fileName} Story_Master_Main 데이터가 없습니다.");
        return null;
    }
    public List<Main_Script_Master_Main> GetStoryMainScriptMasters(string fileName)
    {
        if (storyMasterScriptDict.TryGetValue(fileName, out List<Main_Script_Master_Main> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} Main_Script_Master_Main 데이터가 없습니다.");
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
        Debug.LogWarning($"[JsonManager] {fileName} Main_SuccessRate_Master_Main 데이터가 없습니다.");
        return null;
    }
    //public List<Main_SuccessRate_Master_Main> GetSuccessRatesRanByScene(string sceneCode)
    //{
    //    if (_rates == null)
    //    {
    //        // TextAsset 불러오는 네 방식에 맞춰서 교체
    //        TextAsset ta = Resources.Load<TextAsset>("RandomSuccessRates");
    //        var file = JsonUtility.FromJson<ChoiceRateFile>(ta.text);
    //        _rates = file?.Entries ?? new List<ChoiceRateEntry>();

    //        // ⛑ 하위 호환: 평평한 필드만 있을 경우 Gate로 이식
    //        foreach (var r in _rates)
    //        {
    //            if (r.Gate == null &&
    //                (!string.IsNullOrEmpty(r.Req_StatName) || r.Req_StatMin > 0 ||
    //                 !string.IsNullOrEmpty(r.Req_ItemID) || r.Req_Gold > 0))
    //            {
    //                r.Gate = new ChoiceGate
    //                {
    //                    Req_StatName = r.Req_StatName,
    //                    Req_StatMin = r.Req_StatMin,
    //                    Req_ItemID = r.Req_ItemID,
    //                    Req_Gold = r.Req_Gold,
    //                };
    //            }
    //        }
    //    }
    //}
    public List<Story_Effect_Master> GetStoryMainEffectMasters(string fileName)
    {
        if (storyMasterEffectDict.TryGetValue(fileName, out List<Story_Effect_Master> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} Story_Effect_Master 데이터가 없습니다.");
        return null;
    }
    //이벤트 스토리
    public List<RandomEvents_Master_Event> GetRandomMainMasters(string fileName)
    {
        if (RandomMasterDict.TryGetValue(fileName, out List<RandomEvents_Master_Event> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} RandomEvents_Master_Event 데이터가 없습니다.");
        return null;
    }
    public List<Ran_Script_Master_Event> GetRandomScriptMasters(string fileName)
    {
        if (RandomMasterScriptDict.TryGetValue(fileName, out List<Ran_Script_Master_Event> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} Ran_Script_Master_Event 데이터가 없습니다.");
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
        Debug.LogWarning($"[JsonManager] {fileName} Ran_SuccessRate_Master_Events 데이터가 없습니다.");
        return null;
    }
    public List<Event_Effect_Master> GetRanomEffectMasters(string fileName)
    {
        if (RandomMasterEffectDict.TryGetValue(fileName, out List<Event_Effect_Master> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} Event_Effect_Master 데이터가 없습니다.");
        return null;
    }
    //아이템 목록
    public List<Weapon_Master> GetWeaponMasters(string fileName)
    {
        if (WeaponMasterDict.TryGetValue(fileName, out var list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} Weapon_Master 데이터가 없습니다.");
        return new List<Weapon_Master>(); // ⬅︎ 빈 리스트
    }

    public List<Armor_Master> GetArmorMasters(string fileName)
    {
        if (ArmorMasterDict.TryGetValue(fileName, out var list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} Armor_Master 데이터가 없습니다.");
        return new List<Armor_Master>(); // ⬅︎ 빈 리스트
    }

    public List<Item_Master> GetItemMasters(string fileName)
    {
        if (ItemMasterDict.TryGetValue(fileName, out List<Item_Master> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} Item_Master 데이터가 없습니다.");
        return null;
    }
    public List<ItemData> GetItemDataList(string fileKey)
    {
        if (!ItemMasterDict.TryGetValue(fileKey, out var itemMasters))
        {
            Debug.LogError($"[JsonManager] Item_Master {fileKey} 데이터 없음");
            return new List<ItemData>();
        }

        List<ItemData> list = new();
        foreach (var m in itemMasters)
        {
            list.Add(new ItemData
            {
                Item_ID = m.Item_ID,
                Item_Name = m.Item_NAME,
                Item_Type = m.ItemType,
                Item_Price = m.Item_Price,
                Description = m.Item_Description,

                Option_1_ID = m.Item_Option1,
                Option_Value1 = m.Option1_Value,
                Option_2_ID = m.Item_Option2,
                Option_Value2 = m.Option2_Value,

                Heal_Value = 0,
                Mental_Heal_Value = 0
            });
        }

        return list;
    }
    public List<Option_Master> GetOptionMasters(string fileName)
    {
        if (Option_MasterDict.TryGetValue(fileName, out List<Option_Master> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} Option_Master 데이터가 없습니다.");
        return null;
    }
    //몬스터
    public List<Mon_Master> GetMonMasters(string fileName)
    {
        if (Mon_MasterDict.TryGetValue(fileName, out List<Mon_Master> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} Mon_Master 데이터가 없습니다.");
        return null;
    }
    public List<Mon_Effect_Master> GetMonEffectMasters(string fileName)
    {
        if (Mon_EffectMasterDict.TryGetValue(fileName, out List<Mon_Effect_Master> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} Mon_Effect_Master 데이터가 없습니다.");
        return null;
    }

    //상인
    public List<BlackSmith> GetBlackSmiths(string fileName)
    {
        Debug.Log(fileName);
        if (BlackSmith_Item_Dict.TryGetValue(fileName, out List<BlackSmith> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} BlackSmith 데이터가 없습니다.");
        return null;
    }
    public List<Gradient> GetGradients(string fileName)
    {
        Debug.Log(fileName);
        if (Gradient_Item_Dict.TryGetValue(fileName, out List<Gradient> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} BlackSmith 데이터가 없습니다.");
        return null;
    }

    public List<MerchantItem> GetMerchantItems(string fileKey)
    {
        // 1. 캐시 확인
        if (merchantItemCache.TryGetValue(fileKey, out var cachedList))
            return cachedList;

        // 2. JSON 파일 로드
        TextAsset jsonFile = Resources.Load<TextAsset>("Events/" + fileKey);
        if (jsonFile == null)
        {
            Debug.LogError($"[JsonManager] 상점 JSON 파일 {fileKey} 로드 실패");
            return new List<MerchantItem>();
        }

        try
        {
            // 3. JSON 파싱 (JObject로 수동 파싱)
            var root = JsonConvert.DeserializeObject<Dictionary<string, JArray>>(jsonFile.text);
            if (!root.TryGetValue(fileKey, out var rawArray))
            {
                Debug.LogError($"[JsonManager] JSON에 {fileKey} 키를 찾을 수 없음");
                return new List<MerchantItem>();
            }

            var convertedList = new List<MerchantItem>();

            foreach (var token in rawArray)
            {
                var obj = token as JObject;
                if (obj == null) continue;

                var item = new MerchantItem
                {
                    Item_ID = obj["Item_ID"]?.ToString(),
                    Item_Type = obj["Item_Type"]?.ToString(),
                    Item_Name = obj["Item_Name"]?.ToString(),
                    Item_Price = (int)(obj["Item_Price"]?.ToObject<float>() ?? 0f) // <-- float → int 변환
                };

                convertedList.Add(item);
            }

            merchantItemCache[fileKey] = convertedList;
            return convertedList;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[JsonManager] JSON 파싱 중 오류 발생: {ex.Message}");
            return new List<MerchantItem>();
        }
    }

    public List<ChoiceRequirement> GetChoiceRequirementsByScene(string sceneCode, int choiceNo)
    {
        if (string.IsNullOrEmpty(sceneCode)) return null;
        return _choiceReqBySceneChoice.TryGetValue((sceneCode, choiceNo), out var list) ? list : null;
    }

    // 전체 로드된 Story_Master 파일명 리스트
    public List<string> GetLoadedStoryFiles() => new List<string>(storyMasterDict.Keys);

    //패치 노트 관련
    public List<Patch_Notes> GetPatchNotes(string fileKey)
    {
        if (patchNotesDict.TryGetValue(fileKey, out var list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileKey} Patch_Notes 데이터가 없습니다.");
        return new List<Patch_Notes>();
    }

    public ItemData GetItemDataFromCode(string code)
    {
        if (string.IsNullOrEmpty(code)) return null;

        if (code.StartsWith("Weapon_"))
        {
            var weapon = GetWeaponMasters("Weapon_Master").FirstOrDefault(w => w.Weapon_ID == code);
            if (weapon != null)
            {
                return new ItemData
                {
                    Item_ID = weapon.Weapon_ID,
                    Item_Name = weapon.Weapon_Name,
                    Item_Type = weapon.ItemType
                };
            }
        }
        else if (code.StartsWith("Armor_"))
        {
            var armor = GetArmorMasters("Armor_Master").FirstOrDefault(a => a.Armor_ID == code);
            if (armor != null)
            {
                return new ItemData
                {
                    Item_ID = armor.Armor_ID,
                    Item_Name = armor.Armor_NAME,
                    Item_Type = armor.ItemType
                };
            }
        }
        else if (code.StartsWith("Item_"))
        {
            var Item = GetItemMasters("Item_Master").FirstOrDefault(i => i.Item_NAME == code);
            if (Item != null)
            {
                return new ItemData
                {
                    Item_ID = Item.Item_ID,
                    Item_Name = Item.Item_NAME,
                    Item_Type = Item.ItemType
                };
            }
        }

        // 기타 타입 확장 가능
        return null;
    }

}

/// <summary>
/// JsonUtility로 List<T> 파싱 시 필요한 Wrapper 클래스
/// </summary>
[System.Serializable]
public class Wrapper<T>
{
    public List<T> items;
}



