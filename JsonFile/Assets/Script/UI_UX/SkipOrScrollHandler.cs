using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkipOrScrollHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public float longPressThreshold = 0.3f;
    public System.Action OnTapSkip;
    public ScrollRect targetScrollRect;

    private float pressTime;
    private Vector2 startPos;
    private bool isDragging = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        pressTime = Time.time;
        startPos = eventData.position;
        isDragging = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging && Time.time - pressTime <= longPressThreshold)
        {
            OnTapSkip?.Invoke();
        }

        isDragging = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        float dragDistance = Vector2.Distance(startPos, eventData.position);

        if (Time.time - pressTime > longPressThreshold && dragDistance > 10f)
        {
            isDragging = true;
            if (targetScrollRect != null)
            {
                ExecuteEvents.ExecuteHierarchy<IScrollHandler>(
                    targetScrollRect.gameObject,
                    eventData,
                    ExecuteEvents.scrollHandler
                );
            }
        }
    }
}