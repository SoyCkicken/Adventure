using MyGame;
using Spine;
using Spine.Unity;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossPartCombatManager : MonoBehaviour
{
    public TMP_Text logText;
    public Slider armSlider;
    public Slider legSlider;
    public Slider headSlider;
    public Slider totalHPSlider;
    public SkeletonAnimation BossSkeleton;
    public TESTBoss testBoss;
    public TESTPlayer testPlayer;
    [Header("여기는 임시 버튼 선택입니다")]
    public TMP_Text selectedPartText;
    public Button leftButton;
    public Button rightButton;
    public Button attackButton;
    private bool isPlayerTurn = true;
    private int currentIndex = 0;
    private string selectedPartName = null;
    [Header("사운드 설정")]
    public AudioSource audioSource;
    public AudioClip hitSound;      // 플레이어 공격 성공
    public AudioClip damageSound;   // 보스 공격 성공
    public AudioClip DodgeSound;    // 누구든 간에 회피 성공시 재생


    // 여기서 적과 플레이어에 대해서 정보를 넣고 있는데 이 부분 수정해서 Boss에서 Player에서 정보 넣는 식으로 할 예정
    void Start()
    {
        testPlayer.AddBuff(new FocusBuffData
        {
            OptionID = "Option_003",
            Value = 10,
            Duration = 3,
            Elapsed = 0f
        });

        testBoss.AddBuff("오른쪽 팔", new FocusBuffData
        {
            OptionID = "Option_003",
            Value = 10,
            Duration = 3,
            Elapsed = 0f,
            DamageRatio = 0.5f,
            Target = testBoss
        });

        //leftButton.onClick.AddListener(() => { OnClickLeft(); });
        //rightButton.onClick.AddListener(() => { OnClickRight(); });
        attackButton.onClick.AddListener(() => { OnClickAttack(); });
        UpdateSliders();
        Log("플레이어의 턴입니다.");
    }

    public void OnClickAttack()
    {
        if (!isPlayerTurn)
        {
            Log("지금은 플레이어의 턴이 아닙니다.");
            return;
        }

        var parts = testBoss.GetAttackableParts();
        if (parts.Count == 0)
        {
            Log("공격 가능한 부위가 없습니다.");
            return;
        }

        //string selectedPart = parts[currentIndex];
        testPlayer.PerformAttack(testBoss, selectedPartName);
        Log($"플레이어가 {selectedPartName} 부위를 공격했습니다.");
        //PlayHitSound();
        if (testBoss.IsDead)
        {
            Log("보스를 처치했습니다!");
            return;
        }
        currentIndex = 0;
        UpdateSelectedPartUI();
        isPlayerTurn = false;
        UpdateSliders();
        Invoke(nameof(EnemyTurn), 1.5f);
        testPlayer.TickDebuffs();
    }

    public void SetSelectedPart(string partName)
    {
        selectedPartName = partName;
        selectedPartText.text = $"선택된 부위: {partName}";
    }

    void EnemyTurn()
    {
        if (testBoss.IsDead) return;

        if (testBoss.IsPartBroken("팔")) // 보스 내부에서 부위 확인하도록 구조 개선
        {
            Log("보스의 팔이 파괴되어 공격할 수 없습니다.");
            isPlayerTurn = true;
            Log("플레이어의 턴입니다.");
            return;
        }
        testBoss.PerformAttack(testPlayer);
        Log($"보스가 플레이어를 공격했습니다. ({testBoss.attackPower} 데미지)");

        if (testPlayer.IsDead)
        {
            Log("플레이어가 사망했습니다...");
            return;
        }

        isPlayerTurn = true;
        Log("플레이어의 턴입니다.");
        testBoss.OnEnemyTurnEnd();
    }

    void UpdateSelectedPartUI()
    {
        var parts = testBoss.GetAttackableParts();

        if (parts.Count == 0)
        {
            selectedPartText.text = "공격 가능한 부위 없음";
            return;
        }

        // 선택된 인덱스가 유효한지 검사 → 유효하지 않으면 0으로 초기화
        if (currentIndex >= parts.Count || currentIndex < 0)
            currentIndex = 0;

        selectedPartText.text = $"선택된 부위: {parts[currentIndex]}";
    }

    public void OnClickLeft()
    {
        var parts = testBoss.GetAttackableParts();
        if (parts.Count == 0) return;

        currentIndex = (currentIndex - 1 + parts.Count) % parts.Count;
        UpdateSelectedPartUI();
    }

    public void OnClickRight()
    {
        var parts = testBoss.GetAttackableParts();
        if (parts.Count == 0) return;

        currentIndex = (currentIndex + 1) % parts.Count;
        UpdateSelectedPartUI();
    }

    void UpdateSliders()
    {
        armSlider.value = testBoss.GetPartHPPercent("팔");
        legSlider.value = testBoss.GetPartHPPercent("다리");
        headSlider.value = testBoss.GetPartHPPercent("머리");
        totalHPSlider.value = testBoss.GetTotalHPPercent();
    }

    public void PlayHitSound()
    {
        if (hitSound != null && audioSource != null)
            audioSource.PlayOneShot(hitSound);
    }

    public void PlayDamageSound()
    {
        if (damageSound != null && audioSource != null)
            audioSource.PlayOneShot(damageSound);
    }
    public void PlayDodgeSound()
    {
        if (DodgeSound != null && audioSource != null)
            audioSource.PlayOneShot(DodgeSound);
    }
    public void Log(string message)
    {
        logText.text += message + "\n";
    }
}