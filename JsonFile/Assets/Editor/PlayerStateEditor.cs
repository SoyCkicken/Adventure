//using UnityEngine;
//using UnityEditor;
//using TMPro;

//[CustomEditor(typeof(PlayerState))]
//public class PlayerStateEditor : Editor
//{
//    private bool showStats = true;
//    private bool showResources = true;
//    private bool showUIRefs = true;

//    public override void OnInspectorGUI()
//    {
//        serializedObject.Update();

//        PlayerState ps = (PlayerState)target;


//        GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
//        foldoutStyle.fontStyle = FontStyle.Bold;

//        showStats = EditorGUILayout.Foldout(showStats, "능력치 (Stats)", true, foldoutStyle);
//        if (showStats)
//        {
//            EditorGUI.indentLevel++;
//            ps.Strength = EditorGUILayout.IntField("Strength", ps.Strength);
//            ps.DEX = EditorGUILayout.IntField("Dexterity", ps.DEX);
//            ps.Divinity = EditorGUILayout.IntField("Divinity", ps.Divinity);
//            ps.Int = EditorGUILayout.IntField("Intelligence", ps.Int);
//            ps.MAG = EditorGUILayout.IntField("Wisdom", ps.MAG);
//            ps.Charisma = EditorGUILayout.IntField("Charisma", ps.Charisma);
//            EditorGUI.indentLevel--;
//        }

//        EditorGUILayout.Space();

//        showResources = EditorGUILayout.Foldout(showResources, "자원 (Health / Mental)", true, foldoutStyle);
//        if (showResources)
//        {
//            EditorGUI.indentLevel++;
//            ps.CurrentHealth = EditorGUILayout.IntField("Current Health", ps.CurrentHealth);
//            ps.Health = EditorGUILayout.IntField("Max Health", ps.Health);
//            ps.CurrentMental = EditorGUILayout.IntField("Current Mental", ps.CurrentMental);
//            ps.MP = EditorGUILayout.IntField("Max Mental", ps.MP);
//            EditorGUI.indentLevel--;
//        }

//        EditorGUILayout.Space();

//        showUIRefs = EditorGUILayout.Foldout(showUIRefs, "UI 연결 요소", true, foldoutStyle);
//        if (showUIRefs)
//        {
//            EditorGUI.indentLevel++;
//            ps.Strengthtext = (TextMeshProUGUI)EditorGUILayout.ObjectField("Strength Text", ps.Strengthtext, typeof(TextMeshProUGUI), true);
//            ps.DEXtext = (TextMeshProUGUI)EditorGUILayout.ObjectField("DEX Text", ps.DEXtext, typeof(TextMeshProUGUI), true);
//            ps.CharismaText = (TextMeshProUGUI)EditorGUILayout.ObjectField("Charisma Text", ps.CharismaText, typeof(TextMeshProUGUI), true);
//            ps.DivinityText = (TextMeshProUGUI)EditorGUILayout.ObjectField("Divinity Text", ps.DivinityText, typeof(TextMeshProUGUI), true);
//            ps.IntText = (TextMeshProUGUI)EditorGUILayout.ObjectField("Int Text", ps.IntText, typeof(TextMeshProUGUI), true);
//            ps.MAGtext = (TextMeshProUGUI)EditorGUILayout.ObjectField("MAG Text", ps.MAGtext, typeof(TextMeshProUGUI), true);
//            ps.HealthText = (TextMeshProUGUI)EditorGUILayout.ObjectField("Health Text", ps.HealthText, typeof(TextMeshProUGUI), true);

//            EditorGUI.indentLevel--;
//        }
//        serializedObject.ApplyModifiedProperties();
//    }
//}
