using UnityEngine;

namespace MyGame
{
    public class Character : MonoBehaviour
    {
        public string charaterName;
        public int Health =50;
    }
    public class OptionContext
    {
        public Character User;      // 더미로 붙일 Character 컴포넌트
        public Character Target;
        public int hp;              //체력
        public int damage;
        public int Value;           // 옵션 값
        public int DamageDealt;     // LifeSteal 용
        public int TurnNumber;      // Burn 스택용
    }
}
