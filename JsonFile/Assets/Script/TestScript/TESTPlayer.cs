using System.Collections;
using UnityEngine;

public class TESTPlayer : MonoBehaviour
{
    public BossPartCombatManager BossPartCombatManager;
    public string playerName;
    public int MaxHP = 500;
    public int CurrentHP;
    public int AttackPower = 30;
    public int hitChance = 80; // 명중률 (0~100)

    public bool IsDead => CurrentHP <= 0;

    void Start()
    {
        CurrentHP = MaxHP;
    }

    /// <summary>
    /// 보스의 특정 부위를 공격합니다.
    /// </summary>
    public void PerformAttack(TESTBoss target, string partName)
    {
        if (target == null || target.IsDead) return;

        if (!target.CanAttackPart(partName)) return;

        int evade = target.GetEvadeRate(partName);
        int roll = Random.Range(0, 100);

        Debug.Log($"[Player] 명중 굴림: {roll} vs 명중 필요치: {hitChance - evade}");    

        if (roll >= (hitChance - evade))
        {
            BossPartCombatManager.Log($"[Player] {partName} 부위를 공격했지만 빗나갔습니다!\n");
            BossPartCombatManager.PlayDodgeSound();
            return;
        }

        target.DamagePart(partName, AttackPower);
        BossPartCombatManager.Log($"[Player] {partName} 부위에 {AttackPower} 데미지 적중!\n");
        BossPartCombatManager.PlayHitSound();
    }

    /// <summary>
    /// 보스에게 공격 당했을 때 체력 감소 처리
    /// </summary>
    public void TakeDamage(int amount)
    {
        CurrentHP -= amount;
        CurrentHP = Mathf.Max(CurrentHP, 0);
        Debug.Log($"[Player] 피해: -{amount}, 현재 체력: {CurrentHP}");
    }
}
