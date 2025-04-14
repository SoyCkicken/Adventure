using System.Collections.Generic;
using UnityEngine;

public class ScriptEventDisplayManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform contentParent;       // ScrollView의 Content Transform
    public GameObject dialogBlockPrefab;  // DialogBlock 프리팹 (DialogBlockUI 포함)

    [Header("JSON Data Manager")]
    public JsonManager jsonManager;       // JsonManager에 등록된 Script_Master_Event 리스트를 읽어옴

    private void Start()
    {
        // jsonManager가 설정되지 않았다면 씬 내에서 찾는다.
        if (jsonManager == null)
        {
            jsonManager = FindObjectOfType<JsonManager>();
        }

        // JsonManager에 있는 Script_Master_Event 데이터를 가져온다.
        List<Script_Master_Event> scriptEvents = jsonManager.scriptMasterEvents;
        if (scriptEvents == null || scriptEvents.Count == 0)
        {
            Debug.LogWarning("Script_Master_Event 데이터가 없습니다.");
            return;
        }

        // 각 이벤트 데이터마다 프리팹을 생성해서 Content에 추가한다.
        foreach (Script_Master_Event ev in scriptEvents)
        {
            GameObject entry = Instantiate(dialogBlockPrefab, contentParent);
            // 프리팹에 붙은 DialogBlockUI 컴포넌트를 찾아, 데이터 셋업 실행
            DialogBlockUI ui = entry.GetComponent<DialogBlockUI>();
            if (ui != null)
            {
                ui.SetBlockData(ev);
            }
            else
            {
                Debug.LogError("DialogBlockUI 컴포넌트를 찾을 수 없습니다.");
            }
        }
    }
}
