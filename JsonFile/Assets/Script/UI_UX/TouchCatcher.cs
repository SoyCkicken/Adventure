using UnityEngine;

public class TouchCatcher : MonoBehaviour
{
    public RectTransform scrollViewRect;
    public System.Action onTapOutsideScrollView;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 pos = Input.mousePosition;
            if (!RectTransformUtility.RectangleContainsScreenPoint(scrollViewRect, pos, null))
            {
                onTapOutsideScrollView?.Invoke();
            }
        }
    }
}