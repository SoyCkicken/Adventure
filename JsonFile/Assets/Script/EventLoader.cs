using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Text;
using UnityEditor.Experimental.GraphView;
using System.Runtime.CompilerServices;

public class EventLoader : MonoBehaviour
{
    public Dictionary<string, EventNode> eventMap;  //이벤트 저장 중 
    public Player player;                           //임시
    public TMP_Text id_text;                        //이벤트 ID
    public TMP_Text description_text;               //이벤트 설명(내용)
    //public TMP_Text choices_text;                   //선택지
    public GameObject choiceButtonPrefab; // 버튼 프리팹 연결 필요
    public Transform choiceContainer; // 버튼이 붙을 부모 오브젝트
    public GameObject SkipButton;           //스킵 버튼
    private bool isTyping = false; //타이핑중인지(TypeTextEffect에서 글씨를 추가 중인지 확인하는 변수)
    private bool isSkipClick = false; //스킵 버튼을 눌렀는지
    private Coroutine TypingCorouting; //지금남은 글자들을 합쳐버릴 코루틴 변수 선언
    public List<string> ranEvent_List;
    public int ranEvent_Count;


    private void Awake()
    {
        //이벤트 불러오고
        LoadEventData();
        //이벤트 길이까지만 
        

        //스킵 버튼이 눌렸을때 스킵을 시켜버림
        SkipButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (isTyping)
                isSkipClick = true;
        });
    }


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
        //코루틴으로 정리 했음
        StartCoroutine(RamdomEvent());

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
                if (eventMap[node.id].precursor_ranEvent != null)
                {
                    ranEvent_List.Add(node.id);
                }
                Debug.Log(node.id);
            }

            for (int i = 0; i < ranEvent_List.Count; i++)
            {
                Debug.Log(ranEvent_List[i]);
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
        EventNode node = eventMap[id];
        Debug.Log($"DisplayEvent에서 발생한 디버그입니다 : {node.id}");
        //혹시 모르니 랜덤 이벤트 일경우 발동
        if (node.id == "RamEvent")
        {
            StartCoroutine(RamdomEvent());
            return;
            }

        //이벤트 id값이 비어 있는 경우
        if (!eventMap.ContainsKey(id)) return;
       
        //아닐경우 id에 쭉 넣어서 text에 출력하는중

        id_text.text = $"ID: {node.id}";
        StartCoroutine(TypeTextEffect(node.description));
        Debug.Log($"DisplayEvent에서 발생한 디버그입니다 : {node.description}");

        // 기존 선택지 버튼 삭제
        foreach (Transform child in choiceContainer)
            Destroy(child.gameObject);

        //선택지가 없지 않거나 , 선택지 숫자가 1개 이상일때
        if (node.choices != null && node.choices.Count > 0)
        {
            foreach (var choice in node.choices)
            {
                //버튼의 인스턴스를 생성(복사본)
                GameObject btnObj = Instantiate(choiceButtonPrefab, choiceContainer);
                //버튼 텍스트를 받아옴
                TMP_Text btnText = btnObj.GetComponentInChildren<TMP_Text>();
                //버튼 텍스트에 선택지 내용을 담아서 출력
                btnText.text = choice.text;
                //버튼 컴포넌트
                Button btn = btnObj.GetComponent<Button>();
                //클릭시 발동
                btn.onClick.AddListener(() =>
                {
                    if (isTyping)
                    {
                        isSkipClick = true; // 텍스트 출력 중이면 스킵 요청
                        return;
                    }
                    HandleChoice(choice);   // 클릭 시 처리
                    //Debug.Log($"<color=red>Red{choice.text} 선택지 출력</color>");      //확인 용
                    if (TypingCorouting != null)
                        StopCoroutine(TypingCorouting);
                    //TypingCorouting = StartCoroutine(TypeTextEffect(eventMap[choice.nextNodeSuccess].description));
                });
            }
        }
        else
        {
            // 선택지가 없고 다음 노드가 지정된 경우 → 자동 진행
            if (node.precursor_ranEvent == "true")
            {
                //Debug.Log($"선택지 없음 → 다음 노드 자동 진행: {node.nextNode}");
                //if (Input.GetMouseButtonDown(0))
                //{
                //    Debug.Log("마우스 버튼 눌림!");

                //}
                //DisplayEvent(node.nextNode);

                ranEvent_Count = UnityEngine.Random.Range(0, ranEvent_List.Count);
                Debug.Log(ranEvent_Count);
                Debug.Log($"<b><color=#FF0000>{ranEvent_List[ranEvent_Count].ToString()}</color></b>");

                // 테스트용: 시작 이벤트 노드 출력
                //text에 출력하는 정도만 작성해놨음
                DisplayEvent(ranEvent_List[ranEvent_Count].ToString());
                //이벤트 종료시 리스트에서 삭제
                ranEvent_List.Remove(ranEvent_List[ranEvent_Count]);

            }
            else
            {
                // 마지막 노드이거나 후속 이벤트 없음
                //choices_text.text = "(선택지 없음)";
            }
        }
    }


    //선택지를 받아 오는데 해당 선택지의 조건 등을 확인 
    void HandleChoice(Choice choice)
    {
        
        int playerStat = GetPlayerStat(choice.checkStat);
        Debug.Log(playerStat);
        bool success = playerStat >= Convert.ToInt32(choice.velue); // 기준 수치 (예시로 5 이상이면 성공)
        Debug.Log(Convert.ToInt32(choice.velue));
        Debug.Log($"선택지에서 출력하는 디버그 입니다 {success}");
        string nextId = success ? choice.nextNodeSuccess : choice.nextNodeFail;
        
        Debug.Log($"선택지 결과: {(success ? "성공" : "실패")} → {nextId}");
        DisplayEvent(nextId);
    }
    //텍스트 이펙트를 구현을 위한 아이 이너머레이블 구현
    IEnumerator TypeTextEffect(string text)
    {
        isTyping = true;
        isSkipClick = false;
        SkipButton.SetActive(true);
        Debug.Log("스킵버튼 활성화");
        description_text.text = string.Empty; //문자열을 비우고
        //temp에 원본 텍스트랑 추가할 텍스트를 담아서
        string temp = description_text.text + text;
        Debug.Log(description_text.text);
        //스트링빌더(한글자씩 추가해주는 함수)
        StringBuilder stringBuilder = new StringBuilder();
        if (text != null)
        {
            for (int i = 0; i < text.Length; i++)
            {
                //스킵 버튼 눌렸을때 바로 문자열에 다 쑤셔넣음
                if (isSkipClick)
                {
                    //한번에 다 넣어 버림
                    description_text.text = temp;
                    yield break;
                }
                else 
                {
                    stringBuilder.Append(text[i]);
                    //받은 문자들을 text에 담아서 
                    description_text.text = stringBuilder.ToString();
                    //0.01초마다 한번씩 출력시킴
                    yield return new WaitForSeconds(0.05f);
                    //한글자씩 추가 중
                }
            }
        }
        else
        {
            //RamEvent같은 경우 설명 같은게 하나도 없기 때문에 에러가 발생을 하는데 그걸 막고자 if문 사용했음
            yield break;
        }
        
        isTyping = false;
        SkipButton.SetActive(false);
        isSkipClick = false;
        Debug.Log("스킵버튼 비활성화");
    }
    IEnumerator RamdomEvent()
    {
        ranEvent_Count = UnityEngine.Random.Range(0, ranEvent_List.Count);
        Debug.Log(ranEvent_Count);
        Debug.Log($"<b><color=#FF0000> 해당 이벤트는 랜덤 이벤트 코루틴에서 발생 하였습니다\n{ranEvent_List[ranEvent_Count].ToString()}</color></b>");

        // 테스트용: 시작 이벤트 노드 출력
        //text에 출력하는 정도만 작성해놨음
        DisplayEvent(ranEvent_List[ranEvent_Count].ToString());
        //이벤트 종료시 리스트에서 삭제
        ranEvent_List.Remove(ranEvent_List[ranEvent_Count]);

        yield return null;
    }
}


/*
 지금 구현 해야 하는것 -> 랜덤이벤트 발생을 하면 대부분 1회성 이벤트 들이니
                        해당 이벤트가 종료되면 다시 랜덤한 이벤트를 발생 시키게 하면 된다

-           구현 완료                  -

 */