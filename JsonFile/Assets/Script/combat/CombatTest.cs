using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MyGame;
using UnityEngine.Playables;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;

public class CombatTest : MonoBehaviour
{
    public GameObject NormalBattle;

    public OptionManager optionManager;
    public MonsterOptionManager monsterOptionManager;
    public Character player;
    public Character enemy;
    public PlayerState playerState;
    public InventoryManager inventoryManager;
    public BattleUI battleUI;
    public BuffUI buffUI;
    public Animator enemyanima;
    public GameObject EnemyAttackImage;
    public GameObject ImageGameObject;
    public GameObject PopupObject;

    // 전투 완료 콜백
    private Action<bool> onComplete;
    // 전투 종료시 넘길 변수
    private bool battleOver;

    private void Awake()
    {
        NormalBattle.SetActive(false);
    }
    private void Start()
    {
        playerState = PlayerState.Instance;
    }
    /// <summary>
    /// GameFlowManager가 전투를 시작할 때 호출합니다.
    /// </summary>
    public void RunBattle(Action<bool> onComplete)
    {
        this.onComplete = onComplete;
        if(buffUI == null)
        buffUI = FindObjectOfType<BuffUI>();
        // 옵션 매니저 참조 확보
        if (optionManager == null)
            optionManager = FindObjectOfType<OptionManager>();
        enemyanima = enemy.GetComponent<Animator>();
        // 전투 상태 초기화
        battleOver = false;
        Debug.Log("전투로 넘어 갔습니다!");
        player.Health = player.MaxHealth;
        enemy.Health = enemy.MaxHealth;
        if(monsterOptionManager == null)
        monsterOptionManager = FindObjectOfType<MonsterOptionManager>();
        // 실제 전투 코루틴 실행
        StartCoroutine(ProcessBattle());
    }

    /// <summary>
    /// GameFlowManager가 전투를 중단할 때 호출합니다.
    /// </summary>
    public void StopBattle()
    {
        // 진행 중인 모든 코루틴 정지
        StopAllCoroutines();
        battleOver = true;
    }

    private IEnumerator ProcessBattle()
    {
        float playerChance = (float)player.speed / (player.speed + enemy.speed);
        float rand = Random.value; // 0.0 ~ 1.0
       
        bool playerGoesFirst = rand <= playerChance;
        //Debug.Log($"{rand}나온 값 {playerChance} 플레이어 선공권의 값 = {playerGoesFirst}누가 선공인가?");

        // 선공권: 한 번만 공격
        if (playerGoesFirst)
        {
            yield return StartCoroutine(AttackOnce(player, enemy, isPlayer: true, isEnemy: false));
        }
        else
        {
            yield return StartCoroutine(AttackOnce(enemy, player, isPlayer: false, isEnemy: true));
        }
        //Debug.LogWarning("");

        // 두 캐릭터의 공격루프를 동시에 돌리고,
        // 둘 다 끝날 때까지 대기했다가 onComplete 호출
        var playerLoop = StartCoroutine(AttackLoop(player, enemy, true,false));
        var enemyLoop = StartCoroutine(AttackLoop(enemy, player, false,true));

        yield return playerLoop;
        yield return enemyLoop;
        if (player.Health >= 1)
        {
            //Destroy(enemy.gameObject);
            //Debug.Log("플레이어가 승리하였습니다");
            //playerState.Experience += enemy.GetEXP;
            //playerState.statsUI.UpdateUI();
            //inventoryManager.updateSoulText();
            PopupObject.SetActive(true);
            battleOver = true;

            ConfirmPopup.Show($"전투에서 승리했습니다\n경험치 : {enemy.GetEXP}흭득", () =>
            {
                // ✅ 확인 버튼을 눌렀을 때만 실행
                playerState.Experience += enemy.GetEXP;
                playerState.statsUI.UpdateUI();
                inventoryManager.updateSoulText(); 
                

                player.GetComponent<EquipmentSystem>().Init();

                // 전투 결과 콜백 실행
                onComplete?.Invoke(true);
            }, false);
        }
        else
        {
            Debug.Log("플레이어가 패배하였습니다");
            battleOver = false;
            playerState.CurrentHealth--;
            playerState.CurrentMental--;
            PopupObject.SetActive(true);
            if (playerState.CurrentHealth <= 0 || playerState.CurrentMental <= 0)
            {
                SceneManager.LoadScene("GameOverScene");
            }
            NormalBattle.SetActive(false);
            PopupObject.SetActive(true);
            ConfirmPopup.Show("전투에서 패배했습니다", () =>
            {
                // ✅ 확인 버튼을 눌렀을 때만 실행
                player.GetComponent<EquipmentSystem>().Init();

                // 전투 결과 콜백 실행
                onComplete?.Invoke(true);
            }, false);

        }
        player.GetComponent<EquipmentSystem>().Init();
        //onComplete?.Invoke(battleOver); <-- 아마 여기로 안 들어와질꺼임
    }

    private IEnumerator AttackOnce(Character attacker, Character target, bool isPlayer, bool isEnemy)
    {
        yield return new WaitForSeconds(0.1f); // 약간의 텀
        if (attacker == enemy)
        { Debug.Log("적의 선공권"); }
        else if (attacker == player)
        {
            Debug.Log("플레이어의 선공권");
        }
        attacker.Attack(target); //이거 값에 넣지 않는 이유는 어차피 Attack에서 처리를 하고 있기 때문
        if (attacker == enemy)
        {
            var gameObject = Instantiate(EnemyAttackImage, ImageGameObject.transform.position, Quaternion.identity, ImageGameObject.transform.parent);
            gameObject.transform.localScale = new Vector3(50, 50, 0);
            Destroy(gameObject, 1.18f);
        }
        if (attacker == player)
        {
            enemyanima.Play("MS_Jombie_Hit");
        }

        if (isPlayer && attacker.OnHitOptions != null)
        {
            foreach (var opt in attacker.OnHitOptions)
            {
                var ctx = new OptionContext { User = attacker, Target = target, option_ID = opt.OptionID, Value = opt.Value };
                OptionManager.ApplyOnHitOnly(opt.OptionID, ctx);
            }
        }
        if (isEnemy && attacker.OnEnemyHitOptions != null)
        {
            foreach (var opt in attacker.OnEnemyHitOptions)
            {
                var ctx = new OptionContext { User = attacker, Target = target, option_ID = opt.OptionID, Value = opt.Value };
                if (opt.OptionID != "")
                    monsterOptionManager.ApplyOption(opt.OptionID, ctx);
            }
        }

        battleUI.UpdateUI();

        if (target.Health <= 0)
        {
            // attacker가 살아 있으면 attacker 승리 == 플레이어가 패배 한것

            Debug.Log(battleOver = (player.Health > 0));
            enemy.RemoveTemporaryBuffs();
            player.RemoveTemporaryBuffs();
            buffUI.Clear();
            NormalBattle.SetActive(false);
            PopupObject.SetActive(true);
            ConfirmPopup.Show($"전투에서 승리했습니다\n경험치 : {enemy.GetEXP}흭득", () =>
            {
                // ✅ 확인 버튼을 눌렀을 때만 실행
                playerState.Experience += enemy.GetEXP;
                playerState.statsUI.UpdateUI();
                inventoryManager.updateSoulText();

                player.GetComponent<EquipmentSystem>().Init();

                // 전투 결과 콜백 실행
                onComplete?.Invoke(true);
                PopupObject.SetActive(false);
            }, false);
            yield break;
        }
        else if (attacker.Health <= 0)
        {
            battleOver = false;
            enemy.RemoveTemporaryBuffs();
            player.RemoveTemporaryBuffs();
            buffUI.Clear();
            NormalBattle.SetActive(false);
            PopupObject.SetActive(true);
            ConfirmPopup.Show("전투에서 패배했습니다", () =>
            {
                // ✅ 확인 버튼을 눌렀을 때만 실행
                player.GetComponent<EquipmentSystem>().Init();

                // 전투 결과 콜백 실행
                onComplete?.Invoke(true);
                PopupObject.SetActive(false);
            }, false);
            yield break;
        }
    }

    private IEnumerator AttackLoop(Character attacker, Character target, bool isPlayer ,bool isEnemy)
    {
        while (!battleOver)
        {
            yield return new WaitForSeconds(1f / attacker.speed);
            if (battleOver) yield break;
            attacker.Attack(target);
            if (attacker == enemy)
            {
                var gameObject = Instantiate(EnemyAttackImage, ImageGameObject.transform.position, Quaternion.identity, ImageGameObject.transform.parent);
                gameObject.transform.localScale = new Vector3(75, 75, 0);
                Destroy(gameObject, 1f);
            }
            if (attacker == player)
            {
                enemyanima.Play("MS_Jombie_Hit"); 
            }
            // 플레이어 온히트 옵션 적용
            if (isPlayer && attacker.OnHitOptions != null)
            {
                foreach (var opt in attacker.OnHitOptions)
                {
                    var ctx = new OptionContext
                    {
                        User = attacker,
                        Target = target,
                        option_ID = opt.OptionID,
                        Value = opt.Value
                    };
                    OptionManager.ApplyOnHitOnly(opt.OptionID, ctx);
                }
            }
            if (isEnemy && attacker.OnEnemyHitOptions != null)
            {
                foreach (var opt in attacker.OnEnemyHitOptions)
                {
                    Debug.Log(attacker.OnEnemyHitOptions.Count);
                    var ctx = new OptionContext
                    {
                        User = attacker,
                        Target = target,
                        option_ID = opt.OptionID,
                        Value = opt.Value
                    };
                    //Debug.Log(ctx);
                    if (opt.OptionID != "")
                    {
                        monsterOptionManager.ApplyOption(opt.OptionID, ctx);
                    }
                }
                Debug.Log("<color=black>몬스터 온힛 효과 테스트 적용</color>");
                
            }

            battleUI.UpdateUI();

            // 죽음 판정
            if (target.Health <= 0)
            {
                // attacker가 살아 있으면 attacker 승리 == 플레이어가 패배 한것

                Debug.Log(battleOver = (player.Health > 0));
                enemy.RemoveTemporaryBuffs();
                player.RemoveTemporaryBuffs();
                buffUI.Clear();
                NormalBattle.SetActive(false);
                yield break;
            }
            else if(attacker.Health<=0)
            {
                battleOver = false;
                enemy.RemoveTemporaryBuffs();
                player.RemoveTemporaryBuffs();
                buffUI.Clear();
                NormalBattle.SetActive(false);
                yield break;
            }
        }
    }
}
