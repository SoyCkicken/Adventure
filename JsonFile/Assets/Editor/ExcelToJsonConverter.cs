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
//        GUILayout.Label("📁 폴더 경로 설정", EditorStyles.boldLabel);

//        excelFolderPath = EditorGUILayout.TextField("엑셀 파일 폴더", excelFolderPath);
//        jsonOutputFolder = EditorGUILayout.TextField("JSON 저장 폴더", jsonOutputFolder);
//        classOutputFolder = EditorGUILayout.TextField("C# 클래스 저장 폴더", classOutputFolder);

//        if (GUILayout.Button("폴더 다시 스캔")) ScanExcelFiles();

//        GUILayout.Label($"인식된 엑셀 파일 수: {excelFilePaths.Count}", EditorStyles.label);
//        foreach (var file in excelFilePaths)
//            EditorGUILayout.LabelField("- " + file);

//        if (GUILayout.Button("모든 엑셀 파일 변환 (시트별 JSON + Class)"))
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
//        Debug.Log("모든 파일을 스캔 완료 했습니다");
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
//            Debug.Log($"{table.TableName}를 변환 중입니다");
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
//                if (val is string s && s.TrimStart().StartsWith("["))
//                {
//                    try
//                    {
//                        dict[col.ColumnName] = JArray.Parse(s);
//                    }
//                    catch
//                    {
//                        dict[col.ColumnName] = s; // 파싱 실패 시 문자열 그대로
//                    }
//                }
//                else
//                {
//                    dict[col.ColumnName] = val;
//                }
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
//            string type = InferType(table, col);
//            writer.WriteLine($"    public {type} {fieldName};");
//        }

//        writer.WriteLine("}");
//    }

//    private static string InferType(DataTable table, DataColumn column)
//    {
//        string name = column.ColumnName.ToLower();
//        string[] forceString = { "name", "desc", "text", "title" };
//        foreach (var key in forceString)
//            if (name.Contains(key)) return "string";

//        foreach (DataRow row in table.Rows)
//        {
//            string val = row[column]?.ToString();
//            if (string.IsNullOrWhiteSpace(val)) continue;

//            // 배열 타입 체크
//            if (val.TrimStart().StartsWith("["))
//            {
//                try
//                {
//                    var jarray = JArray.Parse(val);
//                    if (jarray.Count == 0)
//                        return "List<string>"; // 비어있으면 기본 문자열 리스트

//                    var first = jarray.First;

//                    // 숫자 배열인지 확인
//                    if (first.Type == JTokenType.Integer)
//                        return "List<int>";
//                    if (first.Type == JTokenType.String)
//                        return "List<string>";
//                    if (first.Type == JTokenType.Object)
//                        return "List<EffectTrigger>"; // 구조체인 경우

//                    return "List<string>"; // 기본
//                }
//                catch
//                {
//                    return "string"; // 파싱 실패 시
//                }
//            }
//        }

//        return InferSimpleType(table, column); // 기존 숫자, bool 체크 유지
//    }

//    private static string InferSimpleType(DataTable table, DataColumn column)
//    {
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
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using ExcelDataReader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

public class ExcelAutoGenerator : EditorWindow
{
    private string excelFolderPath = "Assets/ExcelFiles";
    private string jsonOutputFolder = "Assets/Resources/Events";
    private string classOutputFolder = "Assets/Json";

    private List<string> excelFilePaths = new List<string>();

    private static string MakeAutoClassName(DataTable table, DataColumn column)
    => $"{SanitizeTypeName(table.TableName)}_{ToPascal(column.ColumnName)}Item";

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
            TypeRegistry.Clear();                 // ✅ 전체 런 한번 초기화
            foreach (var file in excelFilePaths)
                ConvertExcel(file);               // 각 파일: JSON + 주 클래스만 생성

            EmitRegistryClassesOnce();            // ✅ 보조 클래스들 한 파일로만 방출
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
        // ❌ TypeRegistry.Clear();  // 제거: 이제 전체 런 기준으로 한 번만 클리어
        using var stream = File.Open(path, FileMode.Open, FileAccess.Read);
        using var reader = ExcelReaderFactory.CreateReader(stream);
        var conf = new ExcelDataSetConfiguration { ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true } };
        var dataSet = reader.AsDataSet(conf);

        foreach (DataTable table in dataSet.Tables)
        {
            CreateJson(table);
            CreateCSharpClass(table);   // ✅ 이 함수는 이제 "주 클래스"만 생성
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
                if (val is string s)
                {
                    string trimmed = s.Trim();

                    // ✅ (추가) List<클래스>[ ... ] 타입 힌트 처리
                    if (TypeInferUtils.TryParseListTypeHint(trimmed, out var hintedElemType, out var hintedJson))
                    {
                        try
                        {
                            dict[col.ColumnName] = JArray.Parse(hintedJson);
                        }
                        catch
                        {
                            dict[col.ColumnName] = s; // 파싱 실패 시 원본 문자열 유지
                        }
                        continue;
                    }

                    // 기존: 배열 형태 문자열이면 JArray로 보관
                    if (trimmed.StartsWith("["))
                    {
                        try
                        {
                            dict[col.ColumnName] = JArray.Parse(trimmed);
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
        var scriptFilePath = Path.Combine(classOutputFolder, SanitizeTypeName(table.TableName) + ".cs");

        using var writer = new StreamWriter(scriptFilePath, false, Encoding.UTF8);
        writer.WriteLine("using System;");
        writer.WriteLine("using System.Collections.Generic;");
        writer.WriteLine("[Serializable]");
        writer.WriteLine($"public class {SanitizeTypeName(table.TableName)}");
        writer.WriteLine("{");

        foreach (DataColumn col in table.Columns)
        {
            string fieldName = SanitizeVariableName(col.ColumnName);
            string type = InferType(table, col);
            writer.WriteLine($"    public {type} {fieldName};");
        }

        writer.WriteLine("}");

        // ❌ 여기서 레지스트리 클래스들 쓰지 않는다 (중복 원인)
    }

    // =========================
    // 🔧 확장된 타입 추론부
    // =========================



    private static string InferType(DataTable table, DataColumn column)
    {
        string name = column.ColumnName.ToLowerInvariant();

        // 이름 기반 강제 string
        string[] forceString = { "name", "desc", "text", "title" };
        foreach (var key in forceString)
            if (name.Contains(key)) return "string";

        // 컬럼 전체 스캔
        foreach (DataRow row in table.Rows)
        {
            string raw = row[column]?.ToString();
            if (string.IsNullOrWhiteSpace(raw)) continue;

            string val = raw.Trim();

            // 1) List<클래스>[ ... ] 타입 힌트 지원 (최우선)
            if (TypeInferUtils.TryParseListTypeHint(val, out var hintedElemType, out var hintedJson))
            {
                try
                {
                    var jarray = JArray.Parse(hintedJson);
                    // 빈 배열이어도 사용자 힌트를 신뢰
                    if (IsSimpleTypeName(hintedElemType))
                    {
                        return $"List<{NormalizeSimpleType(hintedElemType)}>";
                    }
                    else
                    {
                        // 객체 타입 → 스키마 추론(첫 원소 기준)
                        if (jarray.Count > 0 && jarray.First.Type == JTokenType.Object)
                        {
                            ObjectSchemaInfer.InferClassFromJObject(hintedElemType, (JObject)jarray.First);
                        }
                        // 비어 있어도 힌트 타입을 그대로 사용
                        return $"List<{SanitizeTypeName(hintedElemType)}>";
                    }
                }
                catch
                {
                    return "string"; // 힌트 파싱 실패 시
                }
            }

            // 2) 일반 배열(JSON) 처리: "[ ... ]"
            if (val.StartsWith("["))
            {
                try
                {
                    var jarray = JArray.Parse(val);
                    if (jarray.Count == 0)
                        return "List<string>"; // 기본값

                    var first = jarray.First;

                    // 원시형 배열
                    if (first.Type == JTokenType.Integer) return "List<int>";
                    if (first.Type == JTokenType.Float) return "List<float>";
                    if (first.Type == JTokenType.Boolean) return "List<bool>";
                    if (first.Type == JTokenType.String) return "List<string>";

                    // 객체 배열 → 컬럼명 기반 자동 클래스(예: MainEffect → MainEffectItem)
                    if (first.Type == JTokenType.Object)
                    {
                        string className = MakeAutoClassName(table, column);   // ✅ 테이블명+컬럼명
                        ObjectSchemaInfer.InferClassFromJObject(className, (JObject)first);
                        return $"List<{SanitizeTypeName(className)}>";
                    }

                    return "List<string>"; // 그 외 보수적
                }
                catch
                {
                    return "string"; // 파싱 실패
                }
            }
        }

        // 3) 단일 값 추론(기존 로직 유지)
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

    // =========================
    // 🔧 헬퍼/레지스트리
    // =========================

    private static readonly Regex ListHintRegex = new Regex(
        @"^\s*List<\s*(?<type>[A-Za-z_][A-Za-z0-9_]*)\s*>\s*(?<array>\[.*\])\s*$",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private static class TypeInferUtils
    {
        public static bool TryParseListTypeHint(string cell, out string hintedElemType, out string jsonArray)
        {
            hintedElemType = null;
            jsonArray = null;
            if (string.IsNullOrWhiteSpace(cell)) return false;

            var m = ListHintRegex.Match(cell);
            if (!m.Success) return false;

            hintedElemType = m.Groups["type"].Value.Trim();
            jsonArray = m.Groups["array"].Value.Trim();
            return true;
        }

        public static string MapJTokenToCSharpType(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Integer: return "int";
                case JTokenType.Float: return "float";
                case JTokenType.Boolean: return "bool";
                case JTokenType.String: return "string";
                case JTokenType.Null: return "string";
                case JTokenType.Array: return "List<string>";
                case JTokenType.Object: return "object";
                default: return "string";
            }
        }
    }

    private static class ObjectSchemaInfer
    {
        public static void InferClassFromJObject(string className, JObject obj)
        {
            className = SanitizeTypeName(className);
            TypeRegistry.EnsureClass(className);

            foreach (var prop in obj.Properties())
            {
                var name = prop.Name;
                var val = prop.Value;

                if (val == null || val.Type == JTokenType.Null)
                {
                    TypeRegistry.AddOrWidenProperty(className, name, "string");
                    continue;
                }

                if (val.Type == JTokenType.Object)
                {
                    string nestedClass = $"{className}_{ToPascal(name)}";
                    nestedClass = SanitizeTypeName(nestedClass);
                    TypeRegistry.AddOrWidenProperty(className, name, nestedClass);
                    InferClassFromJObject(nestedClass, (JObject)val);
                }
                else if (val.Type == JTokenType.Array)
                {
                    var arr = (JArray)val;
                    if (arr.Count == 0)
                    {
                        TypeRegistry.AddOrWidenProperty(className, name, "List<string>");
                    }
                    else
                    {
                        var first = arr.First;
                        if (first.Type == JTokenType.Object)
                        {
                            string nestedClass = $"{className}_{ToPascal(name)}Item";
                            nestedClass = SanitizeTypeName(nestedClass);
                            TypeRegistry.AddOrWidenProperty(className, name, $"List<{nestedClass}>");
                            InferClassFromJObject(nestedClass, (JObject)first);
                        }
                        else
                        {
                            string elemType = TypeInferUtils.MapJTokenToCSharpType(first);
                            if (elemType == "object") elemType = "string";
                            TypeRegistry.AddOrWidenProperty(className, name, $"List<{elemType}>");
                        }
                    }
                }
                else
                {
                    string mapped = TypeInferUtils.MapJTokenToCSharpType(val);
                    if (mapped == "object") mapped = "string";
                    TypeRegistry.AddOrWidenProperty(className, name, mapped);
                }
            }
        }
    }

    private static class TypeRegistry
    {
        // className -> (propName -> csharpType)
        private static readonly Dictionary<string, Dictionary<string, string>> _classes
            = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        public static void Clear() => _classes.Clear();

        public static void EnsureClass(string className)
        {
            if (!_classes.ContainsKey(className))
                _classes[className] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public static void AddOrWidenProperty(string className, string propName, string csharpType)
        {
            EnsureClass(className);

            if (_classes[className].TryGetValue(propName, out var exist))
            {
                // 타입 충돌 시 안전하게 string으로 승격
                if (!string.Equals(exist, csharpType, StringComparison.OrdinalIgnoreCase))
                    _classes[className][propName] = "string";
            }
            else
            {
                _classes[className][propName] = csharpType;
            }
        }

        public static IReadOnlyDictionary<string, Dictionary<string, string>> Classes => _classes;
    }

    // =========================
    // 🔧 공통 유틸
    // =========================

    private static bool IsSimpleTypeName(string typeName)
    {
        switch (typeName.Trim().ToLowerInvariant())
        {
            case "int":
            case "int32":
            case "long":
            case "single":
            case "float":
            case "double":
            case "bool":
            case "boolean":
            case "string":
                return true;
            default:
                return false;
        }
    }

    private static string NormalizeSimpleType(string typeName)
    {
        switch (typeName.Trim().ToLowerInvariant())
        {
            case "int32": return "int";
            case "single": return "float";
            case "boolean": return "bool";
            default: return typeName.Trim();
        }
    }

    private static string ToPascal(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        var parts = s.Split(new[] { '_', ' ', '-' }, StringSplitOptions.RemoveEmptyEntries);
        return string.Concat(parts.Select(p => char.ToUpperInvariant(p[0]) + p.Substring(1)));
    }

    private static string SanitizeVariableName(string name)
        => System.Text.RegularExpressions.Regex.Replace(name.Replace(" ", "_").Replace("-", "_"), @"[^a-zA-Z0-9_]", "");

    private static string SanitizeTypeName(string name)
    {
        string s = SanitizeVariableName(name);
        if (string.IsNullOrEmpty(s)) s = "AutoType";
        if (char.IsDigit(s[0])) s = "_" + s;
        return s;
    }

    // 파일 하단 아무 곳에 추가
    private void EmitRegistryClassesOnce()
    {
        if (!Directory.Exists(classOutputFolder)) Directory.CreateDirectory(classOutputFolder);
        var extraPath = Path.Combine(classOutputFolder, "Auto_GeneratedTypes.cs");

        using var writer = new StreamWriter(extraPath, false, Encoding.UTF8);
        writer.WriteLine("using System;");
        writer.WriteLine("using System.Collections.Generic;");

        var emitted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var kv in TypeRegistry.Classes)
        {
            string className = SanitizeTypeName(kv.Key);
            if (!emitted.Add(className)) continue;  // ✅ 중복 방지

            writer.WriteLine();
            writer.WriteLine("[Serializable]");
            writer.WriteLine($"public class {className}");
            writer.WriteLine("{");
            foreach (var p in kv.Value)
                writer.WriteLine($"    public {p.Value} {SanitizeVariableName(p.Key)};");
            writer.WriteLine("}");
        }
    }
}
