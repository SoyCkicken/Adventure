using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public partial class JsonManager
{
    private void LoadAllJsonFiles()
    {
        TextAsset[] jsonFiles = Resources.LoadAll<TextAsset>("Events");

        foreach (TextAsset jsonFile in jsonFiles)
        {
            if (jsonFile == null)
            {
                continue;
            }

            string fileName = jsonFile.name;
            Debug.Log($"[JsonManager] Load attempt: {fileName}");

            if (RuntimeIgnoredEventFiles.Contains(fileName))
            {
                Debug.Log($"[JsonManager] {fileName}.json is skipped because it is not runtime-loaded data.");
                continue;
            }

            try
            {
                if (!TryLoadKnownJsonFile(jsonFile))
                {
                    Debug.LogWarning($"[JsonManager] {fileName}.json has no registered runtime loader.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JsonManager] {fileName}.json parse failed: {ex.Message}");
            }
        }

        RebuildItemLookupIndexes();
    }

    private bool TryLoadKnownJsonFile(TextAsset jsonFile)
    {
        string fileName = jsonFile.name;

        if (fileName.Contains("Story_Master_Main"))
        {
            LoadTable(jsonFile, "Story_Master_Main", storyMasterDict, out List<Story_Master_Main> _);
            return true;
        }

        if (fileName.Contains("Main_Script_Master_Main"))
        {
            LoadTable(jsonFile, "Main_Script_Master_Main", storyMasterScriptDict, out List<Main_Script_Master_Main> _);
            return true;
        }

        if (fileName.Contains("Main_SuccessRate_Master_Main"))
        {
            if (LoadTable(jsonFile, "Main_SuccessRate_Master_Main", storyMastersuccessRateDict, out List<Main_SuccessRate_Master_Main> items))
            {
                IndexByKey(items, _mainSuccessRateByScene, item => item.Scene_Code);
            }
            return true;
        }

        if (fileName.Contains("Story_Effect_Master"))
        {
            LoadTable(jsonFile, "Story_Effect_Master", storyMasterEffectDict, out List<Story_Effect_Master> _);
            return true;
        }

        if (fileName.Contains("RandomEvents_Master_Event"))
        {
            LoadTable(jsonFile, "RandomEvents_Master_Event", RandomMasterDict, out List<RandomEvents_Master_Event> _);
            return true;
        }

        if (fileName.Contains("Ran_Script_Master_Event"))
        {
            LoadTable(jsonFile, "Ran_Script_Master_Event", RandomMasterScriptDict, out List<Ran_Script_Master_Event> _);
            return true;
        }

        if (fileName.Contains("Ran_SuccessRate_Master_Events"))
        {
            if (LoadTable(jsonFile, "Ran_SuccessRate_Master_Events", RandomMasterSuccessRateDict, out List<Ran_SuccessRate_Master_Events> items))
            {
                IndexByKey(items, _RanSuccessRateByScene, item => item.Scene_Code);
            }
            return true;
        }

        if (fileName.Contains("Event_Effect_Master"))
        {
            LoadTable(jsonFile, "Event_Effect_Master", RandomMasterEffectDict, out List<Event_Effect_Master> _);
            return true;
        }

        if (fileName.Contains("Weapon_Master"))
        {
            LoadTable(jsonFile, "Weapon_Master", WeaponMasterDict, out List<Weapon_Master> _, WeaponMasterFieldAliases);
            return true;
        }

        if (fileName.Contains("Armor_Master"))
        {
            LoadTable(jsonFile, "Armor_Master", ArmorMasterDict, out List<Armor_Master> _);
            return true;
        }

        if (fileName.Contains("Item_Master"))
        {
            LoadTable(jsonFile, "Item_Master", ItemMasterDict, out List<Item_Master> _);
            return true;
        }

        if (fileName.Contains("Option_Master"))
        {
            LoadTable(jsonFile, "Option_Master", Option_MasterDict, out List<Option_Master> _);
            return true;
        }

        if (fileName.Contains("OptionEffect_Master"))
        {
            Debug.Log($"[JsonManager] {fileName}.json is authoring data merged into Option_Master and is not loaded directly.");
            return true;
        }

        if (fileName.Contains("Mon_Master"))
        {
            LoadTable(jsonFile, "Mon_Master", Mon_MasterDict, out List<Mon_Master> _);
            return true;
        }

        if (fileName.Contains("Mon_Effect_Master"))
        {
            LoadTable(jsonFile, "Mon_Effect_Master", Mon_EffectMasterDict, out List<Mon_Effect_Master> _);
            return true;
        }

        if (fileName.Contains("BlackSmith"))
        {
            LoadTable(jsonFile, "BlackSmith", BlackSmith_Item_Dict, out List<BlackSmith> _);
            return true;
        }

        if (fileName.Contains("Gradient"))
        {
            LoadTable(jsonFile, "Gradient", Gradient_Item_Dict, out List<Gradient> _);
            return true;
        }

        if (fileName.Contains("Patch_Notes"))
        {
            LoadTable(jsonFile, "Patch_Notes", patchNotesDict, out List<Patch_Notes> _, null, "patch notes");
            return true;
        }

        return false;
    }

    private bool LoadTable<T>(
        TextAsset jsonFile,
        string rootKey,
        Dictionary<string, List<T>> target,
        out List<T> items,
        Dictionary<string, string> fieldAliases = null,
        string logLabel = "rows")
    {
        if (!JsonRuntimeTableParser.TryParseList(jsonFile.text, rootKey, out items, out string error, fieldAliases))
        {
            Debug.LogError($"[JsonManager] {jsonFile.name}.json root '{rootKey}' failed: {error}");
            return false;
        }

        string cleanFileName = Path.GetFileNameWithoutExtension(jsonFile.name);
        target[cleanFileName] = items;
        Debug.Log($"[JsonManager] {jsonFile.name}.json loaded ({items.Count} {logLabel})");
        return true;
    }

    private static void IndexByKey<T>(
        IEnumerable<T> items,
        Dictionary<string, List<T>> index,
        Func<T, string> keySelector)
    {
        foreach (T item in items)
        {
            string key = keySelector(item);
            if (string.IsNullOrEmpty(key))
            {
                continue;
            }

            if (!index.TryGetValue(key, out List<T> list))
            {
                list = new List<T>();
                index[key] = list;
            }

            list.Add(item);
        }
    }

    private void RebuildItemLookupIndexes()
    {
        weaponById.Clear();
        armorById.Clear();
        itemById.Clear();
        optionById.Clear();

        foreach (Weapon_Master weapon in WeaponMasterDict.Values.SelectMany(list => list))
        {
            if (!string.IsNullOrEmpty(weapon?.Weapon_ID))
            {
                weaponById[weapon.Weapon_ID] = weapon;
            }
        }

        foreach (Armor_Master armor in ArmorMasterDict.Values.SelectMany(list => list))
        {
            if (!string.IsNullOrEmpty(armor?.Armor_ID))
            {
                armorById[armor.Armor_ID] = armor;
            }
        }

        foreach (Item_Master item in ItemMasterDict.Values.SelectMany(list => list))
        {
            if (!string.IsNullOrEmpty(item?.Item_ID))
            {
                itemById[item.Item_ID] = item;
            }
        }

        foreach (Option_Master option in Option_MasterDict.Values.SelectMany(list => list))
        {
            if (!string.IsNullOrEmpty(option?.Option_ID))
            {
                optionById[option.Option_ID] = option;
            }
        }
    }
}
