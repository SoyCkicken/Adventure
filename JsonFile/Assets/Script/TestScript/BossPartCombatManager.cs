using MyGame;
using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using Spine;
using Spine.Unity;
using System;
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
    //public TESTPlayer TESTPlayer;
    [Header("플레이어 캐릭터 연동")]
    public Character playerCharacter;  // 실제 Player 오브젝트에 붙은 Character 컴포넌트
    [Header("여기는 임시 버튼 선택입니다")]
    public TMP_Text selectedPartText;
    public Button leftButton;
    public Button rightButton;
    public Button attackButton;
    private bool isPlayerTurn = true;
    private int currentIndex = 0;
    private string selectedPartName = null;
    public GameObject combatCanvas; // 전투 UI 캔버스    
    [Header("사운드 설정")]
    public AudioSource audioSource;
    public AudioClip hitSound;      // 플레이어 공격 성공
    public AudioClip damageSound;   // 보스 공격 성공
    public AudioClip DodgeSound;    // 누구든 간에 회피 성공시 재생

    private Action<bool> onCombatEndCallback;


    // 여기서 적과 플레이어에 대해서 정보를 넣고 있는데 이 부분 수정해서 Boss에서 Player에서 정보 넣는 식으로 할 예정
    void Start()
    {


        playerCharacter.AddFocusBuff(new FocusBuffData
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

        playerCharacter.CurrentHP = playerCharacter.MaxHP; // 플레이어 HP 초기화
        // 부위 선택 가능 목록 설정
        var parts = TESTBoss.GetAttackableParts();

        Debug.Log("[BossPartCombatManager] 집중 전투 매니저 초기화 완료");
    }

    private void ClearSelectionHighlights()
    {
        // 선택 UI 하이라이트 초기화하는 로직이 있다면 여기에 작성
        totalHPSlider.maxValue = TESTBoss.MaxTotalHP;
        totalHPSlider.value = int.MaxValue;
        playerHPSlider.maxValue = playerCharacter.MaxHP;
        playerHPSlider.value = int.MaxValue;
    }

    public void RunFocusBattle(Action<bool> onComplete)
    {
        onCombatEndCallback = onComplete;
        TESTBoss.Init();
        combatCanvas.SetActive(true);
        Initialize();
    }

   public void StopFocusBattle()
    {
        // 집중 전투 종료 로직
        Debug.Log("집중 전투가 종료되었습니다.");
        isPlayerTurn = false;
        selectedPartName = null;
        UpdateSelectedPartUI();
        UpdateSliders();
        playerCharacter.TickDebuffs();
    }

    private void EndCombat(bool playerVictory)
    {
        Debug.Log("[전투 종료] 결과: " + (playerVictory ? "승리" : "패배"));

        // 전투 UI 비활성화
        combatCanvas.SetActive(false);

        // 콜백 호출
        onCombatEndCallback?.Invoke(playerVictory);
        onCombatEndCallback = null;
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

        // 선택한 부위 공격
        playerCharacter.PerformAttack(TESTBoss, selectedPartName);
        Debug.Log($"플레이어가 {selectedPartName} 부위를 공격했습니다.");

        if (TESTBoss.IsDead)
        {
            Debug.Log("보스를 처치했습니다!");
            EndCombat(true);
            return;
        }

        // 🔥 공격 후 부위가 파괴되었는지 확인 → 파괴되었으면 다음 부위로 전환
        if (TESTBoss.IsPartBroken(selectedPartName))
        {
            var newParts = TESTBoss.GetAttackableParts();

            // 파괴된 부위가 빠진 새로운 리스트에서 currentIndex 보정
            if (newParts.Count > 0)
            {
                currentIndex = Mathf.Clamp(currentIndex, 0, newParts.Count - 1);
                selectedPartName = newParts[currentIndex];
            }
            else
            {
                selectedPartName = null;
                Debug.Log("모든 부위가 파괴되어 더 이상 타겟이 없습니다.");
            }
        }

        UpdateSelectedPartUI();
        isPlayerTurn = false;
        UpdateSliders();
        Invoke(nameof(EnemyTurn), 1.5f);
        playerCharacter.TickDebuffs();
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
        TESTBoss.PerformAttack(playerCharacter);
        Debug.Log($"보스가 플레이어를 공격했습니다. ({TESTBoss.attackPower} 데미지)");

        if (playerCharacter.IsDead)
        {
            Debug.Log("플레이어가 사망했습니다...");
            EndCombat(false);
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

        // 현재 타겟팅된 부위가 목록에 없으면 첫 번째 부위로 전환
        if (string.IsNullOrEmpty(selectedPartName) || !parts.Contains(selectedPartName))
        {
            currentIndex = 0;
            selectedPartName = parts[currentIndex];
        }

        selectedPartText.text = $"선택된 부위: {selectedPartName}";
    }

    public void OnClickLeft()
    {
        var parts = TESTBoss.GetAttackableParts();
        if (parts.Count == 0) return;

        int currentIdx = parts.IndexOf(selectedPartName);
        if (currentIdx == -1) currentIdx = 0;

        currentIdx = (currentIdx - 1 + parts.Count) % parts.Count;
        selectedPartName = parts[currentIdx];

        UpdateSelectedPartUI();
    }

    public void OnClickRight()
    {
        var parts = TESTBoss.GetAttackableParts();
        if (parts.Count == 0) return;

        int currentIdx = parts.IndexOf(selectedPartName);
        if (currentIdx == -1) currentIdx = 0;

        currentIdx = (currentIdx + 1) % parts.Count;
        selectedPartName = parts[currentIdx];

        UpdateSelectedPartUI();
    }


    void UpdateSliders()
    {
        totalHPSlider.value = TESTBoss.GetTotalHPPercent();
    }
    void UpdatePlayerSliders()
    {
        playerHPSlider.value = playerCharacter.CurrentHP;
        playerHPSlider.maxValue = playerCharacter.MaxHP;
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