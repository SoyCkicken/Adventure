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
        foreach (var key in forceString)
            if (name.Contains(key)) return "string";

        foreach (DataRow row in table.Rows)
        {
            string val = row[column]?.ToString();
            if (string.IsNullOrWhiteSpace(val)) continue;

            // 배열 타입 체크
            if (val.TrimStart().StartsWith("["))
            {
                try
                {
                    var jarray = JArray.Parse(val);
                    if (jarray.Count == 0)
                        return "List<string>"; // 비어있으면 기본 문자열 리스트

                    var first = jarray.First;

                    // 숫자 배열인지 확인
                    if (first.Type == JTokenType.Integer)
                        return "List<int>";
                    if (first.Type == JTokenType.String)
                        return "List<string>";
                    if (first.Type == JTokenType.Object)
                        return "List<EffectTrigger>"; // 구조체인 경우

                    return "List<string>"; // 기본
                }
                catch
                {
                    return "string"; // 파싱 실패 시
                }
            }
        }

        return InferSimpleType(table, column); // 기존 숫자, bool 체크 유지
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