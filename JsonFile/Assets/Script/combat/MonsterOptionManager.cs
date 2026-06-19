using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using MyGame;
using UnityEngine;

public class MonsterOptionManager : MonoBehaviour
{
    public const string TriggerNoOp = "NoOp";
    public const string TriggerUnknown = "Unknown";
    public const string TriggerBattleStart = "BattleStart";
    public const string TriggerOnHit = "OnHit";

    public static MonsterOptionManager Instance { get; private set; }
    public JsonManager jsonManager;

    private Dictionary<string, IOptionEffect> effects;
    private static readonly Regex MonsterEffectSlotRegex = new Regex(@"^MonPas_Effect(?<index>\d+)$", RegexOptions.Compiled);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        jsonManager = jsonManager ?? JsonManager.Instance ?? FindObjectOfType<JsonManager>();
        InitializeEffects();
    }

    private void InitializeEffects()
    {
        effects = new Dictionary<string, IOptionEffect>
        {
            { "MonEffect_001", new MonsterCorrosionEffect() }
        };
    }

    private void EnsureEffects()
    {
        if (effects == null)
            InitializeEffects();
    }

    public List<Character.MonsterOption> CollectOptions(Mon_Master data)
    {
        return CollectOptionsFromObject(data);
    }

    public List<Character.MonsterOption> CollectOptionsFromObject(object data)
    {
        var options = new List<Character.MonsterOption>();
        if (data == null)
            return options;

        var fields = data.GetType()
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Select(field => new { Field = field, Match = MonsterEffectSlotRegex.Match(field.Name) })
            .Where(item => item.Match.Success)
            .OrderBy(item => int.Parse(item.Match.Groups["index"].Value));

        foreach (var item in fields)
        {
            string optionID = item.Field.GetValue(data) as string;
            if (IsNoOpEffect(optionID))
                continue;

            int slotIndex = int.Parse(item.Match.Groups["index"].Value);
            options.Add(new Character.MonsterOption
            {
                OptionID = optionID,
                Value = ReadEffectValue(data, slotIndex),
                Trigger = ResolveTrigger(optionID),
                SourceID = $"{ReadMonsterID(data)}:{item.Field.Name}"
            });
        }

        return options;
    }

    public void ApplyBattleStartOptions(Character monster, Character target)
    {
        ApplyOptions(monster, target, TriggerBattleStart);
    }

    public void ApplyOnHitOptions(Character monster, Character target)
    {
        ApplyOptions(monster, target, TriggerOnHit);
    }

    public string ResolveTrigger(string optionID)
    {
        EnsureEffects();
        if (IsNoOpEffect(optionID))
            return TriggerNoOp;

        if (IsMonsterEffectID(optionID))
            return effects.ContainsKey(optionID) ? TriggerOnHit : TriggerUnknown;

        if (!IsOptionID(optionID))
            return TriggerUnknown;

        Option_Master option = OptionManager.GetOption(optionID);
        if (option == null)
            return TriggerUnknown;

        if (IsBattleStartOption(option) || IsPassiveOption(option))
            return TriggerBattleStart;
        if (IsOnHitOption(option))
            return TriggerOnHit;

        return TriggerUnknown;
    }

    public bool IsRegisteredMonsterEffect(string optionID)
    {
        EnsureEffects();
        return IsNoOpEffect(optionID) || !IsMonsterEffectID(optionID) || effects.ContainsKey(optionID);
    }

    public string FormatOptionSummary(IEnumerable<Character.MonsterOption> options)
    {
        if (options == null)
            return string.Empty;

        var labels = options
            .Where(option => !IsNoOpEffect(option.OptionID))
            .Select(option =>
            {
                string desc = OptionManager.GetOptionDescription(option.OptionID);
                string label = string.IsNullOrEmpty(desc) ? option.OptionID : desc;
                string trigger = string.IsNullOrEmpty(option.Trigger) ? ResolveTrigger(option.OptionID) : option.Trigger;
                return $"{label}({trigger})";
            })
            .ToList();

        return labels.Count == 0 ? string.Empty : string.Join(", ", labels);
    }

    public void ApplyMonsterOption(string optionID, OptionContext ctx)
    {
        EnsureEffects();
        if (IsNoOpEffect(optionID))
            return;

        if (effects.TryGetValue(optionID, out var effect))
        {
            effect.Apply(ctx);
        }
        else
        {
            Debug.LogWarning($"MonsterOptionManager: unregistered OptionID={optionID}");
        }
    }

    public void ApplyOption(string optionID, OptionContext ctx)
    {
        ApplyMonsterOption(optionID, ctx);
    }

    private void ApplyOptions(Character monster, Character target, string trigger)
    {
        if (monster?.OnEnemyHitOptions == null)
            return;

        foreach (var option in monster.OnEnemyHitOptions)
        {
            ApplyOption(option, monster, target, trigger);
        }
    }

    private void ApplyOption(Character.MonsterOption monsterOption, Character monster, Character target, string trigger)
    {
        string optionID = monsterOption.OptionID;
        if (IsNoOpEffect(optionID))
            return;

        string resolvedTrigger = string.IsNullOrEmpty(monsterOption.Trigger) || monsterOption.Trigger == TriggerUnknown
            ? ResolveTrigger(optionID)
            : monsterOption.Trigger;
        if (resolvedTrigger != trigger)
            return;

        var ctx = new OptionContext
        {
            User = monster,
            Target = target,
            option_ID = optionID,
            Value = GetSafeValue(optionID, monsterOption.Value),
            item_ID = string.IsNullOrEmpty(monsterOption.SourceID) ? optionID : monsterOption.SourceID,
            IsPlayer = false
        };

        if (IsMonsterEffectID(optionID))
        {
            ReportMonsterOption(monsterOption, monster, target, trigger, ctx.Value);
            ApplyMonsterOption(optionID, ctx);
            return;
        }

        if (!IsOptionID(optionID))
        {
            Debug.LogWarning($"[MonsterOptionManager] Unsupported monster option ID={optionID}");
            return;
        }

        Option_Master option = OptionManager.GetOption(optionID);
        if (option == null)
            return;

        if (trigger == TriggerOnHit && IsOnHitOption(option))
        {
            ReportMonsterOption(monsterOption, monster, target, trigger, ctx.Value);
            OptionManager.ApplyOnHitOnly(optionID, ctx);
        }
        else if (trigger == TriggerBattleStart && IsBattleStartOption(option))
        {
            ReportMonsterOption(monsterOption, monster, target, trigger, ctx.Value);
            OptionManager.ApplyBattleStartOnly(optionID, ctx);
        }
        else if (trigger == TriggerBattleStart && IsPassiveOption(option))
        {
            ReportMonsterOption(monsterOption, monster, target, trigger, ctx.Value);
            ApplyMonsterPassiveOption(option, ctx);
        }
    }

    private static void ReportMonsterOption(
        Character.MonsterOption monsterOption,
        Character monster,
        Character target,
        string trigger,
        int value)
    {
        if (monster == null)
            return;

        string optionID = monsterOption.OptionID;
        string desc = OptionManager.GetOptionDescription(optionID);
        string label = string.IsNullOrEmpty(desc) ? optionID : desc;
        string source = string.IsNullOrEmpty(monsterOption.SourceID) ? optionID : monsterOption.SourceID;
        CombatFeedback.Report(
            CombatFeedbackKind.StatusApplied,
            monster,
            target,
            value,
            $"{monster.charaterName} 정예 패시브 발동: {label} ({optionID}, trigger={trigger}, value={value}, source={source})");
    }

    private static void ApplyMonsterPassiveOption(Option_Master option, OptionContext ctx)
    {
        if (ctx.User == null || option == null)
            return;

        ctx.User.AddBuff(new BuffData
        {
            BuffID = $"monster_passive_{ctx.option_ID}",
            OptionID = ctx.option_ID,
            Value = ctx.Value,
            Duration = 0f,
            Elapsed = 0f,
            IsDebuff = false,
            IsPassive = false,
            Target = ctx.User,
            User = ctx.User,
            SourceItemID = ctx.item_ID,
            StatusType = option.StatusType,
            ApplyMode = string.IsNullOrEmpty(option.ApplyMode) ? "Stat" : option.ApplyMode,
            StackPolicy = string.IsNullOrEmpty(option.StackPolicy) ? "Refresh" : option.StackPolicy,
            StackCount = 1,
            MaxStack = option.MaxStack > 0 ? option.MaxStack : 1,
            TriggerType = option.TriggerType,
            ValueMode = option.ValueMode,
            StatType = option.StatType,
            ResistanceType = option.ResistanceType,
            MaxRemoveCount = option.MaxRemoveCount
        });
    }

    private static string ReadMonsterID(object data)
    {
        FieldInfo idField = data.GetType().GetField("Mon_ID", BindingFlags.Public | BindingFlags.Instance);
        return idField?.GetValue(data) as string ?? "monster";
    }

    private static int ReadEffectValue(object data, int slotIndex)
    {
        FieldInfo valueField = data.GetType().GetField($"Effect{slotIndex}_Stat", BindingFlags.Public | BindingFlags.Instance);
        if (valueField == null)
            return 0;

        object raw = valueField.GetValue(data);
        if (raw == null)
            return 0;
        if (raw is int intValue)
            return intValue;
        if (raw is float floatValue)
            return Mathf.RoundToInt(floatValue);
        if (raw is double doubleValue)
            return Mathf.RoundToInt((float)doubleValue);
        if (int.TryParse(raw.ToString(), out int parsed))
            return parsed;

        return 0;
    }

    private static int GetSafeValue(string optionID, int rawValue)
    {
        if (rawValue > 0)
            return rawValue;

        switch (optionID)
        {
            case "Option_003":
            case "Option_008":
            case "Option_009":
            case "Option_010":
            case "Option_013":
                return 1;
            default:
                return 0;
        }
    }

    public static bool IsNoOpEffect(string optionID)
    {
        return string.IsNullOrWhiteSpace(optionID) ||
               optionID == "--" ||
               optionID == "null" ||
               optionID == "MonEffect_000";
    }

    private static bool IsOptionID(string optionID)
    {
        return !string.IsNullOrEmpty(optionID) && optionID.StartsWith("Option_");
    }

    private static bool IsMonsterEffectID(string optionID)
    {
        return !string.IsNullOrEmpty(optionID) && optionID.StartsWith("MonEffect_");
    }

    private static bool IsBattleStartOption(Option_Master option)
    {
        return option.Option_Type == "OnBattleStart" ||
               option.Option_Type == "BattleStart" ||
               option.TriggerType == "OnBattleStart";
    }

    private static bool IsPassiveOption(Option_Master option)
    {
        return option.Option_Type == "Passive" ||
               option.Option_Type == "OnEquip" ||
               option.ApplyMode == "Passive" ||
               option.ApplyMode == "Stat" ||
               option.TriggerType == "OnEquip";
    }

    private static bool IsOnHitOption(Option_Master option)
    {
        return option.Option_Type == "OnHit" ||
               option.TriggerType == "OnHit" ||
               option.TriggerType == "OnAttack" ||
               option.TriggerType == "BeforeAttack";
    }

    private class MonsterCorrosionEffect : IOptionEffect
    {
        public void Apply(OptionContext ctx)
        {
            if (ctx.Target == null || ctx.Value <= 0)
                return;

            ctx.Target.AddBuff(new BuffData
            {
                BuffID = "monster_corrosion",
                OptionID = ctx.option_ID,
                Value = -ctx.Value,
                Duration = 0f,
                Elapsed = 0f,
                IsDebuff = true,
                IsPassive = false,
                Target = ctx.Target,
                User = ctx.User,
                SourceItemID = ctx.item_ID,
                StatusType = "Corrosion",
                ApplyMode = "Stat",
                StackPolicy = "Refresh",
                StackCount = 1,
                MaxStack = 1,
                TriggerType = TriggerOnHit,
                StatType = "Armor"
            });
        }
    }
}
