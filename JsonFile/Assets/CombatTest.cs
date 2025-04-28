using UnityEngine;
using MyGame;  // OptionContext, Character 정의된 네임스페이스

public class CombatTest : MonoBehaviour
{
    public OptionManager optionManager;
    Character user, target;

    void Start()
    {
        // OptionManager 참조 확보
        optionManager = optionManager != null
            ? optionManager
            : FindObjectOfType<OptionManager>();

        // 유저/타겟 캐릭터 더미 생성
        user = CreateDummy("User");
        target = CreateDummy("Target");

        // 각 옵션을 테스트해본다
        TestOption("Option_001", value: 10, dealt: 0, turn: 0);
        TestOption("Option_002", value: 20, dealt: 50, turn: 0);
        TestOption("Option_003", value: 5, dealt: 150, turn: 0);
        TestOption("Option_004", value: 15, dealt: 0, turn: 3);
    }

    Character CreateDummy(string name)
    {
        var go = new GameObject(name);
        var c = go.AddComponent<Character>();
        c.charaterName = name;
        c.Health = 500;
        return c;
    }

    void TestOption(string optionID, int value, int dealt, int turn)
    {
        // 호출 전 로그
        Debug.Log($"[Test] {optionID} ▶ Value={value}, Dealt={dealt}, Turn={turn}");

        // 컨텍스트 채우고 호출
        var ctx = new OptionContext
        {
            User = user,
            Target = target,
            Value = value,
            DamageDealt = dealt,
            TurnNumber = turn
        };
        optionManager.ApplyOption(optionID, ctx);

        // 호출 후 로그 (구현체 내부에서도 로그 찍힌다)
        Debug.Log($"[Test] {optionID} 완료\n");
        //Debug.Log(ctx.User.Health);
    }
}
