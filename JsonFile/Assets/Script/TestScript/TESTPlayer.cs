//using MyGame;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//public class TESTPlayer : MonoBehaviour
//{
//    public BossPartCombatManager BossPartCombatManager;
//    public string playerName;
//    public int MaxHP = 500;
//    public int CurrentHP;
//    public int AttackPower = 30;
//    public int hitChance = 80; // 명중률 (0~100)

//    public List<FocusBuffData> ActiveDebuffs = new();

//    public bool IsDead => CurrentHP <= 0;

//    void Start()
//    {

//        CurrentHP = MaxHP;
//    }

//    public void OnTurnEnd()
//    {
//        List<FocusBuffData> expired = new();

//        foreach (var buff in ActiveDebuffs)
//        {
//            buff.Elapsed += 1f;

//            if (buff.OptionID == "Option_003") // 화상
//            {
//                int damage = Mathf.FloorToInt(MaxHP * (buff.Value / 100f));
//                CurrentHP = Mathf.Max(CurrentHP - damage, 0);
//                Debug.Log($"🔥 [플레이어 화상 피해] {damage} 데미지");
//            }

//            if (buff.Elapsed >= buff.Duration)
//            {
//                expired.Add(buff);
//                Debug.Log($"[버프 만료] {buff.OptionID}");
//            }
//        }

//        foreach (var b in expired)
//            ActiveDebuffs.Remove(b);
//    }

//    public void AddBuff(FocusBuffData newBuff)
//    {
//        var existing = ActiveDebuffs.FirstOrDefault(b => b.OptionID == newBuff.OptionID);
//        if (existing != null)
//        {
//            existing.Elapsed = 0f;
//            existing.Duration = Mathf.Max(existing.Duration, newBuff.Duration);
//        }
//        else
//        {
//            ActiveDebuffs.Add(newBuff);
//        }

//        Debug.Log($"[버프 적용] {newBuff.OptionID} → 지속: {newBuff.Duration}턴");
//    }

//    /// <summary>
//    /// 보스의 특정 부위를 공격합니다.
//    /// </summary>
//    public void PerformAttack(TESTBoss target, string partName)
//    {
//        if (target == null || target.IsDead) return;

//        if (!target.CanAttackPart(partName)) return;

//        int evade = target.GetEvadeRate(partName);
//        int roll = Random.Range(0, 100);

//        Debug.Log($"[Player] 명중 굴림: {roll} vs 명중 필요치: {hitChance - evade}");    

//        if (roll >= (hitChance - evade))
//        {
//            BossPartCombatManager.Log($"[Player] {partName} 부위를 공격했지만 빗나갔습니다!\n");
//            BossPartCombatManager.PlayDodgeSound();
//            return;
//        }

//        target.DamagePart(partName, AttackPower);
//        BossPartCombatManager.Log($"[Player] {partName} 부위에 {AttackPower} 데미지 적중!\n");
//        BossPartCombatManager.PlayHitSound();

//    }

//    /// <summary>
//    /// 보스에게 공격 당했을 때 체력 감소 처리
//    /// </summary>
//    public void TakeDamage(int amount)
//    {
//        CurrentHP -= amount;
//        CurrentHP = Mathf.Max(CurrentHP, 0);
//        Debug.Log($"[Player] 피해: -{amount}, 현재 체력: {CurrentHP}");
//    }
//}
using MyGame;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TESTPlayer : MonoBehaviour
{
    public BossPartCombatManager BossPartCombatManager;
    public string playerName;
    public int MaxHP = 500;
    public int CurrentHP;
    public int AttackPower = 30;
    public int hitChance = 80; // 명중률 (0~100)

    public List<FocusBuffData> ActiveDebuffs = new();

    public bool IsDead => CurrentHP <= 0;

    void Start()
    {
        CurrentHP = MaxHP;
    }

    /// <summary>
    /// 플레이어의 턴 종료 시 디버프 효과 적용 및 갱신
    /// </summary>
    public void TickDebuffs()
    {
        List<FocusBuffData> expired = new();

        foreach (var buff in ActiveDebuffs)
        {
            buff.Elapsed += 1f;

            ApplyBuffEffect(buff);

            if (buff.Elapsed >= buff.Duration)
            {
                expired.Add(buff);
                Debug.Log($"[버프 만료] {buff.OptionID}");
            }
        }

        foreach (var b in expired)
            ActiveDebuffs.Remove(b);
    }

    /// <summary>
    /// 디버프 효과 적용 처리
    /// </summary>
    private void ApplyBuffEffect(FocusBuffData buff)
    {
        if (buff.OptionID == "Option_003") // 화상 예시
        {
            int damage = Mathf.FloorToInt(MaxHP * (buff.Value / 100f));
            TakeDamage(damage, "화상");
            Debug.Log($"🔥 [플레이어 화상 피해] {damage} 데미지");
        }

        // TODO: 다른 디버프 효과도 여기에 추가
    }

    /// <summary>
    /// 디버프 추가 또는 갱신
    /// </summary>
    public void AddBuff(FocusBuffData newBuff)
    {
        var existing = ActiveDebuffs.FirstOrDefault(b => b.OptionID == newBuff.OptionID);
        if (existing != null)
        {
            existing.Elapsed = 0f;
            existing.Duration = newBuff.Duration;
            Debug.Log($"[버프 갱신] {newBuff.OptionID} → 지속 {newBuff.Duration}턴");
        }
        else
        {
            ActiveDebuffs.Add(newBuff);
            Debug.Log($"[버프 적용] {newBuff.OptionID} → 지속 {newBuff.Duration}턴");
        }
    }

    /// <summary>
    /// 보스의 특정 부위를 공격
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
    public void TakeDamage(int amount, string source = "직접 피해")
    {
        CurrentHP -= amount;
        CurrentHP = Mathf.Max(CurrentHP, 0);
        Debug.Log($"[Player] 피해: -{amount} ({source}), 현재 체력: {CurrentHP}");
    }
}
