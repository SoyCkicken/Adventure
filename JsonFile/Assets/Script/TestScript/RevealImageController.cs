using UnityEngine;

public class RevealImageController : MonoBehaviour
{
    public RectTransform maskRect;             // 마스크 오브젝트
    public Vector2 fullSize = new Vector2(300, 300); // 전체 이미지 크기
    public float revealSpeed = 100f;           // 얼마나 빠르게 보일지

    private void Start()
    {
        maskRect.pivot = new Vector2(1f, 1f);         // 우측 상단 기준
        maskRect.sizeDelta = Vector2.zero;            // 처음엔 안 보이게
    }

    private void Update()
    {
        Vector2 currentSize = maskRect.sizeDelta;

        // X는 오른쪽에서 왼쪽으로 증가
        currentSize.x = Mathf.Min(currentSize.x + revealSpeed * Time.deltaTime, fullSize.x);
        currentSize.y = Mathf.Min(currentSize.y + revealSpeed * Time.deltaTime, fullSize.y);

        maskRect.sizeDelta = currentSize;
    }
}