using System.Linq;
using MyGame;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Managers")]
    public JsonManager jsonManager;
    public MonsterOptionManager monsterOptionManager;
    public CombatTest combatTest;
    public GameObject enemy;
    public GameObject player;
    public BattleUI battleUI;
    public BuffUI buffUI;

    [Header("Monster Spawn")]
    public GameObject canves;
    public GameObject monsterPrefab;
    public GameObject canvusImage;

    private GameObject _currentMonster;
    public GameObject CurrentMonster => _currentMonster;

    private void Awake()
    {
        jsonManager = JsonManager.Instance;
        if (monsterOptionManager == null) monsterOptionManager = FindObjectOfType<MonsterOptionManager>();
        if (combatTest == null) combatTest = FindObjectOfType<CombatTest>();
        if (player == null) player = GameObject.FindWithTag("Player");
        canves.SetActive(false);
    }

    public void SpawnMonsterByID(string monsterID)
    {
        canves.SetActive(true);
        if (_currentMonster != null)
        {
            Destroy(_currentMonster);
        }

        var data = jsonManager.GetMonMasters("Mon_Master")
                              .FirstOrDefault(m => m.Mon_ID == monsterID);
        if (data == null)
        {
            Debug.LogError($"[{nameof(MonsterSpawner)}] MonsterData '{monsterID}' was not found.");
            return;
        }

        Vector3 localPosition = new Vector3(10, -125, 0);
        _currentMonster = Instantiate(monsterPrefab, canvusImage.transform.position, Quaternion.identity, canvusImage.transform);
        _currentMonster.transform.localPosition = localPosition;
        _currentMonster.transform.localScale = new Vector3(80, 80, 0);
        _currentMonster.name = data.Mon_Name;
        if (enemy == null) enemy = GameObject.FindWithTag("Enemy");

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

        if (monsterOptionManager == null) monsterOptionManager = FindObjectOfType<MonsterOptionManager>();
        ch.OnEnemyHitOptions.Clear();
        if (monsterOptionManager != null)
        {
            ch.OnEnemyHitOptions.AddRange(monsterOptionManager.CollectOptions(data));
        }

        combatTest.enemy = ch;
        battleUI.SetingUI();
        Debug.Log($"[Spawn] {_currentMonster.name} spawned with {ch.OnEnemyHitOptions.Count} monster options.");
    }
}