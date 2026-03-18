using MyGame;
using Spine;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

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
                slotName = "M_jombie_" + "LeftArm";
            }
            else if (partName == "오른쪽 팔")
            {
                slotName = "M_jombie_" + "RightArm";
            }
            else if (partName == "왼쪽 다리")
            {
                slotName = "M_jombie_" + "LeftLeg";
            }
            else if (partName == "오른쪽 다리")
            {
                slotName = "M_jombie_RightLeg";
                slotName = "M_jombie_" + "RightLeg";
            }
            else if (partName == "몸통")
            {
                slotName = "M_jombie_Body";
                slotName = "M_jombie_" + "Body";
            }
            else if (partName == "머리")
            {
                slotName = "M_jombie_Head";
                slotName = "M_jombie_" + "Head";
            }
            else if (partName == "꼬리")
            {
                slotName = "M_jombie_Tail";
                slotName = "M_jombie_" + "Tail";
            }
                // Spine 슬롯명 규칙 (필요 시 수정)
                // Update the initialization of PartInfo in the InitializePartsFromHitbox method to include all required parameters.
                var part = new PartInfo(
                   partName, // 파츠이름
                   slotName, // 슬롯이름
                     100,      // 최대 HP (임시값, 필요 시 조정)
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
    [SerializeField] public int MaxTotalHP;
    [SerializeField] public int CurrentTotalHP;
    public int hitChance = 80;
    public int GetEXP;

    [SerializeField] public readonly string[] armGroupKeys = { "팔" };
    [SerializeField] public readonly string[] legGroupKeys = { "다리" };

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

                // 1. Spine 슬롯 비활성화 (이미 잘 처리됨)
                skeleton.FindSlot(part.SlotName).Attachment = null;

                // 2. EnemyHitbox 오브젝트 비활성화 처리
                var hitbox = GetComponentsInChildren<EnemyHitbox>(true)
                             .FirstOrDefault(hb => hb.logicalPartName == part.partName);
                if (hitbox != null)
                {
                    hitbox.gameObject.SetActive(false); // 또는 hitbox.enabled = false;
                    Debug.Log($"[Boss] {part.partName}의 콜라이더 오브젝트 비활성화됨");
                }

                // 3. 팔/다리 체크
                CheckArmCondition();
                CheckLegCondition();
            };

            // 예외 처리: 머리 파괴 → 즉사
            if (part.partName.Contains("머리"))
            {
                part.OnBreak = () =>
                {
                    Debug.Log("[Boss] 머리 부위가 파괴되었습니다. 즉사 처리됨!\n");

                    skeleton.FindSlot(part.SlotName).Attachment = null;

                    var hitbox = GetComponentsInChildren<EnemyHitbox>(true)
                                 .FirstOrDefault(hb => hb.logicalPartName == part.partName);
                    if (hitbox != null)
                    {
                        hitbox.gameObject.SetActive(false);
                        Debug.Log($"[Boss] {part.partName}의 콜라이더 오브젝트 비활성화됨");
                    }

                    Kill();
                    PlayDeathAnimation();
                };
            }

            parts[part.partName] = part;
            Debug.Log(part.SlotName);
        }

        //OnEnemyTurnEnd();
    }
    
    public void Init()
    {
        // 총 체력 설정
        // 부위 초기화
        foreach (var part in partList)
        {
            part.CurrentHP = part.MaxHP;
            part.ActiveDebuffs.Clear();
            MaxTotalHP = Mathf.Max(MaxTotalHP, part.MaxHP);
            CurrentTotalHP = MaxTotalHP;
            // OnBreak 이벤트 설정
            part.OnBreak = () =>
            {
                Debug.Log($"[Boss] {part.partName} 부위가 파괴되었습니다!");

                // Spine 슬롯 제거
                skeletonAnimation.Skeleton.FindSlot(part.SlotName).Attachment = null;

                // 콜라이더 비활성화
                var hitbox = GetComponentsInChildren<EnemyHitbox>(true)
                             .FirstOrDefault(hb => hb.logicalPartName == part.partName);
                if (hitbox != null)
                    hitbox.gameObject.SetActive(false);

                // 패널티 조건 검사
                CheckArmCondition();
                CheckLegCondition();
            };

            // 특수: 머리 파괴 → 즉사
            if (part.partName.Contains("머리"))
            {
                part.OnBreak = () =>
                {
                    Debug.Log("[Boss] 머리 부위 파괴 → 즉사 처리");

                    skeletonAnimation.Skeleton.FindSlot(part.SlotName).Attachment = null;

                    var hitbox = GetComponentsInChildren<EnemyHitbox>(true)
                                 .FirstOrDefault(hb => hb.logicalPartName == part.partName);
                    if (hitbox != null)
                        hitbox.gameObject.SetActive(false);

                    Kill();
                    PlayDeathAnimation();
                };
            }

            parts[part.partName] = part;
        }

        Debug.Log($"[Boss Init] {partList.Count}개 부위 초기화 완료");
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
        if (armParts == null || armParts.Count == 0)
        {
            Debug.Log("[Boss] 팔 부위가 없어 패널티를 적용하지 않습니다.");
            return;
        }
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
        if (legParts == null || legParts.Count == 0)
        {
            Debug.Log("[Boss] 다리 부위가 없어 패널티를 적용하지 않습니다.");
            return;
        }
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
        return MaxTotalHP-((float)MaxTotalHP - CurrentTotalHP);
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

    public void PerformAttack(Character target)
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
        public Action<bool> isComplete;

        public bool IsBroken => CurrentHP <= 0;

        public PartInfo(string name, string slotName,int hp,System.Action onBreak = null)
        {
            partName = name;
            SlotName = slotName;
            OnBreak = onBreak;
            MaxHP = hp;
            CurrentHP = hp;
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
