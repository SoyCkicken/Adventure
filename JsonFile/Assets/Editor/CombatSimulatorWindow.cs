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

    private float playerCombatPower = 0f;
    private float enemyCombatPower = 0f;
    private float playerWinRate = 0f;

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
        playerAtkBuffStacks = EditorGUILayout.IntSlider("Attack Buff Stacks", playerAtkBuffStacks, 0, 10);
        playerAtkSpeedBuffStacks = EditorGUILayout.IntSlider("Attack Speed Buff Stacks", playerAtkSpeedBuffStacks, 0, 10);
        playerReflectStacks = EditorGUILayout.IntSlider("Reflect Damage Stacks", playerReflectStacks, 0, 10);
        playerCritStacks = EditorGUILayout.IntSlider("Crit Chance Stacks", playerCritStacks, 0, 10);
        playerFirstCritStacks = EditorGUILayout.IntSlider("First Hit Crit Chance Stacks", playerFirstCritStacks, 0, 10);
        playerEvasionStacks = EditorGUILayout.IntSlider("Evasion Stacks", playerEvasionStacks, 0, 10);
        playerLifeStealStacks = EditorGUILayout.IntSlider("Life Steal Stacks", playerLifeStealStacks, 0, 10);

        GUILayout.Space(10);

        GUILayout.Label("Enemy Stats", EditorStyles.boldLabel);
        enemyAttack = EditorGUILayout.IntField("Attack", enemyAttack);
        enemyHP = EditorGUILayout.IntField("HP", enemyHP);
        enemyAtkBuffStacks = EditorGUILayout.IntSlider("Attack Buff Stacks", enemyAtkBuffStacks, 0, 10);
        enemyAtkSpeedBuffStacks = EditorGUILayout.IntSlider("Attack Speed Buff Stacks", enemyAtkSpeedBuffStacks, 0, 10);
        enemyReflectStacks = EditorGUILayout.IntSlider("Reflect Damage Stacks", enemyReflectStacks, 0, 10);
        enemyCritStacks = EditorGUILayout.IntSlider("Crit Chance Stacks", enemyCritStacks, 0, 10);
        enemyFirstCritStacks = EditorGUILayout.IntSlider("First Hit Crit Chance Stacks", enemyFirstCritStacks, 0, 10);
        enemyEvasionStacks = EditorGUILayout.IntSlider("Evasion Stacks", enemyEvasionStacks, 0, 10);
        enemyLifeStealStacks = EditorGUILayout.IntSlider("Life Steal Stacks", enemyLifeStealStacks, 0, 10);

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

            while (pHP > 0 && eHP > 0)
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


