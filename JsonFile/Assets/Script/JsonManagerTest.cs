// JsonManager.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using System.Diagnostics;

public class JsonManagerTest : MonoBehaviour
{
    [Header("JSON File")]
    public string randomEventsFile = "RandomEvents_Master_Custom_Format";
    public string mainStoryFile = "Story_Master_Custom_Format";

    [HideInInspector]
    public List<RandomEvent> randomEvents;
    public List<Story_Master> mainstorys;
    private Dictionary<int, List<RandomEvent>> _randomEventDict;
    private Dictionary<int, List<Story_Master>> _mainStoryDict;

    void Awake()
    {
        // 로드 및 딕셔너리 생성 소요 시간 측정
        var sw = Stopwatch.StartNew();

        // 1) JSON을 List<RandomEvent>로 로드
        randomEvents = LoadJsonFile<RandomEvent>(randomEventsFile);
        mainstorys = LoadJsonFile<Story_Master>(mainStoryFile);

        // 2) 그룹별로 묶어서 Dictionary 생성 (Script_Index 순 정렬 포함)
        _randomEventDict = randomEvents
            .GroupBy(e => e.RandomEvent_Index)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(ev => ev.Script_Index).ToList()
            );
        _mainStoryDict = mainstorys
             .GroupBy(e => e.Event_Index)
             .ToDictionary(
                 g => g.Key,
                 g => g
                     .OrderBy(ev => ev.Scenc_Index)   // 정렬
                     .ToList());
 

         sw.Stop();
        UnityEngine.Debug.Log($"[JsonManager] Loaded {randomEvents.Count} events into {_randomEventDict.Count} groups in {sw.ElapsedMilliseconds} ms");
        UnityEngine.Debug.Log($"[JsonManager] Loaded {mainstorys.Count} events into {_mainStoryDict.Count} groups in {sw.ElapsedMilliseconds} ms");
    }

    /// <summary>
    /// 특정 그룹 인덱스에 속한 이벤트 리스트를 반환합니다.
    /// </summary>
    public bool TryGetEventsInGroup(int groupIndex, out List<RandomEvent> eventsInGroup)
    {
        return _randomEventDict.TryGetValue(groupIndex, out eventsInGroup);
    }
    public bool TryGetMainsInGroup(int groupIndex, out List<Story_Master> story_Masters)
    {
        return _mainStoryDict.TryGetValue(groupIndex, out story_Masters);
    }

    /// <summary>
    /// 현재 로드된 그룹 인덱스 목록을 반환합니다.
    /// </summary>
    public List<int> EventGroupKeys => _randomEventDict.Keys.ToList();
    public List<int> EventMainKeys => _mainStoryDict.Keys.ToList();

    // JSON 파일을 제네릭 리스트로 로드하는 내부 유틸
    private List<T> LoadJsonFile<T>(string fileName)
    {
        // 확장자 제거 후 Resources.Load 사용
        string resourceName = Path.GetFileNameWithoutExtension(fileName);
        TextAsset jsonAsset = Resources.Load<TextAsset>($"Events/{resourceName}");
        if (jsonAsset == null)
        {
            UnityEngine.Debug.LogError($"Failed to load JSON: Events/{resourceName}");
            return new List<T>();
        }
        return JsonConvert.DeserializeObject<List<T>>(jsonAsset.text);
    }
}
