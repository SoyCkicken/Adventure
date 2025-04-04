using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EventLoader : MonoBehaviour
{
    public string eventFileName = "Events/mercenary_event"; // Resources 폴더 기준
    public TMP_Text id_text; 
    public TMP_Text description_text;
    public TMP_Text choices_text;

    void Start()
    {
        TextAsset json = Resources.Load<TextAsset>(eventFileName);
        if (json != null)
        {
            EventNode node = JsonUtility.FromJson<EventNode>(json.text);
            Debug.Log("Event ID: " + node.id);
            id_text.text = node.id.ToString();
            Debug.Log("설명: " + node.description);
            description_text.text = node.description.ToString();
            foreach (var choice in node.choices)
            {
                Debug.Log("선택지: " + choice.text);
                choices_text.text = node.choices.ToString();
            }
        }
        else
        {
            Debug.LogError("이벤트 파일을 불러올 수 없습니다.");
        }
    }
}
