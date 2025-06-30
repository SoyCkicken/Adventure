using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TESTBoss : MonoBehaviour
{
    [Header("기본 스탯")]
    public BossPartCombatManager BossPartCombatManager;
    public string bossName;
    public int attackPower = 50;
    public int MaxTotalHP;
    public int CurrentTotalHP;
    public int hitChance = 80;

    // 그룹 키워드 (팔, 다리 등)
    private readonly string[] armGroupKeys = { "팔" };
    private readonly string[] legGroupKeys = { "다리" };

    [Header("부위 정보")]
    public List<PartInfo> partList = new();

    private Dictionary<string, MonsterPart> parts = new();
    public bool IsDead => CurrentTotalHP <= 0;
    private bool isAttackDisabled = false;

    void Start()
    {
        CurrentTotalHP = MaxTotalHP;

        foreach (var part in partList)
        {
            if (part.partName.Contains("머리"))
            {
                RegisterPart(part.partName, part.hp, part.EvadeRate, () =>
                {
                    BossPartCombatManager.Log("[Boss] 머리 부위가 파괴되었습니다. 즉사 처리됨!\n");
                    Kill();
                    PlayDeathAnimation();
                });
            }
            else
            {
                RegisterPart(part.partName, part.hp, part.EvadeRate, () =>
                {
                    BossPartCombatManager.Log($"[Boss] {part.partName} 부위가 파괴되었습니다!\n");

                    // 파괴 조건 체크
                    CheckArmCondition();
                    CheckLegCondition();
                });
            }
        }
    }

    private List<string> GetPartNamesContaining(params string[] keywords)
    {
        List<string> result = new();
        foreach (var kv in parts)
        {
            foreach (var keyword in keywords)
            {
                if (kv.Key.Contains(keyword))
                {
                    result.Add(kv.Key);
                    break;
                }
            }
        }
        return result;
    }

    private void CheckArmCondition()
    {
        var armParts = GetPartNamesContaining(armGroupKeys);
        Debug.Log(armParts.Count);
        bool allArmsBroken = armParts.All(partName => IsPartBroken(partName));

        if (allArmsBroken)
        {
            isAttackDisabled = true;
            BossPartCombatManager.Log("[Boss] 모든 팔이 파괴되어 공격할 수 없습니다.\n");
        }
    }

    private void CheckLegCondition()
    {
        var legParts = GetPartNamesContaining(legGroupKeys);
        Debug.Log(legParts.Count);
        bool allLegsBroken = legParts.All(partName => IsPartBroken(partName));

        if (allLegsBroken && parts.ContainsKey("머리"))
        {
            parts["머리"].EvadeRate = 0;
            BossPartCombatManager.Log("[Boss] 모든 다리가 파괴되어 머리 회피율이 0%로 변경되었습니다.\n");
        }
    }   

    public void RegisterPart(string name, int hp, int evadeRate, System.Action onBreak)
    {
        parts[name] = new MonsterPart(name, hp, evadeRate, onBreak);
    }

    public void DamagePart(string name, int amount)
    {
        if (!parts.ContainsKey(name)) return;
        if (IsDead) return;

        parts[name].Damage(amount);
        CurrentTotalHP -= amount;
        CurrentTotalHP = Mathf.Max(CurrentTotalHP, 0);
    }

    public void Kill()
    {
        CurrentTotalHP = 0;
        Debug.Log("보스가 사망하였습니다.");
    }

    public float GetPartHPPercent(string name)
    {
        if (parts.ContainsKey(name))
            return parts[name].CurrentHP / (float)parts[name].MaxHP;
        return 0f;
    }

    public float GetTotalHPPercent()
    {
        return CurrentTotalHP / (float)MaxTotalHP;
    }

    public bool CanAttackPart(string name)
    {
        return parts.ContainsKey(name) && !parts[name].IsBroken;
    }

    public bool IsPartBroken(string name)
    {
        return parts.ContainsKey(name) && parts[name].IsBroken;
    }

    public int GetEvadeRate(string partName)
    {
        if (!parts.ContainsKey(partName)) return 0;
        return parts[partName].EvadeRate;
    }

    public List<string> GetAttackableParts()
    {
        List<string> result = new();
        foreach (var kv in parts)
        {
            if (!kv.Value.IsBroken)
                result.Add(kv.Key);
        }
        return result;
    }

    public void PerformAttack(TESTPlayer target)
    {
        if (isAttackDisabled)
        {
            Debug.Log("[Boss] 모든 팔이 파괴되어 공격할 수 없습니다.");
            return;
        }

        if (target == null || target.IsDead) return;

        int roll = Random.Range(0, 100);
        Debug.Log($"[Boss] 명중 굴림: {roll} vs 명중 필요치: {hitChance}");

        if (roll >= hitChance)
        {
            Debug.Log("[Boss] 공격이 빗나갔습니다.");
            return;
        }

        target.TakeDamage(attackPower);
        Debug.Log($"[Boss] 플레이어에게 {attackPower} 데미지 적중!");
    }

    public void PlayDeathAnimation()
    {
        var skeletonAnim = GetComponent<SkeletonAnimation>();
        if (skeletonAnim != null)
        {
            skeletonAnim.AnimationState.SetEmptyAnimation(0, 0.2f);
            Debug.Log("[Boss] 죽음 애니메이션 실행");
        }
    }

    [System.Serializable]
    public class PartInfo
    {
        public string partName;
        public int hp;
        public string slotName;
        public int EvadeRate = 0;
    }

    public class MonsterPart
    {
        public string Name;
        public int MaxHP;
        public int CurrentHP;
        public int EvadeRate;
        public System.Action OnBreak;
        public bool IsBroken => CurrentHP <= 0;

        public MonsterPart(string name, int hp, int evadeRate, System.Action onBreak)
        {
            Name = name;
            MaxHP = hp;
            CurrentHP = hp;
            EvadeRate = evadeRate;
            OnBreak = onBreak;
        }

        public void Damage(int amount)
        {
            if (IsBroken) return;

            CurrentHP -= amount;
            CurrentHP = Mathf.Max(CurrentHP, 0);

            if (IsBroken)
            {
                OnBreak?.Invoke();
            }
        }
    }
}
