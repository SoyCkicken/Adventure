using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading;

public class DynamicDialogManager : MonoBehaviour
{
    [Header("연동할 프리팹 & 부모")]
    public GameObject dialogBlockPrefab;   // DialogBlockUI 프리팹
    public Transform contentParent;        // 인스턴스될 부모(ScrollView.Content 등)

    [Header("딜레이 설정")]
    public float typingDelay = 0.01f;      // 한글자씩 찍힐 시간

    [Header("JSON 데이터 관리자")]
    public JsonManager jsonManager;        // Script_Master_Event 리스트를 들고 있는 오브젝트

    // 내부 사용 리스트
    private List<DialogBlockUI> blocks = new List<DialogBlockUI>();

    // 현재 처리할 이벤트 인덱스
    private int currentEventIndex = 0;
    private int currentMainIndex = 0;
    void Start()
    {
        // 코루틴으로 순차 표시 시작
        StartCoroutine(ProcessEvents());
    }

    private IEnumerator ProcessEvents()
    {
        // jsonManager 안에 List<Script_Master_Event> 가 있다는 가정
        List<Script_Master_Event> events = jsonManager.scriptMasterEvents;
        //List<Script_Master_Main> mains = jsonManager.scriptMasterMains;

        while (currentEventIndex < events.Count)
        {
            var ev = events[currentEventIndex];

            // 블록 생성 또는 누적 타이핑
            HandleEvent(ev);

            // 텍스트라면 코루틴이 끝날 때까지 대기
            if (ev.displayType == "Text")
            {
                // TypeText 코루틴이 실행될 때까지 잠시 대기
                // (typingDelay * 글자수 + 0.1f 여유)
                yield return new WaitForSeconds(ev.KOR.Length * typingDelay + 0.1f);
            }
            else
            {
                // 이미지라면 짧게 띄워두거나, 버튼 입력 대기 등
                yield return new WaitForSeconds(1f);
            }

            currentEventIndex++;
        }
        //while (currentMainIndex < mains.Count)
        //{
        //    var mi = mains[currentMainIndex];
        //    Debug.Log($"mi.KOR의 값 : {mi.KOR}");
        //    // 블록 생성 또는 누적 타이핑
        //    HandleMain(mi);
        //    Debug.Log("문자열출력중입니다");
        //    // 텍스트라면 코루틴이 끝날 때까지 대기
        //    if (mi.displayType == "Text")
        //    {

        //        // TypeText 코루틴이 실행될 때까지 잠시 대기
        //        // (typingDelay * 글자수 + 0.1f 여유)
        //        yield return new WaitForSeconds(mi.KOR.Length * typingDelay + 0.3f);

        //    }
        //    else
        //    {
        //        // 이미지라면 짧게 띄워두거나, 버튼 입력 대기 등
        //        yield return new WaitForSeconds(1f);
        //    }

        //    currentMainIndex++;
        //}
    }

    private void HandleEvent(Script_Master_Event ev)
    {
        bool isImage = ev.displayType == "Image";

        // 1) 리스트가 비어있거나
        //   2) 이번이 이미지이거나 
        // 2) 마지막 블록이 이미지 타입이면 → 새로 Instantiate
        if (blocks.Count == 0 || isImage || blocks[blocks.Count - 1].imageComp.gameObject.activeSelf)
        {
            // 새 블록 만들기
            var go = Instantiate(dialogBlockPrefab, contentParent);
            var ui = go.GetComponent<DialogBlockUI>();
            blocks.Add(ui);
            RectTransform rt = go.GetComponent<RectTransform>();

            // 이미지 or 텍스트 초기 세팅
            if (isImage)
            {
                // 이미지 띄우고 텍스트 숨김
                rt.sizeDelta = new Vector2(700, 350);
                
                
                ui.imageComp.gameObject.SetActive(true);
                ui.textComp.gameObject.SetActive(false);

                // Resource 폴더에서 로드 예시
                
                Sprite sprite = Resources.Load<Sprite>("Images/" + ev.KOR);
                if (sprite == null)
                {
                    Debug.Log("프로그래머야 이게 뭐냐 버그났잖아!");
                }
                Debug.Log(sprite);
                ui.imageComp.sprite = sprite;
            }
            else
            {
                // 텍스트 블록 빈 상태로 시작
                ui.imageComp.gameObject.SetActive(false);
                ui.textComp.gameObject.SetActive(true);
                ui.textComp.text = string.Empty;

                // 첫 글자 찍기 코루틴 실행
                StartCoroutine(TypeText(ui.textComp, ev.KOR));
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(go.GetComponent<RectTransform>());
        }
        else
        {
            // 마지막 블록이 텍스트 타입 → 누적 타이핑
            var lastUi = blocks[blocks.Count - 1];
            StartCoroutine(TypeText(lastUi.textComp, ev.KOR));
        }
        
    }

    private void HandleMain(Script_Master_Main ev)
    {
        bool isImage = ev.displayType == "Image";

        // 1) 리스트가 비어있거나
        // 2) 마지막 블록이 이미지 타입이면 → 새로 Instantiate
        if (blocks.Count == 0 || isImage || blocks[blocks.Count - 1].imageComp.gameObject.activeSelf)
        {
            // 새 블록 만들기
            var go = Instantiate(dialogBlockPrefab, contentParent);
            var ui = go.GetComponent<DialogBlockUI>();
            blocks.Add(ui);
            RectTransform rt = go.GetComponent<RectTransform>();

            // 이미지 or 텍스트 초기 세팅
            if (isImage)
            {
                // 이미지 띄우고 텍스트 숨김
                rt.sizeDelta = new Vector2(700, 350);


                ui.imageComp.gameObject.SetActive(true);
                ui.textComp.gameObject.SetActive(false);

                // Resource 폴더에서 로드 예시

                Sprite sprite = Resources.Load<Sprite>("Images/" + ev.KOR);
                Debug.Log(ev.KOR);
                if (sprite == null)
                {
                    Debug.Log("프로그래머야 이게 뭐냐 버그났잖아!");
                }
                Debug.Log(sprite);
                dialogBlockPrefab.GetComponentInChildren<Image>().sprite = sprite;
            }
            else
            {
                // 텍스트 블록 빈 상태로 시작
                ui.imageComp.gameObject.SetActive(false);
                ui.textComp.gameObject.SetActive(true);
                ui.textComp.text = string.Empty;

                // 첫 글자 찍기 코루틴 실행
                StartCoroutine(TypeText(ui.textComp, ev.KOR));
            }
            //Debug.Log("출력성공");
        }
        else
        {
            // 마지막 블록이 텍스트 타입 → 누적 타이핑
            var lastUi = blocks[blocks.Count - 1];
            Debug.Log(lastUi.textComp);
            StartCoroutine(TypeText(lastUi.textComp, ev.KOR));
            Debug.Log(ev.KOR);
        }
    }

    // fullText: 출력할 전체 문자열
    // startIndex: fullText의 몇 번째 글자부터 찍을 것인지 (기본 0)
    private IEnumerator TypeText(TMP_Text textComp, string fullText)
    {
        for (int i = 0; i < fullText.Length; i++)
        {
            textComp.text += fullText[i];
            yield return new WaitForSeconds(typingDelay);
        }
    }
}