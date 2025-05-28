using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;
using MyGame;
using Unity.VisualScripting;



//public class OptionContext
//{
//    public Character User;
//    public Character Target;
//    public int hp;

//    // 기본 옵션 값 (ex: 스케일, 퍼센트 등)
//    public int Value;

//    // 상황별 추가 정보
//    public int DamageDealt;   // 예: 라이프스틸
//    public int TurnNumber;    // 예: 연소 스택
//    // ...필요한 필드만 계속 추가
//}

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
public class BleedEffect : IOptionEffect
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
        int heal = Mathf.FloorToInt(ctx.Value);
        ctx.Target.Health -= heal;
        Debug.Log(heal);
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
public class Critical : IOptionEffect
{
    public void Apply(OptionContext ctx)
    {
        // 키를 “장비ID_옵션ID” 로 합쳐서
        var buffKey = $"{ctx.item_ID}_{ctx.option_ID}";
        ctx.User.AddCritBuff(buffKey, ctx.Value);
    }
}
public class BurnEffect : IOptionEffect
{
    public void Apply(OptionContext ctx)
    {
        //예: 매 턴마다 추가 피해를 주는 스택을 만든다
        int heal = Mathf.FloorToInt((ctx.Value) * 3);
        ctx.hp -= heal;
        Debug.Log(ctx.hp);
    }
}
public class MonsterRoarEffect : IOptionEffect
{
    public void Apply(OptionContext ctx)
    {
        //상대방의 방어력을 깎는다
        Debug.Log($"ctx.Target.armor의 값은 {ctx.Target.armor}"+ $"ctx.Value의 값은 : {ctx.Value}");
        int debufArmor = ctx.Target.armor - ctx.Value;
        Debug.Log(debufArmor);
        //Debug.Log($"ctx의 유저는 = {ctx.User}  , ctx.target = {ctx.Target} , ctx.target.armor {ctx.Target.armor}");
        ctx.Target.armor = debufArmor;
        Debug.Log($"몬스터 옵션 테스트용 debug 입니다 {ctx.Target.armor}");
    }
}

// 4) OptionManager
public class OptionManager : MonoBehaviour
{
    public JsonManager jsonManager;
    List<Option_Master> option_Masters;
    Dictionary<string, IOptionEffect> effects;

    void Awake()
    {
        effects = new Dictionary<string, IOptionEffect>()
        {
            {"Effect_Bleed",   new BleedEffect()},
            {"Effect_Fire",new AddFireDamage()},
            {"Effect_Critical",     new Critical()},
            {"Effect_Healing",     new Healting()}, 
            // …추가
        };
    }

    public void ApplyOption(string optionID, OptionContext ctx)
    {
        // Debug.Log("옵션 적용되었습니다");
        var opt = jsonManager.GetOptionMasters("Option_Master")
                     .FirstOrDefault(x => x.Option_ID == optionID);
        Debug.Log(opt);
        if (opt == null)
        {
            Debug.Log("옵션이 없습니다");
            return;
        }
        ;

        if (effects.TryGetValue(opt.Effect_ID, out var effect))
        {
            effect.Apply(ctx);
            Debug.Log(ctx);
        }

        else
            Debug.LogError($"미등록 Effect_ID {opt.Effect_ID}");
    }
}

