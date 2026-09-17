using System;
using UnityEngine;

public class PetCalendarUI : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Transform daysContainer;
    [SerializeField] private CalendarDayUI dayPrefab;

    [Header("Quantidade")]
    [SerializeField] private int daysBeforeToday = 3;
    [SerializeField] private int daysAfterToday = 3;

    [Header("Cores")]
    [SerializeField] private Color pastDayColor = new Color(0.95f, 0.70f, 0.20f);
    [SerializeField] private Color todayColor = new Color(0.92f, 0.78f, 0.48f);
    [SerializeField] private Color futureDayColor = new Color(0.94f, 0.84f, 0.60f);

    [SerializeField] private Color pastTextColor = new Color(0.45f, 0.25f, 0.05f);
    [SerializeField] private Color todayTextColor = new Color(0.55f, 0.35f, 0.10f);
    [SerializeField] private Color futureTextColor = new Color(0.60f, 0.45f, 0.20f);

    private void Start()
    {
        GenerateCalendar();
    }

    public void GenerateCalendar()
    {
        if (daysContainer == null || dayPrefab == null)
        {
            return;
        }

        ClearDays();

        DateTime today = DateTime.Today;

        DateTime startDate =
            today.AddDays(-daysBeforeToday);

        DateTime endDate =
            today.AddDays(daysAfterToday);

        for (
            DateTime date = startDate;
            date <= endDate;
            date = date.AddDays(1)
        )
        {
            CalendarDayUI item =
                Instantiate(dayPrefab, daysContainer);

            if (date.Date < today)
            {
                item.Setup(
                    date.Day,
                    pastDayColor,
                    pastTextColor
                );
            }
            else if (date.Date == today)
            {
                item.Setup(
                    date.Day,
                    todayColor,
                    todayTextColor
                );
            }
            else
            {
                item.Setup(
                    date.Day,
                    futureDayColor,
                    futureTextColor
                );
            }
        }
    }

    private void ClearDays()
    {
        for (int i = daysContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(daysContainer.GetChild(i).gameObject);
        }
    }
}