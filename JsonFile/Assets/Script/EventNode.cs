
using System.Collections.Generic;

public class EventNode
{
    public string id;
    public string description;
    public List<Condition> conditions;
    public List<Choice> choices;
}

[System.Serializable]
public class Condition
{
    public string key;
    public int min;
    public int max;
}

[System.Serializable]
public class Choice
{
    public string text;
    public string nextNodeSuccess;
    public string nextNodeFail;
    public string checkStat;
}

