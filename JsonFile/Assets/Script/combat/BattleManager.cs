using System;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private CombatTest combatTest;
    [SerializeField] private BossPartCombatManager bossPartCombatManager; // 집중 전투용 TESTBoss

    public void StartBattle(Action<bool> onComplete)
    {
        combatTest.RunBattle(onComplete);
    }
    public void FocusBattleStart(Action<bool> onComplete)
    {
        bossPartCombatManager.RunFocusBattle(onComplete);
    }
    public void StopBattle()
    {
        combatTest.StopBattle();
    }
    public void StopFocusBattle()
    {
        bossPartCombatManager.StopFocusBattle();
    }   
}