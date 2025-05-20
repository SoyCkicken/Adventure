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
    public GameObject player;

    [Header("몬스터 프리팹")]
    public GameObject monsterPrefab;

    private void Awake()
    {
        // 자동 참조
        if (jsonManager == null) jsonManager = FindObjectOfType<JsonManager>();
        if (monsterOptionManager == null) monsterOptionManager = FindObjectOfType<MonsterOptionManager>();
        if (combatTest == null) combatTest = FindObjectOfType<CombatTest>();
        if (player == null) player = GameObject.FindWithTag("Player");
    }

    /// <summary>
    /// 외부에서 Monster ID를 전달받아 몬스터를 스폰하고 전투 상대로 설정합니다.
    /// </summary>
    public void SpawnMonsterByID(string monsterID)
    {
        // 1) JSON에서 해당 ID 찾아오기
        var data = jsonManager.GetMonMasters("Mon_Master")
                              .FirstOrDefault(m => m.Mon_ID == monsterID);
        if (data == null)
        {
            Debug.LogError($"[{nameof(MonsterSpawner)}] MonsterData에서 '{monsterID}'를 찾을 수 없습니다.");
            return;
        }

        // 2) 인스턴스 생성
        var go = Instantiate(monsterPrefab, transform.position, Quaternion.identity);
        go.name = data.Mon_Name;

        // 3) Character 컴포넌트 세팅
        var ch = go.GetComponent<Character>();
        ch.charaterName = data.Mon_Name;
        ch.MaxHealth = data.Mon_HP;
        ch.Health = ch.MaxHealth;
        ch.damage = data.Mon_ATK;
        ch.armor = data.Mon_Def;
        ch.speed = data.Mon_Speed;

        Debug.Log($"[Spawn] {data.Mon_Name} 생성 → HP:{ch.Health}, ATK:{ch.damage}, DEF:{ch.armor}, SPD:{ch.speed}");

        // 4) 전투 테스트 스크립트에 적 설정
        combatTest.enemy = ch;

        // 5) 패시브 옵션 적용
        ApplyPassive(data.MonPas_Effect1, data.Effect1_Stat, data.Mon_ID, ch);
        // 필요 시 다른 패시브도 추가

        // 6) (옵션) 전투 시작
        // GameFlowManager에서 이 메서드를 호출한 뒤 battleManager.StartBattle()를 실행하세요.
    }

    private void ApplyPassive(string optionID, int value, string sourceID, Character target)
    {
        if (string.IsNullOrEmpty(optionID) || optionID == "--")
            return;

        var ctx = new OptionContext
        {
            User = target,
            Target = player.GetComponent<Character>(),
            option_ID = optionID,
            Value = value,
            // 필요한 추가 컨텍스트 필드 설정
        };
        monsterOptionManager.ApplyMonsterOption(optionID, ctx);
    }
}
