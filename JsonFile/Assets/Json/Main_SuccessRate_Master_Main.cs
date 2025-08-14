using System;
using System.Collections.Generic;
[Serializable]
public class Main_SuccessRate_Master_Main
{
    public string Scene_Code;
    public int Choice_No;
    public string Success_Formula;
    public string Success_Next_Script;
    public string Fail_Next_Script;
    public List<ChoiceRequirement> ChoiceRequirement;
}
