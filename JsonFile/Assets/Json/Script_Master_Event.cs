public class Script_Master_Event
{
    //Script_Master_Event_Custom_Format
    //이벤트 번호
    public int Event_Index;
    //이벤트 스크립트 번호
    public int EventScript_Index;
    //스크립트 스크립트 예시 (EventScript_1_1 일 경우 이벤트 번호 1 이벤트 스크립트 1)
    public string Script_Code;
    //출력할 내용
    public string KOR;
    //만약 내용이 Text일 경우 문자열만 출력 예정
    //Image일 경우 이미지만 출력 할 예정임
    public string displayType;
    //이벤트 종료 시점
    public string EventBreak;
}
