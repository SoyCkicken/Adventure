//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using UnityEngine.TextCore.Text;
//using MyGame;
//using Character = MyGame.Character;
//using static UnityEngine.GraphicsBuffer;

//// 1) 옵션 효과 인터페이스
//public interface IOptionEffect
//{
//    /// <summary>
//    /// 옵션 효과를 적용
//    /// </summary>
//    /// <param name="user">효과를 발동한 주체</param>
//    /// <param name="target">효과를 받는 대상</param>
//    /// <param name="value">옵션의 수치값</param>

//    void Apply(OptionContext ctx);
//}

//// 2) 효과 구현 예시: 출혈 효과
//public class AddDamage : IOptionEffect
//{
//    public void Apply(OptionContext ctx)
//    {
//        int damage = Mathf.FloorToInt(ctx.Value);
//        ctx.Target.Health -= damage;
//        Debug.Log(damage);
//        Debug.Log(ctx.Target.Health);
//    }
//}

//public class AddFireDamage : IOptionEffect
//{
//    public void Apply(OptionContext ctx)
//    {
//        int damage = Mathf.FloorToInt(ctx.Value);
//        ctx.Target.Health -= damage;
//        Debug.Log(damage);
//        Debug.Log($"{ctx.Target.Health}");
//    }
//}

//public class CriticalBuff : IOptionEffect
//{
//    public void Apply(OptionContext ctx)
//    {
//        // 키를 “장비ID_옵션ID” 로 합쳐서
//        var buffKey = new BuffData
//        {
//            BuffID = $"{ctx.item_ID}_{ctx.option_ID}",
//            User = ctx.User,
//            OptionID = ctx.option_ID,
//            Value = ctx.Value,
//            SourceItemID = ctx.item_ID,
//            IsPassive = true
//        };
//        ctx.User.AddBuff(buffKey); 
//       //ctx.User.buffUI?.SetBuffs(ctx.User.activeBuffs.Values.ToList(), ctx.User);
//    }
//}

//public class SpeedBuff : IOptionEffect
//{
//    public void Apply(OptionContext ctx)
//    {
//        var buffKey = new BuffData
//        {
//            BuffID = $"{ctx.item_ID}_{ctx.option_ID}",
//            User = ctx.User,
//            OptionID = ctx.option_ID,
//            Value = ctx.Value,
//            SourceItemID = ctx.item_ID,
//            IsPassive = true
//        };
//        ctx.User.AddBuff(buffKey);
//        //ctx.User.buffUI?.SetBuffs(ctx.User.activeBuffs.Values.ToList(), ctx.User);
//    }
//}

//public class HealtingBuff : IOptionEffect
//{
//    public void Apply(OptionContext ctx)
//    {
//        var buff = new BuffData
//        {
//            BuffID = $"{ctx.item_ID}_{ctx.option_ID}_onhit",
//            OptionID = ctx.option_ID,
//            Value = ctx.Value,
//            Duration = 5f,
//            Elapsed = 0f,
//            IsDebuff = true,
//            Target = ctx.User,
//            SourceItemID = ctx.item_ID
//        };
//        ctx.User.AddBuff(buff);
//        //ctx.User.buffUI?.SetBuffs(ctx.User.activeBuffs.Values.ToList(), ctx.User);
//        Debug.Log($"{ctx.User}에게 버프 적용 : {ctx.option_ID}");
//    }
//}
////버프로 빼야 되나
//public class BurnDebuffEffect : IOptionEffect
//{
//    public void Apply(OptionContext ctx)
//    {
//        var debuff = new BuffData
//        {
//            BuffID = $"{ctx.item_ID}_{ctx.option_ID}_onhit",
//            OptionID = ctx.option_ID,
//            Value = ctx.Value,
//            Duration = 5f,
//            Elapsed = 0f,
//            IsDebuff = true,
//            Target = ctx.Target,
//            SourceItemID = ctx.item_ID

//        };

//        ctx.Target.AddBuff(debuff);
//        Debug.Log($"{ctx.Target}에게 버프 적용 : {ctx.option_ID}");
//        //ctx.Target.buffUI?.SetBuffs(ctx.Target.activeBuffs.Values.ToList(), ctx.Target);
//    }
//}
//public class OneShot_HPHealing : IOptionEffect
//{
//    public void Apply(OptionContext ctx)
//    {
//        Debug.Log("체력 포션 아이템 사용해서 넘어오는데 성공함");
//        if (ctx.playerState != null)
//        {
//            ctx.playerState.CurrentHealth = Mathf.Min(
//                ctx.playerState.HP,
//                ctx.playerState.CurrentHealth + ctx.Value
//            );
//            Debug.Log($"[PlayerState] 체력 회복: {ctx.Value}");
//        }
//    }
//}
//public class OneShot_MPHealing : IOptionEffect
//{
//    public void Apply(OptionContext ctx)
//    {
//        Debug.Log("도로롱 아이템 사용해서 넘어오는데 성공함");
//        if (ctx.playerState != null)
//        {
//            ctx.playerState.CurrentMental = Mathf.Min(
//                ctx.playerState.MP,
//                ctx.playerState.CurrentMental + ctx.Value
//            );
//            Debug.Log($"[PlayerState] 정신력 회복: {ctx.Value}");
//        }
//    }
//}
//// 4) OptionManager
//public class OptionManager : MonoBehaviour
//{
//    public static OptionManager Instance { get; private set; }
//    public BuffUI buffUI;
//    public JsonManager jsonManager;
//    private static Dictionary<string, Option_Master> optionDict = new();

//    private void Awake()
//    {
//        if (Instance != null && Instance != this)
//        {
//            Destroy(gameObject);
//            return;
//        }
//        Instance = this;
//        DontDestroyOnLoad(gameObject); // 씬 변경 시 유지

//        jsonManager = JsonManager.Instance ?? FindObjectOfType<JsonManager>();
//        buffUI = FindObjectOfType<BuffUI>();
//    }
//    public static void Initialize(JsonManager json)
//    {
//        var options = json.GetOptionMasters("Option_Master");
//        optionDict = options.ToDictionary(x => x.Option_ID, x => x);
//    }

//    private static Dictionary<string, IOptionEffect> effects = new()
//{
//    { "Effect_001", new AddDamage() },
//    { "Effect_002", new CriticalBuff() },
//    { "Effect_003", new BurnDebuffEffect() },
//    { "Effect_004", new HealtingBuff() },
//    { "Effect_005", new SpeedBuff() },
//    { "Effect_006", new OneShot_HPHealing() },
//    { "Effect_007", new OneShot_MPHealing() },

//};
//    //효과에 대한 이름
//    private static Dictionary<string, string> optionDescriptions = new()
//    {
//        { "Option_001", "추가 데미지" },
//        { "Option_002", "크리티컬 확률 증가" },
//        { "Option_003", "화상 피해" },
//        { "Option_004", "최대 체력 " },
//        { "Option_005", "공격속도 버프" },
//        { "Option_006", "1회 실질 체력 회복" },
//        { "Option_007", "1회 실질 정신력 회복" },
//        { "null", "" } // 방어 처리
//    };
//    public static void ApplyOption(string optionID, OptionContext ctx)
//{
//    var opt = GetOption(optionID);

//    if (opt == null)
//    {
//        Debug.LogWarning($"[OptionManager] {optionID} 옵션 정보 없음");
//        return;
//    }

//    if (string.IsNullOrEmpty(opt.Effect_ID))
//    {
//        Debug.LogWarning($"[OptionManager] {optionID}의 Effect_ID가 비어 있음");
//        return;
//    }

//    if (!effects.TryGetValue(opt.Effect_ID, out var effect))
//    {
//        Debug.LogError($"[OptionManager] Effect_ID {opt.Effect_ID} → 미등록 효과");
//        return;
//    }

//    switch (opt.Option_Type)
//    {
//        case "OnEquip":
//        case "Passive":
//            effect.Apply(ctx);
//            Debug.Log($"{ctx.item_ID}장착시 , 패시브 아이템 적용 됨");
//            break;

//        case "OnHit":
//            ctx.User.OnHitOptions.Add(new Character.EquippedOption
//            {
//                OptionID = optionID,
//                Value = ctx.Value,
//                item_ID = ctx.item_ID
//            });
//            Debug.Log($"[OptionManager] OnHit 옵션 {optionID} 등록 완료");
//            break;

//        default:
//            Debug.LogWarning($"[OptionManager] 미지원 Option_Type: {opt.Option_Type}");
//                effect.Apply(ctx);
//            break;
//    }
//}

//    public static void UseItem(ItemData item, OptionContext ctx)
//    {
//        Debug.Log("옵션 매니저로 들어 왔습니다");
//        if (!string.IsNullOrEmpty(item.Option_1_ID))
//            Debug.Log("옵션 처리 직전");
//            OptionManager.ApplyOption(item.Option_1_ID, new OptionContext
//            {
//                User = ctx.User,
//                playerState = ctx.playerState,
//                Value = item.Option_Value1,
//                item_ID = item.Item_ID,
//                option_ID = item.Option_1_ID
//            });
//        Debug.Log("1번 옵션 적용 완료");
//        if (!string.IsNullOrEmpty(item.Option_2_ID))
//            OptionManager.ApplyOption(item.Option_2_ID, new OptionContext
//            {
//                User = ctx.User,
//                Value = item.Option_Value2,
//                item_ID = item.Item_ID,
//                option_ID = item.Option_2_ID
//            });
//        Debug.Log("2번 옵션 적용 완료");
//        Debug.Log("옵션 처리 후");
//    }
//    public static void RemovePassive(string optionID, Character target)
//    {
//        // target.OnHitOptions 에서 해당 OptionID 제거
//        target.OnHitOptions.RemoveAll(opt => opt.OptionID == optionID);

//        // 버프 목록에서 제거가 필요하다면 여기도 추가
//        // 예: target.BuffList.RemoveAll(...)

//        Debug.Log($"[패시브 제거됨] {optionID}");
//    }
//    public static string GetOptionDescription(string optionID)
//    {
//        if (string.IsNullOrEmpty(optionID) || optionID == "null")
//            return null;

//        return optionDescriptions.TryGetValue(optionID, out var desc) ? desc : $"옵션({optionID})";
//    }

//    public static Option_Master GetOption(string optionID)
//    {
//        if (optionDict.TryGetValue(optionID, out var opt))
//            return opt;
//        Debug.LogWarning($"[OptionManager] {optionID} 옵션 정보를 찾을 수 없습니다.");
//        return null;
//    }
//    public static void ApplyOnHitOnly(string optionID, OptionContext ctx)
//    {
//        var opt = GetOption(optionID);

//        if (opt == null || opt.Option_Type != "OnHit")
//            return;

//        if (effects.TryGetValue(opt.Effect_ID, out var effect))
//        {
//            effect.Apply(ctx);
//        }
//    }


//}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MyGame;
using Character = MyGame.Character;

// 1) 옵션 효과 인터페이스
public interface IOptionEffect
{
    void Apply(OptionContext ctx);
}

// 2) 효과 구현들
public class AddDamage : IOptionEffect
{
    public void Apply(OptionContext ctx)
    {
        int damage = Mathf.FloorToInt(ctx.Value);
        ctx.Target.Health -= damage;
        Debug.Log($"[AddDamage] {damage} 데미지 적용, 남은 체력: {ctx.Target.Health}");
    }
}

public class AddFireDamage : IOptionEffect
{
    public void Apply(OptionContext ctx)
    {
        int damage = Mathf.FloorToInt(ctx.Value);
        ctx.Target.Health -= damage;
        Debug.Log($"[AddFireDamage] 화염 {damage} 데미지 적용, 남은 체력: {ctx.Target.Health}");
    }
}

public class CriticalBuff : IOptionEffect
{
    public void Apply(OptionContext ctx)
    {
        var buffData = new BuffData
        {
            BuffID = $"{ctx.item_ID}_{ctx.option_ID}",
            User = ctx.User,
            OptionID = ctx.option_ID,
            Value = ctx.Value,
            SourceItemID = ctx.item_ID,
            IsPassive = true
        };
        ctx.User.AddBuff(buffData);
        Debug.Log($"[CriticalBuff] 크리티컬 버프 적용: {ctx.Value}%");
    }
}

public class SpeedBuff : IOptionEffect
{
    public void Apply(OptionContext ctx)
    {
        var buffData = new BuffData
        {
            BuffID = $"{ctx.item_ID}_{ctx.option_ID}",
            User = ctx.User,
            OptionID = ctx.option_ID,
            Value = ctx.Value,
            SourceItemID = ctx.item_ID,
            IsPassive = true
        };
        ctx.User.AddBuff(buffData);
        Debug.Log($"[SpeedBuff] 속도 버프 적용: {ctx.Value}%");
    }
}

public class HealtingBuff : IOptionEffect
{
    public void Apply(OptionContext ctx)
    {
        var buff = new BuffData
        {
            BuffID = $"{ctx.item_ID}_{ctx.option_ID}_onhit",
            OptionID = ctx.option_ID,
            Value = ctx.Value,
            Duration = 5f,
            Elapsed = 0f,
            IsDebuff = false, // 힐링은 디버프가 아니죠
            Target = ctx.User,
            SourceItemID = ctx.item_ID
        };
        ctx.User.AddBuff(buff);
        Debug.Log($"[HealtingBuff] {ctx.User.charaterName}에게 힐링 버프 적용");
    }
}

public class BurnDebuffEffect : IOptionEffect
{
    public void Apply(OptionContext ctx)
    {
        var debuff = new BuffData
        {
            BuffID = $"{ctx.item_ID}_{ctx.option_ID}_onhit",
            OptionID = ctx.option_ID,
            Value = ctx.Value,
            Duration = 5f,
            Elapsed = 0f,
            IsDebuff = true,
            Target = ctx.Target,
            SourceItemID = ctx.item_ID
        };

        ctx.Target.AddBuff(debuff);
        Debug.Log($"[BurnDebuff] {ctx.Target.charaterName}에게 화상 디버프 적용");
    }
}

public class ForceNextAttackMissEffect : IOptionEffect
{
    public void Apply(OptionContext ctx)
    {
        if (ctx.Target == null)
        {
            Debug.LogWarning("[ForceNextAttackMissEffect] 대상이 없어 효과를 적용하지 못했습니다.");
            return;
        }

        ctx.Target.AddForcedMiss(Mathf.Max(1, ctx.Value));
        Debug.Log($"[ForceNextAttackMissEffect] {ctx.Target.charaterName}의 다음 공격 실패 효과 적용");
    }
}

public class StatBonusEffect : IOptionEffect
{
    public void Apply(OptionContext ctx)
    {
        if (ctx.User == null)
        {
            Debug.LogWarning("[StatBonusEffect] 사용자 캐릭터가 없어 효과를 적용하지 못했습니다.");
            return;
        }

        Option_Master option = OptionManager.GetOption(ctx.option_ID);
        var buff = new BuffData
        {
            BuffID = $"{ctx.item_ID}_{ctx.option_ID}",
            OptionID = ctx.option_ID,
            Value = ctx.Value,
            Duration = 0f,
            Elapsed = 0f,
            IsDebuff = false,
            IsPassive = true,
            Target = ctx.User,
            User = ctx.User,
            SourceItemID = ctx.item_ID,
            StatusType = option?.StatusType,
            ApplyMode = "Stat",
            StackPolicy = "Ignore",
            StackCount = 1,
            MaxStack = 1,
            TriggerType = option?.TriggerType,
            ValueMode = option?.ValueMode,
            StatType = option?.StatType,
            ResistanceType = option?.ResistanceType
        };

        ctx.User.AddBuff(buff);
        Debug.Log($"[StatBonusEffect] {ctx.User.charaterName} {buff.StatType} +{ctx.Value}");
    }
}

public abstract class StatusStackEffect : IOptionEffect
{
    private readonly string fallbackStatusType;
    private readonly bool targetEnemy;
    private readonly bool isDebuff;

    protected StatusStackEffect(string fallbackStatusType, bool targetEnemy, bool isDebuff)
    {
        this.fallbackStatusType = fallbackStatusType;
        this.targetEnemy = targetEnemy;
        this.isDebuff = isDebuff;
    }

    public void Apply(OptionContext ctx)
    {
        Character target = targetEnemy ? ctx.Target : ctx.User;
        if (target == null)
        {
            Debug.LogWarning($"[{fallbackStatusType}] 대상이 없어 상태를 적용하지 못했습니다.");
            return;
        }

        Option_Master option = OptionManager.GetOption(ctx.option_ID);
        var buff = BuildBuff(ctx, option, target);
        target.AddBuff(buff);
        Debug.Log($"[{buff.StatusType}] {target.charaterName} stack +{buff.StackCount}");
    }

    private BuffData BuildBuff(OptionContext ctx, Option_Master option, Character target)
    {
        string statusType = string.IsNullOrEmpty(option?.StatusType) ? fallbackStatusType : option.StatusType;
        int stackCount = Mathf.Max(1, ctx.Value);

        return new BuffData
        {
            BuffID = $"{statusType}_{ctx.option_ID}",
            OptionID = ctx.option_ID,
            Value = ctx.Value,
            Duration = option?.Duration ?? 0f,
            Elapsed = 0f,
            IsDebuff = isDebuff,
            IsPassive = false,
            Target = target,
            User = ctx.User,
            SourceItemID = ctx.item_ID,
            StatusType = statusType,
            ApplyMode = option?.ApplyMode,
            StackPolicy = string.IsNullOrEmpty(option?.StackPolicy) ? "Stack" : option.StackPolicy,
            StackCount = stackCount,
            MaxStack = option != null && option.MaxStack > 0 ? option.MaxStack : 99,
            TriggerType = option?.TriggerType,
            ValueMode = option?.ValueMode,
            BaseChance = option?.BaseChance ?? 0f,
            ChancePerStack = option?.ChancePerStack ?? 0f,
            BaseValue = option?.BaseValue ?? 0f,
            ValuePerStack = option?.ValuePerStack ?? 0f,
            StatType = option?.StatType,
            ResistanceType = option?.ResistanceType,
            MaxRemoveCount = option?.MaxRemoveCount ?? 0
        };
    }
}

public class BleedStackEffect : StatusStackEffect
{
    public BleedStackEffect() : base("Bleed", true, true) { }
}

public class PoisonStackEffect : StatusStackEffect
{
    public PoisonStackEffect() : base("Poison", true, true) { }
}

public class HolyStackEffect : StatusStackEffect
{
    public HolyStackEffect() : base("Holy", false, false) { }
}

public class RegenStackEffect : StatusStackEffect
{
    public RegenStackEffect() : base("Regen", false, false) { }
}

public class FreezeStackEffect : StatusStackEffect
{
    public FreezeStackEffect() : base("Freeze", true, true) { }
}

public class BerserkBattleStartEffect : IOptionEffect
{
    private const int DefaultSpeedPercent = 50;
    private const int SelfDamagePercent = 1;
    private const int EnemyDamageTakenIncreasePercent = 25;
    private const int PlayerDamageTakenIncreasePercent = 30;
    private const int ArmorPenalty = -5;

    public void Apply(OptionContext ctx)
    {
        if (ctx.User == null)
        {
            Debug.LogWarning("[BerserkBattleStartEffect] 사용자 캐릭터가 없어 광폭화를 적용하지 못했습니다.");
            return;
        }

        int speedPercent = ctx.Value > 0 ? ctx.Value : DefaultSpeedPercent;
        int damageTakenIncrease = ctx.IsPlayer || ctx.playerState != null
            ? PlayerDamageTakenIncreasePercent
            : EnemyDamageTakenIncreasePercent;

        ctx.User.AddBuff(new BuffData
        {
            BuffID = "berserk_buff",
            OptionID = ctx.option_ID,
            Value = speedPercent,
            Duration = 0f,
            Elapsed = 0f,
            IsDebuff = false,
            IsPassive = false,
            Target = ctx.User,
            User = ctx.User,
            SourceItemID = ctx.item_ID,
            StatusType = "BerserkBuff",
            ApplyMode = "Stat",
            StackPolicy = "Refresh",
            StackCount = 1,
            MaxStack = 1,
            TriggerType = "OnAttack",
            ValueMode = "Percent",
            StatType = "AttackSpeed",
            SelfDamageOnAttackPercent = SelfDamagePercent
        });

        ctx.User.AddBuff(new BuffData
        {
            BuffID = "berserk_debuff",
            OptionID = ctx.option_ID,
            Value = ArmorPenalty,
            Duration = 0f,
            Elapsed = 0f,
            IsDebuff = true,
            IsPassive = false,
            Target = ctx.User,
            User = ctx.User,
            SourceItemID = ctx.item_ID,
            StatusType = "BerserkDebuff",
            ApplyMode = "Stat",
            StackPolicy = "Refresh",
            StackCount = 1,
            MaxStack = 1,
            TriggerType = "OnDamaged",
            ValueMode = "Percent",
            StatType = "Armor",
            DamageTakenIncreasePercent = damageTakenIncrease
        });

        Debug.Log($"[Berserk] {ctx.User.charaterName} 광폭화 적용: 공속 +{speedPercent}%, 반동 {SelfDamagePercent}%, 받는 피해 +{damageTakenIncrease}%, 방어 {ArmorPenalty}");
    }
}

public class OneShot_HPHealing : IOptionEffect
{
    public void Apply(OptionContext ctx)
    {
        if (ctx.playerState != null)
        {
            ctx.playerState.CurrentHealth = Mathf.Min(
                ctx.playerState.HP,
                ctx.playerState.CurrentHealth + ctx.Value
            );
            Debug.Log($"[OneShot_HPHealing] 체력 {ctx.Value} 회복 완료");
        }
    }
}

public class OneShot_MPHealing : IOptionEffect
{
    public void Apply(OptionContext ctx)
    {
        if (ctx.playerState != null)
        {
            ctx.playerState.CurrentMental = Mathf.Min(
                ctx.playerState.MP,
                ctx.playerState.CurrentMental + ctx.Value
            );
            Debug.Log($"[OneShot_MPHealing] 정신력 {ctx.Value} 회복 완료");
        }
    }
}

// 4) OptionManager - 핵심 수정사항들
public class OptionManager : MonoBehaviour
{
    public static OptionManager Instance { get; private set; }
    public BuffUI buffUI;
    public JsonManager jsonManager;

    // ⭐ 핵심: 옵션 딕셔너리를 static이 아닌 인스턴스 변수로 변경
    private Dictionary<string, Option_Master> optionDict = new();
    private bool isInitialized = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // JsonManager 참조 설정
        jsonManager = JsonManager.Instance ?? FindObjectOfType<JsonManager>();
        buffUI = FindObjectOfType<BuffUI>();

        // JsonManager가 준비되면 초기화
        if (jsonManager != null && jsonManager.IsReady)
        {
            InitializeOptions();
        }
        else if (jsonManager != null)
        {
            jsonManager.OnReady += InitializeOptions;
        }
    }

    // ⭐ 새로운 초기화 메서드
    private void InitializeOptions()
    {
        if (isInitialized) return;

        try
        {
            var options = jsonManager.GetOptionMasters("Option_Master");
            optionDict = options.ToDictionary(x => x.Option_ID, x => x);
            isInitialized = true;

            Debug.Log($"[OptionManager] 옵션 {optionDict.Count}개 로드 완료");
            foreach (var kvp in optionDict)
            {
                Debug.Log($"  - {kvp.Key}: {kvp.Value.Option_Description}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[OptionManager] 옵션 초기화 실패: {e.Message}");
        }
    }

    // 기존 static Initialize 메서드는 호환성을 위해 유지
    public static void Initialize(JsonManager json)
    {
        if (Instance != null)
        {
            Instance.jsonManager = json;
            Instance.InitializeOptions();
        }
    }

    // 효과 딕셔너리
    private static Dictionary<string, IOptionEffect> effects = new()
    {
        { "Effect_001", new AddDamage() },
        { "Effect_002", new CriticalBuff() },
        { "Effect_003", new BurnDebuffEffect() },
        { "Effect_004", new HealtingBuff() },
        { "Effect_005", new SpeedBuff() },
        { "Effect_006", new OneShot_HPHealing() },
        { "Effect_007", new OneShot_MPHealing() },
        { "Effect_008", new ForceNextAttackMissEffect() },
        { "Effect_009", new BleedStackEffect() },
        { "Effect_010", new PoisonStackEffect() },
        { "Effect_011", new HolyStackEffect() },
        { "Effect_012", new RegenStackEffect() },
        { "Effect_013", new FreezeStackEffect() },
        { "Effect_014", new StatBonusEffect() },
        { "Effect_015", new BerserkBattleStartEffect() },
    };

    // 옵션 설명 딕셔너리
    private static Dictionary<string, string> optionDescriptions = new()
    {
        { "Option_001", "추가 데미지" },
        { "Option_002", "크리티컬 확률 증가" },
        { "Option_003", "화상 피해" },
        { "Option_004", "최대 체력" },
        { "Option_005", "공격속도 버프" },
        { "Option_006", "1회 실질 체력 회복" },
        { "Option_007", "1회 실질 정신력 회복" },
        { "Option_008", "다음 공격 실패" },
        { "Option_009", "출혈" },
        { "Option_010", "중독" },
        { "Option_011", "신성" },
        { "Option_012", "재생" },
        { "Option_013", "빙결" },
        { "Option_014", "디버프 피해 저항" },
        { "Option_015", "출혈 저항" },
        { "Option_016", "중독 저항" },
        { "Option_017", "빙결 저항" },
        { "Option_018", "광폭화" },
        { "null", "" }
    };

    // ⭐ ApplyOption 메서드 수정
    public static void ApplyOption(string optionID, OptionContext ctx)
    {
        if (Instance == null || !Instance.isInitialized)
        {
            Debug.LogError("[OptionManager] 인스턴스가 초기화되지 않았습니다!");
            return;
        }

        var opt = Instance.GetOptionInternal(optionID);
        if (opt == null)
        {
            Debug.LogWarning($"[OptionManager] {optionID} 옵션 정보 없음");
            return;
        }

        if (string.IsNullOrEmpty(opt.Effect_ID))
        {
            Debug.LogWarning($"[OptionManager] {optionID}의 Effect_ID가 비어 있음");
            return;
        }

        if (!effects.TryGetValue(opt.Effect_ID, out var effect))
        {
            Debug.LogError($"[OptionManager] Effect_ID {opt.Effect_ID} → 미등록 효과");
            return;
        }

        Debug.Log($"[OptionManager] 옵션 적용 시작: {optionID} (타입: {opt.Option_Type})");

        switch (opt.Option_Type)
        {
            case "OnEquip":
            case "Passive":
                effect.Apply(ctx);
                Debug.Log($"[OptionManager] {ctx.item_ID} 장착시 패시브 적용됨");
                break;

            case "OnHit":
                RegisterEquippedOption(ctx.User.OnHitOptions, optionID, ctx.Value, ctx.item_ID);
                Debug.Log($"[OptionManager] OnHit 옵션 {optionID} 등록 완료");
                break;

            case "OnBattleStart":
            case "BattleStart":
                RegisterEquippedOption(ctx.User.OnBattleStartOptions, optionID, ctx.Value, ctx.item_ID);
                Debug.Log($"[OptionManager] 전투 시작 옵션 {optionID} 등록 완료");
                break;

            default:
                Debug.LogWarning($"[OptionManager] 미지원 Option_Type: {opt.Option_Type}, 기본 적용 시도");
                effect.Apply(ctx);
                break;
        }
    }

    private static void RegisterEquippedOption(List<Character.EquippedOption> options, string optionID, int value, string itemID)
    {
        if (options == null || string.IsNullOrEmpty(optionID))
            return;

        var equippedOption = new Character.EquippedOption
        {
            OptionID = optionID,
            Value = value,
            item_ID = itemID
        };
        int index = options.FindIndex(opt => opt.OptionID == optionID && opt.item_ID == itemID);
        if (index >= 0)
            options[index] = equippedOption;
        else
            options.Add(equippedOption);
    }

    public static void UseItem(ItemData item, OptionContext ctx)
    {
        Debug.Log($"[OptionManager] 아이템 사용: {item.Item_ID}");

        if (!string.IsNullOrEmpty(item.Option_1_ID))
        {
            Debug.Log($"[OptionManager] 옵션1 처리: {item.Option_1_ID}");
            ApplyOption(item.Option_1_ID, new OptionContext
            {
                User = ctx.User,
                playerState = ctx.playerState,
                IsPlayer = ctx.IsPlayer,
                Value = item.Option_Value1,
                item_ID = item.Item_ID,
                option_ID = item.Option_1_ID
            });
        }

        if (!string.IsNullOrEmpty(item.Option_2_ID))
        {
            Debug.Log($"[OptionManager] 옵션2 처리: {item.Option_2_ID}");
            ApplyOption(item.Option_2_ID, new OptionContext
            {
                User = ctx.User,
                playerState = ctx.playerState,
                IsPlayer = ctx.IsPlayer,
                Value = item.Option_Value2,
                item_ID = item.Item_ID,
                option_ID = item.Option_2_ID
            });
        }
    }

    public static void RemovePassive(string optionID, Character target)
    {
        target.OnHitOptions.RemoveAll(opt => opt.OptionID == optionID);
        target.OnBattleStartOptions.RemoveAll(opt => opt.OptionID == optionID);
        Debug.Log($"[OptionManager] 패시브 제거됨: {optionID}");
    }

    public static string GetOptionDescription(string optionID)
    {
        if (string.IsNullOrEmpty(optionID) || optionID == "null")
            return null;

        return optionDescriptions.TryGetValue(optionID, out var desc) ? desc : $"옵션({optionID})";
    }

    // ⭐ GetOption을 인스턴스 메서드로 변경
    public static Option_Master GetOption(string optionID)
    {
        if (Instance == null)
        {
            Debug.LogError("[OptionManager] Instance가 null입니다!");
            return null;
        }
        return Instance.GetOptionInternal(optionID);
    }

    private Option_Master GetOptionInternal(string optionID)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[OptionManager] 아직 초기화되지 않았습니다!");
            return null;
        }

        if (optionDict.TryGetValue(optionID, out var opt))
        {
            Debug.Log($"[OptionManager] 옵션 발견: {optionID} -> {opt.Option_Description}");
            return opt;
        }

        Debug.LogWarning($"[OptionManager] {optionID} 옵션 정보를 찾을 수 없습니다. 사용 가능한 옵션: {string.Join(", ", optionDict.Keys)}");
        return null;
    }

    public static void ApplyOnHitOnly(string optionID, OptionContext ctx)
    {
        var opt = GetOption(optionID);
        if (opt == null || opt.Option_Type != "OnHit")
            return;

        if (effects.TryGetValue(opt.Effect_ID, out var effect))
        {
            effect.Apply(ctx);
        }
    }

    public static void ApplyBattleStartOnly(string optionID, OptionContext ctx)
    {
        var opt = GetOption(optionID);
        if (opt == null)
            return;

        bool isBattleStart =
            opt.Option_Type == "OnBattleStart" ||
            opt.Option_Type == "BattleStart";
        if (!isBattleStart)
            return;

        if (effects.TryGetValue(opt.Effect_ID, out var effect))
        {
            effect.Apply(ctx);
        }
    }

    // 디버그용 메서드 추가
    [ContextMenu("Debug Print All Options")]
    public void DebugPrintOptions()
    {
        Debug.Log($"[OptionManager] 초기화 상태: {isInitialized}");
        Debug.Log($"[OptionManager] 등록된 옵션 수: {optionDict.Count}");
        foreach (var kvp in optionDict)
        {
            Debug.Log($"  {kvp.Key}: {kvp.Value.Option_Description} (Effect: {kvp.Value.Effect_ID}, Type: {kvp.Value.Option_Type})");
        }
    }
}
