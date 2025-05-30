using UnityEngine;
using UnityEditor;

public class CombatSimulatorWindow : EditorWindow
{
    // 플레이어 입력값
    int playerAttack = 30;
    int playerHP = 100;

    // 적 입력값
    int enemyAttack = 25;
    int enemyHP = 80;

    // 시뮬레이션 설정
    int simulationCount = 1000;

    // 플레이어 버프
    bool usePlayerDR = false, usePlayerAS = false, usePlayerReflect = false;
    int playerDRStacks = 0, playerASStacks = 0, playerReflectStacks = 0;
    bool usePlayerCritBuff = false;
    bool usePlayerDodgeBuff = false;
    int playerDodgeStacks = 0;

    // 적 버프
    bool useEnemyDR = false, useEnemyAS = false, useEnemyReflect = false;
    int enemyDRStacks = 0, enemyASStacks = 0, enemyReflectStacks = 0;
    bool useEnemyCritBuff = false;
    bool useEnemyDodgeBuff = false;
    int enemyDodgeStacks = 0;

    // 결과 출력
    float winRate = 0;

    [MenuItem("Tools/Combat Simulator")]
    public static void ShowWindow()
    {
        GetWindow<CombatSimulatorWindow>("Combat Simulator");
    }

    void OnGUI()
    {
        GUILayout.Label("Player Stats", EditorStyles.boldLabel);
        playerAttack = EditorGUILayout.IntField("Attack", playerAttack);
        playerHP = EditorGUILayout.IntField("HP", playerHP);

        GUILayout.Space(5);
        GUILayout.Label("Player Buffs", EditorStyles.boldLabel);
        usePlayerDR = EditorGUILayout.Toggle("Damage Reduction (Player)", usePlayerDR);
        if (usePlayerDR) playerDRStacks = EditorGUILayout.IntSlider("DR Stacks", playerDRStacks, 0, 10);
        usePlayerAS = EditorGUILayout.Toggle("Attack Speed Up (Player)", usePlayerAS);
        if (usePlayerAS) playerASStacks = EditorGUILayout.IntSlider("AS Stacks", playerASStacks, 0, 10);
        usePlayerReflect = EditorGUILayout.Toggle("Reflect Damage (Player)", usePlayerReflect);
        if (usePlayerReflect) playerReflectStacks = EditorGUILayout.IntSlider("Reflect Stacks", playerReflectStacks, 0, 10);
        usePlayerCritBuff = EditorGUILayout.Toggle("Boost First Crit Chance (Player)", usePlayerCritBuff);
        usePlayerDodgeBuff = EditorGUILayout.Toggle("Dodge Rate Buff (Player)", usePlayerDodgeBuff);
        if (usePlayerDodgeBuff) playerDodgeStacks = EditorGUILayout.IntSlider("Dodge Stacks", playerDodgeStacks, 0, 9);

        GUILayout.Space(10);
        GUILayout.Label("Enemy Stats", EditorStyles.boldLabel);
        enemyAttack = EditorGUILayout.IntField("Attack", enemyAttack);
        enemyHP = EditorGUILayout.IntField("HP", enemyHP);

        GUILayout.Space(5);
        GUILayout.Label("Enemy Buffs", EditorStyles.boldLabel);
        useEnemyDR = EditorGUILayout.Toggle("Damage Reduction (Enemy)", useEnemyDR);
        if (useEnemyDR) enemyDRStacks = EditorGUILayout.IntSlider("DR Stacks", enemyDRStacks, 0, 10);
        useEnemyAS = EditorGUILayout.Toggle("Attack Speed Up (Enemy)", useEnemyAS);
        if (useEnemyAS) enemyASStacks = EditorGUILayout.IntSlider("AS Stacks", enemyASStacks, 0, 10);
        useEnemyReflect = EditorGUILayout.Toggle("Reflect Damage (Enemy)", useEnemyReflect);
        if (useEnemyReflect) enemyReflectStacks = EditorGUILayout.IntSlider("Reflect Stacks", enemyReflectStacks, 0, 10);
        useEnemyCritBuff = EditorGUILayout.Toggle("Boost First Crit Chance (Enemy)", useEnemyCritBuff);
        useEnemyDodgeBuff = EditorGUILayout.Toggle("Dodge Rate Buff (Enemy)", useEnemyDodgeBuff);
        if (useEnemyDodgeBuff) enemyDodgeStacks = EditorGUILayout.IntSlider("Dodge Stacks", enemyDodgeStacks, 0, 9);

        GUILayout.Space(10);
        simulationCount = EditorGUILayout.IntField("Simulation Count", simulationCount);

        if (GUILayout.Button("Run Simulation"))
        {
            RunSimulation();
        }

        GUILayout.Space(10);
        GUILayout.Label($"Win Rate: {winRate * 100f:F2}%", EditorStyles.helpBox);
    }

    void RunSimulation()
    {
        int win = 0;

        for (int i = 0; i < simulationCount; i++)
        {
            BattleUnit player = new BattleUnit(playerAttack, playerHP);
            BattleUnit enemy = new BattleUnit(enemyAttack, enemyHP);

            if (CombatSimulator.Simulate(player, enemy,
                usePlayerDR, playerDRStacks, usePlayerAS, playerASStacks, usePlayerReflect, playerReflectStacks, usePlayerCritBuff, usePlayerDodgeBuff, playerDodgeStacks,
                useEnemyDR, enemyDRStacks, useEnemyAS, enemyASStacks, useEnemyReflect, enemyReflectStacks, useEnemyCritBuff, useEnemyDodgeBuff, enemyDodgeStacks))
            {
                win++;
            }
        }

        winRate = (float)win / simulationCount;
    }
}

public class BattleUnit
{
    public int Attack;
    public int HP;

    public BattleUnit(int attack, int hp)
    {
        Attack = attack;
        HP = hp;
    }
}

public static class CombatSimulator
{
    public static bool Simulate(BattleUnit player, BattleUnit enemy,
        bool playerDR, int playerDRStacks, bool playerAS, int playerASStacks, bool playerReflect, int playerReflectStacks, bool playerCritBuff, bool playerDodgeBuff, int playerDodgeStacks,
        bool enemyDR, int enemyDRStacks, bool enemyAS, int enemyASStacks, bool enemyReflect, int enemyReflectStacks, bool enemyCritBuff, bool enemyDodgeBuff, int enemyDodgeStacks)
    {
        int playerHP = player.HP;
        int enemyHP = enemy.HP;

        float playerSpeed = playerAS ? 1f + 0.1f * playerASStacks : 1f;
        float playerReduction = playerDR ? 1f - 0.1f * playerDRStacks : 1f;
        float playerReflectRatio = playerReflect ? 0.1f * playerReflectStacks : 0f;
        float playerDodgeChance = 0.1f + (playerDodgeBuff ? 0.1f * playerDodgeStacks : 0f);

        float enemySpeed = enemyAS ? 1f + 0.1f * enemyASStacks : 1f;
        float enemyReduction = enemyDR ? 1f - 0.1f * enemyDRStacks : 1f;
        float enemyReflectRatio = enemyReflect ? 0.1f * enemyReflectStacks : 0f;
        float enemyDodgeChance = 0.1f + (enemyDodgeBuff ? 0.1f * enemyDodgeStacks : 0f);

        float playerTurn = 0f;
        float enemyTurn = 0f;

        bool playerFirstAttack = true;
        bool enemyFirstAttack = true;

        while (playerHP > 0 && enemyHP > 0)
        {
            if (playerTurn <= enemyTurn)
            {
                if (Random.value < enemyDodgeChance)
                {
                    playerTurn += 1f / playerSpeed;
                    continue;
                }

                float critChance = 0.1f;
                if (playerFirstAttack && playerCritBuff) critChance += 0.5f;
                int baseDamage = Mathf.FloorToInt(Random.Range(0.8f, 1.0f) * player.Attack);
                int finalDamage = Mathf.FloorToInt(baseDamage * (Random.value < critChance ? 2f : 1f));
                int reduced = Mathf.FloorToInt(finalDamage * enemyReduction);
                enemyHP -= Mathf.Max(1, reduced);

                if (enemyReflectRatio > 0f)
                {
                    int reflectDamage = Mathf.FloorToInt(reduced * enemyReflectRatio);
                    playerHP -= Mathf.Max(1, reflectDamage);
                }

                playerFirstAttack = false;
                playerTurn += 1f / playerSpeed;
            }
            else
            {
                if (Random.value < playerDodgeChance)
                {
                    enemyTurn += 1f / enemySpeed;
                    continue;
                }

                float critChance = 0.1f;
                if (enemyFirstAttack && enemyCritBuff) critChance += 0.5f;
                int baseDamage = Mathf.FloorToInt(Random.Range(0.8f, 1.0f) * enemy.Attack);
                int finalDamage = Mathf.FloorToInt(baseDamage * (Random.value < critChance ? 2f : 1f));
                int reduced = Mathf.FloorToInt(finalDamage * playerReduction);
                playerHP -= Mathf.Max(1, reduced);

                if (playerReflectRatio > 0f)
                {
                    int reflectDamage = Mathf.FloorToInt(reduced * playerReflectRatio);
                    enemyHP -= Mathf.Max(1, reflectDamage);
                }

                enemyFirstAttack = false;
                enemyTurn += 1f / enemySpeed;
            }
        }

        return playerHP > 0;
    }
}