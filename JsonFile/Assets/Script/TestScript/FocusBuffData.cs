using UnityEngine;

namespace MyGame
{
    [System.Serializable]
    public class FocusBuffData
    {
        public string BuffID;           // 고유 식별자 (예: bleed_Arm)
        public string OptionID;         // 버프 종류 ID (예: Option_003 → 화상)
        public int Value;               // 수치 (예: 5 = 5%)

        public float Duration;          // 지속 시간
        public float Elapsed = 0f;      // 경과 시간

        public string SourceItemID;     // 무기나 스킬 ID
        public bool IsDebuff = true;    // 기본값 = 디버프

        public string PartName;         // 적용 부위 이름 (예: Arm, Leg)
        public float DamageRatio = 0.5f;// 데미지 분산 비율 (0.0 ~ 1.0)

        public TESTPlayer User;         // 공격한 쪽
        public TESTBoss Target;         // 피격 대상
    }
}
