using UnityEngine;
using UnityEditor;

public class WeightTestSimulator : EditorWindow
{
    float totalAttack = 25.2f;
    float strValue = 4f;
    float dexValue = 4f;
    float strWeight = 0.15f;
    float dexWeight = 0.25f;

    [MenuItem("Tools/Attack Power Calculator")]
    public static void ShowWindow()
    {
        GetWindow<WeightTestSimulator>("°ø°Ý·Â °è»ê±â");
    }

    private void OnGUI()
    {
        GUILayout.Label("½ºÅÈ ±â¹Ý °ø°Ý·Â °è»ê±â", EditorStyles.boldLabel);

        totalAttack = EditorGUILayout.FloatField("ÃÑ °ø°Ý·Â", totalAttack);
        strValue = EditorGUILayout.FloatField("Èû", strValue);
        dexValue = EditorGUILayout.FloatField("¹ÎÃ¸", dexValue);
        strWeight = EditorGUILayout.FloatField("Èû °¡ÁßÄ¡", strWeight);
        dexWeight = EditorGUILayout.FloatField("¹ÎÃ¸ °¡ÁßÄ¡", dexWeight);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("¿ø·¡ °ø°Ý·Â (½ºÅÈ Á¦¿Ü):", $"{CalculateOriginalAttack():0.###}");
    }

    private float CalculateOriginalAttack()
    {
        float strBonus = strValue * strWeight;
        float dexBonus = dexValue * dexWeight;
        return totalAttack - strBonus - dexBonus;
    }
}