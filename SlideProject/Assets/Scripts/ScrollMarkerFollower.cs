using UnityEngine;

public class ScrollMarkerFollower : MonoBehaviour
{
    [Header("Links")]
    public SmoothScrollSnapping scrollSource; 
    public RectTransform mainMarker;         
    public RectTransform indicatorsParent;   

    [Header("Settings")]
    public float followSpeed = 10f;

    void Update()
    {
        if (scrollSource == null || mainMarker == null || indicatorsParent == null)
            return;

        int currentIndex = scrollSource.CurrentIndex;

        if (currentIndex >= 0 && currentIndex < indicatorsParent.childCount)
        {
            Transform targetIndicator = indicatorsParent.GetChild(currentIndex);

            mainMarker.position = Vector3.Lerp(
                mainMarker.position,
                targetIndicator.position,
                Time.deltaTime * followSpeed
            );
        }
    }
}