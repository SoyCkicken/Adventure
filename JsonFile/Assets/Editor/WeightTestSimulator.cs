using UnityEngine;
using UnityEditor;

public class WeightTestSimulator : EditorWindow
{
    float totalAttack = 25.2f;
    float strValue = 4f;
    float dexValue = 4f;
    float intvalue = 4f;
    float intalvalue = 4f;
    float carvalue = 4f;
    float dirvalue = 4f;
    float strWeight = 0.15f;
    float dexWeight = 0.25f;
    float intweight;
    float intalweight;
    float carweight;
    float dirweight;


    [MenuItem("Tools/Attack Power Calculator")]
    public static void ShowWindow()
    {
        GetWindow<WeightTestSimulator>("공격력 계산기");
    }

    private void OnGUI()
    {
        GUILayout.Label("스탯 기반 공격력 계산기", EditorStyles.boldLabel);

        totalAttack = EditorGUILayout.FloatField("총 공격력", totalAttack);
        strValue = EditorGUILayout.FloatField("힘", strValue);
        dexValue = EditorGUILayout.FloatField("민첩", dexValue);
        intvalue = EditorGUILayout.FloatField("지력", intvalue);
        intalvalue = EditorGUILayout.FloatField("지능",intalvalue);
        carvalue = EditorGUILayout.FloatField("카리스마",carvalue);
        dirvalue = EditorGUILayout.FloatField("신성력", dirvalue);
        strWeight = EditorGUILayout.FloatField("힘 가중치", strWeight);
        dexWeight = EditorGUILayout.FloatField("민첩 가중치", dexWeight);
        intweight = EditorGUILayout.FloatField("지력 가중치", intweight);
        intalweight = EditorGUILayout.FloatField("지능 가중치", intalweight);
        carweight = EditorGUILayout.FloatField("카리스마 가중치", carweight);
        dirweight = EditorGUILayout.FloatField("신성력 가중치", dirweight);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("원래 공격력 (스탯 제외 가중치 제외):", $"{CalculateOriginalAttack():0.###}");
    }

    private float CalculateOriginalAttack()
    {
        float strBonus = strValue * strWeight;
        float dexBonus = dexValue * dexWeight;
        float intBonus = intvalue * intweight;
        float intalBonus = intalvalue * intalweight;
        float carBonus = carvalue * carweight;
        float dirBonus = dirvalue * dirweight;
        return totalAttack - strBonus - dexBonus - intBonus-intalBonus-carBonus-dirBonus;
    }
}