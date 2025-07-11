using UnityEngine;
using System.Linq;
using MyGame;
using System;

public class MonsterSpawner : MonoBehaviour
{
    [Header("참조할 매니저")]
    public JsonManager jsonManager;
    public MonsterOptionManager monsterOptionManager;
    public CombatTest combatTest;
    public GameObject enemy;
    public GameObject player;
    public BattleUI battleUI;
    public BuffUI buffUI;

    [Header("몬스터 프리팹")]
    public GameObject monsterPrefab;
    // 생성된 몬스터 인스턴스를 저장할 필드
    private GameObject _currentMonster;

    // 외부에서 접근할 수 있도록 프로퍼티
    public GameObject CurrentMonster => _currentMonster;


    private void Awake()
    {
        // 자동 참조
        if (jsonManager == null) jsonManager = FindObjectOfType<JsonManager>();
        if (monsterOptionManager == null) monsterOptionManager = FindObjectOfType<MonsterOptionManager>();
        if (combatTest == null) combatTest = FindObjectOfType<CombatTest>();
        if (player == null) player = GameObject.FindWithTag("Player");
        //SpawnMonsterByID("monster_001"); //임시로 적이름을 넣고 생성을 시키고
    }

    /// <summary>
    /// 외부에서 Monster ID를 전달받아 몬스터를 스폰하고 전투 상대로 설정합니다.
    /// </summary>
    public void SpawnMonsterByID(string monsterID)
    {
        // (1) 기존 몬스터가 있으면 제거하거나 재활용
        if (_currentMonster != null)
        {
            Destroy(_currentMonster);
        }

        // (2) JSON에서 데이터 찾기
        var data = jsonManager.GetMonMasters("Mon_Master")
                              .FirstOrDefault(m => m.Mon_ID == monsterID);
        if (data == null)
        {
            Debug.LogError($"[{nameof(MonsterSpawner)}] MonsterData에서 '{monsterID}'를 찾을 수 없습니다.");
            return;
        }

        // (3) 인스턴스 생성 후 필드에 저장
        _currentMonster = Instantiate(monsterPrefab, transform.position, Quaternion.identity);
        _currentMonster.name = data.Mon_Name;
        if (enemy == null) enemy = GameObject.FindWithTag("Enemy");

        Debug.Log($"몬스터의 Effect1_Stat의 값 : {data.Effect1_Stat}");
        // (4) Character 세팅
        var ch = _currentMonster.GetComponent<Character>();
        ch.charaterName = data.Mon_Name;
        ch.MaxHealth = data.Mon_HP;
        ch.Health = ch.MaxHealth;
        ch.damage = data.Mon_ATK;
        ch.armor = data.Mon_Def;
        ch.speed = data.Mon_Speed;
        ch.MonPas_Effect1 = data.MonPas_Effect1;
        ch.MonPas_Effect2 = data.MonPas_Effect2;
        ch.MonPas_Value1 = data.Effect1_Stat;
        ch.MonPas_Value2 = data.Effect2_Stat;
        battleUI.Enemy = ch;
        ch.battleUI = battleUI;
        ch.buffUI = buffUI;
        ch.GetEXP = data.Get_Soul;

        if (data.MonPas_Effect1 != null)
        {
            ch.OnEnemyHitOptions.Add(new Character.MonsterOption
            {
                OptionID = data.MonPas_Effect1,
                Value = data.Effect1_Stat
            });
        }

        if (data.MonPas_Effect2 != null)
        {
            ch.OnEnemyHitOptions.Add(new Character.MonsterOption
            {
                OptionID = data.MonPas_Effect2,
                Value = data.Effect2_Stat
            });
        }
            
        // (5) CombatTest에 할당
        combatTest.enemy = ch;

        // (6) 패시브 옵션 적용
        ApplyPassive(data.MonPas_Effect1, data.Effect1_Stat, data.Mon_ID, ch);
        ApplyPassive(data.MonPas_Effect2, data.Effect2_Stat, data.Mon_ID, ch);

        Debug.Log($"[Spawn] {_currentMonster.name} 세팅 완료");
        battleUI.SetingUI();//UI 갱신시켜줌
    }

    //여기서 패시브 등록을 해줌
    private void ApplyPassive(string optionID, int value, string sourceID, Character target)
    {
        if (string.IsNullOrEmpty(optionID) || optionID == "--" || optionID == null)
            return;

        var ctx = new OptionContext
        {
            User = enemy.GetComponent<Character>(),
            Target = player.GetComponent<Character>(),
            option_ID = optionID,
            Value = value,
            // 필요한 추가 컨텍스트 필드 설정
        };
        Debug.Log($"ApplyPassive에서의 {value}");
        Debug.Log($"ApplyPassive = ctx.user의 값 : {ctx.User}\nApplyPassive = ctx.Target = {ctx.Target}");
        monsterOptionManager.ApplyMonsterOption(optionID, ctx);
    }
}
