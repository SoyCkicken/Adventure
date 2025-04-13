public class RandomEvent
{
    //RandomEvents_Master_Custom_Format 정보
    //랜덤 이벤트 번호
    public int RandomEvent_Index;
    //랜덤 이벤트의 스크립트 인덱스
    public int Script_Index;
    //랜덤 이벤트의 스크립트 EventScene_1_1일경우 랜덤이벤트의 번호_랜덤 스크립트의 인덱스
    public string Random_Event_ID;
    //어떤 챕터에서만 나올 예정인지
    public int Chapter_Index;
    //이벤트 내용
    public string Event_Text;
    //첫번째 선택지 내용
    public string Choice1_Text;
    //두번째 선택지 내용
    public string Choice2_Text;
    //세번째 선택지 내용
    public string Choice3_Text;
}
