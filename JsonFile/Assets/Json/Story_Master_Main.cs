using System;
using System.Collections.Generic;
[Serializable]
public class Story_Master_Main
{
    public int Chapter_Index;
    public int Event_Index;
    public int Script_Index;
    public string Scene_Code;
    public string Next_Scene;
    public string Script_Text;
    public List<Main_Effect> Main_Effect;
    public string Choice1_Text;
    public string Choice1_Next_Scene;
    public string Choice2_Text;
    public string Choice2_Next_Scene;
    public string Choice3_Text;
    public string Choice3_Next_Scene;
}
