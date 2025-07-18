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

    void Start()
    {
       
    }
    //적 생성 되었을때 세팅용
    public void SetingUI()
    {
        Debug.Log($"{Enemy.name}");
        enemyNameText.text = Enemy.name;
        //솔직히 세팅 되었을때 값을 가져오는거라 Max값이 아니라 Health값을 가져와도 상관은 없을꺼 같긴한데 일관성 위해서 해둠
        monsterHPSlider.maxValue = Enemy.MaxHealth;
        playerHpSlider.maxValue = player.MaxHealth;
        monsterHPSlider.value = int.MaxValue;
        playerHpSlider.value = int.MaxValue;
    }
    public void UpdateUI()
    {
        //공격시 실행 예정
        //테스트 해보다가 디버프로 데미지 주는 버프일때도 갱신을 하게 만들어야 해서 어택 루프랑 버프 효과 적용할때 호출하게 해놨음
        monsterHPSlider.value = Enemy.Health;
        playerHpSlider.value = player.Health;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
