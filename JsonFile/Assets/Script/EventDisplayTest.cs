using System.Collections.Generic;
using UnityEngine;

public class EventDisplayTest : MonoBehaviour
{
    [SerializeField] private JsonManagerTest jsonManager;
    private System.Random rng = new System.Random();

    private void Start()
    {
        NextRandomEvent();
    }
    /// <summary>
    /// 버튼 등에서 호출: 딕셔너리의 그룹을 랜덤으로 골라
    /// 그 안에 있는 모든 이벤트를 로그로 출력합니다.
    /// </summary>
    public void NextRandomEvent()
    {
        // 1) 사용할 수 있는 그룹 키 목록
        var groupKeys = jsonManager.EventGroupKeys;
        Debug.Log($"[EventDisplay] 사용 가능한 그룹: {groupKeys}");

        if (groupKeys.Count == 0)
        {
            Debug.LogWarning("[EventDisplay] 이벤트 그룹이 하나도 없습니다.");
            return;
        }

        // 2) 랜덤 그룹 선택
        int randomGroup = groupKeys[rng.Next(groupKeys.Count)];
        Debug.Log($"[EventDisplay] 선택된 그룹: {randomGroup}");

        // 3) 선택된 그룹 내 이벤트 리스트 조회
        if (jsonManager.TryGetEventsInGroup(randomGroup, out var events))
        {
            Debug.Log($"[EventDisplay] Group {randomGroup} 에 속한 이벤트 수: {events.Count}");
            // 4) 각 이벤트의 Script_Index 와 텍스트 출력
            foreach (var evt in events)
            {
                Debug.Log($"    ▶ Script_Index {evt.Script_Index} : {evt.Event_Text}");
            }
        }
        else
        {
            Debug.LogWarning($"[EventDisplay] 그룹 {randomGroup} 이벤트를 찾을 수 없습니다.");
        }
    }
}
