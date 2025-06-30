// [1] BossPartCombatManager.cs
using Spine;
using Spine.Unity;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossPartCombatManager : MonoBehaviour
{
    public TMP_Text logText;
    public Slider armSlider;
    public Slider legSlider;
    public Slider headSlider;
    public Slider totalHPSlider;
    public SkeletonAnimation BossSkeleton;
    public TESTBoss testBoss;
    public TESTPlayer testPlayer;
    private bool isPlayerTurn = true;


    /// <summary>
    /// 여기서 적과 플레이어에 대해서 정보를 넣고 있는데 이 부분 수정해서 Boss에서 Player에서 정보 넣는 식으로 할 예정
    /// </summary>
    void Start()
    {
        UpdateSliders();
        Log("플레이어의 턴입니다.");
    }

    public void AttackPart(string partName)
    {
        if (!isPlayerTurn)
        {
            Log("지금은 플레이어의 턴이 아닙니다.");
            return;
        }

        if (!testBoss.CanAttackPart(partName))
        {
            Log($"{partName} 부위는 이미 파괴되어 공격할 수 없습니다.");
            return;
        }

        // ▶ 플레이어가 보스의 특정 부위를 공격하도록 위임
        testPlayer.PerformAttack(testBoss, partName);
        Log($"플레이어가 {partName} 부위를 공격했습니다.");

        if (testBoss.IsDead)
        {
            Log("보스를 처치했습니다!");
            testBoss.PlayDeathAnimation(); // 애니메이션도 Boss 내부로 옮김
            return;
        }

        isPlayerTurn = false;
        UpdateSliders();
        Invoke(nameof(EnemyTurn), 1.5f);
    }

    void EnemyTurn()
    {
        if (testBoss.IsDead) return;

        if (testBoss.IsPartBroken("팔")) // 보스 내부에서 부위 확인하도록 구조 개선
        {
            Log("보스의 팔이 파괴되어 공격할 수 없습니다.");
            isPlayerTurn = true;
            Log("플레이어의 턴입니다.");
            return;
        }
        testBoss.PerformAttack(testPlayer);
        Log($"보스가 플레이어를 공격했습니다. ({testBoss.attackPower} 데미지)");

        if (testPlayer.IsDead)
        {
            Log("플레이어가 사망했습니다...");
            return;
        }

        isPlayerTurn = true;
        Log("플레이어의 턴입니다.");
    }


    void UpdateSliders()
    {
        armSlider.value = testBoss.GetPartHPPercent("팔");
        legSlider.value = testBoss.GetPartHPPercent("다리");
        headSlider.value = testBoss.GetPartHPPercent("머리");
        totalHPSlider.value = testBoss.GetTotalHPPercent();
    }

    void Log(string message)
    {
        logText.text += message + "\n";
    }
}
