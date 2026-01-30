using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SmoothScrollSnapping : MonoBehaviour, IEndDragHandler, IBeginDragHandler
{
    [Header("Links")]
    public ScrollRect scrollRect;
    public RectTransform widthSource;

    [Header("Settings")]
    public float smoothTime = 0.3f;

    [Header("Auto Scroll Timer")]
    public bool useAutoScroll = true;
    public float autoScrollDelay = 5f;
    private float timer;

    [Header("Debug Info")]
    public int CurrentIndex;

    private bool isSnapping;
    private bool isDragging;
    private Vector2 targetPosition;
    private Vector2 currentVelocity;

    [Header("Swipe Distance Settings")]
    [Tooltip("На сколько пикселей нужно протащить палец для переключения")]
    public float swipeThreshold = 50f;
    private float startDragX; // Позиция начала касания

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        isSnapping = false;
        // Запоминаем экранную позицию пальца/курсора в момент нажатия
        startDragX = eventData.position.x;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        float cellWidth = widthSource.rect.width;
        // Текущий индекс, основанный на положении контента
        int targetIdx = CurrentIndex;

        // Вычисляем, на сколько пикселей сдвинулся палец по экрану
        float dragDistance = eventData.position.x - startDragX;

        // Если сдвинули вправо (палец прошел путь > порога) — идем к предыдущему
        if (dragDistance > swipeThreshold)
        {
            targetIdx = CurrentIndex - 1;
        }
        // Если сдвинули влево (палец прошел путь < -порога) — идем к следующему
        else if (dragDistance < -swipeThreshold)
        {
            targetIdx = CurrentIndex + 1;
        }

        ScrollToElement(targetIdx);
        ResetTimer();
    }

    void Start()
    {
        if (scrollRect == null) scrollRect = GetComponent<ScrollRect>();
        ResetTimer();
    }

    void Update()
    {
        UpdateCurrentIndex();
        HandleAutoScroll();

        if (isSnapping && !isDragging)
        {
            scrollRect.content.anchoredPosition = Vector2.SmoothDamp(
                scrollRect.content.anchoredPosition,
                targetPosition,
                ref currentVelocity,
                smoothTime
            );

            if (Vector2.Distance(scrollRect.content.anchoredPosition, targetPosition) < 0.05f)
            {
                scrollRect.content.anchoredPosition = targetPosition;
                currentVelocity = Vector2.zero;
                isSnapping = false;
            }
        }
    }

    private void HandleAutoScroll()
    {
        if (!useAutoScroll || isDragging) return;

        timer += Time.deltaTime;

        if (timer >= autoScrollDelay)
        {
            NextWithLoop();
            ResetTimer();
        }
    }

    private void ResetTimer()
    {
        timer = 0f;
    }

    public void NextWithLoop()
    {
        int childCount = scrollRect.content.childCount;
        if (childCount == 0) return;

        int nextIndex = CurrentIndex + 1;

        if (nextIndex >= childCount)
        {
            nextIndex = 0;
        }

        ScrollToElement(nextIndex);
    }

    public void ScrollToElement(int index)
    {
        if (widthSource == null || scrollRect == null) return;

        int childCount = scrollRect.content.childCount;
        if (childCount == 0) return;

        index = Mathf.Clamp(index, 0, childCount - 1);
        float cellWidth = widthSource.rect.width;

        targetPosition = new Vector2(-(index * cellWidth), scrollRect.content.anchoredPosition.y);

        currentVelocity = Vector2.zero;
        isSnapping = true;
        ResetTimer();
    }

    private void UpdateCurrentIndex()
    {
        if (widthSource == null || scrollRect == null) return;
        float cellWidth = widthSource.rect.width;
        if (cellWidth <= 0) return;

        float currentX = scrollRect.content.anchoredPosition.x;
        CurrentIndex = Mathf.RoundToInt(Mathf.Abs(currentX) / cellWidth);
    }
}