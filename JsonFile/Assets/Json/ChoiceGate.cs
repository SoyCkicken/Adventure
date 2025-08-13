using System;

[Serializable]
public class ChoiceGate
{
    public string Req_StatName;  // 예: "STR", "CHA"
    public int Req_StatMin;      // 최소 수치
    public string Req_ItemID;    // 예: "Item_001"
    public int Req_Gold;         // 최소 골드
}