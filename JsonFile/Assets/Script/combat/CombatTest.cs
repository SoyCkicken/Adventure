//using System;
//using System.Collections;
//using UnityEngine;
//using UnityEngine.UI;
//using MyGame;
//using UnityEngine.Playables;
//using Random = UnityEngine.Random;
//using UnityEngine.SceneManagement;
//using Spine;
//using DG.Tweening;

//public class CombatTest : MonoBehaviour
//{
//    public GameObject NormalBattle;

//    public OptionManager optionManager;
//    public MonsterOptionManager monsterOptionManager;
//    public Character player;
//    public Character enemy;
//    public PlayerState playerState;
//    public InventoryManager inventoryManager;
//    public BattleUI battleUI;
//    public BuffUI buffUI;
//    public Animator Enemy_Animator;
//    public GameObject EnemyAttackImage;
//    public GameObject ImageGameObject;
//    public GameObject PopupObject;
//    public ScreenShake _screenShake;
//    // 전투 완료 콜백
//    private Action<bool> onComplete;
//    // 전투 종료시 넘길 변수
//    private bool battleOver;

//    private void Awake()
//    {
//        optionManager = OptionManager.Instance;
//        monsterOptionManager = MonsterOptionManager.Instance;
//        NormalBattle.SetActive(false);
//    }
//    private void Start()
//    {
//        playerState = PlayerState.Instance;
//    }
//    /// <summary>
//    /// GameFlowManager가 전투를 시작할 때 호출합니다.
//    /// </summary>
//    public void RunBattle(Action<bool> onComplete)
//    {
//        this.onComplete = onComplete;
//        if(buffUI == null)
//        buffUI = FindObjectOfType<BuffUI>();
//        // 옵션 매니저 참조 확보
//        if (optionManager == null)
//            optionManager = FindObjectOfType<OptionManager>();
//        Enemy_Animator = enemy.GetComponent<Animator>();
//        // 전투 상태 초기화
//        battleOver = false;
//        Debug.Log("전투로 넘어 갔습니다!");
//        player.Health = player.MaxHealth;
//        enemy.Health = enemy.MaxHealth;
//        if(monsterOptionManager == null)
//        monsterOptionManager = FindObjectOfType<MonsterOptionManager>();
//        // 실제 전투 코루틴 실행
//        StartCoroutine(ProcessBattle());
//    }

//    /// <summary>
//    /// GameFlowManager가 전투를 중단할 때 호출합니다.
//    /// </summary>
//    public void StopBattle()
//    {
//        // 진행 중인 모든 코루틴 정지
//        StopAllCoroutines();
//        battleOver = true;
//    }

//    private IEnumerator ProcessBattle()
//    {
//        float playerChance = (float)player.speed / (player.speed + enemy.speed);
//        float rand = Random.value;
//        bool playerGoesFirst = rand <= playerChance;

//        if (playerGoesFirst)
//        {
//            yield return StartCoroutine(AttackOnce(player, enemy, isPlayer: true, isEnemy: false));
//        }
//        else
//        {
//            yield return StartCoroutine(AttackOnce(enemy, player, isPlayer: false, isEnemy: true));
//        }

//        // ✅ 선공권 공격 이후 즉사 체크
//        if (player.Health <= 0 || enemy.Health <= 0)
//        {
//            Debug.Log("[선공권] 공격 한방에 전투 종료");

//            // 전투 정리는 CheckDeath에서 이미 처리됨
//            battleOver = true;
//            yield return new WaitUntil(() => battleOver);

//            if (player.Health >= 1)
//            {
//                HandleBattleWin(enemy.GetEXP, enemy);
//            }
//            else
//            {
//                HandleBattleLose();
//            }

//            player.GetComponent<EquipmentSystem>().Init();
//            yield break;
//        }

//        // 일반 턴 전투 루프
//        var playerLoop = StartCoroutine(AttackLoop(player, enemy, true, false));
//        var enemyLoop = StartCoroutine(AttackLoop(enemy, player, false, true));

//        yield return new WaitUntil(() => battleOver);

//        if (player.Health >= 1)
//        {
//            HandleBattleWin(enemy.GetEXP, enemy);
//        }
//        else
//        {
//            HandleBattleLose();
//        }

//        player.GetComponent<EquipmentSystem>().Init();
//    }

//    private void HandleBattleWin(int exp, Character enemy)
//    {
//        PopupObject.SetActive(true);
//        battleOver = true;
//        //enemy.RemoveTemporaryBuffs();
//        //player.RemoveTemporaryBuffs();
//        //buffUI.ClearAll();
//        NormalBattle.SetActive(false);

//        ConfirmPopup.Show($"전투에서 승리했습니다\n경험치 : {exp}흭득", () =>
//        {
//            playerState.Experience += exp;
//            playerState.statsUI.UpdateUI();
//            inventoryManager.UpdateGoldText();
//            player.GetComponent<EquipmentSystem>().Init();
//            onComplete?.Invoke(true);
//        }, false);
//    }

//    private void HandleBattleLose()
//    {
//        battleOver = false;
//        playerState.CurrentHealth--;
//        playerState.CurrentMental--;
//        //enemy.RemoveTemporaryBuffs();
//        //player.RemoveTemporaryBuffs();
//        //buffUI.ClearAll();
//        PopupObject.SetActive(true);
//        NormalBattle.SetActive(false);

//        if (playerState.CurrentHealth <= 0 || playerState.CurrentMental <= 0)
//        {
//            SceneManager.LoadScene("GameOverScene");
//            return;
//        }

//        //NormalBattle.SetActive(false);

//        ConfirmPopup.Show("전투에서 패배했습니다", () =>
//        {
//            player.GetComponent<EquipmentSystem>().Init();
//            onComplete?.Invoke(false);
//        }, false);
//    }

//    private IEnumerator AttackOnce(Character attacker, Character target, bool isPlayer, bool isEnemy)
//    {
//        yield return new WaitForSeconds(0.1f); // 약간의 텀
//        //attacker.Attack(target);
//        PlayAttackEffect(attacker);
//        // 플레이어 온히트 옵션 적용
//        ApplyOnHitOptions(attacker, target, isPlayer, isEnemy);

//        battleUI.UpdateUI();

//        // 죽음 판정
//        CheckDeath(attacker, target);
//    }

//    private IEnumerator AttackLoop(Character attacker, Character target, bool isPlayer ,bool isEnemy)
//    {
//        while (!battleOver)
//        {
//            yield return new WaitForSeconds(1f / attacker.speed);
//            if (battleOver) yield break;
//            //attacker.Attack(target);
//            PlayAttackEffect(attacker);
//            // 플레이어 온히트 옵션 적용
//            ApplyOnHitOptions(attacker, target, isPlayer, isEnemy);

//            battleUI.UpdateUI();

//            // 죽음 판정
//            CheckDeath(attacker, target);
//        }
//    }
//    private void PlayAttackEffect(Character attacker)
//    {
//        if (attacker == enemy)
//        {
//            var (damage, iscrit) = attacker.Attack(player);
//            if (iscrit)
//            {
//                var gameObject = Instantiate(EnemyAttackImage, ImageGameObject.transform.position, Quaternion.identity, ImageGameObject.transform.parent);
//                gameObject.transform.localScale = new Vector3(100, 100, 0);
//                Destroy(gameObject, 1f);
//                if (iscrit)
//                {
//                    if (_screenShake == null)
//                        _screenShake = FindObjectOfType<ScreenShake>(); // 또는 DI로 주입
//                    if (_screenShake != null)
//                    {
//                        // DOVirtual.DelayedCall은 특정 타겟에 묶이지 않아 fx가 파괴되어도 안전.
//                        DOVirtual.DelayedCall(0.4f, () =>
//                        {
//                            // 일반 히트

//                            _screenShake.Shake();
//                            // 크리티컬이면 강하게 (원하면 주석 해제)

//                        })
//                        .SetUpdate(true); // 타임스케일 무시(연출용)
//                        Debug.Log(attacker.speed);
//                    }
//                }
//            }
//            if (gameObject == null)
//            {
//                return;
//            }
//        }

//        if (attacker == player)
//        {
//            var (damage, isCrit) = attacker.Attack(enemy);
//            if (isCrit)
//            {
//                Debug.Log("크리티컬 공격입니다");
//                Enemy_Animator.SetTrigger("isHit");
//            }

//        }
//    }

//    private void ApplyOnHitOptions(Character attacker, Character target, bool isPlayer, bool isEnemy)
//    {
//        if (isPlayer && attacker.OnHitOptions != null)
//        {
//            foreach (var opt in attacker.OnHitOptions)
//            {
//                var ctx = new OptionContext
//                {
//                    User = attacker,
//                    Target = target,
//                    option_ID = opt.OptionID,
//                    Value = opt.Value
//                };
//                OptionManager.ApplyOnHitOnly(opt.OptionID, ctx);
//            }
//        }

//        if (isEnemy && attacker.OnEnemyHitOptions != null)
//        {
//            foreach (var opt in attacker.OnEnemyHitOptions)
//            {
//                var ctx = new OptionContext
//                {
//                    User = attacker,
//                    Target = target,
//                    option_ID = opt.OptionID,
//                    Value = opt.Value
//                };

//                if (!string.IsNullOrEmpty(opt.OptionID))
//                {
//                    monsterOptionManager.ApplyOption(opt.OptionID, ctx);
//                }
//            }
//            Debug.Log("<color=black>몬스터 온힛 효과 테스트 적용</color>");
//        }
//    }
//    private bool CheckDeath(Character attacker, Character target)
//    {
//        if (target.Health <= 0 || attacker.Health <= 0)
//        {
//            Debug.Log($"[CheckDeath] 사망 감지: {target.charaterName} HP={target.Health}, {attacker.charaterName} HP={attacker.Health}");

//            // 버프만 정리하고 캔버스는 유지
//            CleanupBuffsOnly();

//            battleOver = true;
//            return true;
//        }
//        return false;
//    }

//    private void CleanupBuffsOnly()
//    {
//        try
//        {
//            // 버프만 정리
//            enemy?.RemoveTemporaryBuffs();
//            player?.RemoveTemporaryBuffs();
//            buffUI?.ClearAll();

//            Debug.Log("[Combat] 버프 정리 완료 (캔버스 유지)");
//        }
//        catch (System.Exception e)
//        {
//            Debug.LogWarning($"[Combat] 버프 정리 중 오류: {e.Message}");
//        }
//    }
//}

using System;
using System.Collections;
using UnityEngine;
using MyGame;
using Random = UnityEngine.Random;

/// <summary>
/// 전투의 규칙을 관리하고 흐름을 제어하는 '심판' 역할의 클래스입니다.
/// </summary>
public class CombatTest : MonoBehaviour
{
    [Header("전투 대상")]
    public Character player;
    public Character enemy;

    [Header("연동 시스템")]
    public OptionManager optionManager;
    public MonsterOptionManager monsterOptionManager;
    public BattleUI battleUI; // 결과 팝업 등을 띄우기 위함
    public BuffUI buffUI;
    public PlayerState playerState;

    [Header("설정")]
    public GameObject battleCanvas; // 전투 화면 부모 오브젝트
    


    private Action<bool> onComplete; // 전투 종료 후 실행될 콜백 (승리 여부 전달)
    private bool isBattleOver;

    private void Awake()
    {
        // 싱글톤 매니저 연결
        optionManager = OptionManager.Instance;
        monsterOptionManager = MonsterOptionManager.Instance;
        playerState = PlayerState.Instance;

        if (battleCanvas != null) battleCanvas.SetActive(false);
    }

    /// <summary>
    /// 외부(GameFlowManager 등)에서 전투를 시작할 때 호출합니다.
    /// </summary>
    //public void RunBattle(Action<bool> onComplete)
    //{

    //    int atk, hp, def;
    //    PlayerState.Instance.CalculateFinalStats(out atk, out hp, out def);
    //    player.SetupStats(hp, atk, def, 1.0f, PlayerState.Instance.CRT);
    //    if (PlayerState.Instance.equippedWeapon != null)
    //    {
    //        // Character.cs에 옵션 ID를 저장할 변수를 만들어두면 좋습니다.
    //        //player.weaponOptionID = PlayerState.Instance.equippedWeapon.Option_1_ID;
    //        //player.weaponOptionValue = PlayerState.Instance.equippedWeapon.Option_Value1;
    //    }
    //    this.onComplete = onComplete;
    //    this.isBattleOver = false;

    //    // 1. 플레이어 능력치 동기화 (PlayerState -> Character)
    //    if (playerState != null && player != null)
    //    {
    //        // 예시: STR을 공격력으로, DIV를 체력으로 환산하는 로직 (본인 기획에 맞게 수정 가능)
    //        int finalHP = 100 + (playerState.DIV * 10);
    //        int finalATK = playerState.STR * 2;
    //        int finalDEF = playerState.AGI;

    //        player.SetupStats(finalHP, finalATK, finalDEF, 1.0f, playerState.CRT);
    //    }

    //    if (battleCanvas != null) battleCanvas.SetActive(true);
    //    StartCoroutine(CombatCoroutine());
    //}
    public void RunBattle(Action<bool> onComplete)
    {
        isBattleOver = false;
        battleCanvas.SetActive(true);

        // PlayerState에서 장비 포함 능력치 가져와서 설정
        int atk, hp, def;
        PlayerState.Instance.CalculateFinalStats(out atk, out hp, out def);
        player.SetupStats(hp, atk, def, PlayerState.Instance.CRT);

        StartCoroutine(CombatCoroutine());
    }

    private IEnumerator CombatCoroutine()
    {
        while (!isBattleOver)
        {
            // 플레이어 공격
            enemy.TakeDamage(player.damage);
            if (CheckDeath(enemy)) break;
            yield return new WaitForSeconds(1f);
            battleUI.UpdateUI();
            // 적 공격
            player.TakeDamage(enemy.damage);
            if (CheckDeath(player)) break;
            battleUI.UpdateUI();
            yield return new WaitForSeconds(1f);
        }
    }

    private bool CheckDeath(Character target)
    {
        if (target.Health <= 0)
        {
            isBattleOver = true;
            battleCanvas.SetActive(false);
            EndBattle();
            return true;
        }
        return false;
    }

    private IEnumerator ExecuteTurn(Character attacker, Character defender)
    {
        Debug.Log($"{attacker.charaterName}의 공격!");

        // 데미지 계산 (공격력 - 방어력)
        int damageDealt = Mathf.Max(1, attacker.damage - defender.armor);

        // 크리티컬 체크 (기존 Character에 정의된 CitChance 활용)
        bool isCritical = Random.Range(0, 100) < attacker.CitChance;
        if (isCritical)
        {
            damageDealt = Mathf.RoundToInt(damageDealt * 1.5f);
            Debug.Log("<color=red>크리티컬 히트!</color>");
        }

        // 실제 데미지 적용 (이때 defender의 CharacterUI가 자동으로 HP바를 줄임)
        defender.TakeDamage(damageDealt);

        // 온힛(On-Hit) 효과 적용 (기존에 작성하셨던 옵션 시스템 연동)
        ApplyOnHitEffects(attacker, defender);

        // 공격 애니메이션이나 연출을 위한 대기
        yield return new WaitForSeconds(0.5f);
    }

    private void ApplyOnHitEffects(Character attacker, Character target)
    {
        // 기존에 작성하셨던 MonsterOptionManager 로직 유지
        // attacker의 버프/아이템 옵션에 따라 추가 효과 발생
        if (monsterOptionManager != null)
        {
            // 예시: 공격자가 가진 OnEnemyHitOptions를 순회하며 효과 적용
            // (이 부분은 기존에 사용하시던 리스트 구조에 맞춰 호출하시면 됩니다)
            Debug.Log($"{attacker.charaterName}의 추가 효과 체크...");
        }
    }

    private bool CheckBattleEnd()
    {
        if (player.Health <= 0 || enemy.Health <= 0)
        {
            isBattleOver = true;
            return true;
        }
        return false;
    }

    private void EndBattle()
    {
        bool isPlayerWin = player.Health > 0;
        Debug.Log(isPlayerWin ? "플레이어 승리!" : "플레이어 패배...");

        // 사후 정리
        if (buffUI != null) buffUI.ClearAll();

        // 전투 결과 팝업 표시 (BattleUI에 정의된 함수 호출)
        if (battleUI != null)
        {
            battleUI.ShowResultPopup(isPlayerWin);
        }

        // 외부 시스템에 전투 종료 알림
        onComplete?.Invoke(isPlayerWin);
        Debug.Log("전투 종료 알림!");
    }
}