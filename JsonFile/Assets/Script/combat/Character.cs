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
        public interface IEquipmentQuery
        {
            bool IsItemEquipped(string itemID);
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
        private readonly Dictionary<string, BuffData> activeBuffs = new();
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
        public IEquipmentQuery equipmentQuery; // EquipmentSystem에서 주입
        bool IsStillEquipped(BuffData buff)
            => buff.IsPassive && equipmentQuery != null && equipmentQuery.IsItemEquipped(buff.SourceItemID);

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
        private string GetBuffKey(BuffData b)
        {
            return string.IsNullOrEmpty(b.SourceItemID) ? b.BuffID : $"{b.BuffID}:{b.SourceItemID}";
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
            int testnum = UnityEngine.Random.Range(0, 100);
            bool isCrit = testnum < CitChance;
            int finalDamage = isCrit ? damage * 2 : damage;

            Debug.Log($"{isCrit} , {CitChance} {testnum}");
            Debug.Log(finalDamage);

            int dealtDamage = target.TakeDamage(finalDamage);
            return (dealtDamage, isCrit);
        }
        //원래 코드
        public void AddBuff(BuffData buff)
        {
            if (buff == null) return;
            string key = GetBuffKey(buff);

            // 동일 출처/동일 BuffID가 이미 있으면 → 갱신(지속시간 리셋)
            if (activeBuffs.TryGetValue(key, out var existing))
            {
                existing.Duration = buff.Duration;
                existing.Elapsed = 0f;
                existing.Value = buff.Value; // 필요 시 값도 갱신
                Debug.Log($"[Buff] Refresh: {key} (Duration reset, Value={existing.Value})");

                // GameObject가 활성화되어 있을 때만 UI 갱신
                RefreshBuffUI();
                StartBuffRoutineSafe();
                return;
            }

            // 신규 등록
            activeBuffs[key] = buff;

            // 즉시형 효과 로그/즉시 반영
            switch (buff.OptionID)
            {
                case "Option_003": // 화상 즉시 2% (Tick도 별도 진행)
                    int dmg = Mathf.FloorToInt(buff.Target.MaxHealth * 0.02f);
                    buff.Target.Health -= dmg;
                    Debug.Log($"[화상 즉시] {buff.Target.charaterName} -{dmg}");
                    break;
                case "Option_004": // 회복 즉시 2%
                    int heal = Mathf.FloorToInt(buff.Target.MaxHealth * 0.02f);
                    buff.Target.Health = Mathf.Min(buff.Target.MaxHealth, buff.Target.Health + heal);
                    Debug.Log($"[회복 즉시] {buff.Target.charaterName} +{heal}");
                    break;
            }

            // 스탯형은 적용
            ApplyStatDelta(buff, apply: true);

            // GameObject가 활성화되어 있을 때만 UI/루틴 처리
            RefreshBuffUI();
            StartBuffRoutineSafe();

            Debug.Log($"[Buff] Add: {key} ({buff.OptionID}) from {buff.SourceItemID}");
        }

        private void StartBuffRoutineSafe()
        {
            if (buffCoroutine != null) return;

            // GameObject가 활성화되어 있고 enabled 상태일 때만 코루틴 시작
            if (gameObject != null && gameObject.activeInHierarchy && enabled)
            {
                buffCoroutine = StartCoroutine(BuffTickRoutine());
            }
            else
            {
                Debug.LogWarning($"[Buff] {gameObject.name}이 비활성 상태여서 버프 루틴을 시작할 수 없습니다.");
            }
        }
        private void RefreshBuffUI()
        {
            // GameObject가 활성화되어 있을 때만 UI 갱신 코루틴 시작
            if (gameObject != null && gameObject.activeInHierarchy && enabled)
            {
                if (uiRefreshRoutine != null) StopCoroutine(uiRefreshRoutine);
                uiRefreshRoutine = StartCoroutine(DelayUIRefresh());
            }
            else
            {
                // 코루틴을 사용할 수 없는 경우 직접 UI 갱신
                RefreshBuffUIDirect();
            }
        }
        private void RefreshBuffUIDirect()
        {
            try
            {
                buffUI?.SetBuffs(activeBuffs.Values.ToList(), this);
                Debug.Log($"[Buff] UI 직접 갱신 완료 for {gameObject.name}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Buff] UI 갱신 실패: {e.Message}");
            }
        }
        private void ApplyStatDelta(BuffData buff, bool apply)
        {
            switch (buff.OptionID)
            {
                case "Option_002": // 치확
                    CitChance += apply ? buff.Value : -buff.Value;
                    break;

                case "Option_005": // 공속
                    float mul = buff.Value / 100f;
                    if (apply) speed *= (1f + mul);
                    else speed /= (1f + mul);
                    break;
            }
        }

        private IEnumerator BuffTickRoutine()
        {
            WaitForSeconds wait = new WaitForSeconds(1f);

            while (true)
            {
                // 전투 중이 아닐 때는 쉬되, 루틴은 유지 (재진입 비용 방지)
                if (gameFlowManager == null || gameFlowManager.GetCurrentFlowState() != GameFlowManager.FlowState.Battle)
                {
                    yield return wait;
                    continue;
                }

                var expired = new List<string>();

                foreach (var kv in activeBuffs)
                {
                    var buff = kv.Value;

                    // 패시브인데 아직 장착중이면 시간 고정(무한)
                    if (IsStillEquipped(buff))
                    {
                        buff.Elapsed = 0f;
                        continue;
                    }

                        buff.Elapsed += 1f;
                    //버프마다 자동으로 계산중

                    // Tick 효과
                    switch (buff.OptionID)
                    {
                        case "Option_003": // 화상
                            int dmg = Mathf.FloorToInt(buff.Target.MaxHealth * 0.02f);
                            buff.Target.Health -= dmg;
                            Debug.Log($"[화상 Tick] {buff.Target.charaterName} -{dmg}");
                            break;

                        case "Option_004": // 회복
                            int heal = Mathf.FloorToInt(buff.Target.MaxHealth * 0.02f);
                            buff.Target.Health = Mathf.Min(buff.Target.MaxHealth, buff.Target.Health + heal);
                            Debug.Log($"[회복 Tick] {buff.Target.charaterName} +{heal}");
                            break;
                    }

                    if (buff.Duration > 0 && buff.Elapsed >= buff.Duration)
                        expired.Add(kv.Key);
                }

                // 만료 반영
                foreach (var key in expired)
                    RemoveBuff(key);

                // 모두 없어지면 루틴 종료
                if (activeBuffs.Count == 0)
                {
                    buffCoroutine = null;
                    yield break;
                }

                battleUI?.UpdateUI();
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
            var keys = activeBuffs
                .Where(kv => kv.Value.SourceItemID == itemID)
                .Select(kv => kv.Key)
                .ToList();

            foreach (var key in keys)
            {
                var buff = activeBuffs[key];
                ApplyStatDelta(buff, apply: false);
                activeBuffs.Remove(key);

                // ❌ UI 직접 제거 호출 금지 (다른 진영/출처까지 지우는 원인)
                // buffUI?.ClearBuffByID(buff.BuffID);
                Debug.Log($"[장비 해제] {itemID} → {key} 제거");
            }

            // ✅ 단 한 번의 동기화로 안전하게 반영(캐릭터별 맵에서 '보이는 것만 남김')
            RefreshBuffUI();
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

        private void OnDisable()
        {
            if (buffCoroutine != null)
            {
                StopCoroutine(buffCoroutine);
                buffCoroutine = null;
            }
            if (uiRefreshRoutine != null)
            {
                StopCoroutine(uiRefreshRoutine);
                uiRefreshRoutine = null;
            }
        }
        private void OnEnable()
        {
            // 활성 버프가 있고 전투 중이라면 루틴 재시작
            if (activeBuffs.Count > 0 && gameFlowManager != null &&
                gameFlowManager.GetCurrentFlowState() == GameFlowManager.FlowState.Battle)
            {
                StartBuffRoutineSafe();
            }
        }
        public void RemoveBuff(string buffID)
        {
            // buffID 단독으로는 모호 → 해당 ID를 가진 모든 엔트리 제거 (패시브 제외)
            var toRemove = activeBuffs
                .Where(kv => kv.Value.BuffID == buffID && !kv.Value.IsPassive)
                .Select(kv => kv.Key)
                .ToList();

            foreach (var key in toRemove)
                RemoveBuffByKey(key);
        }
        private void RemoveBuffByKey(string key)
        {
            if (!activeBuffs.TryGetValue(key, out var buff)) return;

            if (buff.IsPassive)
            {
                Debug.Log($"[패시브 유지] {key} from {buff.SourceItemID}");
                return;
            }

            ApplyStatDelta(buff, apply: false);
            activeBuffs.Remove(key);

            // ❌ UI 직접 제거 호출 금지
            // buffUI?.ClearBuffByID(buff.BuffID);

            Debug.Log($"[Buff] Removed: {key}");

            // ✅ 상태 변경 직후 동기화
            RefreshBuffUI();
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

        public void TurnDebuff()
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
