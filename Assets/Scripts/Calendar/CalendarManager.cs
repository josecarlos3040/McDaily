using System;
using UnityEngine;
using UnityEngine.UI;

public class CalendarManager : MonoBehaviour
{
    [Header("References")]
    public CalendarDay dayPrefab;
    public MonthHeader monthHeaderPrefab;

    public RectTransform content;
    public RectTransform viewport;
    public ScrollRect scrollRect;
    public CalendarPath calendarPath;

    [Header("Calendar")]
    public int daysBefore = 14;
    public int daysAfter = 14;

    [Header("Layout")]
    public float spacing = 420f;

    [Header("Vertical Position")]
    public float verticalOffset = -1000f;

    [Header("Month Header")]
    public float monthHeaderHeight = 80f;

    private RectTransform todayRect;

    void Start()
    {
        GenerateCalendar();
    }

    void GenerateCalendar()
    {
        DateTime today = DateTime.Today;

        DateTime firstDate =
            today.AddDays(-daysBefore);

        DateTime lastDate =
            today.AddDays(daysAfter);

        int totalDays =
            (lastDate - firstDate).Days + 1;

        // =========================================
        // LIMPAR
        // =========================================

        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Transform child = content.GetChild(i);

            if (child.GetComponent<CalendarDay>() != null ||
                child.GetComponent<MonthHeader>() != null)
            {
                Destroy(child.gameObject);
            }
        }

        todayRect = null;

        Canvas.ForceUpdateCanvases();

        // =========================================
        // CONTENT
        // =========================================

        Vector2[] positions =
            new Vector2[totalDays];

        float contentHeight =
            (totalDays - 1) * spacing + 250f;

        content.sizeDelta =
            new Vector2(
                content.sizeDelta.x,
                contentHeight
            );

        float centerY =
            contentHeight / 2f;

        DateTime previousDate =
            DateTime.MinValue;

        // =========================================
        // DIAS
        // =========================================

        for (int i = 0; i < totalDays; i++)
        {
            DateTime date =
                firstDate.AddDays(i);

            int daysFromToday =
                (date - today).Days;

            float y =
                centerY +
                (daysFromToday * spacing) +
                verticalOffset;

            float x =
                Mathf.Sin(i * 1.2f) * 180f;

            // =====================================
            // MONTH HEADER
            // =====================================

            if (previousDate != DateTime.MinValue &&
                date.Month != previousDate.Month)
            {
                CreateMonthHeader(
                    date.Year,
                    date.Month,
                    y - spacing / 2f
                );
            }

            // =====================================
            // DAY
            // =====================================

            CalendarDay day =
                Instantiate(
                    dayPrefab,
                    content
                );

            RectTransform rect =
                day.GetComponent<RectTransform>();

            rect.localScale =
                Vector3.one;

            rect.anchoredPosition =
                new Vector2(
                    x,
                    y
                );

            positions[i] =
                new Vector2(
                    x,
                    y
                );

            bool isToday =
                date.Date == today.Date;

            day.Setup(
                date.Day,
                isToday
            );

            if (isToday)
            {
                todayRect = rect;
            }

            previousDate = date;
        }

        // =========================================
        // PATH
        // =========================================

        if (calendarPath != null)
        {
            calendarPath.transform.SetAsFirstSibling();

            calendarPath.CreatePath(
                positions
            );
        }

        Canvas.ForceUpdateCanvases();

        CenterCalendar();
    }

    void CreateMonthHeader(
        int year,
        int month,
        float y
    )
    {
        MonthHeader header =
            Instantiate(
                monthHeaderPrefab,
                content
            );

        RectTransform rect =
            header.GetComponent<RectTransform>();

        rect.localScale =
            Vector3.one;

        rect.sizeDelta =
            new Vector2(
                rect.sizeDelta.x,
                monthHeaderHeight
            );

        rect.anchoredPosition =
            new Vector2(
                0f,
                y
            );

        header.SetMonth(
            year,
            month
        );
    }

    void CenterCalendar()
    {
        if (todayRect == null)
            return;

        Canvas.ForceUpdateCanvases();

        scrollRect.verticalNormalizedPosition = 0.5f;
    }
}