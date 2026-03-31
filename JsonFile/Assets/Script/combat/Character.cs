//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using Unity.Mathematics;
//using UnityEngine;
//using Random = UnityEngine.Random;

//namespace MyGame
//{
//    public class Character : MonoBehaviour
//    {
//        public interface IEquipmentQuery
//        {
//            bool IsItemEquipped(string itemID);
//        }

//        [Header("캐릭터 기본 정보입니다 여기 값 + 능력치를 적용 시킬 예정입니다")]
//        public GameFlowManager gameFlowManager;
//        public string charaterName;
//        [SerializeField] PlayerState playerState;
//        public int MaxHealth = 50;
//        public int Health;
//        public int damage;
//        public float speed;
//        public int armor;
//        public int CitChance = 10; //일반적인 크리티컬 확률
//        public int GetEXP;

//        public string weapon_Name;
//        public string armor_Name;
//        public string MonPas_Effect1;
//        public int MonPas_Value1;
//        public string MonPas_Effect2;
//        public int MonPas_Value2;
//        //버프 코루틴 용
//        private Coroutine buffCoroutine;
//        //옵션들 리스트에 기록
//        public List<EquippedOption> OnHitOptions = new List<EquippedOption>();
//        public List<MonsterOption> OnEnemyHitOptions = new List<MonsterOption>();
//        private readonly Dictionary<string, BuffData> activeBuffs = new();
//        public BattleUI battleUI;
//        public BuffUI buffUI;
//        private Coroutine uiRefreshRoutine;

//        [Header("집중전투 전용")]
//        public BossPartCombatManager BossPartCombatManager;
//        public int MaxHP;
//        public int CurrentHP;
//        public int AttackPower = 30;
//        public int hitChance = 80; // 명중률 (0~100)

//        public bool IsDead => CurrentHP <= 0;
//        public IEquipmentQuery equipmentQuery; // EquipmentSystem에서 주입
//        bool IsStillEquipped(BuffData buff)
//            => buff.IsPassive && equipmentQuery != null && equipmentQuery.IsItemEquipped(buff.SourceItemID);

//        public List<FocusBuffData> ActiveDebuffs = new();
//        //이 부분은 캐릭터 클래스의 하위에 있어야 되는 부분이라서 맨 아래로 안배고 맨 위에 넣음

//        private void Start()
//        {
//            playerState = PlayerState.Instance;
//        }
//        [System.Serializable]
//        public struct EquippedOption
//        {
//            public string OptionID;
//            public int Value;
//            public string item_ID;
//        }
//        private string GetBuffKey(BuffData b)
//        {
//            return string.IsNullOrEmpty(b.SourceItemID) ? b.BuffID : $"{b.BuffID}:{b.SourceItemID}";
//        }
//        [System.Serializable]
//        public struct MonsterOption
//        {
//            public string OptionID;
//            public int Value;
//        }
//        // 방어구만큼 경감하고 남은 데미지를 HP에서 깎는다
//        public int TakeDamage(int damage)
//        {
//            int reduced = Mathf.Max(damage - armor, 0);
//            Health -= reduced;
//            Debug.Log($"{charaterName}이(가) 받는 데미지: {damage} → 방어구 {armor} 경감 → 실제 {reduced}. 현재 HP: {Health}");
//            return reduced;
//        }

//        // 기본 공격 메서드
//        public (int dealtDamage, bool isCrit) Attack(Character target)
//        {
//            Debug.Log(damage);
//            Debug.Log($"{charaterName}이(가) {target.charaterName}을(를) 공격: {damage} 데미지 시도");
//            int testnum = UnityEngine.Random.Range(0, 100);
//            bool isCrit = testnum < CitChance;
//            int finalDamage = isCrit ? damage * 2 : damage;

//            Debug.Log($"{isCrit} , {CitChance} {testnum}");
//            Debug.Log(finalDamage);

//            int dealtDamage = target.TakeDamage(finalDamage);
//            return (dealtDamage, isCrit);
//        }
//        //원래 코드
//        public void AddBuff(BuffData buff)
//        {
//            if (buff == null) return;
//            string key = GetBuffKey(buff);

//            // 동일 출처/동일 BuffID가 이미 있으면 → 갱신(지속시간 리셋)
//            if (activeBuffs.TryGetValue(key, out var existing))
//            {
//                existing.Duration = buff.Duration;
//                existing.Elapsed = 0f;
//                existing.Value = buff.Value; // 필요 시 값도 갱신
//                Debug.Log($"[Buff] Refresh: {key} (Duration reset, Value={existing.Value})");

//                // GameObject가 활성화되어 있을 때만 UI 갱신
//                RefreshBuffUI();
//                StartBuffRoutineSafe();
//                Debug.Log($"[Buff] Add: {key} ({buff.OptionID}) on {charaterName}, GM={gameFlowManager != null}");

//                return;
//            }

//            // 신규 등록
//            activeBuffs[key] = buff;

//            // 즉시형 효과 로그/즉시 반영
//            switch (buff.OptionID)
//            {
//                case "Option_003": // 화상 즉시 2% (Tick도 별도 진행)
//                    int dmg = Mathf.FloorToInt(buff.Target.MaxHealth * 0.02f);
//                    buff.Target.Health -= dmg;
//                    Debug.Log($"[화상 즉시] {buff.Target.charaterName} -{dmg}");
//                    break;
//                case "Option_004": // 회복 즉시 2%
//                    int heal = Mathf.FloorToInt(buff.Target.MaxHealth * 0.02f);
//                    buff.Target.Health = Mathf.Min(buff.Target.MaxHealth, buff.Target.Health + heal);
//                    Debug.Log($"[회복 즉시] {buff.Target.charaterName} +{heal}");
//                    break;
//            }

//            // 스탯형은 적용
//            ApplyStatDelta(buff, apply: true);

//            // GameObject가 활성화되어 있을 때만 UI/루틴 처리
//            RefreshBuffUI();
//            StartBuffRoutineSafe();

//            Debug.Log($"[Buff] Add: {key} ({buff.OptionID}) from {buff.SourceItemID}");
//        }

//        private void StartBuffRoutineSafe()
//        {
//            if (buffCoroutine != null) return;

//            // GameObject가 활성화되어 있고 enabled 상태일 때만 코루틴 시작
//            if (gameObject != null && gameObject.activeInHierarchy && enabled)
//            {
//                buffCoroutine = StartCoroutine(BuffTickRoutine());
//            }
//            else
//            {
//                Debug.LogWarning($"[Buff] {gameObject.name}이 비활성 상태여서 버프 루틴을 시작할 수 없습니다.");
//            }
//        }
//        private void RefreshBuffUI()
//        {
//            // GameObject가 활성화되어 있을 때만 UI 갱신 코루틴 시작
//            if (gameObject != null && gameObject.activeInHierarchy && enabled)
//            {
//                if (uiRefreshRoutine != null) StopCoroutine(uiRefreshRoutine);
//                uiRefreshRoutine = StartCoroutine(DelayUIRefresh());
//            }
//            else
//            {
//                // 코루틴을 사용할 수 없는 경우 직접 UI 갱신
//                RefreshBuffUIDirect();
//            }
//        }
//        private void RefreshBuffUIDirect()
//        {
//            try
//            {
//                buffUI?.SetBuffs(activeBuffs.Values.ToList(), this);
//                Debug.Log($"[Buff] UI 직접 갱신 완료 for {gameObject.name}");
//            }
//            catch (System.Exception e)
//            {
//                Debug.LogWarning($"[Buff] UI 갱신 실패: {e.Message}");
//            }
//        }
//        private void ApplyStatDelta(BuffData buff, bool apply)
//        {
//            switch (buff.OptionID)
//            {
//                case "Option_002": // 치확
//                    CitChance += apply ? buff.Value : -buff.Value;
//                    break;

//                case "Option_005": // 공속
//                    float mul = buff.Value / 100f;
//                    if (apply) speed *= (1f + mul);
//                    else speed /= (1f + mul);
//                    break;
//            }
//        }

//        private IEnumerator BuffTickRoutine()
//        {
//            //WaitForSeconds wait = new WaitForSeconds(1f);

//            //while (true)
//            //{
//            //    // 전투 중이 아닐 때는 쉬되, 루틴은 유지 (재진입 비용 방지)
//            //    if (gameFlowManager == null || gameFlowManager.GetCurrentFlowState() != GameFlowManager.FlowState.Battle)
//            //    {
//            //        yield return wait;
//            //        continue;
//            //    }

//            //    var expired = new List<string>();

//            //    foreach (var kv in activeBuffs)
//            //    {
//            //        var buff = kv.Value;

//            //        // 패시브인데 아직 장착중이면 시간 고정(무한)
//            //        if (IsStillEquipped(buff))
//            //        {
//            //            buff.Elapsed = 0f;
//            //            continue;
//            //        }

//            //            buff.Elapsed += 1f;
//            //        //버프마다 자동으로 계산중

//            //        // Tick 효과
//            //        switch (buff.OptionID)
//            //        {
//            //            case "Option_003": // 화상
//            //                int dmg = Mathf.FloorToInt(buff.Target.MaxHealth * 0.02f);
//            //                buff.Target.Health -= dmg;
//            //                Debug.Log($"[화상 Tick] {buff.Target.charaterName} -{dmg}");
//            //                break;

//            //            case "Option_004": // 회복
//            //                int heal = Mathf.FloorToInt(buff.Target.MaxHealth * 0.02f);
//            //                buff.Target.Health = Mathf.Min(buff.Target.MaxHealth, buff.Target.Health + heal);
//            //                Debug.Log($"[회복 Tick] {buff.Target.charaterName} +{heal}");
//            //                break;
//            //        }

//            //        if (buff.Duration > 0 && buff.Elapsed >= buff.Duration)
//            //            expired.Add(kv.Key);
//            //    }

//            //    // 만료 반영
//            //    foreach (var key in expired)
//            //        RemoveBuff(key);

//            //    // 모두 없어지면 루틴 종료
//            //    if (activeBuffs.Count == 0)
//            //    {
//            //        buffCoroutine = null;
//            //        yield break;
//            //    }

//            //    battleUI?.UpdateUI();
//            //    yield return wait;
//            //}
//            WaitForSeconds wait = new WaitForSeconds(1f);

//            while (true)
//            {
//                // 전투가 아니라는 신호가 "명시적으로" 올 때만 멈추고,
//                // gameFlowManager == null 이면 전투 중으로 취급
//                if (gameFlowManager != null &&
//                    gameFlowManager.GetCurrentFlowState() != GameFlowManager.FlowState.Battle)
//                {
//                    yield return wait;
//                    continue;
//                }

//                var expired = new List<string>();

//                foreach (var kv in activeBuffs)
//                {
//                    var buff = kv.Value;

//                    // 패시브(장착 유지)면 시간 고정
//                    if (IsStillEquipped(buff)) { buff.Elapsed = 0f; continue; }

//                    buff.Elapsed += 1f;  // 1초 틱 진행

//                    switch (buff.OptionID)
//                    {
//                        case "Option_003": // 화상
//                            int dmg = Mathf.FloorToInt(buff.Target.MaxHealth * 0.02f);
//                            buff.Target.Health -= dmg;
//                            Debug.Log($"[화상 Tick] {buff.Target.charaterName} -{dmg}");
//                            break;

//                        case "Option_004": // 회복
//                            int heal = Mathf.FloorToInt(buff.Target.MaxHealth * 0.02f);
//                            buff.Target.Health = Mathf.Min(buff.Target.MaxHealth, buff.Target.Health + heal);
//                            Debug.Log($"[회복 Tick] {buff.Target.charaterName} +{heal}");
//                            break;
//                    }

//                    if (buff.Duration > 0 && buff.Elapsed >= buff.Duration)
//                        expired.Add(kv.Key);
//                }

//                foreach (var key in expired)
//                    RemoveBuff(key);

//                if (activeBuffs.Count == 0) { buffCoroutine = null; yield break; }

//                battleUI?.UpdateUI();
//                yield return wait;
//            }
//        }


//        //프레임 대기
//        private IEnumerator DelayUIRefresh()
//        {
//            yield return null; // 1 프레임 대기 (모든 버프 적용 후)
//            buffUI?.SetBuffs(activeBuffs.Values.ToList(), this);
//            uiRefreshRoutine = null;
//        }

//        //장착 해제 시 버프 해제
//        public void RemoveBuffByItem(string itemID)
//        {
//            var keys = activeBuffs
//                .Where(kv => kv.Value.SourceItemID == itemID)
//                .Select(kv => kv.Key)
//                .ToList();

//            foreach (var key in keys)
//            {
//                var buff = activeBuffs[key];
//                ApplyStatDelta(buff, apply: false);
//                activeBuffs.Remove(key);

//                // ❌ UI 직접 제거 호출 금지 (다른 진영/출처까지 지우는 원인)
//                // buffUI?.ClearBuffByID(buff.BuffID);
//                Debug.Log($"[장비 해제] {itemID} → {key} 제거");
//            }

//            // ✅ 단 한 번의 동기화로 안전하게 반영(캐릭터별 맵에서 '보이는 것만 남김')
//            RefreshBuffUI();
//        }

//        //버프 제거용
//        //public void RemoveBuff(string buffID)
//        //{
//        //    if (!activeBuffs.TryGetValue(buffID, out var buff))
//        //    {
//        //        Debug.LogWarning($"[RemoveBuff] 존재하지 않는 버프: {buffID}");
//        //        return;
//        //    }

//        //    // 패시브 버프는 제거하지 않음
//        //    if (buff.IsPassive)
//        //    {
//        //        Debug.Log($"[패시브 유지] {buffID} 은(는) 패시브로 유지됨");
//        //        return;
//        //    }

//        //    // 스탯 복원
//        //    switch (buff.OptionID)
//        //    {
//        //        case "Option_002": // 치명타 확률
//        //            CitChance -= buff.Value;
//        //            Debug.Log($"[RemoveBuff] 치명타 확률 -{buff.Value} → {CitChance}");
//        //            break;

//        //        case "Option_005": // 공속 증가 → 원래 속도로 복원
//        //            float multiplier = buff.Value / 100f;
//        //            speed /= (1f + multiplier);
//        //            Debug.Log($"[RemoveBuff] 공속 복구 → speed = {speed}");
//        //            break;

//        //            // 필요 시 다른 OptionID도 추가 가능
//        //    }

//        //    activeBuffs.Remove(buffID);
//        //    Debug.Log($"[RemoveBuff] 버프 제거 완료: {buffID}");

//        //    // UI 갱신
//        //    buffUI?.SetBuffs(activeBuffs.Values.ToList(), this);
//        //}

//        private void OnDisable()
//        {
//            if (buffCoroutine != null)
//            {
//                StopCoroutine(buffCoroutine);
//                buffCoroutine = null;
//            }
//            if (uiRefreshRoutine != null)
//            {
//                StopCoroutine(uiRefreshRoutine);
//                uiRefreshRoutine = null;
//            }
//        }
//        private void OnEnable()
//        {
//            // 활성 버프가 있고 전투 중이라면 루틴 재시작
//            if (activeBuffs.Count > 0 && gameFlowManager != null &&
//                gameFlowManager.GetCurrentFlowState() == GameFlowManager.FlowState.Battle)
//            {
//                StartBuffRoutineSafe();
//            }
//        }
//        public void RemoveBuff(string buffID)
//        {
//            // buffID 단독으로는 모호 → 해당 ID를 가진 모든 엔트리 제거 (패시브 제외)
//            var toRemove = activeBuffs
//                .Where(kv => kv.Value.BuffID == buffID && !kv.Value.IsPassive)
//                .Select(kv => kv.Key)
//                .ToList();

//            foreach (var key in toRemove)
//                RemoveBuffByKey(key);
//        }
//        private void RemoveBuffByKey(string key)
//        {
//            if (!activeBuffs.TryGetValue(key, out var buff)) return;

//            if (buff.IsPassive)
//            {
//                Debug.Log($"[패시브 유지] {key} from {buff.SourceItemID}");
//                return;
//            }

//            ApplyStatDelta(buff, apply: false);
//            activeBuffs.Remove(key);

//            // ❌ UI 직접 제거 호출 금지
//            // buffUI?.ClearBuffByID(buff.BuffID);

//            Debug.Log($"[Buff] Removed: {key}");

//            // ✅ 상태 변경 직후 동기화
//            RefreshBuffUI();
//        }
//        //일시적인 버프 모두 제거
//        public void RemoveTemporaryBuffs()
//        {
//            var toRemove = activeBuffs
//                .Where(kv => !kv.Value.IsPassive) // 패시브가 아닌 것만 제거
//                .Select(kv => kv.Key)
//                .ToList();

//            foreach (var key in toRemove)
//            {
//                RemoveBuff(key);
//                Debug.Log($"[전투 종료] 일시적 버프 제거됨: {key}");
//            }
//        }

//        public void TurnDebuff()
//        {
//            List<FocusBuffData> expired = new();

//            foreach (var buff in ActiveDebuffs)
//            {
//                buff.Elapsed += 1f;

//                ApplyFocusBuffEffect(buff);

//                if (buff.Elapsed >= buff.Duration)
//                {
//                    expired.Add(buff);
//                    Debug.Log($"[버프 만료] {buff.OptionID}");
//                }
//            }

//            foreach (var b in expired)
//                ActiveDebuffs.Remove(b);
//        }


//        // 디버프 효과 적용 처리
//        private void ApplyFocusBuffEffect(FocusBuffData buff)
//        {
//            if (buff.OptionID == "Option_003") // 화상
//            {
//                int damage = Mathf.FloorToInt(MaxHP * (buff.Value / 100f));
//                TakeDamage(damage, "화상");
//                Debug.Log($"🔥 [플레이어 화상 피해] {damage} 데미지");
//            }

//            // 다른 디버프 효과도 여기에 추가
//        }

//        // 디버프 추가 또는 갱신
//        public void AddFocusBuff(FocusBuffData newBuff)
//        {
//            var existing = ActiveDebuffs.FirstOrDefault(b => b.OptionID == newBuff.OptionID);
//            if (existing != null)
//            {
//                existing.Elapsed = 0f;
//                existing.Duration = newBuff.Duration;
//                Debug.Log($"[버프 갱신] {newBuff.OptionID} → 지속 {newBuff.Duration}턴");
//            }
//            else
//            {
//                ActiveDebuffs.Add(newBuff);
//                Debug.Log($"[버프 적용] {newBuff.OptionID} → 지속 {newBuff.Duration}턴");
//            }
//        }

//        // 보스의 특정 부위를 공격
//        public void PerformAttack(TESTBoss target, string partName)
//        {
//            if (target == null || target.IsDead) return;
//            if (!target.CanAttackPart(partName)) return;

//            int evade = target.GetEvadeRate(partName);
//            int roll = Random.Range(0, 100);

//            Debug.Log($"[Player] 명중 굴림: {roll} vs 명중 필요치: {hitChance - evade}");

//            if (roll >= (hitChance - evade))
//            {
//                Debug.Log($"[Player] {partName} 부위를 공격했지만 빗나갔습니다!\n");
//                BossPartCombatManager.PlayDodgeSound();
//                return;
//            }

//            target.DamagePart(partName, AttackPower);
//            Debug.Log($"[Player] {partName} 부위에 {AttackPower} 데미지 적중!\n");
//            BossPartCombatManager.PlayHitSound();
//        }

//        // 보스에게 공격 당했을 때 체력 감소 처리
//        public void TakeDamage(int amount, string source = "직접 피해")
//        {
//            CurrentHP -= amount;
//            CurrentHP = Mathf.Max(CurrentHP, 0);
//            Debug.Log($"[Player] 피해: -{amount} ({source}), 현재 체력: {CurrentHP}");
//        }
//        public void FocusBattleStateReset()
//        {
//            MaxHP = MaxHealth * 5; // 집중 전투용 HP 설정
//            CurrentHP = MaxHP; // 초기화
//            AttackPower = damage * 5; // 집중 전투용 공격력 설정

//        }
//    }
//    //클래스들은 밑으로 뺐음
//    public class OptionContext
//    {
//        public PlayerState playerState; 
//        public Character User;      // 더미로 붙일 Character 컴포넌트
//        public Character Target;
//        public int Value;
//        public string item_ID;
//        public string option_ID;
//        public float hp; // 예시 추가
//        public override string ToString()
//        {
//            return $"[OptionContext] User: {User.name}, Target: {Target.name}, Value: {Value}, Item: {item_ID}, Option: {option_ID}";
//        }

//    }

//    [System.Serializable]
//    public class BuffData
//    {
//        public string BuffID;         // 고유 ID (예: "crit_001", "burn_stack", etc)
//        public string OptionID;      // 옵션 효과 ID (예: "101" → 치명타 확률)
//        public int Value;            // 수치

//        public float Duration;       // 지속 시간 (0 = 영구)
//        public float Elapsed = 0f;      // 경과 시간

//        public string SourceItemID;  // 버프 유래 (ex: 장비ID)
//        public bool IsPassive;       // 패시브인지 여부
//        public bool IsDebuff;           // 🔥 디버프 여부
//        public Character Target;        // 🔥 디버프일 경우 대상 (적 캐릭터)
//        public Character User;          // 자기 자신

//        //집중 전투용 버프 
//        public FocusPartData FocusData = null;

//        [System.Serializable]
//        public class FocusPartData
//        {
//            public string PartName;      // "Head", "Arm", "Leg" 등
//            public float DamageRatio;    // 0.25f, 0.5f 등
//        }
//    }


//}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MyGame
{
    /// <summary>
    /// 전투 로직(자동 전투용 버프/디버프, 집중전투용 디버프)을 모두 다루는 캐릭터 컴포넌트
    /// - 버프의 "실제 상태"는 activeBuffs 딕셔너리에만 존재(싱글 소스 오브 트루스)
    /// - UI는 매번 RefreshBuffUI()를 통해 activeBuffs 상태를 그대로 반영(표시만)
    /// - 시간 진행(Elapsed 증감)은 이 클래스만 책임 (BuffIconUI는 표시만)
    /// </summary>
    public class Character : MonoBehaviour
    {
        /// <summary>
        /// 패시브 버프의 "장착 유지" 여부를 판단하는 질의 인터페이스
        /// - EquipmentSystem에서 구현체를 주입하여, 특정 ItemID가 현재 장착 중인지 확인
        /// </summary>
        public interface IEquipmentQuery
        {
            bool IsItemEquipped(string itemID);
        }

        [Header("캐릭터 기본 정보(능력치 계산의 베이스)")]
        public GameFlowManager gameFlowManager; // 전투 상태 확인용(자동 전투 틱 허용 여부)
        public string charaterName;

        [SerializeField] PlayerState playerState;
        public int MaxHealth = 50;
        public int Health;
        public int damage;
        public float speed;
        public int armor;
        public int CitChance = 10; // 기본 치확
        public int GetEXP;

        [Header("장비/몬스터 패시브 원천 정보")]
        public string weapon_Name;
        public string armor_Name;
        public string MonPas_Effect1;
        public int MonPas_Value1;
        public string MonPas_Effect2;
        public int MonPas_Value2;

        // 자동 전투 버프 틱 코루틴 핸들
        private Coroutine buffCoroutine;

        [Header("보유 옵션(온힛/피격시 발동 등)")]
        public List<EquippedOption> OnHitOptions = new List<EquippedOption>();      // 본인 공격 시 발동
        public List<MonsterOption> OnEnemyHitOptions = new List<MonsterOption>();   // 적의 공격(피격) 시 발동

        /// <summary>
        /// 자동 전투에서 적용되는 현재 "모든" 버프(패시브/일시적/디버프 포함)
        /// 키: BuffID[:SourceItemID] (같은 버프라도 출처가 다르면 별개로 관리)
        /// </summary>
        private readonly Dictionary<string, BuffData> activeBuffs = new();

        // UI 핸들
        public BattleUI battleUI;
        public BuffUI buffUI;
        private Coroutine uiRefreshRoutine;

        [Header("집중전투 전용(턴 기반)")]
        public BossPartCombatManager BossPartCombatManager;
        public int MaxHP;
        public int CurrentHP;
        public int AttackPower = 30;
        public int hitChance = 80; // 명중률 (0~100)

        /// <summary>집중전투 사망 판정(자동 전투의 Health와는 분리된 체계)</summary>
        public bool IsDead => CurrentHP <= 0;

        /// <summary>장비 장착 여부 질의(패시브 무한 지속 판단용)</summary>
        public IEquipmentQuery equipmentQuery; // EquipmentSystem에서 주입

        /// <summary>
        /// 패시브이면서 해당 아이템이 현재 장착 상태라면 "시간 진행을 멈춰" 무한 지속으로 취급.
        /// (= Elapsed를 0으로 유지)  ※ 집중전투와는 무관, 자동 전투용 버프에만 적용
        /// </summary>
        bool IsStillEquipped(BuffData buff)
            => buff.IsPassive && equipmentQuery != null && equipmentQuery.IsItemEquipped(buff.SourceItemID);

        /// <summary>
        /// 집중전투(턴 기반)에서는 자동 전투 버프와 완전히 분리된 디버프 리스트를 사용
        /// - 턴이 지날 때마다 TurnDebuff()에서 Elapsed +1 및 효과 Tick
        /// </summary>
        // public List<FocusBuffData> ActiveDebuffs = new();

        // NOTE: 집중전투 관련 필드가 파일 하단이 아닌 상단에 위치해야 해서 여기 배치

        private void Start()
        {
            playerState = PlayerState.Instance;
        }

        [System.Serializable]
        public struct EquippedOption
        {
            public string OptionID;
            public int Value;
            public string item_ID; // 출처 아이템(패시브 연동)
        }

        /// <summary>
        /// 자동 전투 버프의 유니크 키 생성
        /// - 동일 BuffID라도 SourceItemID가 다르면 별개로 관리
        /// - OnHit 등 아이템이 아닌 출처면 SourceItemID는 비워둘 수 있음
        /// </summary>
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

        #region 전투/피해 처리(자동 전투 체계)
        // 방어구만큼 경감하고 남은 데미지를 HP에서 깎는다(자동 전투 체계)
        public int TakeDamage(int damage)
        {
            int reduced = Mathf.Max(damage - armor, 0);
            Health -= reduced;
            Debug.Log($"{charaterName}이(가) 받는 데미지: {damage} → 방어구 {armor} 경감 → 실제 {reduced}. 현재 HP: {Health}");
            return reduced;
        }

        // 기본 공격 (자동 전투 체계)
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
        #endregion

        #region 자동 전투: 버프 추가/갱신/적용
        /// <summary>
        /// 자동 전투 버프 추가
        /// - 동일 키(BuffID[:Source])가 존재하면 Duration/Value 갱신 + Elapsed 리셋
        /// - 신규면 등록 후 "즉시형 효과" 1회 반영(예: 화상/회복 시작 즉시)
        /// - 스탯형(치확/공속 등)은 ApplyStatDelta로 즉시 반영
        /// - UI는 "항상" RefreshBuffUI() 한 번으로 동기화(직접 삭제 X)
        /// </summary>
        public void AddBuff(BuffData buff)
        {
            if (buff == null) return;
            string key = GetBuffKey(buff);

            // 동일 출처/동일 BuffID가 이미 있으면 → 갱신(지속시간 리셋 & 값 갱신)
            if (activeBuffs.TryGetValue(key, out var existing))
            {
                existing.Duration = buff.Duration;
                existing.Elapsed = 0f;
                existing.Value = buff.Value; // 필요 시 수치 갱신
                Debug.Log($"[Buff] Refresh: {key} (Duration reset, Value={existing.Value})");

                // UI 동기화 + 틱 루틴 보장
                RefreshBuffUI();
                StartBuffRoutineSafe();

                // 디버깅: 이 캐릭터의 GM 주입 여부를 확인
                Debug.Log($"[Buff] Add: {key} ({buff.OptionID}) on {charaterName}, GM={gameFlowManager != null}");
                return;
            }

            // 신규 등록
            activeBuffs[key] = buff;

            // 즉시형 효과 1회(시작 즉시)
            switch (buff.OptionID)
            {
                case "Option_003": // 화상 즉시 2%(Tick은 별도 루틴에서 1초마다 반복)
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

            // 스탯형(치확/공속 등)은 즉시 적용
            ApplyStatDelta(buff, apply: true);

            // UI 동기화 + 틱 루틴 보장
            RefreshBuffUI();
            StartBuffRoutineSafe();

            Debug.Log($"[Buff] Add: {key} ({buff.OptionID}) from {buff.SourceItemID}");
        }
        #endregion

        #region 자동 전투: 틱 루틴(시간 진행/만료/주기 효과)
        /// <summary>
        /// 버프 틱 루틴(자동 전투)
        /// - 1초마다 Elapsed += 1f (실시간 경과)
        /// - 패시브 + 장착 유지 중이면 Elapsed=0으로 고정(무한)
        /// - Tick 효과(화상/회복 등)는 1초마다 한 번씩 적용
        /// - 만료(Duration > 0 && Elapsed >= Duration) 시 Remove
        ///
        /// [중요 변경: A안]
        /// gameFlowManager == null 인 경우도 "전투 중"으로 간주하여 루틴이 멈추지 않게 함.
        /// (적 캐릭터/소환수 등 GM 주입이 없을 때 FillAmount가 0에서 안 오르던 문제 해결)
        /// </summary>
        private IEnumerator BuffTickRoutine()
        {
            // 1초 틱(프레임 기반으로 바꾸려면 yield return null + Time.deltaTime 누적 구조로)
            WaitForSeconds wait = new WaitForSeconds(1f);

            while (true)
            {
                // 전투가 아니라는 "명시적" 신호가 있을 때만 쉰다.
                // GM이 null이면 전투 중으로 간주(= 루틴 지속)  ← A안 핵심
                if (gameFlowManager != null &&
                    gameFlowManager.GetCurrentFlowState() != GameFlowManager.FlowState.Battle)
                {
                    yield return wait;
                    continue;
                }

                var expired = new List<string>();

                foreach (var kv in activeBuffs)
                {
                    var buff = kv.Value;

                    // 패시브인데 현재도 장착 유지면 시간 고정(무한 지속)
                    if (IsStillEquipped(buff))
                    {
                        buff.Elapsed = 0f;
                        continue;
                    }

                    // 1초 경과
                    buff.Elapsed += 1f;

                    // 1초 Tick 효과(지속 피해/지속 회복 등)
                    switch (buff.OptionID)
                    {
                        case "Option_003": // 화상 Tick
                            int dmg = Mathf.FloorToInt(buff.Target.MaxHealth * 0.02f);
                            buff.Target.Health -= dmg;
                            Debug.Log($"[화상 Tick] {buff.Target.charaterName} -{dmg}");
                            break;

                        case "Option_004": // 회복 Tick
                            int heal = Mathf.FloorToInt(buff.Target.MaxHealth * 0.02f);
                            buff.Target.Health = Mathf.Min(buff.Target.MaxHealth, buff.Target.Health + heal);
                            Debug.Log($"[회복 Tick] {buff.Target.charaterName} +{heal}");
                            break;
                    }

                    // 만료 체크(0 이하 = 영구/패시브)
                    if (buff.Duration > 0 && buff.Elapsed >= buff.Duration)
                        expired.Add(kv.Key);
                }

                // 만료 반영(주의: Remove 내부에서 UI를 직접 지우지 않고 Refresh로 동기화)
                foreach (var key in expired)
                    RemoveBuff(key);

                // 더 이상 버프가 없으면 루틴 종료(다음 AddBuff 시 재시작)
                if (activeBuffs.Count == 0)
                {
                    buffCoroutine = null;
                    yield break;
                }

                // UI 갱신
                battleUI?.UpdateUI();
                yield return wait;
            }
        }
        #endregion

        #region 자동 전투: UI 동기화 유틸리티
        /// <summary>
        /// UI 반영을 1프레임 지연시켜 "여러 버프 변경"을 한 번에 묶어 그림
        /// 비활성/미활성 상태면 코루틴 대신 즉시 적용
        /// </summary>
        private void RefreshBuffUI()
        {
            if (gameObject != null && gameObject.activeInHierarchy && enabled)
            {
                if (uiRefreshRoutine != null) StopCoroutine(uiRefreshRoutine);
                uiRefreshRoutine = StartCoroutine(DelayUIRefresh());
            }
            else
            {
                RefreshBuffUIDirect();
            }
        }

        /// <summary>코루틴을 쓸 수 없는 상황(비활성/씬 전환 등)에서 즉시 UI 갱신</summary>
        private void RefreshBuffUIDirect()
        {
            try
            {
                // BuffUI는 캐릭터별 맵을 분리하여, 넘겨준 목록만 표시/유지한다.
                buffUI?.SetBuffs(activeBuffs.Values.ToList(), this);
                Debug.Log($"[Buff] UI 직접 갱신 완료 for {gameObject.name}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Buff] UI 갱신 실패: {e.Message}");
            }
        }

        /// <summary>1프레임 대기 후 UI 갱신(프레임 내 변경 사항을 모두 반영하도록)</summary>
        private IEnumerator DelayUIRefresh()
        {
            yield return null;
            buffUI?.SetBuffs(activeBuffs.Values.ToList(), this);
            uiRefreshRoutine = null;
        }
        #endregion

        #region 스탯형 옵션 적용/복원
        /// <summary>
        /// 스탯형 옵션(치확/공속 등)을 적용/복원
        /// - AddBuff 시 apply=true 로 적용
        /// - Remove 계열 시 apply=false 로 복원
        /// </summary>
        private void ApplyStatDelta(BuffData buff, bool apply)
        {
            switch (buff.OptionID)
            {
                case "Option_002": // 치확 ±Value
                    CitChance += apply ? buff.Value : -buff.Value;
                    break;

                case "Option_005": // 공속 ±(Value%)
                    float mul = buff.Value / 100f;
                    if (apply) speed *= (1f + mul);
                    else speed /= (1f + mul);
                    break;
            }
        }
        #endregion

        #region 장비/버프 제거(자동 전투)
        /// <summary>
        /// 장비 해제 시 해당 아이템에서 파생된 모든 버프 제거
        /// [중요] UI를 직접 지우지 않고, activeBuffs만 수정 → 마지막에 RefreshBuffUI()
        /// </summary>
        public void RemoveBuffByItem(string itemID)
        {
            var keys = activeBuffs
                .Where(kv => kv.Value.SourceItemID == itemID)
                .Select(kv => kv.Key)
                .ToList();

            foreach (var key in keys)
            {
                var buff = activeBuffs[key];

                // 스탯형 복원(패시브/일시적 공통)
                ApplyStatDelta(buff, apply: false);

                // 실제 상태 제거
                activeBuffs.Remove(key);

                // ⚠️ 예전 코드: buffUI?.ClearBuffByID(buff.BuffID);  ← 전역 삭제/타 진영 삭제 위험
                // 지금은 금지. 마지막에 RefreshBuffUI()로 안전 동기화
                Debug.Log($"[장비 해제] {itemID} → {key} 제거");
            }

            // 단 한 번의 동기화로 안전하게 반영
            RefreshBuffUI();
        }

        /// <summary>
        /// 버프 ID로 일시적 버프만 일괄 제거(패시브 제외)
        /// - BuffID 단독은 모호할 수 있으므로, 내부적으로 key를 조회 후 RemoveByKey 수행
        /// </summary>
        public void RemoveBuff(string buffID)
        {
            var toRemove = activeBuffs
                .Where(kv => kv.Value.BuffID == buffID && !kv.Value.IsPassive)
                .Select(kv => kv.Key)
                .ToList();

            foreach (var key in toRemove)
                RemoveBuffByKey(key);
        }

        /// <summary>
        /// 특정 key의 버프를 제거(일시적만)
        /// - 스탯형 복원 후 activeBuffs에서 제거
        /// - UI 직접 삭제 금지 → 마지막에 RefreshBuffUI()
        /// </summary>
        private void RemoveBuffByKey(string key)
        {
            if (!activeBuffs.TryGetValue(key, out var buff)) return;

            // 패시브는 코드로 제거하지 않음(장비 해제로만 제거)
            if (buff.IsPassive)
            {
                Debug.Log($"[패시브 유지] {key} from {buff.SourceItemID}");
                return;
            }

            // 스탯 복원 → 상태 제거
            ApplyStatDelta(buff, apply: false);
            activeBuffs.Remove(key);
            Debug.Log($"[Buff] Removed: {key}");

            // 상태 변경 직후 UI 동기화
            RefreshBuffUI();
        }

        /// <summary>전투 종료 시 일시적 버프 전부 제거(패시브는 유지)</summary>
        public void RemoveTemporaryBuffs()
        {
            var toRemove = activeBuffs
                .Where(kv => !kv.Value.IsPassive)
                .Select(kv => kv.Key)
                .ToList();

            foreach (var key in toRemove)
            {
                RemoveBuff(key);
                Debug.Log($"[전투 종료] 일시적 버프 제거됨: {key}");
            }
        }
        #endregion

        #region 생명주기(코루틴/갱신 훅)
        private void OnDisable()
        {
            // 씬 전환/비활성화 시 코루틴 정리
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
            // 재활성화 시: 전투 중이고 버프가 남아 있다면 틱 루틴 재시작
            if (activeBuffs.Count > 0 && gameFlowManager != null &&
                gameFlowManager.GetCurrentFlowState() == GameFlowManager.FlowState.Battle)
            {
                StartBuffRoutineSafe();
            }
        }
        #endregion
    }

    // ==============================
    // 아래는 보조 데이터/컨텍스트 클래스들
    // ==============================

    /// <summary>
    /// 옵션 실행 컨텍스트(아이템/사용자/대상/수치 등)
    /// - 장비 시스템/온힛 트리거 등에서 넘겨 쓰기 위한 컨테이너
    /// </summary>
    public class OptionContext
    {
        public PlayerState playerState;
        public Character User;      // 옵션을 사용하는 쪽
        public Character Target;    // 효과를 받는 쪽
        public int Value;
        public string item_ID;
        public string option_ID;
        public float hp; // 예시

        public override string ToString()
        {
            return $"[OptionContext] User: {User.name}, Target: {Target.name}, Value: {Value}, Item: {item_ID}, Option: {option_ID}";
        }
    }

    /// <summary>
    /// 자동 전투용 버프 데이터(패시브/일시적/디버프 공용)
    /// - Duration<=0f : 영구(패시브) 취급
    /// - Target/User를 포함하여 즉시형/틱형 효과에서 사용
    /// </summary>
    [System.Serializable]
    public class BuffData
    {
        public string BuffID;         // 논리 ID (ex. "crit_001", "burn_stack")
        public string OptionID;       // 효과 구분 ID (ex. "Option_002"=치확, "Option_003"=화상)
        public int Value;             // 수치(%) 혹은 정수

        public float Duration;        // 지속시간(초). 0 이하면 영구
        public float Elapsed = 0f;    // 경과시간(초)

        public string SourceItemID;   // 버프 유래(장비 ID 등). null/"" 이면 아이템 외 출처
        public bool IsPassive;        // 패시브 여부(장착 유지 시 무한)
        public bool IsDebuff;         // 디버프 여부(표시/정렬 등에 쓰일 수 있음)
        public Character Target;      // 효과를 받는 대상(예: 화상 피해 대상)
        public Character User;        // 시전자(본인)
    }
}
