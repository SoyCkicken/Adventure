using MyGame;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    [Header("값 가져 와야 하는 곳들")]
    public Character player;
    public Character Enemy;

    [Header("UI와 관련 된 변수")]
    public Slider monsterHPSlider;
    public Slider playerHpSlider;
    public TMP_Text enemyNameText;
    public TMP_Text combatLogText;
    [SerializeField] private int maxCombatLogLines = 6;
    private readonly Queue<string> combatLogLines = new Queue<string>();

    void Start()
    {
       
    }

    private void OnEnable()
    {
        CombatFeedback.OnFeedback += HandleCombatFeedback;
    }

    private void OnDisable()
    {
        CombatFeedback.OnFeedback -= HandleCombatFeedback;
    }
    //적 생성 되었을때 세팅용
    public void SetingUI()
    {
        Debug.Log($"{Enemy.name}");
        if (Enemy != null)
        {
            enemyNameText.text = Enemy.name;
            monsterHPSlider.maxValue = Enemy.MaxHealth;
            monsterHPSlider.value = int.MaxValue;
        }
       
        //솔직히 세팅 되었을때 값을 가져오는거라 Max값이 아니라 Health값을 가져와도 상관은 없을꺼 같긴한데 일관성 위해서 해둠
        playerHpSlider.maxValue = player.MaxHealth;
        playerHpSlider.value = int.MaxValue;
    }
    public void UpdateUI()
    {
        //공격시 실행 예정
        //테스트 해보다가 디버프로 데미지 주는 버프일때도 갱신을 하게 만들어야 해서 어택 루프랑 버프 효과 적용할때 호출하게 해놨음
        if (Enemy != null)
        {
            monsterHPSlider.value = Enemy.Health;
        }
        
        playerHpSlider.value = player.Health;
    }

    public void ClearCombatLog()
    {
        combatLogLines.Clear();
        if (combatLogText != null)
            combatLogText.text = string.Empty;
    }

    private void HandleCombatFeedback(CombatFeedbackEntry entry)
    {
        AppendCombatLog(entry.Message);
    }

    public void AppendCombatLog(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        combatLogLines.Enqueue(message);
        while (combatLogLines.Count > Mathf.Max(1, maxCombatLogLines))
            combatLogLines.Dequeue();

        if (combatLogText != null)
            combatLogText.text = string.Join("\n", combatLogLines);
    }
}
