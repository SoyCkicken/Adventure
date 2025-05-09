using UnityEngine;
using System.Linq;
using MyGame;
using System;  // Character, OptionContext, OptionManager 네임스페이스

public class MonsterSpawner : MonoBehaviour
{
    [Header("참조할 매니저")]
    public JsonManager jsonManager;
    public MonsterOptionManager monsterOptionManager;
    public CombatTest combatTest;
    public GameObject Player;

    [Header("몬스터 프리팹")]
    public GameObject monsterPrefab;   // Character 컴포넌트가 붙어있는 프리팹

    [Header("스폰할 몬스터 ID")]
    public string spawnMonID = "monster_001";

    void Start()
    {
        // 자동 참조
        if (jsonManager == null) jsonManager = FindObjectOfType<JsonManager>();
        if (monsterOptionManager == null) monsterOptionManager = FindObjectOfType<MonsterOptionManager>();
        // JSON에서 해당 ID 찾아오기
        var data = jsonManager.GetMonMasters("Mon_Master")
                              .FirstOrDefault(m => m.Mon_ID == spawnMonID);
        if (data == null)
        {
            Debug.LogError($"[{nameof(MonsterSpawner)}] MonsterData에서 '{spawnMonID}'를 찾을 수 없습니다.");
            return;
        }

        SpawnMonster(data);
    }
    void SpawnMonster(Mon_Master m)
    {
        // 1) 인스턴스 생성
        var go = Instantiate(monsterPrefab, transform.position, Quaternion.identity);
        go.name = m.Mon_Name;

        // 2) Character 세팅
        var ch = go.GetComponent<Character>();
        //순서 중요
        combatTest.enemy = ch;
        ch.charaterName = m.Mon_Name;
        ch.Health = m.Mon_HP;
        ch.damage = m.Mon_ATK;
        ch.armor = m.Mon_Def;
        ch.speed = m.Mon_Speed;

        Debug.Log($"[Spawn] {m.Mon_Name} 생성 → HP:{ch.Health}, ATK:{ch.damage}, DEF:{ch.armor}, SPD:{ch.speed}");

        // 3) 패시브 옵션(예: 크리티컬 버프, 출혈 면역 등) 적용
        ApplyPassive(m.MonPas_Effect1, m.Effect1_Stat, m.Mon_ID, ch);
        //ApplyPassive(m.MonPas_Effect2, m.Effect2_Stat, m.Mon_ID, ch);

        // 4) 필요하면 exp/soul 세팅 (별도 컴포넌트나 필드)
        //var loot = go.AddComponent<MonsterLoot>();
        //loot.exp = m.Get_EXP;
        //loot.soul = m.Get_Soul;
       

    }

    void ApplyPassive(string optionID, int value, string sourceID, Character target)
    {
        if (string.IsNullOrEmpty(optionID) || optionID == "--")
            return;

        var ctx = new OptionContext
        {
            User = target,
            Target = Player.GetComponent<Character>(),    // 패시브라 자기 자신에 적용
            option_ID = optionID,
            Value = value,
            // DamageDealt, TurnNumber 등은 패시브엔 필요 없으므로 0
            //SourceID = sourceID
        };
        monsterOptionManager.ApplyMonsterOption(optionID, ctx);
    }
}
