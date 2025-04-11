using System.Collections.Generic;

[System.Serializable]
public class EventDatabase
{
    public List<EventNode> events;
}

[System.Serializable]
public class EventNode
{
    //이벤트 아이디
    public string id;
    //이벤트 설명
    public string description;
    //전조 이벤트(이게 있어야지 이벤트 시작 지점과 랜덤으로 이벤트를 돌릴때 사용이 가능할꺼임
    public string precursor_ranEvent;
    //조건(턴수나 캐릭터 성향 , 스텟 조건등)
    public List<Condition> conditions;
    //선택지
    public List<Choice> choices;
    //성공,실패 했을때 영향
    public List<Effect> effects;
    //선택지가 없는 이벤트 일 경우 사용하는 다음 노드
    public string nextNode;
    
}

[System.Serializable]
public class Condition
{
    //조건(key같은 경우 조건을 만족했는지 안했는지 확인하는용도
    public string key;
    //최소 턴수
    public int min;
    //최대 턴수
    public int max;
}

[System.Serializable]
public class Choice
{
    //선택지 설명
    public string text;
    //성공시 나오는 설명
    public string nextNodeSuccess;
    //실패시 나오는 설명
    public string nextNodeFail;
    //선택지 설명
    public string checkStat;
    //선택지에서 필요한 스탯 포인트
    public string velue;
}

[System.Serializable]
public class Effect
{
    //조건
    public string key;
    //영향(성공시 실패시 다 작성을 해줘야되며 영향을 여러개를 줄 경우 각자 하나씩 작성
    public string value;
}