using UnityEngine;
using MyGame;
using System.Collections;  // OptionContext, Character 정의된 네임스페이스

public class CombatTest : MonoBehaviour
{
    public OptionManager optionManager;
    public Character player;
    public Character enemy;
    private bool battleOver;

    private void Awake()
    {
        //SetupDummy(enemy, speed: 0.8f, armor: 1, baseDmg: 8,
        //          optID: null, optVal: 0, 10);
    }

    void Start()
    {
        // OptionManager 참조 확보
        optionManager = optionManager != null
            ? optionManager
            : FindObjectOfType<OptionManager>();

        // 유저/타겟 캐릭터 더미 생성
        battleOver = false;

        // 각 옵션을 테스트해본다
        //StartCoroutine(BattleLoop());
        StartCoroutine(AttackLoop(player, enemy, true));
        StartCoroutine(AttackLoop(enemy, player, false));
    }
    void SetupDummy(Character c, float speed, int armor, int baseDmg, string optID, int optVal,int CitChance)
    {
        c.speed = speed;
        c.armor = armor;
        c.damage = baseDmg;
        c.Health = 1000;
        c.charaterName = c.gameObject.name;
        c.CitChance = CitChance;

        // 플레이어만 옵션을 쓸 거라면, Character에 아래 필드만 추가해두고 세팅
    }
    //IEnumerator BattleLoop()
    //{
    //    // 양쪽 생존하는 동안 반복
    //    while (player.Health > 0 && enemy.Health > 0)
    //    {
    //        // — 플레이어 공격
    //        yield return new WaitForSeconds(1f / player.speed);
    //        int dealt = player.damage;

    //        // 옵션 적용 (플레이어만)
    //        foreach (var opt in player.OnHitOptions)
    //        {
    //            var ctx = new OptionContext
    //            {
    //                User = player,
    //                Target = enemy,
    //                Value = opt.Value,
    //                option_ID = opt.OptionID,
    //                item_ID = opt.item_ID
    //            };
    //            optionManager.ApplyOption(opt.OptionID, ctx);
    //        }
    //        player.Attack(enemy);
    //        if (enemy.Health <= 0) break;

    //        // — 적 공격
    //        yield return new WaitForSeconds(1f / enemy.speed);
    //        enemy.Attack(player);
    //    }

    //    // 전투 종료 로그
    //    var winner = player.Health > 0 ? player.charaterName : enemy.charaterName;
    //    Debug.Log($"전투 종료! 승자: {winner}");
    //}
    IEnumerator AttackLoop(Character attacker, Character target, bool isPlayer)
    {
        // 전투 시작 전에 잠깐 딜레이 주고 싶으면 여기에 yield return new WaitForSeconds(0.2f);
        while (!battleOver)
        {
            // 속도에 따른 대기
            yield return new WaitForSeconds(1f / attacker.speed);

            if (battleOver) yield break;

            // 실제 공격 (크리티컬 / 방어구 경감 모두 Character.Attack() 안에서 처리)
            int dealt = attacker.Attack(target);

            // 플레이어만 OnHit 옵션 적용
            if (isPlayer && attacker.OnHitOptions != null)
            {
                foreach (var opt in attacker.OnHitOptions)
                {
                    var ctx = new OptionContext
                    {
                        User = attacker,
                        Target = target,
                        option_ID = opt.OptionID,
                        Value = opt.Value,
                    };
                    optionManager.ApplyOption(opt.OptionID, ctx);
                }
            }

            // 누가 죽었는지 체크
            if (target.Health <= 0)
            {
                battleOver = true;
                Debug.Log($"전투 종료! 승자: {attacker.charaterName}");
                yield break;
            }
        }
    }



    void TestOption(string optionID, int value, int dealt, int turn)
    {
        // 호출 전 로그
        Debug.Log($"[Test] {optionID} ▶ Value={value}, Dealt={dealt}, Turn={turn}");

        // 컨텍스트 채우고 호출
        var ctx = new OptionContext
        {
            User = player,
            Target = enemy,
            Value = value,
        };
        optionManager.ApplyOption(optionID, ctx);

        // 호출 후 로그 (구현체 내부에서도 로그 찍힌다)
        Debug.Log($"[Test] {optionID} 완료\n");
        //Debug.Log(ctx.User.Health);
    }
}
