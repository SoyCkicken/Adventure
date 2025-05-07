//using System;
//using System.Data;
//using System.IO;
//using System.Collections.Generic;
//using UnityEditor;
//using UnityEngine;
//using ExcelDataReader;
//using Newtonsoft.Json;

//public class ExcelToJsonConverter : EditorWindow
//{
//    private List<UnityEngine.Object> excelFiles = new List<UnityEngine.Object>();
//    private const string TARGET_FOLDER = "Assets/ExcelFiles"; // ✅ 자동 인식할 폴더

//    [MenuItem("Tools/Excel → JSON (Auto Folder Scan)")]
//    public static void ShowWindow()
//    {
//        GetWindow<ExcelToJsonConverter>("Excel To JSON (Auto Scan)");
//    }

//    private void OnEnable()
//    {
//        // 기존 EditorPrefs 저장된 경로 로드
//        if (EditorPrefs.HasKey("ExcelToJson_FileList"))
//        {
//            string savedPaths = EditorPrefs.GetString("ExcelToJson_FileList");
//            string[] paths = savedPaths.Split(';');

//            excelFiles.Clear();

//            foreach (string path in paths)
//            {
//                if (!string.IsNullOrEmpty(path))
//                {
//                    var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
//                    if (obj != null && !excelFiles.Contains(obj))
//                        excelFiles.Add(obj);
//                }
//            }
//        }

//        // ✅ 폴더 자동 스캔
//        AutoScanExcelFiles();
//    }

//    private void AutoScanExcelFiles()
//    {
//        if (!AssetDatabase.IsValidFolder(TARGET_FOLDER))
//        {
//            Debug.LogWarning($"폴더 {TARGET_FOLDER} 이 존재하지 않습니다.");
//            return;
//        }

//        string[] guids = AssetDatabase.FindAssets("", new[] { TARGET_FOLDER });

//        foreach (string guid in guids)
//        {
//            string path = AssetDatabase.GUIDToAssetPath(guid);
//            if (path.EndsWith(".xls") || path.EndsWith(".xlsx"))
//            {
//                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
//                if (obj != null && !excelFiles.Contains(obj))
//                {
//                    excelFiles.Add(obj);
//                }
//            }
//        }
//    }

//    private void OnDisable()
//    {
//        // EditorPrefs에 경로 저장
//        List<string> paths = new List<string>();
//        foreach (var obj in excelFiles)
//        {
//            if (obj != null)
//            {
//                string path = AssetDatabase.GetAssetPath(obj);
//                if (!string.IsNullOrEmpty(path))
//                    paths.Add(path);
//            }
//        }
//        EditorPrefs.SetString("ExcelToJson_FileList", string.Join(";", paths));
//    }

//    private void OnGUI()
//    {
//        GUILayout.Label($"엑셀 파일 목록 (자동 스캔 폴더: {TARGET_FOLDER})", EditorStyles.boldLabel);

//        for (int i = 0; i < excelFiles.Count; i++)
//        {
//            EditorGUILayout.BeginHorizontal();
//            excelFiles[i] = EditorGUILayout.ObjectField($"Excel 파일 {i + 1}", excelFiles[i], typeof(UnityEngine.Object), false);

//            if (GUILayout.Button("X", GUILayout.Width(20)))
//            {
//                excelFiles.RemoveAt(i);
//                i--;
//            }
//            EditorGUILayout.EndHorizontal();
//        }

//        EditorGUILayout.Space();

//        if (GUILayout.Button("Add Excel File Slot"))
//        {
//            excelFiles.Add(null);
//        }

//        if (GUILayout.Button("Rescan Folder"))
//        {
//            AutoScanExcelFiles();
//        }

//        EditorGUILayout.Space();

//        if (GUILayout.Button("Convert All to JSON"))
//        {
//            foreach (var excelFile in excelFiles)
//            {
//                if (excelFile == null)
//                {
//                    Debug.LogWarning("빈 슬롯은 무시합니다.");
//                    continue;
//                }

//                string assetPath = AssetDatabase.GetAssetPath(excelFile);
//                string fullPath = Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length), assetPath);

//                if (assetPath.EndsWith(".xls") || assetPath.EndsWith(".xlsx"))
//                {
//                    ConvertExcelToJson(fullPath);
//                }
//                else
//                {
//                    Debug.LogError($"파일 {assetPath} 은(는) .xls/.xlsx 파일이 아닙니다.");
//                }
//            }
//        }
//    }

//    private static void ConvertExcelToJson(string path)
//    {
//        try
//        {
//            using var stream = File.Open(path, FileMode.Open, FileAccess.Read);
//            using var reader = ExcelReaderFactory.CreateReader(stream);

//            var conf = new ExcelDataSetConfiguration
//            {
//                ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
//            };
//            DataSet dataSet = reader.AsDataSet(conf);

//            var workbook = new Dictionary<string, List<Dictionary<string, object>>>();

//            foreach (DataTable table in dataSet.Tables)
//            {
//                var rows = new List<Dictionary<string, object>>();
//                foreach (DataRow dr in table.Rows)
//                {
//                    var dict = new Dictionary<string, object>();
//                    foreach (DataColumn col in table.Columns)
//                        dict[col.ColumnName] = dr[col];
//                    rows.Add(dict);
//                }
//                workbook[table.TableName] = rows;
//            }

//            string json = JsonConvert.SerializeObject(workbook, Formatting.Indented);

//            string folder = "Assets/Resources/ExcelJsons";
//            if (!Directory.Exists(folder))
//                Directory.CreateDirectory(folder);

//            string fileName = Path.GetFileNameWithoutExtension(path) + ".json";
//            string savePath = Path.Combine(folder, fileName);
//            File.WriteAllText(savePath, json);

//            AssetDatabase.Refresh();
//            Debug.Log($"[ExcelToJson] {savePath} 로 변환 완료");
//        }
//        catch (Exception ex)
//        {
//            Debug.LogError($"엑셀 변환 중 에러 발생: {ex.Message}");
//        }
//    }
//}

// Assets/Editor/ExcelToJsonConverter.cs
//using System;
//using System.Data;
//using System.IO;
//using System.Linq;
//using System.Collections.Generic;
//using System.Text;
//using UnityEditor;
//using UnityEngine;
//using ExcelDataReader;
//using Newtonsoft.Json;

//public class ExcelToJsonConverter : EditorWindow
//{
//    private List<UnityEngine.Object> excelFiles = new List<UnityEngine.Object>();
//    private const string TARGET_FOLDER = "Assets/ExcelFiles";
//    private const string JSON_OUTPUT_FOLDER = "Assets/Resources/ExcelJsons";
//    private const string CS_OUTPUT_FOLDER = "Assets/Scripts/Configs";

//    [MenuItem("Tools/Excel → JSON (Auto Folder Scan)")]
//    public static void ShowWindow()
//    {
//        GetWindow<ExcelToJsonConverter>("Excel To JSON (Auto Scan)");
//    }

//    private void OnEnable()
//    {
//        excelFiles.Clear();
//        AutoScanExcelFiles();
//    }

//    private void AutoScanExcelFiles()
//    {
//        if (!AssetDatabase.IsValidFolder(TARGET_FOLDER))
//        {
//            Debug.LogWarning($"[ExcelToJson] 폴더 {TARGET_FOLDER} 이 존재하지 않습니다.");
//            return;
//        }

//        var guids = AssetDatabase.FindAssets("", new[] { TARGET_FOLDER });
//        foreach (var guid in guids)
//        {
//            var path = AssetDatabase.GUIDToAssetPath(guid);
//            if (path.EndsWith(".xls", StringComparison.OrdinalIgnoreCase) ||
//                path.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
//            {
//                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
//                if (obj != null && !excelFiles.Contains(obj))
//                    excelFiles.Add(obj);
//            }
//        }
//    }

//    private void OnGUI()
//    {
//        GUILayout.Label($"엑셀 파일 목록 (자동 스캔 폴더: {TARGET_FOLDER})", EditorStyles.boldLabel);

//        for (int i = 0; i < excelFiles.Count; i++)
//        {
//            EditorGUILayout.BeginHorizontal();
//            excelFiles[i] = EditorGUILayout.ObjectField(
//                $"Excel 파일 {i + 1}",
//                excelFiles[i],
//                typeof(UnityEngine.Object),
//                false
//            );
//            if (GUILayout.Button("X", GUILayout.Width(20)))
//            {
//                excelFiles.RemoveAt(i);
//                i--;
//            }
//            EditorGUILayout.EndHorizontal();
//        }

//        EditorGUILayout.Space();
//        if (GUILayout.Button("Rescan Folder"))
//            AutoScanExcelFiles();

//        EditorGUILayout.Space();
//        if (GUILayout.Button("Convert All to JSON + C# Classes"))
//        {
//            foreach (var excelAsset in excelFiles)
//            {
//                if (excelAsset == null) continue;
//                var assetPath = AssetDatabase.GetAssetPath(excelAsset);
//                var projectRoot = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);
//                var fullPath = Path.Combine(projectRoot, assetPath);

//                if (assetPath.EndsWith(".xls", StringComparison.OrdinalIgnoreCase) ||
//                    assetPath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
//                {
//                    ConvertExcelToJson(fullPath);
//                }
//                else
//                {
//                    Debug.LogError($"[ExcelToJson] 잘못된 파일 형식: {assetPath}");
//                }
//            }
//            AssetDatabase.Refresh();
//            Debug.Log("[ExcelToJson] 모든 변환 및 클래스 생성 완료.");
//        }
//    }

//    private static void ConvertExcelToJson(string fullExcelPath)
//    {
//        try
//        {
//            // 1) 엑셀 읽기
//            using var stream = File.Open(fullExcelPath, FileMode.Open, FileAccess.Read);
//            using var reader = ExcelReaderFactory.CreateReader(stream);
//            var conf = new ExcelDataSetConfiguration
//            {
//                ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
//            };
//            var dataSet = reader.AsDataSet(conf);

//            // 2) 시트별 JSON 변환 & C# 클래스 생성
//            var workbook = new Dictionary<string, object>();
//            var fileBaseName = Path.GetFileNameWithoutExtension(fullExcelPath);

//            foreach (DataTable table in dataSet.Tables)
//            {
//                // --- JSON 변환용 리스트로 만들기
//                var rows = new List<Dictionary<string, object>>();
//                foreach (DataRow dr in table.Rows)
//                {
//                    var dict = new Dictionary<string, object>();
//                    foreach (DataColumn col in table.Columns)
//                        dict[col.ColumnName] = dr[col];
//                    rows.Add(dict);
//                }
//                workbook[table.TableName] = rows;

//                // --- C# 클래스 자동 생성
//                var className = $"{ToPascalCase(fileBaseName)}{ToPascalCase(table.TableName)}Data";
//                var columnNames = table.Columns
//                                       .Cast<DataColumn>()
//                                       .Select(c => c.ColumnName);
//                GenerateCsClass(className, columnNames);
//            }

//            // 3) 전체 워크북을 하나의 JSON으로 저장
//            var json = JsonConvert.SerializeObject(workbook, Formatting.Indented);
//            Directory.CreateDirectory(JSON_OUTPUT_FOLDER);
//            var jsonPath = Path.Combine(JSON_OUTPUT_FOLDER, fileBaseName + ".json");
//            File.WriteAllText(jsonPath, json, Encoding.UTF8);
//            Debug.Log($"[ExcelToJson] {jsonPath} 변환 완료");
//        }
//        catch (Exception ex)
//        {
//            Debug.LogError($"[ExcelToJson] 변환 실패: {ex.Message}");
//        }
//    }

//    /// <summary>
//    /// 각 시트에 대응하는 C# 클래스를 생성합니다.
//    /// </summary>
//    private static void GenerateCsClass(string className, IEnumerable<string> fields)
//    {
//        var sb = new StringBuilder();
//        sb.AppendLine("using System;");
//        sb.AppendLine();
//        sb.AppendLine("namespace Game.Configs");
//        sb.AppendLine("{");
//        sb.AppendLine("    [Serializable]");
//        sb.AppendLine($"    public class {className}");
//        sb.AppendLine("    {");
//        foreach (var raw in fields)
//        {
//            var name = ToPascalCase(raw);
//            sb.AppendLine($"        public string {name};");
//        }
//        sb.AppendLine("    }");
//        sb.AppendLine("}");

//        Directory.CreateDirectory(CS_OUTPUT_FOLDER);
//        var csPath = Path.Combine(CS_OUTPUT_FOLDER, className + ".cs");
//        File.WriteAllText(csPath, sb.ToString(), Encoding.UTF8);
//        Debug.Log($"[ExcelToJson] 생성된 C# 클래스: {csPath}");
//    }

//    /// <summary>
//    /// snake_case, kebab-case, 공백 등을 PascalCase로 변환
//    /// </summary>
//    private static string ToPascalCase(string s)
//    {
//        var parts = s
//            .Replace("-", "_")
//            .Split(new[] { '_', ' ' }, StringSplitOptions.RemoveEmptyEntries);
//        return string.Concat(parts.Select(p =>
//            char.ToUpperInvariant(p[0]) + p.Substring(1)));
//    }
//}

// Assets/Editor/ExcelToJsonConverter.cs
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using ExcelDataReader;
using Newtonsoft.Json;

public class ExcelToJsonConverter : EditorWindow
{
    private List<UnityEngine.Object> excelFiles = new();
    private const string TARGET_FOLDER = "Assets/ExcelFiles";
    private const string JSON_OUTPUT_FOLDER = "Assets/Resources/ExcelJsons";
    private const string CS_OUTPUT_FOLDER = "Assets/Scripts/Configs";

    private static string ProjectRoot =>
        Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);

    [MenuItem("Tools/Excel → JSON (Auto Scan)")]
    public static void ShowWindow() => GetWindow<ExcelToJsonConverter>("Excel → JSON");

    private void OnEnable()
    {
        excelFiles.Clear();
        AutoScanExcelFiles();
    }

    private void AutoScanExcelFiles()
    {
        if (!AssetDatabase.IsValidFolder(TARGET_FOLDER))
        {
            Debug.LogWarning($"[ExcelToJson] 폴더 {TARGET_FOLDER} 이 존재하지 않습니다.");
            return;
        }

        var guids = AssetDatabase.FindAssets("", new[] { TARGET_FOLDER });
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.EndsWith(".xls", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                if (obj != null && !excelFiles.Contains(obj))
                    excelFiles.Add(obj);
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.Label($"엑셀 ({TARGET_FOLDER}) → JSON + C# 자동 변환", EditorStyles.boldLabel);

        if (GUILayout.Button("Rescan Folder"))
            Debug.Log("다시 스캔했습니다");
            AutoScanExcelFiles();

        GUILayout.Space(10);
        if (GUILayout.Button("Convert All to JSON + C# Classes"))
        {
            foreach (var asset in excelFiles)
            {
                if (asset == null) continue;
                var assetPath = AssetDatabase.GetAssetPath(asset);
                var fullPath = Path.Combine(ProjectRoot, assetPath);
                ConvertExcelToJson(fullPath);
            }
            AssetDatabase.Refresh();
            Debug.Log("[ExcelToJson] 모든 변환 완료.");
        }
    }

    private static void ConvertExcelToJson(string fullExcelPath)
    {
        try
        {
            using var stream = File.Open(fullExcelPath, FileMode.Open, FileAccess.Read);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var conf = new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
            };
            var dataSet = reader.AsDataSet(conf);

            var workbookName = Path.GetFileNameWithoutExtension(fullExcelPath);
            // 절대 경로 폴더 준비
            var projectRoot = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);
            var jsonFolder = Path.Combine(projectRoot, JSON_OUTPUT_FOLDER);
            Directory.CreateDirectory(jsonFolder);

            foreach (DataTable table in dataSet.Tables)
            {
                // 1) 시트별 데이터 리스트로 변환
                var rows = new List<Dictionary<string, object>>();
                foreach (DataRow dr in table.Rows)
                {
                    var dict = new Dictionary<string, object>();
                    foreach (DataColumn col in table.Columns)
                        dict[col.ColumnName] = dr[col];
                    rows.Add(dict);
                }

                // 2) 시트별 JSON 파일 쓰기
                var sheetNameSafe = ToPascalCase(table.TableName);
                var fileName = $"{workbookName}_{sheetNameSafe}.json";
                var filePath = Path.Combine(jsonFolder, fileName);
                var jsonText = JsonConvert.SerializeObject(rows, Formatting.Indented);
                File.WriteAllText(filePath, jsonText, Encoding.UTF8);
                Debug.Log($"[ExcelToJson] 시트'{table.TableName}' → {filePath}");

                // 3) 시트별 C# 클래스도 생성
                var className = $"{ToPascalCase(workbookName)}{sheetNameSafe}Data";
                var columnNames = table.Columns.Cast<DataColumn>().Select(c => c.ColumnName);
                GenerateCsClass(className, columnNames);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ExcelToJson] 변환 실패: {ex.Message}");
        }
    }

    private static void GenerateCsClass(string className, IEnumerable<string> fields)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine();
        sb.AppendLine("namespace Game.Configs");
        sb.AppendLine("{");
        sb.AppendLine("    [Serializable]");
        sb.AppendLine($"    public class {className}");
        sb.AppendLine("    {");

        foreach (var raw in fields)
        {
            var name = ToPascalCase(raw);
            sb.AppendLine($"        public string {name};");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        var csFolder = Path.Combine(ProjectRoot, CS_OUTPUT_FOLDER);
        Directory.CreateDirectory(csFolder);
        var csPath = Path.Combine(csFolder, className + ".cs");
        File.WriteAllText(csPath, sb.ToString(), Encoding.UTF8);
        Debug.Log($"[ExcelToJson] C# Class → {csPath}");
    }

    private static string ToPascalCase(string s)
    {
        var parts = s
            .Replace("-", "_")
            .Split(new[] { '_', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        return string.Concat(parts.Select(p =>
            char.ToUpperInvariant(p[0]) + p.Substring(1)));
    }
}

