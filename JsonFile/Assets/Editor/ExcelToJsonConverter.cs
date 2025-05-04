using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ExcelDataReader; // ExcelDataReader 네임스페이스
using Newtonsoft.Json;  // JSON 변환용 (패키지 매니저에서 설치하거나 직접 .dll 추가)

/// <summary>
/// 에디터 메뉴에 "Tools/Excel → JSON" 항목을 추가하고,
/// 선택한 .xlsx/.xls 파일을 JSON으로 변환해 Assets/Resources/ExcelJsons/ 아래에 저장해 줍니다.
/// </summary>
public class ExcelToJsonConverter : EditorWindow
{
    // 윈도우 인스턴스
    private static ExcelToJsonConverter window;
    // 변환할 엑셀 파일 경로
    private string excelPath = "";

    [MenuItem("Tools/Excel → JSON", priority = 100)]
    public static void ShowWindow()
    {
        // 메뉴 클릭 시 윈도우 띄우기
        window = GetWindow<ExcelToJsonConverter>("Excel To JSON");
        window.minSize = new Vector2(400, 100);
    }

    private void OnGUI()
    {
        GUILayout.Label("엑셀 파일 선택", EditorStyles.boldLabel);

        // 1) 파일 패스를 텍스트 필드에 보여주고
        EditorGUILayout.BeginHorizontal();
        excelPath = EditorGUILayout.TextField(excelPath);
        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
            // 2) 파일 다이얼로그 열기 (.xls, .xlsx 필터)
            string path = EditorUtility.OpenFilePanel("Select Excel File", "", "xls,xlsx");
            if (!string.IsNullOrEmpty(path))
                excelPath = path;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // 변환 버튼
        if (GUILayout.Button("Convert to JSON") && !string.IsNullOrEmpty(excelPath))
        {
            ConvertExcelToJson(excelPath);
        }
    }

    /// <summary>
    /// 실제 변환 로직
    /// </summary>
    private static void ConvertExcelToJson(string path)
    {
        // 엑셀 파일을 바이트 스트림으로 열기
        using var stream = File.Open(path, FileMode.Open, FileAccess.Read);
        // ExcelDataReader 초기화 (자동으로 xls/xlsx 구분)
        using var reader = ExcelReaderFactory.CreateReader(stream);
        // DataSet으로 읽어서 각 시트별로 DataTable을 얻음
        var conf = new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
        };
        var dataSet = reader.AsDataSet(conf);

        // JSON으로 직렬화할 오브젝트 만들기
        var workbook = new Dictionary<string, List<Dictionary<string, object>>>();

        foreach (DataTable table in dataSet.Tables)
        {
            var rows = new List<Dictionary<string, object>>();

            // 각 행을 Dictionary<컬럼명, 값>으로 변환
            foreach (DataRow dr in table.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                {
                    dict[col.ColumnName] = dr[col];
                }
                rows.Add(dict);
            }

            workbook[table.TableName] = rows;
        }

        // JSON 문자열 생성 (들여쓰기 옵션)
        string json = JsonConvert.SerializeObject(workbook, Formatting.Indented);

        // 저장할 경로 준비 (Assets/Resources/ExcelJsons/)
        string folder = "Assets/Resources/ExcelJsons";
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        // 원본 파일명으로 .json 확장자 붙여 저장
        string fileName = Path.GetFileNameWithoutExtension(path) + ".json";
        string assetPath = Path.Combine(folder, fileName);
        File.WriteAllText(assetPath, json);

        // 에디터에 갱신 알리기
        AssetDatabase.Refresh();
        Debug.Log($"[ExcelToJson] Successfully converted to {assetPath}");
    }
}
