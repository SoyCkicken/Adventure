using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;
using UnityEngine;

namespace MyGame
{
    public class Character : MonoBehaviour
    {
        public string charaterName;
        public int Health =50;
        public int damage = 10;
        public float speed = 1f;
        public int armor = 5;
        public string Option1_ID;
        public int Option1_Value;
        public int CitChance = 10; //일반적인 크리티컬 확률
        private Dictionary<string, int> critBuffs = new Dictionary<string, int>();

        public int CritChancePercent
            => CitChance + critBuffs.Values.Sum(); //모든 크리티컬 확률 증가 적용해서

        public void AddCritBuff(string buffID, int bonusPercent)
        {
            if (critBuffs.ContainsKey(buffID))
                return; // 이미 적용되었으면 무시

            critBuffs[buffID] = bonusPercent;
            Debug.Log($"[{charaterName}] 크리티컬 버프 적용: +{bonusPercent}%. 최종 확률 = {CritChancePercent}%");
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
            return reduced;
        }

        // 기본 공격 메서드
        public int Attack(Character target)
        {
            Debug.Log($"{charaterName}이(가) {target.charaterName}을(를) 공격: {damage} 데미지 시도");
            bool isCrit = Random.Range(0, 100) < CritChancePercent ? true : false;
            Debug.Log($"{isCrit} , {CritChancePercent} ");
            if (isCrit)
            {
                Debug.Log(damage * 2);
                return target.TakeDamage(damage * 2); }
            else
            { Debug.Log(damage);
                return target.TakeDamage(damage); }

               
        }
    }
    public class OptionContext
    {
        public Character User;      // 더미로 붙일 Character 컴포넌트
        public Character Target;
        public int hp;              //체력
        public int damage;
        public float speed;         //공격속도
        public int armor;           //방어력
        public int Value;           // 옵션 값
        public int DamageDealt;     // LifeSteal 용
        public int TurnNumber;      // Burn 스택용
    }
}
