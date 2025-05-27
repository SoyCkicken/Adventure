using UnityEngine;

public class SkipButtonFilter : MonoBehaviour
{
    public RectTransform scrollViewRect;
    public System.Action onSkip;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 pos = Input.mousePosition;
            bool isOverScroll = RectTransformUtility.RectangleContainsScreenPoint(scrollViewRect, pos, null);
            
            if (!isOverScroll)
            {
                Debug.Log("스킵 실행");
                onSkip?.Invoke();
            }
            else
            {
                if (Input.GetMouseButton(0))
                {
                   // this.gameObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
                }
                Debug.Log("스크롤 영역 터치 → 무시");
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            //this.gameObject.GetComponent<CanvasGroup>().blocksRaycasts = true;
        }
    }
}