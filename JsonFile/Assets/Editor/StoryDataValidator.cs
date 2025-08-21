//// Assets/Editor/StoryDataValidator.cs
//using System.Linq;
//using System.Collections.Generic;
//using UnityEditor;
//using UnityEngine;

//public static class StoryDataValidator
//{
//    // 실행형 타입 집합
//    static readonly HashSet<string> ExecTypes = new() { "MERCHANT", "BATTLE", "IMAGE", "CLAER" };

//    [MenuItem("Tools/Story/Validate MainStory Data")]
//    public static void Validate()
//    {
//        var jm = Object.FindObjectOfType<JsonManager>();
//        if (jm == null)
//        {
//            Debug.LogError("[Validator] JsonManager가 씬에 없음");
//            return;
//        }

//        var stories = jm.GetStoryMainMasters("Story_Master_Main") ?? new List<Story_Master_Main>();
//        var scripts = jm.GetStoryMainScriptMasters("Main_Script_Master_Main") ?? new List<Main_Script_Master_Main>();

//        // SceneCode → Row 맵
//        var byScene = stories.GroupBy(s => s.Scene_Code?.Trim())
//                             .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Script_Index).ToList());

//        // ScriptCode → Meta 맵
//        var meta = scripts.GroupBy(s => s.Script_Code?.Trim())
//                          .ToDictionary(g => g.Key, g => g.First());

//        int warn = 0;

//        foreach (var row in stories)
//        {
//            string scene = row.Scene_Code?.Trim();
//            string scriptCode = row.Script_Text?.Trim();

//            // 스크립트 메타 확인
//            if (!string.IsNullOrEmpty(scriptCode) && meta.TryGetValue(scriptCode, out var m))
//            {
//                // 실행형인데 Next_Scene이 채워져 있으면 경고 (의도한 분기인지 확인하라는 안내)
//                if (ExecTypes.Contains(m.displayType?.Trim()))
//                {
//                    if (!string.IsNullOrWhiteSpace(row.Next_Scene))
//                    {
//                        Debug.LogWarning($"[Validator] 실행형 뒤 Next_Scene 존재: {scene} -> {row.Next_Scene} (type={m.displayType})");
//                        warn++;
//                    }
//                }
//            }
//            else
//            {
//                Debug.LogWarning($"[Validator] 스크립트 메타를 찾지 못함: Scene={scene}, Script={scriptCode}");
//                warn++;
//            }

//            // Next_Scene 유효성 체크
//            if (!string.IsNullOrWhiteSpace(row.Next_Scene))
//            {
//                string next = row.Next_Scene.Trim();
//                if (!byScene.ContainsKey(next))
//                {
//                    Debug.LogWarning($"[Validator] Next_Scene 대상 없음: {scene} -> {next}");
//                    warn++;
//                }
//                else
//                {
//                    // 바로 Break로 끝나는지 간단 스캔(첫 노드 메타 확인)
//                    var nextRow = byScene[next].First();
//                    var nextCode = nextRow.Script_Text?.Trim();
//                    if (!string.IsNullOrEmpty(nextCode) && meta.TryGetValue(nextCode, out var nm))
//                    {
//                        if ((nm.StoryBreak?.Trim()) == "Break")
//                        {
//                            Debug.LogWarning($"[Validator] Next_Scene 점프 즉시 Break 의심: {scene} -> {next} (Script={nextCode})");
//                            warn++;
//                        }
//                    }
//                }
//            }
//        }

//        if (warn == 0) Debug.Log("[Validator] 이상 없음.");
//        else Debug.Log($"[Validator] 경고 {warn}개. 콘솔 로그 참조.");
//    }
//}
