using System;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private CombatTest combatTest;

    public void StartBattle(Action<bool> onComplete)
    {
        combatTest.RunBattle(onComplete);
    }

    public void StopBattle()
    {
        combatTest.StopBattle();
    }
}