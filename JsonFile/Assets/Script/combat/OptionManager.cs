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
public class Healting : IOptionEffect
{
    public void Apply(OptionContext ctx)
    {
        int heal = Mathf.FloorToInt(ctx.Value);
        ctx.User.Health += heal;
        Debug.Log(heal);
        Debug.Log(ctx.User.Health);
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
    { "Effect_004", new AddFireDamage() },
};
    //효과에 대한 이름
    private static Dictionary<string, string> optionDescriptions = new()
    {
        { "Option_001", "추가 데미지" },
        { "Option_002", "추가 공격 속도" },
        { "Option_003", "화상 피해" },
        { "Option_004", "흡혈" },
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
}