// StoryDisplay.cs
using System.Collections.Generic;
using UnityEngine;

public class StoryDisplayTest : MonoBehaviour
{
    [SerializeField] private JsonManagerTest jsonManager;
    [SerializeField] private int startGroup = 0;

    void Start()
    {
        // 표시 시작까지 소요 시간 측정
        float startMs = Time.realtimeSinceStartup * 1000f;
        DisplayGroup(startGroup);
        float elapsed = Time.realtimeSinceStartup * 1000f - startMs;
        var keys = jsonManager.EventMainKeys;
        Debug.Log($"[StoryDisplay] DisplayGroup({startGroup}) completed in {elapsed:F2} ms");
        foreach (int group in keys)
        {
            if (jsonManager.TryGetMainsInGroup(group, out var list))
                Debug.Log($"[StoryDisplay] Group {group} → 이벤트 {list.Count}개");
        }

        // ② 지정 그룹에 속한 이벤트 텍스트들 출력
        if (jsonManager.TryGetMainsInGroup(startGroup, out var startList))
        {
            Debug.Log($"[StoryDisplay] startGroup({startGroup}) 에 속한 이벤트:");
            foreach (var evt in startList)
                Debug.Log($"    ▶ Script_Index {evt.Scene_Code} : {evt.Scene_Text}");
        }
        else
        {
            Debug.LogWarning($"[StoryDisplay] 그룹 {startGroup} 이벤트를 찾을 수 없습니다.");
        }
    }

    private void DisplayGroup(int groupIdx)
    {
        if (jsonManager.TryGetMainsInGroup(groupIdx, out var list))
        {
            // Script_Index 순으로 정렬되어 있으므로 첫 번째 항목만 사용
            var evt = list[0];
            // 실제 UI 로직 대신 콘솔 출력 예시
            Debug.Log($"[StoryDisplay] Event Text: {evt.Scene_Text}");
        }
        else
        {
            Debug.LogWarning($"No events for group {groupIdx}");
        }
    }
    public void NextMainStory()
    {
        
    }
}