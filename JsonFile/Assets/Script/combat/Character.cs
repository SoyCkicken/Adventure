using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MyGame
{
    public class Character : MonoBehaviour
    {
        [Header("캐릭터 기본 정보입니다 여기 값 + 능력치를 적용 시킬 예정입니다")]
        public string charaterName;
        public PlayerState playerState;
        public int MaxHealth = 50;
        public int Health;
        public int damage = 10;
        public float speed = 1f;
        public int armor = 5;
        public int CitChance = 10; //일반적인 크리티컬 확률
      
        public string weapon_Name;
        public string armor_Name;
        public string MonPas_Effect1;
        public int MonPas_Value1;
        public string MonPas_Effect2;
        public int MonPas_Value2;
        
        //옵션들 리스트에 기록
        public List<EquippedOption> OnHitOptions = new List<EquippedOption>();
        public List<MonsterOption> OnEnemyHitOptions = new List<MonsterOption>();
        public Dictionary<string, BuffData> activeBuffs = new();
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

       
        public void Heal(int amount)
        {
            Health += amount;
            Debug.Log($"{charaterName}이(가) {amount}만큼 회복. 현재 HP: {Health}");
        }
        // 방어구만큼 경감하고 남은 데미지를 HP에서 깎는다
        public int TakeDamage(int damage)
        {
            int reduced = Mathf.Max(damage - armor, 0);
            Health -= reduced;
            Debug.Log($"{charaterName}이(가) 받는 데미지: {damage} → 방어구 {armor} 경감 → 실제 {reduced}. 현재 HP: {Health}");
            if (Health <= 0)
            {
                //Destroy(this);
            }
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
        public void AddBuff(BuffData buff)
        {
            if (activeBuffs.ContainsKey(buff.BuffID))
            {
                Debug.Log($"버프 중복 적용 무시됨: {buff.BuffID}");
                return;
            }

            activeBuffs[buff.BuffID] = buff;

            if (buff.OptionID == "Option_002") // 치명타 확률 증가
            {
                Debug.Log($"치명타 확률 +{buff.Value}% 버프 적용됨");
                CitChance += buff.Value;
            }
                

            // 필요 시 스탯 반영
        }

    }
    //클래스들은 밑으로 뺐음
    public class OptionContext
    {
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
        public string SourceItemID;  // 버프 유래 (ex: 장비ID)
        public bool IsPassive;       // 패시브인지 여부
    }

    
}
