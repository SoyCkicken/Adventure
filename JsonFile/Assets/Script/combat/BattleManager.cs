using System;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private CombatTest combatTest;
    [SerializeField] private TESTBoss testBoss; // 집중 전투용 TESTBoss

    public void StartBattle(Action<bool> onComplete)
    {
        combatTest.RunBattle(onComplete);
    }
    public void FocusBattleStart(Action<bool> onComplete)
    {
        testBoss.RunFocusBattle(onComplete);
    }
    public void StopBattle()
    {
        combatTest.StopBattle();
    }
}