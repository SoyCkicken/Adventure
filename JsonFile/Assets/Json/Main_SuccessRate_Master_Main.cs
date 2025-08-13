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

    // ✅ 신설: 한 줄짜리 조건 객체
    public ChoiceGate Gate;

    public string Req_StatName;
    public int Req_StatMin;
    public string Req_ItemID;
    public int Req_Gold;

}
