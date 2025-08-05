using MyGame;
using Spine;
using Spine.Unity;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossPartCombatManager : MonoBehaviour
{
    public Slider totalHPSlider;
    public Slider playerHPSlider;
    public SkeletonAnimation BossSkeleton;
    public TESTBoss TESTBoss;
    public TESTPlayer TESTPlayer;
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
        

        TESTPlayer.AddBuff(new FocusBuffData
        {
            OptionID = "Option_003",
            Value = 10,
            Duration = 3,
            Elapsed = 0f
        });

        TESTBoss.AddBuff("오른팔", new FocusBuffData
        {
            OptionID = "Option_003",
            Value = 10,
            Duration = 3,
            Elapsed = 0f,
            DamageRatio = 0.5f,
            Target = TESTBoss
        });

        
        UpdateSliders();
        Debug.Log("플레이어의 턴입니다.");
        
    }
    public void Initialize()
    {
        if (TESTBoss == null)
        {
            Debug.LogError("[BossPartCombatManager] TESTBoss가 연결되지 않았습니다.");
            return;
        }

        // 선택 부위 초기화
        selectedPartName = null;
        if (audioSource == null)
        {
            audioSource = BossSkeleton.GetComponentInChildren<AudioSource>();
        }
        leftButton.onClick.AddListener(() => { OnClickLeft(); });
        rightButton.onClick.AddListener(() => { OnClickRight(); });
        attackButton.onClick.AddListener(() => { OnClickAttack(); });
        // 선택 UI 초기화 (필요 시 버튼 / 인디케이터 초기화 등)
        ClearSelectionHighlights();

        // 부위 선택 가능 목록 설정
        var parts = TESTBoss.GetAttackableParts();

        Debug.Log("[BossPartCombatManager] 집중 전투 매니저 초기화 완료");
    }

    private void ClearSelectionHighlights()
    {
        // 선택 UI 하이라이트 초기화하는 로직이 있다면 여기에 작성
        totalHPSlider.maxValue = TESTBoss.MaxTotalHP;
        totalHPSlider.value = int.MaxValue;
        playerHPSlider.maxValue = TESTPlayer.MaxHP;
        playerHPSlider.value = int.MaxValue;
    }
    public void OnClickAttack()
    {
        if (!isPlayerTurn)
        {
            Debug.Log("지금은 플레이어의 턴이 아닙니다.");
            return;
        }

        var parts = TESTBoss.GetAttackableParts();
        if (parts.Count == 0)
        {
            Debug.Log("공격 가능한 부위가 없습니다.");
            return;
        }

        //string selectedPart = parts[currentIndex];
        TESTPlayer.PerformAttack(TESTBoss, selectedPartName);
        Debug.Log($"플레이어가 {selectedPartName} 부위를 공격했습니다.");
        //PlayHitSound();
        if (TESTBoss.IsDead)
        {
            Debug.Log("보스를 처치했습니다!");
            return;
        }
        currentIndex = 0;
        UpdateSelectedPartUI();
        isPlayerTurn = false;
        UpdateSliders();
        Invoke(nameof(EnemyTurn), 1.5f);
        TESTPlayer.TickDebuffs();
    }

    public void SetSelectedPart(string partName)
    {
        Debug.Log("부위 선택되었습니다");
        selectedPartName = partName;
        selectedPartText.text = $"선택된 부위: {partName}";
    }

    void EnemyTurn()
    {
        if (TESTBoss.IsDead) return;

        if (TESTBoss.IsPartBroken("팔")) // 보스 내부에서 부위 확인하도록 구조 개선
        {
            Debug.Log("보스의 팔이 파괴되어 공격할 수 없습니다.");
            isPlayerTurn = true;
            Debug.Log("플레이어의 턴입니다.");
            return;
        }
        TESTBoss.PerformAttack(TESTPlayer);
        Debug.Log($"보스가 플레이어를 공격했습니다. ({TESTBoss.attackPower} 데미지)");

        if (TESTPlayer.IsDead)
        {
            Debug.Log("플레이어가 사망했습니다...");
            return;
        }

        isPlayerTurn = true;
        UpdatePlayerSliders();
        Debug.Log("플레이어의 턴입니다.");
        TESTBoss.OnEnemyTurnEnd();
    }

    void UpdateSelectedPartUI()
    {
        var parts = TESTBoss.GetAttackableParts();

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
        var parts = TESTBoss.GetAttackableParts();
        if (parts.Count == 0) return;

        currentIndex = (currentIndex - 1 + parts.Count) % parts.Count;
        UpdateSelectedPartUI();
    }

    public void OnClickRight()
    {
        var parts = TESTBoss.GetAttackableParts();
        if (parts.Count == 0) return;

        currentIndex = (currentIndex + 1) % parts.Count;
        UpdateSelectedPartUI();
    }

    void UpdateSliders()
    {
        totalHPSlider.value = TESTBoss.GetTotalHPPercent();
    }
    void UpdatePlayerSliders()
    {
        playerHPSlider.value = TESTPlayer.CurrentHP;
        playerHPSlider.maxValue = TESTPlayer.MaxHP;
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

}