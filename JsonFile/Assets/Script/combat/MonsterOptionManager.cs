using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyGame;
using UnityEngine;

public class MonsterOptionManager : MonoBehaviour
{
    public JsonManager jsonManager;
    private Dictionary<string, IOptionEffect> effects;

    void Awake()
    {
        if (jsonManager == null)
            jsonManager = FindObjectOfType<JsonManager>();

        effects = new Dictionary<string, IOptionEffect>();
        // 몬스터용 옵션만 등록
        effects["MonEffect_001"] = new MonsterCorrosionEffect();
        //effects["MonEffect_002"] = new PoisonCloudEffect();
        // …필요한 만큼
    }


    public class MonsterCorrosionEffect : IOptionEffect
    {
        public void Apply(OptionContext ctx)
        {
            //상대방의 방어력을 깎는다
            Debug.Log($"ctx.Target.armor의 값은 {ctx.Target.armor}" + $"ctx.Value의 값은 : {ctx.Value}");
            int debufArmor = ctx.Target.armor - ctx.Value;
            Debug.Log(debufArmor);
            //Debug.Log($"ctx의 유저는 = {ctx.User}  , ctx.target = {ctx.Target} , ctx.target.armor {ctx.Target.armor}");
            ctx.Target.armor = debufArmor;
            Debug.Log($"몬스터 옵션 테스트용 debug 입니다 {ctx.Target.armor}");
        }
    }

    public void ApplyMonsterOption(string optionID, OptionContext ctx)
    {
        Debug.Log($"몬스터 옵션의 이름 = {optionID}");
        if (effects.TryGetValue(optionID, out var e))
        {
            e.Apply(ctx);
        }
        else
            Debug.LogWarning($"MonsterOptionManager: 미등록 OptionID={optionID}");
    }
    public void ApplyOption(string optionID, OptionContext ctx)
    {
        // Debug.Log("옵션 적용되었습니다");
        if (effects.TryGetValue(optionID, out var e))
        {
            e.Apply(ctx);
        }
        else if (optionID == null)
        {
            Debug.LogWarning($"MonsterOptionManager: 값이 비워져 있습니다!={optionID}");
        }
        else
        {
            Debug.LogWarning($"MonsterOptionManager: 미등록 OptionID={optionID}");
        }
    }
}
