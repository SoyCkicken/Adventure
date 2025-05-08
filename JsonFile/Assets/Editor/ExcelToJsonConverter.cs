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
//    private List<UnityEngine.Object> excelFiles = new();
//    private const string TARGET_FOLDER = "Assets/ExcelFiles";
//    private const string JSON_OUTPUT_FOLDER = "Assets/Resources/Events";
//    private const string CS_OUTPUT_FOLDER = "Assets/Json";

//    private static string ProjectRoot =>
//        Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);

//    [MenuItem("Tools/Excel → JSON (Auto Scan)")]
//    public static void ShowWindow() => GetWindow<ExcelToJsonConverter>("Excel → JSON");

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
//        GUILayout.Label($"엑셀 ({TARGET_FOLDER}) → JSON + C# 자동 변환", EditorStyles.boldLabel);

//        if (GUILayout.Button("Rescan Folder"))
//            Debug.Log("다시 스캔했습니다");
//            AutoScanExcelFiles();

//        GUILayout.Space(10);
//        if (GUILayout.Button("Convert All to JSON + C# Classes"))
//        {
//            foreach (var asset in excelFiles)
//            {
//                if (asset == null) continue;
//                var assetPath = AssetDatabase.GetAssetPath(asset);
//                var fullPath = Path.Combine(ProjectRoot, assetPath);
//                ConvertExcelToJson(fullPath);
//            }
//            AssetDatabase.Refresh();
//            Debug.Log("[ExcelToJson] 모든 변환 완료.");
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

//            // 각 시트마다 JSON 파일 별도로 생성
//            foreach (DataTable table in dataSet.Tables)
//            {
//                var rows = new List<Dictionary<string, object>>();
//                foreach (DataRow dr in table.Rows)
//                {
//                    var dict = new Dictionary<string, object>();
//                    foreach (DataColumn col in table.Columns)
//                    {
//                        dict[col.ColumnName] = dr[col];
//                    }
//                    rows.Add(dict);
//                }

//                var workbook = new Dictionary<string, List<Dictionary<string, object>>>
//                {
//                    [table.TableName] = rows
//                };

//                string json = JsonConvert.SerializeObject(workbook, Formatting.Indented);

//                string folder = "Assets/Resources/ExcelJsons";
//                if (!Directory.Exists(folder))
//                    Directory.CreateDirectory(folder);

//                // ✅ 파일명 = 시트명 기준
//                string fileName = table.TableName + ".json";
//                string savePath = Path.Combine(folder, fileName);
//                File.WriteAllText(savePath, json);

//                Debug.Log($"[ExcelToJson] {savePath} 로 시트 {table.TableName} 변환 완료");
//            }

//            AssetDatabase.Refresh();
//        }
//        catch (Exception ex)
//        {
//            Debug.LogError($"엑셀 변환 중 에러 발생: {ex.Message}");
//        }
//    }

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

//        var csFolder = Path.Combine(ProjectRoot, CS_OUTPUT_FOLDER);
//        Directory.CreateDirectory(csFolder);
//        var csPath = Path.Combine(csFolder, className + ".cs");
//        File.WriteAllText(csPath, sb.ToString(), Encoding.UTF8);
//        Debug.Log($"[ExcelToJson] C# Class → {csPath}");
//    }

//    private static string ToPascalCase(string s)
//    {
//        var parts = s
//            .Replace("-", "_")
//            .Split(new[] { '_', ' ' }, StringSplitOptions.RemoveEmptyEntries);
//        return string.Concat(parts.Select(p =>
//            char.ToUpperInvariant(p[0]) + p.Substring(1)));
//    }
//}


//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.IO;
//using UnityEditor;
//using UnityEngine;
//using ExcelDataReader;
//using Newtonsoft.Json;

//public class ExcelToJsonAndClassGenerator : EditorWindow
//{
//    private UnityEngine.Object excelFile;

//    [MenuItem("Tools/Excel → JSON + Class")]
//    public static void ShowWindow()
//    {
//        GetWindow<ExcelToJsonAndClassGenerator>("Excel → JSON + Class Generator");
//    }

//    private void OnGUI()
//    {
//        GUILayout.Label("엑셀 파일 드래그 & 드롭 (Project 창의 Asset)", EditorStyles.boldLabel);

//        excelFile = EditorGUILayout.ObjectField("Excel 파일", excelFile, typeof(UnityEngine.Object), false);

//        if (excelFile != null)
//        {
//            string assetPath = AssetDatabase.GetAssetPath(excelFile);
//            string fullPath = Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length), assetPath);

//            EditorGUILayout.LabelField("선택된 파일:", assetPath);

//            if (GUILayout.Button("Convert"))
//            {
//                if (assetPath.EndsWith(".xls") || assetPath.EndsWith(".xlsx"))
//                {
//                    ConvertExcel(fullPath);
//                }
//                else
//                {
//                    Debug.LogError("선택된 파일이 .xls 또는 .xlsx 형식이 아닙니다.");
//                }
//            }
//        }
//        else
//        {
//            EditorGUILayout.HelpBox("엑셀 파일(.xls 또는 .xlsx)을 Project 창에서 여기에 드래그하세요.", MessageType.Info);
//        }
//    }

//    private static void ConvertExcel(string path)
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

//            foreach (DataTable table in dataSet.Tables)
//            {
//                CreateJson(table);
//                CreateCSharpClass(table);
//            }

//            AssetDatabase.Refresh();
//        }
//        catch (Exception ex)
//        {
//            Debug.LogError($"엑셀 변환 중 에러 발생: {ex.Message}");
//        }
//    }

//    private static void CreateJson(DataTable table)
//    {
//        var rows = new List<Dictionary<string, object>>();
//        foreach (DataRow dr in table.Rows)
//        {
//            var dict = new Dictionary<string, object>();
//            foreach (DataColumn col in table.Columns)
//            {
//                dict[col.ColumnName] = dr[col];
//            }
//            rows.Add(dict);
//        }

//        var workbook = new Dictionary<string, List<Dictionary<string, object>>>
//        {
//            [table.TableName] = rows
//        };

//        string json = JsonConvert.SerializeObject(workbook, Formatting.Indented);

//        string jsonFolder = "Assets/Resources/ExcelJsons";
//        if (!Directory.Exists(jsonFolder))
//            Directory.CreateDirectory(jsonFolder);

//        string jsonFileName = table.TableName + ".json";
//        string jsonSavePath = Path.Combine(jsonFolder, jsonFileName);
//        File.WriteAllText(jsonSavePath, json);

//        Debug.Log($"[ExcelToJson] {jsonSavePath} 로 시트 {table.TableName} 변환 완료");
//    }

//    private static void CreateCSharpClass(DataTable table)
//    {
//        string className = table.TableName;
//        string scriptFolder = "Assets/Scripts/Generated";
//        if (!Directory.Exists(scriptFolder))
//            Directory.CreateDirectory(scriptFolder);

//        string scriptFilePath = Path.Combine(scriptFolder, className + ".cs");

//        using StreamWriter writer = new StreamWriter(scriptFilePath, false);
//        writer.WriteLine("// 자동 생성된 클래스 (엑셀 시트명 기반)");
//        writer.WriteLine("using System;");
//        writer.WriteLine("");
//        writer.WriteLine("[System.Serializable]");
//        writer.WriteLine($"public class {className}");
//        writer.WriteLine("{");

//        foreach (DataColumn col in table.Columns)
//        {
//            string typeStr = InferType(col.ColumnName);

//            string fieldName = SanitizeVariableName(col.ColumnName);

//            writer.WriteLine($"    public {typeStr} {fieldName};");
//        }

//        writer.WriteLine("}");

//        Debug.Log($"[ExcelToClass] {scriptFilePath} 클래스 파일 생성 완료");
//    }

//    private static string InferType(string columnName)
//    {
//        string lower = columnName.ToLower();
//        if (lower.Contains("index") || lower.Contains("id"))
//            return "int";
//        else if (lower.Contains("is") || lower.Contains("flag"))
//            return "bool";
//        else
//            return "string"; // 기본값
//    }

//    private static string SanitizeVariableName(string name)
//    {
//        // 공백, 특수문자 제거 → 변수명으로 안전하게 변환
//        string sanitized = name.Replace(" ", "_").Replace("-", "_");
//        sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"[^a-zA-Z0-9_]", "");
//        return sanitized;
//    }
//}

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEditor;
using UnityEngine;
using ExcelDataReader;
using Newtonsoft.Json;

public class ExcelAutoGenerator : EditorWindow
{
    // ✅ 변수: 경로 설정
    private string excelFolderPath = "Assets/ExcelFiles";
    private string jsonOutputFolder = "Assets/Resources/Events2";
    private string classOutputFolder = "Assets/Json2";

    private List<string> excelFilePaths = new List<string>();

    [MenuItem("Tools/Excel Auto Generator")]
    public static void ShowWindow()
    {
        GetWindow<ExcelAutoGenerator>("Excel Auto Generator");
    }

    private void OnEnable()
    {
        ScanExcelFiles();
    }

    private void OnGUI()
    {
        GUILayout.Label("📁 폴더 경로 설정", EditorStyles.boldLabel);

        excelFolderPath = EditorGUILayout.TextField("엑셀 파일 폴더", excelFolderPath);
        jsonOutputFolder = EditorGUILayout.TextField("JSON 저장 폴더", jsonOutputFolder);
        classOutputFolder = EditorGUILayout.TextField("C# 클래스 저장 폴더", classOutputFolder);

        EditorGUILayout.Space();

        if (GUILayout.Button("폴더 다시 스캔"))
        {
            ScanExcelFiles();
        }

        EditorGUILayout.Space();

        GUILayout.Label($"인식된 엑셀 파일 수: {excelFilePaths.Count}", EditorStyles.label);

        foreach (var file in excelFilePaths)
        {
            EditorGUILayout.LabelField("- " + file);
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("모든 엑셀 파일 변환 (시트별 JSON + Class)"))
        {
            foreach (var file in excelFilePaths)
            {
                ConvertExcel(file);
            }
            AssetDatabase.Refresh();
        }
    }

    private void ScanExcelFiles()
    {
        excelFilePaths.Clear();

        if (Directory.Exists(excelFolderPath))
        {
            string[] files = Directory.GetFiles(excelFolderPath, "*.*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                if (file.EndsWith(".xls") || file.EndsWith(".xlsx"))
                {
                    excelFilePaths.Add(file);
                }
            }
            Debug.Log($"[ExcelAutoGenerator] {excelFilePaths.Count}개의 엑셀 파일 인식됨.");
        }
        else
        {
            Debug.LogWarning($"[ExcelAutoGenerator] 폴더 {excelFolderPath} 가 존재하지 않습니다.");
        }
    }

    private void ConvertExcel(string path)
    {
        try
        {
            using var stream = File.Open(path, FileMode.Open, FileAccess.Read);
            using var reader = ExcelReaderFactory.CreateReader(stream);

            var conf = new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
            };

            DataSet dataSet = reader.AsDataSet(conf);

            foreach (DataTable table in dataSet.Tables)
            {
                CreateJson(table);
                CreateCSharpClass(table);
            }

            Debug.Log($"[ExcelAutoGenerator] {Path.GetFileName(path)} 변환 완료.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"엑셀 변환 중 에러 발생 ({Path.GetFileName(path)}): {ex.Message}");
        }
    }

    private void CreateJson(DataTable table)
    {
        var rows = new List<Dictionary<string, object>>();
        foreach (DataRow dr in table.Rows)
        {
            var dict = new Dictionary<string, object>();
            foreach (DataColumn col in table.Columns)
            {
                dict[col.ColumnName] = dr[col];
            }
            rows.Add(dict);
        }

        var workbook = new Dictionary<string, List<Dictionary<string, object>>>
        {
            [table.TableName] = rows
        };

        if (!Directory.Exists(jsonOutputFolder))
            Directory.CreateDirectory(jsonOutputFolder);

        string jsonFileName = table.TableName + ".json";
        string jsonSavePath = Path.Combine(jsonOutputFolder, jsonFileName);

        string json = JsonConvert.SerializeObject(workbook, Formatting.Indented);
        File.WriteAllText(jsonSavePath, json);

        Debug.Log($"[ExcelAutoGenerator] {jsonSavePath} JSON 생성 완료");
    }

    private void CreateCSharpClass(DataTable table)
    {
        if (!Directory.Exists(classOutputFolder))
            Directory.CreateDirectory(classOutputFolder);

        string className = table.TableName;
        string scriptFilePath = Path.Combine(classOutputFolder, className + ".cs");

        using StreamWriter writer = new StreamWriter(scriptFilePath, false);
        writer.WriteLine("// 자동 생성된 클래스 (엑셀 시트명 기반)");
        writer.WriteLine("using System;");
        writer.WriteLine("");
        writer.WriteLine("[System.Serializable]");
        writer.WriteLine($"public class {className}");
        writer.WriteLine("{");

        foreach (DataColumn col in table.Columns)
        {
            string typeStr = InferType(col.ColumnName);
            string fieldName = SanitizeVariableName(col.ColumnName);
            writer.WriteLine($"    public {typeStr} {fieldName};");
        }

        writer.WriteLine("}");

        Debug.Log($"[ExcelAutoGenerator] {scriptFilePath} 클래스 생성 완료");
    }

    private static string InferType(string columnName)
    {
        string lower = columnName.ToLower();
        if (lower.Contains("index") || lower.Contains("id"))
            return "int";
        else if (lower.Contains("is") || lower.Contains("flag"))
            return "bool";
        else
            return "string";
    }

    private static string SanitizeVariableName(string name)
    {
        string sanitized = name.Replace(" ", "_").Replace("-", "_");
        sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"[^a-zA-Z0-9_]", "");
        return sanitized;
    }
}

