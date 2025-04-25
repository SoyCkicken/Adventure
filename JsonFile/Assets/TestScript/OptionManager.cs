using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;
using MyGame;



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
        ctx.hp -= damage;
        Debug.Log(ctx.hp);
    }
}

public class LifeStealEffect : IOptionEffect
{
    public void Apply(OptionContext ctx)
    {
        int heal = Mathf.FloorToInt(ctx.DamageDealt);
        ctx.hp=+heal;
        Debug.Log(ctx.hp);
    }
}
public class Healting : IOptionEffect
{
    public void Apply(OptionContext ctx)
    {
    }
}

public class BurnEffect : IOptionEffect
{
    public void Apply(OptionContext ctx)
    {
        // 예: 매 턴마다 추가 피해를 주는 스택을 만든다
        //ctx.Target.AddStatus(new BurnStatus(
        //    baseDamage: ctx.Value,
        //    extraPerTurn: ctx.TurnNumber));
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
            {"Effect_LifeSteal",new LifeStealEffect()},
            {"Effect_Burn",     new BurnEffect()},
            // …추가
        };
    }

    public void ApplyOption(string optionID, OptionContext ctx)
    {
        Debug.Log("옵션 적용되었습니다");
        var opt = jsonManager.Item_Options
                     .FirstOrDefault(x => x.Option_ID == optionID);
        Debug.Log(opt);
        if (opt == null) 
        {
            Debug.Log("옵션이 없습니다");
        };

        if (effects.TryGetValue(opt.Effect_ID, out var effect))
        {
            effect.Apply(ctx);
            Debug.Log(ctx);
        }
           
        else
            Debug.LogError($"미등록 Effect_ID {opt.Effect_ID}");
    }
    private void Start()
    {
    }
}