using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventDisplay : MonoBehaviour
{
    public Image imageComponent;  // 이미지 UI 컴포넌트 (예: Canvas 하위)
    public TMP_Text textComponent;  // 텍스트 UI 컴포넌트
    private void Start()
    {
        Debug.Log("해당 테스트는 출력 관련하여 테스트입니다");
    }
    // Script_Master_Event 타입의 이벤트 데이터를 받는다고 가정
    public void DisplayEvent(Script_Master_Event scriptEvent)
    {
        if (!string.IsNullOrEmpty(scriptEvent.KOR) && IsImageReference(scriptEvent.KOR))
        {
            string imageName = GetImageName(scriptEvent.KOR);
            Debug.Log(imageName);
            // Resources/Images 폴더에 이미지 스프라이트들이 있어야 함 (확장자 없이 이름)
            Sprite sprite = Resources.Load<Sprite>("Images/" + imageName);
            Debug.Log(sprite);
            if (sprite != null)
            {
                imageComponent.sprite = sprite;
                imageComponent.gameObject.SetActive(true);
                textComponent.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("이미지 로드 실패: " + imageName);
            }
        }
        else
        {
            // 일반 텍스트 출력
            textComponent.text = scriptEvent.KOR;
            textComponent.gameObject.SetActive(true);
            imageComponent.gameObject.SetActive(false);
        }
    }

    bool IsImageReference(string kor)
    {
        return kor.StartsWith("[IMG]");
    }

    string GetImageName(string kor)
    {
        return kor.Substring(5).Trim();
    }
}
