using UnityEditor;
using UnityEngine;

public class CombatSimulatorWindow : EditorWindow
{
    private int playerAttack = 10;
    private int playerHP = 100;
    private int enemyAttack = 10;
    private int enemyHP = 100;

    private int playerAtkBuffStacks = 0;
    private int playerAtkSpeedBuffStacks = 0;
    private int playerReflectStacks = 0;
    private int playerCritStacks = 0;
    private int playerFirstCritStacks = 0;
    private int playerEvasionStacks = 0;
    private int playerLifeStealStacks = 0;

    private int enemyAtkBuffStacks = 0;
    private int enemyAtkSpeedBuffStacks = 0;
    private int enemyReflectStacks = 0;
    private int enemyCritStacks = 0;
    private int enemyFirstCritStacks = 0;
    private int enemyEvasionStacks = 0;
    private int enemyLifeStealStacks = 0;

<<<<<<< Updated upstream
    private float playerCombatPower = 0f;
    private float enemyCombatPower = 0f;
    private float playerWinRate = 0f;
=======
    // 플레이어 버프
    bool usePlayerDR = false, usePlayerAS = false, usePlayerReflect = false;
    int playerDRStacks = 0, playerASStacks = 0, playerReflectStacks = 0;
    bool usePlayerCritBuff = false;
    bool usePlayerDodgeBuff = false;
    int playerDodgeStacks = 0;
    int playerCounterStacks = 0;

    // 적 버프
    bool useEnemyDR = false, useEnemyAS = false, useEnemyReflect = false;
    int enemyDRStacks = 0, enemyASStacks = 0, enemyReflectStacks = 0;
    bool useEnemyCritBuff = false;
    bool useEnemyDodgeBuff = false;
    int enemyDodgeStacks = 0;
    int enemyCounterStacks = 0;

    // 결과 출력
    float winRate = 0;
>>>>>>> Stashed changes

    [MenuItem("Tools/Combat Simulator")]
    public static void ShowWindow()
    {
        GetWindow<CombatSimulatorWindow>("Combat Simulator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Player Stats", EditorStyles.boldLabel);
        playerAttack = EditorGUILayout.IntField("Attack", playerAttack);
        playerHP = EditorGUILayout.IntField("HP", playerHP);
<<<<<<< Updated upstream
        playerAtkBuffStacks = EditorGUILayout.IntSlider("Attack Buff Stacks", playerAtkBuffStacks, 0, 10);
        playerAtkSpeedBuffStacks = EditorGUILayout.IntSlider("Attack Speed Buff Stacks", playerAtkSpeedBuffStacks, 0, 10);
        playerReflectStacks = EditorGUILayout.IntSlider("Reflect Damage Stacks", playerReflectStacks, 0, 10);
        playerCritStacks = EditorGUILayout.IntSlider("Crit Chance Stacks", playerCritStacks, 0, 10);
        playerFirstCritStacks = EditorGUILayout.IntSlider("First Hit Crit Chance Stacks", playerFirstCritStacks, 0, 10);
        playerEvasionStacks = EditorGUILayout.IntSlider("Evasion Stacks", playerEvasionStacks, 0, 10);
        playerLifeStealStacks = EditorGUILayout.IntSlider("Life Steal Stacks", playerLifeStealStacks, 0, 10);
=======

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
        playerCounterStacks = EditorGUILayout.IntSlider("Counter Chance Stacks (Player)", playerCounterStacks, 0, 10);
>>>>>>> Stashed changes

        GUILayout.Space(10);

        GUILayout.Label("Enemy Stats", EditorStyles.boldLabel);
        enemyAttack = EditorGUILayout.IntField("Attack", enemyAttack);
        enemyHP = EditorGUILayout.IntField("HP", enemyHP);
<<<<<<< Updated upstream
        enemyAtkBuffStacks = EditorGUILayout.IntSlider("Attack Buff Stacks", enemyAtkBuffStacks, 0, 10);
        enemyAtkSpeedBuffStacks = EditorGUILayout.IntSlider("Attack Speed Buff Stacks", enemyAtkSpeedBuffStacks, 0, 10);
        enemyReflectStacks = EditorGUILayout.IntSlider("Reflect Damage Stacks", enemyReflectStacks, 0, 10);
        enemyCritStacks = EditorGUILayout.IntSlider("Crit Chance Stacks", enemyCritStacks, 0, 10);
        enemyFirstCritStacks = EditorGUILayout.IntSlider("First Hit Crit Chance Stacks", enemyFirstCritStacks, 0, 10);
        enemyEvasionStacks = EditorGUILayout.IntSlider("Evasion Stacks", enemyEvasionStacks, 0, 10);
        enemyLifeStealStacks = EditorGUILayout.IntSlider("Life Steal Stacks", enemyLifeStealStacks, 0, 10);
=======

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
        enemyCounterStacks = EditorGUILayout.IntSlider("Counter Chance Stacks (Enemy)", enemyCounterStacks, 0, 10);
>>>>>>> Stashed changes

        GUILayout.Space(10);

        if (GUILayout.Button("Simulate"))
        {
            SimulateCombat();
        }

        GUILayout.Space(10);
        GUILayout.Label("Results", EditorStyles.boldLabel);
        GUILayout.Label("Player Combat Power: " + playerCombatPower);
        GUILayout.Label("Enemy Combat Power: " + enemyCombatPower);
        GUILayout.Label("Player Win Rate: " + (playerWinRate * 100f).ToString("F1") + "%");
    }

    private void SimulateCombat()
    {
        float modifiedPlayerAttack = playerAttack * (1f + 0.1f * playerAtkBuffStacks);
        float modifiedPlayerHP = playerHP;
        float playerAttackSpeedMultiplier = 1f + 0.1f * playerAtkSpeedBuffStacks;
        float playerEvasion = 0.05f * playerEvasionStacks;

        float modifiedEnemyAttack = enemyAttack * (1f + 0.1f * enemyAtkBuffStacks);
        float modifiedEnemyHP = enemyHP;
        float enemyAttackSpeedMultiplier = 1f + 0.1f * enemyAtkSpeedBuffStacks;
        float enemyEvasion = 0.05f * enemyEvasionStacks;

        playerCombatPower = (modifiedPlayerAttack * playerAttackSpeedMultiplier * 2f) + modifiedPlayerHP;
        enemyCombatPower = (modifiedEnemyAttack * enemyAttackSpeedMultiplier * 2f) + modifiedEnemyHP;

        int playerWins = 0;
        System.Random rng = new System.Random();

        for (int i = 0; i < 100; i++)
        {
            float pHP = modifiedPlayerHP;
            float eHP = modifiedEnemyHP;
            bool firstPlayerAttack = true;
            bool firstEnemyAttack = true;

<<<<<<< Updated upstream
            while (pHP > 0 && eHP > 0)
=======
            if (CombatSimulator.Simulate(player, enemy,
                usePlayerDR, playerDRStacks, usePlayerAS, playerASStacks, usePlayerReflect, playerReflectStacks, usePlayerCritBuff, usePlayerDodgeBuff, playerDodgeStacks, playerCounterStacks,
                useEnemyDR, enemyDRStacks, useEnemyAS, enemyASStacks, useEnemyReflect, enemyReflectStacks, useEnemyCritBuff, useEnemyDodgeBuff, enemyDodgeStacks, enemyCounterStacks))
>>>>>>> Stashed changes
            {
                // Player hits enemy
                if (rng.NextDouble() >= enemyEvasion)
                {
                    float critChance = 0.1f * playerCritStacks + (firstPlayerAttack ? 0.1f * playerFirstCritStacks : 0f);
                    bool isCrit = rng.NextDouble() < critChance;
                    float damage = modifiedPlayerAttack * (isCrit ? 2f : 1f);
                    float effectiveness = 0.7f + 0.1f * rng.Next(4); // 70%, 80%, 90%, 100%
                    eHP -= damage * effectiveness;
                    pHP += damage * (0.08f * playerLifeStealStacks);
                }
                else if (rng.NextDouble() < playerReflectStacks * 0.1f)
                {
                    pHP += modifiedEnemyAttack * 0.1f * playerReflectStacks;
                }
                firstPlayerAttack = false;

                if (eHP <= 0) break;

                // Enemy hits player
                if (rng.NextDouble() >= playerEvasion)
                {
                    float critChance = 0.1f * enemyCritStacks + (firstEnemyAttack ? 0.1f * enemyFirstCritStacks : 0f);
                    bool isCrit = rng.NextDouble() < critChance;
                    float damage = modifiedEnemyAttack * (isCrit ? 2f : 1f);
                    float effectiveness = 0.7f + 0.1f * rng.Next(4);
                    pHP -= damage * effectiveness;
                    eHP += damage * (0.08f * enemyLifeStealStacks);
                }
                else if (rng.NextDouble() < enemyReflectStacks * 0.1f)
                {
                    eHP += modifiedPlayerAttack * 0.1f * enemyReflectStacks;
                }
                firstEnemyAttack = false;
            }

            if (pHP > 0)
                playerWins++;
        }

        playerWinRate = playerWins / 100f;
    }
}


<<<<<<< Updated upstream
=======
    public BattleUnit(int attack, int hp)
    {
        Attack = attack;
        HP = hp;
    }
}

public static class CombatSimulator
{
    public static bool Simulate(BattleUnit player, BattleUnit enemy,
        bool playerDR, int playerDRStacks, bool playerAS, int playerASStacks, bool playerReflect, int playerReflectStacks, bool playerCritBuff, bool playerDodgeBuff, int playerDodgeStacks, int playerCounterStacks,
        bool enemyDR, int enemyDRStacks, bool enemyAS, int enemyASStacks, bool enemyReflect, int enemyReflectStacks, bool enemyCritBuff, bool enemyDodgeBuff, int enemyDodgeStacks, int enemyCounterStacks)
    {
        int playerHP = player.HP;
        int enemyHP = enemy.HP;

        float playerSpeed = playerAS ? 1f + 0.1f * playerASStacks : 1f;
        float playerReduction = playerDR ? 1f - 0.1f * playerDRStacks : 1f;
        float playerReflectRatio = playerReflect ? 0.1f * playerReflectStacks : 0f;
        float playerDodgeChance = 0.1f + (playerDodgeBuff ? 0.1f * playerDodgeStacks : 0f);
        float playerCounterChance = 0.1f * playerCounterStacks;

        float enemySpeed = enemyAS ? 1f + 0.1f * enemyASStacks : 1f;
        float enemyReduction = enemyDR ? 1f - 0.1f * enemyDRStacks : 1f;
        float enemyReflectRatio = enemyReflect ? 0.1f * enemyReflectStacks : 0f;
        float enemyDodgeChance = 0.1f + (enemyDodgeBuff ? 0.1f * enemyDodgeStacks : 0f);
        float enemyCounterChance = 0.1f * enemyCounterStacks;

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

                if (Random.value < enemyCounterChance)
                {
                    playerHP -= Mathf.Max(1, finalDamage);
                }
                else
                {
                    int reduced = Mathf.FloorToInt(finalDamage * enemyReduction);
                    enemyHP -= Mathf.Max(1, reduced);

                    if (enemyReflectRatio > 0f)
                    {
                        int reflectDamage = Mathf.FloorToInt(reduced * enemyReflectRatio);
                        playerHP -= Mathf.Max(1, reflectDamage);
                    }
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

                if (Random.value < playerCounterChance)
                {
                    enemyHP -= Mathf.Max(1, finalDamage);
                }
                else
                {
                    int reduced = Mathf.FloorToInt(finalDamage * playerReduction);
                    playerHP -= Mathf.Max(1, reduced);

                    if (playerReflectRatio > 0f)
                    {
                        int reflectDamage = Mathf.FloorToInt(reduced * playerReflectRatio);
                        enemyHP -= Mathf.Max(1, reflectDamage);
                    }
                }

                enemyFirstAttack = false;
                enemyTurn += 1f / enemySpeed;
            }
        }

        return playerHP > 0;
    }
}
>>>>>>> Stashed changes
