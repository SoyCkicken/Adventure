using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

public class P0DataCompatibilityTests
{
    private static readonly Dictionary<string, int> ExpectedEventCounts = new Dictionary<string, int>
    {
        { "Armor_Master", 10 },
        { "BlackSmith", 22 },
        { "ChoiceCondition", 3 },
        { "ChoiceConditions", 3 },
        { "Event_Effect_Master", 3 },
        { "General", 20 },
        { "Item_Master", 20 },
        { "Main_Script_Master_Main", 93 },
        { "Main_SuccessRate_Master_Main", 2 },
        { "Mon_concentrate", 2 },
        { "Mon_Effect_Master", 1 },
        { "Mon_Master", 2 },
        { "Option_Master", 13 },
        { "Patch_Notes", 3 },
        { "RandomEvents_Master_Event", 46 },
        { "Ran_Script_Master_Event", 50 },
        { "Ran_SuccessRate_Master_Events", 5 },
        { "Story_Effect_Master", 3 },
        { "Story_Master_Main", 93 },
        { "Weapon_Master", 13 }
    };

    [Test]
    public void ResourcesEventsRootKeysMatchFileNames()
    {
        TextAsset[] assets = Resources.LoadAll<TextAsset>("Events");
        Assert.That(assets, Is.Not.Empty);

        var loadedNames = new HashSet<string>(assets.Select(asset => asset.name));
        CollectionAssert.IsSubsetOf(ExpectedEventCounts.Keys, loadedNames);

        foreach (TextAsset asset in assets)
        {
            Assert.That(TryGetRootArrayCount(asset.text, asset.name, out int count, out string error), Is.True, $"{asset.name}: {error}");
            Assert.That(count, Is.GreaterThanOrEqualTo(0), asset.name);
        }
    }

    [TestCase("Story_Master_Main", 93)]
    [TestCase("Main_Script_Master_Main", 93)]
    [TestCase("RandomEvents_Master_Event", 46)]
    [TestCase("Ran_Script_Master_Event", 50)]
    [TestCase("Weapon_Master", 13)]
    [TestCase("Armor_Master", 10)]
    [TestCase("Item_Master", 20)]
    [TestCase("Option_Master", 13)]
    [TestCase("Mon_Master", 2)]
    public void ImportantJsonRowCountsStayStable(string fileKey, int expectedCount)
    {
        TextAsset asset = Resources.Load<TextAsset>($"Events/{fileKey}");
        Assert.That(asset, Is.Not.Null, fileKey);

        Assert.That(TryGetRootArrayCount(asset.text, fileKey, out int count, out string error), Is.True, error);
        Assert.That(count, Is.EqualTo(expectedCount), fileKey);
    }

    [Test]
    public void WeaponMasterPreservesOneHandedCompatibility()
    {
        TextAsset asset = Resources.Load<TextAsset>("Events/Weapon_Master");
        Assert.That(asset, Is.Not.Null);

        object weapons = ParseWeaponMasters(asset.text);
        Assert.That(weapons, Is.Not.Null);
        Assert.That(CountEnumerable(weapons), Is.EqualTo(13));

        object oldSword = FindByField(weapons, "Weapon_ID", "Weapon_001");
        object shortBow = FindByField(weapons, "Weapon_ID", "Weapon_002");
        object testBurnBlade = FindByField(weapons, "Weapon_ID", "Weapon_013");
        Assert.That(GetField<bool>(oldSword, "One_Handed"), Is.True);
        Assert.That(GetField<bool>(shortBow, "One_Handed"), Is.True);
        Assert.That(GetField<bool>(testBurnBlade, "One_Handed"), Is.True);
        Assert.That(GetField<string>(testBurnBlade, "Weapon_Name"), Is.EqualTo("화염 검"));
        Assert.That(GetField<string>(testBurnBlade, "Option_1_ID"), Is.EqualTo("Option_003"));
        Assert.That(GetField<int>(testBurnBlade, "Option_Value1"), Is.EqualTo(5));
    }

    [Test]
    public void ExcelGeneratedOptionDataPreservesKoreanForcedMissEffect()
    {
        TextAsset asset = Resources.Load<TextAsset>("Events/Option_Master");
        Assert.That(asset, Is.Not.Null);

        Assert.That(asset.text, Does.Contain("\"Option_ID\": \"Option_008\""));
        Assert.That(asset.text, Does.Contain("\"Option_Description\": \"적중 시 대상의 다음 공격 1회 실패\""));
        Assert.That(asset.text, Does.Contain("\"Effect_ID\": \"Effect_008\""));
        Assert.That(asset.text, Does.Contain("\"Option_Type\": \"OnHit\""));
        Assert.That(asset.text, Does.Contain("\"Option_ID\": \"Option_009\""));
        Assert.That(asset.text, Does.Contain("\"StatusType\": \"Bleed\""));
        Assert.That(asset.text, Does.Contain("\"Option_ID\": \"Option_013\""));
        Assert.That(asset.text, Does.Contain("\"StatusType\": \"Freeze\""));
    }

    [Test]
    public void RuntimeItemCodeLookupUsesItemIds()
    {
        TextAsset asset = Resources.Load<TextAsset>("Events/Item_Master");
        Assert.That(asset, Is.Not.Null);

        object items = ParseItemMasters(asset.text);
        object item = InvokeStatic("JsonManager", "FindItemDataByCode", items, "Item_001");
        Assert.That(item, Is.Not.Null);
        Assert.That(GetField<string>(item, "Item_ID"), Is.EqualTo("Item_001"));
    }

    [Test]
    public void JsonManagerIndexesItemMastersById()
    {
        ResetRuntimeSingleton("JsonManager", "Instance");
        Component jsonManager = CreateComponent("JsonManager");

        TextAsset weaponAsset = Resources.Load<TextAsset>("Events/Weapon_Master");
        object weapons = ParseWeaponMasters(weaponAsset.text);
        AddToPrivateDictionary(jsonManager, "WeaponMasterDict", "Weapon_Master", weapons);

        Type optionType = GetRuntimeType("Option_Master");
        object optionMaster = System.Activator.CreateInstance(optionType);
        SetField(optionMaster, "Option_ID", "Option_003");
        SetField(optionMaster, "Effect_ID", "Effect_003");
        AddToPrivateDictionary(jsonManager, "Option_MasterDict", "Option_Master", CreateTypedList("Option_Master", optionMaster));

        object weapon = InvokeInstance(jsonManager, "GetWeaponById", "Weapon_013");
        Assert.That(weapon, Is.Not.Null);
        Assert.That(GetField<string>(weapon, "Option_1_ID"), Is.EqualTo("Option_003"));

        object option = InvokeInstance(jsonManager, "GetOptionById", "Option_003");
        Assert.That(option, Is.Not.Null);
        Assert.That(GetField<string>(option, "Effect_ID"), Is.EqualTo("Effect_003"));

        object itemData = InvokeInstance(jsonManager, "GetItemDataFromCode", "Weapon_013");
        Assert.That(itemData, Is.Not.Null);
        Assert.That(GetField<string>(itemData, "Item_ID"), Is.EqualTo("Weapon_013"));
        Assert.That(GetField<string>(itemData, "Option_1_ID"), Is.EqualTo("Option_003"));
        Assert.That(GetField<int>(itemData, "Option_Value1"), Is.EqualTo(5));
    }

    [Test]
    public void StoryNavigatorNormalizesMainScriptChoiceCodes()
    {
        object normalized = InvokeStatic("StoryNodeNavigator", "NormalizeToSceneCode", "MainScript_1_2_3");
        Assert.That(normalized, Is.EqualTo("MainScene_1_2_3"));
    }

    [Test]
    public void StoryNavigatorSkipsTextLabelNodesWithoutRewards()
    {
        object labelNode = CreateStoryNode("MainScene_1_1_1", "MainScript_1_1_1", 1, 1, 1);
        object targetNode = CreateStoryNode("MainScene_1_1_2", "MainScript_1_1_2", 1, 1, 2);
        object stories = CreateTypedList("Story_Master_Main", labelNode, targetNode);
        object scripts = CreateTypedList("Main_Script_Master_Main", CreateScriptMeta("MainScript_1_1_1", "TEXT"));

        object resolved = ResolveChoiceTarget(stories, scripts, "MainScript_1_1_1", "MainScript_1_1_1", out bool shouldAdvanceFromCurrent);

        Assert.That(shouldAdvanceFromCurrent, Is.False);
        Assert.That(GetField<string>(resolved, "Scene_Code"), Is.EqualTo("MainScene_1_1_2"));
    }

    [Test]
    public void StoryNavigatorKeepsRewardNodesEvenWhenTheyLookLikeLabels()
    {
        object rewardNode = CreateStoryNode("MainScene_1_1_1", "MainScript_1_1_1", 1, 1, 1, hasReward: true);
        object targetNode = CreateStoryNode("MainScene_1_1_2", "MainScript_1_1_2", 1, 1, 2);
        object stories = CreateTypedList("Story_Master_Main", rewardNode, targetNode);
        object scripts = CreateTypedList("Main_Script_Master_Main", CreateScriptMeta("MainScript_1_1_1", "TEXT"));

        object resolved = ResolveChoiceTarget(stories, scripts, "MainScript_1_1_1", "MainScript_1_1_1", out bool shouldAdvanceFromCurrent);

        Assert.That(shouldAdvanceFromCurrent, Is.False);
        Assert.That(GetField<string>(resolved, "Scene_Code"), Is.EqualTo("MainScene_1_1_1"));
    }

    [Test]
    public void ChoiceEvaluatorHandlesFormulaBoundaries()
    {
        Component state = CreateComponent("PlayerState");
        SetField(state, "STR", 5);
        SetField(state, "AGI", 4);

        Assert.That(EvaluateFormula(null, state), Is.EqualTo(0f));
        Assert.That(EvaluateFormula("", state), Is.EqualTo(0f));
        Assert.That(EvaluateFormula("STR*10", state), Is.EqualTo(0.5f).Within(0.0001f));
        Assert.That(EvaluateFormula("DEX * 10", state), Is.EqualTo(0.4f).Within(0.0001f));
        Assert.That(EvaluateSuccess(0f), Is.False);
        Assert.That(EvaluateSuccess(1f), Is.True);
    }

    [Test]
    public void ConditionEvaluatorHandlesKnownRequirementTypes()
    {
        Component state = CreateComponent("PlayerState");
        SetField(state, "STR", 5);
        SetField(state, "Experience", 500);

        object passingRequirements = CreateRequirementList(
            CreateRequirement("STATE", "STR", 5),
            CreateRequirement("GOLD", "GOLD", 500));
        Assert.That(EvaluateConditions(passingRequirements, state, out object passReasons), Is.True);
        Assert.That(CountEnumerable(passReasons), Is.EqualTo(0));

        object missingItemRequirement = CreateRequirementList(CreateRequirement("ITEM", "Item_001", 1));
        Assert.That(EvaluateConditions(missingItemRequirement, state, out object failReasons), Is.False);
        Assert.That(CountEnumerable(failReasons), Is.GreaterThan(0));
    }

    [Test]
    public void PeriodicBuffTicksApplyBurnAndHealingAfterInitialEffect()
    {
        Component burnTarget = CreateCharacter("Burn Target", maxHealth: 100, health: 100);
        object burn = CreateBuffData("test_burn", "Option_003", burnTarget, duration: 5f, value: 5);

        InvokeInstance(burnTarget, "AddBuff", burn);
        Assert.That(GetField<int>(burnTarget, "Health"), Is.EqualTo(95), "Burn should apply Value% once immediately.");

        InvokePrivateInstance(burnTarget, "TickActiveBuffs", 1f);
        Assert.That(GetField<int>(burnTarget, "Health"), Is.EqualTo(90), "Burn should apply Value% again on the next tick.");

        Component healTarget = CreateCharacter("Heal Target", maxHealth: 100, health: 80);
        object heal = CreateBuffData("test_heal", "Option_004", healTarget, duration: 5f, value: 5);

        InvokeInstance(healTarget, "AddBuff", heal);
        Assert.That(GetField<int>(healTarget, "Health"), Is.EqualTo(85), "Healing should apply Value% once immediately.");

        InvokePrivateInstance(healTarget, "TickActiveBuffs", 1f);
        Assert.That(GetField<int>(healTarget, "Health"), Is.EqualTo(90), "Healing should apply Value% again on the next tick.");
    }

    [Test]
    public void ForcedMissEffectConsumesOnlyTheNextAttack()
    {
        Component attacker = CreateCharacter("Miss Attacker", maxHealth: 100, health: 100);
        Component target = CreateCharacter("Miss Target", maxHealth: 100, health: 100);
        SetField(attacker, "damage", 10);
        SetField(attacker, "CitChance", 0);

        InvokeInstance(attacker, "AddForcedMiss", 1);

        object missed = InvokeInstance(attacker, "Attack", target);
        Assert.That(GetField<int>(missed, "Item1"), Is.EqualTo(0));
        Assert.That(GetField<bool>(missed, "Item2"), Is.False);
        Assert.That(GetField<int>(target, "Health"), Is.EqualTo(100));

        object hit = InvokeInstance(attacker, "Attack", target);
        Assert.That(GetField<int>(hit, "Item1"), Is.EqualTo(10));
        Assert.That(GetField<int>(target, "Health"), Is.EqualTo(90));
    }

    [Test]
    public void ForcedMissOptionEffectAddsMissChargeToTarget()
    {
        Component attacker = CreateCharacter("Effect User", maxHealth: 100, health: 100);
        Component target = CreateCharacter("Effect Target", maxHealth: 100, health: 100);
        SetField(target, "damage", 10);
        SetField(target, "CitChance", 0);

        object context = System.Activator.CreateInstance(GetRuntimeType("OptionContext"));
        SetField(context, "User", attacker);
        SetField(context, "Target", target);
        SetField(context, "Value", 1);
        SetField(context, "item_ID", "test_item");
        SetField(context, "option_ID", "Option_008");

        object effect = System.Activator.CreateInstance(GetRuntimeType("ForceNextAttackMissEffect"));
        InvokeInstance(effect, "Apply", context);

        Component victim = CreateCharacter("Effect Victim", maxHealth: 100, health: 100);
        object missed = InvokeInstance(target, "Attack", victim);
        Assert.That(GetField<int>(missed, "Item1"), Is.EqualTo(0));
        Assert.That(GetField<int>(victim, "Health"), Is.EqualTo(100));
    }

    [Test]
    public void ExcelConverterUsesKoreanFallbackEncoding()
    {
        Type converterType = GetRuntimeType("ExcelAutoGenerator");
        MethodInfo method = converterType.GetMethod("GetExcelFallbackEncoding", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        Assert.That(method, Is.Not.Null);

        Encoding encoding = (Encoding)method.Invoke(null, null);
        Assert.That(encoding.CodePage == 949 || encoding.CodePage == 65001, Is.True, encoding.EncodingName);
    }

    [Test]
    public void StackingBleedDamagesAttackerOnAttackAndUsesResistance()
    {
        Component attacker = CreateCharacter("Bleed Attacker", maxHealth: 100, health: 100);
        Component target = CreateCharacter("Bleed Target", maxHealth: 100, health: 100);
        SetField(attacker, "damage", 0);
        SetField(attacker, "BleedResist", 50);

        object bleed = CreateStatusBuff("bleed", "Option_009", "Bleed", attacker, stackCount: 1, isDebuff: true);
        SetField(bleed, "BaseChance", 100f);
        SetField(bleed, "ChancePerStack", 0f);
        SetField(bleed, "BaseValue", 2f);
        SetField(bleed, "ValuePerStack", 2f);
        SetField(bleed, "ResistanceType", "BleedResist");

        InvokeInstance(attacker, "AddBuff", bleed);
        InvokeInstance(attacker, "AddBuff", bleed);
        InvokeInstance(attacker, "Attack", target);

        Assert.That(GetField<int>(attacker, "Health"), Is.EqualTo(98), "2-stack bleed should deal 4%, reduced by 50% resistance.");
    }

    [Test]
    public void StackingRegenHealsOwnerOnAttack()
    {
        Component owner = CreateCharacter("Regen Owner", maxHealth: 100, health: 50);
        Component target = CreateCharacter("Regen Target", maxHealth: 100, health: 100);
        SetField(owner, "damage", 0);

        object regen = CreateStatusBuff("regen", "Option_012", "Regen", owner, stackCount: 1, isDebuff: false);
        SetField(regen, "BaseChance", 100f);
        SetField(regen, "ChancePerStack", 0f);
        SetField(regen, "BaseValue", 1f);
        SetField(regen, "ValuePerStack", 1f);

        InvokeInstance(owner, "AddBuff", regen);
        InvokeInstance(owner, "AddBuff", regen);
        InvokeInstance(owner, "Attack", target);

        Assert.That(GetField<int>(owner, "Health"), Is.EqualTo(52), "2-stack regen should heal 2% on attack.");
    }

    [Test]
    public void HolyCanApplyNextAttackMissChance()
    {
        Component owner = CreateCharacter("Holy Owner", maxHealth: 100, health: 100);
        Component target = CreateCharacter("Holy Target", maxHealth: 100, health: 100);
        Component victim = CreateCharacter("Holy Victim", maxHealth: 100, health: 100);
        SetField(owner, "damage", 0);
        SetField(target, "damage", 10);

        object holy = CreateStatusBuff("holy", "Option_011", "Holy", owner, stackCount: 1, isDebuff: false);
        SetField(holy, "BaseChance", 100f);
        SetField(holy, "ChancePerStack", 0f);
        SetField(holy, "BaseValue", 100f);
        SetField(holy, "ValuePerStack", 0f);
        SetField(holy, "MaxRemoveCount", 3);

        InvokeInstance(owner, "AddBuff", holy);
        InvokeInstance(owner, "Attack", target);
        object missed = InvokeInstance(target, "Attack", victim);

        Assert.That(GetField<int>(missed, "Item1"), Is.EqualTo(0));
        Assert.That(GetField<int>(victim, "Health"), Is.EqualTo(100));
    }

    [Test]
    public void FreezeSlowsOwnerAndCanBlockAction()
    {
        Component frozen = CreateCharacter("Frozen", maxHealth: 100, health: 100);
        Component target = CreateCharacter("Freeze Target", maxHealth: 100, health: 100);
        SetField(frozen, "speed", 10f);
        SetField(frozen, "damage", 10);

        object freeze = CreateStatusBuff("freeze", "Option_013", "Freeze", frozen, stackCount: 2, isDebuff: true);
        SetField(freeze, "BaseChance", 100f);
        SetField(freeze, "ChancePerStack", 0f);
        SetField(freeze, "BaseValue", 10f);
        SetField(freeze, "ValuePerStack", 5f);
        SetField(freeze, "StatType", "Speed");

        InvokeInstance(frozen, "AddBuff", freeze);
        Assert.That(GetField<float>(frozen, "speed"), Is.EqualTo(8.5f).Within(0.001f));

        object blocked = InvokeInstance(frozen, "Attack", target);
        Assert.That(GetField<int>(blocked, "Item1"), Is.EqualTo(0));
        Assert.That(GetField<int>(target, "Health"), Is.EqualTo(100));
    }

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject go in Object.FindObjectsOfType<GameObject>())
        {
            if (go != null && go.name.StartsWith("P0 Test"))
            {
                Object.DestroyImmediate(go);
            }
        }

        ResetRuntimeSingleton("JsonManager", "Instance");
    }

    private static Component CreateComponent(string typeName)
    {
        var go = new GameObject($"P0 Test {typeName}");
        return go.AddComponent(GetRuntimeType(typeName));
    }

    private static Component CreateCharacter(string name, int maxHealth, int health)
    {
        Component character = CreateComponent("Character");
        character.gameObject.name = $"P0 Test {name}";
        SetField(character, "charaterName", name);
        SetField(character, "MaxHealth", maxHealth);
        SetField(character, "Health", health);
        return character;
    }

    private static object CreateBuffData(string buffId, string optionId, Component target, float duration, int value = 0)
    {
        Type buffType = GetRuntimeType("BuffData");
        object buff = System.Activator.CreateInstance(buffType);
        SetField(buff, "BuffID", buffId);
        SetField(buff, "OptionID", optionId);
        SetField(buff, "Value", value);
        SetField(buff, "Duration", duration);
        SetField(buff, "Elapsed", 0f);
        SetField(buff, "IsDebuff", optionId == "Option_003");
        SetField(buff, "IsPassive", false);
        SetField(buff, "Target", target);
        SetField(buff, "SourceItemID", "test_item");
        return buff;
    }

    private static object CreateStatusBuff(string buffId, string optionId, string statusType, Component target, int stackCount, bool isDebuff)
    {
        object buff = CreateBuffData(buffId, optionId, target, duration: 0f, value: stackCount);
        SetField(buff, "StatusType", statusType);
        SetField(buff, "ApplyMode", "Stacking");
        SetField(buff, "StackPolicy", "Stack");
        SetField(buff, "StackCount", stackCount);
        SetField(buff, "MaxStack", 99);
        SetField(buff, "TriggerType", "OnAttack");
        SetField(buff, "ValueMode", "PercentMaxHP");
        SetField(buff, "IsDebuff", isDebuff);
        return buff;
    }

    private static object ParseWeaponMasters(string jsonContent)
    {
        MethodInfo method = GetRuntimeType("JsonManager").GetMethod("TryParseWeaponMasters", BindingFlags.Public | BindingFlags.Static);
        object[] args = { jsonContent, null, null };

        bool success = (bool)method.Invoke(null, args);
        Assert.That(success, Is.True, args[2] as string);
        return args[1];
    }

    private static object ParseItemMasters(string jsonContent)
    {
        MethodInfo method = GetRuntimeType("JsonManager").GetMethod("TryParseItemMasters", BindingFlags.Public | BindingFlags.Static);
        object[] args = { jsonContent, null, null };

        bool success = (bool)method.Invoke(null, args);
        Assert.That(success, Is.True, args[2] as string);
        return args[1];
    }

    private static bool TryGetRootArrayCount(string jsonContent, string rootKey, out int count, out string error)
    {
        MethodInfo method = GetRuntimeType("JsonManager").GetMethod("TryGetRootArray", BindingFlags.Public | BindingFlags.Static);
        object[] args = { jsonContent, rootKey, null, null };

        bool success = (bool)method.Invoke(null, args);
        error = args[3] as string;
        count = success ? GetProperty<int>(args[2], "Count") : 0;
        return success;
    }

    private static float EvaluateFormula(string formula, Component state)
    {
        return (float)InvokeStatic("ChoiceEvaluator", "EvaluateFormula", formula, state);
    }

    private static bool EvaluateSuccess(float rate01)
    {
        return (bool)InvokeStatic("ChoiceEvaluator", "EvaluateSuccess", rate01);
    }

    private static bool EvaluateConditions(object requirements, Component state, out object reasons)
    {
        MethodInfo method = GetRuntimeType("ConditionEvaluator").GetMethod("Evaluate", BindingFlags.Public | BindingFlags.Static);
        object[] args = { requirements, state, null, null, null };

        bool result = (bool)method.Invoke(null, args);
        reasons = args[4];
        return result;
    }

    private static object CreateRequirementList(params object[] requirements)
    {
        Type requirementType = GetRuntimeType("ChoiceRequirement");
        IList list = (IList)System.Activator.CreateInstance(typeof(List<>).MakeGenericType(requirementType));
        foreach (object requirement in requirements)
        {
            list.Add(requirement);
        }

        return list;
    }

    private static object CreateRequirement(string id, string code, int value)
    {
        Type requirementType = GetRuntimeType("ChoiceRequirement");
        object requirement = System.Activator.CreateInstance(requirementType);
        SetField(requirement, "ID", id);
        SetField(requirement, "Code", code);
        SetField(requirement, "Value", value);
        return requirement;
    }

    private static object CreateStoryNode(string sceneCode, string scriptText, int chapter, int eventIndex, int scriptIndex, bool hasReward = false)
    {
        Type storyType = GetRuntimeType("Story_Master_Main");
        object story = System.Activator.CreateInstance(storyType);
        SetField(story, "Scene_Code", sceneCode);
        SetField(story, "Script_Text", scriptText);
        SetField(story, "Chapter_Index", chapter);
        SetField(story, "Event_Index", eventIndex);
        SetField(story, "Script_Index", scriptIndex);
        SetField(story, "Main_Effect", hasReward ? CreateMainEffectList() : null);
        return story;
    }

    private static object CreateScriptMeta(string scriptCode, string displayType)
    {
        Type scriptType = GetRuntimeType("Main_Script_Master_Main");
        object script = System.Activator.CreateInstance(scriptType);
        SetField(script, "Script_Code", scriptCode);
        SetField(script, "displayType", displayType);
        return script;
    }

    private static object CreateMainEffectList()
    {
        Type effectType = GetRuntimeType("Main_Effect");
        object effect = System.Activator.CreateInstance(effectType);
        SetField(effect, "ID", "Effect_001");
        SetField(effect, "Value", 1);
        return CreateTypedList("Main_Effect", effect);
    }

    private static object CreateTypedList(string typeName, params object[] items)
    {
        Type itemType = GetRuntimeType(typeName);
        IList list = (IList)System.Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));
        foreach (object item in items)
        {
            list.Add(item);
        }

        return list;
    }

    private static object ResolveChoiceTarget(object stories, object scripts, string sceneCode, string labelScriptCode, out bool shouldAdvanceFromCurrent)
    {
        MethodInfo method = GetRuntimeType("StoryNodeNavigator").GetMethod("ResolveChoiceTarget", BindingFlags.Public | BindingFlags.Static);
        object[] args = { stories, scripts, sceneCode, labelScriptCode, null };

        object result = method.Invoke(null, args);
        shouldAdvanceFromCurrent = (bool)args[4];
        return result;
    }

    private static object InvokeStatic(string typeName, string methodName, params object[] args)
    {
        MethodInfo method = GetRuntimeType(typeName).GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
        return method.Invoke(null, args);
    }

    private static object InvokeInstance(object target, string methodName, params object[] args)
    {
        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        return method.Invoke(target, args);
    }

    private static object InvokePrivateInstance(object target, string methodName, params object[] args)
    {
        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.That(method, Is.Not.Null, methodName);
        return method.Invoke(target, args);
    }

    private static void AddToPrivateDictionary(object target, string fieldName, string key, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        object dictionary = field.GetValue(target);
        dictionary.GetType().GetMethod("Add").Invoke(dictionary, new[] { key, value });
    }

    private static void ResetRuntimeSingleton(string typeName, string propertyName)
    {
        Type type = GetRuntimeType(typeName);
        FieldInfo backingField = type.GetField($"<{propertyName}>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
        backingField?.SetValue(null, null);
    }

    private static Type GetRuntimeType(string typeName)
    {
        string[] candidates = typeName.Contains(".")
            ? new[] { typeName }
            : new[] { typeName, $"MyGame.{typeName}" };

        Type type = System.AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(assembly => candidates.Select(assembly.GetType))
            .FirstOrDefault(candidate => candidate != null);

        if (type == null)
        {
            throw new System.InvalidOperationException($"Runtime type not found: {typeName}");
        }

        return type;
    }

    private static object FindByField(object items, string fieldName, string value)
    {
        foreach (object item in (IEnumerable)items)
        {
            if (GetField<string>(item, fieldName) == value)
            {
                return item;
            }
        }

        throw new System.InvalidOperationException($"Item with {fieldName}={value} was not found.");
    }

    private static int CountEnumerable(object items)
    {
        return ((IEnumerable)items).Cast<object>().Count();
    }

    private static void SetField(object target, string fieldName, object value)
    {
        target.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance).SetValue(target, value);
    }

    private static T GetField<T>(object target, string fieldName)
    {
        return (T)target.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance).GetValue(target);
    }

    private static T GetProperty<T>(object target, string propertyName)
    {
        return (T)target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance).GetValue(target);
    }
}
