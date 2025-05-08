//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;
//using System.Text;
//using System.Collections;
//using UnityEditor.Experimental.GraphView;

//public class DialogBlockUI : MonoBehaviour
//{
//    public Image imageComp;     // 프리팹 내 Image 컴포넌트
//    public TMP_Text textComp;   // 프리팹 내 TMP_Text 컴포넌트

//    // Script_Master_Event 데이터를 받아 UI를 셋업하는 함수
//    //안쓸 가능성이 생김
//    public void SetBlockDataEvent(Script_Master_Event eventData)
//    {
//        RectTransform rt = GetComponent<RectTransform>();
//        // displayType에 따라 분기 처리
//        if (!string.IsNullOrEmpty(eventData.displayType) && eventData.displayType == "Image")
//        {
            
//            // 이미지 타입: KOR 필드에 이미지 파일명이 들어있다고 가정(확장자 없이)
//            Sprite spr = Resources.Load<Sprite>("Images/" + eventData.KOR);
//            if (spr != null)
//            {
//                rt.sizeDelta = new Vector2(700, 350);
//                imageComp.sprite = spr;
//                imageComp.gameObject.SetActive(true);
//                textComp.gameObject.SetActive(false);
//            }
//            else
//            {
//                Debug.LogError("이미지 로드 실패: " + eventData.KOR);
//                // 이미지 로드에 실패하면 fallback으로 텍스트 표시
//                textComp.text = eventData.KOR;
//                textComp.gameObject.SetActive(true);
//                imageComp.gameObject.SetActive(false);
//            }
//        }
//        else
//        {
//            // 텍스트 타입
//            rt.sizeDelta = new Vector2(700, 75);
//            textComp.text = eventData.KOR;
//            textComp.gameObject.SetActive(true);
//            imageComp.gameObject.SetActive(false);
//        }
//    }
//    public void SetBlockDataMain(Script_Master_Main eventData)
//    {
//        RectTransform rt = GetComponent<RectTransform>();
//        //displayType에 따라 분기 처리
//        if (!string.IsNullOrEmpty(eventData.displayType) && eventData.displayType == "Image")
//        {

//            // 이미지 타입: KOR 필드에 이미지 파일명이 들어있다고 가정(확장자 없이)
//            Sprite spr = Resources.Load<Sprite>("Images/" + eventData.KOR);
//            if (spr != null)
//            {
//                rt.sizeDelta = new Vector2(700, 350);
//                imageComp.sprite = spr;
//                imageComp.gameObject.SetActive(true);
//                textComp.gameObject.SetActive(false);
//            }
//            else
//            {
//                Debug.LogError("이미지 로드 실패: " + eventData.KOR);
//                // 이미지 로드에 실패하면 fallback으로 텍스트 표시
//                textComp.text = eventData.KOR;
//                textComp.gameObject.SetActive(true);
//                imageComp.gameObject.SetActive(false);
//            }
//        }
//        else
//        {
//            // 텍스트 타입
//            rt.sizeDelta = new Vector2(700, 75);
//            textComp.text = eventData.KOR;
//            textComp.gameObject.SetActive(true);
//            imageComp.gameObject.SetActive(false);
//        }

//    }
    
//}
