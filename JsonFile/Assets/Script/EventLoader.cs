using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EventLoader : MonoBehaviour
{
    public Dictionary<string, EventNode> eventMap;
    public TMP_Text id_text;
    public TMP_Text description_text;
    public TMP_Text choices_text;

    void Start()
    {
        //이벤트 불러오고
        LoadEventData();

        // 테스트용: 시작 이벤트 노드 출력
        //text에 출력하는 정도만 작성해놨음
        DisplayEvent("mercenary_event");
    }

    void LoadEventData()
    {
        //json파일 불러옴
        TextAsset json = Resources.Load<TextAsset>("Events/mercenary_event");
        if (json != null)
        {
            //
            EventDatabase database = JsonUtility.FromJson<EventDatabase>(json.text);
            eventMap = new Dictionary<string, EventNode>();
            foreach (var node in database.events)
            {
                eventMap[node.id] = node;
                Debug.Log(node.id);
            }

            Debug.Log($"이벤트 총 {eventMap.Count}개 로드됨");
        }
        else
        {
            Debug.LogError("이벤트 JSON 로딩 실패");
        }
    }

    //이벤트 받아오기
    public EventNode GetEvent(string id)
    {
        //있을경우 넘기고
        if (eventMap.ContainsKey(id)) return eventMap[id];
        //없을경우 debug.LogWarning으로 경고
        Debug.LogWarning("없는 이벤트 ID: " + id);
        return null;
    }

    void DisplayEvent(string id)
    {
        //이벤트 id값이 비어 있는 경우
        if (!eventMap.ContainsKey(id))
        {
            //에러
            Debug.LogError("해당 ID의 이벤트를 찾을 수 없음: " + id);
            return;
        }
        //아닐경우 id에 쭉 넣어서 text에 출력하는중
        EventNode node = eventMap[id];
        id_text.text = $"ID: {node.id}";
        description_text.text = node.description;
        //선택지가 없지 않거나 , 선택지 숫자가 1개 이상일때
        if (node.choices != null && node.choices.Count > 0)
        {
            //선택지 문자열 받아올거
            string choiceText = "";
            //for문으로 넣어서
            for (int i = 0; i < node.choices.Count; i++)
            {
                //저장
                choiceText += $"{i + 1}. {node.choices[i].text}\n";
            }
            //출력
            choices_text.text = choiceText;
        }
        else
        {
            //선택지가 없다고 출력해줌
            choices_text.text = "(선택지 없음)";
        }
    }
}
