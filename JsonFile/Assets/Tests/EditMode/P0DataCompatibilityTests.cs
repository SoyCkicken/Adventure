using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection;
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
        { "Option_Master", 8 },
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
    [TestCase("Option_Master", 8)]
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
        Assert.That(GetField<string>(testBurnBlade, "Option_1_ID"), Is.EqualTo("Option_003"));
        Assert.That(GetField<int>(testBurnBlade, "Option_Value1"), Is.EqualTo(5));
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

    private static void ResetRuntimeSingleton(string typeName, string propertyName)
    {
        Type type = GetRuntimeType(typeName);
        FieldInfo backingField = type.GetField($"<{propertyName}>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
        backingField?.SetValue(null, null);
    }

    private static Type GetRuntimeType(string typeName)
    {
        Type type = System.AppDomain.CurrentDomain
            .GetAssemblies()
            .Select(assembly => assembly.GetType(typeName))
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
