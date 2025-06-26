//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.IO;
//using UnityEditor;
//using UnityEngine;
//using ExcelDataReader;
//using Newtonsoft.Json;

//public class ExcelAutoGenerator : EditorWindow
//{
//    // ✅ 변수: 경로 설정
//    private string excelFolderPath = "Assets/ExcelFiles";
//    private string jsonOutputFolder = "Assets/Resources/Events";
//    private string classOutputFolder = "Assets/Json";

//    private List<string> excelFilePaths = new List<string>();

//    [MenuItem("Tools/Excel Auto Generator")]
//    public static void ShowWindow()
//    {
//        GetWindow<ExcelAutoGenerator>("Excel Auto Generator");
//    }

//    private void OnEnable()
//    {
//        ScanExcelFiles();
//    }

//    private void OnGUI()
//    {
//        GUILayout.Label("📁 폴더 경로 설정", EditorStyles.boldLabel);

//        excelFolderPath = EditorGUILayout.TextField("엑셀 파일 폴더", excelFolderPath);
//        jsonOutputFolder = EditorGUILayout.TextField("JSON 저장 폴더", jsonOutputFolder);
//        classOutputFolder = EditorGUILayout.TextField("C# 클래스 저장 폴더", classOutputFolder);

//        EditorGUILayout.Space();

//        if (GUILayout.Button("폴더 다시 스캔"))
//        {
//            ScanExcelFiles();
//        }

//        EditorGUILayout.Space();

//        GUILayout.Label($"인식된 엑셀 파일 수: {excelFilePaths.Count}", EditorStyles.label);

//        foreach (var file in excelFilePaths)
//        {
//            EditorGUILayout.LabelField("- " + file);
//        }

//        EditorGUILayout.Space();

//        if (GUILayout.Button("모든 엑셀 파일 변환 (시트별 JSON + Class)"))
//        {
//            foreach (var file in excelFilePaths)
//            {
//                ConvertExcel(file);
//            }
//            AssetDatabase.Refresh();
//        }
//    }

//    private void ScanExcelFiles()
//    {
//        excelFilePaths.Clear();

//        if (Directory.Exists(excelFolderPath))
//        {
//            string[] files = Directory.GetFiles(excelFolderPath, "*.*", SearchOption.AllDirectories);
//            foreach (string file in files)
//            {
//                if (file.EndsWith(".xls") || file.EndsWith(".xlsx"))
//                {
//                    excelFilePaths.Add(file);
//                }
//            }
//            Debug.Log($"[ExcelAutoGenerator] {excelFilePaths.Count}개의 엑셀 파일 인식됨.");
//        }
//        else
//        {
//            Debug.LogWarning($"[ExcelAutoGenerator] 폴더 {excelFolderPath} 가 존재하지 않습니다.");
//        }
//    }

//    private void ConvertExcel(string path)
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

//            Debug.Log($"[ExcelAutoGenerator] {Path.GetFileName(path)} 변환 완료.");
//        }
//        catch (Exception ex)
//        {
//            Debug.LogError($"엑셀 변환 중 에러 발생 ({Path.GetFileName(path)}): {ex.Message}");
//        }
//    }

//    private void CreateJson(DataTable table)
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

//        if (!Directory.Exists(jsonOutputFolder))
//            Directory.CreateDirectory(jsonOutputFolder);

//        string jsonFileName = table.TableName + ".json";
//        string jsonSavePath = Path.Combine(jsonOutputFolder, jsonFileName);

//        string json = JsonConvert.SerializeObject(workbook, Formatting.Indented);
//        File.WriteAllText(jsonSavePath, json);

//        Debug.Log($"[ExcelAutoGenerator] {jsonSavePath} JSON 생성 완료");
//    }

//    private void CreateCSharpClass(DataTable table)
//    {
//        if (!Directory.Exists(classOutputFolder))
//            Directory.CreateDirectory(classOutputFolder);

//        string className = table.TableName;
//        string scriptFilePath = Path.Combine(classOutputFolder, className + ".cs");

//        using StreamWriter writer = new StreamWriter(scriptFilePath, false);
//        writer.WriteLine("// 자동 생성된 클래스 (엑셀 시트명 기반)");
//        writer.WriteLine("using System;");
//        writer.WriteLine("");
//        writer.WriteLine("[System.Serializable]");
//        writer.WriteLine($"public class {className}");
//        writer.WriteLine("{");

//        foreach (DataColumn col in table.Columns)
//        {
//            string typeStr = InferType(table, col);
//            string fieldName = SanitizeVariableName(col.ColumnName);
//            writer.WriteLine($"    public {typeStr} {fieldName};");
//        }

//        writer.WriteLine("}");

//        Debug.Log($"[ExcelAutoGenerator] {scriptFilePath} 클래스 생성 완료");
//    }

//    private static string InferType(DataTable table, DataColumn column)
//    {
//        string colName = column.ColumnName.ToLower();

//        // 🔥 강제 string 처리할 컬럼명 (예: 이름, 설명, 텍스트)
//        string[] forceStringCols = { "name", "desc", "text", "title" };

//        foreach (var keyword in forceStringCols)
//        {
//            if (colName.Contains(keyword))
//                return "string";
//        }

//        bool isInt = true;
//        bool isFloat = true;
//        bool isBool = true;

//        foreach (DataRow row in table.Rows)
//        {
//            var value = row[column];
//            if (value == null || value == DBNull.Value)
//                continue;

//            string strVal = value.ToString();

//            if (!(strVal.Equals("true", StringComparison.OrdinalIgnoreCase) || strVal.Equals("false", StringComparison.OrdinalIgnoreCase)))
//                isBool = false;

//            if (!int.TryParse(strVal, out _))
//                isInt = false;

//            if (!float.TryParse(strVal, out _))
//                isFloat = false;

//            if (!isBool && !isInt && !isFloat)
//                break;
//        }

//        if (isBool) return "bool";
//        if (isInt) return "int";
//        if (isFloat) return "float";
//        return "string";
//    }


//    private static string SanitizeVariableName(string name)
//    {
//        string sanitized = name.Replace(" ", "_").Replace("-", "_");
//        sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"[^a-zA-Z0-9_]", "");
//        return sanitized;
//    }
//}
// 수정된 ExcelAutoGenerator.cs - 배열 지원

//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.IO;
//using UnityEditor;
//using UnityEngine;
//using ExcelDataReader;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;

//public class ExcelAutoGenerator : EditorWindow
//{
//    private string excelFolderPath = "Assets/ExcelFiles";
//    private string jsonOutputFolder = "Assets/Resources/Events";
//    private string classOutputFolder = "Assets/Json";

//    private List<string> excelFilePaths = new List<string>();

//    [MenuItem("Tools/Excel Auto Generator")]
//    public static void ShowWindow() => GetWindow<ExcelAutoGenerator>("Excel Auto Generator");

//    private void OnEnable() => ScanExcelFiles();

//    private void OnGUI()
//    {
//        GUILayout.Label("\uD83D\uDCC1 \uD3F4\uB354 \uACBD\uB85C \uC124\uC815", EditorStyles.boldLabel);

//        excelFolderPath = EditorGUILayout.TextField("\uC5D0\uD070 \uD30C\uC77C \uD3F4\uB354", excelFolderPath);
//        jsonOutputFolder = EditorGUILayout.TextField("JSON \uC800\uC7A5 \uD3F4\uB354", jsonOutputFolder);
//        classOutputFolder = EditorGUILayout.TextField("C# \uD074\uB798\uC2A4 \uC800\uC7A5 \uD3F4\uB354", classOutputFolder);

//        if (GUILayout.Button("\uD3F4\uB354 \uB2E4\uC2DC \uC2A4\uCE94")) ScanExcelFiles();

//        GUILayout.Label($"\uC778\uC2DC\uB41C \uC5D0\uD070 \uD30C\uC77C \uC218: {excelFilePaths.Count}", EditorStyles.label);
//        foreach (var file in excelFilePaths)
//            EditorGUILayout.LabelField("- " + file);

//        if (GUILayout.Button("\uBAA8\uB4E0 \uC5D0\uD070 \uD30C\uC77C \uBCC0\uD658 (\uC2DC\uD2B8\uBCC4 JSON + Class)"))
//        {
//            foreach (var file in excelFilePaths)
//                ConvertExcel(file);
//            AssetDatabase.Refresh();
//        }
//    }

//    private void ScanExcelFiles()
//    {
//        excelFilePaths.Clear();
//        if (!Directory.Exists(excelFolderPath)) return;

//        foreach (var file in Directory.GetFiles(excelFolderPath, "*.*", SearchOption.AllDirectories))
//        {
//            if (file.EndsWith(".xls") || file.EndsWith(".xlsx"))
//                excelFilePaths.Add(file);
//        }
//    }

//    private void ConvertExcel(string path)
//    {
//        using var stream = File.Open(path, FileMode.Open, FileAccess.Read);
//        using var reader = ExcelReaderFactory.CreateReader(stream);
//        var conf = new ExcelDataSetConfiguration { ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true } };
//        var dataSet = reader.AsDataSet(conf);

//        foreach (DataTable table in dataSet.Tables)
//        {
//            CreateJson(table);
//            CreateCSharpClass(table);
//        }
//    }

//    private void CreateJson(DataTable table)
//    {
//        var rows = new List<Dictionary<string, object>>();
//        foreach (DataRow dr in table.Rows)
//        {
//            var dict = new Dictionary<string, object>();
//            foreach (DataColumn col in table.Columns)
//            {
//                var val = dr[col];
//                if (val is string s && s.Contains("|"))
//                    dict[col.ColumnName] = s.Split('|');
//                else
//                    dict[col.ColumnName] = val;
//            }
//            rows.Add(dict);
//        }

//        var workbook = new Dictionary<string, List<Dictionary<string, object>>>
//        {
//            [table.TableName] = rows
//        };

//        if (!Directory.Exists(jsonOutputFolder)) Directory.CreateDirectory(jsonOutputFolder);
//        var jsonSavePath = Path.Combine(jsonOutputFolder, table.TableName + ".json");
//        File.WriteAllText(jsonSavePath, JsonConvert.SerializeObject(workbook, Formatting.Indented));
//    }

//    private void CreateCSharpClass(DataTable table)
//    {
//        if (!Directory.Exists(classOutputFolder)) Directory.CreateDirectory(classOutputFolder);
//        var scriptFilePath = Path.Combine(classOutputFolder, table.TableName + ".cs");

//        using var writer = new StreamWriter(scriptFilePath, false);
//        writer.WriteLine("using System;");
//        writer.WriteLine("using System.Collections.Generic;");
//        writer.WriteLine("[Serializable]");
//        writer.WriteLine($"public class {table.TableName}");
//        writer.WriteLine("{");

//        foreach (DataColumn col in table.Columns)
//        {
//            string fieldName = SanitizeVariableName(col.ColumnName);
//            bool isArray = false;

//            foreach (DataRow row in table.Rows)
//            {
//                if (row[col] is string val && val.Contains("|")) { isArray = true; break; }
//            }

//            string type = isArray ? "string[]" : InferType(table, col);
//            writer.WriteLine($"    public {type} {fieldName};");
//        }

//        writer.WriteLine("}");
//    }

//    private static string InferType(DataTable table, DataColumn column)
//    {
//        string name = column.ColumnName.ToLower();
//        string[] forceString = { "name", "desc", "text", "title" };
//        foreach (var key in forceString) if (name.Contains(key)) return "string";

//        bool isInt = true, isFloat = true, isBool = true;
//        foreach (DataRow row in table.Rows)
//        {
//            string s = row[column]?.ToString();
//            if (string.IsNullOrWhiteSpace(s)) continue;
//            if (!(s.Equals("true", StringComparison.OrdinalIgnoreCase) || s.Equals("false", StringComparison.OrdinalIgnoreCase))) isBool = false;
//            if (!int.TryParse(s, out _)) isInt = false;
//            if (!float.TryParse(s, out _)) isFloat = false;
//        }
//        if (isBool) return "bool";
//        if (isInt) return "int";
//        if (isFloat) return "float";
//        return "string";
//    }

//    private static string SanitizeVariableName(string name) => System.Text.RegularExpressions.Regex.Replace(name.Replace(" ", "_").Replace("-", "_"), @"[^a-zA-Z0-9_]", "");
//}

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEditor;
using UnityEngine;
using ExcelDataReader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class ExcelAutoGenerator : EditorWindow
{
    private string excelFolderPath = "Assets/ExcelFiles";
    private string jsonOutputFolder = "Assets/Resources/Events";
    private string classOutputFolder = "Assets/Json";

    private List<string> excelFilePaths = new List<string>();

    [MenuItem("Tools/Excel Auto Generator")]
    public static void ShowWindow() => GetWindow<ExcelAutoGenerator>("Excel Auto Generator");

    private void OnEnable() => ScanExcelFiles();

    private void OnGUI()
    {
        GUILayout.Label("📁 폴더 경로 설정", EditorStyles.boldLabel);

        excelFolderPath = EditorGUILayout.TextField("엑셀 파일 폴더", excelFolderPath);
        jsonOutputFolder = EditorGUILayout.TextField("JSON 저장 폴더", jsonOutputFolder);
        classOutputFolder = EditorGUILayout.TextField("C# 클래스 저장 폴더", classOutputFolder);

        if (GUILayout.Button("폴더 다시 스캔")) ScanExcelFiles();

        GUILayout.Label($"인식된 엑셀 파일 수: {excelFilePaths.Count}", EditorStyles.label);
        foreach (var file in excelFilePaths)
            EditorGUILayout.LabelField("- " + file);

        if (GUILayout.Button("모든 엑셀 파일 변환 (시트별 JSON + Class)"))
        {
            foreach (var file in excelFilePaths)
                ConvertExcel(file);
            AssetDatabase.Refresh();
        }
    }

    private void ScanExcelFiles()
    {
        excelFilePaths.Clear();
        if (!Directory.Exists(excelFolderPath)) return;

        foreach (var file in Directory.GetFiles(excelFolderPath, "*.*", SearchOption.AllDirectories))
        {
            if (file.EndsWith(".xls") || file.EndsWith(".xlsx"))
                excelFilePaths.Add(file);
        }
        Debug.Log("모든 파일을 스캔 완료 했습니다");
    }

    private void ConvertExcel(string path)
    {
        using var stream = File.Open(path, FileMode.Open, FileAccess.Read);
        using var reader = ExcelReaderFactory.CreateReader(stream);
        var conf = new ExcelDataSetConfiguration { ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true } };
        var dataSet = reader.AsDataSet(conf);

        foreach (DataTable table in dataSet.Tables)
        {
            CreateJson(table);
            CreateCSharpClass(table);
            Debug.Log($"{table.TableName}를 변환 중입니다");
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
                var val = dr[col];
                if (val is string s && s.TrimStart().StartsWith("["))
                {
                    try
                    {
                        dict[col.ColumnName] = JArray.Parse(s);
                    }
                    catch
                    {
                        dict[col.ColumnName] = s; // 파싱 실패 시 문자열 그대로
                    }
                }
                else
                {
                    dict[col.ColumnName] = val;
                }
            }
            rows.Add(dict);
        }

        var workbook = new Dictionary<string, List<Dictionary<string, object>>>
        {
            [table.TableName] = rows
        };

        if (!Directory.Exists(jsonOutputFolder)) Directory.CreateDirectory(jsonOutputFolder);
        var jsonSavePath = Path.Combine(jsonOutputFolder, table.TableName + ".json");
        File.WriteAllText(jsonSavePath, JsonConvert.SerializeObject(workbook, Formatting.Indented));
    }

    private void CreateCSharpClass(DataTable table)
    {
        if (!Directory.Exists(classOutputFolder)) Directory.CreateDirectory(classOutputFolder);
        var scriptFilePath = Path.Combine(classOutputFolder, table.TableName + ".cs");

        using var writer = new StreamWriter(scriptFilePath, false);
        writer.WriteLine("using System;");
        writer.WriteLine("using System.Collections.Generic;");
        writer.WriteLine("[Serializable]");
        writer.WriteLine($"public class {table.TableName}");
        writer.WriteLine("{");

        foreach (DataColumn col in table.Columns)
        {
            string fieldName = SanitizeVariableName(col.ColumnName);
            string type = InferType(table, col);
            writer.WriteLine($"    public {type} {fieldName};");
        }

        writer.WriteLine("}");
    }

    private static string InferType(DataTable table, DataColumn column)
    {
        string name = column.ColumnName.ToLower();
        string[] forceString = { "name", "desc", "text", "title" };
        foreach (var key in forceString) if (name.Contains(key)) return "string";

        foreach (DataRow row in table.Rows)
        {
            string val = row[column]?.ToString();
            if (string.IsNullOrWhiteSpace(val)) continue;
            if (val.TrimStart().StartsWith("[")) return "List<EffectTrigger>";
        }

        return InferSimpleType(table, column);
    }

    private static string InferSimpleType(DataTable table, DataColumn column)
    {
        bool isInt = true, isFloat = true, isBool = true;
        foreach (DataRow row in table.Rows)
        {
            string s = row[column]?.ToString();
            if (string.IsNullOrWhiteSpace(s)) continue;
            if (!(s.Equals("true", StringComparison.OrdinalIgnoreCase) || s.Equals("false", StringComparison.OrdinalIgnoreCase))) isBool = false;
            if (!int.TryParse(s, out _)) isInt = false;
            if (!float.TryParse(s, out _)) isFloat = false;
        }
        if (isBool) return "bool";
        if (isInt) return "int";
        if (isFloat) return "float";
        return "string";
    }

    private static string SanitizeVariableName(string name) => System.Text.RegularExpressions.Regex.Replace(name.Replace(" ", "_").Replace("-", "_"), @"[^a-zA-Z0-9_]", "");
}


