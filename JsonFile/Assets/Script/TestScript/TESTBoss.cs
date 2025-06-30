using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TESTBoss : MonoBehaviour
{
    [Header("기본 스탯")]
    public string bossName;
    public int attackPower = 50;
    public int MaxTotalHP;
    public int CurrentTotalHP;

    [Header("부위 정보")]
    public List<PartInfo> partList = new();

    private Dictionary<string, MonsterPart> parts = new();
    public bool IsDead => CurrentTotalHP <= 0;

    void Start()
    {
        CurrentTotalHP = MaxTotalHP;

        foreach (var part in partList)
        {
            RegisterPart(part.partName, part.hp, () =>
            {
                Debug.Log($"[Boss] {part.partName} 부위가 파괴되었습니다.");
                // 슬롯 파괴 연동은 여기서도 가능
            });
        }
    }

    public void RegisterPart(string name, int hp, System.Action onBreak)
    {
        parts[name] = new MonsterPart(name, hp, onBreak);
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

    //공격하는 부분
    public void PerformAttack(TESTPlayer target)
    {
        if (IsPartBroken("팔"))
        {
            Debug.Log("[Boss] 팔이 부서져 공격할 수 없습니다.");
            return;
        }
        if (target == null || target.IsDead) return;

        target.TakeDamage(attackPower);
        Debug.Log($"[Boss] 플레이어에게 {attackPower} 데미지를 입혔습니다.");
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
    }

    public class MonsterPart
    {
        public string Name;
        public int MaxHP;
        public int CurrentHP;
        public System.Action OnBreak;
        public bool IsBroken => CurrentHP <= 0;

        public MonsterPart(string name, int hp, System.Action onBreak)
        {
            Name = name;
            MaxHP = hp;
            CurrentHP = hp;
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
