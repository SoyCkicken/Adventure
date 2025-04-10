using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Text;

public class EventLoader : MonoBehaviour
{
    public Dictionary<string, EventNode> eventMap;  //이벤트 저장 중 
    public Player player;                           //임시
    public TMP_Text id_text;                        //이벤트 ID
    public TMP_Text description_text;               //이벤트 설명(내용)
    public TMP_Text choices_text;                   //선택지
    public GameObject choiceButtonPrefab; // 버튼 프리팹 연결 필요
    public Transform choiceContainer; // 버튼이 붙을 부모 오브젝트


    
    
    //플레이어의 스텟을 받아오고
    int GetPlayerStat(string stat)
    {
        switch (stat.ToLower())
        {
            case "strength": return player.Strength;
            case "agility": return player.Agility;
            case "intelligence": return player.Intelligence;
            case "magic": return player.Magic;
            case "divinity": return player.Divinity;
            case "charisma": return player.Charisma;
            case "HP": return player.HP;
            case "MP": return player.MP;
                //필요시 플레이어에서 스탯 추가 후 여기에 추가 하면 됩니다
            default:
                Debug.LogWarning("알 수 없는 스탯: " + stat);
                return 0;
        }
    }
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

    //void DisplayEvent(string id)
    //{
    //    //이벤트 id값이 비어 있는 경우
    //    if (!eventMap.ContainsKey(id))
    //    {
    //        //에러
    //        Debug.LogError("해당 ID의 이벤트를 찾을 수 없음: " + id);
    //        return;
    //    }
    //    //아닐경우 id에 쭉 넣어서 text에 출력하는중
    //    EventNode node = eventMap[id];
    //    id_text.text = $"ID: {node.id}";
    //    description_text.text = node.description;
    //    //선택지가 없지 않거나 , 선택지 숫자가 1개 이상일때
    //    if (node.choices != null && node.choices.Count > 0)
    //    {
    //        //선택지 문자열 받아올거
    //        string choiceText = "";
    //        //for문으로 넣어서
    //        for (int i = 0; i < node.choices.Count; i++)
    //        {
    //            //저장
    //            choiceText += $"{i + 1}. {node.choices[i].text}\n";
    //        }
    //        //출력
    //        choices_text.text = choiceText;
    //    }
    //    else
    //    {
    //        //선택지가 없다고 출력해줌
    //        choices_text.text = "(선택지 없음)";
    //    }
    //}
    void DisplayEvent(string id)
    {
        if (!eventMap.ContainsKey(id)) return;

        EventNode node = eventMap[id];
        id_text.text = $"ID: {node.id}";
        //StartCoroutine(TypeTextEffect(node.description));

        // 기존 선택지 버튼 삭제
        foreach (Transform child in choiceContainer)
            Destroy(child.gameObject);

        if (node.choices != null && node.choices.Count > 0)
        {
            foreach (var choice in node.choices)
            {
                GameObject btnObj = Instantiate(choiceButtonPrefab, choiceContainer);
                TMP_Text btnText = btnObj.GetComponentInChildren<TMP_Text>();
                btnText.text = choice.text;

                Button btn = btnObj.GetComponent<Button>();
                btn.onClick.AddListener(() =>
                {
                    HandleChoice(choice); // 클릭 시 처리
                    Debug.Log(choice);
                });
            }
        }
        else
        {
            // 선택지가 없고 다음 노드가 지정된 경우 → 자동 진행
            if (!string.IsNullOrEmpty(node.nextNode))
            {
                Debug.Log($"선택지 없음 → 다음 노드 자동 진행: {node.nextNode}");
                DisplayEvent(node.nextNode);
            }
            else
            {
                // 마지막 노드이거나 후속 이벤트 없음
                choices_text.text = "(선택지 없음)";
            }
        }
    }
    //선택지를 받아 오는데 해당 선택지의 조건 등을 확인 
    void HandleChoice(Choice choice)
    {
        int playerStat = GetPlayerStat(choice.checkStat);
        bool success = playerStat >= Convert.ToInt32(choice.velue); // 기준 수치 (예시로 5 이상이면 성공)
        Debug.Log(Convert.ToInt32(choice.velue));

        string nextId = success ? choice.nextNodeSuccess : choice.nextNodeFail;
        Debug.Log($"선택지 결과: {(success ? "성공" : "실패")} → {nextId}");
        DisplayEvent(nextId);
    }
    //텍스트 이펙트를 구현을 위한 아이 이너머레이블 구현
    IEnumerable TypeTextEffect(string text)
    {
        description_text.text = string.Empty; //문자열을 비우고
        StringBuilder stringBuilder = new StringBuilder();
        for (int i = 0; i < text.Length; i++)
        {
            stringBuilder.Append(text[i]);
            description_text.text = stringBuilder.ToString();
            yield return new WaitForSeconds(0.01f);
        }
       
    }
}
