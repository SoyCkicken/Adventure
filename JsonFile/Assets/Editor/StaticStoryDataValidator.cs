// Assets/Editor/StaticStoryDataValidator.cs
// ⚠️ 에디터 전용. 런타임/씬/싱글톤 불필요.
// 엑셀에서 직렬화된 JSON 2개(Story_Master_Main, Main_Script_Master_Main)를 지정 폴더에서 읽어 검사한다.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

public static class StaticStoryDataValidator
{
    // 실행형 타입 집합 (라벨 억제 대상 제외)
    static readonly HashSet<string> ExecTypes = new(StringComparer.OrdinalIgnoreCase)
    { "MERCHANT", "BATTLE", "IMAGE", "CLAER" };

    [MenuItem("Tools/Story/Static Validate (JSON Folder)...")]
    public static void ValidateFolder()
    {
        // 1) JSON 폴더 선택
        string folder = EditorUtility.OpenFolderPanel("Select JSON Folder", Application.dataPath, "");
        if (string.IsNullOrEmpty(folder)) return;

        // 2) 후보 파일 자동 탐색 (파일명 커스텀이면 아래 패턴을 바꿔)
        //    - Story_Master_Main*.json
        //    - Main_Script_Master_Main*.json
        string[] storyFiles = Directory.GetFiles(folder, "Story_Master_Main*.json", SearchOption.AllDirectories);
        string[] scriptFiles = Directory.GetFiles(folder, "Main_Script_Master_Main*.json", SearchOption.AllDirectories);

        if (storyFiles.Length == 0 || scriptFiles.Length == 0)
        {
            EditorUtility.DisplayDialog("Validator", "필수 JSON을 찾지 못했습니다.\n- Story_Master_Main*.json\n- Main_Script_Master_Main*.json", "확인");
            return;
        }

        // 가장 최신(수정시간) 파일 하나씩 선택
        string storyPath = storyFiles.OrderByDescending(File.GetLastWriteTimeUtc).First();
        string scriptPath = scriptFiles.OrderByDescending(File.GetLastWriteTimeUtc).First();

        // 3) 로드 & 파싱
        if (!TryLoadList(storyPath, out List<Story_Master_Main> stories))
        {
            EditorUtility.DisplayDialog("Validator", "Story_Master_Main 파싱 실패", "확인");
            return;
        }
        if (!TryLoadList(scriptPath, out List<Main_Script_Master_Main> scripts))
        {
            EditorUtility.DisplayDialog("Validator", "Main_Script_Master_Main 파싱 실패", "확인");
            return;
        }

        // 4) 인덱싱
        // Scene_Code → 같은 씬의 행들(보통 1행이 시작)
        var byScene = stories
            .Where(s => !string.IsNullOrWhiteSpace(s.Scene_Code))
            .GroupBy(s => s.Scene_Code.Trim())
            .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Script_Index).ToList());

        // Script_Code → 스크립트 메타
        var meta = scripts
            .Where(s => !string.IsNullOrWhiteSpace(s.Script_Code))
            .GroupBy(s => s.Script_Code.Trim())
            .ToDictionary(g => g.Key, g => g.First());

        // 5) 검사
        int warn = 0;
        var report = new StringBuilder();
        report.AppendLine("Type,Scene,Detail");

        foreach (var row in stories)
        {
            string scene = Trim(row.Scene_Code);
            string sCode = Trim(row.Script_Text);
            string nextScene = Trim(row.Next_Scene);

            // 5-1) 스크립트 메타 확인
            Main_Script_Master_Main m = null;
            if (!string.IsNullOrEmpty(sCode))
                meta.TryGetValue(sCode, out m);

            if (m == null)
            {
                Warn($"[META_MISS] 메타 없음: Scene={scene}, Script={sCode}", scene);
                continue;
            }

            // 5-2) 실행형 뒤 Next_Scene 경고 (데이터 실수 빈발)
            if (!string.IsNullOrEmpty(nextScene) && ExecTypes.Contains(Trim(m.displayType)))
            {
                Warn($"[EXEC_NEXT] 실행형 뒤 Next_Scene 존재: {scene} -> {nextScene} (type={m.displayType})", scene);
            }

            // 5-3) Next_Scene 유효성 및 즉시 Break 의심
            if (!string.IsNullOrEmpty(nextScene))
            {
                if (!byScene.TryGetValue(nextScene, out var list))
                {
                    Warn($"[NEXT_404] Next_Scene 대상 없음: {scene} -> {nextScene}");
                }
                else
                {
                    // 점프 대상 첫 행의 스크립트가 Break인지 검사
                    var first = list.First();
                    var firstMeta = !string.IsNullOrEmpty(first.Script_Text) && meta.TryGetValue(Trim(first.Script_Text), out var fm) ? fm : null;
                    if (firstMeta == null)
                    {
                        Warn($"[NEXT_META_MISS] 점프 대상 메타 없음: {scene} -> {nextScene} (Script={first.Script_Text})");
                    }
                    else if (string.Equals(Trim(firstMeta.StoryBreak), "Break", StringComparison.OrdinalIgnoreCase))
                    {
                        Warn($"[NEXT_BREAK] 점프 즉시 Break 의심: {scene} -> {nextScene} (Script={first.Script_Text})");
                    }
                }
            }
        }

        // 6) 결과 출력 + CSV 저장
        if (warn == 0)
        {
            Debug.Log("[StaticValidator] 이상 없음.");
            EditorUtility.DisplayDialog("Validator", "이상 없음.", "확인");
        }
        else
        {
            string outDir = "Assets/Validation";
            if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);
            string outPath = Path.Combine(outDir, $"StoryReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            File.WriteAllText(outPath, report.ToString(), Encoding.UTF8);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Validator", $"경고 {warn}개. 콘솔/CSV 참조.\n{outPath}", "확인");
        }

        // 지역 함수들
        void Warn(string msg, string sceneCode = null)
        {
            warn++;
            Debug.LogWarning(msg);

            // CSV: 타입, 씬, 메시지
            var parts = msg.Split(new[] { ' ' }, 2);
            string type = parts.Length > 0 ? parts[0].Trim('[', ']') : "WARN";
            report.AppendLine($"{type},{Safe(sceneCode)},{Safe(msg)}");
        }

        static string Trim(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        static string Safe(string s) => (s ?? "").Replace(",", " ");
    }

    // Story/Script JSON 파일을 명명된 루트, Wrapper<T>, 배열 루트 포맷으로 지원
    static bool TryLoadList<T>(string path, out List<T> list)
    {
        list = null;
        try
        {
            string json = File.ReadAllText(path, Encoding.UTF8).Trim();
            JToken root = JToken.Parse(json);
            JToken arrayToken = null;

            if (root is JArray)
            {
                arrayToken = root;
            }
            else if (root is JObject obj)
            {
                arrayToken = obj[typeof(T).Name] ?? obj["items"];
            }

            if (arrayToken == null || arrayToken.Type != JTokenType.Array)
            {
                Debug.LogError($"[StaticValidator] {typeof(T).Name} 배열 루트를 찾지 못했습니다: {path}");
                list = new List<T>();
                return false;
            }

            // JsonUtility는 배열 루트 직파싱이 안 되므로 프로젝트 공용 Wrapper<T>로 감싼다.
            string wrappedJson = "{\"items\":" + arrayToken.ToString(Formatting.None) + "}";
            var w2 = JsonUtility.FromJson<Wrapper<T>>(wrappedJson);
            list = w2?.items ?? new List<T>();
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[StaticValidator] {typeof(T).Name} 로드 실패: {path}\n{ex}");
            return false;
        }
    }
}
