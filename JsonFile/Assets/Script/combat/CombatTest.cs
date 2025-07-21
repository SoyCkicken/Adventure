using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MyGame;
using UnityEngine.Playables;

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

    // 전투 완료 콜백
    private Action<bool> onComplete;
    // 전투 종료시 넘길 변수
    private bool battleOver;

    private void Awake()
    {
        NormalBattle.SetActive(false);
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
        // 두 캐릭터의 공격루프를 동시에 돌리고,
        // 둘 다 끝날 때까지 대기했다가 onComplete 호출
        var playerLoop = StartCoroutine(AttackLoop(player, enemy, true,false));
        var enemyLoop = StartCoroutine(AttackLoop(enemy, player, false,true));

        yield return playerLoop;
        yield return enemyLoop;
        if (player.Health >= 1)
        {
            //Destroy(enemy.gameObject);
            Debug.Log("플레이어가 승리하였습니다");
            playerState.Experience += enemy.GetEXP;
            playerState.statsUI.UpdateUI();
            inventoryManager.updateSoulText();

            battleOver = true;
        }
        else
        {
            Debug.Log("플레이어가 패배하였습니다");
            battleOver = false;
            
        }
        player.GetComponent<EquipmentSystem>().Init();
        onComplete?.Invoke(battleOver);
    }

    private IEnumerator AttackLoop(Character attacker, Character target, bool isPlayer ,bool isEnemy)
    {
        while (!battleOver)
        {
            yield return new WaitForSeconds(1f / attacker.speed);
            if (battleOver) yield break;

            int dealt = attacker.Attack(target);
            if (attacker == enemy)
            {
                var gameObject = Instantiate(EnemyAttackImage, ImageGameObject.transform.position, Quaternion.identity,ImageGameObject.transform.parent);
                gameObject.transform.localScale = new Vector3(50, 50, 0);
                Destroy(gameObject, 1f);
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
                yield break;
            }
            else if(attacker.Health<=0)
            {
                battleOver = false;
                enemy.RemoveTemporaryBuffs();
                player.RemoveTemporaryBuffs();
                buffUI.Clear();
                yield break;
            }
        }
    }
}
