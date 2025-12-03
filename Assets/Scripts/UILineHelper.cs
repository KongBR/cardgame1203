using UnityEngine;

public static class UILineHelper
{
    public static void Connect(RectTransform parent, RectTransform start, RectTransform end, GameObject linePrefab)
    {
        GameObject lineObj = GameObject.Instantiate(linePrefab, parent);
        RectTransform lineRect = lineObj.GetComponent<RectTransform>();

        // 중심점
        Vector2 midPoint = (start.anchoredPosition + end.anchoredPosition) / 2f;
        lineRect.anchoredPosition = midPoint;

        // 길이
        float length = Vector2.Distance(start.anchoredPosition, end.anchoredPosition);
        lineRect.sizeDelta = new Vector2(length, lineRect.sizeDelta.y);

        // 각도
        Vector2 dir = end.anchoredPosition - start.anchoredPosition;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        lineRect.rotation = Quaternion.Euler(0, 0, angle);
    }
}
