using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;
using UnityEngine;

namespace MyGame
{
    public class Character : MonoBehaviour
    {
        [Header("캐릭터 기본 정보입니다 여기 값 + 능력치를 적용 시킬 예정입니다")]
        public string charaterName;
        public int Health =50;
        public int damage = 10;
        public float speed = 1f;
        public int armor = 5;
        public int CitChance = 10; //일반적인 크리티컬 확률
        public Dictionary<string, int> critBuffs = new Dictionary<string, int>();
        public string weapon_Name;
        public string armor_Name;
        //람다식이라서 출력이 안되는거임
        public int CritChancePercent 
            => CitChance + critBuffs.Values.Sum(); //모든 크리티컬 확률 증가 적용해서
        [System.Serializable]
        public struct EquippedOption
        {
            public string OptionID;
            public int Value;
            public string item_ID;
        }
        public List<EquippedOption> OnHitOptions = new List<EquippedOption>();
        public void ApplyWeapon(Weapon_Master weapon)
        {
            if (weapon == null) return;

            damage += weapon.Weapon_DMG;
            Debug.Log($"[{charaterName}] '{weapon.Weapon_Name}' 장착 → 공격력 +{weapon.Weapon_DMG} → 최종 공격력 {damage}");
        }
        /// <summary>
        /// 방어구 장착 시 방어력 & 체력에 추가
        /// </summary>
        public void ApplyArmor(Armor_Master Armor)
        {
            if (Armor == null) return;

            armor += Armor.Armor_DEF;
            Health += Armor.Armor_HP;
            Debug.Log($"[{charaterName}] '{Armor.Armor_NAME}' 장착 → 방어력 +{Armor.Armor_DEF}, 체력 +{Armor.Armor_HP} → 최종 방어력 {armor}, 체력 {Health}");
        }

        public void AddCritBuff(string buffID, int bonusPercent)
        {
            Debug.Log($"{buffID} : {bonusPercent}");
            if (critBuffs.ContainsKey(buffID))
            {
                //Debug.Log("중복 적용 되었습니다");
                return; // 이미 적용되었으면 무시

            }
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
            Debug.Log(damage);
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
        public string option_ID;
        public int Value;           // 옵션 값
        public string item_ID;
       
    }
}
