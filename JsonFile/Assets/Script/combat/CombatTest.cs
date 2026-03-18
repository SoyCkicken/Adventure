using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MyGame;
using UnityEngine.Playables;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
using Spine;
using DG.Tweening;

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
    public Animator Enemy_Animator;
    public GameObject EnemyAttackImage;
    public GameObject ImageGameObject;
    public GameObject PopupObject;
    public ScreenShake _screenShake;
    // 전투 완료 콜백
    private Action<bool> onComplete;
    // 전투 종료시 넘길 변수
    private bool battleOver;

    private void Awake()
    {
        optionManager = OptionManager.Instance;
        monsterOptionManager = MonsterOptionManager.Instance;
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
        Enemy_Animator = enemy.GetComponent<Animator>();
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
        float rand = Random.value;
        bool playerGoesFirst = rand <= playerChance;

        if (playerGoesFirst)
        {
            yield return StartCoroutine(AttackOnce(player, enemy, isPlayer: true, isEnemy: false));
        }
        else
        {
            yield return StartCoroutine(AttackOnce(enemy, player, isPlayer: false, isEnemy: true));
        }

        // ✅ 선공권 공격 이후 즉사 체크
        if (player.Health <= 0 || enemy.Health <= 0)
        {
            Debug.Log("[선공권] 공격 한방에 전투 종료");

            // 전투 정리는 CheckDeath에서 이미 처리됨
            battleOver = true;
            yield return new WaitUntil(() => battleOver);

            if (player.Health >= 1)
            {
                HandleBattleWin(enemy.GetEXP, enemy);
            }
            else
            {
                HandleBattleLose();
            }

            player.GetComponent<EquipmentSystem>().Init();
            yield break;
        }

        // 일반 턴 전투 루프
        var playerLoop = StartCoroutine(AttackLoop(player, enemy, true, false));
        var enemyLoop = StartCoroutine(AttackLoop(enemy, player, false, true));

        yield return new WaitUntil(() => battleOver);

        if (player.Health >= 1)
        {
            HandleBattleWin(enemy.GetEXP, enemy);
        }
        else
        {
            HandleBattleLose();
        }

        player.GetComponent<EquipmentSystem>().Init();
    }

    private void HandleBattleWin(int exp, Character enemy)
    {
        PopupObject.SetActive(true);
        battleOver = true;
        //enemy.RemoveTemporaryBuffs();
        //player.RemoveTemporaryBuffs();
        //buffUI.ClearAll();
        NormalBattle.SetActive(false);

        ConfirmPopup.Show($"전투에서 승리했습니다\n경험치 : {exp}흭득", () =>
        {
            playerState.Experience += exp;
            playerState.statsUI.UpdateUI();
            inventoryManager.UpdateGoldText();
            player.GetComponent<EquipmentSystem>().Init();
            onComplete?.Invoke(true);
        }, false);
    }

    private void HandleBattleLose()
    {
        battleOver = false;
        playerState.CurrentHealth--;
        playerState.CurrentMental--;
        //enemy.RemoveTemporaryBuffs();
        //player.RemoveTemporaryBuffs();
        //buffUI.ClearAll();
        PopupObject.SetActive(true);
        NormalBattle.SetActive(false);

        if (playerState.CurrentHealth <= 0 || playerState.CurrentMental <= 0)
        {
            SceneManager.LoadScene("GameOverScene");
            return;
        }

        //NormalBattle.SetActive(false);

        ConfirmPopup.Show("전투에서 패배했습니다", () =>
        {
            player.GetComponent<EquipmentSystem>().Init();
            onComplete?.Invoke(false);
        }, false);
    }

    private IEnumerator AttackOnce(Character attacker, Character target, bool isPlayer, bool isEnemy)
    {
        yield return new WaitForSeconds(0.1f); // 약간의 텀
        //attacker.Attack(target);
        PlayAttackEffect(attacker);
        // 플레이어 온히트 옵션 적용
        ApplyOnHitOptions(attacker, target, isPlayer, isEnemy);

        battleUI.UpdateUI();

        // 죽음 판정
        CheckDeath(attacker, target);
    }

    private IEnumerator AttackLoop(Character attacker, Character target, bool isPlayer ,bool isEnemy)
    {
        while (!battleOver)
        {
            yield return new WaitForSeconds(1f / attacker.speed);
            if (battleOver) yield break;
            //attacker.Attack(target);
            PlayAttackEffect(attacker);
            // 플레이어 온히트 옵션 적용
            ApplyOnHitOptions(attacker, target, isPlayer, isEnemy);

            battleUI.UpdateUI();

            // 죽음 판정
            CheckDeath(attacker, target);
        }
    }
    private void PlayAttackEffect(Character attacker)
    {
        if (attacker == enemy)
        {
            var (damage, iscrit) = attacker.Attack(player);
            if (iscrit)
            {
                var gameObject = Instantiate(EnemyAttackImage, ImageGameObject.transform.position, Quaternion.identity, ImageGameObject.transform.parent);
                gameObject.transform.localScale = new Vector3(100, 100, 0);
                Destroy(gameObject, 1f);
                if (iscrit)
                {
                    if (_screenShake == null)
                        _screenShake = FindObjectOfType<ScreenShake>(); // 또는 DI로 주입
                    if (_screenShake != null)
                    {
                        // DOVirtual.DelayedCall은 특정 타겟에 묶이지 않아 fx가 파괴되어도 안전.
                        DOVirtual.DelayedCall(0.4f, () =>
                        {
                            // 일반 히트

                            _screenShake.Shake();
                            // 크리티컬이면 강하게 (원하면 주석 해제)

                        })
                        .SetUpdate(true); // 타임스케일 무시(연출용)
                        Debug.Log(attacker.speed);
                    }
                }
            }
            if (gameObject == null)
            {
                return;
            }
        }

        if (attacker == player)
        {
            var (damage, isCrit) = attacker.Attack(enemy);
            if (isCrit)
            {
                Debug.Log("크리티컬 공격입니다");
                Enemy_Animator.SetTrigger("isHit");
            }
           
        }
    }

    private void ApplyOnHitOptions(Character attacker, Character target, bool isPlayer, bool isEnemy)
    {
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
                var ctx = new OptionContext
                {
                    User = attacker,
                    Target = target,
                    option_ID = opt.OptionID,
                    Value = opt.Value
                };

                if (!string.IsNullOrEmpty(opt.OptionID))
                {
                    monsterOptionManager.ApplyOption(opt.OptionID, ctx);
                }
            }
            Debug.Log("<color=black>몬스터 온힛 효과 테스트 적용</color>");
        }
    }
    private bool CheckDeath(Character attacker, Character target)
    {
        if (target.Health <= 0 || attacker.Health <= 0)
        {
            Debug.Log($"[CheckDeath] 사망 감지: {target.charaterName} HP={target.Health}, {attacker.charaterName} HP={attacker.Health}");

            // 버프만 정리하고 캔버스는 유지
            CleanupBuffsOnly();

            battleOver = true;
            return true;
        }
        return false;
    }

    private void CleanupBuffsOnly()
    {
        try
        {
            // 버프만 정리
            enemy?.RemoveTemporaryBuffs();
            player?.RemoveTemporaryBuffs();
            buffUI?.ClearAll();

            Debug.Log("[Combat] 버프 정리 완료 (캔버스 유지)");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[Combat] 버프 정리 중 오류: {e.Message}");
        }
    }
}
