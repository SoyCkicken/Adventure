using System;
using System.Collections.Generic;
[Serializable]
public class RandomEvents_Master_Event
{
    public int RandomEvent_Index;
    public int Script_Index;
    public string Random_Event_ID;
    public List<int> Chapter_Index;
    public List<EffectTrigger> Main_Effect;
    public string Event_Text;
    public string Choice1_Text;
    public string Choice2_Text;
    public string Choice3_Text;
}
