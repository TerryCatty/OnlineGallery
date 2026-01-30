using UnityEngine;

[ExecuteInEditMode]
public class CustomHorizontalLayout : MonoBehaviour
{
    public RectTransform widthSource;

    void Update()
    {
        if (widthSource == null) return;
        Arrange();
    }

    private void Arrange()
    {
        RectTransform parentRT = transform as RectTransform;
        
        float targetWidth = widthSource.rect.width;
        float currentXOffset = 0f;

        for (int i = 0; i < transform.childCount; i++)
        {
            RectTransform child = transform.GetChild(i) as RectTransform;
            if (child == null || !child.gameObject.activeSelf) continue;

            child.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);

            float distanceToAnchor = parentRT.rect.width * child.anchorMin.x;

            float pivotOffset = targetWidth * child.pivot.x;

            Vector2 newPos = child.anchoredPosition;
            newPos.x = (currentXOffset - distanceToAnchor) + pivotOffset;
            
            child.anchoredPosition = newPos;

            currentXOffset += targetWidth;
        }

        parentRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentXOffset);
    }
}