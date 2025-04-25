using UnityEngine;

namespace MyGame
{
    public class Character : MonoBehaviour
    {
        public string charaterName;
        public int Health;

        public void Heal(int value)
        {
            Health += value;
            Debug.Log($"{charaterName}가 {value}만큼 회복 했습니다");
        }
        public void bleedCtx(int value)
        {
            Health -= value;
            Debug.Log($"{charaterName}가 {value}만큼 달았습니다");
        }
    }
    public class OptionContext
    {
        public Character User;      // 더미로 붙일 Character 컴포넌트
        public Character Target;
        public int hp;              //체력
        public int Value;           // 옵션 값
        public int DamageDealt;     // LifeSteal 용
        public int TurnNumber;      // Burn 스택용
    }
}
