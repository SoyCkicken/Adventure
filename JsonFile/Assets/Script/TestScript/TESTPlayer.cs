using System.Collections;
using UnityEngine;

public class TESTPlayer : MonoBehaviour
{
    public string playerName;
    public int MaxHP = 500;
    public int CurrentHP;
    public int AttackPower = 30;

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

        target.DamagePart(partName, AttackPower);
        Debug.Log($"[Player] {partName} 부위에 {AttackPower} 데미지를 입혔습니다.");
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
