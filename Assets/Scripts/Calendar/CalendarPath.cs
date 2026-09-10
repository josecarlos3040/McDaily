using UnityEngine;
using UnityEngine.UI;

public class CalendarPath : MonoBehaviour
{
    public Image pathSegmentPrefab;

    public void CreatePath(Vector2[] positions)
    {
        // Remove paths antigos
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < positions.Length - 1; i++)
        {
            CreateSegment(
                positions[i],
                positions[i + 1]
            );
        }
    }

    private void CreateSegment(Vector2 start, Vector2 end)
    {
        Image segment =
            Instantiate(
                pathSegmentPrefab,
                transform
            );

        RectTransform rect =
            segment.GetComponent<RectTransform>();

        Vector2 direction =
            end - start;

        float distance =
            direction.magnitude;

        rect.anchoredPosition =
            (start + end) / 2f;

        rect.sizeDelta =
            new Vector2(
                distance,
                rect.sizeDelta.y
            );

        float angle =
            Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;

        rect.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                angle
            );
    }
}