//using UnityEngine;
//using System.Linq;
//using MyGame;
//using System;  // Character, OptionContext, OptionManager 네임스페이스

//public class MonsterSpawner : MonoBehaviour
//{
//    [Header("참조할 매니저")]
//    public JsonManager jsonManager;
//    public MonsterOptionManager monsterOptionManager;
//    public CombatTest combatTest;
//    public GameObject Player;

//    [Header("몬스터 프리팹")]
//    public GameObject monsterPrefab;   // Character 컴포넌트가 붙어있는 프리팹

//    [Header("스폰할 몬스터 ID")]
//    public string spawnMonID = "monster_001";

//    void Awake()
//    {
//        // 자동 참조
//        if (jsonManager == null) jsonManager = FindObjectOfType<JsonManager>();
//        if (monsterOptionManager == null) monsterOptionManager = FindObjectOfType<MonsterOptionManager>();
//        // JSON에서 해당 ID 찾아오기
//        var data = jsonManager.GetMonMasters("Mon_Master")
//                              .FirstOrDefault(m => m.Mon_ID == spawnMonID);
//        if (data == null)
//        {
//            Debug.LogError($"[{nameof(MonsterSpawner)}] MonsterData에서 '{spawnMonID}'를 찾을 수 없습니다.");
//            return;
//        }

//        SpawnMonster(data);
//    }
//    void SpawnMonster(Mon_Master m)
//    {
//        // 1) 인스턴스 생성
//        var go = Instantiate(monsterPrefab, transform.position, Quaternion.identity);
//        go.name = m.Mon_Name;

//        // 2) Character 세팅
//        var ch = go.GetComponent<Character>();
//        //순서 중요
//        combatTest.enemy = ch;
//        ch.charaterName = m.Mon_Name;
//        ch.MaxHealth = m.Mon_HP;
//        ch.Health = ch.MaxHealth;
//        ch.damage = m.Mon_ATK;
//        ch.armor = m.Mon_Def;
//        ch.speed = m.Mon_Speed;

//        Debug.Log($"[Spawn] {m.Mon_Name} 생성 → HP:{ch.Health}, ATK:{ch.damage}, DEF:{ch.armor}, SPD:{ch.speed}");

//        // 3) 패시브 옵션(예: 크리티컬 버프, 출혈 면역 등) 적용
//        ApplyPassive(m.MonPas_Effect1, m.Effect1_Stat, m.Mon_ID, ch);
//        //ApplyPassive(m.MonPas_Effect2, m.Effect2_Stat, m.Mon_ID, ch);

//        // 4) 필요하면 exp/soul 세팅 (별도 컴포넌트나 필드)
//        //var loot = go.AddComponent<MonsterLoot>();
//        //loot.exp = m.Get_EXP;
//        //loot.soul = m.Get_Soul;


//    }

//    void ApplyPassive(string optionID, int value, string sourceID, Character target)
//    {
//        if (string.IsNullOrEmpty(optionID) || optionID == "--")
//            return;

//        var ctx = new OptionContext
//        {
//            User = target,
//            Target = Player.GetComponent<Character>(),    // 패시브라 자기 자신에 적용
//            option_ID = optionID,
//            Value = value,
//            // DamageDealt, TurnNumber 등은 패시브엔 필요 없으므로 0
//            //SourceID = sourceID
//        };
//        //monsterOptionManager.ApplyMonsterOption(optionID, ctx);
//    }
//}


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
        ch.MonPas_Value1 = data.Effect1_Stat;
        ch.MonPas_Value2 = data.Effect2_Stat;

        // (5) CombatTest에 할당
        combatTest.enemy = ch;

        // (6) 패시브 옵션 적용
        ApplyPassive(data.MonPas_Effect1, data.Effect1_Stat, data.Mon_ID, ch);
        ApplyPassive(data.MonPas_Effect2, data.Effect2_Stat, data.Mon_ID, ch);

        Debug.Log($"[Spawn] {_currentMonster.name} 세팅 완료");
    }

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
