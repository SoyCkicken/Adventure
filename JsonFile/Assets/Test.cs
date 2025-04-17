using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class Test : MonoBehaviour
{
    public List<GameObject> gameObjects;


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
        //TestdebugLog();
        //Script_Master_EventDataLoad();

        Script_Master_MainDataLoad();
    }

    /*
     * 리스트가 비어있을 경우 하나를 꼭 만듬
     * if (objectList.Count == 0)
{
    // 리스트가 비어 있으면 무조건 새 오브젝트 생성
    GameObject obj = Instantiate(prefab, parent);
    objectList.Add(obj);
    //텍스트인지 아닌지 확인하는 프로세싱
    ApplyContent(obj, newContent); // 이미지 or 텍스트 적용
}
값이 있을 경우이니
else
{
    마지막 게임오브젝트가 이미지인지 텍스트인지 확인
    GameObject lastObj = objectList[objectList.Count - 1];
    //이미지가 활성화 되어 있었다면 새로운 오브젝트 추가
    if (lastObj.GetComponentInChildren<Image>().enabled)
    {
        // 마지막이 이미지였다면 새 오브젝트 생성
        GameObject obj = Instantiate(prefab, parent);
        objectList.Add(obj);
        ApplyContent(obj, newContent);
    }
    else
    {
        // 마지막이 텍스트라면, 해당 Text에 글자 하나 추가
        TMP_Text text = lastObj.GetComponentInChildren<TMP_Text>();
        string fullText = "출력할 전체 문자열";
        int currentLength = text.text.Length;

        if (currentLength < fullText.Length)
        {
            text.text += fullText[currentLength];
        }
    }
}
    //이걸 SetBlockDataMain가 대신 하고 있으니 구조를 파악하면 사용이 가능할 예정
    
    void ApplyContent(GameObject obj, string content)
{
    Image img = obj.GetComponentInChildren<Image>();
    TMP_Text txt = obj.GetComponentInChildren<TMP_Text>();

    if (IsImagePath(content))
    {
        Sprite sprite = LoadImage(content); // Resources나 Addressable 등 사용
        img.sprite = sprite;
        img.enabled = true;
        txt.enabled = false;
    }
    else
    {
        txt.text = ""; // 처음은 빈 텍스트
        img.enabled = false;
        txt.enabled = true;
    }
}
     */


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
                //stringBuilder.Append(text[i]);
                ////받은 문자들을 text에 담아서 
                //textComp.text = stringBuilder.ToString();
                //0.01초마다 한번씩 출력시킴
                //sceneText.text += text[i].ToString();
                //yield return new WaitForSeconds(0.05f);
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
