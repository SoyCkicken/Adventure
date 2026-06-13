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
        { "Armor_Master", 43 },
        { "BlackSmith", 86 },
        { "ChoiceCondition", 3 },
        { "ChoiceConditions", 3 },
        { "Event_Effect_Master", 3 },
        { "General", 20 },
        { "Item_Master", 20 },
        { "Main_Script_Master_Main", 93 },
        { "Main_SuccessRate_Master_Main", 2 },
        { "Mon_concentrate", 2 },
        { "Mon_Effect_Master", 1 },
        { "Mon_Master", 10 },
        { "Option_Master", 18 },
        { "OptionEffect_Master", 18 },
        { "Patch_Notes", 3 },
        { "RandomEvents_Master_Event", 46 },
        { "Ran_Script_Master_Event", 50 },
        { "Ran_SuccessRate_Master_Events", 5 },
        { "Story_Effect_Master", 3 },
        { "Story_Master_Main", 93 },
        { "Weapon_Master", 45 }
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
    [TestCase("Weapon_Master", 45)]
    [TestCase("Armor_Master", 43)]
    [TestCase("Item_Master", 20)]
    [TestCase("Option_Master", 18)]
    [TestCase("OptionEffect_Master", 18)]
    [TestCase("Mon_Master", 10)]
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
        Assert.That(CountEnumerable(weapons), Is.EqualTo(45));

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
        Assert.That(asset.text, Does.Contain("\"Option_ID\": \"Option_017\""));
        Assert.That(asset.text, Does.Contain("\"StatType\": \"FreezeResist\""));
        Assert.That(asset.text, Does.Contain("\"Option_ID\": \"Option_018\""));
        Assert.That(asset.text, Does.Contain("\"Effect_ID\": \"Effect_015\""));
        Assert.That(asset.text, Does.Contain("\"Option_Type\": \"OnBattleStart\""));
        Assert.That(asset.text, Does.Contain("\"StatusType\": \"Berserk\""));
    }

    [Test]
    public void OptionEffectAuthoringSheetIsMergedIntoRuntimeOptionMaster()
    {
        TextAsset optionAsset = Resources.Load<TextAsset>("Events/Option_Master");
        TextAsset effectAsset = Resources.Load<TextAsset>("Events/OptionEffect_Master");
        Assert.That(optionAsset, Is.Not.Null);
        Assert.That(effectAsset, Is.Not.Null);

        Assert.That(TryGetRootArrayCount(optionAsset.text, "Option_Master", out int optionCount, out string optionError), Is.True, optionError);
        Assert.That(TryGetRootArrayCount(effectAsset.text, "OptionEffect_Master", out int effectCount, out string effectError), Is.True, effectError);
        Assert.That(optionCount, Is.EqualTo(effectCount));
        Assert.That(optionAsset.text, Does.Contain("\"Option_ID\": \"Option_014\""));
        Assert.That(optionAsset.text, Does.Contain("\"ApplyMode\": \"Stat\""));
        Assert.That(optionAsset.text, Does.Contain("\"StatType\": \"DebuffDamageResist\""));
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

    [Test]
    public void EquipmentSystemAppliesResistanceStatsFromJsonArmorOptions()
    {
        Component jsonManager = CreateComponent("JsonManager");
        Component playerState = CreateComponent("PlayerState");
        Component player = CreateCharacter("Equipment Resistance Player", maxHealth: 100, health: 100);
        Component optionManager = CreateComponent("OptionManager");
        Component equipmentSystem = CreateComponent("EquipmentSystem");
        PrepareJsonAndOptionManagers(jsonManager, optionManager);
        InvokeStatic("OptionManager", "Initialize", jsonManager);

        SetField(playerState, "STR", 10);
        SetField(playerState, "AGI", 10);
        SetField(playerState, "Health", 10);
        SetField(player, "armor_Name", "Armor_011");
        SetField(player, "weapon_Name", "");
        SetField(equipmentSystem, "jsonManager", jsonManager);
        SetField(equipmentSystem, "playerState", playerState);
        SetField(equipmentSystem, "player", player);

        InvokeInstance(equipmentSystem, "Init");

        Assert.That(GetField<int>(player, "armor"), Is.EqualTo(3));
        Assert.That(GetField<int>(player, "DebuffDamageResist"), Is.EqualTo(20));
        Assert.That(GetField<int>(player, "BleedResist"), Is.EqualTo(30));

        InvokeInstance(player, "RemoveBuffByItem", "Armor_011");
        Assert.That(GetField<int>(player, "DebuffDamageResist"), Is.EqualTo(0));
        Assert.That(GetField<int>(player, "BleedResist"), Is.EqualTo(0));
    }

    [Test]
    public void EquipmentSystemRegistersWeaponOnHitOptionAndRuntimeEffectCanFire()
    {
        Component jsonManager = CreateComponent("JsonManager");
        Component playerState = CreateComponent("PlayerState");
        Component player = CreateCharacter("Equipment Weapon Player", maxHealth: 100, health: 100);
        Component target = CreateCharacter("Equipment Weapon Target", maxHealth: 100, health: 100);
        Component optionManager = CreateComponent("OptionManager");
        Component equipmentSystem = CreateComponent("EquipmentSystem");
        PrepareJsonAndOptionManagers(jsonManager, optionManager);
        InvokeStatic("OptionManager", "Initialize", jsonManager);

        SetField(playerState, "STR", 10);
        SetField(playerState, "AGI", 10);
        SetField(player, "weapon_Name", "Weapon_013");
        SetField(player, "armor_Name", "");
        SetField(equipmentSystem, "jsonManager", jsonManager);
        SetField(equipmentSystem, "playerState", playerState);
        SetField(equipmentSystem, "player", player);

        InvokeInstance(equipmentSystem, "Init");

        IList options = (IList)GetField<object>(player, "OnHitOptions");
        Assert.That(options.Count, Is.EqualTo(1));
        object equippedOption = options[0];
        Assert.That(GetField<string>(equippedOption, "OptionID"), Is.EqualTo("Option_003"));
        Assert.That(GetField<int>(equippedOption, "Value"), Is.EqualTo(5));
        Assert.That(GetField<string>(equippedOption, "item_ID"), Is.EqualTo("Weapon_013"));

        object context = System.Activator.CreateInstance(GetRuntimeType("OptionContext"));
        SetField(context, "User", player);
        SetField(context, "Target", target);
        SetField(context, "Value", GetField<int>(equippedOption, "Value"));
        SetField(context, "item_ID", GetField<string>(equippedOption, "item_ID"));
        SetField(context, "option_ID", GetField<string>(equippedOption, "OptionID"));

        InvokeStatic("OptionManager", "ApplyOnHitOnly", GetField<string>(equippedOption, "OptionID"), context);

        Assert.That(GetField<int>(target, "Health"), Is.EqualTo(95), "Weapon_013 should apply its burn option through the runtime on-hit path.");
    }

    [Test]
    public void EquipmentOptionCanApplyDebuffOnBattleStart()
    {
        Component jsonManager = CreateComponent("JsonManager");
        Component player = CreateCharacter("Battle Start Player", maxHealth: 100, health: 100);
        Component enemy = CreateCharacter("Battle Start Enemy", maxHealth: 100, health: 100);
        Component optionManager = CreateComponent("OptionManager");
        PrepareJsonAndOptionManagers(jsonManager, optionManager);
        InvokeStatic("OptionManager", "Initialize", jsonManager);

        object option = InvokeStatic("OptionManager", "GetOption", "Option_003");
        SetField(option, "Option_Type", "OnBattleStart");

        object registerContext = System.Activator.CreateInstance(GetRuntimeType("OptionContext"));
        SetField(registerContext, "User", player);
        SetField(registerContext, "Value", 5);
        SetField(registerContext, "item_ID", "Weapon_BattleStart");
        SetField(registerContext, "option_ID", "Option_003");

        InvokeStatic("OptionManager", "ApplyOption", "Option_003", registerContext);

        IList options = (IList)GetField<object>(player, "OnBattleStartOptions");
        Assert.That(options.Count, Is.EqualTo(1));
        object equippedOption = options[0];
        Assert.That(GetField<string>(equippedOption, "OptionID"), Is.EqualTo("Option_003"));
        Assert.That(GetField<string>(equippedOption, "item_ID"), Is.EqualTo("Weapon_BattleStart"));
        Assert.That(GetField<int>(enemy, "Health"), Is.EqualTo(100), "Registration should not apply before battle target exists.");

        object applyContext = System.Activator.CreateInstance(GetRuntimeType("OptionContext"));
        SetField(applyContext, "User", player);
        SetField(applyContext, "Target", enemy);
        SetField(applyContext, "Value", GetField<int>(equippedOption, "Value"));
        SetField(applyContext, "item_ID", GetField<string>(equippedOption, "item_ID"));
        SetField(applyContext, "option_ID", GetField<string>(equippedOption, "OptionID"));

        InvokeStatic("OptionManager", "ApplyBattleStartOnly", "Option_003", applyContext);

        Assert.That(GetField<int>(enemy, "Health"), Is.EqualTo(95), "OnBattleStart equipment option should apply to the enemy when combat starts.");
    }

    [Test]
    public void BerserkBattleStartBuffIsNonStackingAndChangesCombatStats()
    {
        Component jsonManager = CreateComponent("JsonManager");
        Component playerState = CreateComponent("PlayerState");
        Component player = CreateCharacter("Berserk Player", maxHealth: 100, health: 100);
        Component target = CreateCharacter("Berserk Target", maxHealth: 100, health: 100);
        Component optionManager = CreateComponent("OptionManager");
        PrepareJsonAndOptionManagers(jsonManager, optionManager);
        InvokeStatic("OptionManager", "Initialize", jsonManager);

        SetField(player, "speed", 1f);
        SetField(player, "armor", 3);
        SetField(player, "damage", 0);
        SetField(player, "CitChance", 0);

        object option = InvokeStatic("OptionManager", "GetOption", "Option_018");
        Assert.That(option, Is.Not.Null);
        Assert.That(GetField<string>(option, "Option_Type"), Is.EqualTo("OnBattleStart"));
        Assert.That(GetField<string>(option, "Effect_ID"), Is.EqualTo("Effect_015"));

        object context = System.Activator.CreateInstance(GetRuntimeType("OptionContext"));
        SetField(context, "playerState", playerState);
        SetField(context, "User", player);
        SetField(context, "Target", target);
        SetField(context, "Value", 50);
        SetField(context, "item_ID", "Weapon_Berserk");
        SetField(context, "option_ID", "Option_018");

        InvokeStatic("OptionManager", "ApplyBattleStartOnly", "Option_018", context);
        InvokeStatic("OptionManager", "ApplyBattleStartOnly", "Option_018", context);

        Assert.That(GetField<float>(player, "speed"), Is.EqualTo(1.5f).Within(0.001f), "Berserk speed bonus should not stack.");
        Assert.That(GetField<int>(player, "armor"), Is.EqualTo(-2), "Berserk armor penalty should not stack.");

        InvokeInstance(player, "Attack", target);
        Assert.That(GetField<int>(player, "Health"), Is.EqualTo(99), "Berserk should deal 1% max HP self-damage on attack.");

        object taken = InvokeInstance(player, "TakeDamage", 10);
        Assert.That((int)taken, Is.EqualTo(15), "Player berserk damage taken increase should be 30% after armor calculation.");
        Assert.That(GetField<int>(player, "Health"), Is.EqualTo(84));
    }

    [Test]
    public void DamageTakenIncreaseBuffsAreSummedAfterArmorReduction()
    {
        Component target = CreateCharacter("Damage Taken Target", maxHealth: 100, health: 100);
        SetField(target, "armor", 2);

        object firstDebuff = CreateBuffData("damage_taken_1", "Option_018", target, duration: 0f, value: 0);
        SetField(firstDebuff, "StatusType", "DamageTakenA");
        SetField(firstDebuff, "SourceItemID", "source_a");
        SetField(firstDebuff, "DamageTakenIncreasePercent", 25);

        object secondDebuff = CreateBuffData("damage_taken_2", "Option_018", target, duration: 0f, value: 0);
        SetField(secondDebuff, "StatusType", "DamageTakenB");
        SetField(secondDebuff, "SourceItemID", "source_b");
        SetField(secondDebuff, "DamageTakenIncreasePercent", 30);

        InvokeInstance(target, "AddBuff", firstDebuff);
        InvokeInstance(target, "AddBuff", secondDebuff);

        object taken = InvokeInstance(target, "TakeDamage", 10);

        Assert.That((int)taken, Is.EqualTo(12), "Damage should be floor((10 - 2) * 1.55).");
        Assert.That(GetField<int>(target, "Health"), Is.EqualTo(88));
    }

    [Test]
    public void MonsterOptionCollectorSupportsNumberedSlotsAndSkipsNoOps()
    {
        Component jsonManager = CreateComponent("JsonManager");
        Component optionManager = CreateComponent("OptionManager");
        PrepareJsonAndOptionManagers(jsonManager, optionManager);
        InvokeStatic("OptionManager", "Initialize", jsonManager);
        Component monsterOptionManager = CreateComponent("MonsterOptionManager");

        var data = new MonsterMasterForTest
        {
            Mon_ID = "monster_boss_test",
            MonPas_Effect1 = "--",
            MonPas_Effect2 = "Option_009",
            Effect2_Stat = 0,
            MonPas_Effect3 = "Option_013",
            Effect3_Stat = 2
        };

        object options = InvokeInstance(monsterOptionManager, "CollectOptionsFromObject", data);
        List<object> optionList = ((IEnumerable)options).Cast<object>().ToList();

        Assert.That(optionList.Count, Is.EqualTo(2));
        Assert.That(GetField<string>(optionList[0], "OptionID"), Is.EqualTo("Option_009"));
        Assert.That(GetField<int>(optionList[0], "Value"), Is.EqualTo(0), "Unbalanced values should remain raw 0 until execution.");
        Assert.That(GetField<string>(optionList[0], "Trigger"), Is.EqualTo("OnHit"));
        Assert.That(GetField<string>(optionList[1], "OptionID"), Is.EqualTo("Option_013"));
        Assert.That(GetField<int>(optionList[1], "Value"), Is.EqualTo(2));
    }

    [Test]
    public void MonsterOnHitOptionUsesOptionManagerWithSafeDefaultValue()
    {
        Component jsonManager = CreateComponent("JsonManager");
        Component optionManager = CreateComponent("OptionManager");
        PrepareJsonAndOptionManagers(jsonManager, optionManager);
        InvokeStatic("OptionManager", "Initialize", jsonManager);
        Component monsterOptionManager = CreateComponent("MonsterOptionManager");
        Component monster = CreateCharacter("Monster Option User", maxHealth: 100, health: 100);
        Component target = CreateCharacter("Monster Option Target", maxHealth: 100, health: 100);
        Component victim = CreateCharacter("Monster Option Victim", maxHealth: 100, health: 100);

        SetField(target, "damage", 10);
        SetField(target, "CitChance", 0);
        AddMonsterOption(monster, "Option_008", 0, "OnHit", "monster_test:MonPas_Effect1");

        InvokeInstance(monsterOptionManager, "ApplyOnHitOptions", monster, target);
        object missed = InvokeInstance(target, "Attack", victim);

        Assert.That(GetField<int>(missed, "Item1"), Is.EqualTo(0), "Empty monster value should safely apply one forced-miss charge.");
        Assert.That(GetField<int>(victim, "Health"), Is.EqualTo(100));
    }

    [Test]
    public void MonsterBattleStartPassiveIsTemporaryAndRemovedAfterBattle()
    {
        Component jsonManager = CreateComponent("JsonManager");
        Component optionManager = CreateComponent("OptionManager");
        PrepareJsonAndOptionManagers(jsonManager, optionManager);
        InvokeStatic("OptionManager", "Initialize", jsonManager);
        Component monsterOptionManager = CreateComponent("MonsterOptionManager");
        Component monster = CreateCharacter("Monster Passive User", maxHealth: 100, health: 100);
        Component target = CreateCharacter("Monster Passive Target", maxHealth: 100, health: 100);

        AddMonsterOption(monster, "Option_014", 5, "BattleStart", "monster_test:MonPas_Effect1");

        InvokeInstance(monsterOptionManager, "ApplyBattleStartOptions", monster, target);
        Assert.That(GetField<int>(monster, "DebuffDamageResist"), Is.EqualTo(5));

        InvokeInstance(monster, "RemoveTemporaryBuffs");
        Assert.That(GetField<int>(monster, "DebuffDamageResist"), Is.EqualTo(0));
    }

    [Test]
    public void MonsterMasterReferencesKnownOptionOrMonsterEffectIds()
    {
        Component jsonManager = CreateComponent("JsonManager");
        Component optionManager = CreateComponent("OptionManager");
        PrepareJsonAndOptionManagers(jsonManager, optionManager);
        InvokeStatic("OptionManager", "Initialize", jsonManager);
        Component monsterOptionManager = CreateComponent("MonsterOptionManager");

        TextAsset monsterAsset = Resources.Load<TextAsset>("Events/Mon_Master");
        Assert.That(monsterAsset, Is.Not.Null);
        MonMasterJsonRoot root = JsonUtility.FromJson<MonMasterJsonRoot>(monsterAsset.text);
        Assert.That(root?.Mon_Master, Is.Not.Null);

        foreach (MonsterMasterForTest monster in root.Mon_Master)
        {
            object options = InvokeInstance(monsterOptionManager, "CollectOptionsFromObject", monster);
            foreach (object option in (IEnumerable)options)
            {
                string optionID = GetField<string>(option, "OptionID");
                if (optionID.StartsWith("Option_"))
                {
                    Assert.That(InvokeStatic("OptionManager", "GetOption", optionID), Is.Not.Null, $"{monster.Mon_ID}:{optionID}");
                }
                else if (optionID.StartsWith("MonEffect_"))
                {
                    Assert.That((bool)InvokeInstance(monsterOptionManager, "IsRegisteredMonsterEffect", optionID), Is.True, $"{monster.Mon_ID}:{optionID}");
                }
            }
        }
    }

    [Test]
    public void BerserkUsesExplicitPlayerFlagWhenPlayerStateIsMissing()
    {
        Component jsonManager = CreateComponent("JsonManager");
        Component player = CreateCharacter("Berserk Flag Player", maxHealth: 100, health: 100);
        Component target = CreateCharacter("Berserk Flag Target", maxHealth: 100, health: 100);
        Component optionManager = CreateComponent("OptionManager");
        PrepareJsonAndOptionManagers(jsonManager, optionManager);
        InvokeStatic("OptionManager", "Initialize", jsonManager);

        SetField(player, "speed", 1f);
        SetField(player, "armor", 3);

        object context = System.Activator.CreateInstance(GetRuntimeType("OptionContext"));
        SetField(context, "IsPlayer", true);
        SetField(context, "User", player);
        SetField(context, "Target", target);
        SetField(context, "Value", 50);
        SetField(context, "item_ID", "Weapon_Berserk");
        SetField(context, "option_ID", "Option_018");

        InvokeStatic("OptionManager", "ApplyBattleStartOnly", "Option_018", context);

        object taken = InvokeInstance(player, "TakeDamage", 10);
        Assert.That((int)taken, Is.EqualTo(15), "Explicit player flag should use the 30% player damage taken penalty.");
    }

    [Test]
    public void BattleStartOptionRegistrationUpdatesSameItemOptionInsteadOfDuplicating()
    {
        Component jsonManager = CreateComponent("JsonManager");
        Component player = CreateCharacter("Battle Start Registration Player", maxHealth: 100, health: 100);
        Component optionManager = CreateComponent("OptionManager");
        PrepareJsonAndOptionManagers(jsonManager, optionManager);
        InvokeStatic("OptionManager", "Initialize", jsonManager);

        object context = System.Activator.CreateInstance(GetRuntimeType("OptionContext"));
        SetField(context, "User", player);
        SetField(context, "Value", 50);
        SetField(context, "item_ID", "Weapon_Berserk");
        SetField(context, "option_ID", "Option_018");

        InvokeStatic("OptionManager", "ApplyOption", "Option_018", context);
        SetField(context, "Value", 60);
        InvokeStatic("OptionManager", "ApplyOption", "Option_018", context);

        IList options = (IList)GetField<object>(player, "OnBattleStartOptions");
        Assert.That(options.Count, Is.EqualTo(1));
        Assert.That(GetField<int>(options[0], "Value"), Is.EqualTo(60));
    }

    [Test]
    public void StopBattleRemovesTemporaryCombatBuffs()
    {
        Component combat = CreateComponent("CombatTest");
        Component player = CreateCharacter("Stop Battle Player", maxHealth: 100, health: 100);
        Component enemy = CreateCharacter("Stop Battle Enemy", maxHealth: 100, health: 100);
        GameObject normalBattle = new GameObject("P0 Test Normal Battle");
        SetField(combat, "player", player);
        SetField(combat, "enemy", enemy);
        SetField(combat, "NormalBattle", normalBattle);

        SetField(player, "speed", 1f);
        SetField(player, "armor", 3);

        object speedBuff = CreateBuffData("berserk_buff", "Option_018", player, duration: 0f, value: 50);
        SetField(speedBuff, "StatusType", "BerserkBuff");
        SetField(speedBuff, "ApplyMode", "Stat");
        SetField(speedBuff, "StatType", "AttackSpeed");

        object armorDebuff = CreateBuffData("berserk_debuff", "Option_018", player, duration: 0f, value: -5);
        SetField(armorDebuff, "StatusType", "BerserkDebuff");
        SetField(armorDebuff, "ApplyMode", "Stat");
        SetField(armorDebuff, "StatType", "Armor");

        InvokeInstance(player, "AddBuff", speedBuff);
        InvokeInstance(player, "AddBuff", armorDebuff);
        Assert.That(GetField<float>(player, "speed"), Is.EqualTo(1.5f).Within(0.001f));
        Assert.That(GetField<int>(player, "armor"), Is.EqualTo(-2));

        InvokeInstance(combat, "StopBattle");

        Assert.That(GetField<float>(player, "speed"), Is.EqualTo(1f).Within(0.001f));
        Assert.That(GetField<int>(player, "armor"), Is.EqualTo(3));
        Assert.That(normalBattle.activeSelf, Is.False);
    }

    [Test]
    public void MissingJsonManagerListTablesReturnEmptyCollections()
    {
        Component jsonManager = CreateComponent("JsonManager");
        const string missingKey = "Missing_Table_For_Test";

        string[] listGetterNames =
        {
            "GetStoryMainMasters",
            "GetStoryMainScriptMasters",
            "GetStoryMainSuccessRateMasters",
            "GetStoryMainEffectMasters",
            "GetRandomMainMasters",
            "GetRandomScriptMasters",
            "GetRandomSuccessRateMasters",
            "GetRanomEffectMasters",
            "GetWeaponMasters",
            "GetArmorMasters",
            "GetItemMasters",
            "GetOptionMasters",
            "GetMonMasters",
            "GetMonEffectMasters",
            "GetBlackSmiths",
            "GetGradients",
            "GetPatchNotes"
        };

        foreach (string methodName in listGetterNames)
        {
            object result = InvokeInstance(jsonManager, methodName, missingKey);
            Assert.That(result, Is.Not.Null, methodName);
            Assert.That(CountEnumerable(result), Is.EqualTo(0), methodName);
        }

        object choiceRequirements = InvokeInstance(jsonManager, "GetChoiceRequirementsByScene", "", 1);
        Assert.That(choiceRequirements, Is.Not.Null);
        Assert.That(CountEnumerable(choiceRequirements), Is.EqualTo(0));
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
        ResetRuntimeSingleton("OptionManager", "Instance");
        ResetRuntimeSingleton("MonsterOptionManager", "Instance");
        ResetRuntimeSingleton("PlayerState", "Instance");
    }

    [Serializable]
    private class MonsterMasterForTest
    {
        public string Mon_ID;
        public string Mon_Name;
        public int Mon_HP;
        public int Mon_ATK;
        public int Mon_Def;
        public int Mon_Speed;
        public string MonPas_Effect1;
        public int Effect1_Stat;
        public string MonPas_Effect2;
        public int Effect2_Stat;
        public string MonPas_Effect3;
        public int Effect3_Stat;
        public int Get_EXP;
        public int Get_Soul;
    }

    [Serializable]
    private class MonMasterJsonRoot
    {
        public List<MonsterMasterForTest> Mon_Master;
    }

    private static Component CreateComponent(string typeName)
    {
        var go = new GameObject($"P0 Test {typeName}");
        return go.AddComponent(GetRuntimeType(typeName));
    }

    private static void PrepareJsonAndOptionManagers(Component jsonManager, Component optionManager)
    {
        SetRuntimeSingleton("JsonManager", "Instance", jsonManager);
        InvokePrivateInstance(jsonManager, "LoadAllJsonFiles");
        SetAutoProperty(jsonManager, "IsReady", true);

        SetRuntimeSingleton("OptionManager", "Instance", optionManager);
        SetField(optionManager, "jsonManager", jsonManager);
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

    private static void AddMonsterOption(Component monster, string optionId, int value, string trigger, string sourceId)
    {
        Type monsterOptionType = GetRuntimeType("Character").GetNestedType("MonsterOption");
        object option = System.Activator.CreateInstance(monsterOptionType);
        SetField(option, "OptionID", optionId);
        SetField(option, "Value", value);
        SetField(option, "Trigger", trigger);
        SetField(option, "SourceID", sourceId);

        IList options = (IList)GetField<object>(monster, "OnEnemyHitOptions");
        options.Add(option);
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

    private static void SetRuntimeSingleton(string typeName, string propertyName, object value)
    {
        Type type = GetRuntimeType(typeName);
        FieldInfo backingField = type.GetField($"<{propertyName}>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.That(backingField, Is.Not.Null, $"{typeName}.{propertyName}");
        backingField.SetValue(null, value);
    }

    private static void SetAutoProperty(object target, string propertyName, object value)
    {
        FieldInfo backingField = target.GetType().GetField($"<{propertyName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(backingField, Is.Not.Null, $"{target.GetType().Name}.{propertyName}");
        backingField.SetValue(target, value);
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
