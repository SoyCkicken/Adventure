using MyGame;
using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class TESTBoss : MonoBehaviour
{
    [ContextMenu("부위 자동 등록")]
    public void InitializePartsFromHitbox()
    {
        partList.Clear();
        parts.Clear();

        var hitboxes = skeletonAnimation.GetComponentsInChildren<EnemyHitbox>(true);
        foreach (var hb in hitboxes)
        {
            string partName = hb.logicalPartName;
            string slotName = "M_jombie_" + partName;
            if (partName == "왼쪽 팔")
            {
                slotName = "M_jombie_LeftArm";
            }
            else if (partName == "오른쪽 팔")
            {
                slotName = "M_jombie_RightArm";
            }
            else if (partName == "왼쪽 다리")
            {
                slotName = "M_jombie_LeftLeg";
            }
            else if (partName == "오른쪽 다리")
            {
                slotName = "M_jombie_RightLeg";
            }
                // Spine 슬롯명 규칙 (필요 시 수정)
                // Update the initialization of PartInfo in the InitializePartsFromHitbox method to include all required parameters.
                var part = new PartInfo(
                   partName, // 파츠이름
                   100,      // 체력
                   10,       // 회피율
                   slotName, // 슬롯이름
                   () =>     // 파괴 되었는지?
                   {
                       skeletonAnimation.Skeleton.FindSlot(slotName).Attachment = null;
                       Debug.Log($"[Boss] {partName} 파괴됨");
                   }
                );

            parts[partName] = part;
            partList.Add(part);
        }

        Debug.Log($"[Boss] 자동 등록 완료: 총 {partList.Count}개 부위");
    }


    [Header("기본 스탯")]
    public BossPartCombatManager BossPartCombatManager;
    public SkeletonAnimation skeletonAnimation;
    public string bossName;
    public int attackPower = 50;
    public int MaxTotalHP;
    public int CurrentTotalHP;
    public int hitChance = 80;

    [SerializeField] public readonly string[] armGroupKeys = { "Arm" };
    [SerializeField] public readonly string[] legGroupKeys = { "Leg" };

    [Header("부위 정보")]
    public List<PartInfo> partList = new();
    private Dictionary<string, PartInfo> parts = new();

    public bool IsDead => CurrentTotalHP <= 0;
    private bool isAttackDisabled = false;

    void Start()
    {
        CurrentTotalHP = MaxTotalHP;

        var skeleton = skeletonAnimation.skeleton;
        foreach (var part in partList)
        {
            part.CurrentHP = part.MaxHP;
            part.OnBreak = () =>
            {
                Debug.Log($"[Boss] {part.partName} 부위가 파괴되었습니다!\n");
                skeleton.FindSlot(part.SlotName).Attachment = null;
                CheckArmCondition();
                CheckLegCondition();
            };

            if (part.partName.Contains("머리"))
            {
                part.OnBreak = () =>
                {
                    Debug.Log("[Boss] 머리 부위가 파괴되었습니다. 즉사 처리됨!\n");
                    skeleton.FindSlot(part.SlotName).Attachment = null;
                    Kill();
                    PlayDeathAnimation();
                };
            }

            parts[part.partName] = part;
        }

        //OnEnemyTurnEnd();
    }

    public void OnEnemyTurnEnd()
    {
        foreach (var part in partList)
        {
            part.TickDebuffs(this);
        }
    }

    public void AddBuff(string partName, FocusBuffData buff)
    {
        if (!parts.TryGetValue(partName, out var part))
        {
            Debug.LogWarning($"[AddBuff] 부위 {partName} 이 존재하지 않습니다.");
            return;
        }

        var existing = part.ActiveDebuffs.FirstOrDefault(b => b.OptionID == buff.OptionID);

        if (existing != null)
        {
            existing.Elapsed = 0f;
            existing.Duration = buff.Duration;
            Debug.Log($"[Buff 갱신] {partName}에 {buff.OptionID} 버프 갱신");
        }
        else
        {
            part.ActiveDebuffs.Add(buff);
            Debug.Log($"[Buff 적용] {partName}에 {buff.OptionID} 버프 추가됨");
        }
    }

    private void CheckArmCondition()
    {
        var armParts = GetPartNamesContaining(armGroupKeys);
        bool allArmsBroken = armParts.All(partName => IsPartBroken(partName));

        if (allArmsBroken)
        {
            isAttackDisabled = true;
            Debug.Log("[Boss] 모든 팔이 파괴되어 공격할 수 없습니다.\n");
        }
    }

    private void CheckLegCondition()
    {
        var legParts = GetPartNamesContaining(legGroupKeys);
        bool allLegsBroken = legParts.All(partName => IsPartBroken(partName));

        if (allLegsBroken && parts.ContainsKey("머리"))
        {
            parts["머리"].EvadeRate = 0;
            Debug.Log("[Boss] 모든 다리가 파괴되어 머리 회피율이 0%로 변경되었습니다.\n");
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

    public void DamagePart(string name, int amount)
    {
        if (!parts.ContainsKey(name)) return;
        if (IsDead) return;

        parts[name].Damage(amount);
        CurrentTotalHP -= amount;
        CurrentTotalHP = Mathf.Max(CurrentTotalHP, 0);

        var skeletonAnim = skeletonAnimation.GetComponent<SkeletonAnimation>();
        skeletonAnim.AnimationState.SetAnimation(0, "Hit", false);

        if (CurrentTotalHP <= 0)
        {
            skeletonAnim.AnimationState.SetEmptyAnimation(0, 0.2f);
        }
        else
        {
            skeletonAnim.AnimationState.AddAnimation(0, "Idle", true, 1.0f);
        }
    }

    public void Kill()
    {
        CurrentTotalHP = 0;
        Debug.Log("보스가 사망하였습니다.");
    }

    public float GetPartHPPercent(string name)
    {
        return parts.ContainsKey(name) ? parts[name].CurrentHP / (float)parts[name].MaxHP : 0f;
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
        return parts.ContainsKey(partName) ? parts[partName].EvadeRate : 0;
    }

    public List<string> GetAttackableParts()
    {
        return parts.Where(kv => !kv.Value.IsBroken).Select(kv => kv.Key).ToList();
    }

    public void PlayDeathAnimation()
    {
        var skeletonAnim = skeletonAnimation.GetComponent<SkeletonAnimation>();
        if (skeletonAnim != null)
        {
            skeletonAnim.AnimationState.SetEmptyAnimation(0, 0.2f);
            Debug.Log("[Boss] 죽음 애니메이션 실행");
        }
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
            BossPartCombatManager.PlayDodgeSound();
            return;
        }

        target.TakeDamage(attackPower);
        Debug.Log($"[Boss] 플레이어에게 {attackPower} 데미지 적중!");
        BossPartCombatManager.PlayDamageSound();
    }
    [System.Serializable]
    public class PartInfo
    {
        public string partName;
        public string SlotName;
        public int MaxHP;
        public int CurrentHP;
        public int EvadeRate;

        public List<FocusBuffData> ActiveDebuffs = new();
        public System.Action OnBreak;

        public bool IsBroken => CurrentHP <= 0;

        public PartInfo(string name, int hp, int evadeRate, string slotName, System.Action onBreak = null)
        {
            partName = name;
            MaxHP = hp;
            CurrentHP = hp;
            EvadeRate = evadeRate;
            SlotName = slotName;
            OnBreak = onBreak;
        }

        public void Damage(int amount)
        {
            if (IsBroken) return;

            CurrentHP -= amount;
            CurrentHP = Mathf.Max(CurrentHP, 0);

            if (IsBroken)
                OnBreak?.Invoke();
        }


        public void TickDebuffs(TESTBoss boss)
        {
            List<FocusBuffData> expired = new();

            foreach (var buff in ActiveDebuffs)
            {
                buff.Elapsed += 1f;

                if (buff.OptionID == "Option_003")
                {
                    int totalDmg = Mathf.FloorToInt(buff.Target.MaxTotalHP * (buff.Value / 100f));
                    int partDmg = Mathf.FloorToInt(totalDmg * buff.DamageRatio);
                    int mainDmg = totalDmg - partDmg;

                    Damage(partDmg);
                    boss.CurrentTotalHP = Mathf.Max(boss.CurrentTotalHP - mainDmg, 0);

                    Debug.Log($"[Tick] {partName} 화상: 부위 {partDmg} / 본체 {mainDmg}");
                }

                if (buff.Elapsed >= buff.Duration)
                {
                    expired.Add(buff);
                    Debug.Log($"[버프 만료] {partName}의 {buff.OptionID}");
                }
            }

            foreach (var b in expired)
                ActiveDebuffs.Remove(b);
        }
    }
}
