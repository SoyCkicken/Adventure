using System;
using System.Collections.Generic;
[Serializable]
public class Option_Master
{
    public string Option_ID;
    public string Option_Description;
    public string Effect_ID;
    public string Option_Type;
    public string StatusType;
    public string ApplyMode;
    public string StackPolicy;
    public int MaxStack;
    public int Duration;
    public int TickInterval;
    public string TriggerType;
    public string ValueMode;
    public int BaseChance;
    public int ChancePerStack;
    public int BaseValue;
    public int ValuePerStack;
    public string StatType;
    public string ResistanceType;
    public int MaxRemoveCount;
}
