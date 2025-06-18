using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;
using MyGame;
using Character = MyGame.Character;

// 1) 옵션 효과 인터페이스
public interface IOptionEffect
{
    /// <summary>
    /// 옵션 효과를 적용
    /// </summary>
    /// <param name="user">효과를 발동한 주체</param>
    /// <param name="target">효과를 받는 대상</param>
    /// <param name="value">옵션의 수치값</param>
    void Apply(OptionContext ctx);
}

// 2) 효과 구현 예시: 출혈 효과
public class AddDamage : IOptionEffect
{
    public void Apply(OptionContext ctx)
    {
        int damage = Mathf.FloorToInt(ctx.Value);
        ctx.Target.Health -= damage;
        Debug.Log(damage);
        Debug.Log(ctx.Target.Health);
    }
}

public class AddFireDamage : IOptionEffect
{
    public void Apply(OptionContext ctx)
    {
        int damage = Mathf.FloorToInt(ctx.Value);
        ctx.Target.Health -= damage;
        Debug.Log(damage);
        Debug.Log($"{ctx.Target.Health}");
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
            IsDebuff = true,
            Target = ctx.User,
            SourceItemID = ctx.item_ID
        };
        ctx.Target.AddBuff(buff);
        Debug.Log($"{ctx.User}에게 버프 적용 : {ctx.option_ID}");
    }
}
public class CriticalBuff : IOptionEffect
{
    public void Apply(OptionContext ctx)
    {
        // 키를 “장비ID_옵션ID” 로 합쳐서
        var buffKey = new BuffData
        {
            BuffID = $"{ctx.item_ID}_{ctx.option_ID}",
            OptionID = ctx.option_ID,
            Value = ctx.Value,
            SourceItemID = ctx.item_ID,
            IsPassive = true
        };
        ctx.User.AddBuff(buffKey);
    }
}
//버프로 빼야 되나
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
        Debug.Log($"{ctx.Target}에게 버프 적용 : {ctx.option_ID}");
    }
}

// 4) OptionManager
public class OptionManager : MonoBehaviour
{
    public JsonManager jsonManager;
    private static Dictionary<string, Option_Master> optionDict = new();


    public static void Initialize(JsonManager json)
    {
        var options = json.GetOptionMasters("Option_Master");
        optionDict = options.ToDictionary(x => x.Option_ID, x => x);
    }

    private static Dictionary<string, IOptionEffect> effects = new()
{
    { "Effect_001", new AddDamage() },
    { "Effect_002", new CriticalBuff() },
    { "Effect_003", new BurnDebuffEffect() },
    { "Effect_004", new HealtingBuff() },
};
    //효과에 대한 이름
    private static Dictionary<string, string> optionDescriptions = new()
    {
        { "Option_001", "추가 데미지" },
        { "Option_002", "크리티컬 확률 증가" },
        { "Option_003", "화상 피해" },
        { "Option_004", "회복 버프" },
        { "null", "" } // 방어 처리
    };

    public static void ApplyOption(string optionID, OptionContext ctx)
    {
        var opt = GetOption(optionID); // 이미 캐싱된 optionDict에서 가져오는 방식으로 수정

        if (opt == null)
        {
            Debug.LogWarning($"[OptionManager] {optionID} 옵션 정보 없음");
            return;
        }

        if (!effects.TryGetValue(opt.Effect_ID, out var effect))
        {
            Debug.LogError($"[OptionManager] Effect_ID {opt.Effect_ID} → 미등록 효과");
            return;
        }

        switch (opt.Option_Type)
        {
            case "OnEquip":
            case "Passive":
                effect.Apply(ctx);
                break;

            case "OnHit":
                ctx.User.OnHitOptions.Add(new Character.EquippedOption
                {
                    OptionID = optionID,
                    Value = ctx.Value,
                    item_ID = ctx.item_ID
                });
                Debug.Log($"[OptionManager] OnHit 옵션 {optionID} 등록 완료");
                break;

            default:
                Debug.LogWarning($"[OptionManager] 미지원 Option_Type: {opt.Option_Type}");
                break;
        }
    }

    public static string GetOptionDescription(string optionID)
    {
        if (string.IsNullOrEmpty(optionID) || optionID == "null")
            return null;

        return optionDescriptions.TryGetValue(optionID, out var desc) ? desc : $"옵션({optionID})";
    }

    public static Option_Master GetOption(string optionID)
    {
        if (optionDict.TryGetValue(optionID, out var opt))
            return opt;
        Debug.LogWarning($"[OptionManager] {optionID} 옵션 정보를 찾을 수 없습니다.");
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
    public static void ApplyBuffEffect(BuffData buff)
    {
        var opt = GetOption(buff.OptionID);
        if (opt == null) return;

        if (effects.TryGetValue(opt.Effect_ID, out var effect))
        {
            var ctx = new OptionContext
            {
                User = buff.User, // 대부분의 경우 디버프는 Target에게 적용
                Target = buff.Target,
                Value = buff.Value,
                option_ID = buff.OptionID,
                item_ID = buff.SourceItemID
            };
            effect.Apply(ctx);
        }
    }
    public static void ApplyDeBuffEffect(BuffData buff)
    {
        var opt = GetOption(buff.OptionID);
        if (opt == null) return;

        if (effects.TryGetValue(opt.Effect_ID, out var effect))
        {
            var ctx = new OptionContext
            {
                User = buff.User, // 대부분의 경우 디버프는 Target에게 적용
                Target = buff.Target,
                Value = buff.Value,
                option_ID = buff.OptionID,
                item_ID = buff.SourceItemID
            };
            effect.Apply(ctx);
        }
    }

}



//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using UnityEngine.TextCore.Text;
//using MyGame;
//using Character = MyGame.Character;

//// 1) 옵션 효과 인터페이스
//public interface IOptionEffect
//{
//    void Apply(OptionContext ctx);
//}

//// === 실제 효과 구현 ===
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

//public class HealtingBuff : IOptionEffect
//{
//    public void Apply(OptionContext ctx)
//    {
//        int healing = Mathf.FloorToInt(ctx.Target.MaxHealth * 0.02f);
//        ctx.Target.Health += healing;
//        if (ctx.Target.Health > ctx.Target.MaxHealth)
//            ctx.Target.Health = ctx.Target.MaxHealth;
//        Debug.Log($"[회복 효과] {ctx.Target.charaterName} +{healing} → 현재 HP: {ctx.Target.Health}");
//    }
//}

//public class CriticalBuff : IOptionEffect
//{
//    public void Apply(OptionContext ctx)
//    {
//        var buff = new BuffData
//        {
//            BuffID = $"{ctx.item_ID}_{ctx.option_ID}",
//            OptionID = ctx.option_ID,
//            Value = ctx.Value,
//            SourceItemID = ctx.item_ID,
//            IsPassive = true,
//            User = ctx.User
//        };
//        ctx.User.AddBuff(buff);
//    }
//}

//public class BurnDebuffEffect : IOptionEffect
//{
//    public void Apply(OptionContext ctx)
//    {
//        int damage = Mathf.FloorToInt(ctx.Target.MaxHealth * 0.02f);
//        ctx.Target.Health -= damage;
//        Debug.Log($"[🔥화상 피해] {ctx.Target.charaterName} → {damage} 피해. 현재 HP: {ctx.Target.Health}");
//    }
//}

//// === 옵션 매니저 ===
//public class OptionManager : MonoBehaviour
//{
//    public JsonManager jsonManager;
//    private static Dictionary<string, Option_Master> optionDict = new();

//    public static void Initialize(JsonManager json)
//    {
//        var options = json.GetOptionMasters("Option_Master");
//        optionDict = options.ToDictionary(x => x.Option_ID, x => x);
//    }

//    private static Dictionary<string, IOptionEffect> effects = new()
//    {
//        { "Effect_001", new AddDamage() },
//        { "Effect_002", new CriticalBuff() },
//        { "Effect_003", new BurnDebuffEffect() },
//        { "Effect_004", new HealtingBuff() },
//    };

//    private static Dictionary<string, string> optionDescriptions = new()
//    {
//        { "Option_001", "추가 데미지" },
//        { "Option_002", "크리티컬 확률 증가" },
//        { "Option_003", "화상 피해" },
//        { "Option_004", "회복 버프" },
//        { "null", "" }
//    };

//    public static void ApplyOption(string optionID, OptionContext ctx)
//    {
//        var opt = GetOption(optionID);
//        if (opt == null)
//        {
//            Debug.LogWarning($"[OptionManager] {optionID} 옵션 정보 없음");
//            return;
//        }

//        if (!effects.TryGetValue(opt.Effect_ID, out var effect))
//        {
//            Debug.LogError($"[OptionManager] Effect_ID {opt.Effect_ID} → 미등록 효과");
//            return;
//        }

//        switch (opt.Option_Type)
//        {
//            case "OnEquip":
//            case "Passive":
//                effect.Apply(ctx);
//                break;

//            case "OnHit":
//                ctx.User.OnHitOptions.Add(new Character.EquippedOption
//                {
//                    OptionID = optionID,
//                    Value = ctx.Value,
//                    item_ID = ctx.item_ID
//                });
//                Debug.Log($"[OptionManager] OnHit 옵션 {optionID} 등록 완료");
//                break;

//            default:
//                Debug.LogWarning($"[OptionManager] 미지원 Option_Type: {opt.Option_Type}");
//                break;
//        }
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
//        if (opt == null || opt.Option_Type != "OnHit") return;

//        if (effects.TryGetValue(opt.Effect_ID, out var effect))
//        {
//            effect.Apply(ctx);
//        }
//    }

//    public static void ApplyBuffEffect(BuffData buff)
//    {
//        var opt = GetOption(buff.OptionID);
//        if (opt == null) return;

//        if (effects.TryGetValue(opt.Effect_ID, out var effect))
//        {
//            var ctx = new OptionContext
//            {
//                User = buff.User,
//                Target = buff.Target,
//                Value = buff.Value,
//                option_ID = buff.OptionID,
//                item_ID = buff.SourceItemID
//            };
//            effect.Apply(ctx);
//        }
//    }

//    public static void ApplyDeBuffEffect(BuffData buff)
//    {
//        var opt = GetOption(buff.OptionID);
//        if (opt == null) return;

//        if (effects.TryGetValue(opt.Effect_ID, out var effect))
//        {
//            var ctx = new OptionContext
//            {
//                User = buff.User,
//                Target = buff.Target,
//                Value = buff.Value,
//                option_ID = buff.OptionID,
//                item_ID = buff.SourceItemID
//            };
//            effect.Apply(ctx);
//        }
//    }
//}
