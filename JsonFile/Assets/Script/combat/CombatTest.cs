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
