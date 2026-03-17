//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using Unity.Mathematics;
//using UnityEngine;
//using Random = UnityEngine.Random;

//namespace MyGame
//{
//    /// <summary>
//    /// 전투 로직(자동 전투용 버프/디버프, 집중전투용 디버프)을 모두 다루는 캐릭터 컴포넌트
//    /// - 버프의 "실제 상태"는 activeBuffs 딕셔너리에만 존재(싱글 소스 오브 트루스)
//    /// - UI는 매번 RefreshBuffUI()를 통해 activeBuffs 상태를 그대로 반영(표시만)
//    /// - 시간 진행(Elapsed 증감)은 이 클래스만 책임 (BuffIconUI는 표시만)
//    /// </summary>
//    public class Character : MonoBehaviour
//    {
//        /// <summary>
//        /// 패시브 버프의 "장착 유지" 여부를 판단하는 질의 인터페이스
//        /// - EquipmentSystem에서 구현체를 주입하여, 특정 ItemID가 현재 장착 중인지 확인
//        /// </summary>
//        public interface IEquipmentQuery
//        {
//            bool IsItemEquipped(string itemID);
//        }

//        [Header("캐릭터 기본 정보(능력치 계산의 베이스)")]
//        public GameFlowManager gameFlowManager; // 전투 상태 확인용(자동 전투 틱 허용 여부)
//        public string charaterName;

//        [SerializeField] PlayerState playerState;
//        public int MaxHealth = 50;
//        public int Health;
//        public int damage;
//        public float speed;
//        public int armor;
//        public int CitChance = 10; // 기본 치확
//        public int GetEXP;

//        [Header("장비/몬스터 패시브 원천 정보")]
//        public string weapon_Name;
//        public string armor_Name;
//        public string MonPas_Effect1;
//        public int MonPas_Value1;
//        public string MonPas_Effect2;
//        public int MonPas_Value2;

//        // 자동 전투 버프 틱 코루틴 핸들
//        private Coroutine buffCoroutine;

//        [Header("보유 옵션(온힛/피격시 발동 등)")]
//        public List<EquippedOption> OnHitOptions = new List<EquippedOption>();      // 본인 공격 시 발동
//        public List<MonsterOption> OnEnemyHitOptions = new List<MonsterOption>();   // 적의 공격(피격) 시 발동

//        /// <summary>
//        /// 자동 전투에서 적용되는 현재 "모든" 버프(패시브/일시적/디버프 포함)
//        /// 키: BuffID[:SourceItemID] (같은 버프라도 출처가 다르면 별개로 관리)
//        /// </summary>
//        private readonly Dictionary<string, BuffData> activeBuffs = new();

//        // UI 핸들
//        public BattleUI battleUI;
//        public BuffUI buffUI;
//        private Coroutine uiRefreshRoutine;

//        /// <summary>장비 장착 여부 질의(패시브 무한 지속 판단용)</summary>
//        public IEquipmentQuery equipmentQuery; // EquipmentSystem에서 주입

//        /// <summary>
//        /// 패시브이면서 해당 아이템이 현재 장착 상태라면 "시간 진행을 멈춰" 무한 지속으로 취급.
//        /// (= Elapsed를 0으로 유지)  ※ 집중전투와는 무관, 자동 전투용 버프에만 적용
//        /// </summary>
//        bool IsStillEquipped(BuffData buff)
//            => buff.IsPassive && equipmentQuery != null && equipmentQuery.IsItemEquipped(buff.SourceItemID);

//        // NOTE: 집중전투 관련 필드가 파일 하단이 아닌 상단에 위치해야 해서 여기 배치

//        private void Start()
//        {
//            playerState = PlayerState.Instance;
//        }

//        [System.Serializable]
//        public struct EquippedOption
//        {
//            public string OptionID;
//            public int Value;
//            public string item_ID; // 출처 아이템(패시브 연동)
//        }

//        /// <summary>
//        /// 자동 전투 버프의 유니크 키 생성
//        /// - 동일 BuffID라도 SourceItemID가 다르면 별개로 관리
//        /// - OnHit 등 아이템이 아닌 출처면 SourceItemID는 비워둘 수 있음
//        /// </summary>
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

//        #region 전투/피해 처리(자동 전투 체계)
//        // 방어구만큼 경감하고 남은 데미지를 HP에서 깎는다(자동 전투 체계)
//        public int TakeDamage(int damage)
//        {
//            int reduced = Mathf.Max(damage - armor, 0);
//            Health -= reduced;
//            Debug.Log($"{charaterName}이(가) 받는 데미지: {damage} → 방어구 {armor} 경감 → 실제 {reduced}. 현재 HP: {Health}");
//            return reduced;
//        }

//        // 기본 공격 (자동 전투 체계)
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
//        #endregion

//        #region 자동 전투: 버프 추가/갱신/적용
//        /// <summary>
//        /// 자동 전투 버프 추가
//        /// - 동일 키(BuffID[:Source])가 존재하면 Duration/Value 갱신 + Elapsed 리셋
//        /// - 신규면 등록 후 "즉시형 효과" 1회 반영(예: 화상/회복 시작 즉시)
//        /// - 스탯형(치확/공속 등)은 ApplyStatDelta로 즉시 반영
//        /// - UI는 "항상" RefreshBuffUI() 한 번으로 동기화(직접 삭제 X)
//        /// </summary>
//        public void AddBuff(BuffData buff)
//        {
//            if (buff == null) return;
//            string key = GetBuffKey(buff);

//            // 동일 출처/동일 BuffID가 이미 있으면 → 갱신(지속시간 리셋 & 값 갱신)
//            if (activeBuffs.TryGetValue(key, out var existing))
//            {
//                existing.Duration = buff.Duration;
//                existing.Elapsed = 0f;
//                existing.Value = buff.Value; // 필요 시 수치 갱신
//                Debug.Log($"[Buff] Refresh: {key} (Duration reset, Value={existing.Value})");

//                // UI 동기화 + 틱 루틴 보장
//                RefreshBuffUI();

//                // 디버깅: 이 캐릭터의 GM 주입 여부를 확인
//                Debug.Log($"[Buff] Add: {key} ({buff.OptionID}) on {charaterName}, GM={gameFlowManager != null}");
//                return;
//            }

//            // 신규 등록
//            activeBuffs[key] = buff;

//            // 즉시형 효과 1회(시작 즉시)
//            switch (buff.OptionID)
//            {
//                case "Option_003": // 화상 즉시 2%(Tick은 별도 루틴에서 1초마다 반복)
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

//            // 스탯형(치확/공속 등)은 즉시 적용
//            ApplyStatDelta(buff, apply: true);

//            // UI 동기화 + 틱 루틴 보장
//            RefreshBuffUI();

//            Debug.Log($"[Buff] Add: {key} ({buff.OptionID}) from {buff.SourceItemID}");
//        }
//        #endregion

//        #region 자동 전투: 틱 루틴(시간 진행/만료/주기 효과)
//        /// <summary>
//        /// 버프 틱 루틴(자동 전투)
//        /// - 1초마다 Elapsed += 1f (실시간 경과)
//        /// - 패시브 + 장착 유지 중이면 Elapsed=0으로 고정(무한)
//        /// - Tick 효과(화상/회복 등)는 1초마다 한 번씩 적용
//        /// - 만료(Duration > 0 && Elapsed >= Duration) 시 Remove
//        ///
//        /// [중요 변경: A안]
//        /// gameFlowManager == null 인 경우도 "전투 중"으로 간주하여 루틴이 멈추지 않게 함.
//        /// (적 캐릭터/소환수 등 GM 주입이 없을 때 FillAmount가 0에서 안 오르던 문제 해결)
//        /// </summary>
//        private IEnumerator BuffTickRoutine()
//        {
//            // 1초 틱(프레임 기반으로 바꾸려면 yield return null + Time.deltaTime 누적 구조로)
//            WaitForSeconds wait = new WaitForSeconds(1f);

//            while (true)
//            {
//                // 전투가 아니라는 "명시적" 신호가 있을 때만 쉰다.
//                // GM이 null이면 전투 중으로 간주(= 루틴 지속)  ← A안 핵심
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

//                    // 패시브인데 현재도 장착 유지면 시간 고정(무한 지속)
//                    if (IsStillEquipped(buff))
//                    {
//                        buff.Elapsed = 0f;
//                        continue;
//                    }

//                    // 1초 경과
//                    buff.Elapsed += 1f;

//                    // 1초 Tick 효과(지속 피해/지속 회복 등)
//                    switch (buff.OptionID)
//                    {
//                        case "Option_003": // 화상 Tick
//                            int dmg = Mathf.FloorToInt(buff.Target.MaxHealth * 0.02f);
//                            buff.Target.Health -= dmg;
//                            Debug.Log($"[화상 Tick] {buff.Target.charaterName} -{dmg}");
//                            break;

//                        case "Option_004": // 회복 Tick
//                            int heal = Mathf.FloorToInt(buff.Target.MaxHealth * 0.02f);
//                            buff.Target.Health = Mathf.Min(buff.Target.MaxHealth, buff.Target.Health + heal);
//                            Debug.Log($"[회복 Tick] {buff.Target.charaterName} +{heal}");
//                            break;
//                    }

//                    // 만료 체크(0 이하 = 영구/패시브)
//                    if (buff.Duration > 0 && buff.Elapsed >= buff.Duration)
//                        expired.Add(kv.Key);
//                }

//                // 만료 반영(주의: Remove 내부에서 UI를 직접 지우지 않고 Refresh로 동기화)
//                foreach (var key in expired)
//                    RemoveBuff(key);

//                // 더 이상 버프가 없으면 루틴 종료(다음 AddBuff 시 재시작)
//                if (activeBuffs.Count == 0)
//                {
//                    buffCoroutine = null;
//                    yield break;
//                }

//                // UI 갱신
//                battleUI?.UpdateUI();
//                yield return wait;
//            }
//        }
//        #endregion

//        #region 자동 전투: UI 동기화 유틸리티
//        /// <summary>
//        /// UI 반영을 1프레임 지연시켜 "여러 버프 변경"을 한 번에 묶어 그림
//        /// 비활성/미활성 상태면 코루틴 대신 즉시 적용
//        /// </summary>
//        private void RefreshBuffUI()
//        {
//            if (gameObject != null && gameObject.activeInHierarchy && enabled)
//            {
//                if (uiRefreshRoutine != null) StopCoroutine(uiRefreshRoutine);
//                uiRefreshRoutine = StartCoroutine(DelayUIRefresh());
//            }
//            else
//            {
//                RefreshBuffUIDirect();
//            }
//        }

//        /// <summary>코루틴을 쓸 수 없는 상황(비활성/씬 전환 등)에서 즉시 UI 갱신</summary>
//        private void RefreshBuffUIDirect()
//        {
//            try
//            {
//                // BuffUI는 캐릭터별 맵을 분리하여, 넘겨준 목록만 표시/유지한다.
//                buffUI?.SetBuffs(activeBuffs.Values.ToList(), this);
//                Debug.Log($"[Buff] UI 직접 갱신 완료 for {gameObject.name}");
//            }
//            catch (System.Exception e)
//            {
//                Debug.LogWarning($"[Buff] UI 갱신 실패: {e.Message}");
//            }
//        }

//        /// <summary>1프레임 대기 후 UI 갱신(프레임 내 변경 사항을 모두 반영하도록)</summary>
//        private IEnumerator DelayUIRefresh()
//        {
//            yield return null;
//            buffUI?.SetBuffs(activeBuffs.Values.ToList(), this);
//            uiRefreshRoutine = null;
//        }
//        #endregion

//        #region 스탯형 옵션 적용/복원
//        /// <summary>
//        /// 스탯형 옵션(치확/공속 등)을 적용/복원
//        /// - AddBuff 시 apply=true 로 적용
//        /// - Remove 계열 시 apply=false 로 복원
//        /// </summary>
//        private void ApplyStatDelta(BuffData buff, bool apply)
//        {
//            switch (buff.OptionID)
//            {
//                case "Option_002": // 치확 ±Value
//                    CitChance += apply ? buff.Value : -buff.Value;
//                    break;

//                case "Option_005": // 공속 ±(Value%)
//                    float mul = buff.Value / 100f;
//                    if (apply) speed *= (1f + mul);
//                    else speed /= (1f + mul);
//                    break;
//            }
//        }
//        #endregion

//        #region 장비/버프 제거(자동 전투)
//        /// <summary>
//        /// 장비 해제 시 해당 아이템에서 파생된 모든 버프 제거
//        /// [중요] UI를 직접 지우지 않고, activeBuffs만 수정 → 마지막에 RefreshBuffUI()
//        /// </summary>
//        public void RemoveBuffByItem(string itemID)
//        {
//            var keys = activeBuffs
//                .Where(kv => kv.Value.SourceItemID == itemID)
//                .Select(kv => kv.Key)
//                .ToList();

//            foreach (var key in keys)
//            {
//                var buff = activeBuffs[key];

//                // 스탯형 복원(패시브/일시적 공통)
//                ApplyStatDelta(buff, apply: false);

//                // 실제 상태 제거
//                activeBuffs.Remove(key);

//                // ⚠️ 예전 코드: buffUI?.ClearBuffByID(buff.BuffID);  ← 전역 삭제/타 진영 삭제 위험
//                // 지금은 금지. 마지막에 RefreshBuffUI()로 안전 동기화
//                Debug.Log($"[장비 해제] {itemID} → {key} 제거");
//            }

//            // 단 한 번의 동기화로 안전하게 반영
//            RefreshBuffUI();
//        }

//        /// <summary>
//        /// 버프 ID로 일시적 버프만 일괄 제거(패시브 제외)
//        /// - BuffID 단독은 모호할 수 있으므로, 내부적으로 key를 조회 후 RemoveByKey 수행
//        /// </summary>
//        public void RemoveBuff(string buffID)
//        {
//            var toRemove = activeBuffs
//                .Where(kv => kv.Value.BuffID == buffID && !kv.Value.IsPassive)
//                .Select(kv => kv.Key)
//                .ToList();

//            foreach (var key in toRemove)
//                RemoveBuffByKey(key);
//        }

//        /// <summary>
//        /// 특정 key의 버프를 제거(일시적만)
//        /// - 스탯형 복원 후 activeBuffs에서 제거
//        /// - UI 직접 삭제 금지 → 마지막에 RefreshBuffUI()
//        /// </summary>
//        private void RemoveBuffByKey(string key)
//        {
//            if (!activeBuffs.TryGetValue(key, out var buff)) return;

//            // 패시브는 코드로 제거하지 않음(장비 해제로만 제거)
//            if (buff.IsPassive)
//            {
//                Debug.Log($"[패시브 유지] {key} from {buff.SourceItemID}");
//                return;
//            }

//            // 스탯 복원 → 상태 제거
//            ApplyStatDelta(buff, apply: false);
//            activeBuffs.Remove(key);
//            Debug.Log($"[Buff] Removed: {key}");

//            // 상태 변경 직후 UI 동기화
//            RefreshBuffUI();
//        }

//        /// <summary>전투 종료 시 일시적 버프 전부 제거(패시브는 유지)</summary>
//        public void RemoveTemporaryBuffs()
//        {
//            var toRemove = activeBuffs
//                .Where(kv => !kv.Value.IsPassive)
//                .Select(kv => kv.Key)
//                .ToList();

//            foreach (var key in toRemove)
//            {
//                RemoveBuff(key);
//                Debug.Log($"[전투 종료] 일시적 버프 제거됨: {key}");
//            }
//        }
//        #endregion

//        #region 생명주기(코루틴/갱신 훅)
//        private void OnDisable()
//        {
//            // 씬 전환/비활성화 시 코루틴 정리
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
//        #endregion
//    }

//    // ==============================
//    // 아래는 보조 데이터/컨텍스트 클래스들
//    // ==============================

//    /// <summary>
//    /// 옵션 실행 컨텍스트(아이템/사용자/대상/수치 등)
//    /// - 장비 시스템/온힛 트리거 등에서 넘겨 쓰기 위한 컨테이너
//    /// </summary>

//}

//using System;
//using UnityEngine;
//using MyGame;
//using UnityEngine.TextCore.Text;

//    public class Character : MonoBehaviour
//    {
//        [Header("캐릭터 기본 데이터")]
//        public string charaterName;
//        public int MaxHealth = 50;
//        public int Health;
//        public int damage;
//        public float speed;
//        public int armor;

//    [SerializeField] PlayerState playerState;
//    public int CitChance = 10; // 기본 치확
//    public int GetEXP;

//    [Header("장비/몬스터 패시브 원천 정보")]
//    public string weapon_Name;
//    public string armor_Name;
//    public string MonPas_Effect1;
//    public int MonPas_Value1;
//    public string MonPas_Effect2;

//    // --- [분리 핵심] 이벤트를 통해 UI나 다른 시스템에 알림 ---
//    // (현재 체력, 최대 체력)을 전달하는 이벤트
//    public Action<int, int> OnHealthChanged;
//        // 캐릭터가 사망했을 때 발생하는 이벤트
//        public Action OnDeath;

//        private void Awake()
//        {
//            Health = MaxHealth;
//        }

//        // 캐릭터는 이제 "내 데이터가 변했다"는 사실만 외부에 알립니다.
//        public void TakeDamage(int amount)
//        {
//            Health -= amount;
//            Health = Mathf.Max(0, Health); // 0 이하로 내려가지 않게 방어

//            // 나를 지켜보고 있는(구독한) UI나 매니저에게 알림
//            OnHealthChanged?.Invoke(Health, MaxHealth);

//            if (Health <= 0)
//            {
//                OnDeath?.Invoke();
//            }
//        }
//    public void SetupStats(int hp, int atk, int def, float spd, int crit)
//    {
//        this.MaxHealth = hp;
//        this.Health = hp;
//        this.damage = atk;
//        this.armor = def;
//        this.speed = spd;
//        this.CitChance = crit;

//        // 능력치가 설정되었으니 UI에도 알려줍니다.
//        OnHealthChanged?.Invoke(Health, MaxHealth);
//    }
//}

using System;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public string charaterName;
    public int MaxHealth = 50;
    public int Health;
    public int damage;
    public int armor;
    public int CitChance = 10;

    // 장비에서 넘어온 특수 옵션 리스트
    public List<ItemOption> currentOptions = new List<ItemOption>();

    // UI에 알림을 보내는 이벤트
    public Action<int, int> OnHealthChanged;

    private void Awake()
    {
        Health = MaxHealth;
    }


    public void SetupStats(int hp, int atk, int def, int crit)
    {
        this.MaxHealth = hp;
        this.Health = hp;
        this.damage = atk;
        this.armor = def;
        this.CitChance = crit;
        OnHealthChanged?.Invoke(Health, MaxHealth);
    }

    public void TakeDamage(int amount)
    {
        Health -= amount;
        Health = Mathf.Max(0, Health);
        OnHealthChanged?.Invoke(Health, MaxHealth);
    }
}
public class OptionContext
{
    public PlayerState playerstate;
    public Character user;      // 옵션을 사용하는 쪽
    public Character target;    // 효과를 받는 쪽
    public int value;
    public string item_id;
    public string option_id;
    public float hp; // 예시
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

