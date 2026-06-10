using UnityEngine;
using System.Linq;
using MyGame;
using System;
using System.Collections.Generic;
using UnityEngine.Playables;

public class MonsterSpawner : MonoBehaviour
{
    [Header("������ �Ŵ���")]
    public JsonManager jsonManager;
    public MonsterOptionManager monsterOptionManager;
    public CombatTest combatTest;
    public GameObject enemy;
    public GameObject player;
    public BattleUI battleUI;
    public BuffUI buffUI;

    [Header("���� ������")]
    public GameObject canves;
    public GameObject monsterPrefab;
    // ������ ���� �ν��Ͻ��� ������ �ʵ�
    
    public GameObject canvusImage;
    private GameObject _currentMonster; //<-�̰� ���� ���������� ����ҵ�? 
    // �ܺο��� ������ �� �ֵ��� ������Ƽ
    public GameObject CurrentMonster => _currentMonster;


    private void Awake()
    {
        // �ڵ� ����
        jsonManager = JsonManager.Instance; // ����
        if (monsterOptionManager == null) monsterOptionManager = FindObjectOfType<MonsterOptionManager>();
        if (combatTest == null) combatTest = FindObjectOfType<CombatTest>();
        if (player == null) player = GameObject.FindWithTag("Player");
        canves.SetActive(false);
        //SpawnMonsterByID("monster_001"); //�ӽ÷� ���̸��� �ְ� ������ ��Ű��
    }

    /// <summary>
    /// �ܺο��� Monster ID�� ���޹޾� ���͸� �����ϰ� ���� ���� �����մϴ�.
    /// </summary>
    public void SpawnMonsterByID(string monsterID)
    {
        canves.SetActive(true);
        // (1) ���� ���Ͱ� ������ �����ϰų� ��Ȱ��
        if (_currentMonster != null)
        {
            Destroy(_currentMonster);
        }

        // (2) JSON���� ������ ã��
        var data = jsonManager.GetMonMasters("Mon_Master")
                              .FirstOrDefault(m => m.Mon_ID == monsterID);
        if (data == null)
        {
            Debug.LogError($"[{nameof(MonsterSpawner)}] MonsterData���� '{monsterID}'�� ã�� �� �����ϴ�.");
            return;
        }

        // (3) �ν��Ͻ� ���� �� �ʵ忡 ����
        Vector3 vector3 = new Vector3(10, -125, 0);
        
        _currentMonster = Instantiate(monsterPrefab, canvusImage.transform.position, Quaternion.identity,canvusImage.transform);
        _currentMonster.transform.localPosition = vector3;
        _currentMonster.transform.localScale = new Vector3(80, 80, 0);
        _currentMonster.name = data.Mon_Name;
        if (enemy == null) enemy = GameObject.FindWithTag("Enemy");

        Debug.Log($"������ Effect1_Stat�� �� : {data.Effect1_Stat}");
        // (4) Character ����
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
            
        // (5) CombatTest�� �Ҵ�
        combatTest.enemy = ch;

        // (6) �нú� �ɼ� ����
        ApplyPassive(data.MonPas_Effect1, data.Effect1_Stat, data.Mon_ID, ch);
        ApplyPassive(data.MonPas_Effect2, data.Effect2_Stat, data.Mon_ID, ch);

        Debug.Log($"[Spawn] {_currentMonster.name} ���� �Ϸ�");
        battleUI.SetingUI();//UI ���Ž�����
    }

    //���⼭ �нú� ����� ����
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
            // �ʿ��� �߰� ���ؽ�Ʈ �ʵ� ����
        };
        Debug.Log($"ApplyPassive������ {value}");
        Debug.Log($"ApplyPassive = ctx.user�� �� : {ctx.User}\nApplyPassive = ctx.Target = {ctx.Target}");
        monsterOptionManager.ApplyMonsterOption(optionID, ctx);
    }

    //���� ���� ��
}
 