using UnityEngine;
using MyGame;

public class CombatSystem : MonoBehaviour
{
        public OptionManager optionManager;
   
    void Start()
        {
            if (optionManager == null)
                optionManager = FindObjectOfType<OptionManager>();

            // 더미 캐릭터 생성
            var userGO = new GameObject("User");
            var targetGO = new GameObject("Target");
            var user = userGO.AddComponent<Character>();
            var target = targetGO.AddComponent<Character>();
            user.name = "User";
            target.name = "Target";

            // --- Bleed Test ---
            Debug.Log("=== BleedEffect Test ===");
        var bleedCtx = new OptionContext
        {
            User = user,
            Target = target,
            hp = 50,
                Value = 10            // 예: 초당 10 데미지
        };
            optionManager.ApplyOption("Effect_Bleed", bleedCtx);

            // --- SpeedUp Test ---
            Debug.Log("=== SpeedUpEffect Test ===");
            var speedCtx = new OptionContext
            {
                User = user,
                Target = target,
                Value = 20           // 예: 이동속도 +20%
            };
            optionManager.ApplyOption("Option_SpeedUp", speedCtx);

            // --- LifeSteal Test ---
            Debug.Log("=== LifeStealEffect Test ===");
            int dealtDmg = 50;       // 임의의 가한 데미지
            var lifeCtx = new OptionContext
            {
                User = user,
                Target = target,
                Value = 5,     // 예: 5% 흡혈
                DamageDealt = dealtDmg
            };
            optionManager.ApplyOption("Option_LifeSteal", lifeCtx);

            // --- Burn Test ---
            Debug.Log("=== BurnEffect Test ===");
            int currentTurn = 3;     // 예: 3턴째
            var burnCtx = new OptionContext
            {
                User = user,
                Target = target,
                Value = 15,     // 예: 기본 15 화상 데미지
                TurnNumber = currentTurn
            };
            optionManager.ApplyOption("Option_Burn", burnCtx);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

