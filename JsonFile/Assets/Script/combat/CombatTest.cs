//using UnityEngine;
//using MyGame;
//using System;
//using System.Collections;


//public class CombatTest : MonoBehaviour
//{
//    public OptionManager optionManager;
//    public Character player;
//    public Character enemy;
//    private bool battleOver;

//    private Action<bool> onComplete;

//    private void Awake()
//    {
//        //SetupDummy(enemy, speed: 0.8f, armor: 1, baseDmg: 8,
//        //          optID: null, optVal: 0, 10);
//    }

//    public void RunBattle(Action<bool> onComplete)
//    {
//        this.onComplete = onComplete;
//        StartCoroutine(ProcessBattle());
//    }

//    public void StopBattle()
//    {
//        StopAllCoroutines();
//        // UI 정리나 이펙트 정리 등
//    }


//    void Start()
//    {
//        // OptionManager 참조 확보
//        optionManager = optionManager != null
//            ? optionManager
//            : FindObjectOfType<OptionManager>();

//        // 유저/타겟 캐릭터 더미 생성
//        battleOver = false;

//        // 각 옵션을 테스트해본다
//        //StartCoroutine(BattleLoop());

//    }
//    void SetupDummy(Character c, float speed, int armor, int baseDmg, string optID, int optVal, int CitChance)
//    {
//        c.speed = speed;
//        c.armor = armor;
//        c.damage = baseDmg;
//        c.Health = 1000;
//        c.charaterName = c.gameObject.name;
//        c.CitChance = CitChance;

//        // 플레이어만 옵션을 쓸 거라면, Character에 아래 필드만 추가해두고 세팅
//    }
//    IEnumerator AttackLoop(Character attacker, Character target, bool isPlayer)
//    {
//        // 전투 시작 전에 잠깐 딜레이 주고 싶으면 여기에 yield return new WaitForSeconds(0.2f);
//        while (!battleOver)
//        {
//            // 속도에 따른 대기
//            yield return new WaitForSeconds(1f / attacker.speed);

//            if (battleOver) yield break;

//            // 실제 공격 (크리티컬 / 방어구 경감 모두 Character.Attack() 안에서 처리)
//            int dealt = attacker.Attack(target);

//            // 플레이어만 OnHit 옵션 적용
//            if (isPlayer && attacker.OnHitOptions != null)
//            {
//                foreach (var opt in attacker.OnHitOptions)
//                {
//                    var ctx = new OptionContext
//                    {
//                        User = attacker,
//                        Target = target,
//                        option_ID = opt.OptionID,
//                        Value = opt.Value,
//                    };
//                    optionManager.ApplyOption(opt.OptionID, ctx);
//                }
//            }

//            // 누가 죽었는지 체크
//            if (target.Health <= 0)
//            {
//                Debug.Log($"전투 종료! 승자: {attacker.charaterName}");
//                if (player.Health <= 0)
//                {
//                    battleOver = false;
//                }
//                else
//                {
//                    battleOver = true;
//                }
//                yield break;
//            }

//        }
//    }



//    void TestOption(string optionID, int value, int dealt, int turn)
//    {
//        // 호출 전 로그
//        Debug.Log($"[Test] {optionID} ▶ Value={value}, Dealt={dealt}, Turn={turn}");

//        // 컨텍스트 채우고 호출
//        var ctx = new OptionContext
//        {
//            User = player,
//            Target = enemy,
//            Value = value,
//        };
//        optionManager.ApplyOption(optionID, ctx);

//        // 호출 후 로그 (구현체 내부에서도 로그 찍힌다)
//        Debug.Log($"[Test] {optionID} 완료\n");
//        //Debug.Log(ctx.User.Health);
//    }

//    private IEnumerator ProcessBattle()
//    {
//        StartCoroutine(AttackLoop(player, enemy, true));
//        StartCoroutine(AttackLoop(enemy, player, false));
//        yield return new WaitForSeconds(3f);  // 예시
//        onComplete?.Invoke(battleOver);
//    }
//}


using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MyGame;

public class CombatTest : MonoBehaviour
{
    public OptionManager optionManager;
    public Character player;
    public Character enemy;

    // 전투 완료 콜백
    private Action<bool> onComplete;
    private bool battleOver;

    /// <summary>
    /// GameFlowManager가 전투를 시작할 때 호출합니다.
    /// </summary>
    public void RunBattle(Action<bool> onComplete)
    {
        this.onComplete = onComplete;

        // 옵션 매니저 참조 확보
        if (optionManager == null)
            optionManager = FindObjectOfType<OptionManager>();

        // 전투 상태 초기화
        battleOver = false;
        player.Health = player.MaxHealth;
        enemy.Health = enemy.MaxHealth;

        // 실제 전투 코루틴 실행
        StartCoroutine(ProcessBattle());
    }

    /// <summary>
    /// GameFlowManager가 전투를 중단할 때 호출합니다.
    /// </summary>
    public void StopBattle()
    {
        // 진행 중인 모든 코루틴 정지
        StopAllCoroutines();
        battleOver = true;
    }

    private IEnumerator ProcessBattle()
    {
        // 두 캐릭터의 공격루프를 동시에 돌리고,
        // 둘 다 끝날 때까지 대기했다가 onComplete 호출
        var playerLoop = StartCoroutine(AttackLoop(player, enemy, true));
        var enemyLoop = StartCoroutine(AttackLoop(enemy, player, false));

        yield return playerLoop;
        yield return enemyLoop;

        onComplete?.Invoke(battleOver);
    }

    private IEnumerator AttackLoop(Character attacker, Character target, bool isPlayer)
    {
        while (!battleOver)
        {
            yield return new WaitForSeconds(1f / attacker.speed);
            if (battleOver) yield break;

            int dealt = attacker.Attack(target);

            // 플레이어 온히트 옵션 적용
            if (isPlayer && attacker.OnHitOptions != null)
            {
                foreach (var opt in attacker.OnHitOptions)
                {
                    var ctx = new OptionContext
                    {
                        User = attacker,
                        Target = target,
                        option_ID = opt.OptionID,
                        Value = opt.Value
                    };
                    optionManager.ApplyOption(opt.OptionID, ctx);
                }
            }

            // 죽음 판정
            if (target.Health <= 0)
            {
                // attacker가 살아 있으면 attacker 승리
                battleOver = (player.Health > 0);
                yield break;
            }
        }
    }
}
