using System.Collections;
using System.Collections.Generic;
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
        // 예: Roar, PoisonCloud 등 몬스터 전용
        effects["MonEffect_001"] = new MonsterRoarEffect();
        //effects["MonEffect_002"] = new PoisonCloudEffect();
        // …필요한 만큼
    }

    public void ApplyMonsterOption(string optionID, OptionContext ctx)
    {
        Debug.Log($"몬스터 옵션의 이름 = {optionID}");
        if (effects.TryGetValue(optionID, out var e))
            e.Apply(ctx);
        else
            Debug.LogWarning($"MonsterOptionManager: 미등록 OptionID={optionID}");
    }
}
