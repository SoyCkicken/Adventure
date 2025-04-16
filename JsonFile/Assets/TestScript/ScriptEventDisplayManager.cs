using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class ScriptEventDisplayManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform contentParent;       // ScrollView의 Content Transform
    public GameObject dialogBlockPrefab;  // DialogBlock 프리팹 (DialogBlockUI 포함)
    [Header("JSON Data Manager")]
    public JsonManager jsonManager;       // JsonManager에 등록된 Script_Master_Event 리스트를 읽어옴
    public TMP_Text textComp;
    public string tempstring;
    public string temp;
    public string temp2;

    private void Awake()
    {
        // jsonManager가 설정되지 않았다면 씬 내에서 찾는다.
        if (jsonManager == null)
        {
            jsonManager = FindObjectOfType<JsonManager>();
        }
    }
    private void Start()
    {
        TestdebugLog();
        Script_Master_EventDataLoad();

        //Script_Master_MainDataLoad();


    }

    public void TestdebugLog()
    {
        List<Story_Master> scriptEvents = jsonManager.storyMasters;
        List<Script_Master_Main> script_Master_Mains = jsonManager.scriptMasterMains;
        //Debug.Log($"{script_Master_Mains}가 있는지 확인");
        foreach (Story_Master ev in scriptEvents)
        {
            temp = $"MainScript_{ev.Chapter_Index}_{ev.Event_Index}_{ev.Scenc_Index}";
            
            foreach (Script_Master_Main sm in script_Master_Mains)
            {
                //Debug.Log(sm.KOR);
                //Debug.Log(temp);
                temp2 = sm.Script_Code;//
                //Debug.Log(temp2);
                if (temp == temp2)
                {
                    Debug.Log("temp와 story_Master.Scene_Code가 같은 값을 찾았습니다");
                }
                else
                {
                    Debug.Log($"temp의 값 : {temp}\ntemp2의 값 {temp2}");
                }
            }
        }
    }
    public void Script_Master_EventDataLoad()
    {
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
                ui.SetBlockDataEvent(ev);
            }
            else
            {
                Debug.LogError("DialogBlockUI 컴포넌트를 찾을 수 없습니다.");
            }
        }
    }
    public void Script_Master_MainDataLoad()
    {
        // JsonManager에 있는 Script_Master_Mains 데이터를 가져온다.
        List<Script_Master_Main> scriptMains = jsonManager.scriptMasterMains;
        if (scriptMains == null || scriptMains.Count == 0)
        {
            Debug.LogWarning("Script_Master_Mains 데이터가 없습니다.");
            return;
        }
        // 각 이벤트 데이터마다 프리팹을 생성해서 Content에 추가한다.
        foreach (Script_Master_Main mv in scriptMains)
        {
            //타입이 이미지일 경우
            //대소문자 구분 필수
            if (mv.displayType == "Image")
            {
                //이미지일때만 새로 생성 하는데 이걸 수정을 해야 됨
                //이유 내용이 다 나오고 이미지가 나오는 경우는 드물기 때문
                //이미지와 텍스트가 번갈아가며 나올 예정인데 구분하는 방법이 있어야 함
                GameObject entry = Instantiate(dialogBlockPrefab, contentParent);
                //프리팹에 붙은 DialogBlockUI 컴포넌트를 찾아, 데이터 셋업 실행
                DialogBlockUI ui = entry.GetComponent<DialogBlockUI>();
                if (ui != null)
                {
                    ui.SetBlockDataMain(mv);
                }
            }
            //타입이 텍스트일때
            else
            {
                tempstring += mv.KOR;
            }
            
        }
        StartCoroutine(TypeTextEffect(tempstring));
    }
    IEnumerator TypeTextEffect(string text)
    {

        Debug.Log("스킵버튼 활성화");
        //textComp.text = string.Empty; //문자열을 비우고
        //스트링빌더(한글자씩 추가해주는 함수)
        StringBuilder stringBuilder = new StringBuilder();
        if (text != null)
        {
            for (int i = 0; i < text.Length; i++)
            {
                //한글자씩 추가
                stringBuilder.Append(text[i]);
                //받은 문자들을 text에 담아서 
                textComp.text = stringBuilder.ToString();
                //0.01초마다 한번씩 출력시킴
                yield return new WaitForSeconds(0.05f);
            }
        }
        else
        {
            //RamEvent같은 경우 설명 같은게 하나도 없기 때문에 에러가 발생을 하는데 그걸 막고자 if문 사용했음
            yield break;
        }

        Debug.Log("스킵버튼 비활성화");
    }
}


//지금 전부 다 List로 저장이 되어 있다
//이걸 메서드로써 나누면 재사용하는데 큰 도움이 될것 같은데
