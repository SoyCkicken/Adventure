//using MyGame;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using static UnityEngine.UI.GridLayoutGroup;

//public class BuffUI : MonoBehaviour
//{
//    //public Transform buffParent;
//    public GameObject buffIconPrefab;

//    private List<BuffIconUI> activeIcons = new();
//    public Transform playerBuffParent;
//    public Transform enemyBuffParent;

//    //버프 리스트를 UI로 표시
//    public void SetBuffs(List<BuffData> buffs,Character character)
//    {
//        //Clear();
//        Transform targetParnent = null; ;

//        Debug.Log($"버프 적용 중인 대상 {character.charaterName}");
//        if (character.charaterName == "Player")
//            targetParnent = playerBuffParent;
//        else
//        {
//            targetParnent = enemyBuffParent;
//        }
//        foreach (var buff in buffs)
//        { 
//            var icon = Instantiate(buffIconPrefab, targetParnent).GetComponent<BuffIconUI>();
//            icon.Set(buff);
//            activeIcons.Add(icon);
//        }
//    }

//    // 시간 갱신용
//    public void UpdateBuffTimers()
//    {
//        foreach (var icon in activeIcons)
//        {
//            //icon.UpdateUI();

//        }
//    }

//    public void Clear()
//    {
//        foreach (var icon in activeIcons)
//            Destroy(icon.gameObject);

//        activeIcons.Clear();
//    }
//}



using MyGame;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuffUI : MonoBehaviour
{
    public GameObject buffIconPrefab;
    public Transform playerBuffParent;
    public Transform enemyBuffParent;
    public GameObject battleGameObject; // 자동 전투 화면 캔버스
    [SerializeField] public List<BuffIconUI> activeIcons = new();



    /// <summary>
    /// 전체 버프 리스트를 받아 UI 아이콘으로 표시
    /// </summary>
    public void SetBuffs(List<BuffData> buffs, Character character)
    {
        ClearAll(); // 기존 아이콘 제거

        Transform targetParent = (character.charaterName == "Player") ? playerBuffParent : enemyBuffParent;

        foreach (var buff in buffs)
        {
           battleGameObject.SetActive(true); // 자동 전투 화면 캔버스 활성화
            var iconGO = Instantiate(buffIconPrefab, targetParent);
            var icon = iconGO.GetComponent<BuffIconUI>();

            if (icon == null)
            {
                Debug.LogError($"[BuffUI] BuffIconUI 컴포넌트 없음 - {iconGO.name}");
                continue;
            }

            icon.Set(buff);
            activeIcons.Add(icon);
            Debug.Log($"[BuffUI] 아이콘 추가됨: {buff.BuffID}");
            battleGameObject.SetActive(false); // 아이콘 생성 후 자동 전투 화면 캔버스 비활성화
        }
    }

    /// <summary>
    /// 모든 버프 아이콘 제거
    /// </summary>
    public void ClearAll()
    {
        foreach (var icon in activeIcons)
        {
            Destroy(icon.gameObject);
        }
        activeIcons.Clear();
    }

    /// <summary>
    /// 특정 BuffID에 해당하는 버프 아이콘만 제거
    /// </summary>
    public void ClearBuffByID(string buffID)
    {
        for (int i = activeIcons.Count - 1; i >= 0; i--)
        {
            if (activeIcons[i].buffData != null && activeIcons[i].buffData.BuffID == buffID)
            {
                Destroy(activeIcons[i].gameObject);
                activeIcons.RemoveAt(i);
            }
        }
    }
}
