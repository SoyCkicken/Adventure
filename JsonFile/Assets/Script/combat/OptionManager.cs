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

public class CriticalBuff : IOptionEffect
{
    public void Apply(OptionContext ctx)
    {
        // 키를 “장비ID_옵션ID” 로 합쳐서
        var buffKey = new BuffData
        {
            BuffID = $"{ctx.item_ID}_{ctx.option_ID}",
            User = ctx.User,
            OptionID = ctx.option_ID,
            Value = ctx.Value,
            SourceItemID = ctx.item_ID,
            IsPassive = true
        };
        ctx.User.AddBuff(buffKey);
    }
}

public class SpeedBuff : IOptionEffect
{
    public void Apply(OptionContext ctx)
    {
        var buffKey = new BuffData
        {
            BuffID = $"{ctx.item_ID}_{ctx.option_ID}",
            User = ctx.User,
            OptionID = ctx.option_ID,
            Value = ctx.Value,
            SourceItemID = ctx.item_ID,
            IsPassive = true
        };
        ctx.User.AddBuff(buffKey);
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
        ctx.User.AddBuff(buff);
        Debug.Log($"{ctx.User}에게 버프 적용 : {ctx.option_ID}");
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
public class OneShot_HPHealing : IOptionEffect
{
    public void Apply(OptionContext ctx)
    {
        Debug.Log("체력 포션 아이템 사용해서 넘어오는데 성공함");
        if (ctx.playerState != null)
        {
            ctx.playerState.CurrentHealth = Mathf.Min(
                ctx.playerState.HP,
                ctx.playerState.CurrentHealth + ctx.Value
            );
            Debug.Log($"[PlayerState] 체력 회복: {ctx.Value}");
        }
    }
}
public class OneShot_MPHealing : IOptionEffect
{
    public void Apply(OptionContext ctx)
    {
        Debug.Log("도로롱 아이템 사용해서 넘어오는데 성공함");
        if (ctx.playerState != null)
        {
            ctx.playerState.CurrentMental = Mathf.Min(
                ctx.playerState.MP,
                ctx.playerState.CurrentMental + ctx.Value
            );
            Debug.Log($"[PlayerState] 정신력 회복: {ctx.Value}");
        }
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
    { "Effect_005", new SpeedBuff() },
    { "Effect_006", new OneShot_HPHealing() },
    { "Effect_007", new OneShot_MPHealing() },

};
    //효과에 대한 이름
    private static Dictionary<string, string> optionDescriptions = new()
    {
        { "Option_001", "추가 데미지" },
        { "Option_002", "크리티컬 확률 증가" },
        { "Option_003", "화상 피해" },
        { "Option_004", "최대 체력 " },
        { "Option_005", "공격속도 버프" },
        { "Option_006", "1회 실질 체력 회복" },
        { "Option_007", "1회 실질 정신력 회복" },
        { "null", "" } // 방어 처리
    };

    //public static void ApplyOption(string optionID, OptionContext ctx)
    //{
    //    var opt = GetOption(optionID); // 이미 캐싱된 optionDict에서 가져오는 방식으로 수정

    //    if (opt == null)
    //    {
    //        Debug.LogWarning($"[OptionManager] {optionID} 옵션 정보 없음");
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
    //            break;
    //    }
    //}
    public static void ApplyOption(string optionID, OptionContext ctx)
{
    var opt = GetOption(optionID);

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

    switch (opt.Option_Type)
    {
        case "OnEquip":
        case "Passive":
            effect.Apply(ctx);
            Debug.Log($"{ctx.item_ID}장착시 , 패시브 아이템 적용 됨");
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
                effect.Apply(ctx);
            break;
    }
}

    public static void UseItem(ItemData item, OptionContext ctx)
    {
        Debug.Log("옵션 매니저로 들어 왔습니다");
        if (!string.IsNullOrEmpty(item.Option_1_ID))
            Debug.Log("옵션 처리 직전");
            OptionManager.ApplyOption(item.Option_1_ID, new OptionContext
            {
                User = ctx.User,
                playerState = ctx.playerState,
                Value = item.Option_Value1,
                item_ID = item.Item_ID,
                option_ID = item.Option_1_ID
            });
        Debug.Log("옵션 처리 후");
        if (!string.IsNullOrEmpty(item.Option_2_ID))
            OptionManager.ApplyOption(item.Option_2_ID, new OptionContext
            {
                User = ctx.User,
                Value = item.Option_Value2,
                item_ID = item.Item_ID,
                option_ID = item.Option_2_ID
            });
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