// Assets/Scripts/Editor/SpriteAtlasDuplicateInspector.cs
// 개선된 SpriteAtlas 중복 검사 도구
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

public class SpriteAtlasDuplicateInspector : EditorWindow
{
    [MenuItem("Tools/Sprites/SpriteAtlas Duplicate Inspector")]
    public static void Open() => GetWindow<SpriteAtlasDuplicateInspector>("SpriteAtlas Duplicates");

    // 진행률 표시를 위한 상수
    private const int PROGRESS_THRESHOLD = 10;
    
    [Serializable]
    private class SpriteKey : IEquatable<SpriteKey>
    {
        public string guid;
        public long localId;
        public bool Equals(SpriteKey other) => other != null && guid == other.guid && localId == other.localId;
        public override bool Equals(object obj) => Equals(obj as SpriteKey);
        public override int GetHashCode() => HashCode.Combine(guid, localId);
        public override string ToString() => $"{guid}:{localId}";
    }

    private class SpriteEntry
    {
        public Sprite sprite;
        public string path;
        public string name;
        public SpriteKey key;
        public long fileSize; // 파일 크기 정보 추가
    }

    private class AtlasHit
    {
        public SpriteAtlas atlas;
        public HashSet<UnityEngine.Object> packables = new HashSet<UnityEngine.Object>();
        public string atlasPath; // 캐시된 경로
    }

    private class DuplicateRecord
    {
        public SpriteEntry sprite;
        public List<AtlasHit> hits = new List<AtlasHit>();
        public bool foldout;
        public long totalWastedSize; // 중복으로 인한 낭비 공간
    }

    // UI 상태
    private readonly List<SpriteAtlas> atlases = new List<SpriteAtlas>();
    private Vector2 atlasListScroll;
    private Vector2 resultScroll;
    private string search = "";
    private bool scanned = false;
    private bool showProgressBar = false;
    private float progress = 0f;
    private string progressText = "";

    // 결과 데이터
    private readonly List<DuplicateRecord> duplicates = new List<DuplicateRecord>();
    private int spriteTotal = 0;
    private long totalWastedBytes = 0;

    // 정렬 옵션
    private enum SortBy { Name, Path, AtlasCount, WastedSize }
    private SortBy currentSort = SortBy.Name;
    private bool sortAscending = true;

    void OnGUI()
    {
        // 진행률 표시
        if (showProgressBar)
        {
            EditorGUI.ProgressBar(new Rect(0, 0, position.width, 20), progress, progressText);
            GUILayout.Space(25);
        }

        EditorGUILayout.LabelField("SpriteAtlas Drag & Drop", EditorStyles.boldLabel);
        DrawDropArea();

        EditorGUILayout.Space(4);
        DrawAtlasList();
        DrawControlButtons();

        if (!scanned) return;

        EditorGUILayout.Space(6);
        DrawSummary();
        DrawSearchAndSort();
        EditorGUILayout.Space(2);
        DrawResultList();
    }

    private void DrawDropArea()
    {
        var rect = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));
        var style = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.MiddleCenter };
        GUI.Box(rect, "여기에 SpriteAtlas(.spriteatlas) 에셋을 드래그·드롭", style);
        
        HandleDragAndDrop(rect);
    }

    private void HandleDragAndDrop(Rect rect)
    {
        var e = Event.current;
        if (!rect.Contains(e.mousePosition)) return;

        if (e.type == EventType.DragUpdated || e.type == EventType.DragPerform)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (e.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                int addedCount = 0;
                foreach (var obj in DragAndDrop.objectReferences)
                {
                    if (obj is SpriteAtlas sa && !atlases.Contains(sa))
                    {
                        atlases.Add(sa);
                        addedCount++;
                    }
                }
                if (addedCount > 0)
                {
                    ShowNotification(new GUIContent($"{addedCount}개 Atlas 추가됨"));
                    scanned = false; // 재스캔 필요
                }
                Repaint();
            }
            e.Use();
        }
    }

    private void DrawAtlasList()
    {
        using (new EditorGUILayout.VerticalScope("box"))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("대상 SpriteAtlas 목록", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField($"({atlases.Count}개)", EditorStyles.miniLabel);
            }
            
            atlasListScroll = EditorGUILayout.BeginScrollView(atlasListScroll, GUILayout.Height(140));
            
            for (int i = 0; i < atlases.Count; i++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    atlases[i] = (SpriteAtlas)EditorGUILayout.ObjectField(atlases[i], typeof(SpriteAtlas), false);
                    
                    // 버튼들을 더 작고 일관되게
                    if (GUILayout.Button("핑", GUILayout.Width(40)))
                    {
                        EditorGUIUtility.PingObject(atlases[i]);
                        Selection.activeObject = atlases[i];
                    }
                    if (GUILayout.Button("×", GUILayout.Width(25)))
                    {
                        atlases.RemoveAt(i);
                        i--;
                        scanned = false;
                    }
                }
            }
            EditorGUILayout.EndScrollView();

            DrawAtlasButtons();
        }
    }

    private void DrawAtlasButtons()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Atlas 추가", GUILayout.Width(100)))
            {
                AddAtlasFromDialog();
            }
            
            if (GUILayout.Button("프로젝트의 모든 Atlas", GUILayout.Width(150)))
            {
                AddAllAtlasesInProject();
            }
            
            GUILayout.FlexibleSpace();
            
            GUI.enabled = atlases.Count > 0;
            if (GUILayout.Button("전체 비우기", GUILayout.Width(100)))
            {
                atlases.Clear();
                scanned = false;
            }
            GUI.enabled = true;
        }
    }

    private void AddAtlasFromDialog()
    {
        var path = EditorUtility.OpenFilePanel("SpriteAtlas 선택", Application.dataPath, "spriteatlas");
        if (!string.IsNullOrEmpty(path))
        {
            var projPath = "Assets" + path.Replace(Application.dataPath, "");
            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(projPath);
            if (atlas != null && !atlases.Contains(atlas))
            {
                atlases.Add(atlas);
                scanned = false;
            }
        }
    }

    private void AddAllAtlasesInProject()
    {
        var guids = AssetDatabase.FindAssets("t:SpriteAtlas");
        int addedCount = 0;
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
            if (atlas != null && !atlases.Contains(atlas))
            {
                atlases.Add(atlas);
                addedCount++;
            }
        }
        if (addedCount > 0)
        {
            ShowNotification(new GUIContent($"{addedCount}개 Atlas 추가됨"));
            scanned = false;
        }
    }

    private void DrawControlButtons()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            GUI.enabled = atlases.Count > 1 && !showProgressBar;
            if (GUILayout.Button("중복 스프라이트 스캔", GUILayout.Height(26)))
                ScanDuplicatesAsync();

            GUI.enabled = scanned && duplicates.Count > 0 && !showProgressBar;
            if (GUILayout.Button("CSV로 내보내기", GUILayout.Height(26)))
                ExportCsv();
            
            if (GUILayout.Button("상세 보고서", GUILayout.Height(26)))
                ExportDetailedReport();

            GUI.enabled = true;
        }
    }

    private void DrawSummary()
    {
        string sizeInfo = totalWastedBytes > 0 ? $", 낭비 용량: {FormatBytes(totalWastedBytes)}" : "";
        
        EditorGUILayout.HelpBox(
            $"총 스프라이트: {spriteTotal}, 중복 발견: {duplicates.Count}개{sizeInfo}\n" +
            $"(아래는 스프라이트별로 포함된 아틀라스 목록입니다. 제거/리패킹은 수동으로 진행하세요.)",
            MessageType.Info);
    }

    private void DrawSearchAndSort()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            // 검색
            GUILayout.Label("검색", GUILayout.Width(40));
            string newSearch = EditorGUILayout.TextField(search, GUILayout.ExpandWidth(true));
            if (newSearch != search)
            {
                search = newSearch;
            }
            
            // 정렬
            GUILayout.Label("정렬", GUILayout.Width(30));
            var newSort = (SortBy)EditorGUILayout.EnumPopup(currentSort, GUILayout.Width(100));
            if (newSort != currentSort)
            {
                currentSort = newSort;
                SortDuplicates();
            }
            
            if (GUILayout.Button(sortAscending ? "↑" : "↓", GUILayout.Width(25)))
            {
                sortAscending = !sortAscending;
                SortDuplicates();
            }
        }
    }

    private void SortDuplicates()
    {
        duplicates.Sort((a, b) =>
        {
            int result = currentSort switch
            {
                SortBy.Name => string.CompareOrdinal(a.sprite.name, b.sprite.name),
                SortBy.Path => string.CompareOrdinal(a.sprite.path, b.sprite.path),
                SortBy.AtlasCount => a.hits.Count.CompareTo(b.hits.Count),
                SortBy.WastedSize => a.totalWastedSize.CompareTo(b.totalWastedSize),
                _ => 0
            };
            return sortAscending ? result : -result;
        });
        Repaint();
    }

    private void DrawResultList()
    {
        resultScroll = EditorGUILayout.BeginScrollView(resultScroll);
        var filtered = FilteredDuplicates().ToList();
        
        foreach (var d in filtered)
        {
            DrawDuplicateRecord(d);
        }
        
        if (filtered.Count == 0 && !string.IsNullOrEmpty(search))
        {
            EditorGUILayout.HelpBox("검색 결과가 없습니다.", MessageType.Info);
        }
        
        EditorGUILayout.EndScrollView();
    }

    private void DrawDuplicateRecord(DuplicateRecord d)
    {
        using (new EditorGUILayout.VerticalScope("box"))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                string headerText = d.sprite.name;
                if (d.totalWastedSize > 0)
                    headerText += $" (낭비: {FormatBytes(d.totalWastedSize)})";
                    
                d.foldout = EditorGUILayout.Foldout(d.foldout, headerText, true);
                
                // 아틀라스 개수 표시
                GUILayout.Label($"{d.hits.Count}개 Atlas", EditorStyles.miniLabel, GUILayout.Width(80));
                
                GUILayout.FlexibleSpace();

                DrawRecordButtons(d);
            }

            EditorGUILayout.LabelField($"Path: {d.sprite.path}", EditorStyles.miniLabel);

            if (d.foldout)
            {
                EditorGUILayout.Space(2);
                DrawAtlasHits(d);
            }
        }
    }

    private void DrawRecordButtons(DuplicateRecord d)
    {
        if (GUILayout.Button("스프라이트 핑", GUILayout.Width(100)))
        {
            if (d.sprite.sprite)
            {
                EditorGUIUtility.PingObject(d.sprite.sprite);
                Selection.activeObject = d.sprite.sprite;
            }
        }
        if (GUILayout.Button("텍스처 열기", GUILayout.Width(100)))
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(d.sprite.path);
            if (tex)
            {
                EditorGUIUtility.PingObject(tex);
                Selection.activeObject = tex;
            }
        }
    }

    private void DrawAtlasHits(DuplicateRecord d)
    {
        EditorGUILayout.Space(2);
        for (int i = 0; i < d.hits.Count; i++)
        {
            var hit = d.hits[i];
            using (new EditorGUILayout.VerticalScope("helpbox"))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"Atlas: {hit.atlasPath}", EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("핑", GUILayout.Width(40)))
                    {
                        EditorGUIUtility.PingObject(hit.atlas);
                        Selection.activeObject = hit.atlas;
                    }
                }

                EditorGUILayout.LabelField($"Packables ({hit.packables.Count}):", EditorStyles.miniLabel);
                using (new EditorGUILayout.VerticalScope())
                {
                    foreach (var p in hit.packables)
                    {
                        string pPath = AssetDatabase.GetAssetPath(p);
                        string typeName = p ? p.GetType().Name : "(null)";
                        EditorGUILayout.LabelField($" - [{typeName}] {pPath}", EditorStyles.miniLabel);
                    }
                }
            }
        }
    }

    private IEnumerable<DuplicateRecord> FilteredDuplicates()
    {
        var filtered = duplicates.AsEnumerable();
        
        // 검색 필터
        if (!string.IsNullOrEmpty(search))
        {
            string s = search.Trim().ToLowerInvariant();
            filtered = filtered.Where(d =>
                d.sprite.name.ToLowerInvariant().Contains(s) ||
                d.sprite.path.ToLowerInvariant().Contains(s) ||
                d.hits.Any(h => h.atlasPath.ToLowerInvariant().Contains(s)));
        }
        
        return filtered;
    }

    private async void ScanDuplicatesAsync()
    {
        if (atlases.Count <= 1)
        {
            ShowNotification(new GUIContent("2개 이상 Atlas를 넣어주세요"));
            return;
        }

        showProgressBar = true;
        progress = 0f;
        progressText = "스캔 준비 중...";
        Repaint();

        try
        {
            await ScanDuplicates();
        }
        finally
        {
            showProgressBar = false;
            Repaint();
        }
    }

    private async System.Threading.Tasks.Task ScanDuplicates()
    {
        scanned = true;
        duplicates.Clear();
        spriteTotal = 0;
        totalWastedBytes = 0;

        progress = 0.1f;
        progressText = "아틀라스 분석 중...";
        Repaint();

        var perAtlasSprites = new Dictionary<SpriteAtlas, Dictionary<SpriteKey, Tuple<SpriteEntry, HashSet<UnityEngine.Object>>>>();

        // 각 아틀라스 분석
        for (int atlasIndex = 0; atlasIndex < atlases.Count; atlasIndex++)
        {
            var atlas = atlases[atlasIndex];
            progress = 0.1f + (0.6f * atlasIndex / atlases.Count);
            progressText = $"아틀라스 분석 중... ({atlasIndex + 1}/{atlases.Count})";
            Repaint();

            if (atlasIndex % 5 == 0) // 5개마다 프레임 대기
                await System.Threading.Tasks.Task.Yield();

            var map = new Dictionary<SpriteKey, Tuple<SpriteEntry, HashSet<UnityEngine.Object>>>();
            perAtlasSprites[atlas] = map;

            var packables = SpriteAtlasExtensions.GetPackables(atlas) ?? Array.Empty<UnityEngine.Object>();
            foreach (var p in packables)
            {
                foreach (var se in EnumerateSpritesFromPackable(p))
                {
                    if (!map.TryGetValue(se.key, out var tuple))
                    {
                        tuple = new Tuple<SpriteEntry, HashSet<UnityEngine.Object>>(se, new HashSet<UnityEngine.Object>());
                        map[se.key] = tuple;
                    }
                    var hs = new HashSet<UnityEngine.Object>(tuple.Item2) { p };
                    map[se.key] = new Tuple<SpriteEntry, HashSet<UnityEngine.Object>>(tuple.Item1, hs);
                }
            }
        }

        progress = 0.7f;
        progressText = "중복 검사 중...";
        Repaint();

        // 전역 집계
        var global = new Dictionary<SpriteKey, Tuple<SpriteEntry, List<AtlasHit>>>();

        foreach (var kv in perAtlasSprites)
        {
            var atlas = kv.Key;
            string atlasPath = AssetDatabase.GetAssetPath(atlas);
            
            foreach (var kv2 in kv.Value)
            {
                var key = kv2.Key;
                var entry = kv2.Value.Item1;
                var packs = kv2.Value.Item2;

                if (!global.TryGetValue(key, out var g))
                    g = new Tuple<SpriteEntry, List<AtlasHit>>(entry, new List<AtlasHit>());

                g.Item2.Add(new AtlasHit 
                { 
                    atlas = atlas, 
                    packables = new HashSet<UnityEngine.Object>(packs),
                    atlasPath = atlasPath
                });
                global[key] = g;
            }
        }

        progress = 0.9f;
        progressText = "결과 정리 중...";
        Repaint();

        // 결과 생성
        foreach (var g in global.Values)
        {
            spriteTotal++;
            if (g.Item2.Count > 1)
            {
                var record = new DuplicateRecord
                {
                    sprite = g.Item1,
                    hits = g.Item2,
                    foldout = false
                };
                
                // 낭비 용량 계산 (중복된 횟수 - 1) * 파일 크기
                record.totalWastedSize = g.Item1.fileSize * (g.Item2.Count - 1);
                totalWastedBytes += record.totalWastedSize;
                
                duplicates.Add(record);
            }
        }



        SortDuplicates();

        ShowNotification(new GUIContent(duplicates.Count == 0 ? "중복 없음" : $"중복 {duplicates.Count}개 발견"));
    }

    private IEnumerable<SpriteEntry> EnumerateSpritesFromPackable(UnityEngine.Object packable)
    {
        var list = new List<SpriteEntry>();
        if (!packable) return list;

        string pPath = AssetDatabase.GetAssetPath(packable);

        void AddSpritesFromTexturePath(string texPath)
        {
            var all = AssetDatabase.LoadAllAssetsAtPath(texPath);
            foreach (var a in all)
            {
                if (a is Sprite s && TryMakeKey(s, out var key))
                {
                    var entry = new SpriteEntry 
                    { 
                        sprite = s, 
                        path = texPath, 
                        name = s.name, 
                        key = key 
                    };
                    
                    // 파일 크기 계산
                    var tex = s.texture;
                    if (tex != null)
                    {
                        var texPath2 = AssetDatabase.GetAssetPath(tex);
                        var fileInfo = new FileInfo(texPath2);
                        if (fileInfo.Exists)
                            entry.fileSize = fileInfo.Length;
                    }
                    
                    list.Add(entry);
                }
            }
        }

        if (packable is Sprite sp)
        {
            if (TryMakeKey(sp, out var key))
            {
                var entry = new SpriteEntry { sprite = sp, path = pPath, name = sp.name, key = key };
                
                // 파일 크기 계산
                var tex = sp.texture;
                if (tex != null)
                {
                    var texPath = AssetDatabase.GetAssetPath(tex);
                    var fileInfo = new FileInfo(texPath);
                    if (fileInfo.Exists)
                        entry.fileSize = fileInfo.Length;
                }
                
                list.Add(entry);
            }
        }
        else if (packable is Texture2D)
        {
            AddSpritesFromTexturePath(pPath);
        }
        else
        {
            if (!string.IsNullOrEmpty(pPath) && AssetDatabase.IsValidFolder(pPath))
            {
                var texGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { pPath });
                foreach (var g in texGuids)
                    AddSpritesFromTexturePath(AssetDatabase.GUIDToAssetPath(g));
            }
            else if (!string.IsNullOrEmpty(pPath))
            {
                AddSpritesFromTexturePath(pPath);
            }
        }

        return list;
    }

    private bool TryMakeKey(Sprite s, out SpriteKey key)
    {
        key = null;
        if (!s) return false;
        if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(s, out string guid, out long localId))
        {
            key = new SpriteKey { guid = guid, localId = localId };
            return true;
        }
        return false;
    }

    private void ExportCsv()
    {
        if (duplicates.Count == 0)
        {
            EditorUtility.DisplayDialog("CSV 내보내기", "중복 데이터가 없습니다.", "확인");
            return;
        }

        string path = EditorUtility.SaveFilePanel("CSV 내보내기", Application.dataPath, "SpriteAtlasDuplicates.csv", "csv");
        if (string.IsNullOrEmpty(path)) return;

        try
        {
            using (var sw = new StreamWriter(path))
            {
                sw.WriteLine("SpriteName,SpritePath,FileSize,AtlasCount,WastedSize,AtlasPath,PackableType,PackablePath");
                foreach (var d in duplicates)
                {
                    foreach (var h in d.hits)
                    {
                        foreach (var p in h.packables)
                        {
                            string typeName = p ? p.GetType().Name : "(null)";
                            string pPath = AssetDatabase.GetAssetPath(p);
                            sw.WriteLine($"\"{Escape(d.sprite.name)}\",\"{Escape(d.sprite.path)}\",{d.sprite.fileSize},{d.hits.Count},{d.totalWastedSize},\"{Escape(h.atlasPath)}\",\"{Escape(typeName)}\",\"{Escape(pPath)}\"");
                        }
                    }
                }
            }
            EditorUtility.RevealInFinder(path);
            ShowNotification(new GUIContent("CSV 내보내기 완료"));
        }
        catch (Exception ex)
        {
            Debug.LogError($"CSV 내보내기 실패: {ex}");
            EditorUtility.DisplayDialog("오류", "CSV 저장 중 오류가 발생했습니다. 콘솔을 확인하세요.", "확인");
        }
    }

    private void ExportDetailedReport()
    {
        if (duplicates.Count == 0)
        {
            EditorUtility.DisplayDialog("보고서 내보내기", "중복 데이터가 없습니다.", "확인");
            return;
        }

        string path = EditorUtility.SaveFilePanel("상세 보고서 내보내기", Application.dataPath, "SpriteAtlasDuplicateReport.txt", "txt");
        if (string.IsNullOrEmpty(path)) return;

        try
        {
            using (var sw = new StreamWriter(path))
            {
                sw.WriteLine("=== SpriteAtlas 중복 분석 보고서 ===");
                sw.WriteLine($"생성일시: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sw.WriteLine($"분석 대상 Atlas: {atlases.Count}개");
                sw.WriteLine($"총 스프라이트: {spriteTotal}개");
                sw.WriteLine($"중복 스프라이트: {duplicates.Count}개");
                sw.WriteLine($"총 낭비 용량: {FormatBytes(totalWastedBytes)}");
                sw.WriteLine();

                sw.WriteLine("=== 분석 대상 Atlas 목록 ===");
                foreach (var atlas in atlases)
                {
                    sw.WriteLine($"- {AssetDatabase.GetAssetPath(atlas)}");
                }
                sw.WriteLine();

                sw.WriteLine("=== 중복 스프라이트 상세 ===");
                foreach (var d in duplicates.OrderByDescending(x => x.totalWastedSize))
                {
                    sw.WriteLine($"스프라이트: {d.sprite.name}");
                    sw.WriteLine($"  경로: {d.sprite.path}");
                    sw.WriteLine($"  파일 크기: {FormatBytes(d.sprite.fileSize)}");
                    sw.WriteLine($"  중복된 Atlas: {d.hits.Count}개");
                    sw.WriteLine($"  낭비 용량: {FormatBytes(d.totalWastedSize)}");
                    sw.WriteLine($"  포함된 Atlas:");
                    foreach (var hit in d.hits)
                    {
                        sw.WriteLine($"    - {hit.atlasPath}");
                    }
                    sw.WriteLine();
                }
            }
            EditorUtility.RevealInFinder(path);
            ShowNotification(new GUIContent("보고서 생성 완료"));
        }
        catch (Exception ex)
        {
            Debug.LogError($"보고서 내보내기 실패: {ex}");
            EditorUtility.DisplayDialog("오류", "보고서 저장 중 오류가 발생했습니다.", "확인");
        }
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes == 0) return "0 B";
        
        string[] suffixes = { "B", "KB", "MB", "GB" };
        double size = bytes;
        int suffixIndex = 0;
        
        while (size >= 1024 && suffixIndex < suffixes.Length - 1)
        {
            size /= 1024;
            suffixIndex++;
        }
        
        return $"{size:F1} {suffixes[suffixIndex]}";
    }

    private static string Escape(string s) => (s ?? "").Replace("\"", "\"\"");
}