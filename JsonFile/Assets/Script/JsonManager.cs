using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;



/// <summary>
/// 모든 JSON 파일 자동 로드 및 파싱 (Resources/ExcelJsons 폴더 기준)
/// </summary>
public partial class JsonManager : MonoBehaviour
{
    public bool IsReady { get; private set; }
    public event Action OnReady;
    public static JsonManager Instance { get; private set; }

    private static List<T> EmptyList<T>()
    {
        return new List<T>();
    }

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
    private readonly Dictionary<string, Weapon_Master> weaponById = new Dictionary<string, Weapon_Master>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Armor_Master> armorById = new Dictionary<string, Armor_Master>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Item_Master> itemById = new Dictionary<string, Item_Master>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Option_Master> optionById = new Dictionary<string, Option_Master>(StringComparer.OrdinalIgnoreCase);

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

    private static readonly Dictionary<string, string> WeaponMasterFieldAliases = new Dictionary<string, string>
    {
        // Runtime classes cannot expose a C# field named "One-Handed"; keep the JSON unchanged and alias it at load time.
        { "One-Handed", nameof(Weapon_Master.One_Handed) }
    };

    private static readonly HashSet<string> RuntimeIgnoredEventFiles = new HashSet<string>
    {
        // Authoring/support data that is not mapped to runtime classes in the P0 loader.
        "ChoiceCondition",
        "ChoiceConditions",
        "General",
        "Mon_concentrate"
    };

    public static bool TryGetRootArray(string jsonContent, string rootKey, out JArray array, out string error)
    {
        return JsonRuntimeTableParser.TryGetRootArray(jsonContent, rootKey, out array, out error);
    }

    public static bool TryParseWeaponMasters(string jsonContent, out List<Weapon_Master> weapons, out string error)
    {
        return JsonRuntimeTableParser.TryParseList(
            jsonContent,
            "Weapon_Master",
            out weapons,
            out error,
            WeaponMasterFieldAliases);
    }

    public static bool TryParseItemMasters(string jsonContent, out List<Item_Master> items, out string error)
    {
        return JsonRuntimeTableParser.TryParseList(jsonContent, "Item_Master", out items, out error);
    }
    public static ItemData FindItemDataByCode(IEnumerable<Item_Master> itemMasters, string code)
    {
        if (itemMasters == null || string.IsNullOrEmpty(code))
        {
            return null;
        }

        Item_Master item = itemMasters.FirstOrDefault(i => i.Item_ID == code);
        if (item == null)
        {
            return null;
        }

        return ItemDataFactory.FromItem(item);
    }

    public List<Story_Master_Main> GetStoryMainMasters(string fileName)
    {
        if (storyMasterDict.TryGetValue(fileName, out List<Story_Master_Main> list))
        {
            Debug.Log($"호출이 되었습니다!!");
            Debug.Log(list);
            return list;
        }
            
        Debug.LogWarning($"[JsonManager] {fileName} Story_Master_Main 데이터가 없습니다.");
        return EmptyList<Story_Master_Main>();
    }
    public List<Main_Script_Master_Main> GetStoryMainScriptMasters(string fileName)
    {
        if (storyMasterScriptDict.TryGetValue(fileName, out List<Main_Script_Master_Main> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} Main_Script_Master_Main 데이터가 없습니다.");
        return EmptyList<Main_Script_Master_Main>();
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
        return EmptyList<Main_SuccessRate_Master_Main>();
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
        return EmptyList<Story_Effect_Master>();
    }
    //이벤트 스토리
    public List<RandomEvents_Master_Event> GetRandomMainMasters(string fileName)
    {
        if (RandomMasterDict.TryGetValue(fileName, out List<RandomEvents_Master_Event> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} RandomEvents_Master_Event 데이터가 없습니다.");
        return EmptyList<RandomEvents_Master_Event>();
    }
    public List<Ran_Script_Master_Event> GetRandomScriptMasters(string fileName)
    {
        if (RandomMasterScriptDict.TryGetValue(fileName, out List<Ran_Script_Master_Event> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} Ran_Script_Master_Event 데이터가 없습니다.");
        return EmptyList<Ran_Script_Master_Event>();
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
        return EmptyList<Ran_SuccessRate_Master_Events>();
    }
    public List<Event_Effect_Master> GetRanomEffectMasters(string fileName)
    {
        if (RandomMasterEffectDict.TryGetValue(fileName, out List<Event_Effect_Master> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} Event_Effect_Master 데이터가 없습니다.");
        return EmptyList<Event_Effect_Master>();
    }
    //아이템 목록
    public List<Weapon_Master> GetWeaponMasters(string fileName)
    {
        if (WeaponMasterDict.TryGetValue(fileName, out var list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} Weapon_Master 데이터가 없습니다.");
        return EmptyList<Weapon_Master>();
    }

    public Weapon_Master GetWeaponById(string weaponId)
    {
        if (string.IsNullOrEmpty(weaponId)) return null;
        if (weaponById.TryGetValue(weaponId, out var weapon)) return weapon;

        weapon = WeaponMasterDict.Values.SelectMany(list => list)
            .FirstOrDefault(item => string.Equals(item?.Weapon_ID, weaponId, StringComparison.OrdinalIgnoreCase));
        if (weapon != null) weaponById[weapon.Weapon_ID] = weapon;
        return weapon;
    }

    public List<Armor_Master> GetArmorMasters(string fileName)
    {
        if (ArmorMasterDict.TryGetValue(fileName, out var list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} Armor_Master 데이터가 없습니다.");
        return EmptyList<Armor_Master>();
    }

    public Armor_Master GetArmorById(string armorId)
    {
        if (string.IsNullOrEmpty(armorId)) return null;
        if (armorById.TryGetValue(armorId, out var armor)) return armor;

        armor = ArmorMasterDict.Values.SelectMany(list => list)
            .FirstOrDefault(item => string.Equals(item?.Armor_ID, armorId, StringComparison.OrdinalIgnoreCase));
        if (armor != null) armorById[armor.Armor_ID] = armor;
        return armor;
    }

    public List<Item_Master> GetItemMasters(string fileName)
    {
        if (ItemMasterDict.TryGetValue(fileName, out List<Item_Master> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} Item_Master 데이터가 없습니다.");
        return EmptyList<Item_Master>();
    }

    public Item_Master GetItemMasterById(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return null;
        if (itemById.TryGetValue(itemId, out var item)) return item;

        item = ItemMasterDict.Values.SelectMany(list => list)
            .FirstOrDefault(entry => string.Equals(entry?.Item_ID, itemId, StringComparison.OrdinalIgnoreCase));
        if (item != null) itemById[item.Item_ID] = item;
        return item;
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
            list.Add(ItemDataFactory.FromItem(m));
        }

        return list;
    }
    public List<Option_Master> GetOptionMasters(string fileName)
    {
        if (Option_MasterDict.TryGetValue(fileName, out List<Option_Master> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} Option_Master 데이터가 없습니다.");
        return EmptyList<Option_Master>();
    }

    public Option_Master GetOptionById(string optionId)
    {
        if (string.IsNullOrEmpty(optionId) || optionId == "null") return null;
        if (optionById.TryGetValue(optionId, out var option)) return option;

        option = Option_MasterDict.Values.SelectMany(list => list)
            .FirstOrDefault(entry => string.Equals(entry?.Option_ID, optionId, StringComparison.OrdinalIgnoreCase));
        if (option != null) optionById[option.Option_ID] = option;
        return option;
    }
    //몬스터
    public List<Mon_Master> GetMonMasters(string fileName)
    {
        if (Mon_MasterDict.TryGetValue(fileName, out List<Mon_Master> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} Mon_Master 데이터가 없습니다.");
        return EmptyList<Mon_Master>();
    }
    public List<Mon_Effect_Master> GetMonEffectMasters(string fileName)
    {
        if (Mon_EffectMasterDict.TryGetValue(fileName, out List<Mon_Effect_Master> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} Mon_Effect_Master 데이터가 없습니다.");
        return EmptyList<Mon_Effect_Master>();
    }

    //상인
    public List<BlackSmith> GetBlackSmiths(string fileName)
    {
        Debug.Log(fileName);
        if (BlackSmith_Item_Dict.TryGetValue(fileName, out List<BlackSmith> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} BlackSmith 데이터가 없습니다.");
        return EmptyList<BlackSmith>();
    }
    public List<Gradient> GetGradients(string fileName)
    {
        Debug.Log(fileName);
        if (Gradient_Item_Dict.TryGetValue(fileName, out List<Gradient> list))
            return list;
        Debug.LogWarning($"[JsonManager] {fileName} BlackSmith 데이터가 없습니다.");
        return EmptyList<Gradient>();
    }

    public List<MerchantItem> GetMerchantItems(string fileKey)
    {
        if (merchantItemCache.TryGetValue(fileKey, out var cachedList))
            return cachedList;

        if (BlackSmith_Item_Dict.TryGetValue(fileKey, out List<BlackSmith> loadedRows))
        {
            var convertedRows = ConvertBlackSmithRows(loadedRows);
            merchantItemCache[fileKey] = convertedRows;
            return convertedRows;
        }

        TextAsset jsonFile = Resources.Load<TextAsset>("Events/" + fileKey);
        if (jsonFile == null)
        {
            Debug.LogError($"[JsonManager] 상점 JSON 파일 {fileKey} 로드 실패");
            return new List<MerchantItem>();
        }

        if (!JsonRuntimeTableParser.TryParseList(jsonFile.text, fileKey, out List<BlackSmith> parsedRows, out string error))
        {
            Debug.LogError($"[JsonManager] 상점 JSON 파싱 중 오류 발생: {error}");
            return new List<MerchantItem>();
        }

        var convertedList = ConvertBlackSmithRows(parsedRows);
        merchantItemCache[fileKey] = convertedList;
        return convertedList;
    }

    private static List<MerchantItem> ConvertBlackSmithRows(IEnumerable<BlackSmith> rows)
    {
        var convertedList = new List<MerchantItem>();
        if (rows == null)
            return convertedList;

        foreach (BlackSmith row in rows)
        {
            if (row == null)
                continue;

            convertedList.Add(new MerchantItem
            {
                Item_ID = row.Item_ID,
                Item_Type = row.Item_Type,
                Item_Name = row.Item_Name,
                Item_Price = row.Item_Price
            });
        }

        return convertedList;
    }

    public List<ChoiceRequirement> GetChoiceRequirementsByScene(string sceneCode, int choiceNo)
    {
        if (string.IsNullOrEmpty(sceneCode)) return EmptyList<ChoiceRequirement>();
        return _choiceReqBySceneChoice.TryGetValue((sceneCode, choiceNo), out var list) ? list : EmptyList<ChoiceRequirement>();
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
        return ItemDataFactory.FromCode(this, code);
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
