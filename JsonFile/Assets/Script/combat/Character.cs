using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MyGame
{
    public class Character : MonoBehaviour
    {

        //private void Update()
        //{
        //    UpdateBuffs(Time.deltaTime);
        //}

        private void StartBuffRoutine()
        {
            if (buffCoroutine != null) return;
            buffCoroutine = StartCoroutine(BuffTickRoutine());
        }

        [Header("캐릭터 기본 정보입니다 여기 값 + 능력치를 적용 시킬 예정입니다")]
        public GameFlowManager gameFlowManager;
        public string charaterName;
        [SerializeField] PlayerState playerState;
        public int MaxHealth = 50;
        public int Health;
        public int damage;
        public float speed;
        public int armor;
        public int CitChance = 10; //일반적인 크리티컬 확률
        public int GetEXP;
      
        public string weapon_Name;
        public string armor_Name;
        public string MonPas_Effect1;
        public int MonPas_Value1;
        public string MonPas_Effect2;
        public int MonPas_Value2;
        //버프 코루틴 용
        private Coroutine buffCoroutine;
        //옵션들 리스트에 기록
        public List<EquippedOption> OnHitOptions = new List<EquippedOption>();
        public List<MonsterOption> OnEnemyHitOptions = new List<MonsterOption>();
        public Dictionary<string, BuffData> activeBuffs = new();
        public BattleUI battleUI;
        public BuffUI buffUI;
        private Coroutine uiRefreshRoutine;

        [Header("집중전투 전용")]
        public BossPartCombatManager BossPartCombatManager;
        public int MaxHP;
        public int CurrentHP;
        public int AttackPower = 30;
        public int hitChance = 80; // 명중률 (0~100)

        public bool IsDead => CurrentHP <= 0;

        public List<FocusBuffData> ActiveDebuffs = new();
        //이 부분은 캐릭터 클래스의 하위에 있어야 되는 부분이라서 맨 아래로 안배고 맨 위에 넣음

        private void Start()
        {
            playerState = PlayerState.Instance;
        }
        [System.Serializable]
        public struct EquippedOption
        {
            public string OptionID;
            public int Value;
            public string item_ID;
        }

        private bool IsStillEquipped(BuffData buff)
        {
            if (!buff.IsPassive) return false;

            return buff.SourceItemID == armor_Name || buff.SourceItemID == weapon_Name;
        }

        [System.Serializable]
        public struct MonsterOption
        {
            public string OptionID;
            public int Value;
        }
        // 방어구만큼 경감하고 남은 데미지를 HP에서 깎는다
        public int TakeDamage(int damage)
        {
            int reduced = Mathf.Max(damage - armor, 0);
            Health -= reduced;
            Debug.Log($"{charaterName}이(가) 받는 데미지: {damage} → 방어구 {armor} 경감 → 실제 {reduced}. 현재 HP: {Health}");
            return reduced;
        }

        // 기본 공격 메서드
        public (int dealtDamage, bool isCrit) Attack(Character target)
        {
            Debug.Log(damage);
            Debug.Log($"{charaterName}이(가) {target.charaterName}을(를) 공격: {damage} 데미지 시도");

            bool isCrit = UnityEngine.Random.Range(0, 100) < CitChance;
            int finalDamage = isCrit ? damage * 2 : damage;

            Debug.Log($"{isCrit} , {CitChance} ");
            Debug.Log(finalDamage);

            int dealtDamage = target.TakeDamage(finalDamage);
            return (dealtDamage, isCrit);
        }
        //원래 코드
        public void AddBuff(BuffData buff)
        {
            Debug.Log($"버프를 누가 사용중인가 {this.name}");
            activeBuffs[buff.BuffID] = buff;
            //여기는 버프에 대한 설명만 작성 해주면 됨
            if (buff.OptionID == "Option_002") // 치명타 확률 증가
            {
                Debug.Log($"치명타 확률 +{buff.Value}% 버프 적용됨");
                CitChance += buff.Value;
            }
            if (buff.OptionID == "Option_003")
            {
                int Damagehp = Convert.ToInt32(buff.Target.MaxHealth * 0.02);
                buff.Target.Health -= Damagehp;
                //StartCoroutine(BurnDebuff(buff));
                Debug.Log($"화상 디버프 적용중 : {buff.Target}에게 화상 피해 : {Damagehp}를 적용중입니다");
            }
            if (buff.OptionID == "Option_004")
            {
                int Healinghp = Convert.ToInt32(buff.Target.MaxHealth * 0.02);
                //StartCoroutine(HealthingBuff(buff));
                Debug.Log($"회복 버프 적용중 : {buff.Target}에게 체력 회복 : {Healinghp}를 적용중입니다");
            }
            if (buff.OptionID == "Option_005")
            {
                float multiplier = buff.Value / 100f;
                speed *= (1f + multiplier);  // 예: 20% → speed *= 1.2f
                Debug.Log($"[공속 증가] {charaterName} → speed x{1f + multiplier} = {speed}");
                Debug.Log($"공격 속도 버프 {buff.Value}%만큼 증가");
            }
            Debug.LogWarning($"누가 버프가 추가 되고 있는가? {this}");

            if (uiRefreshRoutine != null) StopCoroutine(uiRefreshRoutine);
            uiRefreshRoutine = StartCoroutine(DelayUIRefresh());
            StartBuffRoutine();
            // 필요 시 스탯 반영
        }
        private IEnumerator BuffTickRoutine()
        {
            WaitForSeconds wait = new WaitForSeconds(1f);

            if (gameFlowManager == null || gameFlowManager.GetCurrentFlowState() != GameFlowManager.FlowState.Battle)
            {
                Debug.Log($"[버프 무시] 현재 상태가 Battle이 아님 → 버프 적용 안 함");
                yield return wait;
            }

            while (true)
            {
                var expired = new List<string>();

                foreach (var kv in activeBuffs)
                {
                    var buff = kv.Value;

                    // ✅ 장착 중인 패시브는 시간 초기화 (무한 유지)
                    if (IsStillEquipped(buff))
                    {
                        buff.Elapsed = 0f;
                        continue;
                    }

                    buff.Elapsed += 1f;

                    // Tick 효과
                    switch (buff.OptionID)
                    {
                        case "Option_003": // 화상
                            int dmg = Mathf.FloorToInt(buff.Target.MaxHealth * 0.02f);
                            buff.Target.Health -= dmg;
                            Debug.Log($"[화상 Tick] {buff.Target.charaterName} → {dmg} 피해");
                            break;

                        case "Option_004": // 회복
                            int heal = Mathf.FloorToInt(buff.Target.MaxHealth * 0.02f);
                            buff.Target.Health = Mathf.Min(buff.Target.MaxHealth, buff.Target.Health + heal);
                            Debug.Log($"[회복 Tick] {buff.Target.charaterName} → {heal} 회복");
                            break;
                    }

                    if (buff.Elapsed >= buff.Duration)
                        expired.Add(kv.Key);
                }

                foreach (var key in expired)
                    RemoveBuff(key);

                if (activeBuffs.Count == 0)
                {
                    buffCoroutine = null;
                    yield break;
                }

                battleUI.UpdateUI();
                yield return wait;
            }
        }


        //프레임 대기
        private IEnumerator DelayUIRefresh()
        {
            yield return null; // 1 프레임 대기 (모든 버프 적용 후)
            buffUI?.SetBuffs(activeBuffs.Values.ToList(), this);
            uiRefreshRoutine = null;
        }

        //장착 해제 시 버프 해제
        public void RemoveBuffByItem(string itemID)
        {
            // itemID가 일치하는 버프만 먼저 찾음
            var matchingBuffs = activeBuffs
                .Where(kv => kv.Value.SourceItemID == itemID)
                .ToList(); // 딕셔너리 순회 중 수정 방지

            foreach (var kv in matchingBuffs)
            {
                var buff = kv.Value;

                // 스탯 복원 처리
                if (buff.OptionID == "Option_002")
                    CitChance -= buff.Value;

                if (buff.OptionID == "Option_005")
                {
                    float multiplier = buff.Value / 100f;
                    speed /= (1f + multiplier);
                    Debug.Log($"[공속 복원] {charaterName} → speed 복구 = {speed}");
                }

                activeBuffs.Remove(kv.Key);
                Debug.Log($"[장비 해제] 버프 제거됨: {kv.Key}");
                buffUI.ClearBuffByID(buff.BuffID); // UI에서 해당 버프 아이콘 제거
            }

            buffUI?.SetBuffs(activeBuffs.Values.ToList(), this);
        }

        //버프 제거용
        //public void RemoveBuff(string buffID)
        //{
        //    if (!activeBuffs.TryGetValue(buffID, out var buff))
        //    {
        //        Debug.LogWarning($"[RemoveBuff] 존재하지 않는 버프: {buffID}");
        //        return;
        //    }

        //    // 패시브 버프는 제거하지 않음
        //    if (buff.IsPassive)
        //    {
        //        Debug.Log($"[패시브 유지] {buffID} 은(는) 패시브로 유지됨");
        //        return;
        //    }

        //    // 스탯 복원
        //    switch (buff.OptionID)
        //    {
        //        case "Option_002": // 치명타 확률
        //            CitChance -= buff.Value;
        //            Debug.Log($"[RemoveBuff] 치명타 확률 -{buff.Value} → {CitChance}");
        //            break;

        //        case "Option_005": // 공속 증가 → 원래 속도로 복원
        //            float multiplier = buff.Value / 100f;
        //            speed /= (1f + multiplier);
        //            Debug.Log($"[RemoveBuff] 공속 복구 → speed = {speed}");
        //            break;

        //            // 필요 시 다른 OptionID도 추가 가능
        //    }

        //    activeBuffs.Remove(buffID);
        //    Debug.Log($"[RemoveBuff] 버프 제거 완료: {buffID}");

        //    // UI 갱신
        //    buffUI?.SetBuffs(activeBuffs.Values.ToList(), this);
        //}
        public void RemoveBuff(string buffID)
        {
            if (!activeBuffs.TryGetValue(buffID, out var buff))
                return;

            if (buff.IsPassive)
            {
                Debug.Log($"[패시브 버프 유지됨] {buffID} , {armor_Name}");
                return;
            }

            activeBuffs.Remove(buffID);

            // UI에서 제거
            buffUI?.ClearBuffByID(buffID);
        }

        //일시적인 버프 모두 제거
        public void RemoveTemporaryBuffs()
        {
            var toRemove = activeBuffs
                .Where(kv => !kv.Value.IsPassive) // 패시브가 아닌 것만 제거
                .Select(kv => kv.Key)
                .ToList();

            foreach (var key in toRemove)
            {
                RemoveBuff(key);
                Debug.Log($"[전투 종료] 일시적 버프 제거됨: {key}");
            }
        }

        public void TickDebuffs()
        {
            List<FocusBuffData> expired = new();

            foreach (var buff in ActiveDebuffs)
            {
                buff.Elapsed += 1f;

                ApplyFocusBuffEffect(buff);

                if (buff.Elapsed >= buff.Duration)
                {
                    expired.Add(buff);
                    Debug.Log($"[버프 만료] {buff.OptionID}");
                }
            }

            foreach (var b in expired)
                ActiveDebuffs.Remove(b);
        }


        // 디버프 효과 적용 처리
        private void ApplyFocusBuffEffect(FocusBuffData buff)
        {
            if (buff.OptionID == "Option_003") // 화상
            {
                int damage = Mathf.FloorToInt(MaxHP * (buff.Value / 100f));
                TakeDamage(damage, "화상");
                Debug.Log($"🔥 [플레이어 화상 피해] {damage} 데미지");
            }

            // 다른 디버프 효과도 여기에 추가
        }

        // 디버프 추가 또는 갱신
        public void AddFocusBuff(FocusBuffData newBuff)
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

        // 보스의 특정 부위를 공격
        public void PerformAttack(TESTBoss target, string partName)
        {
            if (target == null || target.IsDead) return;
            if (!target.CanAttackPart(partName)) return;

            int evade = target.GetEvadeRate(partName);
            int roll = Random.Range(0, 100);

            Debug.Log($"[Player] 명중 굴림: {roll} vs 명중 필요치: {hitChance - evade}");

            if (roll >= (hitChance - evade))
            {
                Debug.Log($"[Player] {partName} 부위를 공격했지만 빗나갔습니다!\n");
                BossPartCombatManager.PlayDodgeSound();
                return;
            }

            target.DamagePart(partName, AttackPower);
            Debug.Log($"[Player] {partName} 부위에 {AttackPower} 데미지 적중!\n");
            BossPartCombatManager.PlayHitSound();
        }

        // 보스에게 공격 당했을 때 체력 감소 처리
        public void TakeDamage(int amount, string source = "직접 피해")
        {
            CurrentHP -= amount;
            CurrentHP = Mathf.Max(CurrentHP, 0);
            Debug.Log($"[Player] 피해: -{amount} ({source}), 현재 체력: {CurrentHP}");
        }
        public void FocusBattleStateReset()
        {
            MaxHP = MaxHealth * 5; // 집중 전투용 HP 설정
            CurrentHP = MaxHP; // 초기화
            AttackPower = damage * 5; // 집중 전투용 공격력 설정

        }
    }
    //클래스들은 밑으로 뺐음
    public class OptionContext
    {
        public PlayerState playerState; 
        public Character User;      // 더미로 붙일 Character 컴포넌트
        public Character Target;
        public int Value;
        public string item_ID;
        public string option_ID;
        public float hp; // 예시 추가
        public override string ToString()
        {
            return $"[OptionContext] User: {User.name}, Target: {Target.name}, Value: {Value}, Item: {item_ID}, Option: {option_ID}";
        }

    }

    [System.Serializable]
    public class BuffData
    {
        public string BuffID;         // 고유 ID (예: "crit_001", "burn_stack", etc)
        public string OptionID;      // 옵션 효과 ID (예: "101" → 치명타 확률)
        public int Value;            // 수치

        public float Duration;       // 지속 시간 (0 = 영구)
        public float Elapsed = 0f;      // 경과 시간

        public string SourceItemID;  // 버프 유래 (ex: 장비ID)
        public bool IsPassive;       // 패시브인지 여부
        public bool IsDebuff;           // 🔥 디버프 여부
        public Character Target;        // 🔥 디버프일 경우 대상 (적 캐릭터)
        public Character User;          // 자기 자신

        //집중 전투용 버프 
        public FocusPartData FocusData = null;

        [System.Serializable]
        public class FocusPartData
        {
            public string PartName;      // "Head", "Arm", "Leg" 등
            public float DamageRatio;    // 0.25f, 0.5f 등
        }
    }

    
}
