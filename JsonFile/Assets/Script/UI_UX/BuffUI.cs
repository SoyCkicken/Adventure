using MyGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffUI : MonoBehaviour
{
    public Transform buffParent;
    public GameObject buffIconPrefab;

    private List<BuffIconUI> activeIcons = new();
    public Transform playerBuffParent;
    public Transform enemyBuffParent;

    /// <summary>
    /// 버프 리스트를 UI로 표시
    /// </summary>
    public void SetBuffs(List<BuffData> buffs,Character character)
    {
        Clear();
        Transform targetParnent = null; ;

        if (character.charaterName == "Player")
            targetParnent = playerBuffParent;
        else
        {
            targetParnent = enemyBuffParent;

        }
            


        foreach (var buff in buffs)
        {
            var icon = Instantiate(buffIconPrefab, buffParent).GetComponent<BuffIconUI>();
            icon.Set(buff);
            activeIcons.Add(icon);
        }
    }

    /// <summary>
    /// 시간 갱신용
    /// </summary>
    public void UpdateBuffTimers()
    {
        foreach (var icon in activeIcons)
        {
            icon.UpdateUI();
        }
    }

    private void Clear()
    {
        foreach (var icon in activeIcons)
            Destroy(icon.gameObject);

        activeIcons.Clear();
    }
}

