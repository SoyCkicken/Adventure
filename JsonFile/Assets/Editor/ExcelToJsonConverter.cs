using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ExcelDataReader;
using Newtonsoft.Json;

public class ExcelToJsonConverter : EditorWindow
{
    private List<UnityEngine.Object> excelFiles = new List<UnityEngine.Object>();
    private const string TARGET_FOLDER = "Assets/ExcelFiles"; // ✅ 자동 인식할 폴더

    [MenuItem("Tools/Excel → JSON (Auto Folder Scan)")]
    public static void ShowWindow()
    {
        GetWindow<ExcelToJsonConverter>("Excel To JSON (Auto Scan)");
    }

    private void OnEnable()
    {
        // 기존 EditorPrefs 저장된 경로 로드
        if (EditorPrefs.HasKey("ExcelToJson_FileList"))
        {
            string savedPaths = EditorPrefs.GetString("ExcelToJson_FileList");
            string[] paths = savedPaths.Split(';');

            excelFiles.Clear();

            foreach (string path in paths)
            {
                if (!string.IsNullOrEmpty(path))
                {
                    var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                    if (obj != null && !excelFiles.Contains(obj))
                        excelFiles.Add(obj);
                }
            }
        }

        // ✅ 폴더 자동 스캔
        AutoScanExcelFiles();
    }

    private void AutoScanExcelFiles()
    {
        if (!AssetDatabase.IsValidFolder(TARGET_FOLDER))
        {
            Debug.LogWarning($"폴더 {TARGET_FOLDER} 이 존재하지 않습니다.");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("", new[] { TARGET_FOLDER });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.EndsWith(".xls") || path.EndsWith(".xlsx"))
            {
                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                if (obj != null && !excelFiles.Contains(obj))
                {
                    excelFiles.Add(obj);
                }
            }
        }
    }

    private void OnDisable()
    {
        // EditorPrefs에 경로 저장
        List<string> paths = new List<string>();
        foreach (var obj in excelFiles)
        {
            if (obj != null)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path))
                    paths.Add(path);
            }
        }
        EditorPrefs.SetString("ExcelToJson_FileList", string.Join(";", paths));
    }

    private void OnGUI()
    {
        GUILayout.Label($"엑셀 파일 목록 (자동 스캔 폴더: {TARGET_FOLDER})", EditorStyles.boldLabel);

        for (int i = 0; i < excelFiles.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            excelFiles[i] = EditorGUILayout.ObjectField($"Excel 파일 {i + 1}", excelFiles[i], typeof(UnityEngine.Object), false);

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                excelFiles.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Add Excel File Slot"))
        {
            excelFiles.Add(null);
        }

        if (GUILayout.Button("Rescan Folder"))
        {
            AutoScanExcelFiles();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Convert All to JSON"))
        {
            foreach (var excelFile in excelFiles)
            {
                if (excelFile == null)
                {
                    Debug.LogWarning("빈 슬롯은 무시합니다.");
                    continue;
                }

                string assetPath = AssetDatabase.GetAssetPath(excelFile);
                string fullPath = Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length), assetPath);

                if (assetPath.EndsWith(".xls") || assetPath.EndsWith(".xlsx"))
                {
                    ConvertExcelToJson(fullPath);
                }
                else
                {
                    Debug.LogError($"파일 {assetPath} 은(는) .xls/.xlsx 파일이 아닙니다.");
                }
            }
        }
    }

    private static void ConvertExcelToJson(string path)
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

            var workbook = new Dictionary<string, List<Dictionary<string, object>>>();

            foreach (DataTable table in dataSet.Tables)
            {
                var rows = new List<Dictionary<string, object>>();
                foreach (DataRow dr in table.Rows)
                {
                    var dict = new Dictionary<string, object>();
                    foreach (DataColumn col in table.Columns)
                        dict[col.ColumnName] = dr[col];
                    rows.Add(dict);
                }
                workbook[table.TableName] = rows;
            }

            string json = JsonConvert.SerializeObject(workbook, Formatting.Indented);

            string folder = "Assets/Resources/ExcelJsons";
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string fileName = Path.GetFileNameWithoutExtension(path) + ".json";
            string savePath = Path.Combine(folder, fileName);
            File.WriteAllText(savePath, json);

            AssetDatabase.Refresh();
            Debug.Log($"[ExcelToJson] {savePath} 로 변환 완료");
        }
        catch (Exception ex)
        {
            Debug.LogError($"엑셀 변환 중 에러 발생: {ex.Message}");
        }
    }
}
