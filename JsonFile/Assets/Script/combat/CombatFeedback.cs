using System;
using System.Collections.Generic;
using MyGame;
using UnityEngine;

public enum CombatFeedbackKind
{
    Info,
    Attack,
    Damage,
    Heal,
    Miss,
    ActionBlocked,
    StatusApplied,
    StatusDamage,
    StatusHeal,
    Cleanse,
    AccuracyDown,
    Death
}

public readonly struct CombatFeedbackEntry
{
    public readonly CombatFeedbackKind Kind;
    public readonly Character Source;
    public readonly Character Target;
    public readonly int Amount;
    public readonly string Message;

    public CombatFeedbackEntry(CombatFeedbackKind kind, Character source, Character target, int amount, string message)
    {
        Kind = kind;
        Source = source;
        Target = target;
        Amount = amount;
        Message = message;
    }
}

public static class CombatFeedback
{
    private const int MaxBufferedEntries = 50;
    private static readonly List<CombatFeedbackEntry> recentEntries = new();

    public static event Action<CombatFeedbackEntry> OnFeedback;

    public static IReadOnlyList<CombatFeedbackEntry> RecentEntries => recentEntries;

    public static void Clear()
    {
        recentEntries.Clear();
    }

    public static void Report(CombatFeedbackKind kind, Character source, Character target, int amount, string message)
    {
        var entry = new CombatFeedbackEntry(kind, source, target, amount, message);
        recentEntries.Add(entry);
        if (recentEntries.Count > MaxBufferedEntries)
            recentEntries.RemoveAt(0);

        Debug.Log(message);
        OnFeedback?.Invoke(entry);
    }
}
