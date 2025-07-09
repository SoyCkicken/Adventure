using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        public string charaterName;
        public PlayerState playerState;
        public int MaxHealth = 50;
        public int Health;
        public int damage;
        public float speed;
        public int armor;
        public int CitChance = 10; //일반적인 크리티컬 확률
      
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
        public BuffUI buffUI;
        //이 부분은 캐릭터 클래스의 하위에 있어야 되는 부분이라서 맨 아래로 안배고 맨 위에 넣음
        [System.Serializable]
        public struct EquippedOption
        {
            public string OptionID;
            public int Value;
            public string item_ID;
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
        public int Attack(Character target)
        {
            Debug.Log(damage);
            Debug.Log($"{charaterName}이(가) {target.charaterName}을(를) 공격: {damage} 데미지 시도");
            bool isCrit = UnityEngine.Random.Range(0, 100) < CitChance ? true : false;
            Debug.Log($"{isCrit} , {CitChance} ");
            
            if (isCrit)
            {
                Debug.Log(damage * 2);
                return target.TakeDamage(damage * 2); }
            else
            { Debug.Log(damage);
                return target.TakeDamage(damage); }

            
        }
        //원래 코드
        public void AddBuff(BuffData buff)
        {
            Debug.Log($"버프를 누가 사용중인가 {this.name}");
            if (activeBuffs.ContainsKey(buff.BuffID))
            {
                Debug.Log($" {buff.Target}에게 버프 중복 적용 무시됨: {buff.BuffID}");
                if (activeBuffs.TryGetValue(buff.BuffID, out var existing))
                {
                    // 중복일 경우: 시간만 초기화
                    existing.Elapsed = 0f;
                    existing.Duration = buff.Duration;
                    Debug.Log($"[Buff 갱신] {buff.BuffID} → 지속 시간 초기화");
                    return;
                }
                return;
            }

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
            buffUI.SetBuffs(activeBuffs.Values.ToList(),this);
            StartBuffRoutine();
            // 필요 시 스탯 반영
        }

        private IEnumerator BuffTickRoutine()
        {
            WaitForSeconds wait = new WaitForSeconds(1f);

            while (true)
            {
                var expired = new List<string>();

                foreach (var kv in activeBuffs)
                {
                    var buff = kv.Value;
                    buff.Elapsed += 1f;

                    // 매 1초마다 적용되는 효과
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

                // 만료되면 루프 종료
                if (activeBuffs.Count == 0)
                {
                    buffCoroutine = null;
                    yield break;
                }

                yield return wait;
            }
        }



        //장착 해제 시 버프 해제
        public void RemoveBuffByItem(string itemID)
        {
            var toRemove = activeBuffs
                .Where(kv => kv.Value.SourceItemID == itemID)
                .Select(kv => kv.Key)
                .ToList();

            foreach (var key in toRemove)
            {
                var buff = activeBuffs[key];

                // 스탯 롤백 처리
                //이건 버프로 능력치 올라갔던거 초기화 해주는부분인데 올라가는 부분이 없으면 빼고 작업하면됨
                if (buff.OptionID == "Option_002")
                    CitChance -= buff.Value;
                if (buff.OptionID == "Option_005")
                {
                    float multiplier = buff.Value / 100f;
                    speed /= (1f + multiplier);  // 원래 속도로 복원
                    Debug.Log($"[공속 복원] {charaterName} → speed 복구 = {speed}");
                }

                activeBuffs.Remove(key);
                Debug.Log($"[Buff 제거] {key}");
            }
            buffUI?.SetBuffs(activeBuffs.Values.ToList(), this);
        }

        //버프 제거용
        public void RemoveBuff(string buffID)
        {
            if (!activeBuffs.TryGetValue(buffID, out var buff))
                return;

            if (buff.IsPassive)
            {
                Debug.Log($"[패시브 버프 유지됨] {buffID}");
                return;
            }

            activeBuffs.Remove(buffID);
            buffUI?.SetBuffs(activeBuffs.Values.ToList(), this);
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
    }

    
}
