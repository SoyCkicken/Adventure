using MyGame;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class BuffUI : MonoBehaviour
{
    //public Transform buffParent;
    public GameObject buffIconPrefab;

    private List<BuffIconUI> activeIcons = new();
    public Transform playerBuffParent;
    public Transform enemyBuffParent;

    //버프 리스트를 UI로 표시
    public void SetBuffs(List<BuffData> buffs,Character character)
    {
        //Clear();
        Transform targetParnent = null; ;

        Debug.Log($"버프 적용 중인 대상 {character.charaterName}");
        if (character.charaterName == "Player")
            targetParnent = playerBuffParent;
        else
        {
            targetParnent = enemyBuffParent;
        }
        foreach (var buff in buffs)
        { 
            var icon = Instantiate(buffIconPrefab, targetParnent).GetComponent<BuffIconUI>();
            icon.Set(buff);
            activeIcons.Add(icon);
        }
    }

    // 시간 갱신용
    public void UpdateBuffTimers()
    {
        foreach (var icon in activeIcons)
        {
            //icon.UpdateUI();
            
        }
    }

    public void Clear()
    {
        foreach (var icon in activeIcons)
            Destroy(icon.gameObject);

        activeIcons.Clear();
    }
}

